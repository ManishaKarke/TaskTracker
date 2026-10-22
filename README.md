# Project Task Tracker

A full-stack task management application built with ASP.NET Core Web API (.NET 8) and Angular 17. It allows a team to manage projects and track tasks within each project.

---

## What This App Does

- **Project Management**: Create, edit, view, and delete projects. The projects list shows the task count for each project. Deleting a project automatically deletes all its tasks.
- **Task Management**: View tasks under each project with server-side pagination, debounced title search, filtering by status and priority, and sorting by due date, priority, or creation date.
- **Quick Status Updates**: Change task status (Todo, In Progress, Done) directly from the list table.
- **Concurrency & Validation**: Full validation on both client and server sides, with optimistic concurrency check (`RowVersion`) on task updates to prevent conflicting edits.

---

## Tech Stack

- **Backend**: ASP.NET Core 8 Web API, Entity Framework Core 8, SQL Server
- **Frontend**: Angular 17, TypeScript, Reactive Forms, RxJS
- **API Documentation**: Swagger / OpenAPI

---

## Getting Started

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 or v20 recommended) and npm
- SQL Server (LocalDB, Express, or standard instance)
- EF Core CLI tool (`dotnet tool install --global dotnet-ef` if not already installed)

---

### 2. Database Setup

1. Open `Backend/TaskTracker/appsettings.json` and configure your SQL Server connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=YOUR_SERVER_NAME;Initial Catalog=ProjectTaskTracker;Integrated Security=True;Encrypt=False"
   }
   ```
2. Apply the EF Core migrations from the `Backend` directory:
   ```bash
   cd Backend
   dotnet ef database update --project TaskTracker.DataAccess --startup-project TaskTracker
   ```

---

### 3. Running the Backend API

1. Navigate to the API project folder:
   ```bash
   cd Backend/TaskTracker
   ```
2. Start the API:
   ```bash
   dotnet run
   ```
3. The API will run at:
   - Base URL: `http://localhost:5238`
   - Swagger UI: `http://localhost:5238/swagger`

---

### 4. Running the Frontend (Angular)

1. Navigate to the frontend directory:
   ```bash
   cd Frontend/TaskTracker
   ```
2. Install dependencies:
   ```bash
   npm install
   ```
3. Check `src/environments/environment.ts` to ensure `apiBaseUrl` points to `http://localhost:5238/api`.
4. Start the development server:
   ```bash
   npm start
   ```
   (or `ng serve`)
5. Open your browser and navigate to `http://localhost:4200`.

---

## Assumptions Made

- **Database**: Used SQL Server with EF Core code-first migrations.
- **Unique Projects**: Project names must be unique across the system.
- **Task Validation**: Due dates cannot be set in the past when creating a new task.
- **Concurrency**: Managed via EF Core `RowVersion` / byte array. Conflicting updates return a 409 status code with an appropriate message.
- **Pagination**: Default page size is 10 items, capped at a maximum of 50 per page.

---

## What I Would Improve With More Time

If I had more time to expand this project, I would focus on:
1. **Automated Testing**: Add unit tests using xUnit and Moq for API controllers and services, plus Jasmine/Karma tests for Angular components.
2. **Authentication & Authorization**: Implement JWT-based authentication and user roles so team members can only edit their assigned tasks.
3. **Kanban Board View**: Add an interactive drag-and-drop board for moving tasks between Todo, In Progress, and Done.
4. **Enhanced UI Notifications**: Replace basic alert/confirm dialogs with toast notifications (e.g., ngx-toastr) for actions and errors.
5. **Docker Support**: Provide a `docker-compose.yml` file to spin up SQL Server, the API, and Angular in containers with a single command.
