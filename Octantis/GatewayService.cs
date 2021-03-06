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
    internal class BackgroundTask : IDisposable
    {
        private Task _task;
        private CancellationTokenSource _cts;
        private bool _disposed;

        public BackgroundTask(Func<CancellationToken, Task> action)
        {
            _cts = new CancellationTokenSource();
            _task = Task.Run(async () => await action(_cts.Token), _cts.Token);
        }

        public async Task CancelAsync()
        {
            _cts.Cancel();
            await _task;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _cts.Cancel();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    internal class GatewayService : BackgroundService, IHostedService, IGateway
    {
        private class Registration : IDisposable
        {
            private bool _disposed;

            private readonly GatewayService _parent;

            public Event Type { get; }
            public Action<object> Action { get; }
            public Type ArgumentType { get; }

            public Registration(GatewayService parent, Event type, Action<object> action, Type argumentType)
            {
                _parent = parent ?? throw new ArgumentNullException(nameof(parent));
                Type = type;
                Action = action ?? throw new ArgumentNullException(nameof(action));
                ArgumentType = argumentType ?? throw new ArgumentNullException(nameof(argumentType));
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!_disposed)
                {
                    if (disposing)
                    {
                        _parent.RemoveEventHandler(this);
                    }
                    _disposed = true;
                }
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        private readonly IOptions<DiscordSettings> _settings;
        private readonly ILogger<GatewayService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly RestApi _restApi;

        private ClientWebSocket? _webSocket;

        private GatewayState _state = GatewayState.Disconnected;
        private TimeSpan _heartbeatInterval;
        private int? _lastSequenceNumber;
        private string? _sessionId;

        private BackgroundTask? _heartbeatTask;

        private Dictionary<Event, List<Registration>> _registrations = new Dictionary<Event, List<Registration>>();

        public GatewayState State => _state;
        public ulong ApplicationId { get; private set; }

        public GatewayService(IOptions<DiscordSettings> settings, ILogger<GatewayService> logger, JsonSerializerOptions jsonOptions, RestApi restApi)
        {
            _settings = settings;
            _logger = logger;
            _jsonOptions = jsonOptions;
            _restApi = restApi;
        }

        public IDisposable AddEventHandler<T>(Event type, Action<T> handler) where T: class, new()
        {
            lock (_registrations)
            {
                if (!_registrations.TryGetValue(type, out var registrationsForType))
                {
                    registrationsForType = new List<Registration>();
                    _registrations[type] = registrationsForType;
                }

                var registration = new Registration(this, type, (arg) => handler((T)arg), typeof(T));
                registrationsForType.Add(registration);
                return registration;
            }
        }

        private void RemoveEventHandler(Registration registration)
        {
            lock (_registrations)
            {
                if (!_registrations.TryGetValue(registration.Type, out var registrationsForType))
                {
                    return;
                }
                registrationsForType.Remove(registration);
            }
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

            _state = GatewayState.Connecting;
            _webSocket = new ClientWebSocket();
            await _webSocket.ConnectAsync(new Uri($"{url}?v=9&encoding=json"), cancellationToken);

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

            if (_heartbeatTask is not null)
            {
                _heartbeatTask.Dispose();
                _heartbeatTask = null;
            }

            _registrations.Clear();

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

        private async Task HandlePacket(string json, CancellationToken cancellationToken)
        {
            var genericGateway = JsonSerializer.Deserialize<GatewayPacket<object>>(json, _jsonOptions);
            if (genericGateway is null)
            {
                _logger.LogError("Unable to parse generic gateway packet");
                return;
            }

            if (genericGateway.SequenceNumber is not null)
                _lastSequenceNumber = genericGateway.SequenceNumber;

            switch (_state)
            {
                case GatewayState.Connecting:
                    if (genericGateway.Opcode == Opcode.Hello)
                    {
                        // We got a Hello, start sending heartbeats!
                        var helloPacket = DeserializePacket<HelloData>(json);
                        if (helloPacket is null || helloPacket.Data is null)
                        {
                            _logger.LogError("Invalid hello packet");
                            return;
                        }
                        _heartbeatInterval = TimeSpan.FromMilliseconds(helloPacket.Data.HeartbeatInterval);
                        _logger.LogDebug("Heartbeat interval set to '{Interval}'", _heartbeatInterval);
                        if (_heartbeatTask is null)
                        {
                            _heartbeatTask = new BackgroundTask(SendHeartbeatAsync);
                        }
                        // Queue up an Identify packet
                        var identifyPacket = new GatewayPacket<IdentifyData>
                        {
                            Data = new IdentifyData
                            {
                                // TODO: Intents
                                Intents = 1,
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
                    }
                    else if (genericGateway.Opcode == Opcode.Dispatch && genericGateway.EventName == Events.Ready)
                    {
                        var readyPacket = DeserializePacket<ReadyData>(json);
                        if (readyPacket is null || readyPacket.Data is null)
                        {
                            _logger.LogError("Invalid ready packet");
                            return;
                        }
                        ApplicationId = readyPacket.Data.User?.Id ?? 0;
                        _sessionId = readyPacket.Data.SessionId;
                        _logger.LogInformation("Connected to gateway v{Version}, application id '{Id}', session id '{Id}'", readyPacket.Data.GatewayVersion, ApplicationId, _sessionId);
                        foreach (var guild in readyPacket.Data.Guilds)
                        {
                            _logger.LogDebug("Guild '{Id}'", guild.Id);
                        }

                        // Got the ready event, move up to connected state!
                        _state = GatewayState.Connected;
                    }
                    else
                    {
                        _logger.LogWarning("Unhandled opcode '{Opcode}'", genericGateway.Opcode);
                    }
                    break;
                case GatewayState.Connected:
                    {
                        // Handle events
                        if (genericGateway.Opcode == Opcode.Heartbeat)
                        {
                            // Force heartbeat send?
                            _logger.LogError("TODO: SEND HEARTBEAT");
                        }
                        else if (genericGateway.Opcode == Opcode.Dispatch)
                        {
                            // EVENTS :D
                            var type = Events.FromString(genericGateway.EventName!);
                            List<Registration>? registrationsForEvent;
                            lock (_registrations)
                            {
                                _registrations.TryGetValue(type, out registrationsForEvent);
                            }
                            if (registrationsForEvent is not null)
                            {
                                foreach (var registration in registrationsForEvent)
                                {
                                    var packetType = typeof(GatewayPacket<>).MakeGenericType(registration.ArgumentType);
                                    dynamic deserializedAsType = JsonSerializer.Deserialize(json, packetType, _jsonOptions)!;
                                    registration.Action(deserializedAsType.Data);
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Unhandled event type '{Type}'", type);
                            }
                        }
                    }
                    break;
            }
        }

        private GatewayPacket<T>? DeserializePacket<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<GatewayPacket<T>>(json, _jsonOptions);
            }
            catch (JsonException e)
            {
                _logger.LogError(e, "Exception when trying to deserialize JSON");
            }
            return null;
        }

        private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Starting Heartbeat task");

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_state != GatewayState.Connected && _state != GatewayState.Connecting)
                    {
                        break;
                    }

                    await Task.Delay(_heartbeatInterval, cancellationToken);
                    await TransmitGatewayAsync(new GatewayPacket<int?>
                    {
                        Opcode = Opcode.Heartbeat,
                        Data = _lastSequenceNumber
                    }, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }

            _logger.LogDebug("Terminating Heartbeat task");

            _heartbeatTask = null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_webSocket is null)
                return;

            // Large buffer so we can (hopefully) fit a message no matter the size
            var receiveBuffer = new Memory<byte>(new byte[1024*1024*10]);

            try
            {
                while(!stoppingToken.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(receiveBuffer, stoppingToken);
                    if (!result.EndOfMessage)
                    {
                        _logger.LogError("Did not get entire message from Gateway");
                        continue;
                    }
                    var json = Encoding.UTF8.GetString(receiveBuffer.Slice(0, result.Count).Span);
                    _logger.LogTrace("Received message '{Type}': '{Json}'", result.MessageType, json);

                    try
                    {
                        await HandlePacket(json, stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected
                    }
                    catch (Exception exc)
                    {
                        _logger.LogError(exc, "Exception when trying to handle gateway packet");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
        }
    }
}
