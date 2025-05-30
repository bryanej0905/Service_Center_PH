using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ServiceCenter.Startup))]
namespace ServiceCenter
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
