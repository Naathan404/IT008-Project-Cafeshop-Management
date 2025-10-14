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
using System.Windows.Media.Animation;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for StaffWindow.xaml
    /// </summary>
    /// 
    public class GridLengthAnimation : AnimationTimeline
    {
        public override Type TargetPropertyType => typeof(GridLength);

        public GridLength From { get; set; }
        public GridLength To { get; set; }

        public override object GetCurrentValue(object defaultOriginValue, object defaultDestinationValue, AnimationClock animationClock)
        {
            double from = From.Value;
            double to = To.Value;

            if (from > to)
                return new GridLength((1 - animationClock.CurrentProgress.Value) * (from - to) + to, GridUnitType.Pixel);
            else
                return new GridLength(animationClock.CurrentProgress.Value * (to - from) + from, GridUnitType.Pixel);
        }

        protected override Freezable CreateInstanceCore() => new GridLengthAnimation();
    }
    public partial class StaffWindow : Window
    {
        private bool isExpanded = false;
        public StaffWindow()
        {
            InitializeComponent();
        }
        private void bdrOrder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new Staff_Order());
        }

        private void bdrMenu_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new Staff_Menu());
        }

        private void bdrDepot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new Staff_Depot());
        }

        private void bdrStatistics_MouseDown(object sender, MouseButtonEventArgs e)
        {
            StaffFrame.Visibility = Visibility.Visible;
            StaffFrame.Navigate(new Staff_Statistics());
        }

        private void bdrStaffWindowFunction_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double from = SidebarColumn.ActualWidth;
            double to = isExpanded ? 80 : 200;

            var anim = new DoubleAnimation(from, to, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            var gridLengthAnim = new GridLengthAnimation
            {
                From = new GridLength(from),
                To = new GridLength(to),
                Duration = anim.Duration
            };

            SidebarColumn.BeginAnimation(ColumnDefinition.WidthProperty, gridLengthAnim);

            isExpanded = !isExpanded;
        }
    }
}
