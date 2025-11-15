using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Models;
using PROG6212_POE.Services;
using System.Diagnostics;

namespace PROG6212_POE.Controllers
{
    public class HRController : Controller
    {
        // Helper method to get current user
        private User GetCurrentUser()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return null;

            var users = DataService.GetUsers();
            return users.FirstOrDefault(u => u.UserId.ToString() == userId);
        }

        // GET: /HR/Dashboard
        public IActionResult Dashboard()
        {
            // Authorization check
            if (HttpContext.Session.GetString("UserRole") != "HR")
                return RedirectToAction("AccessDenied", "Account");

            var currentUser = GetCurrentUser();
            var users = DataService.GetUsers();
            var claims = DataService.GetClaims();

            ViewBag.CurrentUser = currentUser;
            ViewBag.UserCount = users.Count;
            ViewBag.LecturerCount = users.Count(u => u.Role == "Lecturer");
            ViewBag.TotalClaims = claims.Count;

            return View(users);
        }

        // GET: /HR/CreateUser
        public IActionResult CreateUser()
        {
            if (HttpContext.Session.GetString("UserRole") != "HR")
                return RedirectToAction("AccessDenied", "Account");

            var roles = new List<string> { "HR", "Lecturer", "Coordinator", "Manager" };
            ViewBag.Roles = roles;
            return View();
        }

        // POST: /HR/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(User user)
        {
            if (HttpContext.Session.GetString("UserRole") != "HR")
                return RedirectToAction("AccessDenied", "Account");

            var users = DataService.GetUsers();

            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (users.Any(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    ViewBag.Roles = new List<string> { "HR", "Lecturer", "Coordinator", "Manager" };
                    return View(user);
                }

                // Add user through DataService
                DataService.AddUser(user);

                TempData["SuccessMessage"] = $"{user.DisplayRole} {user.FullName} created successfully!";
                return RedirectToAction("Dashboard");
            }

            ViewBag.Roles = new List<string> { "HR", "Lecturer", "Coordinator", "Manager" };
            return View(user);
        }

        // GET: /HR/EditUser/{id}
        public IActionResult EditUser(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "HR")
                return RedirectToAction("AccessDenied", "Account");

            var user = DataService.GetUserById(id);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.Roles = new List<string> { "HR", "Lecturer", "Coordinator", "Manager" };
            return View(user);
        }

        // POST: /HR/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditUser(User updatedUser)
        {
            if (HttpContext.Session.GetString("UserRole") != "HR")
                return RedirectToAction("AccessDenied", "Account");

            var users = DataService.GetUsers();

            if (ModelState.IsValid)
            {
                var existingUser = users.FirstOrDefault(u => u.UserId == updatedUser.UserId);
                if (existingUser != null)
                {
                    // Check if email is taken by another user
                    if (users.Any(u => u.UserId != updatedUser.UserId && u.Email == updatedUser.Email))
                    {
                        ModelState.AddModelError("Email", "This email is already registered to another user.");
                        ViewBag.Roles = new List<string> { "HR", "Lecturer", "Coordinator", "Manager" };
                        return View(updatedUser);
                    }

                    // Update user properties
                    existingUser.Name = updatedUser.Name;
                    existingUser.Surname = updatedUser.Surname;
                    existingUser.Email = updatedUser.Email;
                    existingUser.HourlyRate = updatedUser.HourlyRate;
                    existingUser.Role = updatedUser.Role;
                    existingUser.Password = updatedUser.Password;

                    TempData["SuccessMessage"] = "User updated successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "User not found.";
                }
                return RedirectToAction("Dashboard");
            }

            ViewBag.Roles = new List<string> { "HR", "Lecturer", "Coordinator", "Manager" };
            return View(updatedUser);
        }

        // GET: /HR/GenerateReports
        public IActionResult GenerateReports()
        {
            if (HttpContext.Session.GetString("UserRole") != "HR")
                return RedirectToAction("AccessDenied", "Account");

            var claims = DataService.GetClaims();

            // LINQ queries for reports
            var approvedClaims = claims.Where(c => c.Status == "Approved").ToList();
            var monthlyReport = approvedClaims
                .GroupBy(c => c.Month)
                .Select(g => new {
                    Month = g.Key,
                    TotalAmount = g.Sum(c => c.TotalAmount),
                    ClaimCount = g.Count()
                })
                .ToList();

            ViewBag.MonthlyReport = monthlyReport;
            ViewBag.TotalApprovedAmount = approvedClaims.Sum(c => c.TotalAmount);
            ViewBag.TotalApprovedClaims = approvedClaims.Count;

            return View(approvedClaims);
        }

        // GET: /HR/ViewAllClaims
        public IActionResult ViewAllClaims()
        {
            if (HttpContext.Session.GetString("UserRole") != "HR")
                return RedirectToAction("AccessDenied", "Account");

            var claims = DataService.GetClaims();
            return View(claims);
        }

        // Note: Removed the static helper methods since we're using DataService directly
    }
}