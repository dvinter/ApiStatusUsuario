using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace APIStatusUsuario.Controller
{
    public class ConsultaUsuarioController : ApiController
    {
        [HttpGet]
        public Usuario ConsultaUsuario(string email = "")
        {
            try
            {
                Usuario Usuario = new Usuario();

                char atSign = '@';
                string login = email.Split(atSign)[0];
                string host = email.Split(atSign)[1];

                Usuario.Login = login;

                string formattedOutput = "Get-ADUser -Identity \"" + login + "\" -Properties *";
                Collection<PSObject> retornoUsuario = ExecutarComandoPS(formattedOutput);

                formattedOutput = "net user /domain " + login;
                Collection<PSObject> retornoStatusUsuario = ExecutarComandoPS(formattedOutput);

                if (retornoUsuario.Count > 0)
                {
                    foreach (PSObject outputItem in retornoUsuario)
                    {
                        if (outputItem != null)
                        {
                            foreach (var item in outputItem.Members)
                            {
                                if (item.Name == "GivenName" && item.Value != null)
                                {
                                    Usuario.Nome = item.Value.ToString();
                                    Usuario.Existe = true;
                                }

                                if (item.Name == "Surname" && item.Value != null)
                                {
                                    Usuario.Sobrenome = item.Value.ToString();
                                }
                                if (item.Name == "Title" && item.Value != null)
                                {
                                    Usuario.Cargo = item.Value.ToString();
                                }
                                if (item.Name == "EmailAddress" && item.Value != null)
                                {
                                    Usuario.Email = item.Value.ToString();
                                }
                                if (item.Name == "Enabled" && item.Value != null)
                                {
                                    if (item.Value.ToString().ToLower() == "true")
                                    {
                                        Usuario.Habilitado = "Yes";
                                        Usuario.DetalhesConsulta += " - Usuário Ativo";
                                    }
                                    if (item.Value.ToString().ToLower() == "false")
                                    {
                                        Usuario.Habilitado = "No";
                                        Usuario.DetalhesConsulta += " - Usuário está com a conta Desabilitada.";
                                    }
                                }

                                if (item.Name.ToLower() == "lockedout" && item.Value != null)
                                {
                                    if (item.Value.ToString().ToLower() == "true")
                                    {
                                        Usuario.Logon = "Blocked";
                                        Usuario.DetalhesConsulta += " - Usuário está com Bloqueado por excesso de tentativas de logon.";
                                    }
                                    if (item.Value.ToString().ToLower() == "false")
                                    {
                                        Usuario.Logon = "Enabled";
                                    }

                                }

                                if (item.Name == "logonHours")
                                {
                                    BitArray bitArray = new BitArray((byte[])item.Value);
                                    bool[] vetor = ToBooleanArray(bitArray);

                                    DateTime dataAtual = DateTime.UtcNow;

                                    int diaAtual = (int)dataAtual.DayOfWeek;
                                    int horaAtual = dataAtual.Hour;
                                    int index = (diaAtual * 24) + horaAtual;

                                    Usuario.HoraDeLogonPermitido = vetor[index];

                                    if (Usuario.HoraDeLogonPermitido == false)
                                    {
                                        Usuario.DetalhesConsulta += " - Usuário Bloqueado por está fora do horário permitido no sistema.";
                                    }
                                }

                                if (item.Name == "AccountExpirationDate" && item.Value != null)
                                {
                                    var data = Convert.ToDateTime(item.Value);

                                    if (data < DateTime.Now)
                                    {
                                        Usuario.ContaExpiradaContrato = true;
                                        Usuario.DetalhesConsulta += DataExpirada(data);
                                    }
                                    else
                                    {
                                        Usuario.ContaExpiradaContrato = false;
                                    }
                                }

                                //usuario.AtributosUsuario.Add(new AtributosUsuario
                                //{
                                //    Name = item.Name,
                                //    Value = item.Value.ToString()
                                //});

                            }
                        }
                    }
                }
                else
                {
                    Usuario.Existe = false;
                    Usuario.DetalhesConsulta = " - Usuário não encontrado no Sistema.";
                    return Usuario;
                }

                if (retornoStatusUsuario.Count > 0)
                {
                    foreach (PSObject outputItem in retornoUsuario)
                    {
                        if (outputItem != null)
                        {
                            foreach (var item in outputItem.Members)
                            {
                                //Console.WriteLine("- " + item.Name);
                                //if (item.Name.ToLower() == "accountexpires" && item.Value != null)
                                //{
                                //    Console.WriteLine("account active !!" + item.Value);

                                //    if (item.Value.ToString().ToLower() == "true")
                                //    {
                                //        usuario.Ativado = true;
                                //    }
                                //    else if (item.Value.ToString().ToLower() == "false")
                                //    {
                                //        usuario.Ativado = false;
                                //        usuario.DetalhesConsulta += "Usuário está Desativado.";
                                //    }
                                //}


                                if (item.Name.ToLower() == "passwordexpired" && item.Value != null)
                                {
                                    //Console.WriteLine("password expires!!" + item.Name.ToLower());

                                    if (item.Value.ToString().ToLower() == "true")
                                    {
                                        Usuario.Habilitado = "No";
                                        Usuario.SenhaExpirada = "true";
                                        Usuario.DetalhesConsulta += " - Senha de Usuário Expirada por tempo.";
                                    }
                                    else if (item.Value.ToString().ToLower() == "false")
                                    {
                                        Usuario.Habilitado= "Yes";
                                        Usuario.SenhaExpirada = "false";
                                    }

                                    //var data = Convert.ToDateTime(item.Value);

                                    //if (data < DateTime.Now)
                                    //{
                                    //    usuario.ContaExpiradaContrato = true;
                                    //    usuario.DetalhesConsulta += SenhaExpirada(data);
                                    //}
                                }

                                if (item.Name.ToLower() == "accountactive" && item.Value != null)
                                {
                                    Console.WriteLine("passou aqui!!" + item.Name.ToLower());

                                }
                            }

                        }
                    }
                }
                else
                {
                    Usuario.Existe = false;
                    Usuario.DetalhesConsulta += " - Informação de status do usuário não encontrado no Sistema. Favor informar um login válido.";
                    return Usuario;
                }

                return Usuario;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private Collection<PSObject> ExecutarComandoPS(string formattedOutput)
        {
            using (PowerShell PowerShellInstance = PowerShell.Create())
            {
                Console.WriteLine("Comando powershell no AD: " + formattedOutput);
                Runspace runspace = RunspaceFactory.CreateRunspace();

                runspace.Open();

                Pipeline pipeline = runspace.CreatePipeline();
                PowerShellInstance.AddScript(formattedOutput);
                pipeline.Commands.Add("Out-String");

                Collection<PSObject> psOutput = PowerShellInstance.Invoke();

                runspace.Close();

                return psOutput;
            }
        }

        private string DataExpirada(DateTime data)
        {
            return " - Conta de Usuário expirada na data " + data.Day + "/" + data.Month + "/" + data.Year + " - " + data.ToShortTimeString();
        }

        private string SenhaExpirada(DateTime data)
        {
            return " - Senha de Usuário expirada na data " + data.Day + "/" + data.Month + "/" + data.Year + " - " + data.ToShortTimeString();
        }

        internal static bool[] ToBooleanArray(BitArray _btarray)
        {
            bool[] rtnboolarr = new bool[_btarray.Count];
            for (int b = 0; b < _btarray.Count; b++)
                rtnboolarr[b] = _btarray[b];
            return rtnboolarr;
        }
    }
}
