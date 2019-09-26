﻿using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WebImage.Models
{
    public class ContentModel
    {
        private static List<FileModel> FilesInDirectory;
        public List<FileModel> MyFiles { get; set; }

        public string ApiGetUrl { get; set; }

        public string MaxLen { get; set; }

        public ContentModel(string host, IHostingEnvironment env)
        {
            FilesInDirectory = new List<FileModel>();

            string path = Path.Combine(env.WebRootPath, "imagefolder");

            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    FileInfo f = new FileInfo(file);

                    var sizekb = Math.Round(((decimal)f.Length) / 1024, 1);
                    var sizeMb = Math.Round(((decimal)sizekb) / 1024, 2);

                    FilesInDirectory.Add(new FileModel()
                    {
                        Title = f.Name.Replace("@","").Replace(".",""),
                        Name = f.Name.Replace("@", "").Replace(".", ""),
                        Extension = f.Extension,
                        LengthKb = sizekb,
                        LengthMb = sizeMb,
                        Path = "/images/" + f.Name,
                        Url = host + "/images/" + f.Name
                    });
                }
            }
            MyFiles = FilesInDirectory;
        }


        //private JsonData GetJsonSelection()
        //{
        //    var myjson = this.SelectedFiles.Select(
        //        x => new JsonModel()
        //        {
        //            Url = x.Url,
        //            Title = x.Title,
        //            LengthKb = x.LengthKb,
        //            LengthMb = x.LengthMb
        //        }).ToList();

        //    return new JsonData()
        //    {
        //        MyJson = myjson
        //    };
        //}

        public void AddToSelection(string MySelectedFiles)
        {
            if (!string.IsNullOrEmpty(MySelectedFiles))
            {
                foreach (string fileName in MySelectedFiles.Split(','))
                {
                    MyFiles.FirstOrDefault(x => x.Name.Equals(fileName, StringComparison.InvariantCultureIgnoreCase)).IsSelected=true;
                }
            }
        }

        //public void GetFiles()
        //{
        //    if (SelectedFiles.Any())
        //    {
        //        this.MyFiles = FilesInDirectory.Where(x => !SelectedFiles.Contains(x)).ToList();
        //    }
        //    else
        //    {
        //        this.MyFiles = FilesInDirectory.ToList();
        //    }
        //}
    }



    public class FileModel : JsonModel
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public bool IsSelected { get; set; }
    }

    public class JsonModel
    {
        public string Title { get; set; }
        public decimal LengthKb { get; set; }
        public decimal LengthMb { get; set; }
        public string Url { get; set; }
    }

    public class JsonData
    {
        public List<JsonModel> MyJson { get; set; }
        public Statistics Stat { get; set; }

        public JsonData()
        {
            MyJson = new List<JsonModel>();
        }
    }

    public class Statistics
    {
        public int Count { get; set; }
        public decimal TotalLengthKb { get; set; }
        public decimal TotalLengthMb { get; set; }
        public double ElapsedTime { get; set; }
    }
}
