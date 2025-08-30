using ManagedCommon;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.Maestro;

/// <summary>
/// Main class of this plugin that implement all used interfaces.
/// </summary>
public class Main : IPlugin, IContextMenu, IDisposable
{
    /// <summary>
    /// ID of the plugin.
    /// </summary>
    public static string PluginID => "077861CDA5AB4F12BB04C3C3B8AD3517";

    public string Name => ".NET Maestro";

    public string Description => "Manage Maestro subscriptions (https://maestro.dot.net/)";

    private PluginInitContext Context { get; set; }

    private string IconPath { get; set; }

    private bool Disposed { get; set; }

    /// <summary>
    /// Return a filtered list, based on the given query.
    /// </summary>
    /// <param name="query">The query to filter the list.</param>
    /// <returns>A filtered list, can be empty when nothing was found.</returns>
    public List<Result> Query(Query query)
    {
        var search = query.Search;

        return
        [
            new Result
            {
                QueryTextDisplay = search,
                IcoPath = IconPath,
                Title = "Title: " + search,
                SubTitle = "SubTitle",
                ToolTipData = new ToolTipData("Title", "Text"),
                Action = _ =>
                {
                    Clipboard.SetDataObject(search);
                    return true;
                },
                ContextData = search,
            }
        ];
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

    /// <summary>
    /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
    /// </summary>
    /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
    /// <returns>A list context menu entries.</returns>
    public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
    {
        if (selectedResult.ContextData is string search)
        {
            return
            [
                new ContextMenuResult
                {
                    PluginName = Name,
                    Title = "Copy to clipboard (Ctrl+C)",
                    FontFamily = "Segoe MDL2 Assets",
                    Glyph = "\xE8C8", // Copy
                    AcceleratorKey = Key.C,
                    AcceleratorModifiers = ModifierKeys.Control,
                    Action = _ =>
                    {
                        Clipboard.SetDataObject(search);
                        return true;
                    },
                }
            ];
        }

        return [];
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

    private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/maestro.light.png" : "Images/maestro.dark.png";

    private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);
}
