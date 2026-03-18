# K53 Learners License Prep App

A full-stack web application for South African learners to prepare for the K53 theory test.

---

## Project Structure

```
K53PrepApp/
├── backend/                   ← C# ASP.NET Core Web API
│   ├── K53PrepApp.Api.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── Models/
│   │   ├── Question.cs
│   │   └── TestModels.cs       (Student, TestResult, TestAnswer)
│   ├── Data/
│   │   ├── AppDbContext.cs     (EF Core + SQLite)
│   │   └── SeedData.cs         (63 real K53 questions pre-loaded)
│   └── Controllers/
│       ├── QuestionsController.cs
│       └── StudentsController.cs  (also contains TestsController)
│
└── frontend/                  ← Plain HTML + Tailwind + JS
    ├── index.html              (Student landing + admin login)
    ├── study.html              (Browse & learn all questions)
    ├── test.html               (Timed 64-question practice test)
    ├── results.html            (Test results & insights)
    ├── admin-dashboard.html    (Admin overview + recent tests)
    ├── admin-questions.html    (Add / edit / delete questions)
    ├── admin-students.html     (Student list + test history)
    └── js/
        └── api.js              (Shared API helper)
```

---

## Prerequisites

| Tool | Version | Download |
|------|---------|----------|
| .NET SDK | 8.0 or later | https://dotnet.microsoft.com/download |
| Visual Studio 2022 **or** VS Code | Any recent | https://visualstudio.microsoft.com |
| Live Server (VS Code extension) | Latest | From VS Code Marketplace |

---

## Step-by-Step Setup

### 1. Open the backend in Visual Studio or VS Code

**Visual Studio 2022:**
1. Open Visual Studio
2. Click **"Open a project or solution"**
3. Browse to `K53PrepApp/backend/` and open `K53PrepApp.Api.csproj`

**VS Code:**
1. Open VS Code
2. `File → Open Folder` → select the `K53PrepApp/backend/` folder

---

### 2. Run the backend API

**Visual Studio:**
- Press `F5` or click the green **Run** button
- The API will start on `http://localhost:5000`

**VS Code (Terminal):**
```bash
cd K53PrepApp/backend
dotnet run
```

You should see output like:
```
info: Now listening on http://localhost:5000
```

The database (`k53prep.db`) is created automatically on first run, and all 63 K53 questions are seeded. **Leave this terminal open** — it must keep running.

---

### 3. Open the frontend

**Option A — VS Code Live Server (recommended):**
1. Install the **Live Server** extension in VS Code
   (Search "Live Server" by Ritwick Dey in Extensions panel)
2. Open the `K53PrepApp/frontend/` folder in VS Code
3. Right-click `index.html` → **"Open with Live Server"**
4. Your browser opens at `http://127.0.0.1:5500/index.html`

**Option B — Just open the file:**
- Double-click `frontend/index.html` to open it directly in your browser
- This works for most features, but the browser may block API calls
- Live Server is preferred to avoid this

---

### 4. Using the app

**As a Student:**
1. Open `index.html`
2. Enter your name and phone number (e.g. `John Doe - 082 123 4567`)
3. Click **Study Mode** to browse all questions category by category
4. Click **Take Practice Test** to start a timed 64-question exam
5. After submitting, you are taken to the **Results & Insights** page

**As an Admin:**
1. Click the 🔒 **Admin Access** button (top-right corner of the landing page)
2. Enter the admin code: **`admin1234`**
   *(You can change this in `backend/appsettings.json` → `"AdminCode"`)*
3. You'll be redirected to the Admin Dashboard with three sections:
   - **Overview** — stats and recent test results
   - **Manage Questions** — add, edit, or delete questions
   - **Student Activity** — see all students and their test history

---

## API Endpoints Reference

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/questions` | All active questions (optional `?category=Rules`) |
| GET | `/api/questions/test` | Shuffled test set (28 Rules + 28 Signs + 8 Controls) |
| GET | `/api/questions/{id}/answer` | Get correct answer + explanation |
| POST | `/api/questions` | *(Admin)* Create question |
| PUT | `/api/questions/{id}` | *(Admin)* Update question |
| DELETE | `/api/questions/{id}` | *(Admin)* Soft-delete question |
| POST | `/api/students/identify` | Create or retrieve student by name+phone |
| GET | `/api/students` | *(Admin)* List all students |
| GET | `/api/students/{id}/results` | *(Admin)* Student test history |
| POST | `/api/tests/submit` | Submit a completed test, returns full result |
| GET | `/api/tests/{id}` | Get a saved test result |
| GET | `/api/tests/admin/all` | *(Admin)* All test results |

Admin endpoints require the header: `X-Admin-Code: admin1234`

---

## K53 Test Passing Requirements

| Category | Questions | Minimum to Pass |
|----------|-----------|-----------------|
| Rules of the Road | 28 | 22 (79%) |
| Road Signs | 28 | 22 (79%) |
| Vehicle Controls | 8 | 6 (75%) |
| **Total** | **64** | **All 3 categories must pass** |

---

## Customising & Experimenting

### Change the admin password
Edit `backend/appsettings.json`:
```json
"AdminCode": "your-new-password"
```

### Add more questions
- Use the Admin → Manage Questions UI, or
- Add directly to `backend/Data/SeedData.cs` and re-run

### Change the API port
Edit `backend/Properties/launchSettings.json` (created by dotnet on first run) or add to `appsettings.json`:
```json
"Urls": "http://localhost:5001"
```
Then update the `API_BASE` variable in `frontend/js/api.js`.

### View the SQLite database
Install **DB Browser for SQLite** (https://sqlitebrowser.org) and open `backend/k53prep.db`.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Frontend | HTML5, Tailwind CSS (CDN), Vanilla JavaScript |
| Backend | C# ASP.NET Core 8 Web API |
| Database | SQLite via Entity Framework Core |
| ORM | EF Core (code-first, auto-migrated) |

---

## Troubleshooting

**"Failed to connect to server"**
→ Make sure `dotnet run` is running in the backend folder.

**CORS errors in browser console**
→ Use Live Server instead of opening the HTML file directly.

**Database errors on startup**
→ Delete `backend/k53prep.db` and restart — it will be recreated with seed data.

**Port already in use**
→ Change the port in `appsettings.json` and update `api.js` accordingly.
