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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoffeeShop
{
    /// <summary>
    /// Interaction logic for ForgotPasswordStep3.xaml
    /// </summary>
    public partial class ForgotPasswordStep3 : Page
    {
        private Frame parentFrame;
        public ForgotPasswordStep3(Frame frame)
        {
            InitializeComponent();
            parentFrame = frame;
            pwdBox.Focus();
        }

        private void pwdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                pwdBox2.Focus();
            }
        }

        private void pwdBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnResetPassword.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                btnResetPassword.Focus();
                e.Handled = true;
            }
        }

        private void btnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            // Đóng cửa sổ ResetPasswordWindow và quay lại LoginWindow
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // tạo Page2, margin chuẩn (0)
            var lastPage = new ForgotPasswordStep2(parentFrame);

            parentFrame.Content = lastPage;

            // animate Page3 trượt ra ngoài sang phải
            var animOut = new ThicknessAnimation
            {
                From = new Thickness(0), // Page3 đang ở giữa màn hình
                To = new Thickness(parentFrame.ActualWidth, 0, -parentFrame.ActualWidth, 0), // trượt ra ngoài phải
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            // bắt đầu animation trên Page3
            this.BeginAnimation(MarginProperty, animOut);
        }
    }
}
