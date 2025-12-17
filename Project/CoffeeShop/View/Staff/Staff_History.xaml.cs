using CoffeeShop.Models;
using CoffeeShop.View.General;
using CoffeeShop.ViewModels.StaffVM;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for Staff_History.xaml
    /// </summary>
    public partial class Staff_History : Page
    {
        public Staff_History()
        {
            InitializeComponent();
            this.DataContext = new StaffHistoryViewModel();
        }
    }
}
