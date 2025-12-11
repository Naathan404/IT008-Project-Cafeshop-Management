using CoffeeShop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for Staff_History.xaml
    /// </summary>
    public partial class Staff_History : Page
    {
        private List<OrderHistory> orderHistoryItems = new List<OrderHistory>();
        private ICollectionView orderView;


        public Staff_History()
        {
            InitializeComponent();
            orderView = CollectionViewSource.GetDefaultView(orderHistoryItems);
            LoadOrderHistory();
        }

        private void LoadOrderHistory()
        {
            CultureInfo viVn = new CultureInfo("vn-VN");
            using (var db = new CoffeeShopContext())
            {
                var orders = db.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.Staff)
                    .ToList();
                foreach (var order in orders)
                {
                    orderHistoryItems.Add(new OrderHistory
                    {
                        OrderID = order.OrderId,
                        CustomerName = order.CustomerId == null ? "Khách vãng lai" : order.Customer.CustomerName,
                        EmployeeName = order.Staff.StaffName,
                        OrderDate = order.OrderDate,
                        Total = order.TotalAmount.ToString("N0", viVn),
                        PaymentMethod = order.PaymentMethod
                    });
                }
                dgOrdersHistory.ItemsSource = orderHistoryItems;
            }    
        }

        public class OrderHistory
        {
            public int OrderID { get; set; }
            public string CustomerName { get; set; } = null!;
            public string EmployeeName { get; set; } = null!;
            public DateTime OrderDate { get; set; }
            public string Total { get; set; } = null!;
            public string PaymentMethod { get; set; } = null!;
        }
    }
}
