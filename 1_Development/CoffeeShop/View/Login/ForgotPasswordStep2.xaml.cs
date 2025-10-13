using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using CoffeeShop.Models;
using CoffeeShop.View;
using CoffeeShop.Helper;
using System.Windows.Media;

namespace CoffeeShop.View.Login
{
    /// <summary>
    /// Interaction logic for ForgotPasswordStep2.xaml
    /// </summary>
    public partial class ForgotPasswordStep2 : Page
    {
        private Frame _parentFrame;
        private LoginWindow _loginWindow;
        private string _emailToSend;
        private string _otpCode;

        public ForgotPasswordStep2(Frame frame, LoginWindow loginWindow, string email = "")
        {
            InitializeComponent();

            // Thiết lập UI ban đầu
            txbVeriCode.Focus();
            txblNotify.Visibility = Visibility.Hidden;

            // Gán biến
            _parentFrame = frame;
            _loginWindow = loginWindow;
            _emailToSend = email;
            _otpCode = "";

            // Gửi mã OTP khi vừa hiển thị trang
            Task task = SendOTP();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            DateTime currentTime = DateTime.UtcNow;
            
            using (var db = new CoffeeShopContext())
            {
                string enteredCode = txbVeriCode.Text.Trim();
                var otpEntry = db.OTPRequests.FirstOrDefault(o => o.Email == _emailToSend && o.Code == enteredCode);
                if(otpEntry != null)    // Nếu mã xác hợp lệ
                {
                    // Kiểm tra mã còn hiệu lực hay không
                    if(currentTime > otpEntry.ExpireTime) // Nếu không còn hiệu lực
                    {
                        txblNotify.Text = "Mã hết hiệu lực. Vui lòng chọn gửi lại mã!";
                        txblNotify.Visibility = Visibility.Visible;
                        db.OTPRequests.Remove(otpEntry);
                        return;
                    }
                    // Nếu mã còn hiệu lực, xóa bộ dữ liệu trong csdl và thực hiện di chuyển đến bước 3
                    db.OTPRequests.Remove(otpEntry);
                    db.SaveChanges();
                    MoveToNextPage(_emailToSend);
                }
                else    // Nếu mã xác nhận không hợp lệ
                {
                    txblNotify.Text = "Mã xác nhận không hợp lệ. Vui lòng nhập lại!";
                    txblNotify.Visibility = Visibility.Visible;
                    txbVeriCode.Text = "";
                    txbVeriCode.Focus();
                }    
            }
        }

        // Xử lý sự kiện nhấn Enter trong TextBox mã xác nhận
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

        // Xử lý sự kiện nút Gửi lại mã
        private async void btnResend_Click(object sender, RoutedEventArgs  e)
        {
            txblNotify.Text = "Đã gửi lại mã xác nhận.";
            txblNotify.Visibility = Visibility.Visible;
            txblNotify.Foreground = Brushes.Green;
            txbVeriCode.Text = "";
            txbVeriCode.Focus();
            await SendOTP();
        }

        // Xử lý sự kiện nút Quay lại
        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // tạo Page1, margin chuẩn (0)
            var lastPage = new ForgotPasswordStep1(_parentFrame, _loginWindow);

            // gán Page1 vào Frame **ngay lập tức** để nó hiển thị dưới Page3
            _parentFrame.Content = lastPage;

            // animate Page2 trượt ra ngoài sang phải
            var animOut = new ThicknessAnimation
            {
                From = new Thickness(0), // Page3 đang ở giữa màn hình
                To = new Thickness(_parentFrame.ActualWidth, 0, -_parentFrame.ActualWidth, 0), // trượt ra ngoài phải
                Duration = TimeSpan.FromSeconds(0.4),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
            };

            // bắt đầu animation trên Page3
            this.BeginAnimation(MarginProperty, animOut);
        }

        //
        // Xử lý gửi mã OPT xác nhận qua email người dùng nhập
        //
        private async Task SendOTP()
        {
            _otpCode = CodeGeneratorHelper.GenerateOTPCode(5);

            OTPRequest request = new OTPRequest
            {
                Email = _emailToSend,
                Code = _otpCode,
                ExpireTime = DateTime.UtcNow.AddMinutes(0.5) // mã OTP hết hạn sau 3 phút
            };

            using (var db = new CoffeeShopContext())
            {
                // Xóa các mã OTP cũ chưa sử dụng của email này
                var existingOTPs = db.OTPRequests.Where(o => o.Email == _emailToSend);
                db.OTPRequests.RemoveRange(existingOTPs);
                // Thêm mã OTP mới vào cơ sở dữ liệu
                db.OTPRequests.Add(request);
                db.SaveChanges();
            }

            // Nội dung email
            string subject = "Mã xác nhận đặt lại mật khẩu - 2G1G";
            string body = $"Mã xác nhận của bạn là: {_otpCode}\n" +
                $"Mã có hiệu lực trong 3 phút.\nNếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.";

            // Gửi email
            await MailUtils.SendEmailAsync(_emailToSend, subject, body);
        }

        // Di chuyển đến trang tiếp theo
        private void MoveToNextPage(string emailToSend)
        {
            var nextPage = new ForgotPasswordStep3(_parentFrame, _loginWindow, emailToSend);

            // đặt ban đầu ngoài màn hình phải
            nextPage.Margin = new Thickness(_parentFrame.ActualWidth, 0, -_parentFrame.ActualWidth, 0);

            // gán vào Frame
            _parentFrame.Content = nextPage;

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

        private void txbVeriCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txbVeriCode.Text.Length == 1)
            {
                txblNotify.Foreground = Brushes.Red;
                txblNotify.Text = "Mã xác nhận không hợp lệ. Vui lòng nhập lại!";
                txblNotify.Visibility = Visibility.Hidden;
            }
        }
    }
}
