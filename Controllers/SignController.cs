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


namespace library_admin.Controllers
{
    public class SignController : Controller
    {
        // GET: Sign
        public ActionResult UserSignIn()
        {
            return View();
        }
        public ActionResult userlogin()
        {
            if (Request["id"] == null || Request["pw"] == null|| Request["id"] == ""|| Request["id"] == "")
                return Content("no:账号或者密码不能为空！");
            else
            {
                int user_id = Convert.ToInt32(Request["id"]);
                string user_pw = Request["pw"];
                Entities my = new Entities();
                var result = from num in my.USER_INFO
                             where num.ID == user_id && num.PASSWORD == user_pw
                             select num;
                int i = 0;
                string user_name = null;
                foreach (var u in result)
                {
                    i++;
                    user_name = u.NAME;
                }
                if (i != 0)
                {
                    Session["user_id"] = user_id;
                    Session["user_name"] = user_name;
                    return Content("ok:登录成功！");
                }
                else
                {
                    return Content("no:账号或者密码错误！");
                }
            }
            //Session["user_id"] = user_id;
        }
        public ActionResult adminlogin()
        {
            if (Request["id"] == null || Request["pw"] == null || Request["id"] == "" || Request["id"] == "")
                return Content("no:账号或者密码不能为空！");
            else
            {
                int admin_id = Convert.ToInt32(Request["id"]);
                string admin_pw = Request["pw"];
                Entities my = new Entities();
                var result = from num in my.ADMINISTRATOR
                             where num.ID == admin_id && num.PASSWORD == admin_pw
                             select num;
                int i = 0;
                string admin_name = null;
                foreach (var u in result)
                {
                    i++;
                    admin_name = u.NAME;
                }
                if (i != 0)
                {
                    Session["admin_id"] = admin_id;
                    Session["admin_name"] = admin_name;
                    return Content("ok:登录成功！");
                }
                else
                {
                    return Content("no:账号或者密码错误！");
                }
            }
        }
        [HttpPost]
        public ActionResult UserSignIn(string id, string pw)
        {
            //Sysuser sysuser = UserSever.Login(zhanghao, mima);
            int a = UserSever.UserLogin(id, pw);
            if (a == 1)
            {
                return Content("<script>alert('登录成功');window.location.href='../Home';</script>");//如果输入正确
            }
            else
            {
                return Content("<script>alert('账号或者密码不正确');window.location.href='../Sign/UserSignIn';</script>");
            }
        }
        public ActionResult AdminSignIn()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AdminSignIn(string id, string pw)
        {
            //Sysuser sysuser = UserSever.Login(zhanghao, mima);
            int a = UserSever.AdminLogin(id, pw);
            if (a == 1)
            {
                return Content("<script>alert('登录成功');window.location.href='../Home';</script>");//如果输入正确
            }
            else
            {
                return Content("<script>alert('账号或者密码不正确');window.location.href='../Sign/AdminSignIn';</script>");
            }
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(string id, string pw,string pw1,string phonenumber,string email)
        {
            //Sysuser sysuser = UserSever.Login(zhanghao, mima);
            int a = UserSever.RegisterNew(id, pw);
            //先判断两遍密码是否一样
            if (a == 1)
            {
                return Content("<script>alert('注册成功');window.location.href='../Sign/UserSignIn';</script>");//如果输入正确
            }
            else
            {
                return Content("<script>alert('注册不成功，请您重试');window.location.href='../Sign/Register';</script>");
            }
        }
    }
}