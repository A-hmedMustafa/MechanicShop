using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace MechanicShop.Client.Services;

public sealed class TimeZoneService(IJSRuntime jsRuntime)
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async Task<string> GetLocalTimeZoneAsync()
    {
        return await _jsRuntime.InvokeAsync<string>("getLocalTimeZone");
    }
}