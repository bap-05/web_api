using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLBH_WEB.Models
{
    public class HoaDonCreateDto
    {
        public int MaKhachHang { get; set; }
        public decimal TongTien { get; set; }
        public List<ChiTietHoaDonDto> ChiTietHoaDon { get; set; }
    }

    public class ChiTietHoaDonDto
    {
        public int MaSanPham { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }

}