using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Market_Stok_Yönetimi.Model
{
    public class SatisDTO
    {
        public int SatisID { get; set; }
        public int UrunID { get; set; }
        public string UrunAdi { get; set; }
        public int Miktar { get; set; }
        public decimal BirimFiyat { get; set; }
        public DateTime SatisTarihi { get; set; }
    }
}