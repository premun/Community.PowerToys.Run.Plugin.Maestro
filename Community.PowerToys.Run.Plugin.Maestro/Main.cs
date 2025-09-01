using System;
using System.Collections.Generic;
using Community.PowerToys.Run.Plugin.Maestro.Helpers;
using Maestro.Common;
using ManagedCommon;
using Microsoft.DotNet.ProductConstructionService.Client;
using Microsoft.DotNet.ProductConstructionService.Client.Models;
using Wox.Infrastructure;
using Wox.Plugin;

using BrowserInfo = Wox.Plugin.Common.DefaultBrowserInfo;

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

    private PluginInitContext Context { get; set; } = null!;

    private IProductConstructionServiceApi? _maestroClient;

    private string _iconPath { get; set; } = null!;

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
                return DisplaySubscription(subscription);
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
            _maestroClient ??= PcsApiFactory.GetAuthenticated(accessToken: null, managedIdentityId: null, disableInteractiveAuth: false);
            var subscription = _maestroClient.Subscriptions.GetSubscriptionAsync(subscriptionId).GetAwaiter().GetResult();
            if (subscription != null)
            {
                _subscriptions[subscriptionId] = subscription;
                return DisplaySubscription(subscription);
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
    }

    private List<Result> DisplaySubscription(Subscription subscription)
    {
        var (sourceOwner, sourceRepo) = GitRepoUrlUtils.GetRepoNameAndOwner(subscription.SourceRepository);
        var (targetOwner, targetRepo) = GitRepoUrlUtils.GetRepoNameAndOwner(subscription.TargetRepository);

        string? codeflow = null;
        if (subscription.SourceEnabled)
        {
            codeflow = Environment.NewLine + (subscription.IsBackflow()
                ? $"Backflow ({subscription.TargetDirectory})"
                : $"Forward flow ({subscription.TargetDirectory})");
        }

        List<Result> results =
        [
            new Result
            {
                Title = $"{subscription.Channel.Name}",
                SubTitle =
                    $"""
                    {sourceOwner}/{sourceRepo} -> {targetOwner}/{targetRepo} @ {subscription.TargetBranch}
                    Enabled: {subscription.Enabled}{codeflow}
                    """,
                IcoPath = _iconPath,
                Action = _ => OpenUriInBrowser($"{ProductConstructionServiceApiOptions.ProductionMaestroUri}subscriptions?search={subscription.Id}&showDisabled=True"),
            }
        ];

        if (subscription.LastAppliedBuild != null)
        {
            var buildRepoType = GitRepoUrlUtils.ParseTypeFromUri(subscription.LastAppliedBuild.GetRepository());
            results.Add(new Result
            {
                Title = $"Commit: {subscription.LastAppliedBuild.Commit}",
                IcoPath = buildRepoType == GitRepoType.AzureDevOps ? Icons.AzureDevOps : Icons.GitHub,
                SubTitle = subscription.LastAppliedBuild.GetRepository(),
                Action = action => OpenUriInBrowser(GitRepoUrlUtils.GetRepoAtCommitUri(subscription.LastAppliedBuild.GetRepository(), subscription.LastAppliedBuild.Commit)),
            });

            results.Add(new Result
            {
                Title = $"Last applied build: {subscription.LastAppliedBuild.AzureDevOpsBuildNumber} / BAR ID: {subscription.LastAppliedBuild.Id}",
                IcoPath = Icons.AzureDevOps,
                SubTitle =
                    $"""
                    Applied {subscription.LastAppliedBuild.DateProduced.ToTimeAgo()}
                    """,

                Action = action => OpenUriInBrowser(subscription.LastAppliedBuild.GetBuildLink()),
            });
        }

        results.Add(new Result
        {
            Title = "Edit subscription",
            SubTitle = $"Edit subscription via darc",
            IcoPath = Icons.Edit,
            Action = _ => Helper.OpenCommandInShell("darc", string.Empty, $"update-subscription --id {subscription.Id}"),
        });

        return results;
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

    private void UpdateIconPath(Theme theme) => _iconPath = theme == Theme.Light || theme == Theme.HighContrastWhite
        ? Icons.Maestro
        // TODO Dark theme icon
        : Icons.Maestro;

    private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);

    private static bool OpenUriInBrowser(string uri)
    {
        Helper.OpenCommandInShell(BrowserInfo.Path, BrowserInfo.ArgumentsPattern, uri);
        return true;
    }
}
