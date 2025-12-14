using CoffeeShop.View.Staff;
using System.Collections.ObjectModel;
using CoffeeShop.Models;

namespace CoffeeShop.Service.Interfaces
{
    public interface IDialogService
    {
        public void OpenInsertMaterialWindow(ObservableCollection<Models.DepotItem> depotItems,
            Models.DepotItem? itemToEdit);

        public void OpenDepotHistoryWindow();

        public void OpenReportDepotWindow(List<DepotItem> reportData, string reportPath);
    }
}
