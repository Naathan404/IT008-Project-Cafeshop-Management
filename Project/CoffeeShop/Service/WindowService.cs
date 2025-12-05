using CoffeeShop.Service.Interfaces;
using CoffeeShop.View.Staff;    // Dùng InputWindow (View)
using CoffeeShop.ViewModels.StaffVM;
using System.Collections.ObjectModel;

namespace CoffeeShop.Service
{
    public class WindowService : IDialogService
    {
        // Triển khai hàm theo đúng hợp đồng của Interface
        public void OpenInputWindow(ObservableCollection<StaffDepotViewModel.DepotItem> collection,
                                 StaffDepotViewModel.DepotItem? itemToEdit)
        {
            if (itemToEdit != null)
            {
                // CHẾ ĐỘ CẬP NHẬT: Dùng constructor 2 (truyền đối tượng đang sửa)
                InsertMaterial insertMaterial = new InsertMaterial(itemToEdit, collection);
                insertMaterial.ShowDialog();
            }
            else
            {
                // CHẾ ĐỘ THÊM MỚI: Dùng constructor 1
                InsertMaterial insertMaterial = new InsertMaterial(collection);
                insertMaterial.ShowDialog();
            }
        }
    }
}
