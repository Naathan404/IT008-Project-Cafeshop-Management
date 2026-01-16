using CoffeeShop.View.Staff;
using System.Collections.ObjectModel;
using CoffeeShop.Service.DTOs;

namespace CoffeeShop.Service.Interfaces
{
    public interface IDialogService
    {
        public bool? OpenInsertMaterialWindow(ObservableCollection<DepotItemDTO> depotItems,
            DepotItemDTO? itemToEdit);

        public void OpenDepotHistoryWindow();

        public void OpenReportDepotWindow();

        public bool? OpenInsertCouponWindow(CouponDTO? itemToEdit = null);
    }
}
