using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_SAP_Corodado.Models
{
    public class CotizacionOFlineas
    {
        public string CardCode;
    }
    public class Lineas2
    {
        public int quantity = 0;
        public string ItemCode = "";
        //public double price = 0;                    
    }

    public class CreaCotizacion2
    {
        public string CardCode;
        public List<Lineas> lineas;
    }

    public class VerCotizacion2
    {
        public Cotizacion cotizacion;
        public List<Lineas> Lineas;
    }
}