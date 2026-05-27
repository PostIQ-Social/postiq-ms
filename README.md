# PostIQ Microservice

## Overview
PostIQ is a microservice-based application. This repository contains the source code for the services, starting with the **User** service.

## Prerequisites
*   **.NET 10 SDK** (Ensure `dotnet --version` returns 10.x)
*   **Docker** (For SQL Server)
*   **Homebrew** (Optional, for installing tools)

---

## 🚀 Getting Started

### 1. Database Setup
Start the SQL Server container:
```bash
docker run --cap-add SYS_PTRACE -e 'ACCEPT_EULA=1' -e 'MSSQL_SA_PASSWORD=Password123!' -p 1433:1433 --name postiq-sql -d mcr.microsoft.com/azure-sql-edge
```

Apply Migrations (from `PostIQ/src/Services/User/User.API`):
```bash
dotnet ef database update
```

**⚠️ Important: Seed Initial User**
The system requires a user to exist before syncing content. Connect to the database (using DBeaver, Azure Data Studio, or `sqlcmd`) and run:
```sql
INSERT INTO [User].[Users] (Guid, Email, IsActive, CreatedOn)
VALUES (NEWID(), 'test@example.com', 1, GETDATE());
```
*This creates a user with `UserId = 1`.*

### 2. Run the Application
Navigate to the API folder and run:
```bash
cd PostIQ/src/Services/User/User.API
dotnet run
```
The API will start on:
*   HTTP: `http://localhost:5255`
*   HTTPS: `https://localhost:7155`

---

## 📡 API Reference

### 1. Sync Medium Posts
Triggers a background job to fetch and store stories from a generic RSS feed (currently supports Medium).

*   **Endpoint:** `POST /api/profile/sync`
*   **Headers:** `Content-Type: application/json`

**Request Body:**
```json
{
  "userId": 1,
  "source": "Medium",
  "baseUrl": "https://medium.com/feed/@username"
}
```
*(Replace `@username` with your Medium handle, e.g., `@ev`)*

**Response:**
*   `202 Accepted`: The sync job has been queued in the background.

### 2. Get User Posts
Retrieves the list of synced posts for a user from the database.

*   **Endpoint:** `GET /api/profile/{userId}/posts`
    *   Example: `/api/profile/1/posts`

**Response:**
```json
[
  {
    "postId": 1,
    "userId": 1,
    "source": "Medium",
    "title": "My First Story",
    "link": "https://medium.com/...",
    "content": "...",
    "publishedDate": "2024-01-01T12:00:00",
    "categories": "tech,coding"
  }
]
```

---

## 🛠 Troubleshooting

**"The MERGE statement conflicted with the FOREIGN KEY constraint..."**
*   **Cause**: You are trying to sync posts for a `UserId` that doesn't exist in the `Users` table.
*   **Fix**: Run the **Seed Initial User** SQL script mentioned above.

**"SSL routines:OPENSSL_internal:WRONG_VERSION_NUMBER"**
*   **Cause**: You are hitting the **HTTP** port (`5255`) with an **HTTPS** request.
*   **Fix**: Use `http://localhost:5255` or switch to the HTTPS port (`7155`).