using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octantis.Discord.Api
{
    public enum GatewayState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting
    }

    public interface IGateway
    {
        GatewayState State { get; }

        IDisposable AddEventHandler<T>(Event type, Action<T> handler) where T : class, new();
    }
}
