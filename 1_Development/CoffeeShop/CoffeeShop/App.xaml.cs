using System.Configuration;
using System.Data;
using System.Windows;

namespace CoffeeShop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            MainWindow mainWindow = new MainWindow();
            bool? isLoginSuccess; // bool? nhận 3 trạng thái: true, false và null
            isLoginSuccess = loginWindow.ShowDialog();

            if (isLoginSuccess == true) // Login thanh cong
            {
                mainWindow.Show();
            }
            else if (isLoginSuccess == false) // Tat App
            {
                Shutdown(); // Đóng cửa sổ đang mở (LoginWindow)
            }
            else // isLoginSuccess == null, chưa làm gì hết
            {
            }
        }
    }

}
