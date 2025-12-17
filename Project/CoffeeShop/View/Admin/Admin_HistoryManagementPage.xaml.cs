using CoffeeShop.Models;
using CoffeeShop.View.General;
using CoffeeShop.ViewModels.AdminVM;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;


namespace CoffeeShop.View.Admin
{
    /// <summary>
    /// Interaction logic for HistoryManagementPage.xaml
    /// </summary>
    public partial class HistoryManagementPage : Page
    {
        public HistoryManagementPage()
        {
            InitializeComponent();
            this.DataContext = new AdminHistoryViewModel();
        }
    }
}
