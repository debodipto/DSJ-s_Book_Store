using Microsoft.AspNetCore.Mvc;

namespace DSJsBookStore.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string name, string email, string message)
        {
            // In a real app, send email
            TempData["Success"] = "Thank you for contacting us!";
            return RedirectToAction("Index");
        }
    }
}