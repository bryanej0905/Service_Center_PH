using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceCenter.Models
{
    public enum EnumEstadoTicket
    {
        Abierto,
        Asignado,
        Proceso,
        Cerrado,
        Cerrado_por_Usuario,
    }
}