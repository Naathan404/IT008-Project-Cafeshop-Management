using CoffeeShop.Service.DTOs;
using CoffeeShop.View.Staff;
using CoffeeShop.ViewModels.AdminVM;
using System.Collections.ObjectModel;

namespace CoffeeShop.Service.Interfaces
{
    public interface IDialogService
    {
        public bool? OpenInsertMaterialWindow(ObservableCollection<DepotItemDTO> depotItems,
            DepotItemDTO? itemToEdit);

        public void OpenReportDepotWindow();

        public bool? OpenInsertCouponWindow(CouponDTO? itemToEdit = null);
    }
}
