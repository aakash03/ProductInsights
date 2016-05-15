using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ProductInsight.Models;

namespace ProductInsight.Controllers
{
    public class ResultsController : Controller
    {
        private AzureDBContext AzureDB = new AzureDBContext();

        // GET: Results
        public ActionResult Index()
        {
            return View(AzureDB.ResultSet.ToList());
        }

        // GET: Results/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Results results = AzureDB.ResultSet.Find(id);
            if (results == null)
            {
                return HttpNotFound();
            }
            return View(results);
        }

        // GET: Results/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Results/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,rev_id,rep_score,logisticsDept,financeDept,qualityDept")] Results results)
        {
            if (ModelState.IsValid)
            {
                AzureDB.ResultSet.Add(results);
                AzureDB.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(results);
        }

        // GET: Results/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Results results = AzureDB.ResultSet.Find(id);
            if (results == null)
            {
                return HttpNotFound();
            }
            return View(results);
        }

        // POST: Results/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,rev_id,rep_score,logisticsDept,financeDept,qualityDept")] Results results)
        {
            if (ModelState.IsValid)
            {
                AzureDB.Entry(results).State = EntityState.Modified;
                AzureDB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(results);
        }

        // GET: Results/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Results results = AzureDB.ResultSet.Find(id);
            if (results == null)
            {
                return HttpNotFound();
            }
            return View(results);
        }

        // POST: Results/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Results results = AzureDB.ResultSet.Find(id);
            AzureDB.ResultSet.Remove(results);
            AzureDB.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AzureDB.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
