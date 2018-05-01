namespace Multas_tA.Migrations
{
    using Multas_tA.Models;
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public partial class SequenciaIdAgentes : DbMigration
    {
        // Adicionar uma sequência para obter IDs de Agentes de forma
        // atómica.
        public override void Up()
        {
            int maxIdAgente = 0;

            // Obter o último ID dos agentes, caso já existam agentes na BD...
            // Aqui é seguro fazer isto, a não ser que a BD esteja a ser usada
            // por outra aplicação.
            using (var db = new MultasDb())
            {
                if (db.Agentes.Any())
                {
                    maxIdAgente = db.Agentes.Max(x => x.ID) + 1;
                }
            }

            // Sequências são uma forma atómica de obter números a partir de uma BD.
            // https://docs.microsoft.com/en-us/sql/t-sql/statements/create-sequence-transact-sql?view=sql-server-2017

            // ATENÇÃO: Só estou a fazer concatenação porque T-SQL (SQL do SQL Server) 
            // não suporta parameters com comandos DDL!
            // NUNCA se deve fazer concatenação de strings com variáveis
            // quando se quer fazer uma query SQL, especialmente se os valores são user-provided!!!!
            Sql(@"Create Sequence [dbo].[SeqIdAgente] As Int Start With " + maxIdAgente + ";");
        }
        
        // Se se fizer rollback a esta migração, apagar a sequência criada no upgrade (Up)
        public override void Down()
        {
            Sql("Drop Sequence [dbo].[SeqIdAgente]");
        }
    }
}
