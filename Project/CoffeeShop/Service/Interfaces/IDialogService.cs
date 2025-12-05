using System.Collections.ObjectModel;
using CoffeeShop.ViewModels.StaffVM;

namespace CoffeeShop.Service.Interfaces
{
    public interface IDialogService
    {
        public void OpenInsertMaterialWindow(ObservableCollection<StaffDepotViewModel.DepotItem> depotItems,
            StaffDepotViewModel.DepotItem? itemToEdit);

        public void OpenDepotHistoryWindow(); 
    }
}
