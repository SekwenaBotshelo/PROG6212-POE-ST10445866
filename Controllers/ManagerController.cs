using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Models;
using PROG6212_POE.Services;
using System.Diagnostics;

namespace PROG6212_POE.Controllers
{
    public class ManagerController : Controller
    {
        // Helper method to get current manager
        private User GetCurrentManager()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return null;

            var users = DataService.GetUsers();
            return users.FirstOrDefault(u => u.UserId.ToString() == userId && u.Role == "Manager");
        }

        // GET: /Manager/Dashboard
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Manager")
                return RedirectToAction("AccessDenied", "Account");

            var manager = GetCurrentManager();
            if (manager == null)
                return RedirectToAction("Logout", "Account");

            ViewBag.CurrentUser = manager;

            // Get claims pending approval using DataService
            var claims = DataService.GetClaims();
            var verifiedClaims = claims.Where(c => c.Status == "Verified").ToList();
            var approvedClaims = claims.Where(c => c.Status == "Approved" && c.ApprovedByManagerId == manager.UserId).ToList();

            ViewBag.PendingApprovalCount = verifiedClaims.Count;
            ViewBag.ApprovedCount = approvedClaims.Count;
            ViewBag.TotalClaims = claims.Count;

            return View(verifiedClaims);
        }

        // GET: /Manager/ApproveClaims
        public IActionResult ApproveClaims()
        {
            if (HttpContext.Session.GetString("UserRole") != "Manager")
                return RedirectToAction("AccessDenied", "Account");

            var manager = GetCurrentManager();
            if (manager == null)
                return RedirectToAction("Logout", "Account");

            var claims = DataService.GetClaims();
            var verifiedClaims = claims.Where(c => c.Status == "Verified").ToList();

            ViewBag.CurrentUser = manager;
            return View(verifiedClaims);
        }

        // GET: /Manager/ViewClaimDetails/{id}
        public IActionResult ViewClaimDetails(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Manager")
                return RedirectToAction("AccessDenied", "Account");

            var claims = DataService.GetClaims();
            var claim = claims.FirstOrDefault(c => c.ClaimId == id);

            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.ClaimId = id;
            ViewBag.CurrentUser = GetCurrentManager();
            return View(claim);
        }

        // POST: /Manager/ApproveClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveClaim(int claimId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Manager")
                return RedirectToAction("AccessDenied", "Account");

            var manager = GetCurrentManager();
            if (manager == null)
                return RedirectToAction("Logout", "Account");

            var claims = DataService.GetClaims();
            var claim = claims.FirstOrDefault(c => c.ClaimId == claimId);

            if (claim != null)
            {
                claim.Status = "Approved";
                claim.ApprovedByManagerId = manager.UserId;
                claim.ApprovedDate = DateTime.Now;

                DataService.UpdateClaim(claim);
                TempData["SuccessMessage"] = "Claim approved successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Claim not found.";
            }

            return RedirectToAction("Dashboard");
        }

        // POST: /Manager/RejectClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectClaim(int claimId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Manager")
                return RedirectToAction("AccessDenied", "Account");

            var manager = GetCurrentManager();
            if (manager == null)
                return RedirectToAction("Logout", "Account");

            var claims = DataService.GetClaims();
            var claim = claims.FirstOrDefault(c => c.ClaimId == claimId);

            if (claim != null)
            {
                claim.Status = "Rejected";
                claim.ApprovedByManagerId = manager.UserId;
                claim.ApprovedDate = DateTime.Now;

                DataService.UpdateClaim(claim);
                TempData["SuccessMessage"] = "Claim rejected successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Claim not found.";
            }

            return RedirectToAction("Dashboard");
        }

        // GET: /Manager/Reports
        public IActionResult Reports()
        {
            if (HttpContext.Session.GetString("UserRole") != "Manager")
                return RedirectToAction("AccessDenied", "Account");

            var manager = GetCurrentManager();
            if (manager == null)
                return RedirectToAction("Logout", "Account");

            var claims = DataService.GetClaims();
            var approvedClaims = claims.Where(c => c.Status == "Approved").ToList();

            // LINQ queries for reporting
            var monthlySummary = approvedClaims
                .GroupBy(c => c.Month)
                .Select(g => new {
                    Month = g.Key,
                    TotalAmount = g.Sum(c => c.TotalAmount),
                    ClaimCount = g.Count(),
                    AverageAmount = g.Average(c => c.TotalAmount)
                })
                .ToList();

            ViewBag.MonthlySummary = monthlySummary;
            ViewBag.TotalApprovedAmount = approvedClaims.Sum(c => c.TotalAmount);
            ViewBag.TotalApprovedClaims = approvedClaims.Count;
            ViewBag.CurrentUser = manager;

            return View(approvedClaims);
        }
    }
}