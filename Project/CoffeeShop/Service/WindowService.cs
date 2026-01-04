using CoffeeShop.Service.Interfaces;
using CoffeeShop.View.Staff; 
using System.Collections.ObjectModel;
using CoffeeShop.Service.DTOs;
using CoffeeShop.View.Admin;

namespace CoffeeShop.Service
{
    public class WindowService : IDialogService
    {
        // Triển khai hàm theo đúng hợp đồng của Interface
        public void OpenInsertMaterialWindow(ObservableCollection<DepotItemDTO> collection,
                                 DepotItemDTO? itemToEdit)
        {
            if (itemToEdit != null)
            {
                // CHẾ ĐỘ CẬP NHẬT: Dùng constructor 2 (truyền đối tượng đang sửa)
                InsertMaterial insertMaterial = new InsertMaterial(itemToEdit, collection);
                insertMaterial.ShowDialog();
            }
            else
            {
                // CHẾ ĐỘ THÊM MỚI: Dùng constructor 1
                InsertMaterial insertMaterial = new InsertMaterial(collection);
                insertMaterial.ShowDialog();
            }
        }

        public void OpenDepotHistoryWindow()
        {
            DepotHistory depotHistory = new DepotHistory();
            depotHistory.ShowDialog();
        }

        public void OpenReportDepotWindow()
        {
            ReportDepotWindow reportDepot = new ReportDepotWindow();
            reportDepot.ShowDialog();
        }

        public bool? OpenInsertCouponWindow(CouponDTO? itemToEdit = null)
        {
            // Truyền itemToEdit vào constructor của Window
            InsertCouponWindow insertCoupon = new InsertCouponWindow(itemToEdit);
            return insertCoupon.ShowDialog();
        }
    }
}
