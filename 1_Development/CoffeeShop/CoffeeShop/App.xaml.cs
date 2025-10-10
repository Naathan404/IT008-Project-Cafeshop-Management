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
            View.LoginWindow loginWindow = new View.LoginWindow();
            View.MainWindow mainWindow = new View.MainWindow();
            bool? dialog = loginWindow.ShowDialog();
            if(dialog == false)
            {
                mainWindow.Close();
            }
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Your startup code here

        }

    }

}
