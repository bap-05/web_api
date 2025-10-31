using QLBH_WEB.Models;
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
        private const string CartSession = "CartSession";
        private const string CheckoutCartSession = "CheckoutCartSession";

        private HttpClient GetHttpClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(_apiBaseUrl) };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        // -------------------------------
        // 🛒 HIỂN THỊ GIỎ HÀNG
        // -------------------------------
        public ActionResult Index()
        {
            var cart = Session[CartSession] as Cart ?? new Cart();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View(cart);
        }

        // -------------------------------
        // ➕ THÊM SẢN PHẨM VÀO GIỎ
        // -------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddToCart(int id, int soLuong = 1)
        {
            SanPhamViewModel sanPham;

            using (var client = GetHttpClient())
            {
                HttpResponseMessage result = await client.GetAsync($"api/SanPham/{id}");
                if (!result.IsSuccessStatusCode)
                {
                    TempData["CartError"] = "Không tìm thấy sản phẩm để thêm vào giỏ.";
                    return RedirectToAction("Index");
                }

                sanPham = await result.Content.ReadAsAsync<SanPhamViewModel>();
            }

            var cart = Session[CartSession] as Cart ?? new Cart();
            cart.AddItem(sanPham, soLuong);
            Session[CartSession] = cart;

            TempData["CartSuccess"] = $"Đã thêm {sanPham.TENSANPHAM} vào giỏ hàng!";
            return RedirectToAction("Index");
        }

        // -------------------------------
        // 🔁 CẬP NHẬT GIỎ HÀNG
        // -------------------------------
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
                Session[CartSession] = cart;
                TempData["CartSuccess"] = "Đã cập nhật giỏ hàng.";
            }
            return RedirectToAction("Index");
        }

        // -------------------------------
        // ❌ XÓA SẢN PHẨM KHỎI GIỎ
        // -------------------------------
        public ActionResult RemoveFromCart(int id)
        {
            var cart = Session[CartSession] as Cart;
            cart?.RemoveItem(id);
            Session[CartSession] = cart;
            return RedirectToAction("Index");
        }

        // -------------------------------
        // 🧾 CHUẨN BỊ THANH TOÁN
        // -------------------------------
        [HttpPost]
        public ActionResult PrepareCheckout(int[] selectedItems)
        {
            var cart = Session[CartSession] as Cart;
            if (cart == null || selectedItems == null || selectedItems.Length == 0)
            {
                TempData["CartError"] = "Vui lòng chọn sản phẩm để thanh toán.";
                return RedirectToAction("Index");
            }

            var checkoutCart = new Cart();
            foreach (var id in selectedItems)
            {
                if (cart.Items.ContainsKey(id))
                {
                    var item = cart.Items[id];
                    checkoutCart.AddItem(item.SanPham, item.SoLuong);
                }
            }

            Session[CheckoutCartSession] = checkoutCart;
            return RedirectToAction("Checkout");
        }

        // -------------------------------
        // 💳 TRANG THANH TOÁN
        // -------------------------------
        public ActionResult Checkout()
        {
            if (Session["MaKhachHang"] == null)
                Session["MaKhachHang"] = 1; // test tạm

            var cart = Session[CheckoutCartSession] as Cart;
            if (cart == null || cart.Items.Count == 0)
            {
                TempData["CartError"] = "Không có sản phẩm nào để thanh toán.";
                return RedirectToAction("Index");
            }

            var model = new CheckoutViewModel { Cart = cart };
            return View(model);
        }

        // -------------------------------
        // ✅ GỬI ĐƠN HÀNG (SUBMIT)
        // -------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitCheckout(CheckoutViewModel model)
        {
            if (Session["MaKhachHang"] == null)
                Session["MaKhachHang"] = 1; // test tạm

            int maKhachHang = Convert.ToInt32(Session["MaKhachHang"]);
            var cart = Session[CheckoutCartSession] as Cart;

            if (cart == null || cart.Items.Count == 0)
            {
                TempData["CartError"] = "Giỏ hàng trống.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                model.Cart = cart;
                return View("Checkout", model);
            }

            // Chuẩn bị dữ liệu gửi API
            var hoaDonDto = new HoaDonCreateDto
            {
                MaKhachHang = maKhachHang,
            
                TongTien = cart.GetTotal(),
                ChiTietHoaDon = cart.Items.Values.Select(item => new ChiTietHoaDonDto
                {
                    MaSanPham = item.SanPham.MASANPHAM,
                    SoLuong = item.SoLuong,
                    DonGia = item.SanPham.DONGIA ?? 0
                }).ToList()
            };

            
                using (var client = GetHttpClient())
                {
                    var response = await client.PostAsJsonAsync("api/HoaDon/Create", hoaDonDto);

                    if (response.IsSuccessStatusCode)
                    {
                        // Xóa các sản phẩm vừa thanh toán khỏi giỏ chính
                        var fullCart = Session[CartSession] as Cart;
                        foreach (var key in cart.Items.Keys)
                            fullCart?.RemoveItem(key);

                        Session[CartSession] = fullCart;
                        Session[CheckoutCartSession] = null;

                        TempData["SuccessMessage"] = "Đặt hàng thành công!";
                        return RedirectToAction("Index", "Cart");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Không thể tạo đơn hàng: {response.ReasonPhrase}");
                    }
                }
           
            model.Cart = cart;
            return View("Checkout", model);
        }

        // -------------------------------
        // 🎉 TRANG SAU KHI ĐẶT HÀNG THÀNH CÔNG
        // -------------------------------
        public ActionResult OrderSuccess()
        {
            return View();
        }

        // -------------------------------
        // 🧩 HIỂN THỊ MINI-CART (nếu cần)
        // -------------------------------
        [ChildActionOnly]
        public ActionResult ShoppingCartWidget()
        {
            var cart = Session[CartSession] as Cart ?? new Cart();
            return PartialView("_ShoppingCartWidget", cart);
        }
    }
}
