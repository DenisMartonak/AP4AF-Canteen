using System.Net;
using System.Net.Http.Json;
using UTB.Minute.Contracts;

namespace UTB.Minute.WebApi.Tests;

[Collection("Database collection")]
public class MenuTests(TestFixture fixture)
{
    [Fact]
    public async Task ManageMenu_FullCrudCycle()
    {
        var request = new MenuItemRequestDto(DateOnly.FromDateTime(DateTime.Now), 1, 20);
        var createRes = await fixture.HttpClient.PostAsJsonAsync("/api/menuitems", request);
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var created = await createRes.Content.ReadFromJsonAsync<MenuItemDto>();
        Assert.NotNull(created);
        Assert.Equal(20, created.AvailablePortions);

        var getRes = await fixture.HttpClient.GetAsync("/api/menuitems");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);
        var items = await getRes.Content.ReadFromJsonAsync<MenuItemDto[]>();
        Assert.Contains(items!, m => m.Id == created.Id);

        var updateReq = new MenuItemPutRequestDto(created.Date, 50);
        var updateRes = await fixture.HttpClient.PutAsJsonAsync($"/api/menuitems/{created.Id}", updateReq);
        Assert.Equal(HttpStatusCode.NoContent, updateRes.StatusCode);
        
        var getUpdatedRes = await fixture.HttpClient.GetAsync("/api/menuitems");
        var updatedItems = await getUpdatedRes.Content.ReadFromJsonAsync<MenuItemDto[]>();
        Assert.Equal(50, updatedItems!.First(m => m.Id == created.Id).AvailablePortions);

        var deleteRes = await fixture.HttpClient.DeleteAsync($"/api/menuitems/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);
    }

    [Fact]
    public async Task CreateMenuItem_InvalidMealId_ReturnsBadRequest()
    {
        var request = new MenuItemRequestDto(DateOnly.FromDateTime(DateTime.Now), 99999, 20);
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/menuitems", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMenuItem_NonExistent_ReturnsNotFound()
    {
        var updateReq = new MenuItemPutRequestDto(DateOnly.FromDateTime(DateTime.Now), 50);
        var response = await fixture.HttpClient.PutAsJsonAsync($"/api/menuitems/99999", updateReq);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteMenuItem_NonExistent_ReturnsNotFound()
    {
        var response = await fixture.HttpClient.DeleteAsync($"/api/menuitems/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
