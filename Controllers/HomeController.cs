using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Models;

namespace PROG6212_POE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // If user is already logged in, redirect to their dashboard
            if (HttpContext.Session.GetString("UserId") != null)
            {
                var userRole = HttpContext.Session.GetString("UserRole");
                return userRole switch
                {
                    "HR" => RedirectToAction("Dashboard", "HR"),
                    "Lecturer" => RedirectToAction("Dashboard", "Lecturer"),
                    "Coordinator" => RedirectToAction("Dashboard", "Coordinator"),
                    "Manager" => RedirectToAction("Dashboard", "Manager"),
                    _ => View()
                };
            }

            // Show search error if any
            if (TempData["SearchError"] != null)
            {
                ViewBag.SearchError = TempData["SearchError"].ToString();
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "About CMCS";
            ViewData["Message"] = "The Contract Monthly Claim System (CMCS) is designed to streamline and simplify the process of lecturer claims.";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact Us";
            ViewData["Message"] = "For inquiries or support, please contact us using the details below.";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            _logger.LogError("An error occurred. RequestId: {RequestId}", requestId);

            return View(new ErrorViewModel { RequestId = requestId });
        }

        // Dashboard Search
        [HttpGet]
        public IActionResult DashboardSearch(string dashboard)
        {
            if (string.IsNullOrWhiteSpace(dashboard))
            {
                TempData["SearchError"] = "Please enter a dashboard to search for.";
                return RedirectToAction("Index");
            }

            dashboard = dashboard.ToLower();

            if (dashboard.Contains("lecturer"))
            {
                return RedirectToAction("Dashboard", "Lecturer");
            }
            else if (dashboard.Contains("coordinator"))
            {
                return RedirectToAction("Dashboard", "Coordinator");
            }
            else if (dashboard.Contains("manager"))
            {
                return RedirectToAction("Dashboard", "Manager");
            }
            else if (dashboard.Contains("hr"))
            {
                return RedirectToAction("Dashboard", "HR");
            }

            // No match found
            TempData["SearchError"] = $"No matching dashboard found for '{dashboard}'.";
            return RedirectToAction("Index");
        }
    }
}