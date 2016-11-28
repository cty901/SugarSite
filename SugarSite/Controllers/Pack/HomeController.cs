﻿using Infrastructure.Dao;
using Infrastructure.DbModel;
using Infrastructure.Pub;
using Infrastructure.ViewModels;
using SyntacticSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SqlSugar;
using Infrastructure;
using Infrastructure.Tool;

namespace SugarSite.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(DbService s) : base(s) { }

        #region Page View
        public ActionResult Index()
        {
            ViewBag.IsMainPage = true;
            return View();
        }

        public ActionResult Doc(int typeId = 0)
        {
            var model = new ResultModel<DocResult>();
            _service.Command<HomeOutsourcing, ResultModel<DocResult>>((o, api) =>
            {
                model = api.Get(Url.Action("GetDoc"), new { typeId = typeId });
            });
            ViewBag.IsAdmin = _userInfo != null && _userInfo.RoleId == 1;
            return View(model);
        }

        public ActionResult Reward()
        {
            return View();
        }

        public ActionResult Login(UserInfo user)
        {
            return View();
        }

        public ActionResult Register(UserInfo user, string confirmPassword)
        {
            return View();
        }


        #endregion

        #region Public API
        public JsonResult LoginSubmit(UserInfo user, string returnUrl)
        {
            var model = new ResultModel<string>();
            _service.Command<HomeOutsourcing>((db, o) =>
            {
                ExpCheck.Exception(user.UserName.IsNullOrEmpty() || user.Password.IsNullOrEmpty(), "用户名和密码不能为空！");
                user.Password = new EncryptSugar().MD5(user.Password);
                var loginUser = db.Queryable<UserInfo>().FirstOrDefault(it => it.UserName == user.UserName && it.Password == user.Password);
                if (loginUser == null)
                {
                    model.IsSuccess = false;
                    model.ResultInfo = "邮箱或者密码不正确！";
                }
                else
                {
                    var cm = CacheManager<UserInfo>.GetInstance();
                    string uniqueKey = PubGet.GetUserKey;
                    cm.Add(uniqueKey, loginUser, cm.Day * 365);//保存一年
                    model.IsSuccess = true;
                    model.ResultInfo = returnUrl;
                }

            });
            return Json(model);
        }

        public JsonResult RegisterSubmit(UserInfo user, string confirmPassword, string returnUrl)
        {
            var model = new ResultModel<string>();
            _service.Command<HomeOutsourcing>((db, o) =>
            {
                ExpCheck.Exception(user.UserName.IsNullOrEmpty() || user.Password.IsNullOrEmpty(), "用户名和密码不能为空！");
                ExpCheck.Exception(user.Password!=confirmPassword, "两次密码不一致！");
                user.Password = new EncryptSugar().MD5(user.Password);
                try
                {
                    user.RoleId = PubEnum.RoleType.User.TryToInt();
                    var id= db.Insert(user).ObjToInt();
                    var loginUser = db.Queryable<UserInfo>().InSingle(id);
                    var cm = CacheManager<UserInfo>.GetInstance();
                    string uniqueKey = PubGet.GetUserKey;
                    cm.Add(uniqueKey, loginUser, cm.Day * 365);//保存一年
                    model.IsSuccess = true;
                    model.ResultInfo = returnUrl;
                }
                catch
                {
                    model.IsSuccess = false;
                    model.ResultInfo = "用户注册失败,请联系我们！";
                }
            });
            return Json(model);
        }

        /// <summary>
        /// 获取文档
        /// </summary>
        /// <param name="TypeId"></param>
        /// <param name="Id"></param>
        /// <returns></returns>
        [OutputCache(Duration = 0)]
        public JsonResult GetDoc(int typeId)
        {
            var model = new ResultModel<DocResult>();
            _service.Command<HomeOutsourcing>((db, o) =>
            {
                model.ResultInfo = new Infrastructure.ViewModels.DocResult();
                model.ResultInfo.DocType = db.Queryable<DocType>().ToList();
                if (typeId == 0)//如果没有文章ID取第一条
                {
                    typeId = model.ResultInfo.DocType.Where(it => it.Level == 2).OrderBy(it => it.Level).ThenBy(it => it.Sort).First().Id;
                }
                var list = db.Queryable<DocContent>().Where(it => it.TypeId == typeId).ToList();
                model.ResultInfo.DocContent = list;
                model.ResultInfo.CurrentType = model.ResultInfo.DocType.Single(it => it.Id == typeId);
                model.IsSuccess = true;
            });
            return Json(model, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取验证码图片
        /// </summary>
        public void OutPutVerifyCode()
        {
            _service.Command<HomeOutsourcing>(o =>
            {
                o.OutPutVerifyCode(this.GetVerifyCode);
            });

        }
        #endregion
    }
}