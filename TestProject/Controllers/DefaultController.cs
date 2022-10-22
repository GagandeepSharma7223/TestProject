using System.Configuration;
using System.IO;
using System.Web.Mvc;

namespace TestProject.Controllers
{
    public class DefaultController : Controller
    {
        // GET: Default
        public ActionResult Test()
        {
            var rootDirectory = ConfigurationManager.AppSettings.Get("RootDirectory");
            bool exists = Directory.Exists(Server.MapPath(rootDirectory));

            if (!exists)
                Directory.CreateDirectory(Server.MapPath(rootDirectory));
            ViewBag.RootDirectory = Server.MapPath("~/" + ConfigurationManager.AppSettings.Get("RootDirectory"));
            return View();
        }
    }
}