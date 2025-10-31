using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLBH_API.Data;
using QLBH_API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class HoaDonController : ControllerBase
{
    private readonly DataContext _context;

    public HoaDonController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateHoaDon([FromBody] HoaDonCreateDto dto)
    {
        // ✅ Kiểm tra dữ liệu đầu vào
        if (dto == null || dto.ChiTietHoaDon == null || !dto.ChiTietHoaDon.Any())
        {
            return BadRequest("Thông tin đơn hàng không hợp lệ hoặc rỗng.");
        }

        // ✅ Dùng transaction đảm bảo toàn vẹn dữ liệu
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // 🧾 1️⃣ Tạo hóa đơn chính
                var hoaDon = new tbHoaDon
                {
                    MAKHACHHANG = dto.MaKhachHang,
                    NGAY = DateTime.Now,
                    TONGTIEN = dto.TongTien
                };

                await _context.tbHOADON.AddAsync(hoaDon);
                await _context.SaveChangesAsync(); // để sinh MAHOADON tự động (IDENTITY)

                // 🧾 2️⃣ Thêm chi tiết hóa đơn
                foreach (var item in dto.ChiTietHoaDon)
                {
                    var chiTiet = new tbChiTietHoaDon
                    {
                        MAHOADON = hoaDon.MAHOADON,
                        MASANPHAM = item.MaSanPham,
                        SOLUONG = item.SoLuong,
                        DONGIA = item.DonGia
                    };

                    await _context.tbCHITIETHOADON.AddAsync(chiTiet);
                }

                await _context.SaveChangesAsync();

                // 🛒 3️⃣ Xóa giỏ hàng của khách (nếu có)
                var cartItems = await _context.tbGIOHANG
                    .Where(g => g.MAKHACHHANG == dto.MaKhachHang)
                    .ToListAsync();

                if (cartItems.Any())
                {
                    _context.tbGIOHANG.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();
                }

                // ✅ Commit giao dịch
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Đặt hàng thành công!",
                    maHoaDon = hoaDon.MAHOADON
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new
                {
                    error = "Lỗi máy chủ nội bộ.",
                    detail = ex.Message
                });
            }
        }
    }
}
