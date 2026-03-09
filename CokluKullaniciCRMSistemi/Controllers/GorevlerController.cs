using CokluKullaniciCRMSistemi.Models.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;

namespace CokluKullaniciCRMSistemi.Controllers
{
    public class GorevlerController : Controller
    {
        // GET: Gorevler
        MultiUserCRMEntities1 db = new MultiUserCRMEntities1();
        [HttpGet]
        public ActionResult YeniGorev()
        {
            // Login kontrolü
            if (Session["UserId"] == null)
                return RedirectToAction("Index", "Login");

            return View(new Tasks());
        }


        [HttpPost]
        public ActionResult YeniGorev(Tasks p)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Index", "Login");

            if (p.DueDate == null)
                ModelState.AddModelError("DueDate", "*Son Tarih boş bırakılamaz.");

            if (!ModelState.IsValid || (p.DueDate.HasValue && p.DueDate < DateTime.Today))
            {
                if (p.DueDate.HasValue && p.DueDate < DateTime.Today)
                {
                    ModelState.AddModelError("DueDate", "*Son Tarih, Oluşturulma Tarihinden önce olamaz.");
                }
                return View(p);
            }

            p.CreatedDate = DateTime.Today;
            p.UserId = Convert.ToInt32(Session["UserId"]); // 🔹 sadece giriş yapan kullanıcıya atanıyor

            db.Tasks.Add(p);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Index(string p, int sayfa = 1)
        {
            // 1️⃣ Login kontrolü
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // 2️⃣ Admin kontrolü: admin görev sayfasına giremesin
            bool isAdmin = (Session["KullaniciRol"] is bool) ? (bool)Session["KullaniciRol"] : false;
            if (isAdmin)
            {
                return RedirectToAction("Index", "Admin");
            }

            // 3️⃣ Kullanıcının kendi görevlerini getir
            int userId = Convert.ToInt32(Session["UserId"]);
            var allTasks = db.Tasks
                .Where(t => t.UserId == userId);

            // 4️⃣ Arama filtreleme
            if (!string.IsNullOrEmpty(p))
            {
                allTasks = allTasks.Where(t => t.Title.Contains(p) || t.Users.FullName.Contains(p));
            }

            ViewBag.CurrentFilter = p;

            // 5️⃣ Sayfalama ve sırala
            return View(allTasks
                .OrderBy(m => m.IsCompleted)
                .ToPagedList(sayfa, 10));
        }


        public ActionResult Sil(int id)
        {
            var gorevValue = db.Tasks.Find(id);
            db.Tasks.Remove(gorevValue);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult IdGetir(int id)
        {
            // 1. Görevi ID'ye göre bul
            var gorev = db.Tasks.Find(id);
            if (gorev == null) return HttpNotFound();//HATA KONTROLÜ

            List<SelectListItem> userList = (from u in db.Users.ToList()
                                             select new SelectListItem
                                             {
                                                 Text = u.FullName,
                                                 Value = u.UserId.ToString(),
                                                 //Atanmış kullanıcıyı önceden seçili getirir.
                                                 Selected = u.UserId == gorev.UserId
                                             }).ToList();

            ViewBag.UserList = userList; // View'de dropdown için kullanılacak

            return View("IdGetir", gorev); // Görev nesnesini View'e gönder
        }

        [HttpPost]
        public ActionResult Guncelle(Tasks p)
        {
            if (!ModelState.IsValid || (p.DueDate.HasValue && p.DueDate < p.CreatedDate))
            {
                if (p.DueDate.HasValue && p.DueDate < p.CreatedDate)
                {
                    ModelState.AddModelError("DueDate", "*Son Tarih, Oluşturulma Tarihinden önce olamaz.");
                }

                List<SelectListItem> userList = (from u in db.Users.ToList()
                                                 select new SelectListItem
                                                 {
                                                     Text = u.FullName,
                                                     Value = u.UserId.ToString(),
                                                     Selected = u.UserId == p.UserId
                                                 }).ToList();

                ViewBag.UserList = userList;

                if (!ModelState.IsValid)
                {
                    return View("IdGetir", p);
                }

                return View("IdGetir", p);
            }


            var gorevValue = db.Tasks.Find(p.TaskId);
            if (gorevValue == null)
            {
                return HttpNotFound();
            }


            gorevValue.Title = p.Title;
            gorevValue.Description = p.Description;
            gorevValue.IsCompleted = p.IsCompleted;
            gorevValue.UserId = p.UserId;

            gorevValue.CreatedDate = p.CreatedDate;
            gorevValue.DueDate = p.DueDate;

            db.Entry(gorevValue).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("index");
        }
    }
}