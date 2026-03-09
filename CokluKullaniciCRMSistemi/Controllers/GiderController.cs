using CokluKullaniciCRMSistemi.Models.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CokluKullaniciCRMSistemi.Controllers
{
    public class GiderController : Controller
    {
        // GET: Gider
        MultiUserCRMEntities1 db = new MultiUserCRMEntities1();

        public ActionResult Index(string filtre, int page = 1)
        {
            // ... (Kodun geri kalanı aynı kalır) ...
            int pageSize = 5; // Sayfa başına kayıt sayısı

            // --- FİLTRELEME ---
            var giderler = db.Expenses.AsQueryable();

            if (!string.IsNullOrEmpty(filtre))
            {
                giderler = giderler.Where(x =>
                    x.Category.Contains(filtre) ||
                    x.Description.Contains(filtre)
                );
            }

            // --- SIRALAMA ---
            giderler = giderler.OrderByDescending(x => x.Date);

            // ... (Sayfalama hesaplamaları aynı kalır) ...
            int totalCount = giderler.Count();
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var skipCount = (page - 1) * pageSize;

            var pagedGiderler = giderler.Skip(skipCount).Take(pageSize).ToList();

            // --- VIEW BAG İLE VIEW'A BİLGİ GÖNDERME ---
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentFiltre = filtre;

            return View(pagedGiderler);
        }

        // GET: /Gider/YeniGiderEkle
        public ActionResult YeniGiderEkle()
        {
            return View("GiderFormuView");
        }

        // GET: /Gider/Duzenle/5
        public ActionResult Duzenle(int id)
        {
            var gider = db.Expenses.Find(id);
            if (gider == null)
            {
                return HttpNotFound();
            }
            return View("GiderFormuView", gider);
        }

        // POST: /Gider/Kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Kaydet(Expenses formdanGelenGider)
        {
            // ---- YENİ KAYITSA DOĞRULAMAYI DÜZENLE ----
            // Eğer bu bir yeni kayıt ise (ExpenseId 0'dır),
            // 'UserId' ve 'ExpenseId' için gelen "gereklidir" hatalarını görmezden gel.
            // Çünkü 'UserId'yi biz atayacağız, 'ExpenseId'yi veritabanı atayacak.
            if (formdanGelenGider.ExpenseId == 0)
            {
                ModelState.Remove("ExpenseId");
                ModelState.Remove("UserId");
            }

            // ---- ŞİMDİ MODELİ KONTROL ET ----
            // Artık 'Date' veya 'Amount' gibi *gerçek* hatalar var mı diye bakıyoruz.
            if (!ModelState.IsValid)
            {
                // Hata varsa (örn: Tarih boşsa), formu hatalarla birlikte geri göster
                return View("GiderFormuView", formdanGelenGider);
            }

            // ---- MODEL GEÇERLİ, KAYDETMEYE DEVAM ET ----

            if (formdanGelenGider.ExpenseId == 0)
            {
                // ---- YENİ KAYIT İŞLEMİ ----
                formdanGelenGider.UserId = 1; // TODO: Burası login işleminden sonra dinamik hale getirilecek.
                db.Expenses.Add(formdanGelenGider);
            }
            else
            {
                // ---- GÜNCELLEME İŞLEMİ ----
                db.Entry(formdanGelenGider).State = EntityState.Modified;
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }


        // GET: /Gider/Sil/5
        public ActionResult Sil(int id)
        {
            // ... (Burası aynı, dokunmuyoruz) ...
            var gider = db.Expenses.Find(id);
            if (gider == null)
            {
                return HttpNotFound();
            }
            db.Expenses.Remove(gider);
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