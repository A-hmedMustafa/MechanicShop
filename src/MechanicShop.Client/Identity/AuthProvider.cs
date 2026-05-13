using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace MechanicShop.Client.Identity;

public class AuthProvider(
    ILocalStorageService localStorageService,
    IHttpClientFactory httpClientFactory,
    ILogger<AuthProvider> logger
) : AuthenticationStateProvider, IAccountManagement
{

    private readonly ILocalStorageService _localStorageService = localStorageService;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILogger<AuthProvider> _logger = logger;
    private readonly JsonSerializerOptions serializerOptions = new () {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
    private readonly ClaimsPrincipal emptyPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
    private bool isAuthenticated = false;

    public async Task<FormResult> LoginAsync(string email, string password)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("MechanicShopClient");
            var result = await httpClient.PostAsJsonAsync(
                "/identity/token/generate", new {email, password});

            if (result.IsSuccessStatusCode)
            {
                var response = await result.Content.ReadFromJsonAsync<TokenResponse>(serializerOptions);
                await _localStorageService.SetItemAsync("authResult", response);
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return new FormResult { IsSuccess = true };
            }        
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login.");
        }

        return new FormResult
        {
            IsSuccess = false,
            ErrorList = ["Invalid email and/or password."]
        };
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        isAuthenticated = false;
        var user = emptyPrincipal;

        try
        {
            var httpClient = _httpClientFactory.CreateClient("MechanicShopClient");
            var result = await httpClient.GetAsync("/identity/current-user/claims");
            result.EnsureSuccessStatusCode();

            var response = await result.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<UserInfo>(response, serializerOptions);
            if(userInfo != null)
            {
                List<Claim> claims = [
                    new (ClaimTypes.NameIdentifier, userInfo.UserId),
                    new (ClaimTypes.Name, userInfo.Email),
                    new (ClaimTypes.Email, userInfo.Email)
                ];

                foreach(var role in userInfo.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                claims.AddRange(userInfo.Claims);
                var identity = new ClaimsIdentity(claims, nameof(AuthProvider));
                user = new ClaimsPrincipal(identity);
                isAuthenticated = true;
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "App error");
        }

        return new AuthenticationState(user);
    }

    public async Task LogoutAsync()
    {
        await _localStorageService.RemoveItemAsync("authResult");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<TokenResponse?> RefreshTokenAsync()
    {
        var oldRefreshToken = await _localStorageService.GetItemAsync<TokenResponse>("authResult");
        if (oldRefreshToken.RefreshToken is null)
            return null;
        
        var httpClient = _httpClientFactory.CreateClient("MechanicShopClient");
        var result = await httpClient.PostAsJsonAsync(
            "/identity/token/refresh-token", new {
                ExpiredAccessToken = oldRefreshToken.AccessToken,
                RefreshToken = oldRefreshToken.RefreshToken});

        if(!result.IsSuccessStatusCode)
        {
            return null;
        }

        var newToken = await result.Content.ReadFromJsonAsync<TokenResponse>();              
        if(newToken is null || newToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            return null;
        }
        await _localStorageService.SetItemAsync("authResult", newToken);
        return newToken;
    }
    
    public async Task<bool> CheckAuthenticatedAsync()
    {
        await GetAuthenticationStateAsync();
        return isAuthenticated;
    }
    public async Task<TokenResponse> LoadAccessTokenFromStorageAsync()
    {
        return await _localStorageService.GetItemAsync<TokenResponse>("authResult");
    }

  
}