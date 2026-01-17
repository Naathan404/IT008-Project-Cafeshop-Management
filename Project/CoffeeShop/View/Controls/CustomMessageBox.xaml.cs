using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CoffeeShop.View.Controls
{
    public partial class CustomMessageBox : Window
    {
        public enum MessageType { Info, Warning, Error, Success }
        public enum MessageBoxResult { Ok, Cancel, Yes, No }
        public enum MessageButtons { OK, OKCancel, YesNo, YesNoCancel }

        public MessageBoxResult Result { get; set; }

        public CustomMessageBox(string message,
                                string title = "Thông báo", // Mặc định là "Thông báo"
                                MessageType? type = null,
                                MessageButtons buttons = MessageButtons.OK ) // Mặc định là button OK
        {
            InitializeComponent();

            txtTitle.Text = title;
            txtMessage.Text = message;
            switch (buttons)
            {
                case MessageButtons.OK:
                    btnOk.Visibility = Visibility.Visible;
                    break;
                case MessageButtons.OKCancel:
                    btnOk.Visibility = Visibility.Visible;
                    btnCancel.Visibility = Visibility.Visible;
                    break;
                case MessageButtons.YesNo:
                    btnYes.Visibility = Visibility.Visible;
                    btnNo.Visibility = Visibility.Visible;
                    break;
                case MessageButtons.YesNoCancel:
                    btnCancel.Visibility = Visibility.Visible;
                    btnYes.Visibility = Visibility.Visible;
                    btnNo.Visibility = Visibility.Visible;
                    break;
            }
            DisplayMessageTypeIcon(type);
        }

        public void DisplayMessageTypeIcon(MessageType? type)
        {
            switch (type)
            {
                case MessageType.Info:
                    imgInfo.Visibility = Visibility.Visible;
                    break;
                case MessageType.Warning:
                    imgWarning.Visibility = Visibility.Visible;
                    break;
                case MessageType.Error:
                    imgError.Visibility = Visibility.Visible;
                    break;
                case MessageType.Success:
                    imgSuccess.Visibility = Visibility.Visible;
                    break;
            }
        }
        public static MessageBoxResult Show(string message,
                                            string title = "Thông báo",
                                            MessageType type = MessageType.Info,
                                            MessageButtons buttons = MessageButtons.OK)
        {
            var msgBox = new CustomMessageBox(message, title, type, buttons);
            msgBox.ShowDialog();
            return msgBox.Result;
        }

        private void btnCancel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnOk_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Result = MessageBoxResult.Ok;
            this.Close();
        }

        private void btnYes_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            this.Close();
        }

        private void btnNo_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Result = MessageBoxResult.No;
            this.Close();
        }

        #region Button Events
        private void btn_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D4BA98"));
                if (border.Child is TextBlock txtb)
                {
                    txtb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#340D05"));
                }
            }
        }
        private void btn_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = Brushes.Transparent;
                if (border.Child is TextBlock txtb)
                {
                    txtb.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#766839"));
                }
            }
        }
        #endregion
    }
}