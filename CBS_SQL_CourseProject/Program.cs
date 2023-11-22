using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using System.Configuration.Provider;

namespace CBS_SQL_CourseProject
{
    public static class Program
    {
        public static SqlConnection con;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            var connection = ConfigurationManager.ConnectionStrings["MyData"].ConnectionString;

            con = new SqlConnection(connection);

            con.Open();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }


    }
}
