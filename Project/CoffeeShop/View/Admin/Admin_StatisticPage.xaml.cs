using System.Windows.Controls;
using CoffeeShop.ViewModels.AdminVM;

namespace CoffeeShop.View.Admin
{
    public partial class StatisticPage : Page
    {
        public StatisticPage()
        {
            InitializeComponent();

            this.DataContext = new AdminStatisticsViewModel();
        }
    }
}
