namespace QLBH_API.Models
{
    public class HoaDonCreateDto
    {
        // Giả sử bạn có thông tin khách hàng, ở đây tôi dùng tạm ID 1
        public int MaKhachHang { get; set; } = 1;
        public string DiaChiGiaoHang { get; set; }
        public string GhiChu { get; set; }
        public decimal TongTien { get; set; }
        public List<ChiTietHoaDonDto> ChiTietDonHang { get; set; }
    }
}
