using Microsoft.AspNetCore.Mvc;
using BtOperasyonTakip.Data;
using BtOperasyonTakip.Models;
using System.Linq;

namespace BtOperasyonTakip.Controllers
{
    public class ParametreController : Controller
    {
        private readonly AppDbContext _context;

        public ParametreController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var parametreler = _context.Parametreler.OrderBy(p => p.Tur).ToList();
            return View(parametreler);
        }

        [HttpPost]
        public IActionResult Index(Parametre model)
        {
            if (ModelState.IsValid)
            {
                _context.Parametreler.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(_context.Parametreler.ToList());
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var param = _context.Parametreler.FirstOrDefault(p => p.Id == id);
            if (param != null)
            {
                _context.Parametreler.Remove(param);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
