using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Multas_tA.Models;
using Multas_tA.ViewModels;

namespace Multas_tA.Controllers
{
    public class MultasController : Controller
    {
        private MultasDb db = new MultasDb();

        // GET: Multas
        public ActionResult Index()
        {
            var multas = db.Multas.Include(m => m.Agente).Include(m => m.Condutor).Include(m => m.Viatura);
            return View(multas.ToList());
        }

        // GET: Multas/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Multas multas = db.Multas.Find(id);
            if (multas == null)
            {
                return HttpNotFound();
            }
            return View(multas);
        }

        // GET: Multas/Create
        public ActionResult Create()
        {
            // Criação de um View Model com dados para as dropdowns.
            var model = new ViaturaFormModel
            {
                // Opção nº 1: Usar um SelectList.
                // Nota: nameof(Agentes.ID) -> "ID"
                //       nameof(Agentes.Nome) -> "Nome"
                // (mais resistente também a refactorings).
                AgentesSelectList = new SelectList(db.Agentes, nameof(Agentes.ID), nameof(Agentes.Nome)),
                
                // Opção nº 2: Linq. Mais flexível do que a opção nº 1, mas é mais código...
                // eu prefiro esta opção, para ser sincero.
                ViaturasSelectList = db.Viaturas
                    .Select(viatura => new SelectListItem
                    {
                        Value = viatura.ID.ToString(), // SelectListItem só suporta string.
                        Text = viatura.Marca + " " + viatura.Modelo + " de " + viatura.NomeDono + ", matrícula " + viatura.Matricula
                    })
                    .ToList(), // Convém fazer o ToList()
                CondutoresSelectList = db.Condutores
                    .Select(condutor => new SelectListItem
                    {
                        Value = condutor.ID.ToString(),
                        Text = condutor.Nome + ", carta de condução " + condutor.NumCartaConducao
                    })
                    .ToList()
            };

            return View(model);
        }

        // POST: Multas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViaturaFormModel model)
        {
            if (ModelState.IsValid)
            {
                var multa = new Multas
                {
                    Infracao = model.Infracao,
                    LocalDaMulta = model.LocalDaMulta,

                    // Os seguintes campos são nullable. 
                    // Temos que usar '.Value' para obter o valor,
                    // senão dá erro
                    // (Ex: cannot convert int? to int)
                    DataDaMulta = model.DataDaMulta.Value,
                    ValorMulta = model.ValorMulta.Value,

                    AgenteFK = model.AgenteFK.Value,
                    CondutorFK = model.CondutorFK.Value,
                    ViaturaFK = model.ViaturaFK.Value
                };

                db.Multas.Add(multa);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // TODO: Se obtermos um erro, teremos que reinicializar as nossas dropdowns.

            return View(model);
        }

        // GET: Multas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Multas multas = db.Multas.Find(id);
            if (multas == null)
            {
                return HttpNotFound();
            }
            ViewBag.AgenteFK = new SelectList(db.Agentes, "ID", "Nome", multas.AgenteFK);
            ViewBag.CondutorFK = new SelectList(db.Condutores, "ID", "Nome", multas.CondutorFK);
            ViewBag.ViaturaFK = new SelectList(db.Viaturas, "ID", "Matricula", multas.ViaturaFK);
            return View(multas);
        }

        // POST: Multas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Infracao,LocalDaMulta,ValorMulta,DataDaMulta,AgenteFK,CondutorFK,ViaturaFK")] Multas multas)
        {
            if (ModelState.IsValid)
            {
                db.Entry(multas).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AgenteFK = new SelectList(db.Agentes, "ID", "Nome", multas.AgenteFK);
            ViewBag.CondutorFK = new SelectList(db.Condutores, "ID", "Nome", multas.CondutorFK);
            ViewBag.ViaturaFK = new SelectList(db.Viaturas, "ID", "Matricula", multas.ViaturaFK);
            return View(multas);
        }

        // GET: Multas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Multas multas = db.Multas.Find(id);
            if (multas == null)
            {
                return HttpNotFound();
            }
            return View(multas);
        }

        // POST: Multas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Multas multas = db.Multas.Find(id);
            db.Multas.Remove(multas);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
