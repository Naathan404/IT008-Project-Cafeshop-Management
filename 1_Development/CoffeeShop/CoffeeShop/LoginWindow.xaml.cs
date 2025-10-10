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

namespace CoffeeShop.View
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private List<(string Quote, string Author)> _quotes = new List<(string, string)>
        {
            ("I orchestrate my mornings to the tune of coffee.", "Terri Guillemets"),
            ("Even bad coffee is better than no coffee at all.", "David Lynch"),
            ("Coffee is the balm of the heart and spirit.", "Cellini Caffe"),
            ("There’s nothing sweeter than a cup of bitter coffee.", "Rian Aditia"),
            ("Coffee is a kind of magic you can drink.", "Catherynne M. Valente"),
            ("Life’s too short for bad coffee.", "Nescafe Australia"),
            ("Everyone starts somewhere!", "Anonymous"),
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
            GenerateRandomLoginUI();
        }
        private void GenerateRandomLoginUI()
        {
            var quote = _quotes[new Random().Next(0, _quotes.Count)];
            txblQuote.Text = "\"" + quote.Quote + "\"";
            txblAuthor.Text = "- " + quote.Author + " -";
            imgBanner.Source = new BitmapImage(new Uri(_imgBannerSources[new Random().Next(0, _imgBannerSources.Count)], UriKind.Relative));
        }

        private void ProcessLogin()
        {
            string username = txbFloatingUsernameBox.Text.Trim();
            string passwd = txbFloatingPasswordBox.Password.Trim();
            string hashPasswd = HashHelper.Base64_Encode(passwd);

            using(var db = new CoffeeShopContext())
            {
                var staff = db.Staff.FirstOrDefault(user => user.Username == username && user.PasswordHash == hashPasswd);
                if(staff != null)
                {
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu! \n" + txbFloatingPasswordBox.Password + " \n" +  hashPasswd);
                }
            }
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
    }
}
