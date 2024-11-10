using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NavistarPagos.Models
{
    public class Contracts
    {
        public Nullable<byte> sistema { get; set; }
        public Nullable<int> idcliente { get; set; }
        public string operacion { get; set; }
        public string contrato { get; set; }
        public Nullable<decimal> saldoporVenc { get; set; }
        public Nullable<decimal> saldoMes { get; set; }
        public string fecCorte { get; set; }
        public string fecProxPago { get; set; }
        public string moneda { get; set; }
        public string nombrecliente { get; set; }
        public Nullable<int> idPersona { get; set; }
        public string Fecha { get; set; }
        public string movimiento { get; set; }
        public Nullable<int> id_movimiento { get; set; }
        public string banco { get; set; }
        public string referenciaNumerica { get; set; }
        public Nullable<int> PagoEnTransito { get; set; }
        public Nullable<int> ClavePago { get; set; }
    }

    public class ResumenContratos
    {
        public string Contrato { get; set; }
        public decimal MontoAPagar { get; set; }
        public string Moneda { get; set; }
        public string FechaPago { get; set; }
        public string Domiciliado { get; set; }
        public string Beneficiario { get; set; }
        public string PagoCuentaBanamex { get; set; }
        public string PagoCuentaBBVA { get; set; }
        public string TransferenciaBBVA { get; set; }
        public string Referencia { get; set; }
        public string Status_Contrato { get; set; }

        public string Fechafinmov { get; set; }
        //public string Paperless { get; set; }
        public string EstatusContrato { get; set; }
    }

    public class PopupMessage
    {
        public int ID { get; set; }
        // Se ponen string para poderle concatenar el caracter para le color
        public string Url { get; set; }
        public string Texto { get; set; }
        public string Texto2 { get; set; }
        public string Vista { get; set; }

        public PopupMessage()
        {
            ID = 0;
            Url = "";
            Texto = "";
            Texto2 = "";
            Vista = "";
        }
    }

    public class PolizasExists
    {
        
        public string Poliza { get; set; }
        public bool Existe { get; set; }

        public string MsgPoliza { get; set; }

        public PolizasExists()
        {            
            Poliza = "";
            Existe = false;
            MsgPoliza = "";
        }
    }

    public class PolizasC
    {
        public int ID { get; set; }
        public string NumeroSerie { get; set; }
        public string numeroSerieValue { get; set; }
        public string vContrato { get; set; }

        public string vContratoValue { get; set; }

        public int ePersona { get; set; }

        public PolizasC()
        {
            
            ID = 0;
            NumeroSerie = "";
            vContrato = "";
            vContratoValue = "";
            ePersona = 0;
            numeroSerieValue = "";
        }
    }

    public class MesesC
    {
        public int ID { get; set; }
        public int AnioMes { get; set; }
        public string Mes { get; set; }
        public int ePersona { get; set; }
        public string vContrato { get; set; }


        public MesesC()
        {
            AnioMes = 0;
            ID = 0;
            Mes = "";
            ePersona = 0;
            vContrato = "";
        }
    }

    //public class ContratosC
    //{
    //    public int ePersona { get; set; }
    //    public string vContrato { get; set; }


    //    public ContratosC()
    //    {
    //        ePersona = 0;
    //        vContrato = "";

    //    }
    //}

    public class PopMessageResponse : ResponseDto
    {
        public List<PopupMessage> lstMessages { get; set; }

        public string cveCliente;

        public PopMessageResponse()
        {
            lstMessages = new List<PopupMessage>();
            cveCliente = "";
        }
    }

    public class ResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int Folio { get; set; }
        public bool SessionExpired { get; set; }

        public ResponseDto()
        {
            Success = false;
            Message = "";
            Folio = 0;
            SessionExpired = false;
        }
        public ResponseDto(bool _Success)
        {
            Success = _Success;
            Message = "";
            Folio = 0;
            SessionExpired = false;
        }
    }

    public class ContratoPaperless
    {
        public string Contrato { get; set; }
        public int Paperless { get; set; }
    }
}