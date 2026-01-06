namespace CoffeeShop.ViewModels.AdminVM
{
    // Thông báo chung khi danh sách món thay đổi
    public class ItemsChangedMessage
    {
        public string? Action { get; set; } // "Added", "Updated", "Deleted"
        public int ItemId { get; set; }
    }

    // Thông báo khi thêm món mới
    public class ItemAddedMessage
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int CategoryId { get; set; }
    }

    // Thông báo khi cập nhật món
    public class ItemUpdatedMessage
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int CategoryId { get; set; }
        public bool IsAvailable { get; set; }
    }

    // Thông báo khi xóa món
    public class ItemDeletedMessage
    {
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
    }
}
