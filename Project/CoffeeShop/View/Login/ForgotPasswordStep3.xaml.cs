using CoffeeShop.Models;
using CoffeeShop.View;
using CoffeeShop.Helper;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CoffeeShop.View.Login
{
    /// <summary>
    /// Interaction logic for ForgotPasswordStep3.xaml
    /// </summary>
    public partial class ForgotPasswordStep3 : Page
    {
        private Frame _parentFrame;
        private LoginWindow _loginWindow;
        private string _emailToSend;
        public ForgotPasswordStep3(Frame frame, LoginWindow loginWindow, string emailToSend)
        {
            InitializeComponent();
            _parentFrame = frame;
            _emailToSend = emailToSend;
            _loginWindow = loginWindow;

            pwdBox.Focus();
            txblNotify.Visibility = Visibility.Hidden;
        }

        // Xử lý sự kiện ấn phím Enter của textbox Nhập mật khẩu mới
        private void pwdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                pwdBox2.Focus();
            }
        }

        // Xử lý sự kiện ấn phím Enter của textbox Nhập lại mật khẩu
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
            ResetPassword();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            // tạo Page2, margin chuẩn (0)
            var lastPage = new ForgotPasswordStep2(_parentFrame, _loginWindow, _emailToSend);

            _parentFrame.Content = lastPage;

            // animate Page3 trượt ra ngoài sang phải
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

        // Xử lý việc xác nhận mật khẩu và đổi mật khẩu
        private void ResetPassword()
        {
            if(pwdBox.Password != pwdBox2.Password)
            {
                txblNotify.Visibility = Visibility.Visible;
                pwdBox.Password = pwdBox2.Password = "";
                pwdBox.Focus();
                return;
            }

            using (var db = new CoffeeShopContext())
            {
                // Tìm ra thông tin nhân viên có email đã đăng ký trùng với email đã được xác nhận
                var userToUpdatePasswd = db.Staff.FirstOrDefault(u => u.Email == _emailToSend);
                if (userToUpdatePasswd != null) // Nếu tìm được
                {
                    // Hash mật khẩu
                    string newPasswd = pwdBox2.Password;
                    string newPasswd_base64 = HashHelper.Base64_Encode(newPasswd);
                    string newPasswd_sha256 = HashHelper.SHA256_Encode(newPasswd_base64);

                    // Cập nhật lại mật khẩu mới sau khi đã hash
                    userToUpdatePasswd.PasswordHash = newPasswd_sha256;
                    db.SaveChanges();

                    // Đóng cưa sổ Reset password và quay về login window
                    Window parentWindow = Window.GetWindow(this);
                    if (parentWindow != null)
                    {
                        parentWindow.Close();
                    }
                    _loginWindow.HideWrongPasswordNotify();
                }
                else // Không tìm thấy được nhân viên nào thỏa điều kiện
                {
                    MessageBox.Show("Có lỗi xảy ra. Vui lòng thử lại!");
                    pwdBox.Password = "";
                    pwdBox2.Password = "";
                    pwdBox.Focus();
                }    
            }
        }

        // Ẩn thông báo mật khẩu nhập lại không trùng khớp khi nhập lại mật khẩu một lần nữa
        private void pwdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (pwdBox.Password.Length == 1)
                txblNotify.Visibility = Visibility.Hidden;
        }
    }
}
