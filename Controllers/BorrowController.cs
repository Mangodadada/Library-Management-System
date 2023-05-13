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
//from DateTime import Date;


namespace library_admin.Controllers
{
    public class BorrowController : Controller
    {

        Entities my = new Entities();
        public ActionResult Borrow()
        {

            //只要遍历testContext中的users集合就可以了
            //这里是搜索，为了展示
        
            foreach (var u in my.ADMINISTRATOR)
            {
                ViewBag.Tests = u;
            }
            int ud = Convert.ToInt32(Session["user_id"]);
            var result = from book in my.BOOK
                         join user_book in my.USER_BOOK on book.ID equals user_book.BOOK_ID
                         from classbook in my.CLASSBOOK
                         where book.ISBN == classbook.ISBN
                         where user_book.RETURN_DATE == null
                         where user_book.USER_ID == ud
                  
                         select new
                         {
                             BookId = user_book.BOOK_ID,
                             BorrowTime = user_book.BORROW_DATE,
                             ExpectTime = user_book.EXPECT_DATE,
                             Name = classbook.BOOK_NAME,
                             Author = classbook.AUTHOR,
                             Publisher = classbook.PUBLISHINGHOUSE,
                             ISBN = book.ISBN,
                         };

            int i = 0;
            borrowed_book[] rec = new borrowed_book[12];
            //result.ToList();
            foreach (var u in result)
            {
                borrowed_book a = new borrowed_book();
                a.BookId = u.BookId;
                a.BorrowTime = u.BorrowTime;
                a.ExpectTime = (DateTime)u.ExpectTime;
                a.Name = u.Name;
                a.Author = u.Author;
                a.Publisher = u.Publisher;
                a.ISBN = u.ISBN;
                rec[i] = a;
                i++;
            }
            ViewBag.booklist = rec;
            ViewBag.amount = i;

            ViewBag.Message = "Borrow page.";
            return View();
        }
        //
        //更新
        //写个函数，按键时传入主码信息，调用更新函数
        public ActionResult Update()
        {
            string book_id = Request["pid"];
            int user_id = Convert.ToInt32(Session["user_id"]);
            //Entities my2 = new Entities();
            var result2 = from num in my.USER_BOOK
                          where num.BOOK_ID == book_id && num.USER_ID == user_id && num.RETURN_DATE == null
                          select num;
            var userinfo = result2.FirstOrDefault();
            if (userinfo != null)
            {
                DateTime expect = Convert.ToDateTime(userinfo.EXPECT_DATE);
                userinfo.EXPECT_DATE = expect.AddMonths(1);
                //userinfo.EXPECT_DATE = userinfo.EXPECT_DATE + timedelta(days = 30);
                //userinfo.EXPECT_DATE = userinfo.EXPECT_DATE + DateTime.
                my.SaveChanges();
                return Content(@"<script>alert('续借成功，已延期一个月');window.location.href='/Borrow/Borrow';</script>");
            }
            else
                return Content(@"<script>alert('系统繁忙，请稍后再试！');window.location.href='/Borrow/Borrow';</script>");
        }
        //
    }
}
