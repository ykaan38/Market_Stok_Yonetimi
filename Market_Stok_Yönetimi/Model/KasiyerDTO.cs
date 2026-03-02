using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Market_Stok_Yönetimi.Model
{
    public class KasiyerDTO
    {
        public int KasiyerID { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public int? KasaID { get; set; }
        public string KasaAdi { get; set; }
    }
}