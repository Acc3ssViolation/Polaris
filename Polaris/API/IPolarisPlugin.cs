using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris.API
{
    public interface IPolarisPlugin
    {
        string Name { get; }
        Version Version { get; }

        Task LoadAsync(IServiceProvider serviceProvider);
        Task UnloadAsync();
    }
}
