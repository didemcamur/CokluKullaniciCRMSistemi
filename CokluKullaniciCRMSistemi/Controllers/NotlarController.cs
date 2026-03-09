using CokluKullaniciCRMSistemi.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CokluKullaniciCRMSistemi.Controllers
{
    public class NotlarController : Controller
    {
        // GET: Notlar
        MultiUserCRMEntities1 db= new MultiUserCRMEntities1();
 
        public ActionResult Index()
        {
            // 1️⃣ Login kontrolü
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Login"); // Login sayfasına yönlendir
            }

            // 2️⃣ Admin kontrolü: admin notlar sayfasına giremesin
            bool isAdmin = (Session["KullaniciRol"] is bool) ? (bool)Session["KullaniciRol"] : false;
            if (isAdmin)
            {
                return RedirectToAction("Index", "Admin"); // Admin anasayfasına yönlendir
            }

            // 3️⃣ Giriş yapan kullanıcıya ait notları getir
            int userId = Convert.ToInt32(Session["UserId"]);
            var notes = db.Notes
                .Include("Users")
                .Include("Tasks")
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedDate)
                .ToList();

            return View(notes);
        }


        // GET: Notlar/Create
        public ActionResult Create()
        {
            ViewBag.Users = db.Users.ToList();
            ViewBag.Tasks = db.Tasks.ToList();
            return View();
        }

        // POST: Notlar/Create
        [HttpPost]
        public ActionResult Create(Notes note)
        {
            if (ModelState.IsValid)
            {
                // Oturumdaki kullanıcıyı al
                if (Session["UserId"] != null)
                {
                    int userId = Convert.ToInt32(Session["UserId"]);
                    note.UserId = userId; // Oturum açan kullanıcı atanıyor
                }
                else
                {
                    // Kullanıcı oturumu yoksa (örneğin güvenlik amaçlı)
                    return RedirectToAction("Login", "Account");
                }

                note.CreatedDate = DateTime.Now;

                db.Notes.Add(note);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

         //   ViewBag.Users = db.Users.ToList();
            ViewBag.Tasks = db.Tasks.ToList();
            return View(note);
        }


        // GET: Notlar/Edit/5
        public ActionResult Edit(int id)
        {
            var note = db.Notes.Find(id);
            if (note == null)
            {
                return HttpNotFound();
            }

            ViewBag.Tasks = db.Tasks.ToList();
            return View(note);
        }

        // POST: Notlar/Edit/5
        [HttpPost]
        public ActionResult Edit(Notes note)
        {
            if (ModelState.IsValid)
            {
                var existingNote = db.Notes.Find(note.NoteId);
                if (existingNote != null)
                {
                    existingNote.Title = note.Title;
                    existingNote.Content = note.Content;
                    existingNote.TaskId = note.TaskId;
                    existingNote.UpdatedDate = DateTime.Now;

                    // Oturumdaki kullanıcıyı al
                    if (Session["UserId"] != null)
                    {
                        int userId = Convert.ToInt32(Session["UserId"]);
                        existingNote.UserId = userId;
                    }

                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            ViewBag.Tasks = db.Tasks.ToList();
            return View(note);
        }


        // POST: Notlar/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var note = db.Notes.Find(id);
            if (note != null)
            {
                db.Notes.Remove(note);
                db.SaveChanges();
            }
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