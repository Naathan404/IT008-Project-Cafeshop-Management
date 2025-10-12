using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using CoffeeShop.Models;
using CoffeeShop.View;

namespace CoffeeShop
{
    /// <summary>
    /// Interaction logic for ForgotPasswordStep1.xaml
    /// </summary>
    public partial class ForgotPasswordStep1 : Page
    {
        private Frame parentFrame;
        private LoginWindow loginWindow;
        public ForgotPasswordStep1(Frame frame, LoginWindow loginWindow)
        {
            InitializeComponent();
            txbFloatingEmailBox.Focus();
            parentFrame = frame;

            // Tham chiếu đến Login Window ban đầu
            this.loginWindow = loginWindow;

            // Ẩn thông báo Email chưa được đăng ký
            txblNotify.Visibility = Visibility.Hidden;
        }

        private void txbFloatingEmailBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSendCode.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                txbFloatingEmailBox.Focus();
                e.Handled = true;
            }
        }

        private void btnSendCode_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new CoffeeShopContext())
            {
                String emailFromInput = txbFloatingEmailBox.Text.Trim();
                var email = db.Staff.FirstOrDefault(e => e.Email == emailFromInput.ToString());
                if(email != null)
                {
                    MoveToNextPage(emailFromInput);
                }
                else
                {
                    txblNotify.Visibility = Visibility.Visible;
                    txbFloatingEmailBox.Text = "";
                    txbFloatingEmailBox.Focus();
                }
            }
        }

        private void btnBackToLogin_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }

        private void MoveToNextPage(string emailToSend)
        {
            var nextPage = new ForgotPasswordStep2(parentFrame, loginWindow, emailToSend);

            // đặt ban đầu ngoài màn hình phải
            nextPage.Margin = new Thickness(parentFrame.ActualWidth, 0, -parentFrame.ActualWidth, 0);

            // gán vào Frame
            parentFrame.Content = nextPage;

            // animation slide-in
            var anim = new ThicknessAnimation
            {
                From = nextPage.Margin,
                To = new Thickness(0),
                Duration = TimeSpan.FromSeconds(0.7),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            nextPage.BeginAnimation(MarginProperty, anim);
        }

        private void txbFloatingEmailBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txbFloatingEmailBox.Text.Length == 1) 
                txblNotify.Visibility = Visibility.Hidden;   
        }
    }
}
