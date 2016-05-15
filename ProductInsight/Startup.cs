using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ProductInsight.Startup))]
namespace ProductInsight
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
