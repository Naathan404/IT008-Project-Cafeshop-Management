using System.Windows;
using CoffeeShop.ViewModels.StaffVM;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for ReportDepotWindow.xaml
    /// </summary>
    public partial class ReportDepotWindow : Window
    {
        public ReportDepotWindow()
        {
            InitializeComponent();
            this.DataContext = new ReportDepotViewModel();
        }
    }
}
