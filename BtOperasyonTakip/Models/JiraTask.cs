using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BtOperasyonTakip.Models
{
    public class JiraTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string JiraId { get; set; }

        [Required]
        public string TalepKonusu { get; set; }

        public string TalepAcan { get; set; }

        public string Durum { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

        public ICollection<JiraYorum>? Yorumlar { get; set; }
    }
}
