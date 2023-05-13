using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using library_admin.Models;

namespace library_admin.Controllers
{
    public class SelSeatController : Controller
    {
        Entities db = new Entities();

        // GET: Seat
        public ActionResult Index()
        {


            var seat2 = db.SEAT.Select(s => s).ToArray();
            ViewBag.seat_count = db.SEAT.Select(s => s).Count();
            SeatAd[] sa2 = new SeatAd[db.SEAT.Count()];
            for (int i = 0; i < ViewBag.seat_count; i++)
            {
                sa2[i] = new SeatAd();
            }
            for (int i = 0; i < ViewBag.seat_count; i++)
            {
                sa2[i].STUDYROOM_ID = seat2[i].STUDYROOM_ID;
                sa2[i].SEAT_ID = seat2[i].SEAT_ID.ToString();
                sa2[i].STATE = seat2[i].STATE;
            }
            ViewBag.sa = sa2;
            return View();


        }

        public ActionResult Search()
        {
            string Room = Request.Form["room_id"];
            ViewBag.room = Room;
            var seat = db.SEAT.Where(s => s.STUDYROOM_ID == Room)
                              .Select(s => s).ToArray();
            ViewBag.seat_count = db.SEAT.Where(s => s.STUDYROOM_ID == Room)
                              .Select(s => s).Count();




            if (Room != null && ViewBag.seat_count != 0)
            {

                SeatAd[] sa = new SeatAd[ViewBag.seat_count];
                for (int i = 0; i < ViewBag.seat_count; i++)
                {
                    sa[i] = new SeatAd();
                }
                for (int i = 0; i < ViewBag.seat_count; i++)
                {
                    sa[i].STUDYROOM_ID = seat[i].STUDYROOM_ID;
                    sa[i].SEAT_ID = seat[i].SEAT_ID.ToString();
                    sa[i].STATE = seat[i].STATE;
                }
                ViewBag.sa = sa;
                return View("Index");

            }
            else
            {
                return View("SearchBad");
            }


        }

        public ActionResult Add_Seat()
        {

            var roomid = from r in db.SEAT
                         group r by r.STUDYROOM_ID into g
                         orderby g.Count() descending, g.Key
                         where g.Count() >= 0
                         select new
                         {
                             room_id = g.Key,
                             Count = g.Count()
                         };


            var ray = roomid.ToArray();
            //将生成分类的下拉框选项集合设置给ViewBag 
            ViewBag.CateList_count = roomid.Count();
            string[] rd = new string[ViewBag.CateList_count];
            for (int i = 0; i < ViewBag.CateList_count; i++)
            {
                rd[i] = ray[i].room_id;
            }

            ViewBag.CateList = rd;
            return View();
        }
        public ActionResult Add2()
        {
            //接受
            SEAT sa = new SEAT();
            sa.STUDYROOM_ID = Request.Form["add_roomid"];
            sa.SEAT_ID = int.Parse(Request.Form["add_seatid"]);

            sa.STATE = Request.Form["add_state"];

            ViewBag.sa = sa;
            db.SEAT.Add(sa);
            db.SaveChanges();

            //使用重定向
            return RedirectToAction("Index");
            //return View();
        }

        public ActionResult Delect_Seat(string sid)//选择
        {


            if (sid == null)
            {
                return Content("传入为空" + sid);
            }
            int length = sid.Length;
            string rid = "";
            int i = 5;
            if (length <= 5)
            {
                return Content("传入错误，sid=" + sid);
            }
            rid = sid.Substring(length - i);
            int ssid = 0;
            ssid = int.Parse(sid.Replace(rid, ""));
            SEAT se = db.SEAT.Where(t => t.SEAT_ID == ssid && t.STUDYROOM_ID == rid).FirstOrDefault();
            //
            USER_SEAT huda_seat = new USER_SEAT();
            huda_seat.OP_TYPE = "预约";
            if (Session["user_id"] != null)
            { huda_seat.USER_ID = (int)(Session["user_id"]); }
            else
                return Content("请先登录后再进行操作");
            huda_seat.STUDYROOM_ID = se.STUDYROOM_ID;
            huda_seat.SEAT_ID = se.SEAT_ID;
            huda_seat.OP_TIME = DateTime.Now;//定义选座
            //
            var count1 = db.USER_SEAT.Count(p => p.USER_ID == huda_seat.USER_ID && p.OP_TYPE == "预约");
            var count2 = db.USER_SEAT.Count(p => p.USER_ID == huda_seat.USER_ID && p.OP_TYPE == "取消");
            if (se != null)
            {
                //db.Entry(se).State = System.Data.Entity.EntityState.Deleted;
                //db.SEAT.Remove(se);
                //db.SaveChanges();
                if (count1 == count2)//人物可选
                {
                    if (se.STATE == "空闲")//位置可选
                    {
                        se.STATE = "使用中";
                        db.USER_SEAT.Add(huda_seat);
                        db.SaveChanges();
                    }
                    else return Content("位置被占用，请选择其他座位");
                }
                else return Content("已选择其他的座位，一次只能预约一个座位，请取消座位后再进行选择");

                //使用重定向
                return RedirectToAction("Index");
            }
            else return Content("结果不唯一，错误");
        }
        public ActionResult Edit_Seat(string sid)
        {
            if (sid == null)
            {
                return Content("传入为空" + sid);
            }
            int length = sid.Length;
            string rid = "";
            int i = 5;
            if (length <= 5)
            {
                return Content("传入错误，sid=" + sid);
            }
            rid = sid.Substring(length - i);
            int ssid = 0;
            ssid = int.Parse(sid.Replace(rid, ""));
            SEAT se = db.SEAT.Where(t => t.SEAT_ID == ssid && t.STUDYROOM_ID == rid).FirstOrDefault();
            //
            USER_SEAT huda_seat = new USER_SEAT();
            huda_seat.OP_TYPE = "取消";
            if (Session["user_id"] != null)
            { huda_seat.USER_ID = (int)(Session["user_id"]); }
            else
                return Content("请先登录后再进行操作");
            huda_seat.STUDYROOM_ID = se.STUDYROOM_ID;
            huda_seat.SEAT_ID = se.SEAT_ID;
            huda_seat.OP_TIME = DateTime.Now;//定义选座
            var count1 = db.USER_SEAT.Count(p => p.USER_ID == huda_seat.USER_ID && p.OP_TYPE == "预约"&&p.SEAT_ID == se.SEAT_ID);
            var count2 = db.USER_SEAT.Count(p => p.USER_ID == huda_seat.USER_ID && p.OP_TYPE == "取消" && p.SEAT_ID == se.SEAT_ID);
            //
            if (se != null)
            {
                if (count1 > count2)
                {
                    if (se.STATE == "使用中")

                    {
                        db.USER_SEAT.Add(huda_seat);
                        se.STATE = "空闲";
                        db.SaveChanges();
                    }
                    else
                        return Content("空闲位置无法取消");
                }
                else
                    return Content("未选择该位置，无法取消");

                /*if (se.STATE == "空闲")
                {
                    se.STATE = "使用中";
                }
                else if (se.STATE == "使用中")
                {
                    se.STATE = "空闲";
                }

                db.Entry(se).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();*/
                //使用重定向


                return RedirectToAction("Index");
            }
            else return Content("结果不唯一，错误");
        }


    }





}