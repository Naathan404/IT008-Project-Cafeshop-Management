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
        public bool? OpenInsertMaterialWindow(ObservableCollection<DepotItemDTO> collection,
                                             DepotItemDTO? itemToEdit)
        {
            InsertMaterial insertMaterial;
            if (itemToEdit != null)
            {
                // CHẾ ĐỘ CẬP NHẬT
                insertMaterial = new InsertMaterial(itemToEdit, collection);
            }
            else
            {
                // CHẾ ĐỘ THÊM MỚI
                insertMaterial = new InsertMaterial(collection);
            }
            return insertMaterial.ShowDialog();
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
