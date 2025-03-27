using CinemaManagement.Models;
using CinemaManagement.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CinemaManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly CinemaManagementContext _context;

        public AccountController(CinemaManagementContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu";
                return View();
            }

            var user = await _context.Customers
                .FirstOrDefaultAsync(c => c.Username == username && c.PasswordHash == password);

            if (user == null)
            {
                ViewBag.Error = "Thông tin đăng nhập không chính xác";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("FullName", user.FullName),
                new Claim("CustomerId", user.CustomerId.ToString())
            };

            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(12)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Mật khẩu không khớp");
                    return View(model);
                }

                var existingUser = await _context.Customers
                    .AnyAsync(c => c.Username == model.Username || c.Email == model.Email);

                if (existingUser)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc email đã tồn tại");
                    return View(model);
                }

                // Create a new Customer from the RegisterViewModel
                var customer = new Customer
                {
                    Username = model.Username,
                    PasswordHash = model.Password, // Consider hashing the password
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    IsAdmin = false
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                // Đăng nhập tự động sau khi đăng ký
                await Login(customer.Username, customer.PasswordHash);

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }
    }
}
