/*
 * Student Name: Sarah Shields
 * Student Number: 301264350
 * Submission Date: October 30th, 2024
 */

using _301264350_shields__Lab3.Data;
using _301264350_shields__Lab3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace _301264350_shields__Lab3.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: registration form
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        // POST: registration action
        [HttpPost]
        public async Task<IActionResult> Register(User user)
        {
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("", "Username already exists");
                return View(user); // return to registration page with error message
            }

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }
            return View(user);
        }

        // POST: login action
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()) // Add the UserId as a claim
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                return RedirectToAction("Dashboard", "Movie");
            }
            ModelState.AddModelError("", "Invalid username or password");
            return View("Index");  // login is on the index page
        }

        // POST: logout action
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
