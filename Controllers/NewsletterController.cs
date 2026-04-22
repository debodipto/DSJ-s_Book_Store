using Microsoft.AspNetCore.Mvc;

namespace DSJsBookStore.Controllers
{
    public class NewsletterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Subscribe(string email)
        {
            // In a real app, save to DB or send to service
            TempData["Success"] = "Subscribed successfully!";
            return RedirectToAction("Index");
        }
    }
}