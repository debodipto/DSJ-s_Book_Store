using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DSJsBookStore.Models;
using DSJsBookStore.Models.DTOs;
using Microsoft.Extensions.Logging;

namespace DSJsBookStore.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CartRepository> _logger;

        public CartRepository(
            ApplicationDbContext db,
            IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager,
            ILogger<CartRepository> logger)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        // ================= USER ID =================
        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            return principal == null ? "" : _userManager.GetUserId(principal) ?? "";
        }

        // ================= ADD ITEM =================
        public async Task<int> AddItem(int bookId, int qty)
        {
            var userId = GetUserId();
            var cart = await GetCart(userId);

            if (cart == null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId,
                    CartDetails = new List<CartDetail>()
                };

                _db.ShoppingCarts.Add(cart);
                await _db.SaveChangesAsync();
            }

            var cartItem = await _db.CartDetails
                .FirstOrDefaultAsync(x => x.ShoppingCartId == cart.Id && x.BookId == bookId);

            if (cartItem != null)
            {
                cartItem.Quantity += qty;
            }
            else
            {
                var book = await _db.Books.FindAsync(bookId);
                if (book == null) throw new Exception("Book not found");

                _db.CartDetails.Add(new CartDetail
                {
                    BookId = bookId,
                    ShoppingCartId = cart.Id,
                    Quantity = qty,
                    UnitPrice = book.Price
                });
            }

            await _db.SaveChangesAsync();
            return await GetCartItemCount(userId);
        }

        // ================= REMOVE ITEM =================
        public async Task<int> RemoveItem(int bookId)
        {
            var userId = GetUserId();
            var cart = await GetCart(userId);

            if (cart == null) return 0;

            var cartItem = await _db.CartDetails
                .FirstOrDefaultAsync(x => x.ShoppingCartId == cart.Id && x.BookId == bookId);

            if (cartItem == null)
                return await GetCartItemCount(userId);

            if (cartItem.Quantity <= 1)
                _db.CartDetails.Remove(cartItem);
            else
                cartItem.Quantity--;

            await _db.SaveChangesAsync();
            return await GetCartItemCount(userId);
        }

        // ================= GET CART =================
        public async Task<ShoppingCart?> GetCart(string userId)
        {
            return await _db.ShoppingCarts
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        // ================= GET USER CART =================
        public async Task<ShoppingCart?> GetUserCart()
        {
            var userId = GetUserId();

            return await _db.ShoppingCarts
                .Include(c => c.CartDetails!)
                    .ThenInclude(cd => cd.Book!)
                        .ThenInclude(b => b.Stock!)
                .Include(c => c.CartDetails!)
                    .ThenInclude(cd => cd.Book!)
                        .ThenInclude(b => b.Genre!)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        // ================= CART COUNT =================
        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
                userId = GetUserId();

            return await _db.CartDetails
                .Where(x => x.ShoppingCart != null && x.ShoppingCart.UserId == userId)
                .SumAsync(x => x.Quantity);
        }

        // ================= CHECKOUT =================
        public async Task<CheckoutResult> DoCheckout(CheckoutModel model)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var userId = GetUserId();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("Checkout failed because the current user id was empty.");
                    return new CheckoutResult { Succeeded = false, Message = "Unable to identify the current user." };
                }

                var cart = await GetCart(userId);

                if (cart == null)
                {
                    _logger.LogWarning("Checkout failed because no cart was found for user {UserId}.", userId);
                    return new CheckoutResult { Succeeded = false, Message = "No active cart was found." };
                }

                var cartItems = await _db.CartDetails
                    .Where(x => x.ShoppingCartId == cart.Id)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    _logger.LogWarning("Checkout failed because cart {CartId} had no items.", cart.Id);
                    return new CheckoutResult { Succeeded = false, Message = "Your cart is empty." };
                }

                var status = await _db.OrderStatuses
                    .FirstOrDefaultAsync(x => x.StatusName == "Pending");

                if (status == null)
                {
                    status = new OrderStatus { StatusId = 1, StatusName = "Pending" };
                    _db.OrderStatuses.Add(status);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Created missing Pending order status during checkout.");
                }

                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Name = model?.Name ?? "",
                    Email = model?.Email ?? "",
                    MobileNumber = model?.MobileNumber ?? "",
                    Address = model?.Address ?? "",
                    PaymentMethod = model?.PaymentMethod ?? "",
                    IsPaid = false,
                    OrderStatusId = status.Id
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    var stock = await _db.Stocks
                        .FirstOrDefaultAsync(x => x.BookId == item.BookId);

                    if (stock != null)
                    {
                        if (stock.Quantity < item.Quantity)
                        {
                            _logger.LogWarning(
                                "Checkout failed because stock was insufficient for book {BookId}. Requested {Requested}, available {Available}.",
                                item.BookId,
                                item.Quantity,
                                stock.Quantity);
                            await transaction.RollbackAsync();
                            return new CheckoutResult
                            {
                                Succeeded = false,
                                Message = "One or more items do not have enough stock."
                            };
                        }

                        stock.Quantity -= item.Quantity;
                    }

                    _db.OrderDetails.Add(new OrderDetails
                    {
                        OrderId = order.Id,
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });
                }

                _db.CartDetails.RemoveRange(cartItems);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Checkout succeeded for user {UserId}. OrderId: {OrderId}", userId, order.Id);
                return new CheckoutResult
                {
                    Succeeded = true,
                    OrderId = order.Id,
                    Message = "Your order was placed successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout failed with an exception.");
                await transaction.RollbackAsync();
                return new CheckoutResult
                {
                    Succeeded = false,
                    Message = "Checkout failed due to an unexpected error."
                };
            }
        }

        // ================= ORDERS =================
        public async Task<List<Order>> GetUserOrders()
        {
            var userId = GetUserId();

            return await _db.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreateDate)
                .ToListAsync();
        }

        // ================= ORDER BY ID =================
        public async Task<Order?> GetOrderById(int id)
        {
            return await _db.Orders
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                        .ThenInclude(b => b!.Genre)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        // ================= DELETE ORDER =================
        public async Task DeleteOrder(int id)
        {
            var order = await _db.Orders.FindAsync(id);

            if (order != null)
            {
                _db.Orders.Remove(order);
                await _db.SaveChangesAsync();
            }
        }
    }
}
