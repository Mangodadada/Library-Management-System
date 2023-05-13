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
    public class BookAdInfoController : Controller
    {
        Entities my = new Entities();
        static string search = null;
        // GET: BookAdInfo
        public ActionResult Index()
        {
            string book_id = Request["book_id"];
            //if (book_id != null)
            //{
            //    delete_book(book_id);
            //    ViewBag.delete = 1;
            //}
            search = Request["search"];
            if (search == null || search == "")
            {
                var blogs = from classbook in my.CLASSBOOK
                            join book1 in my.BOOK on classbook.ISBN equals book1.ISBN
                            where book1.STATE != "0"
                            orderby book1.ID descending
                            select new
                            {
                                ID = book1.ID,
                                NAME = classbook.BOOK_NAME,
                                ISBN = book1.ISBN,
                                CALL = classbook.CALL_NUMBER
                            };
                List<admin_book> books = new List<admin_book>();
                int j = 0;
                foreach (var u in blogs)
                {
                    admin_book a = new admin_book();
                    //Console.WriteLine(u.img);
                    a.ISBN = u.ISBN;
                    a.NAME = u.NAME;
                    a.CALL = u.CALL;
                    a.ID = u.ID;
                    books.Add(a);
                    j++;
                }
                List<admin_book> book = new List<admin_book>();
                int pagesize = 10;
                int pageindex = Request["pageindex"] != null ? int.Parse(Request["pageindex"]) : 1;
                int pagecount = Convert.ToInt32(Math.Ceiling((double)j / pagesize));
                pageindex = pageindex < 1 ? 1 : pageindex;
                int start = (pageindex - 1) * pagesize;
                int end = (pagesize * pageindex - 1) < j ? (pagesize * pageindex - 1) : (j - 1);
                book = getbooklist(start, end, books);
                ViewBag.book = book;
                ViewBag.amount = j;
                ViewBag.pagecount = pagecount;
                ViewBag.pageindex = pageindex;
            }
            else
            {
                int j = 0;
                List<admin_book> book = new List<admin_book>();
                var result = from num in my.BOOK
                             join yum in my.CLASSBOOK on num.ISBN equals yum.ISBN
                             where (num.ISBN == search || num.ID == search)&&num.STATE!="0"
                             orderby num.ID descending
                             select new
                             {
                                 ID = num.ID,
                                 NAME = yum.BOOK_NAME,
                                 ISBN = num.ISBN,
                                 CALL = yum.CALL_NUMBER
                             };
                foreach (var u in result)
                {
                    admin_book a = new admin_book();
                    a.ISBN = u.ISBN;
                    a.NAME = u.NAME;
                    a.CALL = u.CALL;
                    a.ID = u.ID;
                    book.Add(a);
                    j++;
                }
                ViewBag.book = book;
                ViewBag.amount = j;
                ViewBag.pagecount = 1;
                ViewBag.pageindex = 1;
            }
            return View();
        }
        public List<admin_book> getbooklist(int start, int end, List<admin_book> books)
        {
            List<admin_book> book = new List<admin_book>();
            for (int i = start; i <= end; i++)
            {
                book.Add(books[i]);
            }
            return book;
        }
        public ActionResult add_book()
        {
            return View();
        }
        public ActionResult add_action()
        {
            if (Request["isbn"] == null || Request["bookshelf"] == null || Request["isbn"] == "" || Request["bookshelf"] == "")
                return Content("no:ISBN或者书架不能为空！");
            else
            {
                string ISBN = Request["isbn"];
                string BOOKSHELF = Request["bookshelf"];
                Entities my = new Entities();
                var result1 = from num in my.CLASSBOOK
                              where num.ISBN == ISBN
                              select num;
                var result2 = from num in my.BOOKSHELF
                              where num.ID == BOOKSHELF
                              select num;
                int i = 0, j = 0;
                foreach (var u in result1)
                {
                    i++;
                }
                foreach (var u in result2)
                {
                    j++;
                }
                if (i != 0 && j != 0)
                {
                    BOOK a = new BOOK();
                    var result3 = from num in my.BOOK
                                  orderby num.ID ascending
                                  select num;
                    int z = 0;
                    foreach (var u in result3)
                    {
                        z = Convert.ToInt32(u.ID);
                    }
                    a.ID = (z + 1).ToString();
                    a.ISBN = ISBN;
                    a.STATE = "1";
                    a.ENTER_DATE = DateTime.Now;
                    a.BOOKSHELF_ID = BOOKSHELF;
                    my.BOOK.Add(a);
                    my.SaveChanges();
                    return Content("ok:添加成功！");
                }
                else if (i == 0)
                {
                    return Content("no:没有相关ISBN的记录，请先补充相关ISBN记录");
                }
                else
                {
                    return Content("no:没有对应书架编号，请确认填写无误");
                }
            }
        }
        public ActionResult modify_book()
        {

            return View();
        }
        public ActionResult delete_book(string book_id)
        {
            book_id = Request["book_id"];
            var result = from num in my.BOOK
                         where num.ID == book_id
                         select num;
            var book = result.FirstOrDefault();
            if (book.STATE != "1")
                return Content(@"<script>alert('此书正有人借阅，不可删除！');window.location.href='/BookAdInfo/Index';</script>");
            else
            {
                book.STATE = "0";
                my.SaveChanges();
                return Content(@"<script>alert('删除成功');window.location.href='/BookAdInfo/Index';</script>");

            }
        }
        public ActionResult borrow_info()
        {
            string book_id = Request["book_id"];
            int borrow_amount = 0, collect_amount = 0, command_amount = 0, pre_amount = 0;
            #region
            List<string> borrows= new List<string>();
            var result1 = from num in my.USER_BOOK
                          where num.BOOK_ID == book_id && num.RETURN_DATE == null
                          select new
                          {
                              USER_ID = num.USER_ID
                          };
            foreach (var u in result1)
            {
                borrow_amount++;
                borrows.Add(u.USER_ID.ToString());
            }
            ViewBag.borrow = borrows;
            ViewBag.borrow_amount = borrow_amount;
            #endregion
            #region
            List<string> collects = new List<string>();
            var result2 = from x in my.BOOK
                          join y in my.CLASSBOOK on x.ISBN equals y.ISBN
                          join z in my.USER_COLLECT on y.ISBN equals z.ISBN
                          where x.ID==book_id
                          select new
                          {
                              USER_ID = z.USER_ID
                          };
            foreach (var u in result2)
            {
                collect_amount++;
                collects.Add(u.USER_ID.ToString());
            }
            ViewBag.collect = collects;
            ViewBag.collect_amount = collect_amount;
            #endregion
            #region
            List<string> commands = new List<string>();
            var result3 = from x in my.BOOK
                          join y in my.CLASSBOOK on x.ISBN equals y.ISBN
                          join z in my.USER_CLASSBOOK on y.ISBN equals z.ISBN
                          where x.ID==book_id
                          select new
                          {
                              USER_ID = z.USER_ID
                          };
            foreach (var u in result3)
            {
                command_amount++;
                commands.Add(u.USER_ID.ToString());
            }
            ViewBag.command = commands;
            ViewBag.command_amount = command_amount;
            #endregion
            #region
            List<string> pres = new List<string>();
            var result4 = from x in my.BOOK
                          join y in my.CLASSBOOK on x.ISBN equals y.ISBN
                          join z in my.USER_PREBOOK on y.ISBN equals z.ISBN
                          where x.ID==book_id
                          select new
                          {
                              USER_ID = z.USER_ID
                          };
            foreach (var u in result4)
            {
                pre_amount++;
                pres.Add(u.USER_ID.ToString());
            }
            ViewBag.pre = pres;
            ViewBag.pre_amount = pre_amount;
            #endregion
            return View();
        }
    }
}