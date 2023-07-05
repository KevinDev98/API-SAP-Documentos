using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API_SAP_Corodado.Models
{
    public class Resultado
    {
        public Boolean error { get; set; }
        public string message { get; set; }
        public string EntryActivity { get; set; }
    }
}