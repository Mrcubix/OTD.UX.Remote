using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Eto.Forms;
using OpenTabletDriver.External.Common.RPC;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.UX;
using OTD.UX.Remote.Lib;

#pragma warning disable CA2255

namespace OTD.UX.Remote;

[PluginName("UX Remote")]
public class UXRemote : IUXRemote, ITool
{
    private static CancellationTokenSource _tokenSource = new();
    private static RpcServer<UXRemote> Host { get; } = new("OTD.UX.Remote");
    private static UXRemote? Instance => Host?.Instance;
    private static bool HasStarted { get; set; }

    [ModuleInitializer]
    public static void InitializeCore()
    {
        if (HasStarted)
            return;

        HasStarted = true;

        _ = Task.Run(Host.MainAsync, _tokenSource.Token);

        Application.Instance.Terminating += OnTerminating;
    }

    public bool Initialize() => true;
    
    public async Task Synchronize()
    {
        if (App.Driver == null || App.Driver.Instance == null)
            return;
#if NET5_0
        App.Settings = await App.Driver.Instance.GetSettings();
#elif NET6_0
        App.Current.Settings = await App.Driver.Instance.GetSettings();
#endif
    }

    public Task SendNotification(string message)
    {
        if (Application.Instance == null)
            return Task.CompletedTask;

        ShowNotification(null, message);

        return Task.CompletedTask;
    }

    public Task SendNotification(string title, string message)
    {
        if (Application.Instance == null)
            return Task.CompletedTask;

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

    public static void OnTerminating(object? sender, EventArgs e)
    {
        _tokenSource.Cancel();
        _tokenSource.Dispose();
    }

    public void Dispose() {}
}
