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

            var currentUser = GetCurrentUser();
            ViewBag.CurrentUser = currentUser;

            // Get all claims for comprehensive reporting
            var allClaims = DataService.GetClaims();

            // Calculate comprehensive statistics
            ViewBag.TotalClaims = allClaims.Count;
            ViewBag.TotalApprovedAmount = allClaims.Where(c => c.Status == "Approved").Sum(c => c.TotalAmount);
            ViewBag.TotalApprovedClaims = allClaims.Count(c => c.Status == "Approved");
            ViewBag.PendingClaims = allClaims.Count(c => c.Status == "Pending Verification" || c.Status == "Verified");

            // Claims by Status data - FIXED: Using anonymous types with all required properties
            var claimsByStatus = allClaims
                .GroupBy(c => c.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(c => c.TotalAmount),
                    AverageAmount = g.Count() > 0 ? g.Average(c => c.TotalAmount) : 0,
                    Percentage = (double)g.Count() / allClaims.Count * 100
                })
                .ToList();

            ViewBag.ClaimsByStatus = claimsByStatus;

            // Monthly Summary data - FIXED: Using anonymous types with all required properties
            var monthlyGroups = allClaims
                .GroupBy(c => c.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    ClaimCount = g.Count(),
                    ApprovedCount = g.Count(c => c.Status == "Approved"),
                    PendingCount = g.Count(c => c.Status == "Pending Verification" || c.Status == "Verified"),
                    TotalAmount = g.Sum(c => c.TotalAmount),
                    GrowthPercentage = 0.0 // Will calculate below
                })
                .OrderBy(m => m.Month)
                .ToList();

            // Calculate growth percentages
            var monthlySummary = new List<object>();
            for (int i = 0; i < monthlyGroups.Count; i++)
            {
                var currentMonth = monthlyGroups[i];
                double growthPercentage = 0;

                if (i > 0)
                {
                    var previousMonth = monthlyGroups[i - 1];
                    var previousAmount = previousMonth.TotalAmount;
                    var currentAmount = currentMonth.TotalAmount;

                    if (previousAmount > 0)
                    {
                        growthPercentage = (currentAmount - previousAmount) / previousAmount * 100;
                    }
                }

                monthlySummary.Add(new
                {
                    currentMonth.Month,
                    currentMonth.ClaimCount,
                    currentMonth.ApprovedCount,
                    currentMonth.PendingCount,
                    currentMonth.TotalAmount,
                    GrowthPercentage = growthPercentage
                });
            }

            ViewBag.MonthlySummary = monthlySummary;

            // Calculate average processing days (simplified)
            var approvedClaims = allClaims.Where(c => c.Status == "Approved" && c.ApprovedDate.HasValue);
            if (approvedClaims.Any())
            {
                ViewBag.AverageProcessingDays = approvedClaims.Average(c => (c.ApprovedDate.Value - c.SubmittedDate).TotalDays);
            }
            else
            {
                ViewBag.AverageProcessingDays = 0;
            }

            return View(allClaims); // Return all claims for the detailed register
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