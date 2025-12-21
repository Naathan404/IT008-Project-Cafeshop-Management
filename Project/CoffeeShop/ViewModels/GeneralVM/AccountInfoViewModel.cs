using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.View.Login;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace CoffeeShop.ViewModels.GeneralVM
{
    public class AccountInfoViewModel : BaseViewModel
    {
        private CoffeeShop.Models.Staff _staff;
        private Window _parentWindow;

        private string _name = null!;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private string _id = null!;
        public string ID
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        private string _role = null!;
        public string Role
        {
            get => _role;
            set
            {
                _role = value;
                OnPropertyChanged();
            }
        }

        private string _username = null!;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        private string _email = null!;
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        private string _shift = null!;
        public string Shift
        {
            get => _shift;
            set
            {
                _shift = value;
                OnPropertyChanged();
            }
        }

        private string _phonenumber = null!;
        public string Phonenumber
        {
            get => _phonenumber;
            set
            {
                _phonenumber = value;
                OnPropertyChanged();
            }
        }   

        private string _baseSalary = null!;
        public string BaseSalary
        {
            get => _baseSalary;
            set
            {
                _baseSalary = value;
                OnPropertyChanged();
            }
        }

        public ICommand QuitCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

        public AccountInfoViewModel(Staff staff, Window parentWindow)
        {
            _staff = staff;
            _parentWindow = parentWindow;

            QuitCommand = new RelayCommand<object>((p) => {
                _parentWindow.Close();
            });

            LogoutCommand = new RelayCommand<object>((p) => {
                UserSession.Instance.ClearSession();
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                _parentWindow.Close();
            });

            Name = _staff.StaffName;
            ID = "Mã nhân viên: NV" + _staff.Phonenumber.Substring(_staff.Phonenumber.Length - 3) + _staff.StaffId;
            Phonenumber = _staff.Phonenumber;
            Username = _staff.Username;
            Email = _staff.Email;
            using (var db = new CoffeeShopContext())
            {
                var shift = db.Shifts.Find(_staff.ShiftId);
                if (shift != null)
                {
                    Shift = shift.StartTime.ToString(@"hh\:mm") + " - " + shift.EndTime.ToString(@"hh\:mm");
                }
                else
                {
                    Shift = "Trống";
                }
            }
            switch (_staff.StaffRole)
            {
                case "Admin":
                    Role = "QUẢN LÝ";
                    break;
                default:
                    Role = "NHÂN VIÊN";
                    break;
            }
            CultureInfo viVn = new CultureInfo("vn-VN");
            BaseSalary = (_staff.BaseSalary)?.ToString("N0", viVn) + "đ/giờ";
        }


        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            UserSession.Instance.ClearSession();
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            _parentWindow.Close();
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow.Close();
        }
    }
}
