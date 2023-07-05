using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_SAP_Corodado.Models
{
    public class CotizacionOF
    {
        public string CardCode { get; set; }
        public string DocDueDate { get; set; }

        public List<Lineas2> lineas;
    }
}