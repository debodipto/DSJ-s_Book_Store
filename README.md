# DSJsBookStore

## Overview

DSJsBookStore is an ASP.NET Core MVC bookstore application built on:

- `.NET 10`
- `ASP.NET Core MVC + Razor Pages`
- `ASP.NET Core Identity`
- `Entity Framework Core`
- `SQLite` via `bookshop.db`

The application supports:

- browsing and filtering books
- admin management for books, genres, stock, orders, analytics, and bulk operations
- user authentication and profile management
- cart, checkout, and order history
- wishlist and reviews

## Startup Flow

The main startup logic lives in [Program.cs](/d:/Projects/DSJsBookStore/Program.cs).

Application boot sequence:

1. Read `DefaultConnection` from configuration.
2. Register `ApplicationDbContext` with SQLite.
3. Register ASP.NET Identity with default UI.
4. Register MVC, Razor Pages, `HttpContextAccessor`, email sender, and claim transformation.
5. Register repositories and shared services in DI.
6. Build the app.
7. Run `DbSeeder.SeedDefaultData(...)`.
8. Configure middleware:
   `UseHttpsRedirection -> UseStaticFiles -> UseRouting -> UseAuthentication -> UseAuthorization`
9. Map default MVC route and Identity Razor Pages.

## Architecture

The codebase mostly follows this request path:

`Browser -> Controller -> Repository (or DbContext) -> ApplicationDbContext -> SQLite -> View/JSON response`

### Main layers

- `Controllers/`
  Handles HTTP requests and returns views or JSON.
- `Repositories/`
  Contains most business/data-access logic.
- `Models/`
  Contains EF entities, DTOs, and view models.
- `Data/`
  Contains `ApplicationDbContext` and startup seeding.
- `Shared/`
  Contains file upload, admin claim transformation, and no-op email sending.
- `Views/`
  Razor UI for user and admin pages.

## Data Model Flow

The main entity relationships come from [ApplicationDbContext.cs](/d:/Projects/DSJsBookStore/Data/ApplicationDbContext.cs) and `Models/`.

Core entities:

- `Genre` -> one genre can have many `Book`
- `Book` -> belongs to one `Genre`
- `Book` -> can have one `Stock`
- `ShoppingCart` -> belongs to one user and contains many `CartDetail`
- `CartDetail` -> links `ShoppingCart` and `Book`
- `Order` -> belongs to one user and contains many `OrderDetails`
- `OrderDetails` -> links `Order` and `Book`
- `Order` -> references one `OrderStatus`
- `Wishlist` -> links user and book
- `Review` -> links user and book

## Seeding Flow

The startup seed logic is in [DbSeeder.cs](/d:/Projects/DSJsBookStore/Data/DbSeeder.cs).

Seed process:

1. Ensure the database exists.
2. Manually create `Wishlists` and `Reviews` tables if missing.
3. Apply pending EF migrations.
4. Ensure roles exist:
   `Admin`, `User`
5. Ensure the default admin account exists:
   `admin@gmail.com / Admin@123`
6. Seed genres if empty.
7. Seed sample books if empty.
8. Seed stock records for books if stock is missing.
9. Seed order statuses if empty.

## Functional Process Flow

### 1. Home and Catalog Flow

Files involved:

- [HomeController.cs](/d:/Projects/DSJsBookStore/Controllers/HomeController.cs)
- [HomeRepository.cs](/d:/Projects/DSJsBookStore/Repositories/HomeRepository.cs)

Flow:

1. User opens `/Home/Index`.
2. Controller receives optional filters:
   search term, genre, price range, sort order.
3. `HomeRepository.GetBooks(...)` builds an EF query.
4. Query includes `Genre` and `Stock`.
5. Results are projected into `BookDisplayModel`.
6. Controller loads genres for filter dropdowns.
7. View renders the book list.

### 2. Book Details and Review Flow

Files involved:

- [BookController.cs](/d:/Projects/DSJsBookStore/Controllers/BookController.cs)
- [ReviewController.cs](/d:/Projects/DSJsBookStore/Controllers/ReviewController.cs)

Flow:

1. User opens `/Book/Details/{id}`.
2. `BookController` loads the selected book through `IBookRepository`.
3. Approved reviews are loaded directly from `_db.Reviews`.
4. A `BookDetailsViewModel` is built with:
   book, reviews, average rating, total reviews.
5. Logged-in users can submit a review.
6. `ReviewController.AddReview(...)` validates:
   rating, comment, user identity, duplicate review.
7. Review is saved with `IsApproved = false`.
8. Admin can later approve or delete the review.

### 3. Cart Flow

Files involved:

- [CartController.cs](/d:/Projects/DSJsBookStore/Controllers/CartController.cs)
- [CartRepository.cs](/d:/Projects/DSJsBookStore/Repositories/CartRepository.cs)

Flow:

1. Logged-in user clicks add-to-cart.
2. `CartController.AddItem(...)` calls `CartRepository.AddItem(...)`.
3. Repository resolves the current user id from `HttpContext`.
4. Repository creates a `ShoppingCart` if one does not exist.
5. If the book already exists in cart, quantity increases.
6. Otherwise, a new `CartDetail` is inserted with current book price.
7. Cart page loads via `GetUserCart()`.
8. Repository returns cart details including `Book`, `Genre`, and `Stock`.

### 4. Checkout and Order Creation Flow

Files involved:

- [CartController.cs](/d:/Projects/DSJsBookStore/Controllers/CartController.cs)
- [CartRepository.cs](/d:/Projects/DSJsBookStore/Repositories/CartRepository.cs)

Flow:

1. User opens checkout page and submits `CheckoutModel`.
2. Controller validates the form.
3. `CartRepository.DoCheckout(...)` starts a DB transaction.
4. Repository validates:
   current user, active cart, cart items.
5. Repository ensures `Pending` order status exists.
6. A new `Order` is created.
7. For each cart item:
   - load stock
   - verify enough quantity exists
   - decrement stock
   - create `OrderDetails`
8. Cart items are removed.
9. Changes are saved and transaction is committed.
10. Controller redirects to success or failure page.

Text flow:

`Cart -> Checkout form -> Validate -> Create Order -> Check Stock -> Create OrderDetails -> Reduce Stock -> Clear Cart -> Success`

### 5. User Order History Flow

Files involved:

- [UserOrderController.cs](/d:/Projects/DSJsBookStore/Controllers/UserOrderController.cs)
- [UserOrderRepository.cs](/d:/Projects/DSJsBookStore/Repositories/UserOrderRepository.cs)

Flow:

1. Logged-in user opens `/UserOrder/UserOrders`.
2. Repository gets current user id.
3. Orders are loaded with `OrderStatus`, `OrderDetails`, `Book`, and `Genre`.
4. View shows past orders and statuses.

### 6. Wishlist Flow

Files involved:

- [WishlistController.cs](/d:/Projects/DSJsBookStore/Controllers/WishlistController.cs)
- [WishlistRepository.cs](/d:/Projects/DSJsBookStore/Repositories/WishlistRepository.cs)

Flow:

1. Logged-in user adds or removes a book from wishlist.
2. Controller reads current user id from claims.
3. Repository checks whether the book already exists in wishlist.
4. Insert or delete happens in `Wishlists`.
5. Wishlist page loads books with their genre data.

### 7. Admin Book Management Flow

Files involved:

- [BookController.cs](/d:/Projects/DSJsBookStore/Controllers/BookController.cs)
- [BookRepository.cs](/d:/Projects/DSJsBookStore/Repositories/BookRepository.cs)
- [FileService.cs](/d:/Projects/DSJsBookStore/Shared/FileService.cs)

Flow:

1. Admin opens add or update book page.
2. Controller loads genre list.
3. On submit, model validation runs.
4. If an image is uploaded:
   - max size is checked
   - extension is validated
   - file is saved to `wwwroot/images`
5. Controller maps DTO to `Book`.
6. Repository inserts or updates the record.
7. Old image is deleted after a successful replacement.

### 8. Admin Genre and Stock Flow

Files involved:

- [GenreController.cs](/d:/Projects/DSJsBookStore/Controllers/GenreController.cs)
- [GenreRepository.cs](/d:/Projects/DSJsBookStore/Repositories/GenreRepository.cs)
- [StockController.cs](/d:/Projects/DSJsBookStore/Controllers/StockController.cs)
- [StockRepository.cs](/d:/Projects/DSJsBookStore/Repositories/StockRepository.cs)

Flow:

1. Admin manages genres with create, update, delete actions.
2. Admin manages stock per book.
3. Stock repository inserts a new `Stock` record if missing, otherwise updates quantity.

### 9. Admin Order Operations Flow

Files involved:

- [AdminOperationsController.cs](/d:/Projects/DSJsBookStore/Controllers/AdminOperationsController.cs)
- [UserOrderRepository.cs](/d:/Projects/DSJsBookStore/Repositories/UserOrderRepository.cs)

Flow:

1. Admin opens all orders.
2. Repository loads every order with related details.
3. Admin can:
   - toggle payment status
   - change order status
4. Repository updates the selected order and saves changes.

### 10. Analytics and Reports Flow

Files involved:

- [AnalyticsController.cs](/d:/Projects/DSJsBookStore/Controllers/AnalyticsController.cs)
- [ReportsController.cs](/d:/Projects/DSJsBookStore/Controllers/ReportsController.cs)
- [ReportRepository.cs](/d:/Projects/DSJsBookStore/Repositories/ReportRepository.cs)

Flow:

1. Admin opens analytics dashboard.
2. Controller queries `ApplicationDbContext` directly for:
   total sales, orders, customers, books, monthly sales, top-selling books, recent orders, low stock.
3. JSON endpoints also provide chart data.
4. Reports page requests top-selling books by date range.
5. Current `ReportRepository.GetTopNSellingBooksByDate(...)` returns an empty list, so that report is not fully implemented yet.

### 11. Bulk Operations Flow

Files involved:

- [BulkOperationsController.cs](/d:/Projects/DSJsBookStore/Controllers/BulkOperationsController.cs)

Flow:

1. Admin selects multiple books or orders.
2. Controller performs bulk stock updates, price updates, deletes, or order status updates.
3. Controller uses `ApplicationDbContext` directly.
4. Changes are saved in one request.

## Authentication and Authorization Flow

Authentication setup is handled in [Program.cs](/d:/Projects/DSJsBookStore/Program.cs).

Authorization behavior:

- general user features like cart, wishlist, reviews, and order history require login
- admin features require role `Admin`
- `AdminClaimsTransformation` also grants the admin role claim to `admin@gmail.com` at runtime

Flow:

`Login -> Auth cookie -> Claims principal -> Optional admin claim transform -> Authorized controller/action`

## Simplified End-to-End Process Flow

```text
User/Admin
   |
   v
Browser Request
   |
   v
Controller
   |
   +--> Repository layer
   |      |
   |      v
   |   ApplicationDbContext
   |      |
   |      v
   |    SQLite
   |
   +--> Sometimes direct DbContext access
   |
   v
ViewModel / DTO / JSON
   |
   v
Razor View Response
```

## Important Notes From The Current Code

- `ReportRepository` is a placeholder and does not yet calculate top-selling books.
- Some business logic is in repositories, but analytics, bulk operations, and reviews use `ApplicationDbContext` directly.
- The seed process creates a default admin user on startup.
- Email sending is currently a no-op implementation.
- Reviews require admin approval before appearing on book details pages.

## Key Files To Start Reading

- [Program.cs](/d:/Projects/DSJsBookStore/Program.cs)
- [ApplicationDbContext.cs](/d:/Projects/DSJsBookStore/Data/ApplicationDbContext.cs)
- [DbSeeder.cs](/d:/Projects/DSJsBookStore/Data/DbSeeder.cs)
- [HomeRepository.cs](/d:/Projects/DSJsBookStore/Repositories/HomeRepository.cs)
- [CartRepository.cs](/d:/Projects/DSJsBookStore/Repositories/CartRepository.cs)
- [UserOrderRepository.cs](/d:/Projects/DSJsBookStore/Repositories/UserOrderRepository.cs)
- [BookController.cs](/d:/Projects/DSJsBookStore/Controllers/BookController.cs)
- [CartController.cs](/d:/Projects/DSJsBookStore/Controllers/CartController.cs)
- [AdminOperationsController.cs](/d:/Projects/DSJsBookStore/Controllers/AdminOperationsController.cs)

## How To Run

```powershell
dotnet restore
dotnet run
```

Default database:

- `bookshop.db`

Default seeded admin:

- email: `admin@gmail.com`
- password: `Admin@123`
