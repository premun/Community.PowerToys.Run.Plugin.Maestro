// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Community.PowerToys.Run.Plugin.Maestro.Maestro.Client;

public class AppCredentialResolverOptions
{
    /// <summary>
    /// Client ID of the Azure application to request the token for
    /// </summary>
    public string AppId { get; }

    /// <summary>
    /// Whether to include interactive login flows
    /// </summary>
    public bool DisableInteractiveAuth { get; set; }

    /// <summary>
    /// Token to use directly instead of authenticating.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Managed Identity to use for the auth
    /// </summary>
    public string? ManagedIdentityId { get; set; }

    /// <summary>
    /// User scope to request the token for (in case of user flows).
    /// </summary>
    public string UserScope { get; set; } = ".default";

    public AppCredentialResolverOptions(string appId)
    {
        AppId = appId;
    }
}
