﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebImage.DBContext;
using WebImage.Models;

namespace WebImage.Controllers
{
    [Produces("application/json")]
    public class HomeController : Controller
    {
        private readonly IjpContext ijpContext;
        private UserManager<IdentityUser> userManager { get; set; }

        public HomeController(IjpContext _ijpContext, UserManager<IdentityUser> _userManager)
        {
            ijpContext = _ijpContext;
            userManager = _userManager;
        }

        [HttpPost("/api/gallery/{id}/user/{userId}")]
        public async Task<JsonResult> GetListSelection(int id, string userId)
        {
            if (id > 0)
            {
                DateTime StartDate = DateTime.Now;
                var mycontainer = new MyContainer(ijpContext, userId);
                mycontainer.myGalleries.LoadGallery(id, userId);

                var mydata = mycontainer.GetFileInfoJson();

                return Json(new JsonGallery()
                {
                    Data = mydata,

                    Stat = new JsonStats()
                    {
                        Count = mydata.MyJson.Count(),
                        TotalLengthKb = mydata.MyJson.Select(x => x.Length).Sum(),
                        ElapsedTime = (DateTime.Now - StartDate).TotalMilliseconds
                    }
                }); 
            }
            else
                return Json(new JsonGallery());
        }

    }
}
