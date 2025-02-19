using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenderPercentageProject
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            // Configura la cadena de conexión a SQL Server (modifícala según tu entorno)
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=AseguradoraSalud;User Id=sa;Password=Clave;TrustServerCertificate=True;");

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
