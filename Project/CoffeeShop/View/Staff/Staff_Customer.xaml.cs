using CoffeeShop.ViewModels.StaffVM;
using System.Windows.Controls;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for Staff_Customer.xaml
    /// </summary>
    public partial class Staff_Customer : Page
    {
        public Staff_Customer()
        {
            InitializeComponent();
            this.DataContext = new CustomerViewModel();
        }
    }
}
