# WorkBotAI API Documentation

Welcome to the WorkBotAI API documentation. This document provides detailed information about the available REST endpoints, including request formats, response samples, and authentication details.

## Base URL
The API is available at: `https://www.geordie.app:441/WorkbotAI_API/api` (default development URL)

## Authentication
Most endpoints require a Bearer JWT token. Obtain a token by calling the `/Auth/login` endpoint.

**Header:**
`Authorization: Bearer <your_jwt_token>`

---

## 1. Authentication

### POST `/Auth/login`
Authenticates a user and returns a JWT token.

**Request Body:**
```json
{
  "email": "admin@example.com",
  "password": "YourSecurePassword!",
  "tenantId": "optional-guid-string"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "admin@example.com",
    "name": "Admin User",
    "role": "SuperAdmin",
    "isSuperAdmin": true,
    "tenantId": "7890abcd-1234-5678-90ab-cdef12345678",
    "tenantName": "Main Office"
  }
}
```

**Response (401 Unauthorized):**
```json
{
  "success": false,
  "error": "Password non valida"
}
```

---

## 2. Users

### GET `/Users`
Retrieves a list of users. Can be filtered by `tenantId`.

**Query Parameters:**
- `tenantId` (Guid, optional): Filter users by tenant.

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "userName": "admin",
      "mail": "admin@example.com",
      "firstName": "Admin",
      "lastName": "User",
      "roleName": "SuperAdmin",
      "isActive": true
    }
  ]
}
```

### GET `/Users/{id}`
Retrieves detailed information about a specific user.

**Response (200 OK):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "userName": "admin",
    "mail": "admin@example.com",
    "firstName": "Admin",
    "lastName": "User",
    "statusName": "Active",
    "isActive": true
  }
}
```

### POST `/Users`
Creates a new user.

**Request Body:**
```json
{
  "userName": "jdoe",
  "password": "Password123!",
  "mail": "jdoe@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "roleId": 2,
  "tenantId": "guid-id",
  "isActive": true,
  "isSuperAdmin": false
}
```

### PUT `/Users/{id}`
Updates an existing user.

### DELETE `/Users/{id}`
Deletes a user (soft delete). Note: SuperAdmins cannot be deleted.

### PUT `/Users/{id}/toggle-status`
Toggles the active status of a user.

### PUT `/Users/{id}/change-password`
Changes a user's password.

**Request Body:**
```json
{
  "newPassword": "NewSecurePassword123!"
}
```

---

## 3. Categories

### GET `/Categories`
Retrieves all categories.

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "Health", "isActive": true },
    { "id": 2, "name": "Beauty", "isActive": true }
  ]
}
```

### POST `/Categories`
Creates a new category.

**Request Body:**
```json
{
  "name": "Fitness",
  "isActive": true
}
```

---

## 4. JobTypes

### GET `/JobTypes`
Retrieves all job types.

**Response (200 OK):**
```json
{
  "success": true,
  "data": [
    { "id": 1, "name": "Doctor", "isActive": true, "gender": "U", "categoryId": 1 }
  ]
}
```

### POST `/JobTypes/bulk`
Creates multiple job type records based on gender flags.

**Request Body:**
```json
{
  "descrizione": "Stylist",
  "m": true,
  "f": true,
  "u": false,
  "categoryId": 2,
  "userId": 1
}
```
*Logic: Generates one record for each true flag (e.g., Stylist (M) and Stylist (F)).*

---

## 5. Tenants (SuperAdmin Only)

### GET `/Tenants`
Retrieves all tenants.

### POST `/Tenants`
Creates a new tenant.

**Request Body:**
```json
{
  "name": "New Tenant",
  "acronym": "NT",
  "categoryId": 1
}
```

---

## 6. Appointments

### GET `/Appointments`
Retrieves appointments with optional filtering.

**Query Parameters:**
- `tenantId` (Guid, optional)
- `statusId` (int, optional)
- `fromDate` (DateTime, optional)
- `toDate` (DateTime, optional)

### POST `/Appointments`
Creates a new appointment.

**Request Body:**
```json
{
  "tenantId": "guid-id",
  "customerId": 1,
  "staffId": 1,
  "statusId": 1,
  "startTime": "2024-05-20T10:00:00Z",
  "endTime": "2024-05-20T11:00:00Z",
  "note": "Initial consultation"
}
```

### GET `/Appointments/today`
Retrieves all appointments for the current day.

---

## 7. System Logs (SuperAdmin Only)

### GET `/SystemLogs`
Retrieves system logs with full request/response context.

---

## 8. Staff

### GET `/Staff`
Retrieves staff members with optional filtering.

**Query Parameters:**
- `tenantId` (Guid, optional)
- `search` (string, optional)
- `jobTypeId` (int, optional)
- `jobTypeGender` (string, optional: 'M', 'F', 'U')

### POST `/Staff`
Creates a new staff member.

**Request Body:**
```json
{
  "tenantId": "guid-id",
  "name": "Mario Rossi",
  "jobTypeId": 1
}
```

---

## 9. Customers

### GET `/Customers`
Retrieves customers for a specific tenant.

**Query Parameters:**
- `tenantId` (Guid, required)
- `search` (string, optional)

### POST `/Customers`
Creates a new customer.

**Request Body:**
```json
{
  "tenantId": "guid-id",
  "fullName": "Anna Bianchi",
  "phone": "+39 123 456 7890",
  "email": "anna@example.com"
}
```

---

## 10. Services

### GET `/Services`
Retrieves services offered by a tenant.

---

## 11. Health Check

### GET `/health` (Anonymous)
Returns the health status of the API and database connection.

**Response (200 OK):**
```json
{
  "status": "healthy",
  "database": "connected",
  "users": 15,
  "tenants": 5,
  "timestamp": "2024-02-19T10:30:00Z"
}
```
