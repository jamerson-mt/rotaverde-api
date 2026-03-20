using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RotaVerdeAPI.Models;

namespace RotaVerdeAPI.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;

                // Garantir que o banco de dados seja criado
                var dbContext = services.GetRequiredService<ApplicationDbContext>();
                dbContext.Database.EnsureCreated();

                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var roles = new[] { "aluno", "professor" };

                foreach (var role in roles)
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                    {
                        roleManager.CreateAsync(new IdentityRole(role)).Wait();
                    }
                }

                // Criar usuário padrão com a role "professor"
                var defaultProfessorEmail = "professor@rotaverde.com";
                var defaultProfessorPassword = "@Password123";
                string defaultAdminEmail = "jjamersonmt@gmail.com";
                string defaultAdminPassword = "@Password123";

                if (userManager.FindByEmailAsync(defaultProfessorEmail).Result == null) // Verifica se o usuário já existe
                {
                    var professorUser = new ApplicationUser
                    {
                        UserName = "lincoln",
                        Email = defaultProfessorEmail,
                        EmailConfirmed = false,
                    };

                    var adminUser = new ApplicationUser
                    {
                        UserName = "jjamersonmt",
                        Email = defaultAdminEmail,
                        EmailConfirmed = false,
                    };



                    var result = userManager
                        .CreateAsync(professorUser, defaultProfessorPassword)// Cria o usuário com a senha especificada
                        .Result; // Aguarda a conclusão da tarefa
                    if (result.Succeeded) // Se a criação do usuário foi bem-sucedida, atribui a role "professor"
                    {
                        userManager.AddToRoleAsync(professorUser, "professor").Wait(); // Aguarda a conclusão da tarefa de atribuição de role
                    }

                    var adminResult = userManager
                        .CreateAsync(adminUser, defaultAdminPassword) // Cria o usuário admin com a senha especificada
                        .Result; // Aguarda a conclusão da tarefa
                    if (adminResult.Succeeded) // Se a criação do usuário admin foi bem-sucedida, atribui a role "professor"
                    {         
                        userManager.AddToRoleAsync(adminUser, "professor").Wait(); // Aguarda a conclusão da tarefa de atribuição de role para o admin
                    }
                }

            }
        }
    }
}
