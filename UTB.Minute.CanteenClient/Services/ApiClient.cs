using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using UTB.Minute.Contracts;

namespace UTB.Minute.CanteenClient.Services;

public class ApiClient(IHttpClientFactory factory, AuthenticationStateProvider authProvider)
{
    public async Task<HttpClient> GetClientAsync()
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
    public async Task CreateMealAsync(MealRequestDto dto) => await (await GetClientAsync()).PostAsJsonAsync("/api/meals", dto);
    public async Task UpdateMealAsync(int id, MealPutRequestDto dto) => await (await GetClientAsync()).PutAsJsonAsync($"/api/meals/{id}", dto);
    public async Task PatchMealActiveAsync(int id, MealPatchActiveDto dto) => await (await GetClientAsync()).PatchAsJsonAsync($"/api/meals/{id}", dto);

    
    public async Task<MenuItemDto[]?> GetMenuItemsAsync() => await (await GetClientAsync()).GetFromJsonAsync<MenuItemDto[]>("/api/menuitems");
    public async Task CreateMenuItemAsync(MenuItemRequestDto dto) => await (await GetClientAsync()).PostAsJsonAsync("/api/menuitems", dto);
    public async Task UpdateMenuItemAsync(int id, MenuItemPutRequestDto dto) => await (await GetClientAsync()).PutAsJsonAsync($"/api/menuitems/{id}", dto);
    public async Task DeleteMenuItemAsync(int id) => await (await GetClientAsync()).DeleteAsync($"/api/menuitems/{id}");
    public async Task<MenuItemDto[]?> GetMenuAsync() => await (await GetClientAsync()).GetFromJsonAsync<MenuItemDto[]>("/api/menu");

    public async Task CreateOrderAsync(OrderRequestDto dto)
    {
        var response = await (await GetClientAsync()).PostAsJsonAsync("/api/orders", dto);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {error}");
        }
    }

    
    public async Task<OrderDto[]?> GetOrdersAsync() 
    {
        var client = await GetClientAsync();
        return await client.GetFromJsonAsync<OrderDto[]>("/api/orders");
    }

    public async Task PatchOrderStateAsync(int id, OrderStatePatchDto dto)
    {
        var client = await GetClientAsync();
        if (client.DefaultRequestHeaders.Authorization == null)
            throw new HttpRequestException("Token z AuthenticationState byl NULL! Claim access_token chybí.");

        var response = await client.PatchAsJsonAsync($"/api/orders/{id}/state", dto);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {error}");
        }
    }
}

