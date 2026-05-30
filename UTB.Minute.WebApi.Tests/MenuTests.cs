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

        var getRes = await fixture.HttpClient.GetAsync("/api/menuitems");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var updateReq = new MenuItemPutRequestDto(created!.Date, 50);
        var updateRes = await fixture.HttpClient.PutAsJsonAsync($"/api/menuitems/{created.Id}", updateReq);
        Assert.Equal(HttpStatusCode.NoContent, updateRes.StatusCode);

        var deleteRes = await fixture.HttpClient.DeleteAsync($"/api/menuitems/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteRes.StatusCode);
    }
}