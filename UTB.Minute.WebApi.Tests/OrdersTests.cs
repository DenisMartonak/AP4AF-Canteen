using System.Net;
using System.Net.Http.Json;
using UTB.Minute.Contracts;
using Xunit;

namespace UTB.Minute.WebApi.Tests;

[Collection("Database collection")]
public class OrdersTests(TestFixture fixture)
{
    [Fact]
    public async Task ProcessOrder_CreateAndChangeState()
    {
        var menuReq = new MenuItemRequestDto(DateOnly.FromDateTime(DateTime.Now), 1, 10);
        var menuRes = await fixture.HttpClient.PostAsJsonAsync("/api/menuitems", menuReq);

        Assert.Equal(HttpStatusCode.Created, menuRes.StatusCode);
        var createdMenu = await menuRes.Content.ReadFromJsonAsync<MenuItemDto>();

        var orderReq = new OrderRequestDto(createdMenu!.Id);

        var createRes = await fixture.HttpClient.PostAsJsonAsync("/api/orders", orderReq);
        Assert.Equal(HttpStatusCode.Created, createRes.StatusCode);
        var createdOrder = await createRes.Content.ReadFromJsonAsync<OrderDto>();

        var getRes = await fixture.HttpClient.GetAsync("/api/orders");
        Assert.Equal(HttpStatusCode.OK, getRes.StatusCode);

        var stateReq = new OrderStatePatchDto("Ready");
        var patchRes = await fixture.HttpClient.PatchAsJsonAsync($"/api/orders/{createdOrder!.Id}/state", stateReq);
        Assert.Equal(HttpStatusCode.NoContent, patchRes.StatusCode);
    }
}