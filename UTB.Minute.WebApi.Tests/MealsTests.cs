using System.Net;
using System.Net.Http.Json;
using UTB.Minute.Contracts;

namespace UTB.Minute.WebApi.Tests;

[Collection("Database collection")]
public class MealsTests(TestFixture fixture)
{
    [Fact]
    public async Task GetMeals_ReturnsOkAndSeededData()
    {
        var response = await fixture.HttpClient.GetAsync("/api/meals");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var meals = await response.Content.ReadFromJsonAsync<MealDto[]>();
        Assert.NotNull(meals);
        Assert.Contains(meals, m => m.Name == "Testovací polévka");
    }

    [Fact]
    public async Task CreateMeal_AddsNewMealSuccessfully()
    {
        var request = new MealRequestDto("Halušky", "Slovenské národní jídlo", 130);
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/meals", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<MealDto>();
        Assert.NotNull(created);
        Assert.Equal("Halušky", created.Name);
    }

    [Fact]
    public async Task UpdateMeal_ModifiesExistingMeal()
    {
        var createRes = await fixture.HttpClient.PostAsJsonAsync("/api/meals", new MealRequestDto("Staré", "Popis", 100));
        var created = await createRes.Content.ReadFromJsonAsync<MealDto>();

        var updateRequest = new MealPutRequestDto("Nové", "Upravený popis", 110);
        var response = await fixture.HttpClient.PutAsJsonAsync($"/api/meals/{created!.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        var getRes = await fixture.HttpClient.GetAsync("/api/meals");
        var meals = await getRes.Content.ReadFromJsonAsync<MealDto[]>();
        var updatedMeal = meals!.First(m => m.Id == created!.Id);
        
        Assert.Equal("Nové", updatedMeal.Name);
        Assert.Equal("Upravený popis", updatedMeal.Description);
        Assert.Equal(110, updatedMeal.Price);
    }

    [Fact]
    public async Task PatchMeal_DeactivatesMealSuccessfully()
    {
        var createRes = await fixture.HttpClient.PostAsJsonAsync("/api/meals", new MealRequestDto("Na zmazanie", "...", 50));
        var created = await createRes.Content.ReadFromJsonAsync<MealDto>();

        var patchRequest = new MealPatchActiveDto(false);
        var response = await fixture.HttpClient.PatchAsJsonAsync($"/api/meals/{created!.Id}", patchRequest);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var getRes = await fixture.HttpClient.GetAsync("/api/meals");
        var meals = await getRes.Content.ReadFromJsonAsync<MealDto[]>();
        var patchedMeal = meals!.First(m => m.Id == created!.Id);
        
        Assert.False(patchedMeal.IsActive);
    }

    [Fact]
    public async Task CreateMeal_NegativePriceOrMissingName_ReturnsBadRequest()
    {
        var request = new MealRequestDto("", "Chybí jméno", 100);
        var response = await fixture.HttpClient.PostAsJsonAsync("/api/meals", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var request2 = new MealRequestDto("Jméno", "Popis", -50);
        var response2 = await fixture.HttpClient.PostAsJsonAsync("/api/meals", request2);
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }
}
