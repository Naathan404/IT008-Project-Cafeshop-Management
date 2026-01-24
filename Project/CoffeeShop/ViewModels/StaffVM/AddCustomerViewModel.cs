using CoffeeShop.Service;
using CoffeeShop.View.Controls;
using CoffeeShop.ViewModels.AdminVM;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using static CoffeeShop.View.Controls.CustomMessageBox;

namespace CoffeeShop.ViewModels.StaffVM
{
    public class AddCustomerViewModel : INotifyPropertyChanged
    {
        #region Property
        public event PropertyChangedEventHandler? PropertyChanged;
        private StaffOrderViewModel _parentVM;

        private string? _customerName;
        public string? CustomerName
        {
            get => _customerName;
            set 
            {
                _customerName = value;
                OnPropertyChanged(nameof(CustomerName)); 
            }
        }

        private string? _customerPhoneNumber;
        public string? CustomerPhoneNumber
        {
            get => _customerPhoneNumber;
            set { _customerPhoneNumber = value; OnPropertyChanged(nameof(CustomerPhoneNumber)); }
        }

        private string? _customerEmail;
        public string? CustomerEmail
        {
            get => _customerEmail;
            set { _customerEmail = value; OnPropertyChanged(nameof(CustomerEmail)); }
        }
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        
        // action đóng window addcustomer
        public Action? CloseWindowAction { get; set; }
        #endregion

        #region Command
        // Command để bấm nút "Thêm"
        public ICommand ConfirmAddCustomerCommand { get; set; }
        public AddCustomerViewModel(StaffOrderViewModel parent)
        {
            _parentVM = parent;
            ConfirmAddCustomerCommand = new RelayCommand<object>(ExecuteAdd);
        }
        #endregion
        private void ExecuteAdd(object param)
        {
            // Kiểm tra dữ liệu nhập
            // Tên khách hàng và số điện thoại không được để trống
            if (string.IsNullOrWhiteSpace(CustomerName) || string.IsNullOrWhiteSpace(CustomerPhoneNumber))
            {
                CustomMessageBox.Show("Tên khách hàng và SĐT khách hàng không được để trống", "Lỗi", MessageButtons.OK, MessageType.Error);
                return;
            }

            if (!Regex.IsMatch(CustomerName, @"^[a-zA-ZÀ-ỹ\s]+$"))
            {
                CustomMessageBox.Show("Tên không được chứa số hay kí tự đặc biệt!",
                                      "Thông báo", MessageButtons.OK, MessageType.Warning);
                return;
            }

            // Số điện thoại chỉ được chứa chữ số
            if (!Regex.IsMatch(CustomerPhoneNumber, @"^0(3|5|7|8|9)[0-9]{8}$"))
            {
                CustomMessageBox.Show("Số điện thoại không hợp lệ! Vui lòng nhập đúng 10 số (đầu 03, 05, 07, 08, 09).",
                                      "Thông báo", MessageButtons.OK, MessageType.Warning);
                return;
            }

            // Email phải có '@' nếu được nhập
            if (!string.IsNullOrWhiteSpace(CustomerEmail))
            {
                string emailPattern = @"^[a-zA-Z0-9]+([\.\-][a-zA-Z0-9]+)*@[a-zA-Z0-9]+(\.[a-zA-Z0-9]+)+$";
                if (!Regex.IsMatch(CustomerEmail, emailPattern))
                {
                    CustomMessageBox.Show("Email không đúng định dạng (Vd: abc@gmail.com).",
                                          "Thông báo", MessageButtons.OK, MessageType.Warning);
                    return;
                }
            }

            CustomerName = CleanName(CustomerName);
            var newCustomer = _parentVM.AddCustomer(CustomerName, CustomerPhoneNumber, CustomerEmail);
            EventAggregator.Instance.Publish(new CustomerChangedMessage { CustomerId = newCustomer.CustomerId });
            // Cập nhật SelectedCustomer tại StaffOrderViewModel
            _parentVM.SelectedCustomer = newCustomer;
            CloseWindowAction?.Invoke();
        }

        // Chỉnh sửa lại tên cho đúng định dạng
        public string CleanName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "";

            name = name.Trim().ToLower();

            name = Regex.Replace(name, @"\s+", " ");

            System.Globalization.CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            System.Globalization.TextInfo textInfo = cultureInfo.TextInfo;

            return textInfo.ToTitleCase(name);
        }
    }
}
