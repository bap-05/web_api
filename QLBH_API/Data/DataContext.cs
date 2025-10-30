using Microsoft.EntityFrameworkCore;
using QLBH_API.Models;
using QLBH_API.Models.QLBH_API.Models;
using System.IO;

namespace QLBH_API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        // Khai báo các bảng mà EF Core sẽ quản lý schema
        public DbSet<tbDanhMuc> tbDANHMUC { get; set; }
        public DbSet<tbSanPham> tbSANPHAM { get; set; }
        public DbSet<tbGioHang> tbGIOHANG { get; set; }
        public DbSet<tbKhachHang> tbKHACHHANG { get; set; }
        public DbSet<tbNhanVien> tbNHANVIEN { get; set; }
        public DbSet<tbHoaDon> tbHOADON { get; set; }
        public DbSet<tbChiTietHoaDon> tbCHITIETHOADON { get; set; }
        public DbSet<CartItemDetail> CartItemDetails { get; set; }

        // THÊM PHƯƠNG THỨC NÀY
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Báo cho EF Core biết CartItemDetail không phải là một bảng
            modelBuilder.Entity<CartItemDetail>().HasNoKey();
        }
    }
}
