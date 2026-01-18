using CoffeeShop.Helper;
using CoffeeShop.Models;
using CoffeeShop.Service.DTOs;
using CoffeeShop.View.Admin;
using CoffeeShop.View.Controls;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using static CoffeeShop.View.Controls.CustomMessageBox;

namespace CoffeeShop.ViewModels.AdminVM
{
    public class AdminStaffManagementViewModel : BaseViewModel
    {
        CultureInfo viVn = new CultureInfo("vi-VN");
        StaffManagementPage _page;

        #region Properties
        CancellationTokenSource _cts;
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string[] RoleListFilter { get; } = new string[]
        {
            "Tất cả",
            "Quản lý",
            "Nhân viên"
        };
        private string _selectedRoleFilter = "Tất cả";
        public string SelectedRoleFilter
        {
            get => _selectedRoleFilter;
            set
            {
                _selectedRoleFilter = value;
                OnPropertyChanged();
                _ = LoadEmployees();
            }
        }

        private string _selectedShiftFilter = "Tất cả";
        public string SelectedShiftFilter
        {
            get => _selectedShiftFilter;
            set
            {
                _selectedShiftFilter = value;
                OnPropertyChanged();
                _ = LoadEmployees();
            }
        }

        private string _searchName = string.Empty;
        public string SearchName
        {
            get => _searchName;
            set
            {
                _searchName = value;
                OnPropertyChanged();
                _ = LoadEmployees();
            }
        }

        private ObservableCollection<StaffDTO> _employeeList = new ObservableCollection<StaffDTO>();
        public ObservableCollection<StaffDTO> EmployeeList
        {
            get => _employeeList;
            set
            {
                _employeeList = value;
                OnPropertyChanged();
            }
        }

        private StaffDTO _selectedEmployee = new StaffDTO();
        public StaffDTO SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged();

                if (_selectedEmployee != null && _selectedEmployee.StaffId > 0)
                {
                    Label = "CẬP NHẬT THÔNG TIN";
                }
                else
                {
                    Label = "THÔNG TIN NHÂN VIÊN";
                }
            }
        }

        public string[] ShiftListFilter { get; } = null!;

        private string _totalEmployees = null!;
        public string TotalEmployees
        {
            get => _totalEmployees;
            set
            {
                _totalEmployees = value;
                OnPropertyChanged();
            }
        }

        private string _totalManagers = null!;
        public string TotalManagers
        {
            get => _totalManagers;
            set
            {
                _totalManagers = value;
                OnPropertyChanged();
            }
        }

        private string _totalStaffs = null!;
        public string TotalStaffs
        {
            get => _totalStaffs;
            set
            {
                _totalStaffs = value;
                OnPropertyChanged();
            }
        }

        public string[] RoleList { get; } = new string[]
        {
            "Quản lý",
            "Nhân viên"
        };

        public string[] GenderList { get; } = new string[]
        {
            "Nam",
            "Nữ"
        };

        public string[] ShiftList { get; } = new string[]
        {
            "Sáng",
            "Chiều",
            "Tối",
            "---"
        };

        private string _label = "THÔNG TIN NHÂN VIÊN";
        public string Label
        {
            get => _label;
            set
            {
                _label = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands
        public ICommand RefreshCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand AddCommand { get; set; }
        #endregion

        public AdminStaffManagementViewModel(StaffManagementPage page)
        {
            _page = page;
            using(var db = new CoffeeShopContext())
            {
                var shifts = db.Shifts.Select(shift => shift.ShiftName).Distinct().ToList();
                shifts.Insert(0, "Tất cả");
                ShiftListFilter = shifts.ToArray();
            }
            SelectedRoleFilter = RoleListFilter[0];
            SelectedShiftFilter = ShiftListFilter[0];
            Label = "THÔNG TIN NHÂN VIÊN";

            RefreshCommand = new RelayCommand<object>((p) =>
            {
                Refresh();
            });

            DeleteCommand = new RelayCommand<object>(async (p) =>
            {
                if (SelectedEmployee == null || SelectedEmployee.StaffId == 0) return;

                var result = CustomMessageBox.Show($"Bạn có chắc muốn xóa nhân viên {SelectedEmployee.StaffName}?", "Xác nhận", MessageButtons.YesNo, MessageType.Question);
                if (result == CustomMessageBox.MessageBoxResult.Yes)
                {
                    using (var db = new CoffeeShopContext())
                    {
                        var staff = await db.Staff.FindAsync(SelectedEmployee.StaffId);
                        if (staff != null)
                        {
                            staff.IsDeleted = true; // Xóa mềm
                            await db.SaveChangesAsync();
                            CustomMessageBox.Show("Đã xóa thành công!", "Thành công", MessageButtons.OK, MessageType.Success);
                            Refresh(); // Tải lại trang
                        }
                    }
                }
            });

            AddCommand = new RelayCommand<object>(p =>
            {
                Label = "THÊM NHÂN VIÊN";
                SetUpAddNewEmployee();
            });

            SaveCommand = new RelayCommand<object>(async p =>
            {
                await ProcessSaveEmployeeInfo();
            });

            _ = LoadEmployees();
        }

        // Fix for CS8030 and CS8031 in LoadEmployees method
        private async Task LoadEmployees()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                EmployeeList.Clear();
            });

            string keyword = SearchName?.Trim().ToLower() ?? "";
            string currentRole = SelectedRoleFilter;
            string currentShift = SelectedShiftFilter;

            try
            {
                IsLoading = true;
                var result = await Task.Run(async () =>
                {
                    if (token.IsCancellationRequested) return null; // <-- Fix: return null instead of a List<Staff>
                    using (var db = new CoffeeShopContext())
                    {
                        var query = db.Staff.
                            AsNoTracking()
                            .Where(o => o.IsDeleted == false)
                            .AsQueryable();

                        // Lọc theo Tên
                        if (!string.IsNullOrEmpty(keyword))
                            query = query.Where(x => x.StaffName.ToLower().Contains(keyword));

                        // Lọc theo Role (Giả sử DB lưu tiếng Việt, nếu DB lưu tiếng Anh thì cần map lại)
                        if (currentRole != "Tất cả")
                        {
                            string dbRole = currentRole == "Quản lý" ? "Admin" : "Employee";
                            query = query.Where(x => x.StaffRole == dbRole);
                        }

                        // Lọc theo Ca
                        if (currentShift != "Tất cả")
                        {
                            var shiftId = db.Shifts.FirstOrDefault(s => s.ShiftName == currentShift)?.ShiftId;
                            if (shiftId != null)
                                query = query.Where(x => x.ShiftId == shiftId);
                        }

                        var data = await query.Include(o => o.Shift).OrderBy(x => x.StaffId).ToListAsync();

                        return new
                        {
                            List = data.Select(r => new StaffDTO
                            {
                                StaffId = r.StaffId,
                                StaffName = r.StaffName,
                                StaffRole = r.StaffRole == "Admin" ? "Quản lý" : "Nhân viên",
                                Username = r.Username,
                                Phonenumber = r.Phonenumber,
                                Email = r.Email,
                                BaseSalary = r.BaseSalary != null ? r.BaseSalary.Value.ToString("N0", viVn) : "---",
                                ShiftName = r.ShiftId != null ? r.Shift!.ShiftName : "---",
                                ShiftId = r.ShiftId.ToString(),
                                Birthday = r.Birthday,
                                StartDate = r.StartDate,
                                Gender = r.Gender,
                            }).ToList(),
                            CountTotal = data.Count,
                            CountAdmin = data.Count(x => x.StaffRole == "Quản lý" || x.StaffRole == "Admin"),
                            CountStaff = data.Count(x => x.StaffRole == "Nhân viên" || x.StaffRole == "Employee")
                        };
                    }
                }, token);

                if (token.IsCancellationRequested || result == null) return; // <-- Fix: check for null

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    EmployeeList.Clear();
                    foreach (var item in result.List)
                    {
                        EmployeeList.Add(item);
                    }
                });
                TotalEmployees = result.CountTotal.ToString();
                TotalManagers = result.CountAdmin.ToString();
                TotalStaffs = result.CountStaff.ToString();

                EmployeeList = new ObservableCollection<StaffDTO>(result.List);
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    IsLoading = false;
            }
        }

        private void SetUpAddNewEmployee()
        {
            SelectedEmployee = null!;
            StaffDTO newEmployee = new StaffDTO()
            {
                StaffId = 0,
                StaffName = null!,
                StaffRole = "Nhân viên",
                BaseSalary = 20000.ToString("N0", viVn),    
                Birthday = DateTime.Now.AddYears(-18),
                StartDate = DateTime.Now,
                Gender = "Nam",
                ShiftName = "Sáng"
            };

            SelectedEmployee = newEmployee;
        }

        private async Task ProcessSaveEmployeeInfo()
        {
            if (SelectedEmployee == null) return;

            if (string.IsNullOrEmpty(SelectedEmployee.StaffName) ||
            string.IsNullOrEmpty(SelectedEmployee.Username) ||
            SelectedEmployee.Birthday == null || SelectedEmployee.StartDate == null)
            {
                CustomMessageBox.Show("Vui lòng nhập đầy đủ Tên và Tài khoản!", "Thông báo", MessageButtons.OK, MessageType.Warning);
                return;
            }
            decimal? finalSalary = null;
            if (!string.IsNullOrEmpty(SelectedEmployee.BaseSalary) && SelectedEmployee.BaseSalary != "---")
            {
                string cleanSalary = SelectedEmployee.BaseSalary.Replace(".", "").Replace(",", "");
                if (decimal.TryParse(cleanSalary, out decimal parsedSalary))
                {
                    finalSalary = parsedSalary;
                }
                else
                {
                    CustomMessageBox.Show("Tiền lương không hợp lệ! Vui lòng nhập số (ví dụ: 5.000.000)", "Lỗi", MessageButtons.OK, MessageType.Error);
                    return;
                }
            }


            using (var db = new CoffeeShopContext())
            {
                int? finalShiftId = null;
                string selectedName = SelectedEmployee.ShiftName;
                if (!string.IsNullOrEmpty(selectedName) && selectedName != "---")
                {
                    var shift = await db.Shifts.FirstOrDefaultAsync(s => s.ShiftName == selectedName);
                    if (shift != null)
                        finalShiftId = shift.ShiftId;
                }

                if (SelectedEmployee.StaffId == 0)
                {
                    if (await db.Staff.AnyAsync(x => x.Username == SelectedEmployee.Username))
                    {
                        CustomMessageBox.Show("Tên đăng nhập đã tồn tại!", "Lỗi", MessageButtons.OK, MessageType.Error);
                        return;
                    }

                    var staff = new Staff
                    {
                        StaffName = SelectedEmployee.StaffName,
                        StaffRole = SelectedEmployee.StaffRole == "Quản lý" ? "Admin" : "Employee",
                        Username = SelectedEmployee.Username,
                        PasswordHash = HashHelper.SHA256_Encode(HashHelper.Base64_Encode("12345678")),
                        Phonenumber = SelectedEmployee.Phonenumber ?? "",
                        Email = SelectedEmployee.Email ?? "",
                        BaseSalary = finalSalary,
                        StartDate = SelectedEmployee.StartDate ?? DateTime.Now,
                        Birthday = SelectedEmployee.Birthday ?? DateTime.Now,
                        Gender = SelectedEmployee.Gender ?? "Khác",
                        IsDeleted = false,
                        ShiftId = finalShiftId
                    };

                    db.Staff.Add(staff);
                    await db.SaveChangesAsync();
                    CustomMessageBox.Show("Thêm nhân viên thành công!", "Thành công", MessageButtons.OK, MessageType.Success);
                }
                else
                {
                    var staff = await db.Staff.FindAsync(SelectedEmployee.StaffId);
                    if (staff != null)
                    {
                        staff.StaffName = SelectedEmployee.StaffName;
                        staff.StaffRole = SelectedEmployee.StaffRole == "Quản lý" ? "Admin" : "Employee";
                        staff.Phonenumber = SelectedEmployee.Phonenumber ?? "";
                        staff.Email = SelectedEmployee.Email ?? "";
                        staff.BaseSalary = finalSalary;
                        staff.ShiftId = finalShiftId;
                        if (SelectedEmployee.StartDate.HasValue) staff.StartDate = SelectedEmployee.StartDate.Value;
                        if (SelectedEmployee.Birthday.HasValue) staff.Birthday = SelectedEmployee.Birthday.Value;
                        staff.Gender = SelectedEmployee.Gender;
                        staff.Username = SelectedEmployee.Username;
                        staff.PasswordHash = HashHelper.SHA256_Encode(HashHelper.Base64_Encode(_page.GetPasswordFromPasswordBox()));

                        await db.SaveChangesAsync();
                        CustomMessageBox.Show("Cập nhật thông tin thành công!", "Thành công", MessageButtons.OK, MessageType.Success);
                    }
                    else
                    {
                        CustomMessageBox.Show("Không tìm thấy nhân viên này (có thể đã bị xóa).", "Thông báo", MessageButtons.OK, MessageType.Error);
                    }    
                }
            }
            Refresh();
        }

        private void Refresh()
        {
            _selectedRoleFilter = RoleListFilter[0];
            OnPropertyChanged(nameof(SelectedRoleFilter));

            _selectedShiftFilter = ShiftListFilter[0];
            OnPropertyChanged(nameof(SelectedShiftFilter));

            _searchName = string.Empty;
            OnPropertyChanged(nameof(SearchName));

            _selectedEmployee = new StaffDTO();
            Label = "THÔNG TIN NHÂN VIÊN";
            _page.SetPasswordToPasswordBox("");

            _ = LoadEmployees();
        }
    }
}
