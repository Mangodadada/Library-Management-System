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
    public class HomeController : Controller
    {
        Entities my = new Entities();

        public ActionResult Index()
        {
            string ID = Request["id"] != null ? Request["id"] : "#";
            if (ID == "exit")
                Session["user_id"] = null;

            //只要遍历testContext中的users集合就可以了
            foreach (var u in my.LIBRARY)
            {
                ViewBag.Tests = u;
            }
            int book_amount = 0;
            var bookamount = from num in my.BOOK
                             where num.STATE != "0"
                             select num;
            foreach (var u in bookamount)
            {
                book_amount++;
            }
            ViewBag.book_amount = book_amount;
            var blogs = from num in my.NOTICE
                            //where num.ID == "9" || num.ID == "8" || num.ID == "7" || num.ID == "6" || num.ID == "5" || num.ID == "4" || num.ID == "3" || num.ID == "2" || num.ID == "1"
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
            ViewBag.notice = notices;

            var result = from classbook in my.CLASSBOOK
                         join recommend in my.RECOMMEND on classbook.ISBN equals recommend.ISBN
                         select new
                         {
                             img = classbook.IMAGE,
                             author = classbook.AUTHOR,
                             publish = classbook.PUBLISHINGHOUSE,
                             recom = recommend.RECOMMEND1,
                             name=classbook.BOOK_NAME,
                             ISBN=classbook.ISBN
                         };
            int i = 0;
            index_book[] rec = new index_book[3];
            //result.ToList();
            foreach (var u in result)
            {
                index_book a = new index_book();
                //Console.WriteLine(u.img);
                a.img = u.img;
                a.name = u.name;
                a.author = u.author;
                a.recom = u.recom;
                a.publish = u.publish;
                a.ISBN = u.ISBN;
                rec[i] = a;
                i++;
            }
            ViewBag.recommend = rec;
            ViewBag.amount = i;
            return View();
        }

        public ActionResult About(FormCollection form)
        {
            // ViewBag.Message = "Your application description page.";
            string name = Request.Form["name"];

            ViewBag.Name = name;
            return View();
        }
       // [HttpPost]
       public ActionResult info_admin()
        {
            return View();
        }
        public ActionResult add_info()
        {
            NOTICE newnotice = new NOTICE();
            newnotice.TITLE = Request["TITLE"];
            newnotice.CONTENT = Request["CONTENT"];
            newnotice.PUBLISH_DATE = DateTime.Now;
            newnotice.AD_ID = Convert.ToInt32(Session["admin_id"]);
            var blogs = from num in my.NOTICE
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
            int j = 0;
            foreach (var u in blogs)
            {
                j++;
            }
            newnotice.ID = j.ToString();
            if (newnotice.TITLE != null && newnotice.CONTENT != null)
            {
                my.NOTICE.Add(newnotice);
                my.SaveChanges();

                //string title = Request.Form["title"];
                //string content = Request.Form["content"];
                //ViewBag.t = newnotice.TITLE;
                //DateTime punish;
                return Content("ok:发布成功！");
            }
            else if (newnotice.TITLE == null)
                return Content("no:公告标题不能为空！");
            else
                return Content("no:公告内容不能为空！");
        }
        public ActionResult info_stu()
        {
            //if (id == null)
            //    id = "1";
            //  ViewBag.Message = "Your contact page.";
            //string a = Request["order"].ToString();
            //ViewBag.order = a;

            //动态获取id 
            string ID = Request["id"]!=null? Request["id"] :"1";
            //根据动态获取的id进行筛选，选出相应数据
            var notice = from num in my.NOTICE
                         where num.ID == ID
                         select new
                         {
                             Year = num.PUBLISH_DATE.Year,
                             Mouth = num.PUBLISH_DATE.Month,
                             Day = num.PUBLISH_DATE.Day,
                             TITLE = num.TITLE,
                             CONTENT = num.CONTENT,
                             ID = num.ID
                         };
            index_notice a = new index_notice();
            foreach (var u in notice)
            {
                //Console.WriteLine(u.img);
                a.Year = u.Year;
                a.Mouth = u.Mouth;
                a.Day = u.Day;
                a.TITLE = u.TITLE;
                a.CONTENT = u.CONTENT;
                a.ID = u.ID;
            }
            //将数据传给前端view
            ViewBag.notice = a;
            //ViewBag.content = "内容";
            //ViewBag.data = "2022/8/21";
            return View();
        }
        public ActionResult info_main()
        {
            var blogs = from num in my.NOTICE
                            //where num.ID == "9" || num.ID == "8" || num.ID == "7" || num.ID == "6" || num.ID == "5" || num.ID == "4" || num.ID == "3" || num.ID == "2" || num.ID == "1"
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
            List<index_notice> notice = new List<index_notice>();
            int pagesize = 14;
            int pageindex = Request["pageindex"] != null ? int.Parse(Request["pageindex"]) : 1;
            int pagecount = Convert.ToInt32(Math.Ceiling((double)j/pagesize));
            pageindex = pageindex < 1 ? 1 : pageindex;
            int start = (pageindex - 1) * pagesize;
            int end = (pagesize * pageindex - 1) < j ? (pagesize * pageindex - 1) : (j - 1);
            notice = getnoticelist(start, end, notices);
            ViewBag.notice = notice;
            ViewBag.amount = j;
            ViewBag.pagecount = pagecount;
            ViewBag.pageindex = pageindex;
            return View();
        }
        public List<index_notice> getnoticelist(int start,int end, List<index_notice> notices)
        {
            List<index_notice> notice = new List<index_notice>();
            for(int i=start;i<=end;i++)
            {
                notice.Add(notices[i]);
            }
            return notice;
        }
    }
}