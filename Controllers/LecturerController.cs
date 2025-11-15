using Microsoft.AspNetCore.Mvc;
using PROG6212_POE.Models;
using System.Collections.Generic;

namespace PROG6212_POE.Controllers
{
    public class LecturerController : Controller
    {
        // Temporary storage for submitted claims
        private static List<Claim> _claims = new List<Claim>();
        private static int _nextClaimId = 101; // starting ClaimId

        // Dashboard shows all claims
        public IActionResult Dashboard()
        {
            var claimsToShow = _claims ?? new List<Claim>();
            return View(claimsToShow);
        }

        // GET: SubmitClaim form
        public IActionResult SubmitClaim()
        {
            return View(new Claim());
        }

        // POST: SubmitClaim form
        [HttpPost]
        public IActionResult SubmitClaim(Claim claim)
        {
            if (ModelState.IsValid)
            {
                claim.ClaimId = _nextClaimId++;
                _claims.Add(claim);

                TempData["SuccessMessage"] = "Claim submitted successfully!";
                return RedirectToAction("Dashboard"); // go back to Dashboard
            }

            return View(claim);
        }

        // View Submitted Claims (optional separate page)
        public IActionResult TrackClaim()
        {
            var claimsToShow = _claims ?? new List<Claim>();
            return View("TrackStatus", claimsToShow);
        }

        // Upload Supporting Documents page
        public IActionResult UploadDocuments()
        {
            return View("UploadDocument");
        }

        [HttpPost]
        public IActionResult UploadDocuments(IFormFile supportingFile)
        {
            if (supportingFile != null && supportingFile.Length > 0)
            {
                // Allowed extensions
                var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".txt" };
                var extension = Path.GetExtension(supportingFile.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    TempData["ErrorMessage"] = "Invalid file type. Only PDF and image files are allowed.";
                    return RedirectToAction("UploadDocuments");
                }

                // Save file
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, supportingFile.FileName);

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