using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDb>(opt => opt.UseInMemoryDatabase("leaves"));
builder.Services.AddProblemDetails();
var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }));

app.MapGet("/leaves", async (AppDb db) => await db.LeaveRequests.AsNoTracking().ToListAsync());

app.MapPost("/leaves", async (LeaveRequest req, AppDb db) =>
{
    // Simple business rule: max 20 weeks combined per year
    if (req.WeeksRequested < 0 || req.WeeksRequested > 20)
        return Results.Problem("WeeksRequested must be between 0 and 20", statusCode: 400);

    db.LeaveRequests.Add(req);
    await db.SaveChangesAsync();
    return Results.Created($"/leaves/{req.Id}", req);
});

app.MapPut("/leaves/{id:int}", async (int id, LeaveRequest update, AppDb db) =>
{
    var existing = await db.LeaveRequests.FindAsync(id);
    if (existing is null) return Results.NotFound();

    existing.EmployeeId = update.EmployeeId;
    existing.StartDate = update.StartDate;
    existing.WeeksRequested = update.WeeksRequested;
    existing.Type = update.Type;

    await db.SaveChangesAsync();
    return Results.Ok(existing);
});

app.MapDelete("/leaves/{id:int}", async (int id, AppDb db) =>
{
    var existing = await db.LeaveRequests.FindAsync(id);
    if (existing is null) return Results.NotFound();
    db.LeaveRequests.Remove(existing);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> options) : base(options) {}
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
}

public enum LeaveType { Family, Medical, Other }

public class LeaveRequest
{
    public int Id { get; set; }

    [Required, StringLength(32)]
    public string EmployeeId { get; set; } = "";

    [Required]
    public DateOnly StartDate { get; set; }

    [Range(0, 20)]
    public int WeeksRequested { get; set; }

    [Required]
    public LeaveType Type { get; set; }
}
