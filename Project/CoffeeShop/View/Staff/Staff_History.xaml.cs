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
        CultureInfo viVn = new CultureInfo("vn-VN");

        public Staff_History()
        {
            InitializeComponent();
            LoadOrderHistory();
        }

        private void LoadOrderHistory()
        {
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
                        CustomerName = order.Customer != null ? order.Customer.CustomerName : "Khách vãng lai",
                        EmployeeName = order.Staff.StaffName,
                        OrderDate = order.OrderDate.ToString("HH:mm:ss"),
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
            public string OrderDate { get; set; }
            public string Total { get; set; } = null!;
            public string PaymentMethod { get; set; } = null!;
        }

        private void TextChangedEvt(object sender, TextChangedEventArgs e)
        {
            FilterData();
        }

        private void SelectedTimeChangedEvt(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            FilterData();
        }

        private void FilterData()
        {
            string keyword = txbCustomerName.Text.Trim();
            DateTime? start = timePickerStartTime.SelectedTime;
            DateTime? end = timePickerEndTime.SelectedTime;
            orderHistoryItems.Clear();


            using (var db = new CoffeeShopContext())
            {
                var query = db.Orders
                              .Include(o => o.Customer)
                              .Include(o => o.Staff)
                              .AsQueryable();

                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(o => o.Customer != null &&
                                             o.Customer.CustomerName.Contains(keyword));
                }

                if (start.HasValue)
                {
                    DateTime startDateTime = DateTime.Today.Add(start.Value.TimeOfDay);
                    query = query.Where(o => o.OrderDate >= startDateTime);
                }
                if (end.HasValue)
                {
                    DateTime endDateTime = DateTime.Today.Add(end.Value.TimeOfDay);
                    query = query.Where(o => o.OrderDate <= endDateTime);
                }

                var orders = query.ToList();

                foreach (var order in orders)
                {
                    orderHistoryItems.Add(new OrderHistory
                    {
                        OrderID = order.OrderId,
                        CustomerName = order.Customer != null ? order.Customer.CustomerName : "Khách vãng lai",
                        EmployeeName = order.Staff.StaffName,
                        OrderDate = order.OrderDate.ToString("HH:mm:ss"),
                        Total = order.TotalAmount.ToString("N0", viVn),
                        PaymentMethod = order.PaymentMethod
                    });
                }

                dgOrdersHistory.ItemsSource = orderHistoryItems;
                dgOrdersHistory.Items.Refresh();
            }
        }

        private void DetailClickEvt(object sender, RoutedEventArgs e)
        {
            OrderHistory? selectedItem =  dgOrdersHistory.SelectedItem as OrderHistory;
            if (selectedItem == null)
                return;
            OrderDetailWindow orderDetailWindow = new OrderDetailWindow(selectedItem.OrderID);
            orderDetailWindow.ShowDialog();
        }
    }
}
