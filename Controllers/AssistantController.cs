using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using library_admin.Models;
using System.Data;
using System.Configuration;

using System.Data.SqlClient;

namespace library_admin.Controllers
{
    public class AssistantController : Controller
    {

        Entities db = null;//数据库实例

       
        // GET: Assistant
        public ActionResult Index()
        {
            if(db ==null)// 暂时做不到设为全局
            {
                db = new Entities();
            }

            var linq = (from w in db.USER_INFO
                        select new
                        {
                            Name = w.NAME,
                            ID = w.ID
                        });
            List<assistant_list> list = new List<assistant_list>();
            foreach (var tmp in linq)
            {
                //if (i > ) break;
                assistant_list a = new assistant_list();
                a.name = tmp.Name;
                a.id = tmp.ID;
                list.Add(a);
            }
            ViewBag.DataList = list;
            ViewBag.DataCount = list.Count();


            //查询结果
            //var text = Request["keyboard"];

            return View("View");
            
        }

        [HttpPost]
        public ActionResult List()
        {
            //拿到搜索框的值
            var req = Request;
            var text = req["text"];

            List<string> ttext = new List<string>();
            foreach(string str in text.Split(' '))
            {
                if (str == null) continue;
                ttext.Add(str);
            }

            //查询人员
            if (db == null)
            {
                db = new Entities();
            }
            var linq = (from w in db.USER_INFO
                        select new
                        {
                            Name = w.NAME,
                            ID = w.ID
                        });
            foreach(string tmp in ttext)
            {
                int i = -1;
                bool b = int.TryParse(tmp,out i);
                
                if(b)
                    //做不到模糊查询ID，因为有报错：linq无法使用强制转换的函数
                    linq = linq.Where(w => w.Name.Contains(tmp) || w.ID == i);
                else
                    linq = linq.Where(w => w.Name.Contains(tmp));
            }
            List<assistant_list> list = new List<assistant_list>();
            foreach (var tmp in linq)
            {
                //if (i > ) break;
                assistant_list a = new assistant_list();
                a.name = tmp.Name;
                a.id = tmp.ID;
                list.Add(a);
            }
            ViewBag.DataList = list;
            ViewBag.DataCount = list.Count();
            return PartialView("List");
        }

        [HttpPost]
        public ActionResult Detail()
        {
            var req = Request;
            var index = int.Parse(req["index"]);

            if (db == null)
            {
                db = new Entities();
            }

            //个人
            var infoLinq = (from w in db.USER_INFO
                            join x in db.PUNISH on w.CREDIT equals x.CREDIT
                            join y in db.LIMIT on x.LIMIT_TYPE equals y.LIMIT_TYPE
                            where w.ID == index
                            select new
                            {
                                Name = w.NAME,
                                ID = w.ID,
                                Credit = w.CREDIT,
                                ExBookAmount = y.RE_BOOKAMOUNT,
                                BrBookAmount = y.USE_BOOKAMOUNT,
                                BrBookTime = y.USE_BOOKTIME
                            });
            //现自习室
            var seatLinq = (from w in db.USER_SEAT
                            where w.USER_ID == index
                                  &&
                                  w.OP_TYPE == "预约"
                            select new
                            {
                                StudyRoom = w.STUDYROOM_ID,
                                Seat = w.SEAT_ID,
                                OPSeat = w.OP_TYPE,
                                Time = w.OP_TIME
                            });
            //现书籍
            var bookLinq = (from w in db.USER_BOOK
                            join x in db.BOOK on w.BOOK_ID equals x.ID
                            join y in db.CLASSBOOK on x.ISBN equals y.ISBN
                            where w.USER_ID == index
                            select new
                            {
                                ISBN = y.ISBN,
                                BookName = y.BOOK_NAME,
                                Publish = y.PUBLISHINGHOUSE,
                                Author = y.AUTHOR,
                                Borrow = w.BORROW_DATE,
                                Expect = w.EXPECT_DATE,
                                Return = w.RETURN_DATE
                            }
                            );

            assistant_list person = new assistant_list();
            foreach( var item in infoLinq)
            {
                person.name = item.Name;
                person.id = item.ID;
                person.credit = (int)item.Credit;
                person.limit_re_book_amount = (int)item.ExBookAmount;
                person.limit_use_book_amount = (int)item.BrBookAmount;
                person.limit_use_book_time = (item.BrBookTime).ToString();
            }
            foreach(var item in seatLinq)
            {
                assistant_room seat = new assistant_room();
                seat.room_id = item.StudyRoom;
                seat.seat_id = (int)item.Seat;
                seat.op = item.OPSeat;
                seat.time = item.Time;
                person.rooms.Add(seat);
            }
            foreach(var item in bookLinq)
            {
                assistant_book book = new assistant_book();
                book.ISBN = item.ISBN;
                book.name = item.BookName;
                book.publish = item.Publish;
                book.author = item.Author;
                book.borrow_date = (item.Borrow).ToString();
                book.expect_date = (item.Expect).ToString();
                book.return_date = (item.Return).ToString();
                person.books.Add(book);
            }

            ViewBag.detailIndex = index;
            ViewBag.Detail = person;
            return PartialView("Detail");
        }
    }
}