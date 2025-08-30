// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Community.PowerToys.Run.Plugin.Maestro.Maestro.Client.Models;

public class Channel(int id, string name, string classification)
{
    public int Id { get; } = id;

    public string Name { get; } = name;

    public string Classification { get; } = classification;
}
