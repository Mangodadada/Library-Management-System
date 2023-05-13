using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Mvc;
using library_admin.Models;
using System.Data;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data.SqlClient;
using library_admin.DAL;

namespace library_admin.Controllers
{
    public class feedbackController : Controller
    {
        Entities my = new Entities();
        // GET: feedback
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult feed_main()
        {
            var blogs = from num in my.FEEDBACK
                        orderby num.SUBMIT_DATE descending
                        select new
                        {
                            Year = num.SUBMIT_DATE.Year,
                            Mouth = num.SUBMIT_DATE.Month,
                            Day = num.SUBMIT_DATE.Day,
                            TITLE = num.TITLE,
                            CONTENT = num.CONTENT,
                            ID = num.ID,
                            TEL = num.TEL,
                            MAIL=num.MAIL,
                            NAME=num.NAME
                        };
            List<feed> feeds = new List<feed>();
            int j = 0;
            foreach (var u in blogs)
            {
                feed a = new feed();
                //Console.WriteLine(u.img);
                a.Year = u.Year;
                a.Mouth = u.Mouth;
                a.Day = u.Day;
                a.TITLE = u.TITLE;
                a.CONTENT = u.CONTENT;
                a.ID = u.ID;
                a.NAME = u.NAME;
                a.TEL = u.TEL;
                a.MAIL = u.MAIL;
                feeds.Add(a);
                j++;
            }
            List<feed> feed = new List<feed>();
            int pagesize = 14;
            int pageindex = Request["pageindex"] != null ? int.Parse(Request["pageindex"]) : 1;
            int pagecount = Convert.ToInt32(Math.Ceiling((double)j / pagesize));
            pageindex = pageindex < 1 ? 1 : pageindex;
            int start = (pageindex - 1) * pagesize;
            int end = (pagesize * pageindex - 1) < j ? (pagesize * pageindex - 1) : (j - 1);
            feed = getfeedlist(start, end, feeds);
            ViewBag.feed = feed;
            ViewBag.amount = j;
            ViewBag.pagecount = pagecount;
            ViewBag.pageindex = pageindex;
            return View();
        }
        public List<feed> getfeedlist(int start, int end, List<feed> feeds)
        {
            List<feed> feed = new List<feed>();
            for (int i = start; i <= end; i++)
            {
                feed.Add(feeds[i]);
            }
            return feed;
        }
        public ActionResult feed_admin()
        {
            int ID = Request["id"] != null ? Convert.ToInt32(Request["id"]) : 1;
            var feed = from num in my.FEEDBACK
                         where num.ID == ID
                         select new
                         {
                             Year = num.SUBMIT_DATE.Year,
                             Mouth = num.SUBMIT_DATE.Month,
                             Day = num.SUBMIT_DATE.Day,
                             TITLE = num.TITLE,
                             CONTENT = num.CONTENT,
                             ID = num.ID,
                             TEL = num.TEL,
                             MAIL = num.MAIL,
                             NAME = num.NAME
                         };
            feed a = new feed();
            foreach (var u in feed)
            {
                //Console.WriteLine(u.img);
                a.Year = u.Year;
                a.Mouth = u.Mouth;
                a.Day = u.Day;
                a.TITLE = u.TITLE;
                a.CONTENT = u.CONTENT;
                a.ID = u.ID;
                a.NAME = u.NAME;
                a.TEL = u.TEL;
                a.MAIL = u.MAIL;
            }
            ViewBag.feed = a;
            return View();
        }
        public ActionResult feed_stu()
        {
            feed_add(Request["feed_title"], Request["feed_content"], Request["feed_name"], Request["feed_tel"], Request["feed_mail"]);
            return View();
        }
        public void feed_add(string title, string content,string name,string tel,string mail)
        {
            FEEDBACK newfeed = new FEEDBACK();
            newfeed.TITLE = title;
            newfeed.CONTENT = content;
            newfeed.NAME = name;
            newfeed.TEL = tel;
            newfeed.MAIL = mail;
            newfeed.SUBMIT_DATE = DateTime.Now;
            var blogs = from num in my.FEEDBACK
                        orderby num.SUBMIT_DATE descending
                        select num;
            int j = 0;
            foreach (var u in blogs)
            {
                j++;
            }
            newfeed.ID = (j + 1);
            if (newfeed.TITLE != null)
            {
                my.FEEDBACK.Add(newfeed);
                my.SaveChanges();
            }
            //string title = Request.Form["title"];
            //string content = Request.Form["content"];
            Console.WriteLine("ok");
        }
    }
}