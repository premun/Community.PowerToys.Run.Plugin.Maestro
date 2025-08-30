using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using ManagedCommon;
using Microsoft.DotNet.ProductConstructionService.Client;
using Microsoft.DotNet.ProductConstructionService.Client.Models;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Maestro;

/// <summary>
/// Main class of this plugin that implement all used interfaces.
/// </summary>
public class Main : IPlugin, IDelayedExecutionPlugin, IDisposable
{
    /// <summary>
    /// ID of the plugin.
    /// </summary>
    public static string PluginID => "077861CDA5AB4F12BB04C3C3B8AD3517";

    public string Name => ".NET Maestro";

    public string Description { get; } = $"Manage Maestro subscriptions ({ProductConstructionServiceApiOptions.ProductionMaestroUri})";

    private PluginInitContext Context { get; set; }

    private IProductConstructionServiceApi MaestroClient;

    private string _iconPath { get; set; }

    private readonly Dictionary<Guid, Subscription> _subscriptions = [];

    private bool Disposed { get; set; }

    public List<Result> Query(Query query)
    {
        if (string.IsNullOrEmpty(query.Search.Trim()) || !Guid.TryParse(query.Search.Trim(), out var subscriptionId))
        {
            return
            [
                new Result()
                {
                    Title = "Insert subscription ID",
                    SubTitle = $"E.g. {Guid.NewGuid()}",
                    IcoPath = _iconPath,
                }
            ];
        }
        else
        {
            if (_subscriptions.TryGetValue(subscriptionId, out var subscription))
            {
                return
                [
                    new Result
                    {
                        Title = $"Subscription {subscriptionId} ({subscription.Channel})",
                        SubTitle = $"{subscription.SourceRepository} -> {subscription.TargetRepository} / {subscription.TargetBranch}",
                        IcoPath = _iconPath,
                        ContextData = subscriptionId.ToString(),
                        Action = _ =>
                        {
                            Clipboard.SetDataObject(subscriptionId.ToString());
                            return true;
                        },
                    }
                ];
            }

            return
            [
                new Result
                {
                    Title = $"Loading subscription {subscriptionId}...",
                    SubTitle = $"Fetching from " + ProductConstructionServiceApiOptions.ProductionMaestroUri,
                    IcoPath = _iconPath,
                }
            ];
        }
    }

    public List<Result> Query(Query query, bool delayedExecution)
    {
        if (!delayedExecution)
        {
            return Query(query);
        }

        var search = query.Search;

        if (!Guid.TryParse(search.Trim(), out var subscriptionId))
        {
            return Query(query);
        }

        if (_subscriptions.ContainsKey(subscriptionId))
        {
            return Query(query);
        }

        try
        {
            var subscription = MaestroClient.Subscriptions.GetSubscriptionAsync(subscriptionId).GetAwaiter().GetResult();
            if (subscription != null)
            {
                _subscriptions[subscriptionId] = subscription;
            }
            else
            {
                return
                [
                    new Result
                    {
                        Title = $"Subscription {subscriptionId} not found",
                        SubTitle = $"No subscription with ID {subscriptionId} exists in Maestro",
                        IcoPath = _iconPath,
                    }
                ];
            }
        }
        catch (Exception ex)
        {
            return 
            [
                new Result
                {
                    Title = $"Error loading subscription {subscriptionId}",
                    SubTitle = ex.Message,
                    IcoPath = _iconPath,
                }
            ];
        }

        return Query(query);
    }

    /// <summary>
    /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
    /// </summary>
    /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
    public void Init(PluginInitContext context)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Context.API.ThemeChanged += OnThemeChanged;
        UpdateIconPath(Context.API.GetCurrentTheme());

        // MaestroClient = PcsApiFactory.GetAuthenticated(accessToken: null, managedIdentityId: null, disableInteractiveAuth: false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
    /// </summary>
    /// <param name="disposing">Indicate that the plugin is disposed.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (Disposed || !disposing)
        {
            return;
        }

        if (Context?.API != null)
        {
            Context.API.ThemeChanged -= OnThemeChanged;
        }

        Disposed = true;
    }

    private void UpdateIconPath(Theme theme) => _iconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/maestro.png" : "Images/maestro.png";

    private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
}
