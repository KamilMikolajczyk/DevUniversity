using Microsoft.EntityFrameworkCore;
using University.Data;

namespace University.DataExtension;

public static class MigrationExtension
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<UnivercityDbContext>();
        dbContext.Database.Migrate();
    }
}
