using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(OnlineLibrary.Startup))]
namespace OnlineLibrary
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
