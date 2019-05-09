﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WatsonWebserver;

namespace TestStaticRoutes
{
    static class Program
    {
        static void Main()
        {
            Server s = new Server("127.0.0.1", 9000, false, DefaultRoute);
            s.StaticRoutes.Add(HttpMethod.GET, "/hello/", GetHelloRoute);
            s.StaticRoutes.Add(HttpMethod.GET, "/world/", GetWorldRoute);
            s.StaticRoutes.Add(HttpMethod.POST, "/data/", PostDataRoute);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        static HttpResponse GetHelloRoute(HttpRequest req)
        { 
            return ResponseBuilder(req, "Watson says hello from the GET /hello static route!");
        }

        static HttpResponse GetWorldRoute(HttpRequest req)
        { 
            return ResponseBuilder(req, "Watson says hello from the GET /world static route!");
        }

        static HttpResponse PostDataRoute(HttpRequest req)
        {
            string text = null;
            if (req.Data != null) text = Encoding.UTF8.GetString(req.Data);
            return ResponseBuilder(req, text);
        }

        static HttpResponse DefaultRoute(HttpRequest req)
        { 
            return ResponseBuilder(req, WatsonCommon.SerializeJson(req));
        }

        static HttpResponse ResponseBuilder(HttpRequest req, string text)
        {
            return new HttpResponse(req, 200, null, "text/plain", Encoding.UTF8.GetBytes(text));
        }
    }
}
