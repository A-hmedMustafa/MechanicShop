using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

namespace MechanicShop.Client.Hubs;

public class WorkOrderHubClient : IAsyncDisposable
{

    private readonly HubConnection _hubConnection;
    private bool _isStarted;
    private bool _isDisposed;

    public WorkOrderHubClient(IWebAssemblyHostEnvironment environment)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl($"{environment.BaseAddress}hubs/workorders")
            .WithAutomaticReconnect()
            .Build();
    }
    
    public async Task StartAsync(Func<Task> onWorkOrderChanged)
    {
        if(_isStarted || _isDisposed)
            return;

        _hubConnection.On("WorkOrderChanged", async () =>
        {
            if(!_isDisposed)
                await onWorkOrderChanged.Invoke();
        });    
        await _hubConnection.StartAsync();
        _isStarted = true;
    }

    public async ValueTask DisposeAsync()
    {
        if(_isDisposed)
            return;

        _isDisposed = true;

        if(_hubConnection.State is HubConnectionState.Connected or HubConnectionState.Connecting)
            await _hubConnection.StopAsync();

        await _hubConnection.DisposeAsync();        
    }
}