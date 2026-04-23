# DSJ's Book Store

DSJ's Book Store is an ASP.NET Core MVC web application for managing and selling books online. The project includes customer-facing shopping features, admin management tools, reporting, analytics, and supporting modules such as authentication, stock tracking, reviews, and wishlist management.

## Project Overview

This project is built as a complete online bookstore system where users can browse books, search by different criteria, add items to cart, place orders, manage wishlists, and review purchased books. On the admin side, the system supports book management, genre management, stock control, bulk operations, order handling, reports, and analytics dashboards.

## Main Features

### Customer Features

- Browse books from the home page
- View book details
- Search books by title, genre, author, and price range
- Add books to cart
- Checkout and place orders
- View personal order history
- Add books to wishlist
- Submit and view reviews
- Access recommendations page
- Manage user profile

### Admin Features

- Add, update, and manage books
- Manage genres
- Manage stock quantities
- View all orders
- Update order status
- Use bulk operations
- View reports
- Access analytics dashboard
- Monitor low-stock items

### Supporting Features

- ASP.NET Core Identity authentication
- Role-based authorization
- SQLite database integration
- Seeded default data at startup
- Email sender service support
- Admin claims transformation

## Modules Included

The current project contains the following main modules:

- `Home`
- `Book`
- `Genre`
- `Author`
- `Cart`
- `Wishlist`
- `Review`
- `Search`
- `Recommendations`
- `UserOrder`
- `Profile`
- `Newsletter`
- `Contact`
- `About`
- `Stock`
- `Reports`
- `Analytics`
- `AdminOperations`
- `BulkOperations`

## Tech Stack

- `ASP.NET Core MVC`
- `ASP.NET Core Identity`
- `Entity Framework Core`
- `SQLite`
- `Razor Views`
- `C#`
- `Bootstrap`

## Project Structure

```text
DSJsBookStore/
|-- Areas/
|   |-- Identity/
|-- Constants/
|-- Controllers/
|-- Data/
|-- Migrations/
|-- Models/
|   |-- DTOs/
|   |-- ViewModels/
|-- Repositories/
|-- Shared/
|-- Views/
|-- wwwroot/
|-- Program.cs
|-- appsettings.json
|-- DSJsBookStore.csproj
```

## Important Controllers

- `BookController` for book management and display
- `CartController` for shopping cart and checkout
- `SearchController` for filtering and search
- `WishlistController` for wishlist management
- `ReviewController` for review-related actions
- `UserOrderController` for user order history
- `StockController` for inventory handling
- `ReportsController` for reporting views
- `AnalyticsController` for admin analytics dashboard
- `AdminOperationsController` for overall admin order operations

## Database and Storage

The application uses SQLite as the main database.

- Connection string: `Data Source=bookshop.db`
- EF Core migrations are included in the `Migrations` folder
- Startup seeding is handled from `Data/DbSeeder.cs`

## Authentication and Authorization

The application uses ASP.NET Core Identity for authentication and user management.

- User registration and login are enabled
- Identity UI is included under `Areas/Identity`
- Role-based protection is used for admin-only pages
- Claims transformation is used for admin role handling

## How to Run

### Prerequisites

- .NET SDK compatible with `net10.0`
- Visual Studio or VS Code

### Run Steps

1. Clone or open the project folder.
2. Restore dependencies.
3. Run the project.

Example commands:

```powershell
dotnet restore
dotnet run
```

Default launch profiles include:

- `http://localhost:5000`
- `https://localhost:7158`
- `http://localhost:5232`

## Repository Pattern

This project uses repositories to separate data access logic from controllers.

Main repositories include:

- `HomeRepository`
- `BookRepository`
- `CartRepository`
- `GenreRepository`
- `StockRepository`
- `ReportRepository`
- `UserOrderRepository`
- `WishlistRepository`

## Analytics and Reporting

The project already includes business insight features such as:

- total sales summary
- total orders count
- total customers count
- total books count
- monthly sales analysis
- top-selling books
- recent orders
- low-stock alerts

## Search and Recommendation

The bookstore also includes intelligent browsing-related modules:

- Search page with title, author, genre, and price filtering
- Recommendation page for featured or suggested books
- Review-based user feedback support

## Configuration

Important configuration files:

- `Program.cs` for dependency injection and middleware setup
- `appsettings.json` for database and app configuration
- `Properties/launchSettings.json` for local launch profiles

## Notes

- The app seeds default data during startup.
- Development login does not require confirmed email.
- Static files such as book images are stored under `wwwroot/images`.

## Future Improvement Ideas

- Online payment integration
- Better personalized recommendation logic
- Advanced search ranking
- Review sentiment analysis
- Admin export features
- Dashboard charts with more KPIs
- Email notification workflows

## Author

This repository contains the DSJ's Book Store academic/project implementation built with ASP.NET Core MVC.
