using CoffeeShop.View.Controls;
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
            set { _customerName = value; OnPropertyChanged(nameof(CustomerName)); }
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
                CustomMessageBox.Show("Tên khách hàng và SĐT khách hàng không được để trống", "Lỗi", MessageType.Error , MessageButtons.OK);
                return;
            }

            string namePattern = @"^[\p{L} ]+$";
            // Tên KH chỉ được chứa chữ cái
            if (!Regex.IsMatch(CustomerName, namePattern))
            {
                CustomMessageBox.Show("Tên khách hàng không được chứa chữ số và ký tự đặc biệt!", "Lỗi định dạng", MessageType.Error, MessageButtons.OK);
                return;
            }

            // Số điện thoại chỉ được chứa chữ số
            if (!Regex.IsMatch(CustomerPhoneNumber, @"^[0-9]+$"))
            {
                CustomMessageBox.Show("Số điện thoại chỉ được chứa các chữ số!", "Lỗi định dạng", MessageType.Error, MessageButtons.OK);
                return;
            }

            // Email phải có '@' nếu được nhập
            if (!string.IsNullOrWhiteSpace(CustomerEmail))
            {
                if (!CustomerEmail.Contains("@"))
                {
                    CustomMessageBox.Show("Email không hợp lệ (thiếu ký tự @)!", "Lỗi định dạng", MessageType.Error, MessageButtons.OK);
                    return;
                }
            }
            var newCustomer = _parentVM.AddCustomer(CustomerName, CustomerPhoneNumber, CustomerEmail);
            // Cập nhật SelectedCustomer tại StaffOrderViewModel
            _parentVM.SelectedCustomer = newCustomer;
            CloseWindowAction?.Invoke();
        }
    }

}
