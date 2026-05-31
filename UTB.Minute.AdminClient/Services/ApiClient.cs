using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using UTB.Minute.Contracts;

namespace UTB.Minute.AdminClient.Services;

public class ApiClient(IHttpClientFactory factory, AuthenticationStateProvider authProvider)
{
    private async Task<HttpClient> GetClientAsync()
    {
        var client = factory.CreateClient("api");
        var authState = await authProvider.GetAuthenticationStateAsync();
        var token = authState.User.FindFirst("access_token")?.Value;
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    
    public async Task<MealDto[]?> GetMealsAsync() => await (await GetClientAsync()).GetFromJsonAsync<MealDto[]>("/api/meals");
    public async Task CreateMealAsync(MealRequestDto dto) { var r = await (await GetClientAsync()).PostAsJsonAsync("/api/meals", dto); r.EnsureSuccessStatusCode(); }
    public async Task UpdateMealAsync(int id, MealPutRequestDto dto) { var r = await (await GetClientAsync()).PutAsJsonAsync($"/api/meals/{id}", dto); r.EnsureSuccessStatusCode(); }
    public async Task PatchMealActiveAsync(int id, MealPatchActiveDto dto) { var r = await (await GetClientAsync()).PatchAsJsonAsync($"/api/meals/{id}", dto); r.EnsureSuccessStatusCode(); }

    
    public async Task<MenuItemDto[]?> GetMenuItemsAsync() => await (await GetClientAsync()).GetFromJsonAsync<MenuItemDto[]>("/api/menuitems");
    public async Task CreateMenuItemAsync(MenuItemRequestDto dto) { var r = await (await GetClientAsync()).PostAsJsonAsync("/api/menuitems", dto); r.EnsureSuccessStatusCode(); }
    public async Task UpdateMenuItemAsync(int id, MenuItemPutRequestDto dto) { var r = await (await GetClientAsync()).PutAsJsonAsync($"/api/menuitems/{id}", dto); r.EnsureSuccessStatusCode(); }
    public async Task DeleteMenuItemAsync(int id) { var r = await (await GetClientAsync()).DeleteAsync($"/api/menuitems/{id}"); r.EnsureSuccessStatusCode(); }
}

