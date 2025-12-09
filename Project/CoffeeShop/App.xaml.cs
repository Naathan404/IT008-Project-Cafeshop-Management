using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Media;

namespace CoffeeShop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var paletteHelper = new PaletteHelper();

            // QUAN TRỌNG: Thay vì GetTheme() (sẽ lỗi vì chưa có theme), ta tạo mới luôn
            MaterialDesignThemes.Wpf.Theme theme = new MaterialDesignThemes.Wpf.Theme();
            theme.SetBaseTheme(BaseTheme.Light);

            // Định nghĩa màu Cà phê
            var primaryColor = Color.FromRgb(0xB1, 0x82, 0x52);
            var secondaryColor = Color.FromRgb(0x39, 0x1B, 0x05);

            theme.SetPrimaryColor(primaryColor);
            theme.SetSecondaryColor(secondaryColor);

            // Cài đặt chi tiết (Tùy chọn nhưng nên có cho đẹp)
            theme.PrimaryLight = new ColorPair(Color.FromRgb(0xE0, 0xCE, 0xB5), Colors.Black);
            theme.PrimaryMid = new ColorPair(primaryColor, Colors.White);
            theme.PrimaryDark = new ColorPair(Color.FromRgb(0x39, 0x1B, 0x05), Colors.White);

            // Áp dụng theme
            paletteHelper.SetTheme(theme);


            // --- BƯỚC 2: KHỞI TẠO VÀ HIỂN THỊ CỬA SỔ ---
            View.Login.LoginWindow loginWindow = new View.Login.LoginWindow();
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

        }

    }

}
