using APIStatusUsuario.Filters;
using Owin;
using System.Web.Http;

namespace APIStatusUsuario
{
    public class Server
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "API Status Usuário",
                routeTemplate: "api/{controller}/{email}",
                defaults: new
                {
                    txPesquisa = RouteParameter.Optional,
                }
            );
            config.Filters.Add(new BasicAuthenticationAttribute());
            app.UseWebApi(config);
        }
    }
}
