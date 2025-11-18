using System;
using System.Collections.Generic;

namespace BtOperasyonTakip.Models
{
    public class DashboardViewModel
    {
        public int ToplamMusteri { get; set; }
        public int AktifMusteri { get; set; }
        public int PasifMusteri { get; set; }
        public int BuAyEklenen { get; set; }
        public int Bekleyen { get; set; }

        public List<int> AylikMusteriSayilari { get; set; } = new();
        public List<string> AyEtiketleri { get; set; } = new();

        public int JiraBeklemede { get; set; }
        public int JiraAktif { get; set; }
        public int JiraTamamlandi { get; set; }


        public List<Musteri> Musteriler { get; set; } = new();
        public List<JiraTask> JiraTasks { get; set; } = new();

        public List<ToplantiNotu> ToplantiNotlari { get; set; } = new();
    }
}
