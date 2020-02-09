﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using WebImage.Context;

namespace WebImage.Models
{
    public class ContentModel
    {
        private readonly IjpContext ijpContext;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IWebHostEnvironment hostingEnv;

        public List<IjpCategory> Category { get; set; }
        public List<IjpFile> FileContent { get; set; }
        public List<MyGallery> MySavedGalleries { get; set; }
        public List<FileModel> MyFiles { get; set; }
        public string TypeSelected { get; set; }
        public string ApiUrl { get; set; }
        public string HideAttributeList { get; set; }
        public string Bookmark { get; set; }
        //public string[] Descriptions { get; set; }
        public string NewGalleryDescription { get; set; }
        public string NewGalleryName { get; set; }

        public List<string> MyGalleryNames { get; set; }


        public ContentModel(IWebHostEnvironment _hostingEnv, IHttpContextAccessor _httpContextAccessor, IjpContext _ijpContext, string imgs = "", string hc = "", string gallery = "")
        {
            hostingEnv = _hostingEnv;
            httpContextAccessor = _httpContextAccessor;
            ijpContext = _ijpContext;

            MyGalleryNames = new List<string>();
            var request = httpContextAccessor.HttpContext.Request;
            string host = request.Host.ToString();

            MySavedGalleries = new List<MyGallery>();

            foreach (var item in ijpContext.Gallery)
            {
                MySavedGalleries.Add(new MyGallery()
                {
                    Gallery = item,
                    GalleryFile = ijpContext.GalleryFile.Where(x => x.GalleryId == item.GalleryId).ToList(),
                    IsSelected = item.Name == gallery
                });
            }

            if (string.IsNullOrEmpty(gallery))
            {
                Bookmark = string.IsNullOrEmpty(imgs) ? "" : imgs;
                HideAttributeList = string.IsNullOrEmpty(hc) ? "" : hc;
                ApiUrl = request.Scheme + "://" + request.Host + "/api/gallery?imgs=" + Bookmark + "&hc=" + HideAttributeList;
            }
            else
            {
                Bookmark = MySavedGalleries.FirstOrDefault(x => x.IsSelected).Gallery.Images;
                HideAttributeList = MySavedGalleries.FirstOrDefault(x => x.IsSelected).Gallery.Columns;
                ApiUrl = MySavedGalleries.FirstOrDefault(x => x.IsSelected).Gallery.Url;
            }

            MyFiles = new List<FileModel>();

            foreach (var myimg in ijpContext.File)
            {
                MyFiles.Add(new FileModel()
                {
                    FileId = myimg.FileId,
                    CategoryId = myimg.CategoryId,
                    RawFormat = myimg.RawFormat,
                    IsPrivate = myimg.IsPrivate,
                    IsLandscape = myimg.IsLandscape,
                    LengthKB = myimg.LengthKB,
                    Name = myimg.Name,
                    Title = myimg.Title,
                    Width = myimg.Width,
                    Height = myimg.Height,
                    HorizontalResolution = myimg.HorizontalResolution,
                    VerticalResolution = myimg.VerticalResolution,
                    PixelFormat = myimg.PixelFormat,
                    IsSelected = (string.IsNullOrEmpty(gallery)) ? false : MySavedGalleries.FirstOrDefault(z => z.IsSelected).IsImageInGallery(myimg.FileId) ? true : false,
                    Url = host + "/images/" + myimg.Name,
                    Path = Path.Combine(".", "imagefolder", myimg.Name),
                    Thumb = Path.Combine(".", "imagefolder", "Thumbs", Path.GetFileNameWithoutExtension(myimg.Name) + "_thumb" + Path.GetExtension(myimg.Name)),
                    Content = Helper.byteFile(Path.Combine(hostingEnv.WebRootPath, "imagefolder", myimg.Name))
                });
            }

            Category = new List<IjpCategory>();
            Category.AddRange(ijpContext.Category.OrderBy(x => x.Name));

            ijpContext.Gallery.OrderBy(a => a.Name).ToList().ForEach(x => this.MyGalleryNames.Add(new string(x.Name + " - " + x.Description + " (" + x.DateInsert.ToLocalTime() + ")")));

        }

        public void SaveGallery(int galleryId, string galleryName, string galleryDescription, string userId)
        {
            using TransactionScope scope = new TransactionScope();

            if (galleryId==0)
            {

                if (!string.IsNullOrEmpty(this.Bookmark))
                {
                    var mygallery = new IjpGallery()
                    {
                        Name = galleryName,
                        Description = galleryDescription,
                        Url = this.ApiUrl,
                        Columns = this.HideAttributeList,
                        DateInsert = DateTime.Now,
                        UserId = userId,
                        Images=this.Bookmark
                    };

                    ijpContext.Gallery.Add(mygallery);
                    ijpContext.SaveChanges();

                    var images = Helper.Decode(this.Bookmark);

                    foreach (string img in images.Split(','))
                    {
                        var file = ijpContext.File.FirstOrDefault(x => x.Name.Equals(img));

                        if (file != null)
                        {
                            ijpContext.GalleryFile.Add(new IjpGalleryFile
                            {
                                FileId = file.FileId,
                                GalleryId = mygallery.GalleryId,
                                Description = "description of " + img
                            });
                        }
                    }
                    ijpContext.SaveChanges();
                }
            }
            else
            {
                var mygallery = this.MySavedGalleries.FirstOrDefault(x => x.Gallery.GalleryId == galleryId);

                if (mygallery != null)
                {
                    mygallery.Gallery.Name = galleryName;
                    mygallery.Gallery.Description = galleryDescription;
                    mygallery.Gallery.DateUpdate = DateTime.Now;
                    mygallery.Gallery.UserId = userId;
                    mygallery.Gallery.Url = this.ApiUrl;
                    mygallery.Gallery.Columns = this.HideAttributeList;
                    mygallery.Gallery.Images = this.Bookmark;
                }





                ijpContext.SaveChanges();
            }


            scope.Complete();
        }


        public string AttributeChecked(string columnName)
        {
            string columnsList = Helper.Decode(HideAttributeList);
            var isChecked = columnsList.Contains(columnName) ? "" : "checked";
            return isChecked;
        }

        public string DisplayInfo(string columnName)
        {
            string columnsList = Helper.Decode(HideAttributeList);
            var isvisible = columnsList.Contains(columnName) ? "none" : "block";
            return isvisible;
        }


        public void AddToSelection(string MySelectedFiles)
        {
            if (!string.IsNullOrEmpty(MySelectedFiles))
            {
                foreach (string fileName in MySelectedFiles.Split(','))
                {
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        MyFiles.FirstOrDefault(x => x.Name.Equals(fileName)).IsSelected = true;
                    }
                }
            }
        }

        public void RemoveFromSelection(string MySelectedFiles)
        {
            if (!string.IsNullOrEmpty(MySelectedFiles))
            {
                MyFiles.FirstOrDefault(x => x.Name.Equals(MySelectedFiles, StringComparison.InvariantCultureIgnoreCase)).IsSelected = false;
            }
        }


        public MyData GetFileInfoJson(string imgs, string hc)
        {
            string hidecolumn = string.IsNullOrEmpty(hc) ? "" : Helper.Decode(hc);

            MyData myData = new MyData();

            if (!string.IsNullOrEmpty(imgs))
            {
                var selectedImages = Helper.Decode(imgs);

                this.AddToSelection(selectedImages);

                this.MyFiles.Where(a => a.IsSelected).ToList().ForEach(x => myData.MyJson.Add(new MyJson()
                {
                    Url = hidecolumn.Contains("url") ? "" : x.Url,
                    Name = hidecolumn.Contains("name") ? "" : x.Name,
                    Title = hidecolumn.Contains("title") ? "" : x.Title,
                    Format = hidecolumn.Contains("format") ? "" : x.RawFormat,
                    Length = hidecolumn.Contains("length") ? 0 : (double)x.LengthKB,
                    IsLandscape = (hidecolumn.Contains("landscape") ? null : x.IsLandscape.ToString()),
                    PixelFormat = hidecolumn.Contains("pixel") ? "" : x.PixelFormat,
                    Size = hidecolumn.Contains("size") ? "" : x.Width.ToString() + " X " + x.Height.ToString(),
                    Resolution = hidecolumn.Contains("resolution") ? "" : x.HorizontalResolution + " X " + x.VerticalResolution,
                    Content = hidecolumn.Contains("content") ? null : x.Content
                }));
            }
            else
            {
                this.MyFiles.ForEach(x => myData.MyJson.Add(new MyJson()
                {
                    Url = x.Url,
                    Name = x.Name,
                    Title = x.Title,
                    Format = x.RawFormat,
                    Length = (double)x.LengthKB,
                    IsLandscape = x.IsLandscape.ToString(),
                    PixelFormat = x.PixelFormat,
                    Size = x.Width.ToString() + " X " + x.Height.ToString(),
                    Resolution = x.HorizontalResolution + " X " + x.VerticalResolution,
                    Content = x.Content
                }));
            }
            return myData;
        }
    }


    public class FileModel : IjpFile
    {
        public bool IsSelected { get; set; }
        public string Path { get; set; }
        public string Url { get; set; }
        public string Thumb { get; set; }
        public byte[]? Content { get; set; }
    }


    public class JsonData
    {
        public MyData MyData { get; set; }
        public Statistics Stat { get; set; }

        public JsonData()
        {
            MyData = new MyData();
        }

    }


    public class MyData
    {
        public List<MyJson> MyJson { get; set; }
        public string Profile { get; set; }

        public MyData()
        {
            MyJson = new List<MyJson>();
        }
    }



    public class MyJson
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Format { get; set; }
        public double Length { get; set; }
        public string IsLandscape { get; set; }
        public string PixelFormat { get; set; }
        public string Size { get; set; }
        public string Resolution { get; set; }
        public string Url { get; set; }
        public byte[] Content { get; set; }
    }



    public class Statistics
    {
        public int Count { get; set; }
        public double TotalLengthKb { get; set; }
        public double ElapsedTime { get; set; }
    }
}
