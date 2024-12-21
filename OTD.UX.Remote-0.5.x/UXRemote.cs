using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Eto.Drawing;
using Eto.Forms;
using OpenTabletDriver.Desktop;
using OpenTabletDriver.External.Common.RPC;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.UX;
using OpenTabletDriver.UX.Controls;
using OTD.UX.Remote.Lib;
using OTD.UX.Remote.Lib.Extensions;

#pragma warning disable CA2255

namespace OTD.UX.Remote;

[PluginName("UX Remote")]
public class UXRemote : IUXRemote, ITool
{
    public static readonly Type TypeInfo = typeof(UXRemote).GetTypeInfo();
    public static Type? TabletSwitcherTypeInfo { get; set; }

    private static CancellationTokenSource _tokenSource = new();
    private static RpcServer<UXRemote> Host { get; set; } = new("OTD.UX.Remote");
    private static UXRemote? Instance => Host?.Instance;
    private static bool HasStarted { get; set; }

    #region static initialization

    [ModuleInitializer]
    public static void ModuleInitialize()
    {
        if (CheckIfUX() && InitializeCore())
            Log.Write("UX Remote", "UX Remote started successfully.");
    }

    private static bool CheckIfUX()
    {
        return AppDomain.CurrentDomain.GetAssemblies()
                                      .Any(x => x.GetName().Name == "OpenTabletDriver.UX");
    }

    public static bool InitializeCore()
    {
        if (HasStarted)
            return false;

        HasStarted = true;
        TabletSwitcherTypeInfo = typeof(App).Assembly.GetType("OpenTabletDriver.UX.Controls.TabletSwitcherPanel+TabletSwitcher");

        Application.Instance.Terminating += OnTerminating;

#if NET5_0
        App.SettingsChanged += OnSettingsChanged;
#elif NET6_0
        App.Current.PropertyChanged += OnPropertyChanged;
#endif

        OnSettingsChanged(GetSettings());

        return true;
    }

    private static void OnSettingsChanged(Settings? settings)
    {
        if (settings == null)
            return;

        switch (IsEnabled(), Host)
        {
            case (true, null):
                Host = new RpcServer<UXRemote>("OTD.UX.Remote");
                Host.ConnectionStateChanged += OnConnectionStateChanged;
                _ = Task.Run(Host.MainAsync, _tokenSource.Token);
                ShowNotification(null, "RPC Server started.");
                break;
            case (true, _):
                _ = Task.Run(Host.MainAsync, _tokenSource.Token);
                break;
            case (false, _):
                Host.Dispose();
                break;
        }
    }

#if NET6_0
    public static void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(App.Settings))
            OnSettingsChanged(App.Current?.Settings);
    }
#endif

    private static void OnConnectionStateChanged(object? sender, bool isConnected)
    {
        Log.Debug("UX Remote", isConnected ? "New client connected." : "Client disconnected.");
    }

    #endregion

    public bool Initialize() => true;

    public Task Synchronize()
    {
        _ = Task.Run(SynchronizeCore);
        return Task.CompletedTask;
    }

    public async Task SynchronizeCore()
    {
        if (App.Driver == null || App.Driver.Instance == null)
            return;

        if (await App.Driver.Instance.GetSettings() is not Settings settings)
            return;

        Application.Instance.AsyncInvoke(() => SetSettings(settings));

#if NET6_0
        // Above isn't enough in 0.6.x, as for some reasons, it doesn't update the OutputMode Page
        if (Application.Instance.MainForm is not MainForm mainForm || TabletSwitcherTypeInfo == null)
            return;

        // Hope mainForm.Content is TabletSwitcherPanel
        if (mainForm.Content is not TabletSwitcherPanel tabletSwitcherPanel)
            return;

        var instance = typeof(TabletSwitcherPanel).GetValue<object>(tabletSwitcherPanel, "tabletSwitcher");

        Application.Instance.AsyncInvoke(() => SetProfiles(instance, settings));
#endif
    }

    public Task SendNotification(string message)
        => SendNotification(null!, message);

    public Task SendNotification(string title, string message)
    {
        if (Application.Instance != null)
            ShowNotification(title, message);

        return Task.CompletedTask;
    }

    private static void ShowNotification(string? title, string message)
    {
        var notification = new Notification
        {
            Title = title ?? "OpenTabletDriver",
            Message = message,
            ContentImage = App.Logo,
            ID = "log-message-notification"
        };

        notification.Show();
    }

    #region Static Helper Methods

    private static bool IsEnabled()
    {
        var settings = GetSettings();

        var store = settings?.Tools.FirstOrDefault(x => x.Path == TypeInfo?.FullName);

        return store != null && store.Enable == true;
    }

    public static Settings? GetSettings()
    {
#if NET5_0
        return App.Settings;
#elif NET6_0
        return App.Current.Settings;
#endif
    }

    public static void SetSettings(Settings settings)
    {
#if NET5_0
        App.Settings = settings;
#elif NET6_0
        App.Current.Settings = settings;
#endif
    }

#if NET6_0
    public static void SetProfiles(object? instance, Settings settings)
    {
        // TabletSwitcher is a private class of TabletSwitcherPanel
        TabletSwitcherTypeInfo?.SetPropertyValue(instance, "Profiles", settings.Profiles);
    }
#endif

    public static void OnTerminating(object? sender, EventArgs e)
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
    }

    #endregion

    public void Dispose() { }
}
