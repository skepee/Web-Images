﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebImage.Context;
using WebImage.Models;

namespace WebImage.Controllers
{
    public class HomeController : Controller
    {
        private readonly IjpContext ijpContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IHostingEnvironment hostingEnv;

        private string Host { get; set; }

        public HomeController(IHostingEnvironment _hostingEnv, IHttpContextAccessor _httpContextAccessor, IjpContext _ijpContext)
        {
            hostingEnv = _hostingEnv;
            httpContextAccessor = _httpContextAccessor;
            ijpContext = _ijpContext;

            //TestData d = new TestData(hostingEnv, httpContextAccessor, ijpContext);
            //d.AddFromDirectory();
            //d.DownloadImages();

            var request = httpContextAccessor.HttpContext.Request;
            Host = request.Host.ToString();
        }

        //[ResponseCache(Duration = 30, Location = ResponseCacheLocation.Client, NoStore = false)]
        public IActionResult GenerateJson()
        {
            ContentModel myFiles = new ContentModel(Host, hostingEnv, ijpContext);
            return View(myFiles);
        }

        public IActionResult ClientCarousel()
        {
            return View("ClientCarousel");
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult RemoveFromSelectionList(string pars)
        {
            ContentModel myFiles = new ContentModel(Host, hostingEnv, ijpContext);
            if (!string.IsNullOrEmpty(pars))
            {
                string decpars = Helper.Decode(pars);

                string decSelectedFiles = decpars.Split("|")[0];
                myFiles.TypeSelected = decpars.Split("|")[1];
                myFiles.Profile = decpars.Split("|")[2];

                myFiles.RemoveFromSelection(decSelectedFiles);
                myFiles.ApiGetUrl = Request.Scheme + "://" + Request.Host + "/api/getselection/" + pars;
            }
            return View("GenerateJson", myFiles);
        }


        public IActionResult AddToSelectionList(string pars)
        {
            ContentModel myFiles = new ContentModel(Host, hostingEnv, ijpContext);
            if (!string.IsNullOrEmpty(pars))
            {
                string decpars = Helper.Decode(pars);

                string decSelectedFiles = decpars.Split("|")[0];
                myFiles.TypeSelected= decpars.Split("|")[1];
                myFiles.Profile = decpars.Split("|")[2];

                myFiles.AddToSelection(decSelectedFiles);
                myFiles.ApiGetUrl = Request.Scheme + "://" + Request.Host + "/api/getselection/" + pars;
            }
            return View("GenerateJson", myFiles);
        }

        public IActionResult Admin()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //[HttpGet("/api/getmax/{MaxLengthKB}")]
        //public JsonResult GetList(decimal MaxLengthKB)
        //{
        //    DateTime StartDate = DateTime.Now;

        //    ContentModel myfiles = new ContentModel(Host, hostingEnv);
        //    List<FileModel> files = new List<FileModel>();

        //    if (MaxLengthKB > 0)
        //    {
        //        files = myfiles.MyFiles.Where(x => x.LengthKb <= MaxLengthKB).ToList();
        //    }
        //    else
        //    {
        //        files = myfiles.MyFiles;
        //    }

        //    var mydata = new MyData()
        //    {
        //        MyJson = files.Select(
        //        x => new JsonModel()
        //        {
        //            Url = x.Url,
        //            Title = x.Title,
        //            LengthKb = x.LengthKb,
        //            LengthMb = x.LengthMb
        //        }).ToList(),

        //        Profile = "test"
        //    };

        //    var c = new Statistics()
        //    {
        //        Count = files.Count(),
        //        TotalLengthKb = files.Select(x => x.LengthKb).Sum(),
        //        TotalLengthMb = files.Select(x => x.LengthMb).Sum(),
        //        ElapsedTime = (DateTime.Now - StartDate).TotalDays

        //    };

        //    return Json(new JsonData()
        //    {
        //        MyData = new List<MyData>()
        //        {
        //            mydata
        //        },

        //        Stat = c
        //    });
        //}


        [HttpGet("/api/getselection/{pars}")]
        public JsonResult GetListSelection(string pars)
        {
            string decodedString =  Helper.Decode(pars);

            DateTime StartDate = DateTime.Now;

            ContentModel myfiles = new ContentModel(Host, hostingEnv, ijpContext);
            List<FileModel> files = new List<FileModel>();

            if (!string.IsNullOrEmpty(decodedString))
            {
                string decSelectedFiles = decodedString.Split("|")[0];
                //string TypeSelected = decodedString.Split("|")[1];
                string Profile = decodedString.Split("|")[2];

                files = myfiles.MyFiles.Where(x => decSelectedFiles.Split(",").Contains(x.Name)).ToList();

                var mydata = new MyData()
                {
                    MyJson = files.Select(
                    x => new FileContent()
                    {
                        Url = x.Url,
                        Title = x.Title,
                        LengthKB = x.LengthKB,
                        LengthMB = x.LengthMB                        
                    }).ToList(),

                    Profile = Profile
                };

                return Json(new JsonData()
                {
                    MyData = new List<MyData>()
                    {
                        mydata
                    },

                    Stat = new Statistics()
                    {
                        Count = files.Count(),
                        TotalLengthKb = files.Select(x => x.LengthKB).Sum(),
                        TotalLengthMb = files.Select(x => x.LengthMB).Sum(),
                        ElapsedTime = (DateTime.Now - StartDate).TotalDays
                    }
                });
            }
            else
            {
                return Json(null);
            }
        }
    }
}
