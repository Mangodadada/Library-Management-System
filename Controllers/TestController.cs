using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Data.SqlClient;

namespace library_admin.Controllers
{
    public class TestController : Controller
    {
        // GET: Test
        public ActionResult Index()
        {
            OracleConnection conn = null;
            try
            {
                conn = OpenConn();
                var cmd = conn.CreateCommand();
                cmd.CommandText = "select * from test";
                cmd.CommandType = CommandType.Text;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine(string.Format("AwbPre:{0},AwbNo:{1}", reader["AwbPre"], reader["AwbNo"]));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                CloseConn(conn);
            }
            Console.Read();
            //string connString = ConfigurationManager.ConnectionStrings["Conn"].ToString();
            //DataSet ds = new DataSet();
            //string sql= "select capacity from library where library_name=XXX大学图书馆";
            //using (OracleConnection conn = new OracleConnection(connString))
            //{
            //    OracleCommand cmd = new OracleCommand(sql, conn);
            //    OracleDataAdapter adapter = new OracleDataAdapter(cmd);
            //    adapter.Fill(ds);        
            //}
            //Console.WriteLine(ds);
            return View();
        }
        static OracleConnection OpenConn()
        {
            OracleConnection conn = new OracleConnection();
            conn.ConnectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=43.142.16.90)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)));Persist Security Info=True;User ID=system;Password=456fourfivesix;";
            conn.Open();
            return conn;
        }

        static void CloseConn(OracleConnection conn)
        {
            if (conn == null) { return; }
            try
            {
                if (conn.State != ConnectionState.Closed)
                {
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                conn.Dispose();
            }
        }
    }
}