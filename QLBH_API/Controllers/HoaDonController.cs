// Trong QLBH_API/Controllers/HoaDonController.cs
using Microsoft.AspNetCore.Mvc;
using QLBH_API.Data;
using QLBH_API.Models; // Nơi chứa tbHOADON, tbCHITIETHOADON
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

    [HttpPost("create")]
    public async Task<IActionResult> CreateHoaDon([FromBody] HoaDonCreateDto dto)
    {
        if (dto == null || !dto.ChiTietDonHang.Any())
        {
            return BadRequest("Thông tin đơn hàng không hợp lệ.");
        }

        // Sử dụng Transaction để đảm bảo tính toàn vẹn dữ liệu
        // Hoặc cả 2 bảng cùng lưu, hoặc không bảng nào được lưu
        using (var transaction = await _context.Database.BeginTransactionAsync())
        {
            try
            {
                // 1. TẠO HÓA ĐƠN CHÍNH
                var hoaDon = new tbHoaDon
                {
                    MAKHACHHANG = dto.MaKhachHang, // Cần xử lý logic lấy MaKhachHang (ví dụ: từ session/login)
                    NGAYLAP = DateTime.Now,
                    TONGTIEN = dto.TongTien,
                    DIACHIGIAOHANG = dto.DiaChiGiaoHang,
                    GHICHU = dto.GhiChu,
                    TRANGTHAI = 1 // 1 = Mới đặt
                };

                _context.tbHOADON.Add(hoaDon);
                await _context.SaveChangesAsync(); // Lưu để lấy được MAHOADON (Identity)

                // 2. TẠO CÁC CHI TIẾT HÓA ĐƠN
                foreach (var item in dto.ChiTietDonHang)
                {
                    var chiTiet = new tbChiTietHoaDon
                    {
                        MAHOADON = hoaDon.MAHOADON, // Lấy ID vừa tạo
                        MASANPHAM = item.MaSanPham,
                        SOLUONG = item.SoLuong,
                        DONGIA = item.DonGia
                    };
                    _context.tbCHITIETHOADON.Add(chiTiet);
                }

                await _context.SaveChangesAsync(); // Lưu chi tiết

                // 3. (Optional) Xóa giỏ hàng của khách
                var cartItems = _context.tbGIOHANG.Where(g => g.MAKHACHHANG == dto.MaKhachHang);
                _context.tbGIOHANG.RemoveRange(cartItems);
                await _context.SaveChangesAsync();


                await transaction.CommitAsync(); // Hoàn tất giao dịch

                return Ok(new { message = "Đặt hàng thành công!", maHoaDon = hoaDon.MAHOADON });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Hoàn tác nếu có lỗi
                return StatusCode(500, $"Lỗi máy chủ nội bộ: {ex.Message}");
            }
        }
    }
}