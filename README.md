# UserDashboard - Full-Stack Login Application

A complete full-stack web application featuring user registration, authentication, and a protected user dashboard. Built with React (frontend) and ASP.NET Core 8 (backend), backed by PostgreSQL.

## 🏗️ Architecture

```
┌─────────────────┐     HTTP/REST      ┌──────────────────────┐     EF Core     ┌──────────────┐
│  React Frontend │ ──────────────────► │  ASP.NET Core 8 API  │ ──────────────► │  PostgreSQL  │
│  (Vite + React) │ ◄────────────────── │  (JWT Auth + BCrypt) │ ◄────────────── │   Database   │
└─────────────────┘    JWT Tokens       └──────────────────────┘                 └──────────────┘
     Port 3000                                Port 5000                             Port 5432
```

### Frontend Structure
```
frontend/src/
├── components/
│   └── PrivateRoute.jsx    # JWT-based route guard
├── pages/
│   ├── Register.jsx        # Registration form with validation
│   ├── Login.jsx           # Login form with JWT storage
│   └── UserDetails.jsx     # Protected user profile page
└── services/
    └── api.js              # Axios instance with auth interceptor
```

### Backend Structure
```
backend/UserDashboard.API/
├── Controllers/
│   └── AuthController.cs   # Register, Login, GetUser endpoints
├── Data/
│   └── AppDbContext.cs     # Entity Framework Core context
├── DTOs/
│   ├── RegisterDto.cs      # Registration request model
│   ├── LoginDto.cs         # Login request model
│   └── UserDto.cs          # User response model
├── Models/
│   └── User.cs             # User entity
└── Services/
    ├── IAuthService.cs     # Service interface
    └── AuthService.cs      # JWT + BCrypt implementation
```

### Database Schema
```sql
CREATE TABLE Users (
    Id           SERIAL PRIMARY KEY,
    FirstName    VARCHAR NOT NULL,
    LastName     VARCHAR NOT NULL,
    Email        VARCHAR NOT NULL UNIQUE,
    PasswordHash VARCHAR NOT NULL
);
```

## 🔐 Authentication Flow

1. **Register**: User submits form → API validates → BCrypt hashes password → saves to DB
2. **Login**: User submits credentials → API verifies BCrypt hash → generates JWT token → returns token
3. **Access Protected Route**: Frontend includes `Authorization: Bearer <token>` header → API validates JWT → returns user data

## 🚀 Quick Start

### Prerequisites
- [Docker](https://www.docker.com/get-started) & Docker Compose
- OR: Node.js 18+, .NET 8 SDK, PostgreSQL 15+

### Option 1: Docker Compose (Recommended)

```bash
# Clone the repository
git clone https://github.com/Subham-CB/UserDashboard.git
cd UserDashboard

# Start all services
docker compose up --build

# Access the application
# Frontend: http://localhost:3000
# Backend API: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

### Option 2: Local Development

**Start the database:**
```bash
docker compose -f docker-compose.dev.yml up -d
```

**Start the backend:**
```bash
cd backend
dotnet restore
dotnet run --project UserDashboard.API
# API runs at http://localhost:5000
```

**Start the frontend:**
```bash
cd frontend
npm install
npm run dev
# App runs at http://localhost:3000
```

## 📡 API Endpoints

| Method | Endpoint | Auth Required | Description |
|--------|----------|---------------|-------------|
| POST | `/api/auth/register` | No | Register a new user |
| POST | `/api/auth/login` | No | Login and receive JWT token |
| GET | `/api/auth/user` | Yes (Bearer token) | Get authenticated user details |

### Request/Response Examples

**POST /api/auth/register**
```json
// Request
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "password": "securepassword"
}

// Response 200 OK
{ "message": "Registration successful." }

// Response 409 Conflict
{ "message": "Email already registered." }
```

**POST /api/auth/login**
```json
// Request
{
  "email": "john@example.com",
  "password": "securepassword"
}

// Response 200 OK
{ "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." }

// Response 401 Unauthorized
{ "message": "Invalid email or password." }
```

**GET /api/auth/user**
```json
// Header: Authorization: Bearer <token>

// Response 200 OK
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com"
}
```

## 🧪 Testing

### Run Backend Tests
```bash
cd backend
dotnet test
```

Tests include:
- **Unit Tests** (`AuthServiceTests`): Register, Login, GetUserByEmail scenarios
- **Integration Tests** (`AuthControllerIntegrationTests`): Full HTTP round-trip tests against in-memory database

### Run Frontend Build
```bash
cd frontend
npm install
npm run build
```

## ⚙️ Configuration

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `ConnectionStrings__DefaultConnection` | `Host=localhost;...` | PostgreSQL connection string |
| `Jwt__Key` | (required) | JWT signing secret (min 32 chars) |
| `Jwt__Issuer` | `UserDashboard` | JWT issuer |
| `Jwt__Audience` | `UserDashboard` | JWT audience |
| `Cors__AllowedOrigins__0` | `http://localhost:3000` | Allowed CORS origin |
| `VITE_API_URL` | (empty, uses proxy) | Frontend API base URL |

### Production Configuration
For production, update `Jwt__Key` with a strong secret:
```bash
# Generate a secure key
openssl rand -base64 32
```

## 🐳 Docker Details

The application uses a multi-stage Docker build:

**Backend Dockerfile stages:**
1. `build` — Restores and builds the .NET solution
2. `test` — Runs unit and integration tests
3. `publish` — Creates optimized production build
4. `final` — Minimal runtime image with only the published output

**Frontend Dockerfile stages:**
1. `builder` — Installs dependencies and builds with Vite
2. Final — nginx serves static files and proxies `/api` to backend

## 📁 Project Structure

```
UserDashboard/
├── frontend/                    # React frontend application
│   ├── src/
│   │   ├── components/          # Reusable components
│   │   ├── pages/               # Page components
│   │   └── services/            # API service layer
│   ├── Dockerfile
│   ├── nginx.conf
│   └── package.json
├── backend/                     # C# .NET 8 API
│   ├── UserDashboard.API/       # Main API project
│   ├── UserDashboard.Tests/     # Test project
│   ├── UserDashboard.slnx       # Solution file
│   └── Dockerfile
├── docker-compose.yml           # Production orchestration
├── docker-compose.dev.yml       # Development database only
└── README.md
```

## 🔧 Technology Stack

| Layer | Technology |
|-------|-----------|
| Frontend | React 18, React Router v6, Axios, Vite |
| Backend | ASP.NET Core 8, Entity Framework Core |
| Database | PostgreSQL 15 |
| Auth | JWT Bearer tokens, BCrypt password hashing |
| Testing | xUnit, WebApplicationFactory (integration) |
| Containerization | Docker, Docker Compose, nginx |