using CoffeeShop.ViewModels.AdminVM;
using System.Windows.Controls;

namespace CoffeeShop.View.Admin
{
    /// <summary>
    /// Interaction logic for EmployeeManagementPage.xaml
    /// </summary>
    public partial class StaffManagementPage : Page
    {
        public StaffManagementPage()
        {
            InitializeComponent();
            this.DataContext = new AdminStaffManagementViewModel(this);
        }

        public string GetPasswordFromPasswordBox()
        {
            return pwbPassword.Password;
        }

        public void SetPasswordToPasswordBox(string psswd)
        {
            pwbPassword.Password = psswd;
        }
    }
}
