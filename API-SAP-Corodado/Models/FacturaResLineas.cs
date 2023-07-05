using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_SAP_Corodado.Models
{
    public class FacturaResLineas
    {
        public string CardCode;
    }
        public class Lineas1
        {
            public int quantity = 0;
            public string ItemCode = "";
        
            //public double price = 0;                    
        }

        public class CreaFactura
        {
            public string CardCode;
            public List<Lineas1> lineas;
        }

        public class VerFactura
        {
            public Cotizacion cotizacion;
            public List<Lineas> Lineas;
        }
    
}