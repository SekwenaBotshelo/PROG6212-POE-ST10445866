using Microsoft.AspNetCore.Mvc;

namespace PROG6212_POE.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult ApproveClaims()
        {
            return View();
        }

        public IActionResult ViewClaimDetails(int id)
        {
            ViewBag.ClaimId = id;
            return View();
        }

        public IActionResult Reports()
        {
            return View();
        }
    }
}