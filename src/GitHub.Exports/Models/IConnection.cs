using System;
using GitHub.Primitives;
using Octokit;

namespace GitHub.Models
{
    public interface IConnection
    {
        HostAddress HostAddress { get; }
        string Username { get; }
        User User { get; }
        bool IsLoggedIn { get; }
        Exception ConnectionError { get; }
    }
}
