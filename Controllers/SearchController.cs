using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using library_admin.Models;
using System.Data;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data.SqlClient;
using library_admin.DAL;
using System.Text;
using System.Globalization;

namespace library_admin.Controllers
{
    public class SearchController : Controller
    {
        //获取数据库
        Entities my = new Entities();
        static string isbn = "9787532131990";

        // GET: Search
        public ActionResult Search()
        {
            //全部数据
            var temp1 = my.BOOK.Where(u => u.STATE != "0");            
            var temp = my.CLASSBOOK.AsNoTracking().Join(temp1, u => u.ISBN, t => t.ISBN, (u, t)=>u);
            //搜索词
            string condition = Request["search"] != null ? Request["search"] : null;
            if (condition != null)
                Session["search"] = condition;  //保存当前值
            else if (Session["search"] != null && condition != "")
                condition = Session["search"].ToString();  //读取值
            //搜索类型 默认按书名搜索(TITLE)
            //string type = Request["search-sort"] != null ? Request["search-sort"] : "TITLE";
            string type = Request["search-sort"];
            if (type != null)
                Session["search-sort"] = type;
            else if (Session["search-sort"] != null)
                type = Session["search-sort"].ToString();
            else
                type = "TITLE";
            //分类           
            string _class = Request["_class"];
            if (_class != null && _class!="/")
                Session["_class"] = _class;  //保存当前值
            else if (Session["_class"] != null)
                _class = Session["_class"].ToString();
            if(Request["del"] == "1")
            {//清空分类
                _class = null;
                Session["_class"] = null;
            }
            //页面大小
            int pageSize = 6;
            //页码
            int pageindex = Request["pageindex"] != null ? int.Parse(Request["pageindex"]) : 1;
            //总页数
            int pagecount = 1;
            //总条数
            int count = 0;
            //升降序 升序/true 降序/false
            bool isAsc = true;
            if (Request["order"] == "ASC")
                isAsc = true;
            else if (Request["order"] == "DESC")
                isAsc = false;
            else if (Session["order"] != null)
                isAsc = Convert.ToBoolean(Session["order"]);
            Session["order"] = isAsc;
            //排列类型
            string sort = Request["sort"];
            if (sort != null)
                Session["sort"] = sort;
            else if (Session["sort"] != null)
                sort = Session["sort"].ToString();
            else
                sort = "COMMON";
            //结果列表
            List<search_book> booklist = new List<search_book>();
            List<book_category> book_Categories = new List<book_category>();
            //条件查询
            if (!string.IsNullOrEmpty(condition))
            {
                if (type == "TITLE")
                    temp = temp.Where(u => u.BOOK_NAME.Contains(condition));
                if (type == "AUTHOR")
                    temp = temp.Where(u => u.AUTHOR.Contains(condition));
                if (type == "PUBLISH")
                    temp = temp.Where(u => u.PUBLISHINGHOUSE.Contains(condition));
                if (type == "ISBN")
                    temp = temp.Where(u => u.ISBN == condition);
                if (type == "CALL_NUMBER")
                    temp = temp.Where(u => u.CALL_NUMBER.Contains(condition));
            }
            //分类查询
            if (!string.IsNullOrEmpty(_class))
            {
                temp = temp.Where(u => u.TYPE == _class);
            }
            //记录总条数
            count = temp.Count();
            //分类
            var category = from bk in temp
                           group bk by bk.TYPE
                           into c
                           select new
                           {
                               c.Key,
                               tcount = c.Count()
                           };
            foreach (var ct in category)
            {
                book_category c = new book_category();
                c.type = ct.Key;
                c.num = ct.tcount;
                book_Categories.Add(c);
            }
            //分页
            pagecount = Convert.ToInt32(Math.Ceiling((double)count / pageSize));
            var books = LoadPageItems(temp, pageSize, pageindex, sort, isAsc);
            foreach (var bk in books)
            {
                search_book bl = new search_book();
                bl.book_name = bk.BOOK_NAME;
                bl.author = bk.AUTHOR;
                bl.publish = bk.PUBLISHINGHOUSE;
                bl.publish_date = string.Format("{0:d}", bk.PUBLISH_DATE);
                bl.call_number = bk.CALL_NUMBER;
                bl.isbn = bk.ISBN;
                booklist.Add(bl);
            }
            ViewBag.bk_condition = condition;
            ViewBag.bk_type = type;
            ViewBag.bk_isAsc = isAsc;
            ViewBag.bk_sort = sort;
            ViewBag.bk_class = _class;
            ViewBag.bk_category = book_Categories;
            ViewBag.booklist = booklist;
            ViewBag.bk_count = count;
            ViewBag.bk_pcount = booklist.Count;
            ViewBag.bk_pageindex = pageindex;
            ViewBag.bk_pagecount = pagecount;
            return View();
        }
        public ActionResult Book()
        {
            string command = Request["command_content"];
            string op = Request["op"];
            int u_id = Convert.ToInt32(Session["user_id"]);
            isbn = Request["isbn"] != null ? Request["isbn"] : isbn;
            if (op == "1")
                collect(isbn);
            else if (op == "2")
                ucollect(isbn);
            //else if (op == "3")
            //    pre(isbn);
            else if (op == "4")
                upre(isbn);
            var result = from num in my.USER_COLLECT
                         where num.USER_ID == u_id && num.ISBN == isbn
                         select num;
            var result1 = from num in my.USER_PREBOOK
                          where num.USER_ID == u_id && num.ISBN == isbn
                          select num;
            int i = 0, j = 0;
            foreach (var u in result)
                i++;
            foreach (var u in result1)
                j++;
            if (i == 0)
                ViewBag.collect_late = "未收藏";
            else
            {
                var col = result.FirstOrDefault();
                if (col.COLLECT_STATE == "未收藏")
                {
                    ViewBag.collect_late = "未收藏";
                }
                else
                {
                    ViewBag.collect_late = "已收藏";
                }
            }
            if (j == 0)
                ViewBag.pre = "未预约";
            else
            {
                var pre = result1.FirstOrDefault();
                if (pre.PRE_STATE == "未预约")
                {
                    ViewBag.pre = "未预约";
                }
                else
                {
                    ViewBag.pre = "已预约";
                }
            }
            if (command != null)
                add_command(command, isbn);
            search_book search_book = new search_book();
            var classbook = my.CLASSBOOK.Find(isbn);
            var book = my.BOOK.Where(u => u.ISBN == isbn);
            foreach(var bk in book) 
            { 
                search_book.bookshelf_id = bk.BOOKSHELF_ID;
                if (bk.STATE == "1")
                    search_book.state = "在馆";
                if (bk.STATE == "2")
                    search_book.state = "已预约";
            }
            var bookshelf = my.BOOKSHELF.Find(search_book.bookshelf_id);
            var comment = my.USER_CLASSBOOK.Where(u => u.ISBN == isbn);
            foreach (var cm in comment)
            {
                string c = cm.CONTENT;
                search_book.comments.Add(c);
            }
            search_book.comment_num = search_book.comments.Count();
            
            search_book.isbn = classbook.ISBN;
            search_book.book_name = classbook.BOOK_NAME;
            search_book.publish = classbook.PUBLISHINGHOUSE;
            search_book.author = classbook.AUTHOR;
            search_book.type = classbook.TYPE;
            search_book.publish_date = string.Format("{0:d}", classbook.PUBLISH_DATE);
            search_book._abstract = classbook.ABSTRACT;
            search_book.call_number = classbook.CALL_NUMBER;
            search_book.image = classbook.IMAGE != null ? classbook.IMAGE : "https://webpac.tongji.edu.cn/tpl/images/nobook.jpg";
            search_book.floor = bookshelf.FLOOR.ToString();
            
            ViewBag.search_book = search_book;           
            return View();
        }
            //分页排序
            public IQueryable<CLASSBOOK> LoadPageItems(IQueryable<CLASSBOOK>book, int pageSize, int pageIndex,string sort, bool isAsc = true)
        {
            var temp = book;
            if (isAsc)
            { 
                /*if (sort == "TITLE")
                    temp = temp.OrderBy (u => u.BOOK_NAME).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                else if (sort == "AUTHOR")
                    temp = temp.OrderBy(u => u.AUTHOR).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                else if (sort == "PUBLISH")
                    temp = temp.OrderBy(u => u.PUBLISHINGHOUSE).Skip(pageSize * (pageIndex - 1)).Take(pageSize);*/
                if (sort == "PUBLISH_DATE")
                    temp = temp.OrderBy(u => u.PUBLISH_DATE).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                if (sort == "COMMON")
                    temp = temp.OrderBy(u => u.ISBN).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                return temp.AsQueryable();
            }
            else
            {
                /*if (sort == "TITLE")
                    temp = temp.OrderByDescending(u => u.BOOK_NAME).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                if (sort == "AUTHOR")
                    temp = temp.OrderByDescending(u => u.AUTHOR).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                if (sort == "PUBLISH")
                    temp = temp.OrderByDescending(u => u.PUBLISHINGHOUSE).Skip(pageSize * (pageIndex - 1)).Take(pageSize);*/
                if (sort == "PUBLISH_DATE")
                    temp = temp.OrderByDescending(u => u.PUBLISH_DATE).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                if (sort == "COMMON")
                    temp = temp.OrderByDescending(u => u.ISBN).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
                return temp.AsQueryable();
            }
        }
        public void collect(string isbn)
        {
            int u_id = Convert.ToInt32(Session["user_id"]);
            var result = from num in my.USER_COLLECT
                         where num.USER_ID == u_id && num.ISBN == isbn
                         select num;
            int i = 0;
            foreach (var u in result)
                i++;
            if (i == 0)
            {
                USER_COLLECT a = new USER_COLLECT();
                a.COLLECT_STATE = "已收藏";
                a.ISBN = isbn;
                a.USER_ID = Convert.ToInt32(Session["user_id"]);
                my.USER_COLLECT.Add(a);
                my.SaveChanges();
            }
            else
            {
                var result1 = result.FirstOrDefault();
                result1.COLLECT_STATE = "已收藏";
                my.SaveChanges();
            }
            //RedirectToAction("Book");
        }
        public void ucollect(string isbn)
        {
            int u_id = Convert.ToInt32(Session["user_id"]);
            var result = from num in my.USER_COLLECT
                         where num.USER_ID == u_id && num.ISBN == isbn
                         select num;
            var result1 = result.FirstOrDefault();

            result1.COLLECT_STATE = "未收藏";
            my.SaveChanges();
        }
        public void add_command(string content, string isbn)
        {
            USER_CLASSBOOK a = new USER_CLASSBOOK();
            a.USER_ID = Convert.ToInt32(Session["user_id"]);
            a.OP_TYPE = "评论";
            a.OP_DATE = DateTime.Now;
            a.ISBN = isbn;
            a.CONTENT = content;
            my.USER_CLASSBOOK.Add(a);
            my.SaveChanges();
        }
        public ActionResult pre()
        {
            string isbn = Request["isbn"];
            int u_id = Convert.ToInt32(Session["user_id"]);
            var x = from num in my.USER_PREBOOK
                    where num.USER_ID == u_id&&num.PRE_STATE== "已预约"
                    select num;
            int pre_amount = 0;
            foreach (var u in x)
                pre_amount++;
            var y = from a in my.USER_INFO
                    join b in my.PUNISH on a.CREDIT equals b.CREDIT
                    join c in my.LIMIT on b.LIMIT_TYPE equals c.LIMIT_TYPE
                    select new
                    {
                        re_bookamount = c.RE_BOOKAMOUNT
                    };
            var z = y.FirstOrDefault();
            int re_amount = Convert.ToInt32(z.re_bookamount);
            if (pre_amount >= re_amount)
                return Content(@"<script>alert('预约书籍数超过可预约书籍数，预约失败！');window.location.href='/Search/Book';</script>");
            else
            {
                var result1 = from num in my.USER_PREBOOK
                              where num.USER_ID == u_id && num.ISBN == isbn
                              select num;
                int i = 0;
                foreach (var u in result1)
                    i++;
                if (i == 0)
                {
                    USER_PREBOOK a = new USER_PREBOOK();
                    a.PRE_STATE = "已预约";
                    a.ISBN = isbn;
                    a.USER_ID = Convert.ToInt32(Session["user_id"]);
                    my.USER_PREBOOK.Add(a);
                    my.SaveChanges();
                }
                else
                {
                    var pre = result1.FirstOrDefault();
                    pre.PRE_STATE = "已预约";
                    my.SaveChanges();
                }
                return Content(@"<script>alert('预约成功');window.location.href='/Search/Book';</script>");
            }
        }
        public void upre(string isbn)
        {
            int u_id = Convert.ToInt32(Session["user_id"]);
            var result1 = from num in my.USER_PREBOOK
                          where num.USER_ID == u_id && num.ISBN == isbn
                          select num;
            var pre = result1.FirstOrDefault();

            pre.PRE_STATE = "未预约";
            my.SaveChanges();
        }
    }
}