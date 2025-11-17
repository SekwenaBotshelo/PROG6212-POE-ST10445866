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

        // POST: /Lecturer/SubmitClaim - UPDATED TO STORE CALCULATED AMOUNT
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

                // Calculate total amount
                var totalAmount = model.TotalHours * (double)lecturer.HourlyRate;

                // Create new claim
                var newClaim = new Claim
                {
                    LecturerId = lecturer.UserId,
                    Month = model.Month,
                    TotalHours = model.TotalHours,
                    Notes = model.Notes ?? string.Empty,
                    SubmittedDate = DateTime.Now,
                    Status = "Pending Verification",
                    // Store the hourly rate and calculated amount
                    StoredHourlyRate = lecturer.HourlyRate,
                    StoredTotalAmount = (decimal)totalAmount
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

        // GET: /Lecturer/UploadDocuments - UPDATED TO SUPPORT CLAIM ID
        public IActionResult UploadDocuments(int? claimId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.CurrentUser = GetCurrentLecturer();

            // If claimId is provided, verify the claim belongs to the current user
            if (claimId.HasValue)
            {
                var lecturer = GetCurrentLecturer();
                var claim = DataService.GetClaims().FirstOrDefault(c => c.ClaimId == claimId && c.LecturerId == lecturer.UserId);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found or you don't have permission to access it.";
                    // Still show the upload page but with error message
                }
            }

            return View();
        }

        // POST: /Lecturer/UploadDocuments - UPDATED TO HANDLE CLAIM-SPECIFIC UPLOADS
        [HttpPost]
        public IActionResult UploadDocuments(IFormFile supportingFile, int? claimId)
        {
            if (HttpContext.Session.GetString("UserRole") != "Lecturer")
                return RedirectToAction("AccessDenied", "Account");

            var lecturer = GetCurrentLecturer();
            if (lecturer == null)
                return RedirectToAction("Logout", "Account");

            try
            {
                if (supportingFile == null || supportingFile.Length == 0)
                {
                    TempData["ErrorMessage"] = "No file selected!";
                    return RedirectToAction("UploadDocuments", new { claimId });
                }

                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
                var extension = Path.GetExtension(supportingFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "Invalid file type. Only PDF, Word, and image files are allowed.";
                    return RedirectToAction("UploadDocuments", new { claimId });
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string fileName;
                string successMessage;

                if (claimId.HasValue)
                {
                    // Claim-specific upload
                    var claim = DataService.GetClaims().FirstOrDefault(c => c.ClaimId == claimId && c.LecturerId == lecturer.UserId);
                    if (claim == null)
                    {
                        TempData["ErrorMessage"] = "Claim not found or you don't have permission to access it.";
                        return RedirectToAction("UploadDocuments", new { claimId });
                    }

                    fileName = $"claim_{claimId}_{DateTime.Now:yyyyMMddHHmmss}_{supportingFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        supportingFile.CopyTo(stream);
                    }

                    // Update claim with document information
                    claim.DocumentPath = fileName;
                    claim.DocumentOriginalName = supportingFile.FileName;

                    successMessage = $"Document uploaded successfully for Claim #{claimId}!";
                    return RedirectToAction("TrackClaims");
                }
                else
                {
                    // General document upload
                    fileName = $"document_{DateTime.Now:yyyyMMddHHmmss}_{supportingFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        supportingFile.CopyTo(stream);
                    }

                    successMessage = "File uploaded successfully!";
                }

                TempData["SuccessMessage"] = successMessage;
                return RedirectToAction("UploadDocuments", new { claimId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("UploadDocuments", new { claimId });
            }
        }
    }
}