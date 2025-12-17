using CoffeeShop.Models;
using CoffeeShop.ViewModels.GeneralVM;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Esf;
using System.Globalization;
using System.Windows;

namespace CoffeeShop.View.General
{
    /// <summary>
    /// Interaction logic for OrderDetails.xaml
    /// </summary>
    public partial class OrderDetailWindow : Window
    {
        public OrderDetailWindow(int orderID)
        {
            InitializeComponent();
            this.DataContext = new OrderDetailViewModel(orderID);
        }
    }
}
