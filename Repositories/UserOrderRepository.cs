using DSJsBookStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DSJsBookStore.Repositories
{
    public class UserOrderRepository : IUserOrderRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;
        private static readonly (int StatusId, string StatusName)[] DefaultOrderStatuses =
        [
            (1, "Pending"),
            (2, "Confirmed"),
            (3, "Processing"),
            (4, "Shipped"),
            (5, "Out for Delivery"),
            (6, "Delivered"),
            (7, "Cancelled"),
            (8, "Returned"),
            (9, "Refund")
        ];

        public UserOrderRepository(
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task ChangeOrderStatus(UpdateOrderStatusModel data)
        {
            await EnsureOrderStatusesAsync();

            var order = await _db.Orders.FindAsync(data.OrderId);

            if (order == null)
                throw new InvalidOperationException($"Order with id {data.OrderId} not found");

            var statusExists = await _db.OrderStatuses.AnyAsync(x => x.Id == data.OrderStatusId);
            if (!statusExists)
                throw new InvalidOperationException($"Order status with id {data.OrderStatusId} not found");

            order.OrderStatusId = data.OrderStatusId;
            await _db.SaveChangesAsync();
        }

        public async Task<Order?> GetOrderById(int id)
        {
            return await _db.Orders
                .Include(x => x.OrderDetails)
                    .ThenInclude(x => x.Book)
                .Include(x => x.OrderStatus)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<OrderStatus>> GetOrderStatuses()
        {
            await EnsureOrderStatusesAsync();

            return await _db.OrderStatuses
                .OrderBy(x => x.StatusId)
                .ToListAsync();
        }

        private async Task EnsureOrderStatusesAsync()
        {
            var existingStatuses = await _db.OrderStatuses.ToListAsync();
            var hasChanges = false;

            foreach (var (statusId, statusName) in DefaultOrderStatuses)
            {
                var existingStatus = existingStatuses
                    .FirstOrDefault(x => x.StatusId == statusId || x.StatusName == statusName);

                if (existingStatus == null)
                {
                    _db.OrderStatuses.Add(new OrderStatus
                    {
                        StatusId = statusId,
                        StatusName = statusName
                    });
                    hasChanges = true;
                    continue;
                }

                if (existingStatus.StatusId != statusId || existingStatus.StatusName != statusName)
                {
                    existingStatus.StatusId = statusId;
                    existingStatus.StatusName = statusName;
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await _db.SaveChangesAsync();
            }
        }

        public async Task TogglePaymentStatus(int orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);

            if (order == null)
                throw new InvalidOperationException($"Order with id {orderId} not found");

            order.IsPaid = !order.IsPaid;
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<Order>> UserOrders(bool getAll = false)
        {
            var orders = _db.Orders
                .Include(x => x.OrderStatus)
.Include(x => x.OrderDetails)
    .ThenInclude(x => x.Book)
        .ThenInclude(b => b!.Genre)
                .AsQueryable();

            if (!getAll)
            {
                var userId = GetUserId();

                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged in");

                orders = orders.Where(x => x.UserId == userId);
            }

            return await orders.ToListAsync();
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;

            if (principal == null)
                return "";

            return _userManager.GetUserId(principal) ?? "";
        }
    }
}
