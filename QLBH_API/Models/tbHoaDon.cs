using QLBH_API.Models.QLBH_API.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_API.Models
{
    [Table("tbHOADON")]
    public class tbHoaDon
    {
        [Key]
        public int MAHOADON { get; set; }

        public decimal? MAKHACHHANG { get; set; }
        public decimal? MANHANVIEN { get; set; }
        public DateTime? NGAYLAP { get; set; }
        public decimal? TONGTIEN { get; set; }

        [StringLength(255)]
        public string GHICHU { get; set; }

        [StringLength(255)]
        public string DIACHIGIAOHANG { get; set; }

        public int? TRANGTHAI { get; set; }

        // Khóa ngoại tới Khách hàng
        [ForeignKey("MAKHACHHANG")]
        public virtual tbKhachHang tbKHACHHANG { get; set; }

        // Khóa ngoại tới Nhân viên
        [ForeignKey("MANHANVIEN")]
        public virtual tbNhanVien tbNHANVIEN { get; set; }

        // Quan hệ: Một hóa đơn có nhiều chi tiết
        public virtual ICollection<tbChiTietHoaDon> tbCHITIETHOADON { get; set; }

        public tbHoaDon()
        {
            tbCHITIETHOADON = new HashSet<tbChiTietHoaDon>();
        }
    }
}
