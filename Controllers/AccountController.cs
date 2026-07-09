using LibraryHub.Data;
using Microsoft.AspNetCore.Mvc;

namespace LibraryHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly LibraryContext _context;

        public AccountController(LibraryContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var admin = _context.Admins
                .FirstOrDefault(a => a.Username == username && a.Password == password);

            if (admin != null)
            {
                HttpContext.Session.SetString("AdminUser", admin.Username);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Kullanıcı adı veya şifre hatalı.";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}