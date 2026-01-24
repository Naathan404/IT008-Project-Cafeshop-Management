using CoffeeShop.Models;
using CoffeeShop.Service.DTOs;
using CoffeeShop.View.Controls;
using CoffeeShop.View.General;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32; // Cần cái này cho SaveFileDialog
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static CoffeeShop.View.Controls.CustomMessageBox;

namespace CoffeeShop.ViewModels.AdminVM
{
    public class AdminCustomerViewModel : BaseViewModel
    {
        CultureInfo viVn = new CultureInfo("vi-VN");

        // Properties
        private ObservableCollection<CustomerDTO> _customerList;
        public ObservableCollection<CustomerDTO> CustomerList
        {
            get => _customerList;
            set { _customerList = value; OnPropertyChanged(); }
        }

        private CustomerDTO _selectedCustomer = null!;
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
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

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
        public ICommand DeleteCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand ShowOrderDetailCommand { get; set; }

        // [ADMIN ONLY] Command xuất Excel
        public ICommand ExportExcelCommand { get; set; }

        public AdminCustomerViewModel()
        {
            CustomerList = new ObservableCollection<CustomerDTO>();
            SelectedCustomer = new CustomerDTO();

            RefreshCommand = new RelayCommand<object>(p => Refresh());
            AddCommand = new RelayCommand<object>(p => PrepareAdd());
            SaveCommand = new RelayCommand<object>(async p => await SaveCustomer());
            DeleteCommand = new RelayCommand<object>(async p => await DeleteCustomer());
            ShowOrderDetailCommand = new RelayCommand<object>((p) =>
            {
                ShowOrderDetail();
            }, (p) =>
            {
                return SelectedOrder != null;
            });

            ExportExcelCommand = new RelayCommand<object>(p => ExportToExcel());

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
                        Tier = c.Tier ?? "MEMBER", // Mặc định nếu null
                        JoinDate = c.JoinDate
                    }).ToList();

                    CustomerList = new ObservableCollection<CustomerDTO>(dtoList);
                }
            }
            catch (Exception ex) { CustomMessageBox.Show("Lỗi tải dữ liệu: " + ex.Message, "Lỗi", MessageButtons.OK, MessageType.Error); }
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

/*                    var displayList = orders.Select(o => new
                    {
                        DisplayID = $"HD{o.OrderID:D4}",
                        Date = o.OrderDate,
                        Total = o.TotalAmount.ToString("N0", viVn),
                        Discount = string.IsNullOrEmpty(o.DiscountCode) ? "-" : o.DiscountCode
                    }).ToList();*/

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
                Tier = "MEMBER",
                Point = 0,
                CustomerName = "",
                PhoneNumber = "",
                Email = ""
            };
            
            TransactionHistory = new ObservableCollection<Order>();
        }

        private async Task SaveCustomer()
        {
            if (SelectedCustomer == null) return;

            // Kiểm thử dữ liệu
            if (string.IsNullOrEmpty(SelectedCustomer.CustomerName) || string.IsNullOrEmpty(SelectedCustomer.PhoneNumber))
            {
                CustomMessageBox.Show("Vui lòng nhập Tên và Số điện thoại!", "Thông báo", MessageButtons.OK, MessageType.Warning); return;
            }

            if (!Regex.IsMatch(SelectedCustomer.CustomerName, @"^[a-zA-ZÀ-ỹ\s]+$"))
            {
                CustomMessageBox.Show("Tên không được chứa số hay kí tự đặc biệt!",
                                      "Thông báo", MessageButtons.OK, MessageType.Warning);
                return;
            }

            if (!Regex.IsMatch(SelectedCustomer.PhoneNumber, @"^0(3|5|7|8|9)[0-9]{8}$"))
            {
                CustomMessageBox.Show("Số điện thoại không hợp lệ! Vui lòng nhập đúng 10 số (đầu 03, 05, 07, 08, 09).",
                                      "Thông báo", MessageButtons.OK, MessageType.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(SelectedCustomer.Email))
            {
                string emailPattern = @"^[a-zA-Z0-9]+([\.\-][a-zA-Z0-9]+)*@[a-zA-Z0-9]+(\.[a-zA-Z0-9]+)+$";
                if (!Regex.IsMatch(SelectedCustomer.Email, emailPattern))
                {
                    CustomMessageBox.Show("Email không đúng định dạng(Vd: abc@gmail.com).",
                                          "Thông báo", MessageButtons.OK, MessageType.Warning);
                    return;
                }
            }

            // Lưu vào DB
            using (var db = new CoffeeShopContext())
            {
                string newTier = CalculateTier(SelectedCustomer.Point);

                if (SelectedCustomer.CustomerID == 0) // Thêm mới
                {
                    if (await db.Customers.AnyAsync(c => c.PhoneNumber == SelectedCustomer.PhoneNumber && !c.IsDeleted))
                    {
                        CustomMessageBox.Show("Số điện thoại này đã tồn tại!", "Thông báo", MessageButtons.OK, MessageType.Warning); return;
                    }

                    var newCus = new Customer
                    {
                        CustomerName = SelectedCustomer.CustomerName,
                        PhoneNumber = SelectedCustomer.PhoneNumber,
                        Email = SelectedCustomer.Email,
                        Point = SelectedCustomer.Point, 
                        Tier = newTier,                
                        JoinDate = DateTime.Now,
                        IsDeleted = false
                    };
                    db.Customers.Add(newCus);
                    await db.SaveChangesAsync();
                    CustomMessageBox.Show("Thêm khách hàng thành công!", "Thành công", MessageButtons.OK, MessageType.Success);
                }
                else        // Cập nhật 
                {
                    var cus = await db.Customers.FindAsync(SelectedCustomer.CustomerID);
                    if (cus != null)
                    {
                        cus.CustomerName = SelectedCustomer.CustomerName;
                        cus.PhoneNumber = SelectedCustomer.PhoneNumber;
                        cus.Email = SelectedCustomer.Email;
                        cus.Point = SelectedCustomer.Point;
                        cus.Tier = newTier;

                        await db.SaveChangesAsync();
                        CustomMessageBox.Show("Cập nhật thông tin thành công!", "Thành công", MessageButtons.OK, MessageType.Success);
                    }
                }
            }
            Refresh();
        }

        private async Task DeleteCustomer()
        {
            if (SelectedCustomer == null || SelectedCustomer.CustomerID == 0) return;

            if (CustomMessageBox.Show($"Bạn chắc chắn muốn xóa khách hàng '{SelectedCustomer.CustomerName}'?\nHành động này không thể hoàn tác.",
                "Cảnh báo", MessageButtons.YesNo, MessageType.Warning) == CustomMessageBox.MessageBoxResult.Yes)
            {
                using (var db = new CoffeeShopContext())
                {
                    var cus = await db.Customers.FindAsync(SelectedCustomer.CustomerID);
                    if (cus != null) 
                    { 
                        cus.IsDeleted = true; await db.SaveChangesAsync(); 
                    }
                }
                Refresh();
            }
        }

        private void ShowOrderDetail()
        {
            if (SelectedOrder == null) return;
            int orderId = SelectedOrder.OrderId;

            OrderDetailWindow orderDetailWindow = new OrderDetailWindow(orderId);
            orderDetailWindow.ShowDialog();
        }

        private void Refresh()
        {
            SearchKeyword = "";
            PrepareAdd();
            _ = LoadCustomers();
        }


        // tính hạng thành viên
        private string CalculateTier(int? point)
        {
            if (point == null) return "MEMBER";
            if (point >= 3000) return "VIP100";
            if (point >= 1500) return "VIP10";    
            if (point >= 500) return "VIP1";   
            return "MEMBER";
        }

        // xuất Excel
        private void ExportToExcel()
        {
            try
            {
                if (CustomerList == null || CustomerList.Count == 0)
                {
                    CustomMessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageButtons.OK, MessageType.Warning);
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel CSV (*.csv)|*.csv",
                    FileName = $"DanhSachKhachHang_{DateTime.Now:ddMMyyyy_HHmm}.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    StringBuilder csvContent = new StringBuilder();

                    // Header
                    csvContent.AppendLine("Mã KH,Họ Tên,Số Điện Thoại,Email,Điểm Tích Lũy,Hạng Thành Viên,Ngày Tham Gia");

                    // Rows
                    foreach (var item in CustomerList)
                    {
                        string line = $"{item.CustomerID}," +
                                      $"\"{item.CustomerName}\"," + 
                                      $"'{item.PhoneNumber}," +      
                                      $"{item.Email}," +
                                      $"{item.Point}," +
                                      $"{item.Tier}," +
                                      $"{item.JoinDate:dd/MM/yyyy}";
                        csvContent.AppendLine(line);
                    }

                    File.WriteAllText(saveFileDialog.FileName, csvContent.ToString(), Encoding.UTF8);

                    CustomMessageBox.Show("Xuất file thành công!", "Thành công", MessageButtons.OK, MessageType.Success);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show("Có lỗi khi xuất file: " + ex.Message, "Lỗi", MessageButtons.OK, MessageType.Error);
            }
        }
    }
}