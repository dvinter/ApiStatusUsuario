using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace APIStatusUsuario
{
    public class Usuario
    {
        public string Nome { get; set; }
        public string Login { get; set; }
        public string Sobrenome { get; set; }
        public string Cargo { get; set; }
        public string Email { get; set; }
        public bool ContaExpiradaContrato { get; set; }
        public bool Existe { get; set; }
        public string DetalhesConsulta { get; set; }
        public bool HoraDeLogonPermitido { get; set; }
        public string Logon { get; set; }
        public string Habilitado { get; set; }
        public string SenhaExpirada { get; set; }


    }
}
