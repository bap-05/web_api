using QLBH_API.Models.QLBH_API.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLBH_API.Models
{
    [Table("tbHOADON")]
    public class tbHoaDon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MAHOADON { get; set; }
        public int MAKHACHHANG { get; set; }
        public DateTime NGAY { get; set; }
        public decimal TONGTIEN { get; set; }
    }
}
