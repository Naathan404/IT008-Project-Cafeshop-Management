# CODING CONVENTION  

Thống nhất sử dụng các từ tiếng Anh trong code. Comment code hay commit vẫn có thể tiếng Việt bình thường
### Đặt tên biến:
Là danh từ, nếu là kiểu bool phải có từ is ở trước
- Private: _tenBien, ví dụ _isInteractable
- Protected: _tenBien
- Public: tenBien
- Hằng số: TEN_HANG_SO
### Đặt tên class, tên hàm, tên enum
- Class: TenLop
- Func: TenHam (tên hàm phải là động từ)
- Nếu là hàm trả về bool, phải là: IsTenHam, ví dụ IsChecked();
- Enum: ETenEnum
- Interface: ITenInterface
### Khối lệnh
- Phải để dấu { xuống dòng so với tên hàm hoặc tên lớp, tên enum,...
- Mỗi câu lệnh trên 1 dòng
### Tiền tố 1 số control
- Thường sẽ là viết tắt 3 chữ cái, đặt tên theo dạng: tentientoTenControl, ví dụ btnNext
- Panel: pnl
- Button: btn
- Check: chk
- Dialog: dlg
- form: frm
- ...
