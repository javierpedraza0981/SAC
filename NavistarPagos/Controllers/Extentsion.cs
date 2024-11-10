using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NavistarPagos.Controllers
{
    public static class Extentsion
    {
        static public string ToFormatMoney(this string original)
        {
            original = String.Format("{0:C}", original);
            return original;
        }
    }
}