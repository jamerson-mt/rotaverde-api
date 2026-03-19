using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace RotaVerdeAPI.Models
{
    public class TurmaModel
    {
        public int Id { get; set; } // Identificador único da turma
        public required string Nome { get; set; } // Nome da turma
        public string? Descricao { get; set; } // Descrição da turma
        public required int AnoLetivo { get; set; } // Ano letivo da turma
        public required Enum.Turno Turno { get; set; } // Turno (ex.: Matutino, Vespertino, Noturno)
        public DateTime DataCriacao { get; set; } // Data de criação da turma
        public required string CriadorId { get; set; } // ID do usuário que criou a turma (relacionado ao IdentityUser)
        public ApplicationUser? Criador { get; set; } // Relacionamento com o usuário criador da turma

        // Dentro da classe TurmaModel

        // Relacionamento com usuários (alunos e professores)
    }
}
