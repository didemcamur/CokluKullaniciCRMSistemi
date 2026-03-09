using CokluKullaniciCRMSistemi.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;

namespace CokluKullaniciCRMSistemi.Controllers
{
    public class GelirController : Controller
    {
        // GET: Gelir
        MultiUserCRMEntities1 db = new MultiUserCRMEntities1();
        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    // 1. ADIM: Oturum kontrolü (Authentication) - Giriş yapılmamışsa Login'e
        //    if (Session["KullaniciEmail"] == null)
        //    {
        //        filterContext.Result = RedirectToAction("Index", "Login");
        //        return;
        //    }

        //    // 2. ADIM: Rol kontrolü (Authorization) - Kullanıcı buraya girmeye çalışıyorsa Kullanici sayfasına at.
        //    bool isAdmin = (Session["KullaniciRol"] is bool) ? (bool)Session["KullaniciRol"] : false;

        //    if (!isAdmin) // Eğer IsOwner FALSE ise (yani kullanıcı ise)
        //    {
        //        // Kullanıcının bu sayfaya erişimi yetkisizdir, Kullanici sayfasına yönlendir.
        //        filterContext.Result = RedirectToAction("Index", "Kullanici"); // 💡 BURAYI DÜZELTTİK: Yanlış yetkideki kullanıcıyı doğru yerine at.
        //        return;
        //    }

        //    base.OnActionExecuting(filterContext); // Admin ise devam et.
        //}
        public ActionResult Index()
        {
            // 1️⃣ Login kontrolü
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Login"); // Login sayfasına gönder
            }

            // 2️⃣ Admin kontrolü
            bool isAdmin = (Session["KullaniciRol"] is bool) ? (bool)Session["KullaniciRol"] : false;
            if (isAdmin)
            {
                return RedirectToAction("Index", "Admin"); // Admin anasayfasına gönder
            }

            // 3️⃣ Kullanıcıya ait gelirleri getir
            int userId = Convert.ToInt32(Session["UserId"]);
            var gelirler = db.Incomes
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.Date)
                .ToList();

            return View(gelirler);
        }

        [HttpGet]
        public ActionResult Duzenle(int id)
        {
            var gelir = db.Incomes.Find(id);
            if (gelir == null)
                return HttpNotFound();

            ViewBag.Users = db.Users.ToList(); // kullanıcı listesini dropdown için gönder
            return View(gelir);
        }

        // GELİR DÜZENLEME (POST)
        [HttpPost]
        public ActionResult Duzenle(Incomes g)
        {
            var gelir = db.Incomes.Find(g.IncomeId);
            if (gelir == null)
                return HttpNotFound();

            gelir.Source = g.Source;
            gelir.Amount = g.Amount;
            gelir.Date = g.Date;
            gelir.UserId = g.UserId;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GELİR SİLME
        public ActionResult Sil(int id)
        {
            var gelir = db.Incomes.Find(id);
            if (gelir == null)
                return HttpNotFound();

            db.Incomes.Remove(gelir);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult YeniGelirEkle()
        {
            return View();
        }
        [HttpPost]
        public ActionResult YeniGelirEkle(Incomes a)
        {
            if (!ModelState.IsValid)
            {
                return View(a);
            }

            a.Date = DateTime.Now;

            if (Session["UserId"] != null)
            {
                int userId = Convert.ToInt32(Session["UserId"]); ;

                var user = db.Users.FirstOrDefault(u => u.UserId == userId);

                if (user != null)
                {
                    a.UserId = user.UserId;
                }
                else
                {
                    ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                    return View(a);
                }
            }
            else
            {
                ModelState.AddModelError("", "Lütfen önce giriş yapın.");
                return View(a);
            }

            db.Incomes.Add(a);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    }

}
