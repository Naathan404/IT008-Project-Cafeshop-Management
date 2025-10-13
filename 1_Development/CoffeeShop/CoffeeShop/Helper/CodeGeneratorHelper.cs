using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeShop.Helper
{
    internal class CodeGeneratorHelper
    {
        public static string GenerateOTPCode(int len = 5)
        {
            string code;
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            // Dung Enumerable.Repeat(_chuoi, _solan) de lap lai chuoi ky tu 5 lan. => No se tao ra 5 chuoi giong het nhau
            // LinQ Select se duyệt qua từng chuỗi trong 5 chuỗi được tạo ra. Mỗi chuỗi nó sẽ chọn ngẫu nhiên một ký tự từ chuỗi chars.
            // ToArray để chuyển đổi kết quả ([5 ký tự được chọn ngẫu nhiên được lưu tạm vào chuỗi liệt kê Sequence]) thành một mảng ký tự.
            code = new string(Enumerable.Repeat(chars, len).Select(s => s[random.Next(s.Length)]).ToArray());
            return code;
        }
    }
}
