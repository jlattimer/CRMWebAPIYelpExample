using System.Web;
using System.Web.Mvc;

namespace CRMWebAPIYelpExample
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
