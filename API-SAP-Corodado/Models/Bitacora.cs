using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace API_SAP_Corodado.Models
{
    public class Bitacora
    {
        public void CrearBitacora(string cadena)
        {
            string fecha = DateTime.Now.ToString();

            string path = ConfigurationManager.AppSettings["rutaBitacora"] + "bitacora_ApiGramosa";
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    sw.WriteLine("**** BITÁCORA DE ERRORES Y EXCEPCIONES  ****");
                    sw.WriteLine(fecha + ": " + cadena);

                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(fecha + ": " + cadena);
                }
            }



        }
    }
}