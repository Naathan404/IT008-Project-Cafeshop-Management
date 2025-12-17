using CoffeeShop.Service;
using CoffeeShop.Service.Interfaces;
using CoffeeShop.ViewModels.StaffVM;
using System.Windows.Controls;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for Staff_Depot.xaml
    /// </summary>
    public partial class Staff_Depot : Page
    {
        public Staff_Depot()
        {
            InitializeComponent();
            IDialogService dialogService = new WindowService();
            this.DataContext = new StaffDepotViewModel(dialogService);
        }
    }
}
