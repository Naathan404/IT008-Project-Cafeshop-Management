using CoffeeShop.Models;
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
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Helper;
using System.Windows.Media.Animation;
using System.Printing;

namespace CoffeeShop.View
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private List<(string Quote, string Author)> _quotes = new List<(string, string)>
        {
            ("Thà uống cà phê tồi, còn hơn không có giọt nào.", "David Lynch"),
            ("Cà phê là liều thuốc chữa lành cho trái tim và tâm hồn.", "Cellini Caffe"),
            ("Chẳng có gì ngọt ngào hơn một tách cà phê đắng.", "Rian Aditia"),
            ("Cà phê là một thứ ma thuật có thể uống.", "Catherynne M. Valente"),
            ("Cuộc đời quá ngắn để uống cà phê tồi.", "Nescafe Australia"),
            ("Cứ mỗi phút bạn giận dữ là bạn đánh mất sáu mươi giây hạnh phúc.", "Ralph Waldo Emerson"),
            ("Cà phê ngon phải đen như địa ngục, đắng như ác quỷ và ngọt ngào như tình yêu.", "Charles M. de Talleyrand"),
            ("Cuộc đời cũng như một tách cà phê. Quan trọng không phải là cà phê ngon hay dở, mà là cách ta thưởng thức nó.", "Khuyết danh"),
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


        public LoginWindow()
        {
            InitializeComponent();
            WarmUpDatabase();
            GenerateRandomLoginUI();
        }
        private void GenerateRandomLoginUI()
        {
            var quote = _quotes[new Random().Next(0, _quotes.Count)];
            txblQuote.Text = "\"" + quote.Quote + "\"";
            txblAuthor.Text = "- " + quote.Author + " -";
            imgBanner.Source = new BitmapImage(new Uri(_imgBannerSources[new Random().Next(0, _imgBannerSources.Count)], UriKind.Relative));

            txblNotify.Visibility = Visibility.Hidden;
        }

        private void WarmUpDatabase()
        {
            using (var db = new CoffeeShopContext())
            {
                try
                {
                    db.Staff.Take(1).Any();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối cơ sở dữ liệu! Vui lòng kiểm tra lại kết nối.\n" + ex.Message, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();

                }
            }
        }

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
                            LoginStaffWindow();
                            break;
                        case "Admin":
                            LoginAdminWindow();
                            break;
                        default:
                            MessageBox.Show("Unidentified user");
                            break;
                    }
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

        private void LoginAdminWindow()
        {
            AdminWindow adminWindow = new AdminWindow();
            adminWindow.Show();
            this.Close();
        }
        private void LoginStaffWindow()
        {
            StaffWindow staffWindow = new StaffWindow();
            staffWindow.Show();
            this.Close();
        }


        private void btnEvt_LoginButtonClick(object sender, RoutedEventArgs e)
        {
            ProcessLogin();
        }

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

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
    }
}
