
    // Trong QLBH_API/Models/tbNHANVIEN.cs
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace QLBH_API.Models
    {
        [Table("tbNHANVIEN")]
        public class tbNhanVien
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public decimal MANHANVIEN { get; set; }

            [StringLength(100)]
            public string HOTEN { get; set; }

            [StringLength(5)]
            public string GIOITINH { get; set; }

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

            public decimal? ID_PERMISSION { get; set; }

            // Quan hệ: Một nhân viên lập nhiều hóa đơn
            public virtual ICollection<tbHoaDon> tbHOADON { get; set; }

            public tbNhanVien()
            {
                tbHOADON = new HashSet<tbHoaDon>();
            }
        }
    }

