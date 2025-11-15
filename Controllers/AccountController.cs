using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Models;
using PROG6212_POE.Services;
using System.Diagnostics;

namespace PROG6212_POE.Controllers
{
    public class AccountController : Controller
    {
        // Static method to access users from other controllers
        public static List<User> GetUsers()
        {
            return DataService.GetUsers();
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            // If user is already logged in, redirect to their dashboard
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToDashboard();
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    ModelState.AddModelError("", "Please enter both email and password.");
                    return View();
                }

                var users = DataService.GetUsers();
                var user = users.FirstOrDefault(u => u.Email == email && u.Password == password);

                if (user != null)
                {
                    // Store user info in session
                    HttpContext.Session.SetString("UserId", user.UserId.ToString());
                    HttpContext.Session.SetString("UserRole", user.Role);
                    HttpContext.Session.SetString("UserName", user.FullName);

                    TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";
                    return RedirectToDashboard();
                }

                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Login error: {ex.Message}");
                ModelState.AddModelError("", "An error occurred during login. Please try again.");
                return View();
            }
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        // Helper method to redirect to appropriate dashboard
        private IActionResult RedirectToDashboard()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            return userRole switch
            {
                "HR" => RedirectToAction("Dashboard", "HR"),
                "Lecturer" => RedirectToAction("Dashboard", "Lecturer"),
                "Coordinator" => RedirectToAction("Dashboard", "Coordinator"),
                "Manager" => RedirectToAction("Dashboard", "Manager"),
                _ => RedirectToAction("Login")
            };
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}