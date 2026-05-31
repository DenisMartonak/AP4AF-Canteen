namespace UTB.Minute.Contracts
{
    public record MealDto(int Id, string Name, string Description, decimal Price, bool IsActive);
    public record MealRequestDto(string Name, string Description, decimal Price);
    public record MealPutRequestDto(string Name, string Description, decimal Price);
    public record MealPatchActiveDto(bool IsActive);


    public record MenuItemDto(int Id, DateOnly Date, int MealId, string MealName, int AvailablePortions, decimal Price);
    public record MenuItemRequestDto(DateOnly Date, int MealId, int AvailablePortions);
    public record MenuItemPutRequestDto(DateOnly Date, int AvailablePortions);


    public record OrderDto(int Id, int MenuItemId, string MealName, string State);
    public record OrderRequestDto(int MenuItemId);
    public record OrderStatePatchDto(string State);
}
