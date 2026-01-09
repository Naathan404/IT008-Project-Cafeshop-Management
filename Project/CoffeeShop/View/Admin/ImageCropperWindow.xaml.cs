using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CoffeeShop.View.Admin
{
    public partial class ImageCropperWindow : Window
    {
        private Point _startPoint;
        private double _originalLeft, _originalTop;
        private bool _isDragging = false;
        private BitmapImage _originalBitmap;
        public BitmapSource CroppedImage { get; private set; }

        public ImageCropperWindow(string filePath)
        {
            InitializeComponent();
            LoadImage(filePath);
            this.Loaded += (s, e) => AdjustSelectionRect();

            // Tự động căn lại khi người dùng kéo giãn cửa sổ máy tính
            this.SizeChanged += (s, e) => AdjustSelectionRect();
        }

        private void LoadImage(string path)
        {
            try
            {
                _originalBitmap = new BitmapImage();
                _originalBitmap.BeginInit();
                _originalBitmap.UriSource = new Uri(path, UriKind.Absolute);
                _originalBitmap.CacheOption = BitmapCacheOption.OnLoad;
                _originalBitmap.EndInit();
                SourceImage.Source = _originalBitmap;
            }
            catch { this.Close(); }
        }

        private void UpdateOverlay()
        {
            if (SourceImage.ActualWidth == 0 || SelectionRect.Width == 0) return;

            FullArea.Rect = new Rect(0, 0, SourceImage.ActualWidth, SourceImage.ActualHeight);

            double left = Canvas.GetLeft(SelectionRect);
            double top = Canvas.GetTop(SelectionRect);

            // Kiểm tra an toàn trước khi vẽ vùng đục lỗ
            if (!double.IsNaN(left) && !double.IsNaN(top))
            {
                CropArea.Rect = new Rect(left, top, SelectionRect.Width, SelectionRect.Height);
            }
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _startPoint = e.GetPosition(CanvasCropper); // Lấy vị trí chuột so với Canvas

                // Lấy vị trí hiện tại của khung
                _originalLeft = Canvas.GetLeft(SelectionRect);
                _originalTop = Canvas.GetTop(SelectionRect);

                // Nếu khung chưa có tọa độ (lần đầu), mặc định là 0,0
                if (double.IsNaN(_originalLeft)) _originalLeft = 0;
                if (double.IsNaN(_originalTop)) _originalTop = 0;

                SelectionRect.CaptureMouse(); // Giữ chuột để kéo mượt hơn
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            Point currentPoint = e.GetPosition(CanvasCropper);
            double deltaX = currentPoint.X - _startPoint.X;
            double deltaY = currentPoint.Y - _startPoint.Y;

            double newLeft = _originalLeft + deltaX;
            double newTop = _originalTop + deltaY;

            // Tính biên an toàn: không bao giờ để Max < 0
            double maxLeft = Math.Max(0, CanvasCropper.Width - SelectionRect.Width);
            double maxTop = Math.Max(0, CanvasCropper.Height - SelectionRect.Height);

            // Ép khung nằm trong ảnh
            newLeft = Math.Clamp(newLeft, 0, maxLeft);
            newTop = Math.Clamp(newTop, 0, maxTop);

            Canvas.SetLeft(SelectionRect, newLeft);
            Canvas.SetTop(SelectionRect, newTop);

            UpdateOverlay();
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            SelectionRect.ReleaseMouseCapture();
        }

        private void Crop_Click(object sender, RoutedEventArgs e)
        {
            // Tính toán tỉ lệ giữa ảnh gốc và ảnh hiển thị trên màn hình
            double scaleX = _originalBitmap.PixelWidth / SourceImage.ActualWidth;
            double scaleY = _originalBitmap.PixelHeight / SourceImage.ActualHeight;

            int x = (int)(Canvas.GetLeft(SelectionRect) * scaleX);
            int y = (int)(Canvas.GetTop(SelectionRect) * scaleY);
            int w = (int)(SelectionRect.Width * scaleX);
            int h = (int)(SelectionRect.Height * scaleY);

            // Cắt ảnh
            try
            {
                CroppedImage = new CroppedBitmap(_originalBitmap, new Int32Rect(x, y, w, h));
                this.DialogResult = true;
            }
            catch
            {
                MessageBox.Show("Vùng cắt không hợp lệ!");
            }
        }

        private void AdjustSelectionRect()
        {
            Dispatcher.BeginInvoke(new Action(() => {
                if (SourceImage.ActualWidth == 0 || SourceImage.ActualHeight == 0) return;

                double imgW = SourceImage.ActualWidth;
                double imgH = SourceImage.ActualHeight;

                // Tìm cạnh nhỏ nhất của ảnh để làm chuẩn cho hình vuông
                double minImgSide = Math.Min(imgW, imgH);

                // Nếu cạnh nhỏ nhất của ảnh < 300, khung sẽ fit sát theo cạnh đó
                // Nếu ảnh lớn, khung sẽ để mặc định là 300 (hoặc tùy bạn chỉnh)
                double side = (minImgSide < 300) ? minImgSide : 300;

                SelectionRect.Width = side;
                SelectionRect.Height = side;

                // Tính toán để đưa khung vào giữa ảnh
                double left = (imgW - side) / 2;
                double top = (imgH - side) / 2;

                Canvas.SetLeft(SelectionRect, left);
                Canvas.SetTop(SelectionRect, top);

                UpdateOverlay();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;
    }
}