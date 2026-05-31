namespace UTB.Minute.Db
{
    public class Order
    {
        public int Id { get; set; }
        public OrderState State { get; set; } = OrderState.Preparing;
        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }

    }
}

