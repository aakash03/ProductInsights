using System.Web;
using System.Web.Optimization;

namespace ProductInsight
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/Scripts/Portal/portaljs").Include(
                      "~/Scripts/Portal/*.js"));
            
            bundles.Add(new ScriptBundle("~/Scripts/Portal/flot/flotjs").Include(
                      "~/Scripts/Portal/flot/jquery.flot.js",
                      "~/Scripts/Portal/flot/jquery.flot.tooltip.min.js",
                      "~/Scripts/Portal/flot/jquery.flot.resize.js",
                      "~/Scripts/Portal/flot/jquery.flot.pie.js",
                      "~/Scripts/Portal/flot/flot-data.js"
                      ));
            
            bundles.Add(new ScriptBundle("~/Scripts/Portal/morris/morris").Include(
                      "~/Scripts/Portal/morris/raphael.min.js",
                      "~/Scripts/Portal/morris/morris.min.js"
                     // "~/Scripts/Portal/morris/morris-data.js"
                      ));
          
  
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/portalcss").Include(
                      "~/Content/Portal/*.css"));
        
            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = true;
        }
    }
}
