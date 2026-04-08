# Payroll Management System API Documentation

A comprehensive payroll management system API built with .NET Core. This system handles employee management, attendance tracking, salary calculations, allowances, bonuses, deductions, and payslip generation.

## 📋 Table of Contents

- [Features](#features)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Authentication](#authentication)
- [API Endpoints](#api-endpoints)
  - [Auth](#auth)
  - [Employee](#employee)
  - [Department](#department)
  - [Attendance](#attendance)
  - [Allowance](#allowance)
  - [Bonus](#bonus)
  - [Deduction](#deduction)
  - [Salary Rule](#salary-rule)
  - [Payslip](#payslip)
- [Data Models](#data-models)
- [Enums](#enums)
- [Security](#security)

---

## 🚀 Features

- **Employee Management**: CRUD operations for employee records
- **Department Management**: Organize employees by departments with base salaries
- **Attendance Tracking**: Record daily attendance with Present/Absent/Late status
- **Allowance Management**: Configure global, department-level, or individual allowances
- **Bonus Management**: Set up fixed or percentage-based bonuses
- **Deduction Management**: Define various deduction types
- **Salary Rules**: Automated penalty calculation based on attendance
- **Payslip Generation**: Generate individual or bulk payslips with approval workflow
- **Authentication & Authorization**: JWT-based secure authentication

---

## 💻 Technology Stack

| Technology | Version |
|------------|---------|
| .NET Core | 8.0+ |
| Entity Framework Core | 8.0+ |
| SQL Server | 2019+ |
| JWT Bearer Authentication | - |
| Swagger/OpenAPI | 3.0 |

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

---

### 👤 Employee

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Employee` | Get all employees |
| GET | `/api/Employee/{id}` | Get employee by ID |
| POST | `/api/Employee` | Create new employee |
| PUT | `/api/Employee/{id}` | Update employee |
| DELETE | `/api/Employee/{id}` | Delete employee |

**Create Employee Request:**
```json
{
  "username": "john.doe",
  "email": "john@company.com",
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+1234567890",
  "position": "Software Engineer",
  "departmentId": 1,
  "hireDate": "2024-01-15T00:00:00"
}
```

---

### 🏢 Department

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Department` | Get all departments |
| GET | `/api/Department/{id}` | Get department by ID |
| POST | `/api/Department` | Create new department |
| PUT | `/api/Department/{id}` | Update department |
| DELETE | `/api/Department/{id}` | Delete department |

**Create Department Request:**
```json
{
  "name": "Engineering",
  "baseSalary": 50000
}
```

---

### 📊 Attendance

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Attendance/{employeeId}` | Get attendance by employee (optional: month, year query params) |
| GET | `/api/Attendance/summary/{employeeId}` | Get attendance summary |
| POST | `/api/Attendance` | Record single attendance |
| POST | `/api/Attendance/bulk` | Record bulk attendance |
| PUT | `/api/Attendance/{id}` | Update attendance record |
| DELETE | `/api/Attendance/{id}` | Delete attendance record |

**Record Attendance Request:**
```json
{
  "employeeId": 1,
  "date": "2024-03-15T00:00:00",
  "status": "Present"
}
```

---

### 💰 Allowance

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Allowance` | Get all allowances |
| GET | `/api/Allowance/{id}` | Get allowance by ID |
| POST | `/api/Allowance` | Create allowance |
| PUT | `/api/Allowance/{id}` | Update allowance |
| DELETE | `/api/Allowance/{id}` | Delete allowance |

**Create Allowance Request:**
```json
{
  "name": "Housing Allowance",
  "amount": 5000,
  "scope": "Global",
  "departmentId": null,
  "employeeId": null
}
```

---

### 🎁 Bonus

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Bonus` | Get all bonuses |
| GET | `/api/Bonus/{id}` | Get bonus by ID |
| POST | `/api/Bonus` | Create bonus |
| PUT | `/api/Bonus/{id}` | Update bonus |
| DELETE | `/api/Bonus/{id}` | Delete bonus |

**Create Bonus Request:**
```json
{
  "name": "Performance Bonus",
  "type": "Percentage",
  "value": 10,
  "scope": "Individual",
  "employeeId": 1
}
```

---

### 📉 Deduction

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Deduction` | Get all deductions |
| GET | `/api/Deduction/{id}` | Get deduction by ID |
| POST | `/api/Deduction` | Create deduction |
| PUT | `/api/Deduction/{id}` | Update deduction |
| DELETE | `/api/Deduction/{id}` | Delete deduction |

---

### 📏 Salary Rule

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/SalaryRule` | Get all salary rules |
| GET | `/api/SalaryRule/{id}` | Get salary rule by ID |
| POST | `/api/SalaryRule` | Create salary rule |
| PUT | `/api/SalaryRule/{id}` | Update salary rule |
| DELETE | `/api/SalaryRule/{id}` | Delete salary rule |
| PATCH | `/api/SalaryRule/{id}/toggle` | Toggle rule active status |

**Create Salary Rule Request:**
```json
{
  "name": "Absent Penalty",
  "conditionType": "Absent",
  "threshold": 3,
  "penaltyDays": 1
}
```

---

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

**Generate Payslip Request:**
```json
{
  "employeeId": 1,
  "month": 3,
  "year": 2024
}
```

---

## 📦 Data Models

### Employee
| Field | Type | Description |
|-------|------|-------------|
| id | int | Primary key |
| userId | int | Foreign key to User |
| departmentId | int | Foreign key to Department |
| firstName | string | Employee first name |
| lastName | string | Employee last name |
| email | string | Employee email |
| phone | string | Contact number |
| position | string | Job position |
| hireDate | datetime | Date of joining |
| isActive | boolean | Active status |

### Department
| Field | Type | Description |
|-------|------|-------------|
| id | int | Primary key |
| name | string | Department name |
| baseSalary | decimal | Default base salary |
| createdAt | datetime | Creation timestamp |

### Attendance
| Field | Type | Description |
|-------|------|-------------|
| id | int | Primary key |
| employeeId | int | Foreign key to Employee |
| date | datetime | Attendance date |
| status | enum | Present/Absent/Late |
| createdAt | datetime | Record creation time |

### Payslip
| Field | Type | Description |
|-------|------|-------------|
| id | int | Primary key |
| employeeId | int | Foreign key to Employee |
| month | int | Month (1-12) |
| year | int | Year |
| baseSalary | decimal | Base salary amount |
| totalAllowances | decimal | Sum of allowances |
| totalBonuses | decimal | Sum of bonuses |
| totalDeductions | decimal | Sum of deductions |
| attendancePenalty | decimal | Penalty amount |
| netPayable | decimal | Final payable amount |
| status | enum | Draft/Approved/Paid |
| generatedAt | datetime | Generation timestamp |

---

## 🔢 Enums

### AllowanceScope / BonusScope / DeductionScope
| Value | Description |
|-------|-------------|
| Global | Applies to all employees |
| Department | Applies to specific department |
| Individual | Applies to specific employee |

### AttendanceStatus
| Value | Description |
|-------|-------------|
| Present | Employee was present |
| Absent | Employee was absent |
| Late | Employee arrived late |

### BonusType
| Value | Description |
|-------|-------------|
| Fixed | Fixed amount |
| Percentage | Percentage of base salary |

### PayslipStatus
| Value | Description |
|-------|-------------|
| Draft | Generated but not approved |
| Approved | Approved for payment |
| Paid | Payment completed |

### RuleConditionType
| Value | Description |
|-------|-------------|
| Absent | Penalty based on absence |
| Late | Penalty based on late arrivals |

---

## 🔒 Security

This API uses JWT Bearer authentication. Include the token in the Authorization header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Default Roles
- **Admin**: Full system access
- **Manager**: Can manage employees, attendance, and generate payslips
- **Employee**: View-only access to own data and payslips

---

## 📝 License

This project is licensed under the MIT License.

---

## 👨‍💻 Author

**Minhaj Uddin Hassan**
- Software Developer Intern
- Premier University

---

<p align="center">Built with ❤️ by Minhaj Uddin Hassan</p>
<p align="center">© 2024 Payroll Management System. All rights reserved.</p>