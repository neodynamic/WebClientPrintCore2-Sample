﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Neodynamic.SDK.Web;
using Microsoft.AspNetCore.Hosting;

namespace WCPASPNETCOREWebSiteTest.Controllers
{
    public class DemoPrintFileController : Controller
    {
        private readonly IHostingEnvironment _hostEnvironment;


        public DemoPrintFileController(IHostingEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;

        }

        public IActionResult Index()
        {
            return View();
        }


        public object PrintFile(string useDefaultPrinter, string printerName, string fileType)
        {
            string fileName = Guid.NewGuid().ToString("N") + "." + fileType;
            string filePath = null;
            switch (fileType)
            {
                case "PDF":
                    filePath = "/files/LoremIpsum.pdf";
                    break;
                case "TXT":
                    filePath = "/files/LoremIpsum.txt";
                    break;
                case "DOC":
                    filePath = "/files/LoremIpsum.doc";
                    break;
                case "XLS":
                    filePath = "/files/SampleSheet.xls";
                    break;
                case "JPG":
                    filePath = "/files/penguins300dpi.jpg";
                    break;
                case "PNG":
                    filePath = "/files/SamplePngImage.png";
                    break;
                case "TIF":
                    filePath = "/files/patent2pages.tif";
                    break;
            }

            if (filePath != null)
            {
                PrintFile file = new PrintFile(_hostEnvironment.ContentRootPath + filePath, fileName);
                ClientPrintJob cpj = new ClientPrintJob();
                cpj.PrintFile = file;
                if (useDefaultPrinter == "checked" || printerName == "null")
                    cpj.ClientPrinter = new DefaultPrinter();
                else
                    cpj.ClientPrinter = new InstalledPrinter(System.Net.WebUtility.UrlDecode(printerName));

                return File(cpj.GetContent(), "application/octet-stream");
            }
            else
            {
                return BadRequest("File not found!");
            }
        }

    }
}
