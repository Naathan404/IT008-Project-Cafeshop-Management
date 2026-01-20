using CoffeeShop.ViewModels.AdminVM;
using CoffeeShop.ViewModels.StaffVM;
using System.Windows;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for OrderHistoryReportPreview.xaml
    /// </summary>
    public partial class OrderHistoryReportPreview : Window
    {
        public OrderHistoryReportPreview(List<AdminHistoryViewModel.OrderHistory> orders)
        {
            InitializeComponent();
            this.DataContext = new OrderHistoryReportViewModel(orders);
        }
    }
}
