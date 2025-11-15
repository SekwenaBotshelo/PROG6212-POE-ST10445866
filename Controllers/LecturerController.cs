using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Models;
using System.Diagnostics;

namespace PROG6212_POE.Controllers
{
    public class LecturerController : Controller
    {
        // Helper method to get current lecturer
        private User GetCurrentLecturer()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var users = AccountController.GetUsers();
            return users.FirstOrDefault(u => u.UserId.ToString() == userId && u.Role == "Lecturer");
        }

        // Helper method to get user by ID
        private User GetUserById(int userId)
        {
            var users = AccountController.GetUsers();
            return users.FirstOrDefault(u => u.UserId == userId);
        }

        // GET: /Lecturer/Dashboard
        public IActionResult Dashboard()
        {
            // Authorization check
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("AccessDenied", "Account");

            var lecturer = GetCurrentLecturer();
            if (lecturer == null)
                return RedirectToAction("Logout", "Account");

            ViewBag.CurrentUser = lecturer;

            // Get only this lecturer's claims
            var claims = HRController.GetClaims();
            var lecturerClaims = claims.Where(c => c.LecturerId == lecturer.UserId).ToList();
            return View(lecturerClaims);
        }

        // GET: /Lecturer/SubmitClaim
        public IActionResult SubmitClaim()
        {
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("AccessDenied", "Account");

            var lecturer = GetCurrentLecturer();
            if (lecturer == null)
                return RedirectToAction("Logout", "Account");

            var claim = new Claim
            {
                LecturerId = lecturer.UserId,
                Lecturer = lecturer
            };

            ViewBag.CurrentUser = lecturer;
            return View(claim);
        }

        // POST: /Lecturer/SubmitClaim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitClaim(Claim claim, IFormFile supportingDocument)
        {
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("AccessDenied", "Account");

            var lecturer = GetCurrentLecturer();
            if (lecturer == null)
                return RedirectToAction("Logout", "Account");

            // Auto-populate from logged-in lecturer
            claim.LecturerId = lecturer.UserId;
            claim.Lecturer = lecturer;
            claim.SubmittedDate = DateTime.Now;

            // Validation: Max 180 hours
            if (claim.TotalHours > 180)
            {
                ModelState.AddModelError("TotalHours", "Maximum 180 hours per month allowed.");
            }

            if (ModelState.IsValid)
            {
                var claims = HRController.GetClaims();
                claim.ClaimId = claims.Count > 0 ? claims.Max(c => c.ClaimId) + 1 : 101;

                // Handle file upload
                if (supportingDocument != null && supportingDocument.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{claim.ClaimId}_{supportingDocument.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        supportingDocument.CopyTo(stream);
                    }

                    claim.DocumentPath = fileName;
                    claim.DocumentOriginalName = supportingDocument.FileName;
                }

                HRController.AddClaim(claim);
                TempData["SuccessMessage"] = "Claim submitted successfully! It is now pending verification.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.CurrentUser = lecturer;
            return View(claim);
        }

        // GET: /Lecturer/TrackClaims
        public IActionResult TrackClaims()
        {
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("AccessDenied", "Account");

            var lecturer = GetCurrentLecturer();
            if (lecturer == null)
                return RedirectToAction("Logout", "Account");

            var claims = HRController.GetClaims();
            var lecturerClaims = claims.Where(c => c.LecturerId == lecturer.UserId).ToList();
            ViewBag.CurrentUser = lecturer;
            return View(lecturerClaims);
        }

        // GET: /Lecturer/UploadDocuments
        public IActionResult UploadDocuments()
        {
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.CurrentUser = GetCurrentLecturer();
            return View();
        }

        [HttpPost]
        public IActionResult UploadDocuments(IFormFile supportingFile)
        {
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("AccessDenied", "Account");

            if (supportingFile != null && supportingFile.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                var extension = Path.GetExtension(supportingFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "Invalid file type. Only PDF, Word, and image files are allowed.";
                    return RedirectToAction("UploadDocuments");
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"document_{DateTime.Now:yyyyMMddHHmmss}_{supportingFile.FileName}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    supportingFile.CopyTo(stream);
                }

                TempData["SuccessMessage"] = "File uploaded successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "No file selected!";
            }

            return RedirectToAction("UploadDocuments");
        }
    }
}