using CokluKullaniciCRMSistemi.Models.Entity;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CokluKullaniciCRMSistemi.Controllers
{
    public class KullaniciController : Controller
    {
        // GET: Kullanici
        MultiUserCRMEntities1 db = new MultiUserCRMEntities1();
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 1. ADIM: Oturum kontrolü (Authentication) - Giriş yapılmamışsa Login'e
            if (Session["KullaniciEmail"] == null)
            {
                filterContext.Result = RedirectToAction("Index", "Login");
                return;
            }

            // 2. ADIM: Rol kontrolü (Authorization) - Admin buraya girmeye çalışıyorsa Admin sayfasına at.
            bool isAdmin = (Session["KullaniciRol"] is bool) ? (bool)Session["KullaniciRol"] : false;

            if (isAdmin) // Eğer IsOwner TRUE ise (yani admin ise)
            {
                // Adminin bu sayfaya erişimi yetkisizdir, Admin sayfasına yönlendir.
                filterContext.Result = RedirectToAction("Index", "Admin"); // 💡 BURAYI DÜZELTTİK: Yanlış yetkideki kullanıcıyı doğru yerine at.
                return;
            }

            base.OnActionExecuting(filterContext); // Kullanıcı ise devam et.
        }
        public ActionResult Index()
        {
            int userId = Convert.ToInt32(Session["UserId"]);

            var gelirler = db.Incomes
                             .Where(g => g.UserId == userId)
                             .OrderByDescending(g => g.Date)
                             .ToList();

            var giderler = db.Expenses
                              .Where(g => g.UserId == userId)
                              .OrderByDescending(g => g.Date)
                              .ToList();

            var model = new KullaniciDashboardViewModel
            {
                ToplamGelir = gelirler.Sum(g => g.Amount),
                ToplamGider = giderler.Sum(g => g.Amount),
                Bakiye = gelirler.Sum(g => g.Amount) - giderler.Sum(g => g.Amount),
                Gelirler = gelirler.Take(5).ToList(),
                Giderler = giderler.Take(5).ToList()
            };

            return View(model);
        }

        // Nested class olarak viewmodel
        public class KullaniciDashboardViewModel
        {
            public decimal ToplamGelir { get; set; }
            public decimal ToplamGider { get; set; }
            public decimal Bakiye { get; set; }

            public List<Incomes> Gelirler { get; set; }
            public List<Expenses> Giderler { get; set; }
        }

        public ActionResult Cikis()
        {
            Session.Clear(); // Session Üzerindeki dataları temizle.
            return RedirectToAction("Index", "Login"); // Çıkış yapılınca index'in Login kısmına git.
        }

    }
    }
