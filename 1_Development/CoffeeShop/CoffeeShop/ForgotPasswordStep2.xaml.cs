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
    /// Interaction logic for ForgotPasswordStep2.xaml
    /// </summary>
    public partial class ForgotPasswordStep2 : Page
    {
        private Frame parentFrame;
        public ForgotPasswordStep2(Frame frame)
        {
            InitializeComponent();
            txbVeriCode.Focus();
            parentFrame = frame;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            var nextPage = new ForgotPasswordStep3(parentFrame);

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

        private void txbVeriCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnNext.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                txbVeriCode.Focus();
                e.Handled = true;
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // tạo Page1, margin chuẩn (0)
            var lastPage = new ForgotPasswordStep1(parentFrame);

            // gán Page1 vào Frame **ngay lập tức** để nó hiển thị dưới Page3
            parentFrame.Content = lastPage;

            // animate Page2 trượt ra ngoài sang phải
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
