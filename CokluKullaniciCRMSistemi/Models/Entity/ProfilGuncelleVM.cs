using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CokluKullaniciCRMSistemi.Models
{
    public class ProfilGuncelleVM
    {
        public int UserId { get; set; }
        [Required(ErrorMessage = "Ad Soyad boş bırakılamaz.")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "Kullanıcı adı boş bırakılamaz.")]
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        [Required(ErrorMessage = "Email boş bırakılamaz.")]
        [EmailAddress(ErrorMessage = "Geçersiz email adresi.")]
        public string Email { get; set; }
        public Nullable<bool> IsOwner { get; set; }

        [DataType(DataType.Password)]
        public string MevcutSifre { get; set; }

        [DataType(DataType.Password)]
        public string YeniSifre { get; set; }

        [DataType(DataType.Password)]
        [Compare("YeniSifre", ErrorMessage = "Yeni şifre ve tekrarı eşleşmiyor.")]
        public string YeniSifreTekrari { get; set; }
    }
}