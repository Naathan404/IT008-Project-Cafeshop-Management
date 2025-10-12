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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CoffeeShop
{
    /// <summary>
    /// Interaction logic for ForgotPasswordStep1.xaml
    /// </summary>
    public partial class ForgotPasswordStep1 : Page
    {
        private Frame parentFrame;
        public ForgotPasswordStep1(Frame frame)
        {
            InitializeComponent();
            txbFloatingEmailBox.Focus();
            parentFrame = frame;
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
            var nextPage = new ForgotPasswordStep2(parentFrame);

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

        private void btnBackToLogin_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }
    }
}
