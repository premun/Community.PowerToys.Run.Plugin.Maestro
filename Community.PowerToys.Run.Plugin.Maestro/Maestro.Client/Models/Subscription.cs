// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;

#nullable disable
namespace Community.PowerToys.Run.Plugin.Maestro.Maestro.Client.Models;

public class Subscription(
    Guid id,
    bool enabled,
    bool sourceEnabled,
    string sourceRepository,
    string targetRepository,
    string targetBranch,
    string sourceDirectory,
    string targetDirectory,
    string pullRequestFailureNotificationTags,
    List<string> excludedAssets)
{
    public Guid Id { get; } = id;

    public Channel Channel { get; set; }

    public string SourceRepository { get; } = sourceRepository;

    public string TargetRepository { get; } = targetRepository;

    public string TargetBranch { get; } = targetBranch;

    public bool Enabled { get; } = enabled;

    public bool SourceEnabled { get; } = sourceEnabled;

    public string SourceDirectory { get; } = sourceDirectory;

    public string TargetDirectory { get; } = targetDirectory;

    public string PullRequestFailureNotificationTags { get; } = pullRequestFailureNotificationTags;

    public List<string> ExcludedAssets { get; } = excludedAssets;

    public bool IsBackflow() => SourceEnabled && !string.IsNullOrEmpty(SourceDirectory);

    public bool IsForwardFlow() => SourceEnabled && !string.IsNullOrEmpty(TargetDirectory);
}
