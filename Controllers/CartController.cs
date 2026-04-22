using DSJsBookStore.Models;
using DSJsBookStore.Models.DTOs;
using DSJsBookStore.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DSJsBookStore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepo;

        public CartController(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }

        public async Task<IActionResult> AddItem(int bookId, int qty = 1)
        {
            await _cartRepo.AddItem(bookId, qty);
            return RedirectToAction(nameof(GetUserCart));
        }

        public async Task<IActionResult> RemoveItem(int bookId)
        {
            await _cartRepo.RemoveItem(bookId);
            return RedirectToAction(nameof(GetUserCart));
        }

        public async Task<IActionResult> GetUserCart()
        {
            var cart = await _cartRepo.GetUserCart();
            return View(cart ?? new ShoppingCart());
        }

        public async Task<IActionResult> GetTotalItemInCart()
        {
            var count = await _cartRepo.GetCartItemCount();
            return Ok(count);
        }

        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _cartRepo.DoCheckout(model);

            TempData["OrderMessage"] = result.Message;

            if (!result.Succeeded)
                return RedirectToAction(nameof(OrderFailure));

            return RedirectToAction(nameof(OrderSuccess), new { orderId = result.OrderId });
        }

        public async Task<IActionResult> OrderSuccess(int? orderId = null)
        {
            Order? order = null;
            if (orderId.HasValue)
            {
                order = await _cartRepo.GetOrderById(orderId.Value);
            }

            return View(order);
        }

        public IActionResult OrderFailure()
        {
            return View();
        }

        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _cartRepo.GetOrderById(id);

            if (order == null)
                return RedirectToAction(nameof(OrderFailure));

            return View(order);
        }

        public async Task<IActionResult> DeleteOrder(int id)
        {
            await _cartRepo.DeleteOrder(id);
            return RedirectToAction(nameof(OrderFailure));
        }
    }
}
