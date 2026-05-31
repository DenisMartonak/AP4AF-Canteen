using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Duende.AccessTokenManagement.OpenIdConnect;
using UTB.Minute.AdminClient.Components;
using UTB.Minute.AdminClient.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = ".UTB.Minute.Admin";
})
.AddKeycloakOpenIdConnect(
    serviceName: "keycloak",
    realm: "utb-minute",
    options =>
    {
        options.ClientId = "utb-minute-adminclient";
        options.ClientSecret = "qDW7aoS5LVNmQNqA6oTHNyBRp5Ahsdge";
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.Scope.Add("openid");
        options.Scope.Add("offline_access");
        options.SaveTokens = true;
        options.RequireHttpsMetadata = false; 
        options.TokenValidationParameters.NameClaimType = "preferred_username";
        
        options.Events = new OpenIdConnectEvents
        {
            OnTicketReceived = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    var accessToken = context.Properties?.GetTokenValue("access_token");
                    if (accessToken != null)
                    {
                        identity.AddClaim(new Claim("access_token", accessToken));
                    }
                }
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var principal = context.Principal;
                if (principal?.Identity is ClaimsIdentity identity)
                {
                    var realmAccessClaim = principal.FindFirst("realm_access");
                    
                    
                    if (realmAccessClaim == null && context.TokenEndpointResponse?.AccessToken != null)
                    {
                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(context.TokenEndpointResponse.AccessToken);
                        realmAccessClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "realm_access");
                    }

                    if (realmAccessClaim != null)
                    {
                        try 
                        {
                            var realmAccess = JsonDocument.Parse(realmAccessClaim.Value);
                            if (realmAccess.RootElement.TryGetProperty("roles", out var rolesElement))
                            {
                                foreach (var role in rolesElement.EnumerateArray())
                                {
                                    identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                                }
                            }
                        }
                        catch { }
                    }
                }
                return Task.CompletedTask;
            }
        };
    }
);

builder.Services.AddOpenIdConnectAccessTokenManagement(options =>
    options.RefreshBeforeExpiration = TimeSpan.FromSeconds(30)
);

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddServiceDiscovery();
builder.Services.ConfigureHttpClientDefaults(http => http.AddServiceDiscovery());


builder.Services.AddHttpClient("api", client => client.BaseAddress = new Uri("https://webapi"))
    .AddServiceDiscovery();


builder.Services.AddScoped<ApiClient>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/login", async (HttpContext ctx, string? returnUrl) =>
{
    string redirectUri = "/";
    if (!string.IsNullOrWhiteSpace(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
    {
        redirectUri = returnUrl;
    }

    await ctx.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = redirectUri,
        IsPersistent = false
    });
});

app.MapPost("/logout", async (HttpContext ctx) =>
{
    string? idToken = await ctx.GetTokenAsync("id_token");
    await ctx.RevokeRefreshTokenAsync();
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    await ctx.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
    {
        RedirectUri = "/",
        Parameters = { { "id_token_hint", idToken ?? string.Empty } }
    });
});

app.Run();

