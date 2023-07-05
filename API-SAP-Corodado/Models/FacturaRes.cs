using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_SAP_Corodado.Models
{
    public class FacturaRes
    {
        public string CardCode { get; set; }
        public string DocDueDate { get; set; }
        public string DirEntrega { get; set; }
        //public int Pago { get; set; }

        public List<Lineas1> lineas;
    }
}