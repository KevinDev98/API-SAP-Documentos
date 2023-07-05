using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using API_SAP_Corodado.Models;
using SAPbobsCOM;

namespace API_SAP_Corodado.Controllers
{
    public class CotizacionController : ApiController
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
        ConexionBD DN = new ConexionBD();

        public CotizacionController()
        {
            server = ConfigurationManager.AppSettings["ServidorSQL"];
            user = ConfigurationManager.AppSettings["UsuarioSQL"];
            bd = ConfigurationManager.AppSettings["database"];
            pass = ConfigurationManager.AppSettings["PasswordSQL"];

            LicenseServer = ConfigurationManager.AppSettings["LicenseServer"];
            DbUserName = ConfigurationManager.AppSettings["usuario"];
            DbPassword = ConfigurationManager.AppSettings["pass"];
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
                bitacora.CrearBitacora(ex.Message + ": En conexión de SAP");

            }
            return response;
        }

        [HttpPost]
        public Resultado PostCotizacion(CotizacionOF json)
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
                    Documents oCotizacion;
                    oCotizacion = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oQuotations);
                    oCotizacion.CardCode = CardCode;
                    oCotizacion.DocDueDate = Convert.ToDateTime(DocDueDate);
                    oCotizacion.Comments = "Cotización generada para venta de la ECCOMERCE";
                    foreach (var linea in json.lineas)
                    {
                        //                        double price = linea.price;                                            
                        oCotizacion.Lines.ItemCode = linea.ItemCode;
                        oCotizacion.Lines.Quantity = linea.quantity;
                        oCotizacion.Lines.Add();
                        //OPedido.Lines.Price = linea.price;                        

                        // OPedido.SaveXML("C:\\bitacora\\prueba.xml");
                        // MessageBox.Show("Documento " + oCompany.GetNewObjectKey().ToString() + " creado con éxito");
                    }
                    lErrCode = oCotizacion.Add();
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
                        //oCompany.Disconnect();
                        // response.error = false;
                        response.message = "Cotización generada con exito ";                        
                        string DocNum = DN.EjecutarQuery("select  top 1 DocEntry from OQUT where Comments='Cotización generada para venta de la ECCOMERCE' order by DocEntry desc");
                        //response.EntryActivity = " Pedido : " + oCompany.GetNewObjectKey().ToString();
                        response.EntryActivity = " Cotización : " + DocNum;
                        string rutaXML = ConfigurationManager.AppSettings["GuardaXML"];
                        oCompany.GetNewObjectKey().ToString();
                        string guardaXML = rutaXML + "Cotizacion" + DocNum + ".xml";
                        oCotizacion.SaveXML(guardaXML);
                        //----LINEAS AÑADIDAS 28-10-2020
                        if (oCotizacion.GetByKey(Convert.ToInt32(DocNum)))
                        {
                            Documents PedidoCop;
                            PedidoCop= (Documents)oCompany.GetBusinessObject(BoObjectTypes.oOrders);
                            PedidoCop.CardCode = oCotizacion.CardCode;
                            PedidoCop.DocDate = DateTime.Today;

                            PedidoCop.TaxDate = DateTime.Today;

                            PedidoCop.Confirmed = SAPbobsCOM.BoYesNoEnum.tYES;

                            PedidoCop.TransportationCode = oCotizacion.TransportationCode;

                            PedidoCop.ShipToCode = oCotizacion.ShipToCode;

                            PedidoCop.SalesPersonCode = oCotizacion.SalesPersonCode;

                            PedidoCop.NumAtCard = oCotizacion.NumAtCard;

                            PedidoCop.ContactPersonCode = oCotizacion.ContactPersonCode;
                            for (int i = 0; i < oCotizacion.Lines.Count; i++)

                            {

                                oCotizacion.Lines.SetCurrentLine(i);

                                PedidoCop.Lines.BaseEntry = oCotizacion.DocEntry;

                                PedidoCop.Lines.BaseLine = i;

                                PedidoCop.Lines.BaseType = (int)SAPbobsCOM.BoObjectTypes.oQuotations;

                                PedidoCop.Lines.Add();
                                
                            }

                            PedidoCop.DocDueDate = DateTime.Today;

                            int res = PedidoCop.Add();
                            if (res==0)
                            {
                                
                                response.message = "Cotización-Pedido generados con exito ";
                                ConexionBD DN = new ConexionBD();
                                string DocNum1 = DN.EjecutarQuery("select  top 1 DocEntry from ORDR where Comments='Articulo Vendido desde la ECCOMERCE' order by DocEntry desc");
                                //response.EntryActivity = " Pedido : " + oCompany.GetNewObjectKey().ToString();
                                response.EntryActivity = " Pedido : " + DocNum1;
                                string rutaXML1 = ConfigurationManager.AppSettings["GuardaXML"];
                                oCompany.GetNewObjectKey().ToString();
                                string guardaXML1 = rutaXML + "PEDIDO" + DocNum + ".xml";
                                PedidoCop.SaveXML(guardaXML1);
                                if (PedidoCop.GetByKey(Convert.ToInt32(DocNum1)))
                                {
                                    Documents FacturaCop;
                                    FacturaCop = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oInvoices);
                                    FacturaCop.CardCode = PedidoCop.CardCode;
                                    FacturaCop.DocDate = DateTime.Today;

                                    FacturaCop.TaxDate = DateTime.Today;

                                    FacturaCop.Confirmed = SAPbobsCOM.BoYesNoEnum.tYES;

                                    FacturaCop.TransportationCode = PedidoCop.TransportationCode;

                                    FacturaCop.ShipToCode = PedidoCop.ShipToCode;

                                    FacturaCop.SalesPersonCode = PedidoCop.SalesPersonCode;

                                    FacturaCop.NumAtCard = PedidoCop.NumAtCard;

                                    FacturaCop.ContactPersonCode = PedidoCop.ContactPersonCode;
                                    for (int i = 0; i < PedidoCop.Lines.Count; i++)

                                    {
                                        string DistNumber = DN.EjecutarQuery("select TOP 1 DistNumber from OBTN A INNER JOIN OITM B ON A.ItemCode=B.ItemCode Where B.ItemCode='" + oCotizacion.Lines.ItemCode + "' order by InDate desc");
                                        FacturaCop.Lines.SetCurrentLine(i);

                                        FacturaCop.Lines.BaseEntry = PedidoCop.DocEntry;

                                        FacturaCop.Lines.BaseLine = i;

                                        FacturaCop.Lines.BaseType = (int)SAPbobsCOM.BoObjectTypes.oOrders;
                                        FacturaCop.Lines.BatchNumbers.BatchNumber = DistNumber;
                                        FacturaCop.Lines.BatchNumbers.Quantity = oCotizacion.Lines.Quantity;
                                        FacturaCop.Lines.BatchNumbers.Add();
                                        FacturaCop.Lines.Add();

                                    }

                                    FacturaCop.DocDueDate = DateTime.Today;

                                    int res1 = FacturaCop.Add();
                                    if (res1==0)
                                    {
                                        oCompany.Disconnect();
                                        // response.error = false;                        
                                        response.message = "Factura generada con exito ";

                                        string DocNum2 = DN.EjecutarQuery("select  top 1 DocNum from OINV where Comments='Articulo Vendido desde la ECCOMERCE' order by DocEntry desc");
                                        //response.EntryActivity = " Pedido : " + oCompany.GetNewObjectKey().ToString();
                                        response.EntryActivity = " Factura : " + DocNum;
                                        string rutaXML2 = ConfigurationManager.AppSettings["GuardaXML"];
                                        oCompany.GetNewObjectKey().ToString();
                                        string guardaXML2 = rutaXML + "Factura" + DocNum + ".xml";
                                        FacturaCop.SaveXML(guardaXML2);
                                    }
                                    else
                                    {
                                        int temp_int = lErrCode;
                                        string temp_string = sErrMsg;
                                        oCompany.GetLastError(out temp_int, out temp_string);
                                        response.error = true;
                                        response.message = temp_string;
                                        bitacora.CrearBitacora(temp_string + ": En la creación del documento");

                                    }
                                }
                            }
                            else
                            {
                                int temp_int = lErrCode;
                                string temp_string = sErrMsg;
                                oCompany.GetLastError(out temp_int, out temp_string);
                                response.error = true;
                                response.message = temp_string;
                                bitacora.CrearBitacora(temp_string + ": En la creación del documento");
                                
                            }
                        }

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
