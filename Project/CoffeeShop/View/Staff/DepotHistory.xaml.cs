using System.Windows;
using CoffeeShop.Service;
using CoffeeShop.Service.Interfaces;
using CoffeeShop.ViewModels.StaffVM;
using System.Windows.Controls;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for DepotHistory.xaml
    /// </summary>
    public partial class DepotHistory : Window
    {
        public DepotHistory()
        {
            InitializeComponent();
            this.DataContext = new DepotHistoryViewModel();
        }
    }
}
