using Market_Stok_Yönetimi.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
public class HomeController : Controller
{
    deneme1Entities db = new deneme1Entities();
    public ActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Login(string kullaniciAdi, string sifre)
    {

        var admin = db.Admin.SqlQuery("SELECT * FROM Admin WHERE KullaniciAdi = @p0 AND Sifre = @p1", kullaniciAdi, sifre).FirstOrDefault();

        if (admin != null)
        {
            Session["Admin"] = admin.AdminID;
            return RedirectToAction("Index");
        }
        ViewBag.Hata = "Kullanıcı adı veya şifre hatalı";
        return View();
    }
    public ActionResult Logout()
    {
        Session.Abandon();
        return RedirectToAction("Login");
    }

    public ActionResult Index()
    {
        if (Session["Admin"] == null)
            return RedirectToAction("Login");

        ViewBag.AzalanStoklar = db.Urun.SqlQuery("SELECT * FROM Urun WHERE StokMiktari < 10").ToList();
        ViewBag.SonSatislar = db.Database.SqlQuery<SatisDTO>(@"
            SELECT TOP 5
                s.SatisID, s.UrunID, s.Miktar, s.BirimFiyat, s.SatisTarihi,
                u.UrunAdi
            FROM Satis s
            JOIN Urun u ON s.UrunID = u.UrunID
            ORDER BY s.SatisTarihi DESC").ToList();

        ViewBag.SonAlislar = db.Database.SqlQuery<AlisDTO>(@"
             SELECT TOP 5
                a.AlisID, a.UrunID, a.Miktar, a.BirimFiyat, a.AlisTarihi,
                u.UrunAdi
            FROM Alis a
            INNER JOIN Urun u ON a.UrunID = u.UrunID
            ORDER BY a.AlisTarihi DESC").ToList();

        return View();
    }

    public ActionResult Urunler()
    {
        if (Session["Admin"] == null)
            return RedirectToAction("Login");

        var urunListesi = db.Urun.ToList();
        return View(urunListesi);
    }

    public ActionResult Create()
    {
        return View();
    }


    [HttpPost]
    public ActionResult Create(string BirimFiyat, Urun urun)
    {
        BirimFiyat = BirimFiyat.Replace(".", ",");

        if (decimal.TryParse(BirimFiyat, out decimal fiyat))
        {
            urun.BirimFiyat = fiyat;
            db.Urun.Add(urun);
            db.SaveChanges();
            return RedirectToAction("Urunler");
        }

        ModelState.AddModelError("BirimFiyat", "Geçerli bir fiyat giriniz (örn: 90,99)");
        return View(urun);
    }

    public ActionResult Edit(int id)
    {
        var urun = db.Urun.Find(id);
        if (urun == null)
            return HttpNotFound();
        return View(urun);
    }

    [HttpPost]
    public ActionResult Edit(string BirimFiyat, Urun urun)
    {
        BirimFiyat = BirimFiyat.Replace(".", ",");


        if (decimal.TryParse(BirimFiyat, out decimal fiyat))
        {
            urun.BirimFiyat = fiyat;
            db.Entry(urun).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Urunler");
        }
        return View(urun);
    }

    [HttpPost]
    public ActionResult Delete(int id)
    {
        var urun = db.Urun.Find(id);
        if (urun == null)
            return HttpNotFound();

        db.Database.ExecuteSqlCommand("DELETE FROM Alis WHERE UrunID = @p0", id);
        db.Database.ExecuteSqlCommand("DELETE FROM Satis WHERE UrunID = @p0", id);
        db.Database.ExecuteSqlCommand("DELETE FROM Urun WHERE UrunID = @p0", id);

        TempData["Mesaj"] = "Ürün ve ilişkili alış/satış kayıtları silindi.";

        return RedirectToAction("Urunler");
    }

    public ActionResult Satislar()
    {
        var satislar = db.Database.SqlQuery<SatisDTO1>(@"
            SELECT
                s.SatisID,
                s.UrunID,
                u.UrunAdi,
                u.BirimFiyat,
                s.KasaID,
                k.KasaAdi,
                s.KasiyerID,
                ks.Ad,
                ks.Soyad,
                s.Miktar,
                s.SatisTarihi
            FROM Satis s
            JOIN Urun u ON s.UrunID = u.UrunID
            JOIN Kasa k ON s.KasaID = k.KasaID
            JOIN Kasiyer ks ON s.KasiyerID = ks.KasiyerID").ToList();
        return View(satislar);
    }

    public ActionResult Kasiyerler()
    {
        var kasiyerler = db.Database.SqlQuery<KasiyerDTO>(@"
            SELECT
                k.KasiyerID, k.Ad, k.Soyad, k.KasaID,
                ka.KasaAdi
            FROM Kasiyer k
            LEFT JOIN Kasa ka ON k.KasaID = ka.KasaID").ToList();
        return View(kasiyerler);
    }

    [HttpPost]
    public ActionResult SatisYap(int urunID, int adet, int kasiyerID)
    {
        var urun = db.Urun.Find(urunID);

        if (urun != null && urun.StokMiktari >= adet)
        {
            decimal birimFiyat = urun.BirimFiyat;
            decimal toplamTutar = birimFiyat * adet;

            var satis = new Satis
            {
                UrunID = urunID,
                Miktar = adet,
                BirimFiyat = birimFiyat,
                ToplamTutar = toplamTutar,
                SatisTarihi = DateTime.Now,
                KasiyerID = kasiyerID,
                KasaID = (int)(db.Kasiyer.Find(kasiyerID)?.KasaID)
            };

            db.Satis.Add(satis);
            urun.StokMiktari -= adet;
            db.SaveChanges();
        }
        return RedirectToAction("Satislar");
    }


    public ActionResult SatisYap()
    {
        ViewBag.UrunList = new SelectList(db.Urun, "UrunID", "UrunAdi");
        ViewBag.Kasiyerler = db.Database.SqlQuery<KasiyerDTO>(@"
            SELECT
                k.KasiyerID, k.Ad, k.Soyad, k.KasaID,
                ka.KasaAdi
            FROM Kasiyer k
            LEFT JOIN Kasa ka ON k.KasaID = ka.KasaID").ToList();
        return View();
    }

    [HttpGet]
    public ActionResult AlisYap()
    {
        ViewBag.Urunler = new SelectList(db.Urun.ToList(), "UrunID", "UrunAdi");
        return View();
    }

    [HttpPost]
    public ActionResult AlisYap(int urunID, int miktar, string birimFiyat)
    {

        if (string.IsNullOrWhiteSpace(birimFiyat))
        {
            return new HttpStatusCodeResult(400, "Fiyat boş olamaz");
        }

        birimFiyat = birimFiyat.Replace(".", ",");

        if (!decimal.TryParse(birimFiyat, out decimal fiyat))
        {
            return new HttpStatusCodeResult(400, "Geçersiz fiyat formatı! Örnek: 90,99");
        }

        var urun = db.Urun.Find(urunID);
        if (urun != null)
        {
            var alis = new Alis
            {
                UrunID = urunID,
                Miktar = miktar,
                BirimFiyat = fiyat,
                AlisTarihi = DateTime.Now
            };

            db.Alis.Add(alis);

            urun.StokMiktari += miktar;

            db.SaveChanges();
        }

        return RedirectToAction("Index", "Home");
    }
}