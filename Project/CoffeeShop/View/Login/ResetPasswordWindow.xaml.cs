using CoffeeShop.View;
using System;
using System.Collections.Generic;
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

namespace CoffeeShop.View.Login
{
    /// <summary>
    /// Interaction logic for ResetPasswordWindow.xaml
    /// </summary>
    public partial class ResetPasswordWindow : Window
    {
        private LoginWindow loginWindow;
        public ResetPasswordWindow(LoginWindow loginWindow)
        {
            InitializeComponent();
            ForgotPasswordFrame.Navigate(new ForgotPasswordStep1(ForgotPasswordFrame, loginWindow));
            this.loginWindow = loginWindow;
        }
    }
}
