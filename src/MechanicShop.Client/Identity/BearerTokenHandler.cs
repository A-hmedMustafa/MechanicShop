using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace MechanicShop.Client.Identity;

public class BearerTokenHandler(IAccountManagement accountManagement) : DelegatingHandler
{
    private readonly IAccountManagement _accountManagement = accountManagement;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var authResult = await _accountManagement.LoadAccessTokenFromStorageAsync();
        if(authResult.AccessToken is null)
            return await base.SendAsync(request, cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authResult.AccessToken);

        var response = await SendAsync(request, cancellationToken);
        if(response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !response.Headers.Contains("X-Retry"))
        {
            var newToken = await _accountManagement.RefreshTokenAsync();
            if(newToken is not null)
            {
                var newRequest = await CloneRequest(request, cancellationToken);
                newRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken.AccessToken);
                newRequest.Headers.Add("X-Retry", "true");
                return await base.SendAsync(newRequest, cancellationToken);
            }

            await _accountManagement.LogoutAsync();
        } 
        return response;
    }
    private static async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage oldRequest, CancellationToken cancellationToken)
    {
        var newRequest = new HttpRequestMessage(oldRequest.Method, oldRequest.RequestUri)
        {
            Version = oldRequest.Version
        };

        if(oldRequest.Content != null)
        {
            var memoryStream = new MemoryStream();
            oldRequest.Content.CopyToAsync(memoryStream).Wait();
            memoryStream.Position = 0;
            newRequest.Content = new StreamContent(memoryStream);

            foreach(var header in oldRequest.Content.Headers)
            {
                newRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach(var header in oldRequest.Headers)
        {
            newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return newRequest;
    }
}