using System.Linq;
using System.Web;
using System.Web.Mvc;
using library_admin.Models;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;

namespace library_admin.Controllers
{
    public class AdministratorPageController : Controller
    {
        // GET: AdministratorPage

        public ActionResult Index()
        {

            Entities entities = new Entities();

            var blogs = from num in entities.NOTICE
                        orderby num.PUBLISH_DATE descending
                        select new
                        {
                            Year = num.PUBLISH_DATE.Year,
                            Mouth = num.PUBLISH_DATE.Month,
                            Day = num.PUBLISH_DATE.Day,
                            TITLE = num.TITLE,
                            CONTENT = num.CONTENT,
                            ID = num.ID
                        };

            List<index_notice> notices = new List<index_notice>();
            int j = 0;
            foreach (var u in blogs)
            {
                index_notice a = new index_notice();
                //Console.WriteLine(u.img);
                a.Year = u.Year;
                a.Mouth = u.Mouth;
                a.Day = u.Day;
                a.TITLE = u.TITLE;
                a.CONTENT = u.CONTENT;
                a.ID = u.ID;
                notices.Add(a);
                j++;
            }


            ViewBag.Notice = notices;

            return View();
        }

    }
}
