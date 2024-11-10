using System;
using System.Data.SqlClient;
using System.Data;
using System.Text;

namespace BaseDatos
{
    public class BDatos:DataBase
    {
        
        private string m_orgname;
        public string Orgname
        {
            get { return m_orgname; }
            set { m_orgname = value; }
        }
        public BDatos(): base()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public BDatos(String connectionStringName): base(connectionStringName)
        {
            Orgname = connectionStringName;
        }
        public int RunProcedureT(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            return RunProcedure(storedProcName, parameters, out rowsAffected);
        }
        public SqlDataReader RunProcedureT(string storedProcName, IDataParameter[] parameters)
        {
            return RunProcedure(storedProcName, parameters);
        }
    }
    public class Utilizar
    {
        public Utilizar()
        {
        }
        public static void Nuevo()
        {
            BDatos bDatos = new BDatos();
        }
    }
}