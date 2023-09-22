﻿using Wheel.DependencyInjection;

namespace Wheel.Core.Users
{
    public interface ICurrentUser : IScopeDependency
    {
        bool IsAuthenticated { get; }
        string? Id { get; }
        string UserName { get; }

        string[] Roles { get; }

        bool IsInRoles(string role);
    }
}
