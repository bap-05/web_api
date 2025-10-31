namespace QLBH_API.Models
{
    public class HoaDonCreateDto
    {
        // Giả sử bạn có thông tin khách hàng, ở đây tôi dùng tạm ID 1
       
            public int MaKhachHang { get; set; }
            public decimal TongTien { get; set; }
            public List<ChiTietDonHangDto> ChiTietHoaDon { get; set; }
        
    }
}
