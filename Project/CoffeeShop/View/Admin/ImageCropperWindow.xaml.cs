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
            // Lấy tọa độ thực tế của khung vàng trên Canvas
            double left = Canvas.GetLeft(SelectionRect);
            double top = Canvas.GetTop(SelectionRect);
            double width = SelectionRect.Width;
            double height = SelectionRect.Height;

            // Tính toán tỉ lệ giữa kích thước file ảnh thật và kích thước hiển thị trên màn hình
            var bitmap = (BitmapSource)SourceImage.Source;
            double scaleX = bitmap.PixelWidth / SourceImage.ActualWidth;
            double scaleY = bitmap.PixelHeight / SourceImage.ActualHeight;

            // Vùng cắt thực tế trên file gốc
            Int32Rect rect = new Int32Rect(
                (int)(left * scaleX),
                (int)(top * scaleY),
                (int)(width * scaleX),
                (int)(height * scaleY)
            );

            try
            {
                CroppedBitmap cropped = new CroppedBitmap(bitmap, rect);
                this.CroppedImage = cropped; // Property để ViewModel nhận ảnh
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Vùng chọn vượt quá biên ảnh hoặc không hợp lệ!");
            }
        }

        private void AdjustSelectionRect()
        {
            Dispatcher.BeginInvoke(new Action(() => {
                if (SourceImage.ActualWidth == 0 || SourceImage.ActualHeight == 0) return;

                double imgW = SourceImage.ActualWidth;
                double imgH = SourceImage.ActualHeight;

                // Tỉ lệ mục tiêu là 250 / 180 = 1.3888
                double targetRatio = 250.0 / 180.0;
                double currentImgRatio = imgW / imgH;

                double finalW, finalH;

                // Nếu ảnh "gầy" hơn tỉ lệ mục tiêu -> lấy chiều rộng ảnh làm chuẩn
                if (currentImgRatio < targetRatio)
                {
                    finalW = imgW;
                    finalH = imgW / targetRatio;
                }
                // Nếu ảnh "béo" hơn tỉ lệ mục tiêu -> lấy chiều cao ảnh làm chuẩn
                else
                {
                    finalH = imgH;
                    finalW = imgH * targetRatio;
                }

                // Giảm xuống một chút (ví dụ 90%) để người dùng dễ nhìn thấy viền nếu muốn
                // Hoặc để 100% (finalW, finalH) nếu muốn sát khít hoàn toàn
                SelectionRect.Width = finalW;
                SelectionRect.Height = finalH;

                // Căn giữa
                Canvas.SetLeft(SelectionRect, (imgW - finalW) / 2);
                Canvas.SetTop(SelectionRect, (imgH - finalH) / 2);

                UpdateOverlay();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.DialogResult = false;
    }
}