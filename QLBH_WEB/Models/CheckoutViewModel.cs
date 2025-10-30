// Trong QLBH_WEB/Models/CheckoutViewModel.cs
using QLBH_WEB.Models; // Giả sử Cart.cs ở đây

namespace QLBH_WEB.Models
{
    public class CheckoutViewModel
    {
        // Thông tin giỏ hàng để hiển thị
        public Cart Cart { get; set; }

        // Thông tin khách hàng nhập vào form
        public string HoTen { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public string GhiChu { get; set; }
    }
}