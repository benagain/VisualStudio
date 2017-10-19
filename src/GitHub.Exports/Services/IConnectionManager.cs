using System;
using System.Threading.Tasks;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Services
{
    public interface IConnectionManager
    {
        IReadOnlyObservableCollection<IConnection> Connections { get; }

        Task<IConnection> GetConnection(HostAddress address);
        Task<IReadOnlyObservableCollection<IConnection>> GetLoadedConnections();
        Task<IConnection> LogIn(HostAddress address, string username, string password);
        Task LogOut(HostAddress address);
    }
}