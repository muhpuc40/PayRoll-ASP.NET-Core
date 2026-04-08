# 💼 Payroll Management System API

> A comprehensive payroll management REST API built with **.NET Core 8**, handling employee management, attendance tracking, salary calculations, and payslip generation.

![.NET](https://img.shields.io/badge/.NET_Core-8.0+-512BD4?style=flat&logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-8.0+-512BD4?style=flat&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-2019+-CC2927?style=flat&logo=microsoftsqlserver&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-OpenAPI_3.0-85EA2D?style=flat&logo=swagger&logoColor=black)
![JWT](https://img.shields.io/badge/Auth-JWT_Bearer-000000?style=flat&logo=jsonwebtokens&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=flat)

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 👤 Employee Management | Full CRUD for employee records |
| 🏢 Department Management | Organize employees with department-level base salaries |
| 📊 Attendance Tracking | Daily records with Present / Absent / Late status |
| 💰 Allowance Management | Global, department-level, or per-employee allowances |
| 🎁 Bonus Management | Fixed or percentage-based bonuses |
| 📉 Deduction Management | Configurable deduction types |
| 📏 Salary Rules | Automated penalty calculation based on attendance |
| 📄 Payslip Generation | Individual & bulk payslips with approval workflow |
| 🔐 Authentication & Authorization | Role-based JWT authentication |

---

## ⚙️ Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server
- Visual Studio 2022 / VS Code

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/yourusername/payroll-api.git
cd payroll-api
```

2. **Update database connection string**

   Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=PayrollDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

3. **Run database migrations**
```bash
dotnet ef database update
```

4. **Run the application**
```bash
dotnet run
```

5. **Access Swagger UI**
```
https://localhost:5001/swagger
```

---

## 🔐 Authentication

All endpoints (except `/api/Auth/login`) require JWT Bearer authentication.

### Login

```http
POST /api/Auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "username": "admin",
  "role": "Admin",
  "mustChangePassword": false
}
```

Include the token in subsequent requests:
```
Authorization: Bearer {your_token}
```

---

## 📡 API Endpoints

### 🔑 Auth

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Auth/login` | Authenticate user and get JWT token |
| PUT | `/api/Auth/change-password` | Change user password |

### 👤 Employee

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Employee` | Get all employees |
| GET | `/api/Employee/{id}` | Get employee by ID |
| POST | `/api/Employee` | Create new employee |
| PUT | `/api/Employee/{id}` | Update employee |
| DELETE | `/api/Employee/{id}` | Delete employee |

### 🏢 Department

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Department` | Get all departments |
| GET | `/api/Department/{id}` | Get department by ID |
| POST | `/api/Department` | Create new department |
| PUT | `/api/Department/{id}` | Update department |
| DELETE | `/api/Department/{id}` | Delete department |

### 📊 Attendance

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Attendance/{employeeId}` | Get attendance by employee (optional: month, year) |
| GET | `/api/Attendance/summary/{employeeId}` | Get attendance summary |
| POST | `/api/Attendance` | Record single attendance |
| POST | `/api/Attendance/bulk` | Record bulk attendance |
| PUT | `/api/Attendance/{id}` | Update attendance record |
| DELETE | `/api/Attendance/{id}` | Delete attendance record |

### 💰 Allowance

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Allowance` | Get all allowances |
| GET | `/api/Allowance/{id}` | Get allowance by ID |
| POST | `/api/Allowance` | Create allowance |
| PUT | `/api/Allowance/{id}` | Update allowance |
| DELETE | `/api/Allowance/{id}` | Delete allowance |

### 🎁 Bonus

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Bonus` | Get all bonuses |
| GET | `/api/Bonus/{id}` | Get bonus by ID |
| POST | `/api/Bonus` | Create bonus |
| PUT | `/api/Bonus/{id}` | Update bonus |
| DELETE | `/api/Bonus/{id}` | Delete bonus |

### 📉 Deduction

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Deduction` | Get all deductions |
| GET | `/api/Deduction/{id}` | Get deduction by ID |
| POST | `/api/Deduction` | Create deduction |
| PUT | `/api/Deduction/{id}` | Update deduction |
| DELETE | `/api/Deduction/{id}` | Delete deduction |

### 📏 Salary Rule

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/SalaryRule` | Get all salary rules |
| GET | `/api/SalaryRule/{id}` | Get salary rule by ID |
| POST | `/api/SalaryRule` | Create salary rule |
| PUT | `/api/SalaryRule/{id}` | Update salary rule |
| DELETE | `/api/SalaryRule/{id}` | Delete salary rule |
| PATCH | `/api/SalaryRule/{id}/toggle` | Toggle rule active status |

### 📄 Payslip

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Payslip` | Get all payslips (filter by month/year) |
| GET | `/api/Payslip/summary` | Get payslip summary |
| GET | `/api/Payslip/{id}` | Get payslip by ID |
| GET | `/api/Payslip/employee/{employeeId}` | Get employee payslips |
| POST | `/api/Payslip/generate` | Generate single payslip |
| POST | `/api/Payslip/generate-all` | Generate bulk payslips |
| PUT | `/api/Payslip/{id}/approve` | Approve payslip |
| PUT | `/api/Payslip/{id}/pay` | Mark payslip as paid |
| DELETE | `/api/Payslip/{id}` | Delete payslip |

---

## 🔒 Security

This API uses JWT Bearer authentication. Include the token in the Authorization header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Default Roles

| Role | Access |
|------|--------|
| Admin | Full system access |
| Manager | Manage employees, attendance, and generate payslips |
| Employee | View-only access to own data and payslips |

---

## 📝 License

MIT License

---

<p align="center">Built with ❤️ by Minhaj Uddin Hassan</p>
<p align="center">© 2026 Payroll Management System. All rights reserved.</p>
