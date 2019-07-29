using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(UrnBMS.Startup))]
namespace UrnBMS
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
