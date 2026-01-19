using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.Service.DTOs;
using CoffeeShop.View.Controls;
using CoffeeShop.View.General;
using CoffeeShop.ViewModels.AdminVM;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Tls;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Input;
using static CoffeeShop.View.Controls.CustomMessageBox;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class CustomerViewModel : BaseViewModel
    {
        CultureInfo viVn = new CultureInfo("vi-VN");

        // Properties
        private ObservableCollection<CustomerDTO> _customerList;
        public ObservableCollection<CustomerDTO> CustomerList
        {
            get => _customerList;
            set { _customerList = value; OnPropertyChanged(); }
        }

        private CustomerDTO _selectedCustomer;
        public CustomerDTO SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged();
                if (_selectedCustomer != null && _selectedCustomer.CustomerID > 0)
                {
                    _ = LoadTransactionHistory(_selectedCustomer.CustomerID);
                }
                else
                {
                    TransactionHistory = new ObservableCollection<Order>();
                }
            }
        }

        private Order _selectedOrder;
        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
                // Cập nhật trạng thái nút bấm ngay khi chọn
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Danh sách lịch sử giao dịch
        private ObservableCollection<Order> _transactionHistory;
        public ObservableCollection<Order> TransactionHistory
        {
            get => _transactionHistory;
            set { _transactionHistory = value; OnPropertyChanged(); }
        }

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { _searchKeyword = value; OnPropertyChanged(); _ = LoadCustomers(); }
        }

        // Commands
        public ICommand RefreshCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        //public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand ShowOrderDetailCommand {  get; set; }

        public CustomerViewModel()
        {
            CustomerList = new ObservableCollection<CustomerDTO>();
            SelectedCustomer = new CustomerDTO(); // Tránh null reference

            RefreshCommand = new RelayCommand<object>(p => Refresh());
            AddCommand = new RelayCommand<object>(p => PrepareAdd());
            SaveCommand = new RelayCommand<object>(async p => await SaveCustomer());

            EventAggregator.Instance.Subscribe<OrderCompletedMessage>(async (msg) =>
            {
                await LoadCustomers();
            });

            ShowOrderDetailCommand = new RelayCommand<object>((p) =>
            {
                ShowOrderDetail();
            }, (p) =>
            {
                return SelectedOrder != null;
            });

            //DeleteCommand = new RelayCommand<object>(async p => await DeleteCustomer());

            _ = LoadCustomers();
        }

        private async Task LoadCustomers()
        {
            string keyword = SearchKeyword.ToLower().Trim();
            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var query = db.Customers.AsNoTracking().Where(c => !c.IsDeleted).AsQueryable();

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        query = query.Where(c => c.CustomerName.ToLower().Contains(keyword) ||
                                                 c.PhoneNumber!.Contains(keyword));
                    }

                    var data = await query.OrderByDescending(c => c.Point).ToListAsync();

                    var dtoList = data.Select(c => new CustomerDTO
                    {
                        CustomerID = c.CustomerId,
                        CustomerName = c.CustomerName,
                        PhoneNumber = c.PhoneNumber ?? "---",
                        Email = c.Email ?? "",
                        Point = c.Point,
                        Tier = c.Tier ?? "VIP1",
                        JoinDate = c.JoinDate
                    }).ToList();

                    CustomerList = new ObservableCollection<CustomerDTO>(dtoList);
                }
            }
            catch (Exception ex) { CustomMessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private async Task LoadTransactionHistory(int customerId)
        {
            try
            {
                using (var db = new CoffeeShopContext())
                {
                    var orders = await db.Orders
                        .AsNoTracking()
                        .Where(o => o.CustomerId == customerId)
                        .OrderByDescending(o => o.OrderDate)
                        //.Select(o => new
                        //{
                        //    OrderID = o.OrderId,
                        //    OrderDate = o.OrderDate,
                        //    TotalAmount = o.TotalAmount,
                        //    DiscountCode = o.Discount != null ? o.Discount.DiscountCode : ""
                        //})
                        .Take(50)
                        .ToListAsync();

                    //var displayList = orders.Select(o => new
                    //{
                    //    DisplayID = $"HD{o.OrderID:D4}",
                    //    Date = o.OrderDate,
                    //    Total = o.TotalAmount.ToString("N0", viVn),
                    //    Discount = string.IsNullOrEmpty(o.DiscountCode) ? "-" : o.DiscountCode
                    //}).ToList();

                    TransactionHistory = new ObservableCollection<Order>(orders);
                }
            }
            catch { }
        }

        private void PrepareAdd()
        {
            SelectedCustomer = null!;
            SelectedCustomer = new CustomerDTO
            {
                CustomerID = 0,
                JoinDate = DateTime.Now,
                Tier = "VIP1",
                Point = 0,
                CustomerName = "",
                PhoneNumber = ""
            };
        }

        private async Task SaveCustomer()
        {
            if (SelectedCustomer == null) return;

            // Kiểm thử
            if (string.IsNullOrEmpty(SelectedCustomer.CustomerName) || string.IsNullOrEmpty(SelectedCustomer.PhoneNumber))
            {
                CustomMessageBox.Show("Vui lòng nhập Tên và Số điện thoại!", "Thông báo", MessageButtons.OK, MessageType.Warning); 
                return;
            }
            
            if (!Regex.IsMatch(SelectedCustomer.CustomerName, @"^[a-zA-ZÀ-ỹ\s]+$"))
            {
                CustomMessageBox.Show("Tên khách hàng chỉ được chứa chữ cái và khoảng trắng!", "Thông báo", MessageButtons.OK, MessageType.Warning);
                return;
            }

            if (!Regex.IsMatch(SelectedCustomer.PhoneNumber, @"^[0-9]{10,11}$"))
            {
                CustomMessageBox.Show("Số điện thoại không hợp lệ (chỉ chứa số, từ 10-11 chữ số)!", "Thông báo", MessageButtons.OK, MessageType.Warning);
                return;
            }
            if (!string.IsNullOrEmpty(SelectedCustomer.Email))
            {
                if (!SelectedCustomer.Email.Contains("@") || !Regex.IsMatch(SelectedCustomer.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    CustomMessageBox.Show("Email không đúng định dạng!", "Thông báo", MessageButtons.OK, MessageType.Warning);
                    return;
                }
            }

            using (var db = new CoffeeShopContext())
            {
                if (SelectedCustomer.CustomerID == 0) // Thêm mới
                {
                    if (await db.Customers.AnyAsync(c => c.PhoneNumber == SelectedCustomer.PhoneNumber && !c.IsDeleted))
                    {
                        CustomMessageBox.Show("Số điện thoại này đã tồn tại!", "Lỗi", MessageButtons.OK, MessageType.Error); return;
                    }

                    var newCus = new Customer
                    {
                        CustomerName = SelectedCustomer.CustomerName,
                        PhoneNumber = SelectedCustomer.PhoneNumber,
                        Email = SelectedCustomer.Email,
                        Point = 0,
                        Tier = "VIP1",
                        JoinDate = DateTime.Now,
                        IsDeleted = false
                    };
                    db.Customers.Add(newCus);
                    await db.SaveChangesAsync();
                    CustomMessageBox.Show("Thêm khách hàng thành công!", "Thành công", MessageButtons.OK, MessageType.Success);
                }
                else // Cập nhật
                {
                    var cus = await db.Customers.FindAsync(SelectedCustomer.CustomerID);
                    if (cus != null)
                    {
                        cus.CustomerName = SelectedCustomer.CustomerName;
                        cus.PhoneNumber = SelectedCustomer.PhoneNumber;
                        cus.Email = SelectedCustomer.Email;
                        await db.SaveChangesAsync();
                        CustomMessageBox.Show("Cập nhật thành công!", "Thành công", MessageButtons.OK, MessageType.Success);
                    }
                }
            }
            Refresh();
        }

        private void ShowOrderDetail()
        {
            if (SelectedOrder == null) return;

            int orderId = SelectedOrder.OrderId;

            OrderDetailWindow orderDetailWindow = new OrderDetailWindow(orderId);
            orderDetailWindow.ShowDialog();
        }

        //private async Task DeleteCustomer()
        //{
        //    if (SelectedCustomer == null || SelectedCustomer.CustomerID == 0) return;
        //    if (MessageBox.Show("Bạn có chắc muốn xóa khách hàng này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
        //    {
        //        using (var db = new CoffeeShopContext())
        //        {
        //            var cus = await db.Customers.FindAsync(SelectedCustomer.CustomerID);
        //            if (cus != null) { cus.IsDeleted = true; await db.SaveChangesAsync(); }
        //        }
        //        Refresh();
        //    }
        //}

        private void Refresh()
        {
            SearchKeyword = "";
            PrepareAdd();
            _ = LoadCustomers();
        }
    }
}