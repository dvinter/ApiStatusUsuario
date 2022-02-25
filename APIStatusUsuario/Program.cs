using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APIStatusUsuario
{
    class Program
    {
        static void Main(string[] args)
        {
            string URL = new Uri(ConfigurationManager.AppSettings["api-url"]).ToString();

            using (WebApp.Start<Server>(URL))
            {
                Console.WriteLine("Webservice is running in " + ConfigurationManager.AppSettings["api-url"].ToString());
                Console.ReadLine();
            }
        }
    }

}
