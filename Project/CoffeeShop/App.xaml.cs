using CoffeeShop.Models;
using CoffeeShop.View;
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
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            var loadingWindow = new LoadingView();
            loadingWindow.Show();

            // Khởi động cơ sở dữ liệu
            await Task.Run(async () =>
            {
                using (var db = new CoffeeShopContext())
                {
                    try
                    {
                        await db.Database.CanConnectAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi kết nối cơ sở dữ liệu! Vui lòng kiểm tra lại kết nối.\n" + ex.Message, "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
                        Application.Current.Shutdown();

                    }
                }
            });

            InitDefaultColors();

            // --- BƯỚC 2: KHỞI TẠO VÀ HIỂN THỊ CỬA SỔ ---
            View.Login.LoginWindow loginWindow = new View.Login.LoginWindow();
            loadingWindow.Close();
            bool? dialog = loginWindow.ShowDialog();
            if(dialog == false)
            {
                loginWindow.Close();
            }
        }

        private void InitDefaultColors()
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
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // Thực hiện các thao tác dọn dẹp
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
        }

    }

}
