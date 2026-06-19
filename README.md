# QuizHub - Live Quiz Platform

QuizHub is a modern web application for organizing and participating in live, real-time quizzes.

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (version 18+)
- SQL Server (Express, Developer, or LocalDB edition all work)
- Visual Studio 2022 — recommended, since this is a multi-project solution and Visual Studio handles the database migration step far more reliably than the CLI (see step 3)

### Project structure

The backend is split across four projects:

- `QuizHub.Api` — the host application. Contains `appsettings.json` and the connection string. This is the **startup project**.
- `QuizHub.Data` — contains the EF Core `DbContext`, entity models, and migrations.
- `QuizHub.Services` — business logic.
- `QuizHub.Shared` — shared DTOs.

This matters because EF Core commands need to know about both `QuizHub.Data` (where the migrations live) and `QuizHub.Api` (where the connection string lives) at the same time — see step 3 below.

Throughout this guide, **📍 marks which folder your terminal needs to be in** before running a command.

### 1. Clone the repository

📍 *Any folder where you want the project to live.*

```bash
git clone https://github.com/VojinVelimirovic/QuizHub.git
cd QuizHub
```

After cloning, locate the `.sln` file inside the cloned folder — this is the same file you'd double-click to open the project in Visual Studio. The folder containing it is referred to below as the **solution folder**, and the steps that need it will say so explicitly.

### 2. Configure the database connection

Open `QuizHub.Api/appsettings.json` (no terminal needed — open it in your editor/IDE) and update the connection string to point to your own SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=QuizHubDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**Finding your server name** depends on what kind of SQL Server you have installed:

- **LocalDB** (a common default if SQL Server came bundled with Visual Studio): open a terminal (📍 any folder — this command doesn't care where you run it from) and run:
  ```bash
  sqllocaldb info
  ```
  This lists your LocalDB instance name — usually `MSSQLLocalDB`. Your server name is then `(localdb)\MSSQLLocalDB`.
- **SQL Server Express / Developer / full install**: open **SQL Server Configuration Manager** (search for it in the Windows Start menu) → **SQL Server Services**. The name in parentheses next to "SQL Server" is your instance name (e.g. `.\SQLEXPRESS`).
- Alternatively, if you have SQL Server Management Studio (SSMS) installed, its connection dialog usually auto-detects the right value.

> ⚠️ **Backslash trap:** JSON requires backslashes to be escaped, so `appsettings.json` needs `(localdb)\\MSSQLLocalDB` (double backslash). But when you type a server name directly into a terminal command (steps 3 and 6 below), use a **single** backslash: `(localdb)\MSSQLLocalDB`. Copy-pasting the JSON version into a terminal will fail with a "server not found" error.

### 3. Apply database migrations

**Recommended: Visual Studio Package Manager Console**

This solution has separate projects for the API host and the database layer, so running EF Core commands needs a bit of extra context that Visual Studio handles automatically:

1. Open the `.sln` file in Visual Studio.
2. In Solution Explorer, right-click `QuizHub.Api` → **Set as Startup Project** (it should already be set this way, but confirm).
3. Go to **Tools → NuGet Package Manager → Package Manager Console**.
4. At the top of the Package Manager Console panel, find the **"Default project"** dropdown and select `QuizHub.Data`.
5. Run:
   ```powershell
   Update-Database
   ```
   This creates the database and applies all migrations.

If `Update-Database` isn't recognized, run this once first (with `QuizHub.Data` still selected as the default project), then retry:

```powershell
Install-Package Microsoft.EntityFrameworkCore.Tools
```

**Alternative: command line (no Visual Studio)**

📍 *Solution folder (the one containing `QuizHub.Api`, `QuizHub.Data`, `QuizHub.Services`, `QuizHub.Shared` as subfolders — see step 1).*

```bash
dotnet tool install --global dotnet-ef
dotnet ef database update --project QuizHub.Data\QuizHub.Data.csproj --startup-project QuizHub.Api\QuizHub.Api.csproj
```

### 4. Trust the local HTTPS development certificate (first time only)

📍 *Any folder.*

```bash
dotnet dev-certs https --trust
```

### 5. Start the backend

📍 *Navigate into the `QuizHub.Api` folder (inside the solution folder from step 1).*

```bash
dotnet run --launch-profile https
```

> The `--launch-profile https` flag matters: running plain `dotnet run` without it uses a different default profile that only binds to `http://localhost:5123`, not `https://localhost:7208`.

The backend will be available at:
- `https://localhost:7208` (and also `http://localhost:5123`)
- Swagger UI: `https://localhost:7208/swagger`

> Alternatively, in Visual Studio, use the dropdown next to the green Run button to select the **https** profile (not **IIS Express**), then run with F5 / Ctrl+F5.

**Leave this terminal open and running** — it needs to stay active for the rest of the setup.

**Troubleshooting — "address already in use":** This means a previous `dotnet run` process is still running and holding the port. Find and stop it with:
```bash
netstat -ano | findstr :5123
taskkill /PID <pid_from_above> /F
```

### 6. Create an admin account (one-time setup)

There's no admin signup option in the UI — every account registers with the default `User` role, so the first admin has to be created manually. Do this now, before starting the frontend.

1. With the backend still running (step 5), open `https://localhost:7208/swagger` in your browser.
2. Find the registration endpoint (under the Users/Auth section), click **Try it out**, fill in a username/email/password, and click **Execute**. This creates the account directly — no frontend required.
3. Promote that account to admin.

   📍 *Open a **new, second terminal** — don't reuse the one running the backend. Any folder is fine; `sqlcmd` doesn't care about your working directory. This is a one-time command — close this terminal once you're done with it.*

   ```bash
   sqlcmd -S YOUR_SERVER_NAME -d YOUR_DATABASE_NAME -E -Q "UPDATE Users SET Role = 'Admin' WHERE Username = 'your_username';"
   ```

   - `-S` is your SQL Server instance name from step 2 (remember: single backslash here, e.g. `(localdb)\MSSQLLocalDB`)
   - `-d` is your database name
   - `-E` uses your Windows login (matches `Trusted_Connection=True`)
   - Replace `your_username` with the account you just registered

   To confirm it worked:

   ```bash
   sqlcmd -S YOUR_SERVER_NAME -d YOUR_DATABASE_NAME -E -Q "SELECT Username, Role FROM Users;"
   ```

   If `sqlcmd` isn't recognized as a command, it isn't installed/on PATH. As a fallback, use Visual Studio: **View → SQL Server Object Explorer** → connect to your server → expand your database → `Tables` → right-click `Users` → **View Data**, and edit the `Role` cell directly.

You can now close that second terminal — it was only needed for this one command.

### 7. Start the frontend

📍 *Open a new, third terminal. Navigate into the `quizhub-frontend` folder (inside the solution folder from step 1). Leave the backend terminal from step 5 running — don't close it.*

```bash
npm install
npm run dev
```

The frontend will be available at: `http://localhost:5173`

> From here on, you only need two terminals running at once: backend (step 5) and frontend (this step) — the same as any typical full-stack project. The SQL terminal from step 6 is no longer needed.

### 8. Verify the setup

- Open `http://localhost:5173` in your browser and log in with the admin account you created in step 6
- Register a second, regular account
- Create a quiz, then open it in a second browser tab (or incognito window, logged in as the other account) to test joining and playing live

## Frontend Scripts

- `npm run dev` — Start development server
- `npm run build` — Build for production
- `npm run preview` — Preview production build

## Features

- User registration & authentication
- Quiz creation, management, and playing
- Quiz result leaderboards
- Live multiplayer quiz rooms

## License

This project is licensed under the MIT License — see the [LICENSE](./LICENSE) file for details.
