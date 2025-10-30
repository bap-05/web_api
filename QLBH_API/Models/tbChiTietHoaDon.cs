// Trong QLBH_API/Models/tbCHITIETHOADON.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_API.Models
{
    [Table("tbCHITIETHOADON")]
    public class tbChiTietHoaDon
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MAHOADON { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MASANPHAM { get; set; }

        public int? SOLUONG { get; set; }
        public decimal? DONGIA { get; set; }

        // Khóa ngoại tới Hóa đơn
        [ForeignKey("MAHOADON")]
        public virtual tbHoaDon tbHOADON { get; set; }

        // Khóa ngoại tới Sản phẩm (giả sử bạn đã có lớp tbSanPham.cs)
        [ForeignKey("MASANPHAM")]
        public virtual tbSanPham tbSanPham { get; set; }
    }
}