using CoffeeShop.View.Login;
using System.Globalization;
using System.Windows;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for AccountWindow.xaml
    /// </summary>
    public partial class AccountWindow : Window
    {
        private CoffeeShop.Models.Staff _staff;
        public AccountWindow(CoffeeShop.Models.Staff staff)
        {
            InitializeComponent();
            _staff = staff;

            // Load dữ liệu tài khoản lên UI
            LoadAccountData();
        }
        private void LoadAccountData()
        {
            txblName.Text = _staff.StaffName;
            txblPhonenumber.Text = _staff.Phonenumber;
            txblEmail.Text = _staff.Email;
            switch(_staff.StaffRole)
            {
                case "Admin":
                    txblRole.Text = "Quản lý";
                    break;
                default:
                    txblRole.Text = "Nhân viên";
                    break;
            }
            CultureInfo viVn = new CultureInfo("vn-VN");
            txblBaseSalary.Text = (_staff.BaseSalary)?.ToString("N0", viVn) + "đ/giờ";
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Owner.Close();
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}
