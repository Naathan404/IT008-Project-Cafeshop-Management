using CoffeeShop.Models;
using CoffeeShop.Service.DTOs;
using CoffeeShop.View.General;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.GeneralVM
{
    public class OrderDetailViewModel : BaseViewModel
    {
        private int _orderID;
        CultureInfo viVn = new CultureInfo("vn-VN");

        public OrderDetailViewModel(int orderID)
        {
            _orderID = orderID;

            LoadOrderDetail();
        }

        private ObservableCollection<OrderDetailDTO> _orderDetails = new ObservableCollection<OrderDetailDTO>();
        public ObservableCollection<OrderDetailDTO> OrderDetails
        {
            get { return _orderDetails; }
            set
            {
                _orderDetails = value;
                OnPropertyChanged();
            }
        }

        private string? _displayID;
        public string DisplayID
        {
            get { return _displayID!; }
            set
            {
                _displayID = value;
                OnPropertyChanged();
            }
        }

        private string? _tableName;
        public string TableName
        {
            get { return _tableName!; }
            set
            {
                _tableName = value;
                OnPropertyChanged();
            }
        }

        private string? _orderDate;
        public string OrderDate
        {
            get { return _orderDate!; }
            set
            {
                _orderDate = value;
                OnPropertyChanged();
            }
        }

        private string? _customerName;
        public string CustomerName
        {
            get { return _customerName!; }
            set
            {
                _customerName = value;
                OnPropertyChanged();
            }
        }

        private string? _staffName;
        public string StaffName
        {
            get { return _staffName!; }
            set
            {
                _staffName = value;
                OnPropertyChanged();
            }
        }

        private string? _paymentMethod;
        public string PaymentMethod
        {
            get { return _paymentMethod!; }
            set
            {
                _paymentMethod = value;
                OnPropertyChanged();
            }
        }

        private string? _subTotal;
        public string SubTotal
        {
            get { return _subTotal!; }
            set
            {
                _subTotal = value;
                OnPropertyChanged();
            }
        }

        private string? _discountMoney;
        public string DiscountMoney
        {
            get { return _discountMoney!; }
            set
            {
                _discountMoney = value;
                OnPropertyChanged();
            }
        }

        private string? _totalAmount;
        public string TotalAmount
        {
            get { return _totalAmount!; }
            set
            {
                _totalAmount = value;
                OnPropertyChanged();
            }
        }

        private void LoadOrderDetail()
        {
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
                DisplayID = order.DisplayID;
                TableName = order.Table == null ? "Mang đi" : order.Table.TableName;
                OrderDate = order.OrderDate.ToString("ddMMyy hh:mm:ss");
                PaymentMethod = "HTTT: " + order.PaymentMethod;
                CustomerName = (order.Customer == null ? "Khách vãng lai" : order.Customer.CustomerName);
                StaffName = order.Staff.StaffName;
                SubTotal = order.SubTotal.ToString("N0", viVn);
                DiscountMoney = order.DiscountMoney?.ToString("N0", viVn) ?? "0 đ";
                TotalAmount = order.TotalAmount.ToString("N0", viVn);

                // Hiển thị chi tiết từng sản phẩm đã mua
                var orderItems = db.OrderDetails.Where(o => o.OrderId == _orderID)
                    .Include(o => o.Price)
                    .Include(o => o.Price.Item)
                    .Include(o => o.Price.Size)
                    .ToList();

                var displayItems = new List<OrderDetailDTO>();
                foreach (var item in orderItems)
                {
                    displayItems.Add(new OrderDetailDTO
                    {
                        ItemName = item.Price.Item.ItemName,
                        SizeName = item.Price.Size == null ? "---" : item.Price.Size.SizeName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice.ToString("N0", viVn),
                        TotalPrice = (item.Quantity * item.UnitPrice).ToString("N0", viVn),
                        Note = item.Note == null ? "" : item.Note
                    });
                }
                
                OrderDetails = new ObservableCollection<OrderDetailDTO>(displayItems);
            }
        }
    }
}