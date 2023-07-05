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
    public class BusinessPartnerController : ApiController
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
        public BusinessPartnerController()
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
        public Resultado PostBusinessPartner(BPdatos json)
        {
            Resultado response = new Resultado();
            oCompany = new Company();
            oCompany.GetNewObjectKey().ToString();
            Random rand = new Random();
            ConexionBD cons = new ConexionBD();
            string BPregis=cons.EjecutarQuery("select COUNT(*) from OCRD");
            
            string CardCode = Convert.ToString(DateTime.Now.Year)+"-"+ BPregis + "-" + Convert.ToString((char)rand.Next('A', 'Z'));
            string CardName = json.CardName;
            string RFC = json.RFC;
            string Phone = json.Phone1;
            string Cel = json.Cellular;
            string E_mail = json.E_Mail;
            string Password = json.Password;
            //string ID_Addres = json.Address;
            string Street, Block, City, ZipCode, County;
            Street = json.Calle;
            Block = json.Colonia;
            City = json.City;
            ZipCode = json.CodPost;
            County = json.DelMuni;
//            Country = json.Country;

            

            var respuesta = ConectaSAP(1);
            try
            {
                if (respuesta.error == false)
                {
                    BusinessPartners OBP;
                    OBP = (BusinessPartners)oCompany.GetBusinessObject(BoObjectTypes.oBusinessPartners);
                    OBP.CardType = BoCardTypes.cCustomer;
                    OBP.CardCode = CardCode;
                    OBP.CardName = CardName;
                    OBP.FederalTaxID = RFC;
                    OBP.Phone1 = Phone;
                    OBP.Cellular = Cel;
                    OBP.EmailAddress = E_mail;
                    OBP.Password = Password;
                    OBP.Address = Street;
                    OBP.Block = Block;
                    OBP.City = City;
                    OBP.ZipCode = ZipCode;
                    OBP.County = County;
                    OBP.PayTermsGrpCode=8;
                    //OBP.Country = Country;
                    
                    

                    lErrCode = OBP.Add();
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
                        //response.error = false;
                        response.message = "Socio de negocios creado con exito ";
                        ConexionBD BP = new ConexionBD();
                        string Cliente = BP.EjecutarQuery("Select TOP 1 CardCode from OCRD Order by CreateDate desc");
                        response.EntryActivity = " NUEVO CLIENTE : " + oCompany.GetNewObjectKey().ToString();
                        //response.EntryActivity = " NUEVO CLIENTE : " + Cliente;

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
