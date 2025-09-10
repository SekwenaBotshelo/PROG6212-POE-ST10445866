using Microsoft.AspNetCore.Mvc;

namespace PROG6212_POE.Controllers
{
    public class LecturerController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult SubmitClaim()
        {
            return View();
        }

        public IActionResult UploadDocuments()
        {
            return View();
        }

        public IActionResult TrackClaim()
        {
            return View();
        }
    }
}