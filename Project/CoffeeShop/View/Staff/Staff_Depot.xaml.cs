using CoffeeShop.Service;
using CoffeeShop.Service.Interfaces;
using CoffeeShop.ViewModels.StaffVM;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            // 2. Truyền Service cụ thể đó vào Constructor của ViewModel
            this.DataContext = new StaffDepotViewModel(dialogService);
        }
        // Hàm giúp đóng mở popup filter
        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (popupFilterBorder.Visibility == Visibility.Collapsed)
            {
                popupFilterBorder.Visibility = Visibility.Visible;
            }
            else
            {
                popupFilterBorder.Visibility = Visibility.Collapsed;
            }
            e.Handled = true;
        }
        // Hàm áp dụng bộ lọc
    }
}
