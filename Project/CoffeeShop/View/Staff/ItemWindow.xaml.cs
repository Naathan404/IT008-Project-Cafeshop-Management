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

namespace CoffeeShop.View.Staff
{
    public partial class ItemWindow : Window
    {
        public ItemWindow()
        {
            InitializeComponent();
        }

        public ItemWindow(object dataContext) : this()
        {
            DataContext = dataContext;
        }

        #region ItemSize Events
        private void bdrItemSize_MouseEnter(object sender, MouseEventArgs e)
        {

        }
        private void bdrItemSizeS_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        
        private void bdrItemSizeM_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void bdrItemSizeL_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        #endregion

        private void bdrExit_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void bdrExit_MouseEnter(object sender, MouseEventArgs e)
        {

        }
    }
}
