using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeShop.ViewModels.StaffVM;

namespace CoffeeShop.Service.Interfaces
{
    public interface IDialogService
    {
        void OpenInputWindow(ObservableCollection<StaffDepotViewModel.DepotItem> depotItems,
            StaffDepotViewModel.DepotItem? itemToEdit);
    }
}
