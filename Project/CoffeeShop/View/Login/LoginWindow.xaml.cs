using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CoffeeShop.Models;
using CoffeeShop.Helper;
using CoffeeShop.View.Staff;
using CoffeeShop.View.Admin;
using CoffeeShop.Service;

namespace CoffeeShop.View.Login
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        #region Random Fields
        // Danh sách câu nói hay về cà phê và hình nền banner
        private List<(string Quote, string Author)> _quotes = new List<(string, string)>
        {
            ("Thà uống cà phê tồi, còn hơn không có giọt nào.", "David Lynch"),
            ("Cà phê là liều thuốc chữa lành cho trái tim và tâm hồn.", "Cellini Caffe"),
            ("Chẳng có gì ngọt ngào hơn một tách cà phê đắng.", "Rian Aditia"),
            ("Cà phê là một thứ ma thuật có thể uống.", "C.M. Valente"),
            ("Cuộc đời quá ngắn để uống cà phê tồi.", "Nescafe Australia"),
            ("Cứ mỗi phút bạn giận dữ là bạn đánh mất sáu mươi giây hạnh phúc.", "R.W.Emerson"),
            ("Cà phê ngon phải đen như địa ngục, đắng như ác quỷ và ngọt ngào như tình yêu.", "C.M. de Talleyrand"),
            ("Cuộc đời cũng như một tách cà phê. Quan trọng không phải là ngon hay dở, mà là cách ta thưởng thức nó.", "Khuyết danh"),
            ("Cà phê là người bạn đồng hành tuyệt vời trong những khoảnh khắc suy tư.", "Khuyết danh"),
            ("Cà phê không chỉ là một thức uống, mà là một nghệ thuật sống.", "Khuyết danh")
        };
        private List<String> _imgBannerSources = new List<string>()
        { 
            "/Assets/Images/imgBanner_1.png",
            "/Assets/Images/imgBanner_2.png", 
            "/Assets/Images/imgBanner_3.png",
            "/Assets/Images/imgBanner_4.png",
            "/Assets/Images/imgBanner_5.jpg",
        };
        #endregion

        // Constructor
        public LoginWindow()
        {
            InitializeComponent();
            txbFloatingUsernameBox.Focus();
            GenerateRandomLoginUI();
        }

        // Tạo giao diện đăng nhập ngẫu nhiên
        private void GenerateRandomLoginUI()
        {
            var quote = _quotes[new Random().Next(0, _quotes.Count)];
            txblQuote.Text = "\"" + quote.Quote + "\"";
            txblAuthor.Text = "- " + quote.Author + " -";
            imgBanner.Source = new BitmapImage(new Uri(_imgBannerSources[new Random().Next(0, _imgBannerSources.Count)], UriKind.Relative));

            txblNotify.Visibility = Visibility.Hidden;
        }

        // Xử lý đăng nhập
        private void ProcessLogin()
        {
            string username = txbFloatingUsernameBox.Text.Trim();
            string passwd = txbFloatingPasswordBox.Password.Trim();
            string hashPasswd_base64 = HashHelper.Base64_Encode(passwd);
            string hashPasswd_sha256 = HashHelper.SHA256_Encode(hashPasswd_base64);

            using (var db = new CoffeeShopContext())
            {
                var account = db.Staff.FirstOrDefault(user => user.Username == username && user.PasswordHash == hashPasswd_sha256);
                if (account != null) // Nếu không tìm thấy kết quả thì account == null
                {
                    switch(account.StaffRole)
                    {
                        case "Employee":
                            LoginStaffWindow(account);
                            break;
                        case "Admin":
                            LoginAdminWindow(account);
                            break;
                        default:
                            MessageBox.Show("Unidentified user");
                            break;
                    }
                    UserSession.Instance.SetUser(account);
                }
                else
                {
                    txblNotify.Visibility = Visibility.Visible;
                    txbFloatingUsernameBox.Text = "";
                    txbFloatingPasswordBox.Password = "";
                    txbFloatingUsernameBox.Focus();
                }
            }
        }

        // Mở cửa sổ AdminWindow
        private void LoginAdminWindow(CoffeeShop.Models.Staff account)
        {
            AdminWindow adminWindow = new AdminWindow(account);
            adminWindow.Show();
            this.Close();
        }
        // Mở cửa sổ StaffWindow
        private void LoginStaffWindow(CoffeeShop.Models.Staff account)
        {  
            StaffWindow staffWindow = new StaffWindow(account);
            staffWindow.Show();
            this.Close();
        }

        // Xử lý sự kiện khi nhấn nút Đăng nhập
        private void btnEvt_LoginButtonClick(object sender, RoutedEventArgs e)
        {
            ProcessLogin();
        }

        // Xử lý sự kiện nhấn Enter trong TextBox để chuyển sang PasswordBox
        private void txbFloatingUsernameBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                txbFloatingPasswordBox.Focus();
                e.Handled = true;
            }
            else if(e.Key == Key.Down)
            {
                txbFloatingPasswordBox.Focus();
                e.Handled = true;
            }     
        }

        // Xử lý sự kiện nhấn Enter trong PasswordBox để đăng nhập
        private void txbFloatingPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnLogin.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                e.Handled = true;
            }
            else if(e.Key == Key.Up)
            {
                txbFloatingUsernameBox.Focus();
                e.Handled = true;
            }
        }

        // Mở liên kết trang web khi bấm vào Hyperlink "About us"
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        // Mở cửa sổ Quên mật khẩu
        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            ResetPasswordWindow forgotPwdWindow = new ResetPasswordWindow(this);
            forgotPwdWindow.ShowDialog();
        }

        // Ẩn thông báo Nhập sai mật khẩu
        public void HideWrongPasswordNotify()
        {
            txblNotify.Visibility = Visibility.Hidden;
        }

        private void txbFloatingUsernameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(txbFloatingUsernameBox.Text.Length == 1)
                txblNotify.Visibility = Visibility.Hidden;
        }
    }
}
