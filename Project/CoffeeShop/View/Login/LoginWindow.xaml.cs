using CoffeeShop.Helper;
using CoffeeShop.Models;
using CoffeeShop.Service;
using CoffeeShop.View.Admin;
using CoffeeShop.View.Staff;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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

        private DispatcherTimer _imageTimer;

        // Constructor
        public LoginWindow()
        {
            InitializeComponent();
            txbFloatingUsernameBox.Focus();
            GenerateRandomLoginUI();

            StartImageRotationTimer();
        }

        private void StartImageRotationTimer()
        {
            _imageTimer = new DispatcherTimer();
            _imageTimer.Interval = TimeSpan.FromSeconds(5); // Chạy mỗi 5 giây
            _imageTimer.Tick += ImageTimer_Tick;
            _imageTimer.Start();
        }

        private void ImageTimer_Tick(object sender, EventArgs e)
        {
            ChangeImageWithAnimation();
        }

        private void ChangeImageWithAnimation()
        {
            // setup Scale và Translate
            TransformGroup group = new TransformGroup();
            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            TranslateTransform trans = new TranslateTransform(0, 0);
            group.Children.Add(scale);
            group.Children.Add(trans);

            imgBanner.RenderTransformOrigin = new Point(0.5, 0.5);
            imgBanner.RenderTransform = group;

            // hiệu ứng blurrrrrrrrrrrrrrrr
            System.Windows.Media.Effects.BlurEffect blur = new System.Windows.Media.Effects.BlurEffect();
            blur.Radius = 0;
            imgBanner.Effect = blur;

            // thời gian hoạt ảnh
            TimeSpan duration = TimeSpan.FromSeconds(0.25);

            /// Anim ảnh cũ biến mất
            // anim trượt lên
            DoubleAnimation moveUpOut = new DoubleAnimation(0, -50, duration);
            // anim blur đi
            DoubleAnimation blurOut = new DoubleAnimation(0, 20, duration);
            // anim mờ đi
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, duration);
            // anim thu nhỏ lại
            DoubleAnimation shrinkOut = new DoubleAnimation(1.0, 0.85, duration);

            // Khi ảnh cũ biến mất xong
            fadeOut.Completed += (s, e) =>
            {
                // --- GIAI ĐOẠN 2: ĐỔI DATA ---
                string newSource = _imgBannerSources[new Random().Next(0, _imgBannerSources.Count)];
                imgBanner.Source = new BitmapImage(new Uri(newSource, UriKind.Relative));

                // Tạm đặt vị trí ảnh mới bên dưới
                trans.Y = 50;
                /// Anim ảnh mới xuất hiện
                // anim trượt lên
                DoubleAnimation moveUpIn = new DoubleAnimation(50, 0, duration);
                moveUpIn.EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut };
                // anim blur vào
                DoubleAnimation blurIn = new DoubleAnimation(20, 0, duration);
                // anim hết mờ dần
                DoubleAnimation fadeIn = new DoubleAnimation(0, 1, duration);
                // anim phóng to ra
                DoubleAnimation shrinkIn = new DoubleAnimation(1.1, 1.0, duration);
                shrinkIn.EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut };

                // Kích hoạt Animation vào
                trans.BeginAnimation(TranslateTransform.YProperty, moveUpIn);
                imgBanner.Effect.BeginAnimation(System.Windows.Media.Effects.BlurEffect.RadiusProperty, blurIn);
                imgBanner.BeginAnimation(Image.OpacityProperty, fadeIn);
                scale.BeginAnimation(ScaleTransform.ScaleXProperty, shrinkIn);
                scale.BeginAnimation(ScaleTransform.ScaleYProperty, shrinkIn);
            };

            // Kích hoạt Animation ra
            trans.BeginAnimation(TranslateTransform.YProperty, moveUpOut);
            imgBanner.Effect.BeginAnimation(System.Windows.Media.Effects.BlurEffect.RadiusProperty, blurOut);
            imgBanner.BeginAnimation(Image.OpacityProperty, fadeOut);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, shrinkOut);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, shrinkOut);
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
