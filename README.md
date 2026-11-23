# Motor & Property Claims Management Platform

A web-based claims management system for handling motor and property insurance claims. The platform supports claim submission, claim assessment, approval workflows, management oversight, settlement and payment tracking.

## Technology Stack

| Layer | Technologies |
|---|---|
| Backend | .NET 10, C#, ASP.NET Core Web API |
| Frontend | Angular, TypeScript, HTML, SCSS |
| Database | SQL Server LocalDB |
| ORM | Entity Framework Core |
| Authentication | ASP.NET Core Identity, JWT |
| Testing | xUnit, integration testing |

## Main Features

### Claimant
- Register and log in.<img width="902" height="672" alt="Screenshot 2026-11-24 015001" src="https://github.com/user-attachments/assets/33d165d8-3742-4286-9d1f-40e08f78d279" />

- Submit motor and property claims.<img width="1653" height="911" alt="Screenshot 2026-11-24 015120" src="https://github.com/user-attachments/assets/668e0191-9643-4a72-a795-cb2af150a6dc" />

- View and track submitted claims.<img width="1913" height="906" alt="Screenshot 2026-11-24 015047" src="https://github.com/user-attachments/assets/e7800487-b96f-490c-8ae5-9dcb8a961fb1" />

- View individual claim details.<img width="1258" height="907" alt="Screenshot 2026-11-24 015152" src="https://github.com/user-attachments/assets/5f5e6159-19ac-4b19-a0d6-55118b7cf1b1" />

- Respond to requests for additional information.

### Claims Officer
- View and filter the claims worklist.<img width="1910" height="898" alt="Screenshot 2026-11-24 015209" src="https://github.com/user-attachments/assets/89352210-2a1a-4de8-80d2-439b513b4e7f" />

- Pick up and review claims.
- Request additional information.
- Propose settlement amounts.
- Approve or reject claims.<img width="1258" height="897" alt="Screenshot 2026-11-24 015249" src="https://github.com/user-attachments/assets/d8c7d05a-8c90-4630-9fcd-5c5a13510c29" /><img width="1269" height="900" alt="Screenshot 2026-11-24 015312" src="https://github.com/user-attachments/assets/488cdeef-6473-4802-9101-f43e991736c8" />


- Escalate claims requiring manager approval.

### Manager
- View management dashboard and claim statistics.
- Monitor financial exposure by currency.
- Review claims awaiting manager approval.
- Approve or reject escalated claims.<img width="1905" height="908" alt="Screenshot 2026-11-24 015331" src="https://github.com/user-attachments/assets/e8b471fe-2fdb-4703-adc5-7519fe87aedf" />

### Settlement and Payment
- Record approved claim settlements.
- Update payment outcomes.
- Record payment references.
- Track pending, successful and failed payments.<img width="1914" height="903" alt="Screenshot 2026-11-24 015348" src="https://github.com/user-attachments/assets/a2d8fa92-0ea3-4d26-ab74-2e02545d3828" />


## Application Architecture

The backend follows a layered architecture to separate responsibilities.

```text
MotorPropertyClaims
│
├── src/
│   ├── Claims.Api/              # REST API and controllers
│   ├── Claims.Application/      # Application handlers and DTOs
│   ├── Claims.Domain/           # Domain entities and business rules
│   └── Claims.Infrastructure/   # Database and repositories
│
├── tests/
│   ├── Claims.UnitTests/
│   └── Claims.IntegrationTests/
│
├── frontend/
│   └── claims-portal/           # Angular application
│
├── AI-WORKING-JOURNAL.md
└── README.md
```

## Prerequisites

Install the following before running the application:

- .NET 10 SDK
- Node.js and npm
- Angular CLI
- SQL Server LocalDB
- Git

## Running the Application Locally

The backend and frontend must run separately.

### 1. Clone the repository

```bash
git clone <your-github-repository-url>
cd MotorPropertyClaims
```

### 2. Configure the backend

Ensure SQL Server LocalDB is installed and available.

Configure the required development settings and JWT signing key using ASP.NET Core User Secrets. Development account passwords and other sensitive configuration should not be committed to Git.

Restore and build the backend:

```bash
dotnet restore
dotnet build
```

Run the API:

```bash
dotnet run --project src/Claims.Api
```

The configured development API address is:

```text
http://localhost:5223
```

The API base URL is:

```text
http://localhost:5223/api/v1
```

### 3. Configure and run the Angular frontend

Open another terminal:

```bash
cd frontend/claims-portal
npm install
npm start
```

Open the application in your browser:

```text
http://localhost:4200
```

Ensure the API is running before using the frontend.

## Development Accounts

The development environment supports the following seeded roles:

| Account | Role |
|---|---|
| claimant@example.com | Claimant |
| officer@example.com | Claims Officer |
| manager@example.com | Manager |

Development passwords are maintained separately through User Secrets and are not included in this repository.

## Testing

The backend test projects include unit and integration tests.

Run the backend build:

```bash
dotnet build
```

Run the backend tests:

```bash
dotnet test
```

Build the Angular frontend:

```bash
cd frontend/claims-portal
npm run build
```

The application and its core workflows have been tested locally.

## AI Working Journal

The development process included AI-assisted architecture discussions, implementation guidance, debugging and code review.

Refer to [AI Working Journal](AI-WORKING-JOURNAL.md) for a record of significant AI interactions, recommendations, implementation decisions and reasoning.
