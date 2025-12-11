using MailKit.Search;
using Microsoft.Identity.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static MaterialDesignThemes.Wpf.Theme;

namespace CoffeeShop.View.Staff
{
    /// <summary>
    /// Interaction logic for Staff_Depot.xaml
    /// </summary>
    public partial class Staff_Depot : Page
    {
        private ICollectionView itemsView;
        List<DepotItem> items = new List<DepotItem>();
        public Staff_Depot()
        {
            InitializeComponent();
            itemsView = CollectionViewSource.GetDefaultView(items);
            AddDatagridData();
            LoadUnitData();
        }


        public void AddDatagridData()
        {
            // Add datas for datagrid
            items.Add(new DepotItem
            {
                ID = 1,
                Name = "Cà phê hạt",
                Quantity = 50,
                Unit = "kg",
                Note = "Hết hàng sớm"
            });

            items.Add(new DepotItem
            {
                ID = 2,
                Name = "Ly nhựa 500ml",
                Quantity = 200,
                Unit = "cái",
                Note = "As you can see, this is a super long note."
            });

            items.Add(new DepotItem
            {
                ID = 3,
                Name = "Trân châu",
                Quantity = 100,
                Unit = "túi",
                Note = ""
            });
            items.Add(new DepotItem
            {
                ID = 3,
                Name = "Trân châu",
                Quantity = 100,
                Unit = "túi",
                Note = ""
            });
            items.Add(new DepotItem
            {
                ID = 3,
                Name = "Trân châu",
                Quantity = 100,
                Unit = "túi",
                Note = ""
            });
            items.Add(new DepotItem
            {
                ID = 3,
                Name = "Trân châu",
                Quantity = 100,
                Unit = "túi",
                Note = ""
            });
            items.Add(new DepotItem
            {
                ID = 3,
                Name = "Trân châu",
                Quantity = 100,
                Unit = "túi",
                Note = ""
            });
            items.Add(new DepotItem
            {
                ID = 3,
                Name = "Trân châu",
                Quantity = 100,
                Unit = "túi",
                Note = ""
            });
            items.Add(new DepotItem
            {
                ID = 3,
                Name = "Trân châu",
                Quantity = 100,
                Unit = "túi",
                Note = ""
            });
            items.Add(new DepotItem
            {
                ID = 3,
                Name = "Trân châu",
                Quantity = 100,
                Unit = "túi",
                Note = ""
            });
            items.Add(new DepotItem
            {
                ID = 3,
                Name = "Trân châu",
                Quantity = 100,
                Unit = "túi",
                Note = ""
            });
            items.Add(new DepotItem
            {
                ID = 3,
                Name = "Trân châu",
                Quantity = 100,
                Unit = "túi",
                Note = ""
            });
            items.Add(new DepotItem
            {
                ID = 3,
                Name = "Trân châu",
                Quantity = 100,
                Unit = "túi",
                Note = ""
            });
            dgDepot.ItemsSource = items;
            dgDepot.ItemsSource = itemsView;
        }

        public void LoadUnitData()
        {
            cbUnitFilter.Items.Add("Kg");
            cbUnitFilter.Items.Add("Cái");
            cbUnitFilter.Items.Add("Túi");
        }

        // Class for adding datas
        public class DepotItem
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
            public string Unit { get; set; }
            public string Note { get; set; }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // Get datagrid's View
            var view = CollectionViewSource.GetDefaultView(dgDepot.ItemsSource);
            view.Filter = FilterItems;
        }

        private bool FilterItems(object item)
        {
            string searchText = txbSearch.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                return true;
            }
            
            if (item is DepotItem depotItem)
            {
                // Chuyen kieu item: object -> DepotItem
                // Chuyen text cua item thanh lower de so sanh

                // Tim kiem theo ten
                if (depotItem.Name != null && depotItem.Name.ToLower().Contains(searchText))
                {
                    return true;
                }
            }
            return false;
        }

        private void txbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnSearch.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
                e.Handled = true;
            }
        }

        private void btnPopup_Click(object sender, RoutedEventArgs e)
        {
            popupFilter.IsOpen = !popupFilter.IsOpen;
        }

        private void btnApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            // 1. Lấy giá trị cho Amount Filter (chuyển sang int)
            bool isAmountMinValid = int.TryParse(txbMin1.Text, out int amountMin);
            bool isAmountMaxValid = int.TryParse(txbMax1.Text, out int amountMax);
            string unit = cbUnitFilter.SelectedItem.ToString().ToLower();
            // 3. Định nghĩa hàm lọc
            itemsView.Filter = item =>
            {
                // Ép kiểu đối tượng item
                if (item is DepotItem depotItem)
                {
                    // --- LOGIC CHO AMOUNT ---

                    // Nếu không nhập Min/Max, thì điều kiện tương ứng là TRUE (luôn thỏa mãn)
                    bool amountPassesMin = !isAmountMinValid || (depotItem.Quantity >= amountMin);
                    bool amountPassesMax = !isAmountMaxValid || (depotItem.Quantity <= amountMax);

                    // Tổng hợp điều kiện Amount
                    bool amountFilterPassed = amountPassesMin && amountPassesMax;

                    bool unitFilterPassed = (unit == depotItem.Unit);
                    return amountFilterPassed && unitFilterPassed;
                }
                return false;
            };

            // 4. Cập nhật DataGrid
            itemsView.Refresh();

            // 5. Đóng Popup sau khi lọc (nếu muốn)
            popupFilter.IsOpen = false;
        }
    }
}
