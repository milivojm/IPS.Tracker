using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Optimization.React;

namespace IPS.Tracker.Web.App_Start
{
    public class BundleConfig
    {        
        public static void RegisterBundles(BundleCollection bundles)
        {            
            bundles.Add(new ScriptBundle("~/bundles/react").Include(
                "~/Scripts/react.development.js",
                "~/Scripts/react-dom.development.js",
                "~/Scripts/babel.js"
            ));
        }
    }
}