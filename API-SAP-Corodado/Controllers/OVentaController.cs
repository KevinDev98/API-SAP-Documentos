using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using API_SAP_Corodado.Models;
using System.Data.Common;
using System.Data;
using System.Xml;

namespace API_SAP_Corodado.Controllers
{
    
    public class OVentaController : ApiController
    {
        string server;
        string user;
        string bd;
        string pass;

        public string sErrMsg;
        public int lErrCode;
        public int lRetCode;
        public Boolean Conectado = false;
        public Company oCompany;
        string LicenseServer;
        string DbUserName;
        string DbPassword;
        string TipoSQL;

        Bitacora bitacora = new Bitacora();
       
        /// Esta función "desencripta" la cadena que le envíamos en el parámentro de entrada.
        

        public OVentaController()
        {
            server = ConfigurationManager.AppSettings["ServidorSQL"];
            user = ConfigurationManager.AppSettings["UsuarioSQL"];
            bd = ConfigurationManager.AppSettings["database"];
            pass = ConfigurationManager.AppSettings["PasswordSQL"];
           
            LicenseServer = ConfigurationManager.AppSettings["LicenseServer"];
            DbUserName = ConfigurationManager.AppSettings["usuario"];
            DbPassword =ConfigurationManager.AppSettings["pass"];
            TipoSQL = ConfigurationManager.AppSettings["TipoSQL"];

        }

        private Resultado ConectaSAP(int tipo)
        {
            Resultado response = new Resultado();
            try
            {                
                oCompany = new Company();

                oCompany.Server = server;
                oCompany.LicenseServer = LicenseServer;
                oCompany.DbUserName = user;
                oCompany.DbPassword = pass;

                if (TipoSQL == "2008")
                {
                    oCompany.DbServerType = (SAPbobsCOM.BoDataServerTypes.dst_MSSQL2008);
                }
                else if (TipoSQL == "2012")
                {
                    oCompany.DbServerType = (SAPbobsCOM.BoDataServerTypes.dst_MSSQL2012);                    
                }
                else if (TipoSQL == "2014")
                {
                    oCompany.DbServerType = (SAPbobsCOM.BoDataServerTypes.dst_MSSQL2014);
                }
                else if (TipoSQL == "2016")
                {
                    oCompany.DbServerType = (SAPbobsCOM.BoDataServerTypes.dst_MSSQL2016);
                }

                oCompany.UseTrusted = false;
                oCompany.CompanyDB = bd;
                oCompany.UserName = DbUserName;
                oCompany.Password = DbPassword;

                // Connecting to a company DB
                lRetCode = oCompany.Connect();

                if (lRetCode != 0)//Si la conexión da respuesta diferente de Cero "0" manda mensaje de error
                {
                    int temp_int = lErrCode;
                    string temp_string = sErrMsg;
                    oCompany.GetLastError(out temp_int, out temp_string);
                    response.error = true;
                    response.message = temp_string;
                    oCompany.Disconnect();
                    bitacora.CrearBitacora(temp_string + "; En conexión de SAP");
                   
                }
                else
                {
                    response.error = false;
                    response.message = "CONEXION EXITOSA!";//Si la conexión devuelve un valor "0" entonces se realiza una conexión exitosaq
                    // Disable controls
                }
            }
            catch (Exception ex)
            {
                oCompany.Disconnect();
                response.error = true;
                response.message = ex.Message;
                bitacora.CrearBitacora(ex.Message +  ": En conexión de SAP");
               
            }
            return response;
        }
                
        [HttpPost]
        public Resultado PostCotizacion(OrdenVenta json)
        {
            Resultado response = new Resultado();
            oCompany = new Company();
            oCompany.GetNewObjectKey().ToString();
            string CardCode = json.CardCode;
            string DocDueDate = json.DocDueDate;            

            var respuesta = ConectaSAP(1);
            try
             {
                if (respuesta.error == false)
                {
                    Documents OPedido;
                    OPedido = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oOrders);
                    Documents OcOT;
                    OcOT = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oOrders);
                    OPedido.CardCode = CardCode;                   
                    OPedido.DocDueDate = Convert.ToDateTime(DocDueDate);
                    OPedido.Comments = "Articulo Vendido desde la ECCOMERCE";
                    OPedido.Confirmed = SAPbobsCOM.BoYesNoEnum.tYES;
                    foreach (var linea in json.lineas)
                    {
                        OcOT.Lines.SetCurrentLine(0);              
                        //OPedido.Lines.ItemCode = linea.ItemCode;
                        //OPedido.Lines.Quantity = linea.quantity;
                        OPedido.Lines.BaseEntry = 61849;
                        OPedido.Lines.BaseLine = 0;
                        OPedido.Lines.BaseType = -1;                        
                        //OPedido.Lines.Add();
                    }
                    lErrCode = OPedido.Add();
                    if (lErrCode != 0)
                    {
                        int temp_int = lErrCode;
                        string temp_string = sErrMsg;
                        oCompany.GetLastError(out temp_int, out temp_string);
                        response.error = true;
                        response.message = temp_string;
                        bitacora.CrearBitacora(temp_string + ": En la creación del documento");
                        
                    }
                    else
                    {
                        oCompany.Disconnect();                        
                        // response.error = false;
                        response.message = "Pedido generado con exito " ;
                        ConexionBD DN = new ConexionBD();
                        string DocNum = DN.EjecutarQuery("select  top 1 DocEntry from ORDR where Comments='Articulo Vendido desde la ECCOMERCE' order by DocEntry desc");
                        //response.EntryActivity = " Pedido : " + oCompany.GetNewObjectKey().ToString();
                        response.EntryActivity = " Pedido : " + DocNum;
                        string rutaXML = ConfigurationManager.AppSettings["GuardaXML"];
                        oCompany.GetNewObjectKey().ToString();
                        string guardaXML = rutaXML + "PEDIDO" + DocNum + ".xml";
                        OPedido.SaveXML(guardaXML);

                    }
                }
            }
            catch (Exception ex)
            {
                //response.error = true;
                response.message = ex.Message;
                //----
                bitacora.CrearBitacora(ex.Message + ": En la creación del Documento");
               
            }

            return response;
        }

    }
}
