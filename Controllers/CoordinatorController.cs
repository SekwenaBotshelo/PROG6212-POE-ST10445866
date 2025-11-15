using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Models;
using PROG6212_POE.Services;
using System.Diagnostics;

namespace PROG6212_POE.Controllers
{
    public class CoordinatorController : Controller
    {
        // Helper method to get current coordinator
        private User GetCurrentCoordinator()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return null;

            var users = DataService.GetUsers();
            return users.FirstOrDefault(u => u.UserId.ToString() == userId && u.Role == "Coordinator");
        }

        // GET: /Coordinator/Dashboard
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Coordinator")
                return RedirectToAction("AccessDenied", "Account");

            var coordinator = GetCurrentCoordinator();
            if (coordinator == null)
                return RedirectToAction("Logout", "Account");

            ViewBag.CurrentUser = coordinator;

            // Get claims pending verification using DataService
            var claims = DataService.GetClaims();
            var pendingClaims = claims.Where(c => c.Status == "Pending Verification").ToList();
            var verifiedClaims = claims.Where(c => c.Status == "Verified" && c.VerifiedByCoordinatorId == coordinator.UserId).ToList();

            ViewBag.PendingCount = pendingClaims.Count;
            ViewBag.VerifiedCount = verifiedClaims.Count;
            ViewBag.TotalClaims = claims.Count;

            return View(pendingClaims);
        }

        // GET: /Coordinator/VerifyClaims
        public IActionResult VerifyClaims()
        {
            if (HttpContext.Session.GetString("UserRole") != "Coordinator")
                return RedirectToAction("AccessDenied", "Account");

            var coordinator = GetCurrentCoordinator();
            if (coordinator == null)
                return RedirectToAction("Logout", "Account");

            var claims = DataService.GetClaims();
            var pendingClaims = claims.Where(c => c.Status == "Pending Verification").ToList();

            ViewBag.CurrentUser = coordinator;
            return View(pendingClaims);
        }

        // GET: /Coordinator/ViewClaimDetails/{id}
        public IActionResult ViewClaimDetails(int id)
        {
            if (HttpContext.Session.GetString("UserRole") != "Coordinator")
                return RedirectToAction("AccessDenied", "Account");

            var claims = DataService.GetClaims();
            var claim = claims.FirstOrDefault(c => c.ClaimId == id);

            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.ClaimId = id;
            ViewBag.CurrentUser = GetCurrentCoordinator();
            return View(claim);
        }

        // POST: /Coordinator/VerifyClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyClaim(int claimId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Coordinator")
                return RedirectToAction("AccessDenied", "Account");

            var coordinator = GetCurrentCoordinator();
            if (coordinator == null)
                return RedirectToAction("Logout", "Account");

            var claims = DataService.GetClaims();
            var claim = claims.FirstOrDefault(c => c.ClaimId == claimId);

            if (claim != null)
            {
                claim.Status = "Verified";
                claim.VerifiedByCoordinatorId = coordinator.UserId;
                claim.VerifiedDate = DateTime.Now;

                DataService.UpdateClaim(claim);
                TempData["SuccessMessage"] = "Claim verified successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Claim not found.";
            }

            return RedirectToAction("Dashboard");
        }

        // POST: /Coordinator/RejectClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RejectClaim(int claimId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Coordinator")
                return RedirectToAction("AccessDenied", "Account");

            var coordinator = GetCurrentCoordinator();
            if (coordinator == null)
                return RedirectToAction("Logout", "Account");

            var claims = DataService.GetClaims();
            var claim = claims.FirstOrDefault(c => c.ClaimId == claimId);

            if (claim != null)
            {
                claim.Status = "Rejected";
                claim.VerifiedByCoordinatorId = coordinator.UserId;
                claim.VerifiedDate = DateTime.Now;

                DataService.UpdateClaim(claim);
                TempData["SuccessMessage"] = "Claim rejected successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Claim not found.";
            }

            return RedirectToAction("Dashboard");
        }
    }
}