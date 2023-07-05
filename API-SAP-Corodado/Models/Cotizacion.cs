using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_SAP_Corodado.Models
{

    public class Cotizacion
    {
        public string CardCode;        

    }

    public class Lineas
        {            
            public int quantity = 0; 
            public string ItemCode = "";
            //public double price = 0;                    
        }

        public class CreaCotizacion
        {
            public string CardCode;
            public List<Lineas> lineas;               
        }

        public class VerCotizacion
        {
        public Cotizacion cotizacion;
        public List<Lineas> Lineas;
        }
}