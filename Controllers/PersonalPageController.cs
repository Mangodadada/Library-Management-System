using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using library_admin.Models;
using System.Web.Mvc;
using System.Data;

namespace library_admin.Controllers
{
    public class PersonalPageController : Controller
    {
        // GET: PersonalPage
        Entities entities = new Entities();
        public ActionResult Index()
        {
            int id = 2052710;
            int i = 0;
            decimal credit = 100;
            //个人页面基本参数：name ID credit
            ViewBag.name = "test1";
            if(Session["user_id"] != null)
            {
                id = (int)Session["user_id"];
                var user = entities.USER_INFO.Where(p => p.ID == id).ToList();
                if (user != null)
                {
                    ViewBag.name = user[0].NAME;
                    credit = user[0].CREDIT;
                }
            }
            ViewBag.ID = id;
            ViewBag.Credit = credit;
            //名片参数
            //信用状态
            var punish = entities.PUNISH.Where(p => p.CREDIT == credit).ToList();
            var limit_type = punish[0].LIMIT_TYPE;
            //权限信息
            var limit = entities.LIMIT.Where(p => p.LIMIT_TYPE == limit_type).ToList();
            var rebook_num = limit[0].RE_BOOKAMOUNT;
            var usebook_num = limit[0].USE_BOOKAMOUNT;
            var usebook_time = limit[0].USE_BOOKTIME;
            //当前借阅
            var borrowed_num = entities.USER_BOOK.Count(p => p.USER_ID == id );
            //预约座位数量
            var Order_num = entities.USER_SEAT.Count(p => p.USER_ID == id && p.OP_TYPE == "预约");
            //取消预约座位数量
            var Cancel_num = entities.USER_SEAT.Count(p => p.USER_ID == id && p.OP_TYPE == "取消");
            var seat_num = Order_num - Cancel_num;
            //过期图书
            //var expired_num = entities.USER_BOOK.Count(p => p.USER_ID == id && (DateTime.Compare(DateTime.Now, (DateTime)p.EXPECT_DATE)== 1));
            //预约图书数量
            var prebook_num = entities.USER_PREBOOK.Count(p => p.USER_ID == id && p.PRE_STATE == "已预约");
            //借阅书籍部分
            var book_data = entities.USER_BOOK.Where(p => p.USER_ID == id).OrderByDescending(p=>p.BORROW_DATE).ToArray();
            //评论书籍部分
            var classbook_data = entities.USER_CLASSBOOK.Where(p => p.USER_ID == id).OrderByDescending(p => p.OP_DATE).ToArray();
            //收藏书籍部分
            var collect_data = entities.USER_COLLECT.Where(p => p.USER_ID == id).ToArray();
            //预约座位部分 
            var seat_data = entities.USER_SEAT.Where(p => p.USER_ID == id).OrderByDescending(p => p.OP_TIME).ToArray();
            //获取公告数据
            //var notice = entities.NOTICE.OrderBy(p => p.PUBLISH_DATE).ToArray();
            //获取enum型数据集
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
            //将enum型转换为列表
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

            //转换数据
            BookDataView[] BookData = new BookDataView[3];
            SeatDataView[] SeatData = new SeatDataView[3];
            ClassBookDataView[] ClassbookData = new ClassBookDataView[3];
            for(i=0;i<3;i++)
            {
                BookData[i] = new BookDataView();
                SeatData[i] = new SeatDataView();
                ClassbookData[i] = new ClassBookDataView();
            }
            
            for(i = 0; i < 3 && i< book_data.Length; i++)
            {
                var q1 = book_data[i].BOOK_ID;
                var q2 = entities.BOOK.Where(p => p.ID == q1).ToList();
                var q3 = q2[0].ISBN;
                var q4 = entities.CLASSBOOK.Where(p => p.ISBN == q3).ToList();
                var name = q4[0].BOOK_NAME;
                BookData[i].BOOK_ID = name;
                BookData[i].BORROW_DATE = book_data[i].BORROW_DATE.ToString("yyyy-MM-dd");

                Console.WriteLine(book_data[i].BORROW_DATE.ToString("yyyy-MM-dd"));
               // BookData[i].RETURN_DATE = book_data[i].RETURN_DATE.ToString("yyyy-MM-dd");

                BookData[i].RETURN_DATE = book_data[i].EXPECT_DATE.ToString();
                if(DateTime.Compare(DateTime.Now, (DateTime)book_data[i].EXPECT_DATE) >= 0)
                {
                    BookData[i].STATE = "过期";
                }
                else
                    BookData[i].STATE = "正常借阅";

            }

            for (i = 0; i < 3 && i < seat_data.Length; i++)
            {
                SeatData[i].STUDYROOM_ID = seat_data[i].STUDYROOM_ID.ToString();
                SeatData[i].SEAT_ID = seat_data[i].SEAT_ID.ToString();
                SeatData[i].OP_TIME = seat_data[i].OP_TIME.ToString();
                SeatData[i].OP_TYPE = seat_data[i].OP_TYPE.ToString();
            }

            var i2 = 0;
            for (i = 0; i < 3 && i < classbook_data.Length; i++)
            {
                var q = classbook_data[i].ISBN;
                var u=entities.CLASSBOOK.Where(p => p.ISBN == q).ToArray();
                ClassbookData[i].BOOK_ID = u[0].BOOK_NAME;
                ClassbookData[i].COTENT = classbook_data[i].CONTENT.ToString();
                ClassbookData[i].OP_TIME = classbook_data[i].OP_DATE.ToString();
                ClassbookData[i].OP_TYPE = classbook_data[i].OP_TYPE.ToString();
                i2 = i + 1;
            }
            for (i = i2; i < 3 && (i-i2) < collect_data.Length; i++)
            {
                var q = collect_data[i - i2].ISBN;
                var u = entities.CLASSBOOK.Where(p => p.ISBN == q).ToArray();
                ClassbookData[i].BOOK_ID = u[0].BOOK_NAME;
                ClassbookData[i].COTENT = "-";
                ClassbookData[i].OP_TIME = "-";
                ClassbookData[i].OP_TYPE = collect_data[i - i2].COLLECT_STATE.ToString();
            }


            ViewBag.BookData = BookData;
            ViewBag.BorrowNum = borrowed_num;
            //ViewBag.ExpiredNum = expired_num;
            ViewBag.PrebookNum = prebook_num;
            ViewBag.SeatNum = seat_num;
            ViewBag.Notice = notices;
            ViewBag.SeatData = SeatData;
            ViewBag.ClassbookData = ClassbookData;
            ViewBag.LimitType = limit_type;
            ViewBag.RebookNum = rebook_num;
            ViewBag.UsebookNum = usebook_num;
            ViewBag.UsebookTime = usebook_time;
            
            return View();
        }

        public ActionResult Classbook()
        {
            //页面ID
            string ID = Request["id"] != null ? Request["id"] : "collect";
            ViewBag.Message = "我的收藏图书";
            //个人信息id
            int id = Session["user_id"] != null ? (int)Session["user_id"] : 2052710;
            var result = from book in entities.BOOK
                         join user_collect in entities.USER_COLLECT on book.ISBN equals user_collect.ISBN
                         from classbook in entities.CLASSBOOK
                         where book.ISBN == classbook.ISBN
                         where user_collect.USER_ID == id
                         where user_collect.COLLECT_STATE == "已收藏"
                         select new
                         {
                             Name = classbook.BOOK_NAME,
                             Author = classbook.AUTHOR,
                             Publisher = classbook.PUBLISHINGHOUSE,
                             ISBN = book.ISBN,
                         };
            foreach (var u in entities.ADMINISTRATOR)
            {
                ViewBag.Tests = u;
            }
            if (ID == "pre")
            {
                ViewBag.Message = "我的预约图书";
                result = from book in entities.BOOK
                             join user_prebook in entities.USER_PREBOOK on book.ISBN equals user_prebook.ISBN
                             from classbook in entities.CLASSBOOK
                             where book.ISBN == classbook.ISBN
                             where user_prebook.USER_ID == id
                             where user_prebook.PRE_STATE == "已预约"
                             select new
                             {
                                 Name = classbook.BOOK_NAME,
                                 Author = classbook.AUTHOR,
                                 Publisher = classbook.PUBLISHINGHOUSE,
                                 ISBN = book.ISBN,
                             };
            }
           

            int i = 0;
            borrowed_book[] rec = new borrowed_book[12];
            //result.ToList();
            foreach (var u in result)
            {
                borrowed_book a = new borrowed_book();
               // a.BookId = null;
               // a.BorrowTime = DateTime.Now;
               // a.ExpectTime = DateTime.Now;
                a.Name = u.Name;
                a.Author = u.Author;
                a.Publisher = u.Publisher;
                a.ISBN = u.ISBN;
                rec[i] = a;
                i++;
            }
            ViewBag.booklist = rec;
            ViewBag.amount = i;

           
            return View();
        }

    }

}
