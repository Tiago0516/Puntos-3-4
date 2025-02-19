using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GenderPercentageProject
{
    // Modelo de datos para usuario
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }
        public string Primer_Nombre { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public string Correo { get; set; } = "";
        public string Telefono { get; set; } = "";
        public DateTime? Fecha_Nacimiento { get; set; }
        public string Genero { get; set; } = ""; 
        public string UsuarioTipo { get; set; } = "";
    }


    public class Facturas
    {
        [Key]
        public int FacturaId { get; set; }
        public DateTime Fecha_Factura { get; set; }
        public int PacienteId { get; set; }  
        public int DoctorId { get; set; }
        public decimal Total { get; set; }
    }

    // Contexto de datos usando SQL Server
    public class AppDbContext : DbContext
    {
        public DbSet<Usuario> Usuario { get; set; } = null!;
        public DbSet<Facturas> Facturas { get; set; } = null!;

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=localhost,1433;Database=AseguradoraSalud;User Id=sa;Password=Clave;TrustServerCertificate=True;");
            }
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer("Server=localhost,1433;Database=AseguradoraSalud;User Id=sa;Password=Clave;TrustServerCertificate=True;")
                .Options;

            using (var context = new AppDbContext(options))
            {

                var totalPacientes = await context.Usuario.CountAsync(u => u.UsuarioTipo == "Paciente");

                if (totalPacientes == 0)
                {
                    Console.WriteLine("No se encontraron pacientes.");
                    return;
                }

                var porcentajes = await context.Usuario
                    .Where(p => (p.Genero == "M" || p.Genero == "F") && p.UsuarioTipo == "Paciente")
                    .GroupBy(p => p.Genero)
                    .Select(g => new
                    {
                        Genero = g.Key,
                        Porcentaje = (double)g.Count() / totalPacientes * 100
                    })
                    .ToListAsync();

                Console.WriteLine("Porcentaje de pacientes por género:");
                foreach (var item in porcentajes)
                {
                    Console.WriteLine($"Género: {item.Genero}, Porcentaje: {item.Porcentaje:F2}%");
                }

                var facturacionPorDoctor = await context.Facturas
                    .GroupBy(i => i.DoctorId)
                    .Select(g => new
                    {
                        DoctorId = g.Key,
                        TotalFacturacion = g.Sum(i => i.Total)
                    })
                    .Join(
                        context.Usuario.Where(u => u.UsuarioTipo == "Doctor"),
                        factura => factura.DoctorId,
                        doctor => doctor.UsuarioId,
                        (factura, doctor) => new
                        {
                            NombreDoctor = doctor.Primer_Nombre + " " + doctor.Apellidos,
                            factura.TotalFacturacion
                        }
                    )
                    .ToListAsync();

                Console.WriteLine("Facturación generada por cada doctor:");
                foreach (var item in facturacionPorDoctor)
                {
                    Console.WriteLine($"Doctor: {item.NombreDoctor}, Total Facturación: {item.TotalFacturacion:C}");
                }
            }

            Console.WriteLine("Presione cualquier tecla para salir...");
            Console.ReadKey();
        }
    }
}
