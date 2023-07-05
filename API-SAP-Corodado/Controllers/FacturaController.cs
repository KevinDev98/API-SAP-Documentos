using API_SAP_Corodado.Models;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace API_SAP_Corodado.Controllers
{
    public class FacturaController : ApiController
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
        public FacturaController()
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
        public Resultado PostFactura(FacturaRes json)
        {
            Resultado response = new Resultado();
            oCompany = new Company();
            oCompany.GetNewObjectKey().ToString();
            string CardCode = json.CardCode;
            string DocDueDate = json.DocDueDate;
            string DirEntrega = json.DirEntrega;
            //int Pago = json.Pago;
            var respuesta = ConectaSAP(1);
            try
            {
                if (respuesta.error == false)
                {
                    Documents Factura;
                    Factura = (Documents)oCompany.GetBusinessObject(BoObjectTypes.oInvoices);
                    //Factura.ReserveInvoice = SAPbobsCOM.BoYesNoEnum.tYES;
                    Factura.CardCode = CardCode;
                    Factura.DocDueDate = Convert.ToDateTime(DocDueDate);
                    Factura.Comments = "Articulo Vendido desde la ECCOMERCE";
                    Factura.Address=DirEntrega;
                    //Factura.PaymentGroupCode = Pago;
                    Factura.PaymentGroupCode = 9;
                                       

                    foreach (var linea in json.lineas)
                    {
                        //                        double price = linea.price;                                            
                        string DistNumber = DN.EjecutarQuery("select TOP 1 DistNumber from OBTN A INNER JOIN OITM B ON A.ItemCode=B.ItemCode Where B.ItemCode='" + linea.ItemCode + "' order by InDate desc");
                        Factura.Lines.ItemCode = linea.ItemCode;
                        Factura.Lines.Quantity = linea.quantity;
                        Factura.Lines.BatchNumbers.BatchNumber = DistNumber;
                        Factura.Lines.BatchNumbers.Quantity =linea.quantity;
                        Factura.Lines.BatchNumbers.Add();
                        Factura.Lines.Add();

                        // OPedido.SaveXML("C:\\bitacora\\prueba.xml");
                        // MessageBox.Show("Documento " + oCompany.GetNewObjectKey().ToString() + " creado con éxito");
                    }
                    lErrCode = Factura.Add();
                    
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
                        response.message = "Factura generada con exito ";
                        
                        string DocNum = DN.EjecutarQuery("select  top 1 DocNum from OINV where Comments='Articulo Vendido desde la ECCOMERCE' order by DocEntry desc");
                        //response.EntryActivity = " Pedido : " + oCompany.GetNewObjectKey().ToString();
                        response.EntryActivity = " Factura : " + DocNum;
                        string rutaXML = ConfigurationManager.AppSettings["GuardaXML"];
                        oCompany.GetNewObjectKey().ToString();
                        string guardaXML = rutaXML + "Factura" + DocNum + ".xml";
                        Factura.SaveXML(guardaXML);

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
