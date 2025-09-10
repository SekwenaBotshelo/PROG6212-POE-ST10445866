using Microsoft.AspNetCore.Mvc;

namespace PROG6212_POE.Controllers
{
    public class CoordinatorController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult VerifyClaims()
        {
            return View();
        }

        public IActionResult ViewClaimDetails(int id)
        {
            ViewBag.ClaimId = id;
            return View();
        }
    }
}