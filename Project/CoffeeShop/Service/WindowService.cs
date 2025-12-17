using CoffeeShop.Service.Interfaces;
using CoffeeShop.View.Staff;    // Dùng InputWindow (View)
using System.Collections.ObjectModel;
using CoffeeShop.Service.DTOs;

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

        public void OpenReportDepotWindow(List<DepotItemDTO> reportData, string reportPath)
        {
            ReportDepotWindow reportDepot = new ReportDepotWindow(reportData, reportPath);
            reportDepot.ShowDialog();
        }
    }
}
