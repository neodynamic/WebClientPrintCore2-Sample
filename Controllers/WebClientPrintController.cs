﻿using System;
using Microsoft.AspNetCore.Mvc;

using Neodynamic.SDK.Web;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;

namespace WCPASPNETCOREWebSiteTest.Controllers
{
    [Authorize]
    public class WebClientPrintController : Controller
    {
        //IMPORTANT NOTE >>>>>>>>>>
        // We're going to use MemoryCache to store users related stuff like
        // the list of printers and they have the WCPP client utility installed
        // BUT you can change it based on your dev needs!!!
        // For instance, you could use a Distributed Cache instead!
        //>>>>>>>>>>>>>>>>>>>>>>>>>
        private readonly IMemoryCache _MemoryCache;

        public WebClientPrintController(IMemoryCache memCache)
        {
            _MemoryCache = memCache;            
        }

        [AllowAnonymous]
        public object ProcessRequest()
        {
            //get session ID
            string sessionID = HttpContext.Request.Query["sid"].ToString();

            //get Query String
            string queryString = HttpContext.Request.QueryString.Value;

            try
            {
                //Determine and get the Type of Request 
                RequestType prType = WebClientPrint.GetProcessRequestType(queryString);

                if (prType == RequestType.GenPrintScript ||
                    prType == RequestType.GenWcppDetectScript)
                {
                    //Let WebClientPrint to generate the requested script
                    byte[] script = WebClientPrint.GenerateScript(Url.Action("ProcessRequest", "WebClientPrint", null, HttpContext.Request.Scheme), queryString);

                    return File(script, "text/javascript");
                }
                else if (prType == RequestType.ClientSetWcppVersion)
                {
                    //This request is a ping from the WCPP utility
                    //so store the session ID indicating it has the WCPP installed
                    //also store the WCPP Version if available
                    string wcppVersion = HttpContext.Request.Query["wcppVer"].ToString();
                    if (string.IsNullOrEmpty(wcppVersion))
                        wcppVersion = "1.0.0.0";

                    _MemoryCache.Set(sessionID + "wcppInstalled", wcppVersion);
                }
                else if (prType == RequestType.ClientSetInstalledPrinters)
                {
                    //WCPP Utility is sending the installed printers at client side
                    //so store this info with the specified session ID
                    string printers = HttpContext.Request.Query["printers"].ToString();
                    if (printers.Length > 0)
                        printers = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(printers));

                    _MemoryCache.Set(sessionID + "printers", printers);
                    
                }
                else if (prType == RequestType.ClientGetWcppVersion)
                {
                    //return the WCPP version for the specified sid if any
                    bool sidWcppVersion = (_MemoryCache.Get<string>(sessionID + "wcppInstalled") != null);
                    
                    return Ok(sidWcppVersion ? _MemoryCache.Get<string>(sessionID + "wcppInstalled") : "");

                }
                else if (prType == RequestType.ClientGetInstalledPrinters)
                {
                    //return the installed printers for the specified sid if any
                    bool sidHasPrinters = (_MemoryCache.Get<string>(sessionID + "printers") != null);

                    return Ok(sidHasPrinters ? _MemoryCache.Get<string>(sessionID + "printers") : "");
                }
                
                
            }
            catch
            {
                return BadRequest();
            }
            
            return null;
        }
    }
}
