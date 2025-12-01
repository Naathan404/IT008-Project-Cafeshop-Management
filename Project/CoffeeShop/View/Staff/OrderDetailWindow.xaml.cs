using CoffeeShop.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for OrderDetails.xaml
    /// </summary>
    public partial class OrderDetailWindow : Window
    {
        private int _orderID;
        CultureInfo viVn = new CultureInfo("vn-VN");
        public OrderDetailWindow(int orderID)
        {
            InitializeComponent();
            _orderID = orderID;

            LoadOrderDetail();
        }

        private void LoadOrderDetail()
        {
            dgOrderDetail.Items.Clear();
            using (var db = new CoffeeShopContext())
            {
                var order = db.Orders
                    .Include(o => o.Table)
                    .Include(o => o.Customer)
                    .Include(o => o.Staff)
                    .FirstOrDefault(o => o.OrderId == _orderID);
                if (order == null)
                {
                    return;
                }

                // Hiển thị thông tin chung của order
                txblOrderID.Text = "Mã số hóa đơn: " + order.OrderId;
                txblTable.Text = order.Table == null ? "Mang đi" : order.Table.TableName;
                txblOrderDate.Text = "Thời gian: " + order.OrderDate;
                txblPaymentMethod.Text = "HTTT: " + order.PaymentMethod;
                txblCustomer.Text = "Khách hàng: " + (order.Customer == null ? "Khách vãng lai" : order.Customer.CustomerName);
                txblemployee.Text = "Nhân viên: " + order.Staff.StaffName;
                txblSubTotal.Text = order.SubTotal.ToString("N0", viVn);
                txblDiscountMoney.Text = order.DiscountMoney.ToString("N0", viVn);
                txblTotalAmount.Text = order.TotalAmount.ToString("N0", viVn);

                // Hiển thị chi tiết từng sản phẩm đã mua
                List<ItemDetail> displayItems = new List<ItemDetail>();
                var orderItems = db.OrderDetails.Where(o => o.OrderId == _orderID)
                    .Include(o => o.Price)
                    .Include(o => o.Price.Item)
                    .Include(o => o.Price.Size)
                    .ToList();
                foreach(var item in orderItems)
                {
                    displayItems.Add(new ItemDetail
                    {
                        oName = item.Price.Item.ItemName,
                        oSize = item.Price.Size == null ? "---" : item.Price.Size.SizeName,
                        oQuantity = item.Quantity,
                        oPrice = item.UnitPrice.ToString("N0", viVn),
                        oTotal = (item.Quantity * item.UnitPrice).ToString("N0", viVn),
                        oNote = item.Note == null ? "" : item.Note
                    });
                }    
                dgOrderDetail.ItemsSource = displayItems;
            }
        }

        public class ItemDetail
        {
            public string oName { get; set; }
            public string oSize { get; set; }
            public int oQuantity { get; set; }
            public string oPrice { get; set; }
            public string oTotal { get; set; }
            public string oNote { get; set; }
        }
    }
}
