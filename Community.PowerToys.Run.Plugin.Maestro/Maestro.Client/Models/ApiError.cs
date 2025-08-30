// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Community.PowerToys.Run.Plugin.Maestro.Maestro.Client.Models;

public class ApiError
{
    public ApiError()
    {
    }

    public string Message { get; set; }

    public List<string> Errors { get; set; }
}
