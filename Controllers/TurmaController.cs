using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RotaVerdeAPI.Data; // Namespace do DbContext
using RotaVerdeAPI.Models; // Namespace do modelo Turma

namespace RotaVerdeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TurmaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TurmaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Turma
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TurmaModel>>> GetAll()
        {
            var turmas = await _context.Turmas.ToListAsync();
            return Ok(turmas);
        }

        // GET: api/Turma/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TurmaModel>> GetById(int id)
        {
            var turma = await _context.Turmas.FindAsync(id);
            if (turma == null)
            {
                return NotFound();
            }
            return Ok(turma);
        }

        // POST: api/Turma
        [HttpPost]
        public async Task<ActionResult<TurmaModel>> Create([FromBody] TurmaModel turma)
        {
            _context.Turmas.Add(turma);
            await _context.SaveChangesAsync();

            // Carregar propriedades relacionadas, se necessário
            var turmaCriada = await _context
                .Turmas.Include(t => t.Criador) // Inclua o criador, se aplicável
                .FirstOrDefaultAsync(t => t.Id == turma.Id);

            return CreatedAtAction(nameof(GetById), new { id = turma.Id }, turmaCriada);
        }

        // PUT: api/Turma/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] TurmaModel turmaAtualizada)
        {
            if (id != turmaAtualizada.Id)
            {
                return BadRequest("ID da URL não coincide com o ID do corpo.");
            }

            // 1. Busca a turma existente incluindo a lista de usuários atual
            var turmaNoBanco = await _context
                .Turmas.Include(t => t.Usuarios)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (turmaNoBanco == null)
            {
                return NotFound();
            }

            // 2. Atualiza os dados básicos da turma
            turmaNoBanco.Nome = turmaAtualizada.Nome;
            turmaNoBanco.Descricao = turmaAtualizada.Descricao;
            turmaNoBanco.AnoLetivo = turmaAtualizada.AnoLetivo;
            turmaNoBanco.Turno = turmaAtualizada.Turno;

            // 3. Lógica para atualizar os Alunos/Usuários
            // Se a turma que veio do front-end tem uma lista de usuários (mesmo que só com IDs)
            if (turmaAtualizada.Usuarios != null)
            {
                // Limpa a lista atual para reconstruir com os novos selecionados
                turmaNoBanco.Usuarios.Clear();

                // Extrai os IDs dos usuários enviados pelo front-end
                var idsNovosUsuarios = turmaAtualizada.Usuarios.Select(u => u.Id).ToList();

                // Busca os objetos ApplicationUser reais no banco para esses IDs
                var novosUsuariosNoBanco = await _context
                    .Users.Where(u => idsNovosUsuarios.Contains(u.Id))
                    .ToListAsync();

                // Adiciona os objetos reais à coleção da turma
                foreach (var usuario in novosUsuariosNoBanco)
                {
                    turmaNoBanco.Usuarios.Add(usuario);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Turmas.Any(t => t.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Turma/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var turma = await _context.Turmas.FindAsync(id);
            if (turma == null)
            {
                return NotFound();
            }

            _context.Turmas.Remove(turma);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
