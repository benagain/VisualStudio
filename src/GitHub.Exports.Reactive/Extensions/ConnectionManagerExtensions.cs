using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;

namespace GitHub.Extensions
{
    public static class ConnectionManagerExtensions
    {
        public static async Task<bool> IsLoggedIn(this IConnectionManager cm)
        {
            var connections = await cm.GetLoadedConnections();
            return connections.Any(x => x.ConnectionError == null);
        }

        public static async Task<bool> IsLoggedIn(this IConnectionManager cm, HostAddress address)
        {
            var connections = await cm.GetLoadedConnections();
            return connections.Any(x => x.HostAddress == address && x.ConnectionError == null);
        }

        public static async Task<IConnection> LookupConnection(this IConnectionManager cm, IRepositoryModel repository)
        {
            if (repository != null)
            {
                var address = HostAddress.Create(repository.CloneUrl);
                var connections = await cm.GetLoadedConnections();
                return connections.FirstOrDefault(x => x.HostAddress == address);
            }

            return null;
        }

        public static IObservable<IConnection> LookupConnection(this IConnectionManager cm, ILocalRepositoryModel repository)
        {
            return Observable.Return(repository?.CloneUrl != null
                ? cm.Connections.FirstOrDefault(c => c.HostAddress.Equals(HostAddress.Create(repository.CloneUrl)))
                : null);
        }

        public static IObservable<IConnection> GetFirstLoggedInConnection(this IConnectionManager cm)
        {
            return cm.GetLoadedConnections()
                .ToObservable()
                .Select(x => x.FirstOrDefault(y => y.IsLoggedIn));
        }
    }
}
