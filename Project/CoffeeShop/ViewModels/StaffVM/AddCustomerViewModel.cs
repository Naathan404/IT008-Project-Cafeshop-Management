using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

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
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                MessageBox.Show("Tên khách hàng không được để trống");
                return;
            }

            if (string.IsNullOrWhiteSpace(CustomerPhoneNumber))
            {
                MessageBox.Show("Số điện thoại không được để trống");
                return;
            }
            var newCustomer = _parentVM.AddCustomer(CustomerName, CustomerPhoneNumber, CustomerEmail);
            // Cập nhật SelectedCustomer tại StaffOrderViewModel
            _parentVM.SelectedCustomer = newCustomer;
            CloseWindowAction?.Invoke();
        }
    }

}
