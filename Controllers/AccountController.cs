using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Models;
using System.Diagnostics;

namespace PROG6212_POE.Controllers
{
    public class AccountController : Controller
    {
        // Static list to simulate database (replace with DbContext later)
        private static List<User> _users = new List<User>();
        private static int _nextUserId = 1;

        // Static method to access users from other controllers
        public static List<User> GetUsers()
        {
            return _users;
        }

        // Initialize with some default users for testing
        static AccountController()
        {
            _users.Add(new User
            {
                UserId = _nextUserId++,
                Name = "HR",
                Surname = "Manager",
                Email = "hr@university.com",
                HourlyRate = 0,
                Role = "HR",
                Password = "password123"
            });

            _users.Add(new User
            {
                UserId = _nextUserId++,
                Name = "John",
                Surname = "Lecturer",
                Email = "lecturer@university.com",
                HourlyRate = 350,
                Role = "Lecturer",
                Password = "password123"
            });

            _users.Add(new User
            {
                UserId = _nextUserId++,
                Name = "Sarah",
                Surname = "Coordinator",
                Email = "coordinator@university.com",
                HourlyRate = 0,
                Role = "Coordinator",
                Password = "password123"
            });

            _users.Add(new User
            {
                UserId = _nextUserId++,
                Name = "Michael",
                Surname = "Manager",
                Email = "manager@university.com",
                HourlyRate = 0,
                Role = "Manager",
                Password = "password123"
            });
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

                var user = _users.FirstOrDefault(u => u.Email == email && u.Password == password);

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