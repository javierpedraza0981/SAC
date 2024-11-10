using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using NavistarPagos.Data;
/// <summary>
/// Summary description for DataBase
/// </summary>
public abstract class DataBase
{
    
    protected SqlConnection Connection;
    private string m_connectionString;
    Validaciones mod = new Validaciones();

    public DataBase()
    {
        m_connectionString = ConfigurationManager.ConnectionStrings["bd"].ConnectionString;
        Connection = new SqlConnection(m_connectionString);
    }
    public DataBase(string connectionStringName)
    {
        try
        {
            m_connectionString = connectionStringName;
            Connection = new SqlConnection(m_connectionString);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message + " - " + connectionStringName);
        }
    }
    protected string ConnectionString
    {
        get
        {
            return m_connectionString;
        }
    }
    public static SqlParameter NewParameter(string parameterName, object dato, string tipo)
    {
        SqlParameter sqlParameter = null;
        if (tipo == "output")
        {
            if (dato.GetType().FullName.IndexOf("String") >= 0)
            {
                sqlParameter = new SqlParameter(parameterName, SqlDbType.VarChar, dato.ToString().Length);
                sqlParameter.Value = dato.ToString();
            }
            else
            {
                sqlParameter = new SqlParameter(parameterName, dato);
            }
            sqlParameter.Direction = ParameterDirection.Output;
        }
        else
        {
            sqlParameter = new SqlParameter(parameterName, dato);
        }
        return sqlParameter;
    }
    private SqlCommand BuildIntCommand(string storedProcName, IDataParameter[] parameters)
    {
        SqlCommand command = BuildQueryCommand(storedProcName, parameters);
        command.Parameters.Add(new SqlParameter("ReturnValue", SqlDbType.Int, 4, /* Size */
            ParameterDirection.ReturnValue, false, /* is nullable */
            0, /* byte precision */
            0, /* byte scale */
            string.Empty, DataRowVersion.Default, null));
        return command;
    }
    private SqlCommand BuildQueryCommand(string storedProcName, IDataParameter[] parameters)
    {
        return BuildQueryCommand(storedProcName, parameters, CommandType.StoredProcedure);
    }
    private SqlCommand BuildQueryCommand(string cmd, IDataParameter[] parameters, CommandType commandType)
    {
        SqlCommand command = new SqlCommand(cmd, Connection);
        command.CommandTimeout = 200;
        command.CommandType = commandType;
        foreach (SqlParameter parameter in parameters)
        {
            if (parameter != null)
            {
                int pos = cmd.IndexOf("?");
                if (pos >= 0) cmd = cmd.Substring(0, pos) + parameter.ParameterName + cmd.Substring(pos + 1);
                command.Parameters.Add(parameter);
            }
        }
        command.CommandText = cmd;
        return command;
    }

    #region RunProcedure

    public SqlDataReader RunCommand(string sqlCommand, IDataParameter[] parameters)
    {
        SqlDataReader returnReader;
        try
        {
            if (Connection.State == ConnectionState.Closed) Connection.Open();
            SqlCommand command = BuildQueryCommand(sqlCommand, parameters, CommandType.Text);
            returnReader = command.ExecuteReader();
        }
        catch (SqlException ex)
        {
            InsertaError(sqlCommand, parameters, ex);
            throw new Exception(ex.Message);
        }
        return returnReader;
    }
    public DataTable RunQueryDT(string sqlCommand, IDataParameter[] parameters)
    {
        DataTable dt = new DataTable("datos");
        try
        {
            if (Connection.State == ConnectionState.Closed) Connection.Open();
            SqlDataAdapter adap = new SqlDataAdapter(sqlCommand, Connection);
            adap.Fill(dt);
        }
        catch (SqlException ex)
        {
            InsertaError(sqlCommand, parameters, ex);
            throw (new Exception(sqlCommand + " " + ex.Message));
        }
        return dt;
    }
    public int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
    {
        try
        {
            int result;
            Connection.Open();
            
            SqlCommand command = BuildIntCommand(storedProcName, parameters);
            rowsAffected = command.ExecuteNonQuery();
            result = (int)command.Parameters["ReturnValue"].Value;
            return result;
        }
        catch (SqlException ex)
        {
            if (storedProcName.IndexOf("_Error_") < 0)
            {
                InsertaError(storedProcName, parameters, ex);
            }
            throw new Exception(ex.Message);
        }
        finally
        {
            if (Connection.State == ConnectionState.Open) Connection.Close();
        }
    }
    public int RunProcedure(string storedProcName, IDataParameter[] parameters, out string mensaje, out int error)
    {
        mensaje = "";
        error = 0;
        int rowsAffected = 0;
        int salida = RunProcedure(storedProcName, parameters, out rowsAffected);
        mensaje = parameters[parameters.Length - 2].Value.ToString();
        error = int.Parse(parameters[parameters.Length - 1].Value.ToString());
        if (error != 0)
        {
            throw (new Exception("Error: " + error.ToString() + " " + mensaje));
        }
        return salida;
    }
    public SqlDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
    {
        SqlDataReader returnReader;
        try
        {
            Connection.Open();
            SqlCommand command = BuildQueryCommand(storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
        }
        catch (SqlException ex)
        {
            InsertaError(storedProcName, parameters, ex);
            throw new Exception(ex.Message);
        }
        finally
        {
            if (Connection.State == ConnectionState.Open) Connection.Close();
        }
        return returnReader;
    }

    public DataSet RunProcedure2(string storedProcName, IDataParameter[] parameters, string tableName, out string _connect)
    {
        try
        {


            DataSet dataSet = new DataSet();
            Connection.Open();
            _connect = Connection.ConnectionString;
            //SqlDataAdapter sqlDA = new SqlDataAdapter();

            //sqlDA.SelectCommand = BuildQueryCommand(storedProcName, parameters);
            //sqlDA.Fill(dataSet, tableName);
            Connection.Close();
            return dataSet;
        }
        catch (SqlException ex)
        {
            InsertaError(storedProcName, parameters, ex);
            throw new Exception(ex.Message);
        }
        finally
        {
            if (Connection.State == ConnectionState.Open) Connection.Close();
        }
    }
    public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName, out string _connect)
    {
        try
        {


            DataSet dataSet = new DataSet();
            Connection.Open();
            _connect = Connection.ConnectionString;
            SqlDataAdapter sqlDA = new SqlDataAdapter();

            sqlDA.SelectCommand = BuildQueryCommand(storedProcName, parameters);
            sqlDA.Fill(dataSet, tableName);
            return dataSet;
        }
        catch (SqlException ex)
        {
            InsertaError(storedProcName, parameters, ex);
            throw new Exception(ex.Message);
        }
        finally
        {
            if (Connection.State == ConnectionState.Open) Connection.Close();
        }
    }
    public DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
    {
        try
        {
            DataSet dataSet = new DataSet();
            Connection.Open();
            mod.Log_Diario("DataBase", Connection.ConnectionString);
            SqlDataAdapter sqlDA = new SqlDataAdapter();

            sqlDA.SelectCommand = BuildQueryCommand(storedProcName, parameters);

            mod.Log_Diario("DataBase", storedProcName);
            sqlDA.Fill(dataSet, tableName);
            return dataSet;
        }
        catch (SqlException ex)
        {
            //InsertaError(storedProcName, parameters, ex);
            mod.Log_Diario("RunProcedure Error", ex.Message);
            throw new Exception(ex.Message);
        }
        finally
        {
            if (Connection.State == ConnectionState.Open) Connection.Close();
        }
    }
    public void RunProcedure(string storedProcName, IDataParameter[] parameters, DataSet dataSet, string tableName)
    {
        try
        {
            Connection.Open();
            SqlDataAdapter sqlDA = new SqlDataAdapter();
            sqlDA.SelectCommand = BuildIntCommand(storedProcName, parameters);
            sqlDA.Fill(dataSet, tableName);
        }
        catch (SqlException ex)
        {
            InsertaError(storedProcName, parameters, ex);
            throw new Exception(ex.Message);
        }
        finally
        {
            if (Connection.State == ConnectionState.Open) Connection.Close();
        }
    }

    #endregion

    protected String GetSqlExceptionMessage(SqlException ex)
    {
        StringBuilder errorMessages = new StringBuilder();
        for (int i = 0; i < ex.Errors.Count; i++)
        {
            errorMessages.Append("Index #" + i + "\n" +
                "Message: " + ex.Errors[i].Message + "\n" +
                "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                "Source: " + ex.Errors[i].Source + "\n" +
                "Procedure: " + ex.Errors[i].Procedure + "\n");
        }
        return errorMessages.ToString();
    }
    public void InsertaError(string sSQL, IDataParameter[] parameters, SqlException ex)
    {
        string msg = "";
        if (ex != null)
        {
            msg = GetSqlExceptionMessage(ex);
        }
        //PagoEnLinea.Error error = new PagoEnLinea.Error();
        //error.MensajeError = msg;
        //error.FechaAlta = DateTime.Now;
        //if (parameters != null)
        //{
        //    string sep = "Parametros: ";
        //    foreach (SqlParameter parameter in parameters)
        //    {
        //        if (parameter != null)
        //        {
        //            sSQL += sep + parameter.ParameterName + ": " + parameter.SqlValue.ToString();
        //            sep = ", ";
        //        }
        //    }
        //}
        //error.Sentencia = sSQL;
        //PagoEnLinea.Error.Agregar(error);
    }
    public void Close()
    {
        Connection.Close();        
    }
}

