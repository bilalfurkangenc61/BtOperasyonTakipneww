using Microsoft.AspNetCore.Mvc;
using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BtOperasyonTakip.Controllers
{
    public class MusteriController : Controller
    {
        private readonly AppDbContext _context;

        public MusteriController(AppDbContext context)
        {
            _context = context;
        }

        private void LoadDropdowns()
        {
            ViewBag.Teknolojiler = _context.Parametreler
                .Where(p => p.Tur == "Teknoloji")
                .OrderBy(p => p.ParAdi)
                .ToList();

            ViewBag.TalepEdenler = _context.Parametreler
                .Where(p => p.Tur == "TalepEden")
                .OrderBy(p => p.ParAdi)
                .ToList();
        }

        [HttpGet]
        public IActionResult Index()
        {
            LoadDropdowns();

            var musteriler = _context.Musteriler
                .OrderByDescending(m => m.KayitTarihi)
                .ThenByDescending(m => m.MusteriID)
                .ToList();

            return View(musteriler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(Musteri model)
        {
            LoadDropdowns();

            if (ModelState.IsValid)
            {
                if (!model.KayitTarihi.HasValue || model.KayitTarihi.Value.Year < 2000)
                    model.KayitTarihi = DateTime.Now;

                _context.Musteriler.Add(model);
                _context.SaveChanges();
                TempData["Success"] = "✅ Müşteri başarıyla eklendi.";

                return RedirectToAction("Index");
            }

            var musteriler = _context.Musteriler
                .OrderByDescending(m => m.KayitTarihi)
                .ThenByDescending(m => m.MusteriID)
                .ToList();

            return View(musteriler);
        }

        [HttpGet("/Musteri/GetMusteri/{id}")]
        public IActionResult GetMusteri(int id)
        {
            var musteri = _context.Musteriler.FirstOrDefault(m => m.MusteriID == id);
            if (musteri == null)
                return NotFound();

            return Json(musteri);
        }

        [HttpPost("/Musteri/UpdateMusteri")]
        public IActionResult UpdateMusteri(Musteri musteri)
        {
            if (musteri == null || musteri.MusteriID == 0)
                return BadRequest("Geçersiz müşteri verisi.");

            var existing = _context.Musteriler.FirstOrDefault(m => m.MusteriID == musteri.MusteriID);
            if (existing == null)
                return NotFound("Müşteri bulunamadı.");

            existing.Firma = musteri.Firma;
            existing.FirmaYetkilisi = musteri.FirmaYetkilisi;
            existing.SiteUrl = musteri.SiteUrl;
            existing.Teknoloji = musteri.Teknoloji;
            existing.Durum = musteri.Durum;
            existing.TalepSahibi = musteri.TalepSahibi;
            existing.Telefon = musteri.Telefon;
            existing.Aciklama = musteri.Aciklama;
            if (existing.KayitTarihi == null || existing.KayitTarihi.Value.Year < 2000)
                existing.KayitTarihi = DateTime.Now;

            _context.SaveChanges();
            return Ok("Müşteri güncellendi.");
        }

        [HttpPost("/Musteri/DeleteMusteri/{id}")]
        public IActionResult DeleteMusteri(int id)
        {
            var musteri = _context.Musteriler.FirstOrDefault(m => m.MusteriID == id);
            if (musteri == null)
                return NotFound();

            _context.Musteriler.Remove(musteri);
            _context.SaveChanges();

            return Ok("Müşteri silindi.");
        }
    }
}
