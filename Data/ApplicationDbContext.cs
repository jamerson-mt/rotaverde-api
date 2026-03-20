using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RotaVerdeAPI.Models;

namespace RotaVerdeAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSet para TurmaModel
        public DbSet<TurmaModel> Turmas { get; set; }
        public DbSet<AtividadeModel> atividades { get; set; }
        public DbSet<FormModel> forms { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder
                .Entity<ApplicationUser>()
                .HasOne<TurmaModel>() // Usuario tem uma Turma
                .WithMany() // Turma tem muitos Usuarios
                .HasForeignKey(u => u.TurmaId)
                .OnDelete(DeleteBehavior.SetNull); // AO APAGAR TURMA, TURMAID VIRA NULL

            // Configurações adicionais podem ser feitas aqui
        }
    }
}
