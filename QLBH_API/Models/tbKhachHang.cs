namespace QLBH_API.Models
{
    // Trong QLBH_API/Models/tbKHACHHANG.cs
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace QLBH_API.Models
    {
        [Table("tbKHACHHANG")]
        public class tbKhachHang
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public decimal MAKHACHHANG { get; set; }

            [StringLength(100)]
            public string TENKHACHHANG { get; set; }

            [StringLength(255)]
            public string DIACHI { get; set; }

            [StringLength(15)]
            public string DIENTHOAI { get; set; }

            [StringLength(100)]
            public string EMAIL { get; set; }

            [StringLength(50)]
            public string TENDANGNHAP { get; set; }

            [StringLength(255)]
            public string MATKHAU { get; set; }

            // Quan hệ: Một khách hàng có nhiều hóa đơn
            public virtual ICollection<tbHoaDon> tbHOADON { get; set; }

            public tbKhachHang()
            {
                tbHOADON = new HashSet<tbHoaDon>();
            }
        }
    }
}
