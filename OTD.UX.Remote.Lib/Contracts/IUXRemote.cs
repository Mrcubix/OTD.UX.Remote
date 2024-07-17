namespace OTD.UX.Remote.Lib;

public interface IUXRemote
{
    public Task Synchronize();

    public Task SendNotification(string message);

    public Task SendNotification(string title, string message);
}
