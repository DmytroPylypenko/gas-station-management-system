# Gas Station & Cafe Management System

A robust full-stack web application developed on the ASP.NET Core framework. This system streamlines retail operations for gas stations, integrating fuel sales, cafe order processing, and inventory management into a unified digital workspace.

![Build Status](https://img.shields.io/badge/build-passing-brightgreen)
![Platform](https://img.shields.io/badge/platform-.NET%208.0-blue)
![Docker](https://img.shields.io/badge/container-docker-blue)
![License](https://img.shields.io/badge/license-MIT-green)

## Project Overview

The primary goal of this project is to digitize the workflow of a gas station service point. It implements a Role-Based Access Control (RBAC) system to serve three distinct operational roles: Customers, Service Staff (Baristas/Cashiers), and Administrators.

The solution is architected using the **Model-View-Controller (MVC)** pattern and follows **Clean Architecture** principles, ensuring scalability and maintainability.

## Technology Stack

### Backend
* **Framework:** ASP.NET Core 8.0 (C#)
* **ORM:** Entity Framework Core (Code-First approach)
* **Database:** Microsoft SQL Server 2022
* **Authentication:** ASP.NET Core Identity
* **Email Service:** MailKit (SMTP)
* **Testing:** xUnit

### Frontend
* **Framework:** ASP.NET Core Razor Views
* **Styling:** Bootstrap 5 (Custom Corporate Theme)
* **Scripting:** JavaScript, jQuery
* **Notifications:** SweetAlert2

### Infrastructure
* **Containerization:** Docker (for SQL Server database)
* **Localization:** Fully localized for the `uk-UA` region (Currency: UAH, Date format: dd.MM.yyyy)

## Functional Modules

### 1. Customer Module
* **Product Catalog:** Categorized view for Fuel and Food items.
* **Shopping Cart:** Session-based persistence with granular quantity selection (liters vs. units).
* **Order Processing:** Secure checkout workflow converting session data to database transactions.
* **Order History:** User dashboard for tracking past purchases and status updates.

### 2. Staff Workstation
* **Cashier Dashboard:**
    * Sales register view.
    * Manual order completion (e.g., for fuel-only transactions).
    * Receipt generation and printing.
    * Inventory monitoring with low-stock indicators.
* **Barista Panel:**
    * Real-time queue for food preparation.
    * Status workflow: `New` -> `Processing` -> `Completed`.
    * Automated filtering (hides fuel-only orders).

### 3. Administration
* **System Dashboard:** Real-time analytics on revenue, active orders, and user base.
* **Product Management:** CRUD operations with validation.
* **User Management:** Interface for assigning and revoking roles (Admin, Cashier, Barista).

## Installation and Setup

### Prerequisites
Ensure the following tools are installed on your local machine:
* .NET 8.0 SDK
* Docker Desktop (or Docker Engine on Linux)
* Git

### Step 1: Clone the Repository
```bash
git clone [https://github.com/YOUR_USERNAME/GasStationManagementSystem.git](https://github.com/YOUR_USERNAME/GasStationManagementSystem.git)
cd GasStationManagementSystem
```

### Step 2: Infrastructure Setup
Start the SQL Server container using Docker. This ensures a consistent database environment.

```bash
sudo docker run -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=StrongPassword123!" \
  -p 1433:1433 \
  --name sql_server_dev \
  -d [mcr.microsoft.com/mssql/server:2022-latest](https://mcr.microsoft.com/mssql/server:2022-latest)
```

### Step 3: Configuration
Create a file named `appsettings.Development.json` in the `GasStationSystem.Web` directory to store local secrets.

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=GasStationDB;User Id=sa;Password=StrongPassword123!;TrustServerCertificate=True;Encrypt=False"
  },
  "EmailSettings": {
    "Mail": "your-email@gmail.com",
    "DisplayName": "GasStation Security",
    "Password": "your-app-password",
    "Host": "smtp.gmail.com",
    "Port": 587
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Step 4: Database Initialization
Apply migrations to create the database schema. The application includes a Seeder that will populate initial data (Products, Roles, Users) automatically.

```bash
dotnet ef database update
```

### Step 5: Launch
Run the web application.

```bash
dotnet run --project GasStationSystem.Web
```

### Default Credentials for Testing
The system comes pre-configured with the following accounts for testing different roles:
| Role | Email | Password |
| :--- | :--- | :--- |
| **Administrator** | `admin@gas.com` | `Admin123!` |
| **Cashier** | `cashier@gas.com` | `Cashier123!` |
| **Barista** | `barista@gas.com` | `Barista123!` |

### Testing
To execute the unit test suite (validating Cart logic and Controller behavior):

```bash
dotnet test
```
