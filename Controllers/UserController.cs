using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RotaVerdeAPI.Models;

namespace RotaVerdeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: api/user
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = new List<object>();
            foreach (var user in _userManager.Users.ToList())
            {
                var roles = await _userManager.GetRolesAsync(user);
                users.Add(
                    new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        user.TurmaId,
                        Roles = roles,
                    }
                );
            }
            return Ok(users);
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(
            string id,
            [FromBody] Dictionary<string, string> updates
        )
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            // Atualizar os campos do usuário com base no dicionário de atualizações
            if (updates.TryGetValue("UserName", out var userName))
                user.UserName = userName;
            if (updates.TryGetValue("Email", out var email))
                user.Email = email;
            if (updates.TryGetValue("FullName", out var fullName))
                user.FullName = fullName;
            if (updates.TryGetValue("Address", out var address))
                user.Address = address;
            if (
                updates.TryGetValue("turmaId", out var turmaId)
                && int.TryParse(turmaId, out var turmaIdInt)
            )
                user.TurmaId = turmaIdInt;

            var result = await _userManager.UpdateAsync(user); // Atualizar o usuário
            if (!result.Succeeded) // Verificar se a atualização foi bem-sucedida
            {
                return BadRequest(result.Errors); //
            }
            //retorna dados atualizados do usuário, menos dados sensíveis
            return Ok(new { user.UserName, user.Email });
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}
