using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Multas_tA.ApiViewModels;
using Multas_tA.Models;

namespace Multas_tA.Api
{
    // Controller API dos agentes.
    // Para informação sobre routing, ver o App_Start/WebApiConfig.cs
    public class AgentesController : ApiController
    {
        #region Base de dados

        // Referência para a base de dados.
        private MultasDb db = new MultasDb();

        #endregion

        #region CRUD: "Read" de agentes
        
        // CRUD: Obter uma lista de Agentes
        // GET: api/Agentes
        public IHttpActionResult GetAgentes()
        {
            // Para prevenir referências circulares, 
            // estou a fazer uso do operador Select do Linq
            // para construir objectos que não têm as listas
            // de multas (é o campo problemático porque as Multas referenciam Agentes).
            // O Select, quando usado no contexto da Entity Framework (db.Agentes)
            // influencia as colunas que são colocadas no SELECT do SQL gerado
            // (é uma otimização de performance...)
            // Experimentem comentar o Select() e ver o que acontece
            // (cuidado: o Visual Studio, programas, ou até o PC pode crashar por
            // falta de memória, por isso gravem o que estão a fazer!)
            var resultado = db.Agentes
                .Select(agente => new // new { } permite definir um objeto anónimo (sem class) em .net.
                {
                    agente.ID, // ID = agente.ID,
                    agente.Nome, // Nome = agente.Nome,
                    agente.Esquadra, // Esquadra = agente.Esquadra,
                    agente.Fotografia // Fotografia = agente.Fotografia
                })
                .ToList(); // O ToList() executa a query na base de dados e guarda os resultados numa List<>.

            // HTTP 200 OK com o JSON resultante (Array de objetos que representam agentes)
            return Ok(resultado);
        }

        // CRUD: Obter um agente, através do seu ID.
        // - Se o agente não existe -> 404 (Not Found)
        // GET: api/Agentes/5
        [ResponseType(typeof(Agentes))]
        public IHttpActionResult GetAgentes(int id)
        {
            Agentes agentes = db.Agentes.Find(id);
            if (agentes == null)
            {
                return NotFound();
            }

            // Nota: Cuidado que aqui também pode ocorrer o problema das referências
            // circulares! Fica como exercício para casa...
            // Não precisam do Select aqui (Linq é só para listas),
            // por isso seria algo como
            // var resultado = new { ??? };

            return Ok(agentes);
        }

        #endregion

        #region CRUD: Criação de agentes

        // CRUD: Criar um agente.
        // - Se o agente não é válido (validações do MVC) -> 400 (Bad Request)
        // - Se estiver tudo OK -> 201 (Created) com o objeto do Agente.
        // Nota: Este método faz uso de view models
        // por questões de validações e segurança.
        // Ver classe 'ApiViewModels/CreateAgenteViewModel' para mais detalhes.
        // POST: api/Agentes
        [ResponseType(typeof(Agentes))]
        public IHttpActionResult PostAgentes(PostAgenteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // O BadRequest permite usar o ModelState
                // para informar o cliente dos erros de validação
                // tal como no MVC.
                return BadRequest(ModelState);
            }

            // Copiar os campos do 'model' para um novo objeto de 'Agentes'.
            // Definir o ID do agente (F12 sobre "GetAgente()")
            // Isto é garantido que funcione, seja para uma operação, seja para múltiplas
            // em simultâneo (ex: muitas pessoas a aceder ao site/API), ao contrário
            // do método db.Agentes.Max(agente => agente.ID) + 1, que NÃO É ATÓMICO,
            // e abre a possibilidade de crashes ou erros de consistência 
            // (tema de Sistemas Distribuidos)
            // Ver a migração Migrations/201804251626256_SequenciaIdAgentes.cs
            var agentes = new Agentes
            {
                ID = db.GetIdAgente(),
                Nome = model.Nome,
                Esquadra = model.Esquadra
            };

            db.Agentes.Add(agentes);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                // Seria muito provável que o método
                // db.Agentes.Max(agente => agente.ID) + 1
                // fizesse com que este if resultasse no Conflict (HTTP 409).
                if (AgentesExists(agentes.ID))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            // Num create, é boa prática devolver uma representação 
            // do resultado da criação do objeto na base de dados.
            return CreatedAtRoute("DefaultApi", new { id = agentes.ID }, agentes);
        }

        #endregion

        #region CRUD: Update de agentes

        // CRUD: Atualizar (PUT) um agente, através do seu ID.
        // O uso do [FromBody] e [FromUri] é para "desambiguar" e ajudar a Web API
        // a distinguir de onde é que devem vir os valores dos campos.
        // --
        // - Se o agente não é válido (validações do MVC) -> 400 (Bad Request)
        // - Se o agente não existe -> 404 (Not Found)
        // PUT: api/Agentes/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutAgentes([FromUri] int id, [FromBody] Agentes agentes)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != agentes.ID)
            {
                return BadRequest();
            }

            db.Entry(agentes).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AgentesExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        #endregion

        #region CRUD: Apagar um agente

        // CRUD: Apagar um agente
        // - Se o agente não existe -> 404 (Not Found)
        // DELETE: api/Agentes/5
        [ResponseType(typeof(Agentes))]
        public IHttpActionResult DeleteAgentes(int id)
        {
            Agentes agentes = db.Agentes.Find(id);
            if (agentes == null)
            {
                return NotFound();
            }

            db.Agentes.Remove(agentes);
            db.SaveChanges();

            return Ok(agentes);
        }

        #endregion

        #region Métodos utilitários e Dispose

        // Fecha a ligação à base de dados.
        // Este método é chamado pela framework ASP.NET automáticamente,
        // por isso não precisam de o fazer.
        // Nota: quando se criam objetos que usam coisas como BDs, sockets,
        // ou ficheiros, em .net, implementa-se a interface IDisposable.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // Função criada pelo template que verifica se já
        // existe um agente com um determinado ID.
        private bool AgentesExists(int id)
        {
            return db.Agentes.Count(e => e.ID == id) > 0;
        } 
        #endregion
    }
}