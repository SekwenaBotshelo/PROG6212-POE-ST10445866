using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Models;
using PROG6212_POE.Services;
using System.Diagnostics;

namespace PROG6212_POE.Controllers
{
    public class LecturerController : Controller
    {
        // Helper method to get current lecturer
        private User GetCurrentLecturer()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId)) return null;

            var users = DataService.GetUsers();
            return users.FirstOrDefault(u => u.UserId.ToString() == userId && u.Role == "Lecturer");
        }

        // GET: /Lecturer/Dashboard
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("AccessDenied", "Account");

            var lecturer = GetCurrentLecturer();
            if (lecturer == null)
                return RedirectToAction("Logout", "Account");

            ViewBag.CurrentUser = lecturer;

            // Get only this lecturer's claims
            var lecturerClaims = DataService.GetClaimsByLecturer(lecturer.UserId);
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

        // POST: /Lecturer/SubmitClaim - SIMPLIFIED WORKING VERSION
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
            claim.Status = "Pending Verification";

            // CRITICAL: Clear Notes validation if empty and ensure it's not null
            if (string.IsNullOrEmpty(claim.Notes))
            {
                ModelState.Remove("Notes");
                claim.Notes = string.Empty;
            }

            // Manual validation for hours
            if (claim.TotalHours <= 0 || claim.TotalHours > 180)
            {
                ModelState.AddModelError("TotalHours", "Hours must be between 1 and 180.");
            }

            // Manual validation for month
            if (string.IsNullOrEmpty(claim.Month))
            {
                ModelState.AddModelError("Month", "Month is required.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle file upload
                    if (supportingDocument != null && supportingDocument.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{supportingDocument.FileName}";
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            supportingDocument.CopyTo(stream);
                        }

                        claim.DocumentPath = fileName;
                        claim.DocumentOriginalName = supportingDocument.FileName;
                    }

                    // Add the claim to storage
                    DataService.AddClaim(claim);

                    TempData["SuccessMessage"] = $"Claim #{claim.ClaimId} submitted successfully! It is now pending verification.";
                    return RedirectToAction("Dashboard");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                }
            }

            // If we get here, there were validation errors
            claim.Lecturer = lecturer;
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

            var lecturerClaims = DataService.GetClaimsByLecturer(lecturer.UserId);
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