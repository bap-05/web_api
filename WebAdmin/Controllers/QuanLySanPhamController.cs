using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebAdmin.Models;

namespace WEB_APP.Controllers
{
    public class QuanLySanPhamController : Controller
    {
        private readonly string _apiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
        private HttpClient GetHttpClient()
        {
            var client = new HttpClient();
            client.BaseAddress = new System.Uri(_apiBaseUrl);
            return client;
        }

        // GET: Hiển thị danh sách sản phẩm
        public async Task<ActionResult> Index()
        {
            IEnumerable<SanPhamViewModel> sanPhamList;
            using (var client = GetHttpClient())
            {
                HttpResponseMessage result = await client.GetAsync("api/SanPham");
                if (result.IsSuccessStatusCode)
                {
                    sanPhamList = await result.Content.ReadAsAsync<IEnumerable<SanPhamViewModel>>();
                }
                else
                {
                    sanPhamList = new List<SanPhamViewModel>();
                    ModelState.AddModelError(string.Empty, "Lỗi khi lấy danh sách sản phẩm từ API.");
                }
            }
            return View(sanPhamList);
        }

        // GET: Hiển thị chi tiết sản phẩm
        public async Task<ActionResult> Details(int id)
        {
            SanPhamViewModel sanPham;
            using (var client = GetHttpClient())
            {
                HttpResponseMessage result = await client.GetAsync($"api/SanPham/{id}");
                if (result.IsSuccessStatusCode)
                {
                    sanPham = await result.Content.ReadAsAsync<SanPhamViewModel>();
                }
                else
                {
                    return HttpNotFound();
                }
            }
            return View(sanPham);
        }

        // GET: Hiển thị form tạo mới VỚI DANH SÁCH DANH MỤC
        public async Task<ActionResult> Create()
        {
            // Gọi API để lấy danh sách danh mục
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("api/DanhMuc");
                if (response.IsSuccessStatusCode)
                {
                    var danhMucs = await response.Content.ReadAsAsync<IEnumerable<DanhMucViewModel>>();
                    // Gửi danh sách qua ViewBag để View có thể tạo DropDownList
                    ViewBag.DanhMucList = new SelectList(danhMucs, "MADANHMUC", "TENDANHMUC");
                }
                else
                {
                    // Nếu lỗi, vẫn tạo một SelectList rỗng để View không bị crash
                    ViewBag.DanhMucList = new SelectList(new List<DanhMucViewModel>(), "MADANHMUC", "TENDANHMUC");
                    ModelState.AddModelError(string.Empty, "Không thể tải danh sách danh mục từ API.");
                }
            }
            return View();
        }

        // POST: Xử lý tạo mới sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(SanPhamViewModel sanPham, HttpPostedFileBase hinhAnhFile)
        {
            // Nếu model hợp lệ (không có lỗi validation)
            if (ModelState.IsValid)
            {
                // Xử lý lưu file ảnh
                if (hinhAnhFile != null && hinhAnhFile.ContentLength > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnhFile.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    hinhAnhFile.SaveAs(path);
                    sanPham.HINHANH = fileName;
                }

                // Gọi API để tạo mới
                using (var client = GetHttpClient())
                {
                    HttpResponseMessage result = await client.PostAsJsonAsync("api/SanPham", sanPham);
                    if (result.IsSuccessStatusCode)
                    {
                        TempData["SuccessMessage"] = "Đã thêm sản phẩm thành công!";
                        return RedirectToAction("Index");
                    }
                }
                ModelState.AddModelError(string.Empty, "Lỗi khi tạo sản phẩm qua API.");
            }

            // --- PHẦN SỬA LỖI QUAN TRỌNG NHẤT NẰM Ở ĐÂY ---
            // Nếu ModelState không hợp lệ, chúng ta phải tải lại danh sách danh mục
            // trước khi trả về View để người dùng có thể chọn lại.
            using (var client = GetHttpClient())
            {
                HttpResponseMessage response = await client.GetAsync("api/DanhMuc");
                if (response.IsSuccessStatusCode)
                {
                    var danhMucs = await response.Content.ReadAsAsync<IEnumerable<DanhMucViewModel>>();
                    // Tạo lại SelectList, giữ lại giá trị danh mục người dùng đã chọn trước đó
                    ViewBag.DanhMucList = new SelectList(danhMucs, "MADANHMUC", "TENDANHMUC", sanPham.MADANHMUC);
                }
                else
                {
                    ViewBag.DanhMucList = new SelectList(new List<DanhMucViewModel>(), "MADANHMUC", "TENDANHMUC");
                }
            }

            // Trả về view với model hiện tại để người dùng sửa lỗi
            return View(sanPham);
        }



        // GET: Hiển thị form chỉnh sửa
        public async Task<ActionResult> Edit(int id)
        {
            SanPhamViewModel sanPham;
            using (var client = GetHttpClient())
            {
                HttpResponseMessage result = await client.GetAsync($"api/SanPham/{id}");
                if (result.IsSuccessStatusCode)
                {
                    sanPham = await result.Content.ReadAsAsync<SanPhamViewModel>();
                }
                else
                {
                    return HttpNotFound();
                }
            }
            return View(sanPham);
        }

        //PUT: Xử lý chỉnh sửa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, SanPhamViewModel sanPham, HttpPostedFileBase hinhAnhFile)
        {
            if (ModelState.IsValid)
            {
                // BƯỚC 1: XỬ LÝ LƯU FILE ẢNH (NẾU CÓ)
                if (hinhAnhFile != null && hinhAnhFile.ContentLength > 0)
                {
                    // Tạo tên file duy nhất để tránh trùng lặp
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(hinhAnhFile.FileName);

                    // Lấy đường dẫn vật lý để lưu file vào thư mục Content/images của WEB_APP
                    var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);

                    // Lưu file
                    hinhAnhFile.SaveAs(path);

                    // Cập nhật lại tên ảnh mới vào model `sanPham` để chuẩn bị gửi đi
                    sanPham.HINHANH = fileName;
                }
                // Nếu không có file mới, `sanPham.HINHANH` sẽ giữ nguyên giá trị cũ từ HiddenFor trong View.

                // BƯỚC 2: GỬI DỮ LIỆU ĐÃ CẬP NHẬT LÊN API
                using (var client = GetHttpClient())
                {
                    // Gửi đối tượng sanPham (đã có tên ảnh mới nếu có) lên API dưới dạng JSON
                    HttpResponseMessage result = await client.PutAsJsonAsync($"api/SanPham/{id}", sanPham);

                    if (result.IsSuccessStatusCode)
                    {
                        return RedirectToAction("Index");
                    }
                }
                ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật sản phẩm qua API.");
            }
            return View(sanPham);
        }

        // POST: Xử lý xóa sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            using (var client = GetHttpClient())
            {
                HttpResponseMessage result = await client.DeleteAsync($"api/SanPham/{id}");
                if (!result.IsSuccessStatusCode)
                {
                    // Thêm thông báo lỗi nếu muốn
                }
            }
            return RedirectToAction("Index");
        }
    }
}