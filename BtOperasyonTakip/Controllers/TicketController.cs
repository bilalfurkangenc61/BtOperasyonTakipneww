using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using System.Linq;

namespace BtOperasyonTakip.Controllers
{
    [Authorize(Roles = "Saha,Operasyon")]
    public class TicketController : Controller
    {
        private readonly AppDbContext _context;

        public TicketController(AppDbContext context)
        {
            _context = context;
        }

        // Index: Saha sadece kendi ticketlarını, Operasyon tüm ticketları görsün (arama desteği ile)
        public IActionResult Index(string? searchFirma)
        {
            IQueryable<Ticket> query = _context.Tickets;

            if (User.IsInRole("Saha"))
            {
                var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;
                query = query.Where(t => t.OlusturanUserId == userId);
            }

            // Firma adına göre arama (MusteriWebSitesi'nde)
            if (!string.IsNullOrWhiteSpace(searchFirma))
            {
                query = query.Where(t => t.MusteriWebSitesi.Contains(searchFirma) || 
                                        t.YazilimciAdi.Contains(searchFirma) ||
                                        t.YazilimciSoyadi.Contains(searchFirma));
            }

            var tickets = query.OrderByDescending(t => t.OlusturmaTarihi).ToList();

            // ViewBag'e arama kriterini geri gönder (arama kutusunda göstermek için)
            ViewBag.SearchFirma = searchFirma;

            return View(tickets);
        }

        // Create GET: Ticket oluşturma formu (Saha için)
        [HttpGet]
        public IActionResult Create()
        {
            if (!User.IsInRole("Saha"))
                return Unauthorized();

            return View();
        }

        // Create POST: Ticket kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Ticket ticket)
        {
            if (!User.IsInRole("Saha"))
                return Unauthorized();

            if (!ModelState.IsValid)
                return View(ticket);

            try
            {
                var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var id) ? id : 0;

                ticket.OlusturanUserId = userId;
                ticket.OlusturanKullaniciAdi = User.Identity?.Name ?? "Bilinmiyor";
                ticket.OlusturmaTarihi = DateTime.UtcNow;
                ticket.Durum = "Onay Bekleniyor";

                _context.Tickets.Add(ticket);
                _context.SaveChanges();

                TempData["Success"] = "✅ Ticket başarıyla oluşturuldu! Onay bekleniyor.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Hata: {ex.Message}");
                return View(ticket);
            }
        }

        // Detail: Ticket detayını göster
        [HttpGet]
        public IActionResult Detail(int id)
        {
            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == id);
            if (ticket == null)
                return NotFound();

            // Saha sadece kendi ticket'larını görebilir
            if (User.IsInRole("Saha"))
            {
                var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;
                if (ticket.OlusturanUserId != userId)
                    return Unauthorized();
            }

            return View(ticket);
        }

        // Approve with Technology: Operasyon tarafından onay ve teknoloji seçimi
        [HttpPost]
        [Authorize(Roles = "Operasyon")]
        public IActionResult ApproveWithTechnology([FromBody] ApproveWithTechRequest request)
        {
            if (request?.Id <= 0)
                return Json(new { success = false, message = "Geçersiz Ticket ID!" });

            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == request.Id);
            if (ticket == null)
                return Json(new { success = false, message = "Ticket bulunamadı!" });

            try
            {
                // Teknoloji seçilmemişse hata
                if (string.IsNullOrWhiteSpace(request.TeknolojiBilgisi))
                    return Json(new { success = false, message = "Lütfen bir teknoloji seçiniz!" });

                var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;

                // Teknoloji bilgisini güncelle
                ticket.TeknolojiBilgisi = request.TeknolojiBilgisi;

                // Aynı web sitesi ile müşteri var mı kontrol et
                var mevcutMusteri = _context.Musteriler
                    .FirstOrDefault(m => m.SiteUrl == ticket.MusteriWebSitesi);

                Musteri musteri;

                if (mevcutMusteri != null)
                {
                    musteri = mevcutMusteri;
                }
                else
                {
                    // Yeni müşteri oluştur
                    musteri = new Musteri
                    {
                        Firma = ticket.MusteriWebSitesi,
                        FirmaYetkilisi = $"{ticket.YazilimciAdi} {ticket.YazilimciSoyadi}",
                        Telefon = ticket.IrtibatNumarasi,
                        SiteUrl = ticket.MusteriWebSitesi,
                        Teknoloji = request.TeknolojiBilgisi,
                        Durum = "Aktif",
                        TalepSahibi = ticket.OlusturanKullaniciAdi,
                        Aciklama = ticket.Aciklama,
                        KayitTarihi = DateTime.UtcNow
                    };

                    _context.Musteriler.Add(musteri);
                    _context.SaveChanges();
                }

                // Ticket durumunu güncelle
                ticket.Durum = "Onaylandi";
                ticket.OnaylayanUserId = userId;
                ticket.OnaylayanKullaniciAdi = User.Identity?.Name ?? "Bilinmiyor";
                ticket.OnaylamaTarihi = DateTime.UtcNow;
                ticket.KararAciklamasi = request.KararAciklamasi;
                ticket.MusteriID = musteri.MusteriID;

                _context.SaveChanges();

                return Json(new { success = true, message = "✅ Ticket onaylandı ve müşteri kaydı oluşturuldu!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        // Reject: Operasyon tarafından red (Operasyon için)
        [HttpPost]
        [Authorize(Roles = "Operasyon")]
        public IActionResult Reject([FromBody] ApproveRejectRequest request)
        {
            if (request?.Id <= 0)
                return Json(new { success = false, message = "Geçersiz Ticket ID!" });

            var ticket = _context.Tickets.FirstOrDefault(t => t.Id == request.Id);
            if (ticket == null)
                return Json(new { success = false, message = "Ticket bulunamadı!" });

            try
            {
                var userId = int.TryParse(User.FindFirst("UserId")?.Value, out var uid) ? uid : 0;

                ticket.Durum = "Reddedildi";
                ticket.OnaylayanUserId = userId;
                ticket.OnaylayanKullaniciAdi = User.Identity?.Name ?? "Bilinmiyor";
                ticket.OnaylamaTarihi = DateTime.UtcNow;
                ticket.KararAciklamasi = request.KararAciklamasi ?? "Açıklama yapılmadı";

                _context.SaveChanges();

                return Json(new { success = true, message = "❌ Ticket reddedildi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }
    }

    public class ApproveWithTechRequest
    {
        public int Id { get; set; }
        public string? TeknolojiBilgisi { get; set; }
        public string? KararAciklamasi { get; set; }
    }

    public class ApproveRejectRequest
    {
        public int Id { get; set; }
        public string? KararAciklamasi { get; set; }
    }
}
