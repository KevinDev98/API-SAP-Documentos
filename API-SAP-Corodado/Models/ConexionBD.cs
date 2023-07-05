using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data.Common;
using API_SAP_Corodado.Models;
using System.Data;
using System.Configuration;

namespace API_SAP_Corodado.Models
{
    public class ConexionBD
    {
        private String con;
        private SqlConnection MiConexion = new SqlConnection();
        string server, bd, user, pass;
        String resultado;
        public static SqlConnection Conectar(string server, string bd, string user, string pass)
        {
            SqlConnection conexion = new SqlConnection("server='" + server + "';database='" + bd + "';uid='" + user + "';password='" + pass + "'"); //Cadena de conexion para las consultas
            conexion.Open();//Se abre la conexion para consumir la BD
            return conexion;
        }

        public static string DesEncriptar(string _cadenaAdesencriptar)
        {
            string result = string.Empty;
            byte[] decryted =
            Convert.FromBase64String(_cadenaAdesencriptar);
            System.Text.Encoding.Unicode.GetString(decryted, 0, decryted.ToArray().Length);
            result = System.Text.Encoding.Unicode.GetString(decryted);
            return result;
        }

        public DbDataReader GetDataReader(string procedureName, List<DbParameter> parameters, SqlConnection connection, CommandType commandType = CommandType.StoredProcedure)
        {
            DbDataReader ds;
            Bitacora bitacora = new Bitacora();

            try
            {

                {
                    DbCommand cmd = this.GetCommand(connection, procedureName, commandType);
                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }

                    ds = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
            }
            catch (Exception ex)
            {
                bitacora.CrearBitacora(ex.Message + "Data Reader ConexiónBD");
                throw;
            }

            return ds;
        }

        public DbCommand GetCommand(DbConnection connection, string commandText, CommandType commandType)
        {
            SqlCommand command = new SqlCommand(commandText, connection as SqlConnection);
            command.CommandType = commandType;
            return command;
        }

        public string EjecutarQuery(string sql)
        {
            string result;

            try
            {

                string server = ConfigurationManager.AppSettings["ServidorSQL"];
                string user = ConfigurationManager.AppSettings["UsuarioSQL"];
                string bd = ConfigurationManager.AppSettings["database"];
                string pass = ConfigurationManager.AppSettings["PasswordSQL"];

                //Conectar(server, bd, user, pass);
                con = "data source=" + server + ";initial catalog='" + bd + "'; User Id='" + user + "';password='" + pass + "'"; //Cadena de conexion para las consultas                
                MiConexion.ConnectionString = con;
                MiConexion.Open();
                SqlCommand Exec1 = new SqlCommand(sql, MiConexion);

                string Etiqueta;
                Etiqueta = Convert.ToString(Exec1.ExecuteScalar());
                result = Etiqueta;
                MiConexion.Close();
                resultado = result;
            }
            catch (Exception ex)
            {
                // Bitacora.CrearBitacora(ex.Message);
                MiConexion.Close();
            }
            return resultado;
        }
    }
}