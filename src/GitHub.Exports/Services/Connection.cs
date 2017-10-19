using System;
using GitHub.Models;
using GitHub.Primitives;

namespace GitHub.Services
{
    public class Connection : IConnection
    {
        public Connection(
            HostAddress hostAddress,
            string userName,
            Octokit.User user,
            Exception connectionError)
        {
            HostAddress = hostAddress;
            Username = userName;
            User = user;
            ConnectionError = connectionError;
        }

        public HostAddress HostAddress { get; }
        public string Username { get; }
        public Octokit.User User { get; }
        public bool IsLoggedIn => ConnectionError == null;
        public Exception ConnectionError { get; }
    }
}
