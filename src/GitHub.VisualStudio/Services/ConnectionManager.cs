using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GitHub.Api;
using GitHub.Extensions;
using GitHub.Models;
using GitHub.Primitives;
using GitHub.Services;
using IGitHubClient = Octokit.IGitHubClient;
using GitHubClient = Octokit.GitHubClient;
using User = Octokit.User;

namespace GitHub.VisualStudio
{
    [Export(typeof(IConnectionManager))]
    public class ConnectionManager : IConnectionManager
    {
        readonly IProgram program;
        readonly IConnectionCache cache;
        readonly IKeychain keychain;
        readonly ILoginManager loginManager;
        readonly TaskCompletionSource<object> loaded;
        readonly Lazy<ObservableCollectionEx<IConnection>> connections;

        [ImportingConstructor]
        public ConnectionManager(
            IProgram program,
            IConnectionCache cache,
            IKeychain keychain,
            ILoginManager loginManager)
        {
            this.program = program;
            this.cache = cache;
            this.keychain = keychain;
            this.loginManager = loginManager;
            loaded = new TaskCompletionSource<object>();
            connections = new Lazy<ObservableCollectionEx<IConnection>>(
                this.CreateConnections,
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public IReadOnlyObservableCollection<IConnection> Connections => connections.Value;

        public async Task<IConnection> GetConnection(HostAddress address)
        {
            return (await GetLoadedConnections()).FirstOrDefault(x => x.HostAddress == address);
        }

        public async Task<IReadOnlyObservableCollection<IConnection>> GetLoadedConnections()
        {
            return await GetLoadedConnectionsInternal();
        }

        public async Task<IConnection> LogIn(HostAddress address, string userName, string password)
        {
            var conns = await GetLoadedConnectionsInternal();

            if (conns.Any(x => x.HostAddress == address))
            {
                throw new InvalidOperationException($"A connection to {address} already exists.");
            }

            var client = CreateClient(address);
            var user = await loginManager.Login(address, client, userName, password);
            var connection = new Connection(address, userName, user, null);
            conns.Add(connection);
            return connection;
        }

        public async Task LogOut(HostAddress address)
        {
            var connection = await GetConnection(address);

            if (connection == null)
            {
                throw new KeyNotFoundException($"Could not find a connection to {address}.");
            }

            var client = CreateClient(address);
            await loginManager.Logout(address, client);
            connections.Value.Remove(connection);
        }

        ObservableCollectionEx<IConnection> CreateConnections()
        {
            var result = new ObservableCollectionEx<IConnection>();
            LoadConnections(result).Forget();
            return result;
        }

        public async Task<ObservableCollectionEx<IConnection>> GetLoadedConnectionsInternal()
        {
            var result = Connections;
            await loaded.Task;
            return connections.Value;
        }

        async Task LoadConnections(ObservableCollection<IConnection> result)
        {
            try
            {
                foreach (var c in await cache.Load())
                {
                    var client = CreateClient(c.HostAddress);
                    User user = null;
                    Exception error = null;

                    try
                    {
                        user = await loginManager.LoginFromCache(c.HostAddress, client);
                    }
                    catch (Exception e)
                    {
                        error = e;
                    }

                    result.Add(new Connection(c.HostAddress, c.UserName, user, error));
                }
            }
            finally
            {
                loaded.SetResult(null);
            }
        }

        IGitHubClient CreateClient(HostAddress address)
        {
            return new GitHubClient(
                program.ProductHeader,
                new KeychainCredentialStore(keychain, address),
                address.ApiUri);
        }
    }
}
