using System;
using System.Data.SqlClient;
using System.Text;
using System.Data;
using System.Collections;
using BaseDatos;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NavistarPagos.Entity;
using NavistarPagos.Controllers;
using NavistarPagos.Models;
using ServiciosCorporativosNfc.Business;
using ServiciosCorporativosNfc.QualitasWs;
using System.Text.RegularExpressions;

//**********************************************************************************/
//** Autor            : Altum Technologies, S.A. de C.V.                         ***/
//** Fecha Creación   : 28/03/2019                                               ***/
//** Función          : Clase para manejar la colección de la claves PagoEnLinea ***/
//**********************************************************************************/

namespace PagoEnLinea
{
	[Serializable]
	public class ColeccionPagoEnLinea : ColeccionBaseBSC
	{

		// Constructor de la ColeccionPagoEnLinea
		public ColeccionPagoEnLinea()
		{
			Buscar();
		}
		public void Buscar()
		{
			BaseDatos.BDatos conector = new BaseDatos.BDatos();
			this.NameClase = "PagoEnLinea.PagoEnLinea";

			String sSQL;
			//Obtenemos todos las Elementos existentes
			sSQL =  "SELECT   id_pagoEnLinea, id_cliente, contrato, referencia, ";
			sSQL += "         montoAPagar, id_origen, usuario, codigoRespuesta, ";
			sSQL += "         autorizacion, nombre_cliente_pago, nombre_banco_emisor, fecha_aplicacion, ";
			sSQL += "         cuenta_deposito, cuenta_pago, codigo_metodo_pago, metodo_pago, ";
			sSQL += "         respuesta, entrada, id_Bitacora, id_estatusTimbrado, id_estatus, fecha_alta, fecha_modificacion, id_estatusNavistar, id_pagoNavistar, respuestaNavistar";
			sSQL += " FROM    pagosEnLinea";
			sSQL += " WHERE   Id_Estatus = 1";
			sSQL += " ORDER   BY contrato";
			SqlParameter[] paramArray = new SqlParameter[1];
			SqlDataReader dr = null;
			try
			{
				// Se recuperan los datos y se agregan a la coleccion
				dr = conector.RunCommand(sSQL, paramArray);
				while (dr.Read())
				{
					PagoEnLinea pagoEnLinea = new PagoEnLinea();
					pagoEnLinea.ClavePagoEnLinea = Int32.Parse(dr.GetValue(0).ToString());
					pagoEnLinea.ClaveCliente = dr.GetInt32(1);
					pagoEnLinea.Contrato = dr.GetString(2);
					pagoEnLinea.Referencia = dr.GetString(3);
					pagoEnLinea.MontoAPagar = dr.GetDecimal(4);
					pagoEnLinea.ClaveOrigen = dr.GetByte(5);
					pagoEnLinea.Usuario = dr.GetString(6);
					pagoEnLinea.CodigoRespuesta = dr.GetString(7);
					pagoEnLinea.Autorizacion = dr.GetString(8);
					pagoEnLinea.NombreClientePago = dr.GetString(9);
					pagoEnLinea.NombreBancoEmisor = dr.GetString(10);
					pagoEnLinea.FechaAplicacion = dr.GetDateTime(11);
					pagoEnLinea.CuentaDeposito = dr.GetString(12);
					pagoEnLinea.CuentaPago = dr.GetString(13);
					pagoEnLinea.CodigoMetodoPago = dr.GetString(14);
					pagoEnLinea.MetodoPago = dr.GetString(15);
					pagoEnLinea.Respuesta = dr.GetString(16);
					pagoEnLinea.Entrada = dr.GetString(17);
					pagoEnLinea.ClaveBitacora = int.Parse(dr.GetValue(18).ToString());
					pagoEnLinea.ClaveEstatusTimbrado = int.Parse(dr.GetValue(19).ToString());
					pagoEnLinea.ClaveEstatus = dr.GetInt16(20);
					pagoEnLinea.FechaAlta = dr.GetDateTime(21);
					pagoEnLinea.FechaModificacion = dr.GetDateTime(22);
					pagoEnLinea.ClaveEstatusNavistar = int.Parse(dr.GetValue(23).ToString());
					pagoEnLinea.ClavePagoNavistar = int.Parse(dr.GetValue(24).ToString());
					pagoEnLinea.RespuestaNavistar = dr.GetString(25);
					this.List.Add(pagoEnLinea);
				}
				dr.Close();
			}
			catch(Exception ex)
			{
				throw(new Exception("PagoEnLinea.Buscar: " + ex.Message));
			}
			finally
			{
				if (dr != null) if (!dr.IsClosed) dr.Close();
				conector.Close();
			}
		}
		/// <summary>
		/// Obtenemos un Elemento por su indice
		/// </summary>
		public PagoEnLinea PorIndice(int indice)
		{
			return this[indice];
		}
		public PagoEnLinea PorLlave(int clave)
		{
			for (int i = 0; i < this.NumeroElementos; i++)
			{
				if(this.PorIndice(i).ClavePagoEnLinea == clave) return this.PorIndice(i);
			}
			return null;
		}
		/// <summary>
		/// Insertamos un elemento PagoEnLinea a la colección
		/// </summary>
		public void InsertaPagoEnLinea(PagoEnLinea pagoEnLinea)
		{
			this.List.Add(pagoEnLinea);
		}

		#region Indexers
		public PagoEnLinea this[int index]
		{
			get{return this.List[index] as PagoEnLinea;}
			set{this.List[index] = value; }
		}
		#endregion
	}

	//****************************************************************************************************/
	//** Función : Definición de la Clase PagoEnLinea con todos sus constructores, atributos y métodos ***/
	//****************************************************************************************************/
	[Serializable]
	public class PagoEnLinea
	{
		/// <summary>
		/// Constructor vacio de la clase, para poder crear una instancia con los atributos default
		/// </summary>
		public PagoEnLinea()
		{
		}

		/// <summary>
		/// Constructor de la clase con la recuperación de sus datos de un registro de la base de datos
		/// </summary>
		public PagoEnLinea(int clavePagoEnLinea)
		{
			String sSQL;
			//Obtenemos el PagoEnLinea que corresponda con la clave que se recibe
			sSQL = "SELECT    id_pagoEnLinea, id_cliente, contrato, referencia, ";
			sSQL += "         montoAPagar, id_origen, usuario, codigoRespuesta, ";
			sSQL += "         autorizacion, nombre_cliente_pago, nombre_banco_emisor, fecha_aplicacion, ";
			sSQL += "         cuenta_deposito, cuenta_pago, codigo_metodo_pago, metodo_pago, ";
			sSQL += "         respuesta, entrada, id_Bitacora, id_estatusTimbrado, id_estatus, fecha_alta, fecha_modificacion, id_estatusNavistar, id_pagoNavistar, respuestaNavistar";
			sSQL += " FROM    pagosEnLinea";
			sSQL += " WHERE   id_pagoEnLinea = @id_pagoEnLinea";
			SqlParameter[] paramArray = new SqlParameter[2];
			paramArray[0] = BDatos.NewParameter("@id_pagoEnLinea", clavePagoEnLinea, "");
			// Creamos una Instancia de una conexion y un comando
			BaseDatos.BDatos conector = new BaseDatos.BDatos();
			SqlDataReader dr = null;
			try
			{
				// Obtenemos los datos y los asignamos a los atributos
				dr = conector.RunCommand(sSQL, paramArray);
				if(dr.Read())
				{
					this.clavePagoEnLinea = Int32.Parse(dr.GetValue(0).ToString());
					this.claveCliente = dr.GetInt32(1);
					this.contrato = dr.GetString(2);
					this.referencia = dr.GetString(3);
					this.montoAPagar = dr.GetDecimal(4);
					this.claveOrigen = Int32.Parse(dr.GetByte(5).ToString());
					this.usuario = dr.GetString(6);
					this.codigoRespuesta = dr.GetString(7);
					this.autorizacion = dr.GetString(8);
					this.nombreClientePago = dr.GetString(9);
					this.nombreBancoEmisor = dr.GetString(10);
					this.fechaAplicacion = dr.GetDateTime(11);
					this.cuentaDeposito = dr.GetString(12);
					this.cuentaPago = dr.GetString(13);
					this.codigoMetodoPago = dr.GetString(14);
					this.metodoPago = dr.GetString(15);
					this.respuesta = dr.GetString(16);
					this.entrada = dr.GetString(17);
					this.claveBitacora = int.Parse(dr.GetValue(18).ToString());
					this.claveEstatusTimbrado = int.Parse(dr.GetValue(19).ToString());
					this.claveEstatus = int.Parse(dr.GetValue(20).ToString());
					this.fechaAlta = dr.GetDateTime(21);
					this.fechaModificacion = dr.GetDateTime(22);
					this.claveEstatusNavistar = int.Parse(dr.GetValue(23).ToString());
					this.clavePagoNavistar = int.Parse(dr.GetValue(24).ToString());
					this.respuestaNavistar = dr.GetString(25);
				}
				dr.Close();
			}
			catch(Exception ex)
			{
				throw(new Exception("PagoEnLinea: " + ex.Message));
			}
			finally
			{
				if (dr != null) if (!dr.IsClosed) dr.Close();
				conector.Close();
			}
		}
		/// <summary>
		/// Método estático para actualizar los datos de la clase en la tabla
		/// </summary>
		public static void Actualizar(PagoEnLinea pagoEnLinea)
		{
			String sSQL = "";
			SqlParameter[] paramArray = new SqlParameter[25];
			// Se actualiza el registro seleccionado
			sSQL = "UPDATE   pagosEnLinea";
			sSQL += " SET    id_cliente = ?,";
			sSQL += "        contrato = ?,";
			sSQL += "        referencia = ?,";
			sSQL += "        montoAPagar = ?,";
			sSQL += "        id_origen = ?,";
			sSQL += "        usuario = ?,";
			sSQL += "        codigoRespuesta = ?,";
			sSQL += "        autorizacion = ?,";
			sSQL += "        nombre_cliente_pago = ?,";
			sSQL += "        nombre_banco_emisor = ?,";
			sSQL += "        fecha_aplicacion = ?,";
			sSQL += "        cuenta_deposito = ?,";
			sSQL += "        cuenta_pago = ?,";
			sSQL += "        codigo_metodo_pago = ?,";
			sSQL += "        metodo_pago = ?,";
			sSQL += "        respuesta = ?,";
			sSQL += "        entrada = ?,";
			sSQL += "        id_bitacora = ?,";
			sSQL += "        id_estatusTimbrado = ?,";
			sSQL += "        id_estatus = ?,";
			sSQL += "        fecha_modificacion = ?,";
			sSQL += "        id_estatusNavistar = ?,";
			sSQL += "        id_pagoNavistar = ?,";
			sSQL += "        respuestaNavistar = ?";
			sSQL += " WHERE  id_pagoEnLinea = ?";
			paramArray[0] = BDatos.NewParameter("@id_cliente", pagoEnLinea.claveCliente, "");
			paramArray[1] = BDatos.NewParameter("@contrato", pagoEnLinea.contrato, "");
			paramArray[2] = BDatos.NewParameter("@referencia", pagoEnLinea.referencia, "");
			paramArray[3] = BDatos.NewParameter("@montoAPagar", pagoEnLinea.montoAPagar, "");
			paramArray[4] = BDatos.NewParameter("@id_origen", pagoEnLinea.claveOrigen, "");
			paramArray[5] = BDatos.NewParameter("@usuario", pagoEnLinea.usuario, "");
			paramArray[6] = BDatos.NewParameter("@codigoRespuesta", pagoEnLinea.codigoRespuesta, "");
			paramArray[7] = BDatos.NewParameter("@autorizacion", pagoEnLinea.autorizacion, "");
			paramArray[8] = BDatos.NewParameter("@nombre_cliente_pago", pagoEnLinea.nombreClientePago, "");
			paramArray[9] = BDatos.NewParameter("@nombre_banco_emisor", pagoEnLinea.nombreBancoEmisor, "");
			paramArray[10] = BDatos.NewParameter("@fecha_aplicacion", pagoEnLinea.fechaAplicacion, "");
			paramArray[11] = BDatos.NewParameter("@cuenta_deposito", pagoEnLinea.cuentaDeposito, "");
			paramArray[12] = BDatos.NewParameter("@cuenta_pago", pagoEnLinea.cuentaPago, "");
			paramArray[13] = BDatos.NewParameter("@codigo_metodo_pago", pagoEnLinea.codigoMetodoPago, "");
			paramArray[14] = BDatos.NewParameter("@metodo_pago", pagoEnLinea.metodoPago, "");
			paramArray[15] = BDatos.NewParameter("@respuesta", pagoEnLinea.respuesta, "");
			paramArray[16] = BDatos.NewParameter("@entrada", pagoEnLinea.entrada, "");
			paramArray[17] = BDatos.NewParameter("@id_bitacora", pagoEnLinea.claveBitacora, "");
			paramArray[18] = BDatos.NewParameter("@id_estatusTimbrado", pagoEnLinea.claveEstatusTimbrado, "");
			paramArray[19] = BDatos.NewParameter("@id_estatus", pagoEnLinea.claveEstatus, "");
			paramArray[20] = BDatos.NewParameter("@fecha_modificacion", pagoEnLinea.fechaModificacion, "");
			paramArray[21] = BDatos.NewParameter("@id_estatusNavistar", pagoEnLinea.claveEstatusNavistar, "");
			paramArray[22] = BDatos.NewParameter("@id_pagoNavistar", pagoEnLinea.clavePagoNavistar, "");
			paramArray[23] = BDatos.NewParameter("@respuestaNavistar", pagoEnLinea.respuestaNavistar, "");
			paramArray[24] = BDatos.NewParameter("@id_pagoEnLinea", pagoEnLinea.clavePagoEnLinea, "");
			string largos = "";
            for(int i=0; i < paramArray.Length; i++)
            {
                if (paramArray[i] != null)
                {
                    SqlParameter par = (SqlParameter)paramArray[i];
                    largos += " " + par.ParameterName.Replace("@", "") + " - " + par.Value.ToString().Length + ". ";
                }
            }
			BaseDatos.BDatos conector = new BaseDatos.BDatos();
			try
			{
				conector.RunCommand(sSQL, paramArray);
			}
			catch(Exception ex)
			{
				throw(new Exception(largos + " " + ex.Message));
			}
			finally
			{
				conector.Close();
			}
		}
		/// <summary>
		/// Método estático para insertar los datos de la clase en la tabla de la base de datos
		/// </summary>
		public static int Agregar(PagoEnLinea pagoEnLinea)
		{
			String sSQL = "";
			String sSQL2 = "";
			// Creamos una Instancia de una conexion y un comando
			BaseDatos.BDatos conector = new BaseDatos.BDatos();
			SqlParameter[] paramArray1 = new SqlParameter[1];
			SqlParameter[] paramArray2 = new SqlParameter[25];
			try
			{
				//Insertamos el Registro
				sSQL2 = "INSERT INTO pagosEnLinea";
				sSQL2 += " (id_cliente, contrato, referencia, ";
				sSQL2 += " montoAPagar, id_origen, usuario, codigoRespuesta, ";
				sSQL2 += " autorizacion, nombre_cliente_pago, nombre_banco_emisor, fecha_aplicacion, ";
				sSQL2 += " cuenta_deposito, cuenta_pago, codigo_metodo_pago, metodo_pago, ";
				sSQL2 += " respuesta, entrada, id_bitacora, id_estatusTimbrado, id_estatus, fecha_alta, fecha_modificacion, id_estatusNavistar, id_pagoNavistar, respuestaNavistar) ";
				sSQL2 += " OUTPUT INSERTED.id_pagoEnLinea";
				sSQL2 += " VALUES(@id_cliente,@contrato,@referencia,@montoAPagar,@id_origen,@usuario,@codigoRespuesta,@autorizacion,@nombre_cliente_pago,@nombre_banco_emisor,@fecha_aplicacion,@cuenta_deposito,@cuenta_pago,@codigo_metodo_pago,@metodo_pago,@respuesta,@entrada,@id_bitacora,@id_estatusTimbrado,@id_estatus,@fecha_alta,@fecha_modificacion,0,0,'')";
				paramArray2[1] = BDatos.NewParameter("@id_cliente", pagoEnLinea.claveCliente, "");
				paramArray2[2] = BDatos.NewParameter("@contrato", pagoEnLinea.contrato, "");
				paramArray2[3] = BDatos.NewParameter("@referencia", pagoEnLinea.referencia, "");
				paramArray2[4] = BDatos.NewParameter("@montoAPagar", pagoEnLinea.montoAPagar, "");
				paramArray2[5] = BDatos.NewParameter("@id_origen", pagoEnLinea.claveOrigen, "");
				paramArray2[6] = BDatos.NewParameter("@usuario", pagoEnLinea.usuario, "");
				paramArray2[7] = BDatos.NewParameter("@codigoRespuesta", pagoEnLinea.codigoRespuesta, "");
				paramArray2[8] = BDatos.NewParameter("@autorizacion", pagoEnLinea.autorizacion, "");
				paramArray2[9] = BDatos.NewParameter("@nombre_cliente_pago", pagoEnLinea.nombreClientePago, "");
				paramArray2[10] = BDatos.NewParameter("@nombre_banco_emisor", pagoEnLinea.nombreBancoEmisor, "");
				paramArray2[11] = BDatos.NewParameter("@fecha_aplicacion", pagoEnLinea.fechaAplicacion, "");
				paramArray2[12] = BDatos.NewParameter("@cuenta_deposito", pagoEnLinea.cuentaDeposito, "");
				paramArray2[13] = BDatos.NewParameter("@cuenta_pago", pagoEnLinea.cuentaPago, "");
				paramArray2[14] = BDatos.NewParameter("@codigo_metodo_pago", pagoEnLinea.codigoMetodoPago, "");
				paramArray2[15] = BDatos.NewParameter("@metodo_pago", pagoEnLinea.metodoPago, "");
				paramArray2[16] = BDatos.NewParameter("@respuesta", pagoEnLinea.respuesta, "");
				paramArray2[17] = BDatos.NewParameter("@entrada", pagoEnLinea.entrada, "");
				paramArray2[18] = BDatos.NewParameter("@id_bitacora", pagoEnLinea.claveBitacora, "");
				paramArray2[19] = BDatos.NewParameter("@id_estatusTimbrado", pagoEnLinea.claveEstatusTimbrado, "");
				paramArray2[20] = BDatos.NewParameter("@id_estatus", pagoEnLinea.claveEstatus, "");
				paramArray2[21] = BDatos.NewParameter("@fecha_alta", pagoEnLinea.fechaAlta, "");
				paramArray2[22] = BDatos.NewParameter("@fecha_modificacion", pagoEnLinea.fechaModificacion, "");
				IDataReader dr = conector.RunCommand(sSQL2, paramArray2);
                if (dr.Read()) int.TryParse(dr.GetValue(0).ToString(), out pagoEnLinea.clavePagoEnLinea);
                dr.Close();
                string largos = "";
                for (int i = 0; i < paramArray2.Length; i++)
                {
                    if (paramArray2[i] != null)
                    {
                        SqlParameter par = (SqlParameter)paramArray2[i];
                        largos += (" " + par.ParameterName.Replace("@", "") + " - " + par.Value.ToString().Length + ". ");
                    }
                }
            }
			catch(Exception ex)
			{
				throw(new Exception(ex.Message));
			}
			finally
			{
				conector.Close();
			}
			return pagoEnLinea.clavePagoEnLinea;
		}
		/// <summary>
		/// Método estático para dar de baja el registro de la tabla
		/// </summary>
		public static void Borrar(PagoEnLinea pagoEnLinea)
		{
			String sSQL;

			//Actualizamos la informacion
			sSQL = "UPDATE   pagosEnLinea";
			sSQL += " SET    Id_Estatus = 100";
			sSQL += " WHERE  id_pagoEnLinea = ?";
			SqlParameter[] paramArray = new SqlParameter[2];
			paramArray[0] = BDatos.NewParameter("@id_pagoEnLinea", pagoEnLinea.clavePagoEnLinea, "");
			BaseDatos.BDatos conector = new BaseDatos.BDatos();
			try
			{
				conector.RunCommand(sSQL, paramArray);
			}
			catch(Exception ex)
			{
				throw(new Exception(ex.Message));
			}
			finally
			{
				conector.Close();
			}
		}

		private int clavePagoEnLinea;
		public int ClavePagoEnLinea
		{
			get{return clavePagoEnLinea;}
			set{clavePagoEnLinea = value;}
		}
		private int claveCliente;
		public int ClaveCliente
		{
			get{return claveCliente;}
			set{claveCliente = value;}
		}
		private String contrato = "";
		public String Contrato
		{
			get{return contrato;}
			set{contrato = value;}
		}
		private String referencia = "";
		public String Referencia
		{
			get{return referencia;}
			set{referencia = value;}
		}
		private decimal montoAPagar = 0;
		public decimal MontoAPagar
		{
			get{return montoAPagar;}
			set{montoAPagar = value;}
		}
		private int claveOrigen = 0;
		public int ClaveOrigen
		{
			get{return claveOrigen;}
			set{claveOrigen = value;}
		}
		private String usuario = "";
		public String Usuario
		{
			get{return usuario;}
			set{usuario = value;}
		}
		private String codigoRespuesta = "";
		public String CodigoRespuesta
		{
			get{return codigoRespuesta;}
			set{codigoRespuesta = value;}
		}
		private String autorizacion = "";
		public String Autorizacion
		{
			get{return autorizacion;}
			set{autorizacion = value;}
		}
		private String nombreClientePago = "";
		public String NombreClientePago
		{
			get{return nombreClientePago;}
			set{nombreClientePago = value;}
		}
		private String nombreBancoEmisor = "";
		public String NombreBancoEmisor
		{
			get{return nombreBancoEmisor;}
			set{nombreBancoEmisor = value;}
		}
		private DateTime fechaAplicacion;
		public DateTime FechaAplicacion
		{
			get{return fechaAplicacion;}
			set{fechaAplicacion = value;}
		}
		private String cuentaDeposito = "";
		public String CuentaDeposito
		{
			get{return cuentaDeposito;}
			set{cuentaDeposito = value;}
		}
		private String cuentaPago = "";
		public String CuentaPago
		{
			get{return cuentaPago;}
			set{cuentaPago = value;}
		}
		private String codigoMetodoPago = "";
		public String CodigoMetodoPago
		{
			get{return codigoMetodoPago;}
			set{codigoMetodoPago = value;}
		}
		private String metodoPago = "";
		public String MetodoPago
		{
			get{return metodoPago;}
			set{metodoPago = value;}
		}
		private String entrada = "";
		public String Entrada
		{
			get { return entrada; }
			set { entrada = value; }
		}
		private String respuesta = "";
		public String Respuesta
		{
			get{return respuesta;}
			set{respuesta = value;}
		}
		private int clavePagoNavistar = 0;
		public int ClavePagoNavistar
		{
			get { return clavePagoNavistar; }
			set { clavePagoNavistar = value; }
		}
		private String respuestaNavistar = "";
		public String RespuestaNavistar
		{
			get { return respuestaNavistar; }
			set { respuestaNavistar = value; }
		}
		private int claveBitacora;
		public int ClaveBitacora
		{
			get { return claveBitacora; }
			set { claveBitacora = value; }
		}
		private int claveEstatusTimbrado;
		public int ClaveEstatusTimbrado
		{
			get { return claveEstatusTimbrado; }
			set { claveEstatusTimbrado = value; }
		}
		private int claveEstatus;
		public int ClaveEstatus
		{
			get{return claveEstatus;}
			set{claveEstatus = value;}
		}
		private int claveEstatusNavistar;
		public int ClaveEstatusNavistar
		{
			get { return claveEstatusNavistar; }
			set { claveEstatusNavistar = value; }
		}
		private DateTime fechaAlta;
		public DateTime FechaAlta
		{
			get{return fechaAlta;}
			set{fechaAlta = value;}
		}
		private DateTime fechaModificacion;
		public DateTime FechaModificacion
		{
			get{return fechaModificacion;}
			set{fechaModificacion = value;}
		}
        public cliente Cliente
        {
            get
            {
                sfiinternationalEntities Entity = new sfiinternationalEntities();
                cliente Cliente = Entity.clientes.Where(x => x.cveCliente == this.claveCliente).FirstOrDefault();
                return Cliente;
            }
        }
    }
}
