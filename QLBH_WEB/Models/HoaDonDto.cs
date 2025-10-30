using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLBH_WEB.Models
{
    public class ChiTietHoaDonDto
    {
        public int MaSanPham { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }

    public class HoaDonCreateDto
    {
        public int MaKhachHang { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public string GhiChu { get; set; }
        public decimal TongTien { get; set; }
        public List<ChiTietHoaDonDto> ChiTietDonHang { get; set; }
    }
}