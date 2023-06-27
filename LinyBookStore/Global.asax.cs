using CaptchaMvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using CaptchaMvc.Infrastructure;
using CaptchaMvc.Interface;
using CaptchaMvc.Models;

namespace LinyBookStore
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var captchaManager = (DefaultCaptchaManager)CaptchaUtils.CaptchaManager;
            captchaManager.CharactersFactory = () => "my characters";
            captchaManager.PlainCaptchaPairFactory = length =>
            {
                string randomText = RandomText.Generate(captchaManager.CharactersFactory(), length);
                bool ignoreCase = false;
                return new KeyValuePair<string, ICaptchaValue>(Guid.NewGuid().ToString("N"),
                        new StringCaptchaValue(randomText, randomText, ignoreCase));
            };
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Application["Notification"] = "";
        }
    }
}
