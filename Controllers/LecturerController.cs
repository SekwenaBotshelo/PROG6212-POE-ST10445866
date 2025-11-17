using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Models;
using PROG6212_POE.Services;

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

            ViewBag.CurrentUser = lecturer;
            return View(new Claim { LecturerId = lecturer.UserId });
        }

        // POST: /Lecturer/SubmitClaim - CLEAN VERSION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitClaim(Claim model, IFormFile supportingDocument)
        {
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("AccessDenied", "Account");

            var lecturer = GetCurrentLecturer();
            if (lecturer == null)
                return RedirectToAction("Logout", "Account");

            ViewBag.CurrentUser = lecturer;

            try
            {
                // Manual validation
                if (string.IsNullOrEmpty(model.Month))
                {
                    TempData["ErrorMessage"] = "Month is required.";
                    return View(model);
                }

                if (model.TotalHours <= 0 || model.TotalHours > 180)
                {
                    TempData["ErrorMessage"] = "Hours must be between 1 and 180.";
                    return View(model);
                }

                // Create new claim
                var newClaim = new Claim
                {
                    LecturerId = lecturer.UserId,
                    Month = model.Month,
                    TotalHours = model.TotalHours,
                    Notes = model.Notes ?? string.Empty,
                    SubmittedDate = DateTime.Now,
                    Status = "Pending Verification"
                };

                // Handle file upload if provided
                if (supportingDocument != null && supportingDocument.Length > 0)
                {
                    var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                    var extension = Path.GetExtension(supportingDocument.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["ErrorMessage"] = "Invalid file type. Only PDF, Word, and image files are allowed.";
                        return View(model);
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"claim_{DateTime.Now:yyyyMMddHHmmss}_{supportingDocument.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        supportingDocument.CopyTo(stream);
                    }

                    // Save file info to claim
                    newClaim.DocumentPath = fileName;
                    newClaim.DocumentOriginalName = supportingDocument.FileName;
                }

                // Add the claim to storage
                DataService.AddClaim(newClaim);

                TempData["SuccessMessage"] = $"Claim #{newClaim.ClaimId} submitted successfully! It is now pending verification.";
                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return View(model);
            }
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