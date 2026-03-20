using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RotaVerdeAPI.Models;
using RotaVerdeAPI.Models.Auth; // Atualizado para o namespace correto

namespace RotaVerdeAPI.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        // Armazena os números gerados e suas associações
        private static readonly ConcurrentDictionary<int, string> GeneratedNumbers = new();
        private static int CurrentNumber = 0;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // POST: api/Auth/Register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (
                string.IsNullOrWhiteSpace(request.Username)
                || string.IsNullOrWhiteSpace(request.Email)
                || string.IsNullOrWhiteSpace(request.Password)
            )
            {
                return BadRequest(new { Message = "Todos os campos são obrigatórios." });
            }

            var user = new ApplicationUser { UserName = request.Username, Email = request.Email };
            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new { Message = "Usuário registrado com sucesso!" });
        }

        // POST: api/Auth/Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            if (
                string.IsNullOrWhiteSpace(request.Username)
                || string.IsNullOrWhiteSpace(request.Password)
            )
            {
                return BadRequest(new { Message = "Usuário e senha são obrigatórios." });
            }

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return Unauthorized(new { Message = "Usuário não encontrado!" });
            }
            // Verifica a senha e configura o cookie de autenticação
            var result = await _signInManager.PasswordSignInAsync(
                request.Username,
                request.Password,
                false, // Não persistir o cookie (não lembrar do login)
                false // Não bloquear o usuário em caso de falha
            ); //
            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Credenciais inválidas." });
            }

            var roles = await _userManager.GetRolesAsync(user); // Obtém as roles do usuário

            return Ok(
                new
                {
                    Message = "Login realizado com sucesso!",
                    User = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        Roles = roles, // Adiciona as roles na resposta
                    },
                }
            );
        }

        [Authorize]
        // POST: api/Auth/Logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync(); // Limpa o cookie de autenticação

            return Ok(new { Message = "Logout realizado com sucesso!" });
        }

        // POST: api/Auth/AssignRole
        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
        {
            if (
                string.IsNullOrWhiteSpace(request.Username)
                || string.IsNullOrWhiteSpace(request.Role)
            )
            {
                return BadRequest(new { Message = "Usuário e role são obrigatórios." });
            }

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return NotFound(new { Message = "Usuário não encontrado." });
            }

            if (request.Role != "aluno" && request.Role != "professor")
            {
                return BadRequest(new { Message = "Role inválida. Use 'aluno' ou 'professor'." });
            }

            var result = await _userManager.AddToRoleAsync(user, request.Role);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(
                new
                {
                    Message = $"Role '{request.Role}' atribuída ao usuário '{request.Username}' com sucesso!",
                }
            );
        }

        // POST: api/Auth/GenerateNumber
        [HttpPost("generate")]
        public IActionResult GenerateNumber()
        {
            if (CurrentNumber > 20)
            {
                return BadRequest(new { Message = "Limite de números gerados atingido." });
            }

            int newNumber = CurrentNumber++;
            GeneratedNumbers.TryAdd(newNumber, string.Empty); // Número gerado sem associação inicial

            return Ok(new { Number = newNumber });
        }

        // GET: api/Auth/GeneratedNumbers
        [HttpGet("generated")]
        public IActionResult GetGeneratedNumbers()
        {
            var numbers = GeneratedNumbers.Select(kvp => new
            {
                Number = kvp.Key,
                AssignedTo = kvp.Value, // Nome do usuário associado (ou null)
            });

            return Ok(numbers);
        }

        // POST: api/Auth/AssignNumber
        [HttpPost("assign-number")]
        public async Task<IActionResult> AssignNumber([FromBody] AssignNumberRequest request)
        {
            if (!GeneratedNumbers.ContainsKey(request.Number))
            {
                return BadRequest(new { Message = "Número inválido ou não gerado." });
            }

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return NotFound(new { Message = "Usuário não encontrado." });
            }

            //faca validacao se usuario ja tem numero atribuido
            if (GeneratedNumbers.Any(kvp => kvp.Value == request.Username))
            {
                return BadRequest(new { Message = "Usuário já possui um número atribuído." });
            }

            GeneratedNumbers[request.Number] = request.Username; // Associa o número ao usuário

            return Ok(
                new
                {
                    Message = $"Número {request.Number} atribuído ao usuário '{request.Username}' com sucesso!",
                }
            );
        }

        // POST: api/Auth/LoginByNumber
        [HttpPost("login-by-number")]
        public async Task<IActionResult> LoginByNumber([FromBody] LoginByNumberRequest request)
        {
            if (
                !GeneratedNumbers.TryGetValue(request.Number, out var username)
                || string.IsNullOrWhiteSpace(username)
            )
            {
                return Unauthorized(
                    new { Message = "Número inválido ou não associado a um usuário." }
                );
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return Unauthorized(new { Message = "Usuário não encontrado." });
            }

            // Configura o cookie de autenticação
            await _signInManager.SignInAsync(
                user,
                new AuthenticationProperties
                {
                    IsPersistent = true, // Define o cookie como persistente
                }
            );

            return Ok(
                new
                {
                    Message = "Login realizado com sucesso!",
                    User = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                    },
                }
            );
        }

        // GET: api/Auth/ObserveNumber
        [HttpGet("observe-number/{number}")]
        public IActionResult ObserveNumber(int number)
        {
            if (!GeneratedNumbers.ContainsKey(number))
            {
                return BadRequest(new { Message = "Número inválido ou não gerado." });
            }

            var assignedTo = GeneratedNumbers[number];
            if (string.IsNullOrWhiteSpace(assignedTo))
            {
                return Ok(
                    new { Message = "Número ainda não foi associado a um aluno.", status = false }
                );
            }

            return Ok(
                new
                {
                    Message = $"Número {number} foi associado ao aluno '{assignedTo}'.",
                    status = true,
                }
            );
        }
    }
}
