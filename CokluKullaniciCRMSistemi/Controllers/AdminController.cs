using CokluKullaniciCRMSistemi.Models;
using CokluKullaniciCRMSistemi.Models.Entity;
using PagedList;
using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace CokluKullaniciCRMSistemi.Controllers
{
    public class AdminController : Controller
    {
        MultiUserCRMEntities1 db = new MultiUserCRMEntities1();
        // GET: Admin
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 1. ADIM: Oturum kontrolü (Authentication) - Giriş yapılmamışsa Login'e
            if (Session["KullaniciEmail"] == null)
            {
                filterContext.Result = RedirectToAction("Index", "Login");
                return;
            }

            // 2. ADIM: Rol kontrolü (Authorization) - Kullanıcı buraya girmeye çalışıyorsa Kullanici sayfasına at.
            bool isAdmin = (Session["KullaniciRol"] is bool) ? (bool)Session["KullaniciRol"] : false;

            if (!isAdmin) // Eğer IsOwner FALSE ise (yani kullanıcı ise)
            {
                // Kullanıcının bu sayfaya erişimi yetkisizdir, Kullanici sayfasına yönlendir.
                filterContext.Result = RedirectToAction("Index", "Kullanici"); // 💡 BURAYI DÜZELTTİK: Yanlış yetkideki kullanıcıyı doğru yerine at.
                return;
            }

            base.OnActionExecuting(filterContext); // Admin ise devam et.
        }
        public string md5sifrele(string sifre)
        {

            if (string.IsNullOrEmpty(sifre))
                throw new ArgumentException("Şifre boş olamaz!", nameof(sifre));

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(sifre);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        private string CreateMD5Hash(string input)
        {
            // MD5CryptoServiceProvider sınıfının yeni bir örneğini oluştur
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Byte dizisini 32 karakterlik hexadecimal string'e çevir
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    // Her byte'ı iki haneli hexadecimal formatında formatla
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }


        public ActionResult Index(string p, int sayfa = 1)
        {
            // 1️⃣ Kullanıcıları sorgula
            var admins = db.Users.AsQueryable();

            // 2️⃣ Filtre uygula
            if (!string.IsNullOrEmpty(p))
            {
                if (int.TryParse(p, out int userId))
                {
                    admins = admins.Where(m => m.UserId == userId);
                }
                else
                {
                    admins = admins.Where(m => m.FullName.Contains(p));
                }
            }

            // 3️⃣ Veritabanı gerçekten veri döndürmezse bile boş liste ver
            var model = admins.OrderBy(u => u.UserId).ToList();

            // 4️⃣ View’a null değil, boş IPagedList gönder
            var pagedModel = model.ToPagedList(sayfa, 10);

            ViewBag.arama = p;
            return View(pagedModel);
        }


        public ActionResult Sil(int id)
        {
            var userValue = db.Users.Find(id);
            db.Users.Remove(userValue);
            db.SaveChanges();
            return RedirectToAction("index");
        }
        public ActionResult IdGetir(int id)
        {
            var userValue = db.Users.Find(id);

            if (userValue == null)
            {
                return HttpNotFound();
            }

            // Users den ProfilGuncelleVM e veri aktarımı
            var viewModel = new ProfilGuncelleVM
            {
                UserId = userValue.UserId,
                FullName = userValue.FullName,
                Username = userValue.Username,
                Email = userValue.Email,
                IsOwner = userValue.IsOwner,

                // Şifre alanlarının boş gösterilmesi için
                MevcutSifre = null,
                YeniSifre = null
            };

            return View("IdGetir", viewModel);
        }

        private static ProfilGuncelleVM GetViewModel(Users userValue)
        {
            return new ProfilGuncelleVM
            {
                UserId = userValue.UserId,
                FullName = userValue.FullName,
                Username = userValue.Username,
                Email = userValue.Email,
                IsOwner = userValue.IsOwner,

                // Şifre alanlarının boş gösterilmesi için
                MevcutSifre = null,
                YeniSifre = null
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guncelle(ProfilGuncelleVM p)
        {
            if (!ModelState.IsValid)
            {
                return View("IdGetir");
            }

            var userValue = db.Users.Find(p.UserId);
            if (userValue == null)
            {
                return HttpNotFound();
            }

            //Profil bilgilerini güncelle
            userValue.FullName = p.FullName;
            userValue.Username = p.Username;
            userValue.Email = p.Email;
            userValue.IsOwner = p.IsOwner ?? false;

            //Şifre güncelleme kontrolü
            if (!string.IsNullOrEmpty(p.YeniSifre))
            {
                // Eğer YeniSifre girilmişse MevcutSifre de girilmiş olmalı
                if (string.IsNullOrEmpty(p.MevcutSifre))
                {
                    ModelState.AddModelError("MevcutSifre", "Yeni şifre belirlemek için mevcut şifreyi girmelisiniz.");
                    return View("IdGetir");
                }

                string enteredPasswordHash = CreateMD5Hash(p.MevcutSifre);

                if (enteredPasswordHash != userValue.PasswordHash)
                {
                    ModelState.AddModelError("MevcutSifre", "Mevcut şifreniz hatalı.");
                    return View("IdGetir");
                }

                userValue.PasswordHash = CreateMD5Hash(p.YeniSifre);
            }

            // VT kaydı
            db.Entry(userValue).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index");
        }
    
        [HttpGet]
        public  ActionResult AddUser()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddUser(Users user)
        {
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                ModelState.AddModelError("PasswordHash", "Şifre alanı boş olamaz!");
                return View(user);
            }

            if (ModelState.IsValid)
            {
                // boş şifreyi kontrol ediyoruz
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    ModelState.AddModelError("PasswordHash", "Şifre alanı boş olamaz!");
                    return View(user);
                }

                // Şifreyi MD5 ile şifrele
                user.PasswordHash = md5sifrele(user.PasswordHash);
                user.CreatedDate = DateTime.Now;

                db.Users.Add(user);
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(user);
        }

        public ActionResult Cikis()
        {
            Session.Clear(); // Session Üzerindeki dataları temizle.
            return RedirectToAction("Index", "Login"); // Çıkış yapılınca index'in Login kısmına git.
        }
    }
}