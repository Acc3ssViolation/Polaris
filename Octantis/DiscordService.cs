using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octantis.Discord.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Octantis
{
    internal class DiscordService : BackgroundService, IHostedService
    {
        private readonly IOptions<DiscordSettings> _settings;
        private readonly ILogger<DiscordService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly RestApi _restApi;

        private ClientWebSocket? _webSocket;

        public DiscordService(IOptions<DiscordSettings> settings, ILogger<DiscordService> logger, JsonSerializerOptions jsonOptions, RestApi restApi)
        {
            _settings = settings;
            _logger = logger;
            _jsonOptions = jsonOptions;
            _restApi = restApi;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started Discord Service");
            _logger.LogDebug("Mod role: {ModRole}", _settings.Value.ModRole);


            _webSocket = new ClientWebSocket();
          
            var url = (await _restApi.GetAsync<GetGatewayResponse>("/gateway", cancellationToken))?.Url ?? string.Empty;

            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogError("Unable to get Gateway URL");
                return;
            }

            _logger.LogDebug("Got Gateway Url '{Url}'", url);

            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(new Uri($"{url}?v=9&encoding=json"), cancellationToken);
            
            var helloPacket = new GatewayPacket<HelloData>
            {
                Data = new HelloData
                {
                    HeartbeatInterval = 45000
                },
                Opcode = Opcode.Hello,
            };

            await TransmitGatewayAsync(helloPacket, cancellationToken);

            var identifyPacket = new GatewayPacket<IdentifyData>
            {
                Data = new IdentifyData
                {
                    Intents = (1 << 9) | 1,
                    Token = _settings.Value.Token,
                    Properties = new IdentifyDataProperties
                    {
                        Browser = "octantis",
                        Device = "octantis",
                        Os = "linux"
                    }
                },
                Opcode = Opcode.Identify
            };

            await TransmitGatewayAsync(identifyPacket, cancellationToken);

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            if (_webSocket is not null)
            {
                _webSocket.Dispose();
                _webSocket = null;
            }

            _logger.LogInformation("Stopped Discord Service");
        }

        private async Task TransmitGatewayAsync<T>(T data, CancellationToken cancellationToken)
        {
            if (_webSocket is null || _webSocket.State != WebSocketState.Open)
                return;
            
            var json = JsonSerializer.Serialize(data, _jsonOptions);
            _logger.LogTrace("Sending '{Json}'", json);
            await _webSocket.SendAsync(Encoding.UTF8.GetBytes(json), WebSocketMessageType.Text, true, cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_webSocket is null)
                return;

            try
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    var receiveBuffer = new Memory<byte>(new byte[4096]);
                    var result = await _webSocket.ReceiveAsync(receiveBuffer, stoppingToken);
                    if (!result.EndOfMessage)
                    {
                        _logger.LogError("Did not get entire message from Gateway");
                        continue;
                    }
                    var json = Encoding.UTF8.GetString(receiveBuffer.Slice(0, result.Count).Span);
                    _logger.LogTrace("Received message '{Type}': '{Json}'", result.MessageType, json);                    
                    
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }
    }
}
