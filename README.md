# Personal Finance Tracker

A full-stack personal finance and expense tracking application built with .NET 8 Minimal API and React.

## Features

- **Categories Management**: Create, view, and delete expense/income categories
- **Transaction Tracking**: Record income and expenses with categories, dates, and notes
- **Dashboard**: Visual charts showing monthly expense distribution and financial summary
- **User Scoping**: All data is scoped to individual users (currently using header-based auth)
- **Responsive Design**: Modern, mobile-friendly UI

## Tech Stack

### Backend
- .NET 8 Minimal API
- Entity Framework Core 8
- SQL Server (LocalDB)
- Clean Architecture (Domain/Infrastructure/API layers)

### Frontend
- React 18 + TypeScript
- Vite build tool
- Recharts for data visualization
- Day.js for date handling
- Axios for API communication

## Project Structure

```
finance-tracker/
├── FinanceTracker.sln
├── FinanceTracker.Api/          # Web API project
├── FinanceTracker.Domain/        # Domain models and entities
├── FinanceTracker.Infrastructure/# EF Core and data access
└── frontend/                    # React frontend application
```

## Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full instance)
- Node.js 18+ and npm
- Visual Studio 2022 or VS Code

## Setup Instructions

### Backend Setup

1. **Clone and navigate to the project**
   ```bash
   cd WhereDoUSpend
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update connection string** (if needed)
   Edit `FinanceTracker.Api/appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=.;Database=FinanceTracker;Trusted_Connection=True;TrustServerCertificate=True"
     }
   }
   ```

4. **Create and update database**
   ```bash
   dotnet ef migrations add Init --project FinanceTracker.Infrastructure --startup-project FinanceTracker.Api
   dotnet ef database update --project FinanceTracker.Infrastructure --startup-project FinanceTracker.Api
   ```

5. **Run the API**
   ```bash
   dotnet run --project FinanceTracker.Api
   ```
   
   The API will be available at:
   - HTTPS: https://localhost:5001
   - HTTP: http://localhost:5000
   - Swagger UI: https://localhost:5001/swagger

### Frontend Setup

1. **Navigate to frontend directory**
   ```bash
   cd frontend
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start development server**
   ```bash
   npm run dev
   ```
   
   The frontend will be available at http://localhost:5173

## API Endpoints

### Authentication
All endpoints use header-based authentication with `x-user-id` header. Default user is "demo".

### Categories
- `GET /api/categories` - List user categories
- `POST /api/categories` - Create new category
- `DELETE /api/categories/{id}` - Delete category

### Transactions
- `GET /api/transactions` - List transactions (with optional date filters)
- `POST /api/transactions` - Create new transaction
- `DELETE /api/transactions/{id}` - Delete transaction

### Reports
- `GET /api/reports/summary?year={year}&month={month}` - Monthly summary by category

## Usage

1. **Start both backend and frontend**
2. **Create categories** for your expenses (e.g., Food, Travel, Bills)
3. **Add transactions** with amounts, types (Income/Expense), dates, and categories
4. **View dashboard** to see expense distribution and financial summary
5. **Monitor your spending** patterns over time

## Demo Data

The application comes with sample data for the "demo" user:
- Categories: Food, Travel, Bills, Salary, Entertainment, Shopping
- Sample transactions for demonstration

## Development

### Adding New Features
- **Backend**: Add new endpoints in `Program.cs`, create DTOs in `Models/` folder
- **Frontend**: Add new components and update the API client in `src/api.ts`

### Database Changes
- Modify entities in `FinanceTracker.Domain/`
- Update `AppDbContext` in `FinanceTracker.Infrastructure/`
- Create new migration: `dotnet ef migrations add MigrationName`
- Update database: `dotnet ef database update`

## Future Enhancements

- **Authentication**: Replace header-based auth with JWT + ASP.NET Identity
- **Validation**: Add FluentValidation for DTOs
- **AI Features**: Category suggestions based on transaction amounts and notes
- **Advanced Reports**: Yearly summaries, trend analysis, budget tracking
- **Mobile App**: React Native or PWA version

## Troubleshooting

### Common Issues

1. **Database Connection Error**
   - Ensure SQL Server is running
   - Check connection string in `appsettings.json`
   - Verify LocalDB is installed and running

2. **CORS Errors**
   - Backend CORS is configured for `http://localhost:5173`
   - Ensure frontend is running on the correct port

3. **Build Errors**
   - Run `dotnet restore` to restore packages
   - Check .NET version compatibility

4. **Frontend Build Issues**
   - Delete `node_modules` and run `npm install`
   - Ensure Node.js version is compatible

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## License

This project is open source and available under the MIT License. 