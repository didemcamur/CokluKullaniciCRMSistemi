using CokluKullaniciCRMSistemi.Models.Entity;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CokluKullaniciCRMSistemi.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        MultiUserCRMEntities1 db = new MultiUserCRMEntities1();
        public static string MD5Sifrele(string sifre)  // => Login İçin MD5 Şifreleme Fonksiyonu.
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(sifre);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        [HttpPost]
        public ActionResult Index(string emailOrUsername, string sifre) //=> Login.
        {
            // Şifreyi hashle
            string hashedSifre = MD5Sifrele(sifre);

            // DB den ilgili verileri al.
            var kullanici = db.Users.FirstOrDefault(k =>
                (k.Email == emailOrUsername || k.Username == emailOrUsername) &&
                k.PasswordHash == hashedSifre
            );

            if (kullanici != null)
            {
                Session["KullaniciEmail"] = kullanici.Email;
                Session["KullaniciRol"] = kullanici.IsOwner;
                Session["UserId"] = kullanici.UserId;
                Session["KullaniciAdSoyad"] = kullanici.FullName;

                if (kullanici.IsOwner)
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Kullanici");
                }
            }
            else
            {
                ViewBag.Hata = "Eposta ya da Şifre Hatalı";
                return View();
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View(); // Login Ekranı.
        }

        public ActionResult Cikis()
        {
            Session.Clear(); // Session Üzerindeki dataları temizle.
            return RedirectToAction("Index", "Login"); // Çıkış yapılınca index'in Login kısmına git.
        }
    }
}