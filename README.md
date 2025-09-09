# LeaveRequestsApi
Minimal CRUD API using EF Core InMemory to model leave requests with a simple 20‑weeks/year rule.

## Run
```bash
dotnet restore
dotnet run
curl http://localhost:5000/healthz

# create
curl -X POST http://localhost:5000/leaves -H "Content-Type: application/json" -d '{ "EmployeeId":"E123", "StartDate":"2025-01-15", "WeeksRequested":6, "Type":0 }'
```
Extend with authentication, persistence (SQL Server / EF migrations), and reporting endpoints.
