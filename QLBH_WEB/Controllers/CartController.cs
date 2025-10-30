using QLBH_WEB.Models; // Đảm bảo namespace này là đúng
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace QLBH_WEB.Controllers
{
    public class CartController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private const string CartSession = "CartSession"; // Tên của key lưu trong Session

        private HttpClient GetHttpClient()
        {
            var client = new HttpClient { BaseAddress = new System.Uri(_apiBaseUrl) };
            return client;
        }

        // GET: /Cart/Index
        // Hiển thị trang giỏ hàng từ dữ liệu trong Session
        public ActionResult Index()
        {
            var cart = Session[CartSession] as Cart;
            if (cart == null)
            {
                cart = new Cart(); // Nếu chưa có giỏ hàng, tạo mới
            }
            return View(cart);
        }

        // POST: /Cart/AddToCart
        // Thêm sản phẩm vào giỏ hàng và lưu vào Session
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddToCart(int id, int soLuong = 1)
        {
            SanPhamViewModel sanPham;

            // Luôn lấy thông tin sản phẩm mới nhất từ API để đảm bảo đúng giá
            using (var client = GetHttpClient())
            {
                HttpResponseMessage result = await client.GetAsync($"api/SanPham/{id}");
                if (result.IsSuccessStatusCode)
                {
                    sanPham = await result.Content.ReadAsAsync<SanPhamViewModel>();
                }
                else
                {
                    TempData["CartError"] = "Không tìm thấy sản phẩm để thêm vào giỏ.";
                    return Redirect(Request.UrlReferrer.ToString());
                }
            }

           
            var cart = Session[CartSession] as Cart ?? new Cart();

            // thêm sản phẩm vào đối tượng giỏ hàng trong bộ nhớ
            cart.AddItem(sanPham, soLuong);
            Session[CartSession] = cart;
            return RedirectToAction("Index");
        }

        // POST: /Cart/UpdateCart
        // Cập nhật số lượng các sản phẩm trong giỏ hàng trong Session
        [HttpPost]
        public ActionResult UpdateCart(FormCollection form)
        {
            var cart = Session[CartSession] as Cart;
            if (cart != null)
            {
                foreach (var key in form.AllKeys)
                {
                    if (key.StartsWith("quantity_"))
                    {
                        int maSP = int.Parse(key.Substring(9));
                        int soLuong = int.Parse(form[key]);
                        cart.UpdateItem(maSP, soLuong);
                    }
                }
                Session[CartSession] = cart; // Lưu lại thay đổi vào Session
            }
            return RedirectToAction("Index");
        }

        // GET: /Cart/RemoveFromCart/5
        // Xóa một sản phẩm khỏi giỏ hàng trong Session
        public ActionResult RemoveFromCart(int id)
        {
            var cart = Session[CartSession] as Cart;
            if (cart != null)
            {
                cart.RemoveItem(id);
                Session[CartSession] = cart; // Lưu lại thay đổi vào Session
            }
            return RedirectToAction("Index");
        }

        // Child Action để hiển thị widget giỏ hàng trên mọi trang
        [ChildActionOnly]
        public ActionResult ShoppingCartWidget()
        {
            var cart = Session[CartSession] as Cart ?? new Cart();
            return PartialView("_ShoppingCartWidget", cart);
        }
        public ActionResult Checkout()
        {
            Cart cart = (Cart)Session["Cart"];
            if (cart == null || cart.Items.Count == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart
            };

            return View(model); // Trả về View Checkout.cshtml
        }

        // POST: /Cart/SubmitCheckout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitCheckout(CheckoutViewModel model)
        {
            Cart cart = (Cart)Session["Cart"];
            if (cart == null)
            {
                // Xử lý lỗi giỏ hàng trống
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                model.Cart = cart;
                return View("Checkout", model); // Trả lại trang checkout nếu nhập liệu lỗi
            }

            // 1. Chuẩn bị dữ liệu DTO để gửi
            var dto = new HoaDonCreateDto
            {
                // Cần có logic lấy MaKhachHang (ví dụ: sau khi đăng nhập)
                // Tạm thời hardcode
                MaKhachHang = 1,
                DiaChiGiaoHang = model.DiaChiGiaoHang,
                GhiChu = model.GhiChu,
                TongTien = cart.GetTotal(),
                ChiTietDonHang = new List<ChiTietHoaDonDto>()
            };

            foreach (var item in cart.Items)
            {
                dto.ChiTietDonHang.Add(new ChiTietHoaDonDto
                {
                    MaSanPham = item.Value.SanPham.MASANPHAM, // <-- Sửa lại
                    SoLuong = item.Value.SoLuong,     // <-- Sửa lại
                    DonGia = item.Value.SanPham.DONGIA ?? 0
                });
            }

            // 2. Gọi API để tạo hóa đơn
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.PostAsJsonAsync("api/hoadon/create", dto);

                    if (response.IsSuccessStatusCode)
                    {
                        // Xóa giỏ hàng khỏi session
                        Session["Cart"] = null;

                        // Chuyển đến trang cảm ơn
                        return RedirectToAction("OrderSuccess");
                    }
                    else
                    {
                        // Xử lý lỗi từ API
                        ModelState.AddModelError("", "Không thể tạo đơn hàng. Vui lòng thử lại. " + response.ReasonPhrase);
                        model.Cart = cart;
                        return View("Checkout", model);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi: " + ex.Message);
                model.Cart = cart;
                return View("Checkout", model);
            }
        }

        // GET: /Cart/OrderSuccess
        public ActionResult OrderSuccess()
        {
            return Content("Chúc mừng! Bạn đã đặt hàng thành công.");
        }
    }
}