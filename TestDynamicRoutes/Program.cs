﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WatsonWebserver;

namespace TestDynamicRoutes
{
    class Program
    {
        static void Main()
        {
            Server s = new Server("127.0.0.1", 9000, false, DefaultRoute);
            s.DynamicRoutes.Add(HttpMethod.GET, new Regex("^/foo/\\d+$"), GetFooWithId);
            s.DynamicRoutes.Add(HttpMethod.GET, new Regex("^/foo/(.*?)/(.*?)/?$"), GetFooMultipleChildren);
            s.DynamicRoutes.Add(HttpMethod.GET, new Regex("^/foo/(.*?)/?$"), GetFooOneChild);
            s.DynamicRoutes.Add(HttpMethod.GET, new Regex("^/foo/?$"), GetFoo); 
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        static HttpResponse GetFooWithId(HttpRequest req)
        {
            return ResponseBuilder(req, "Watson says hello from the GET /foo with ID dynamic route!");
        }

        static HttpResponse GetFooMultipleChildren(HttpRequest req)
        { 
            return ResponseBuilder(req, "Watson says hello from the GET /foo with multiple children dynamic route!");
        }

        static HttpResponse GetFooOneChild(HttpRequest req)
        { 
            return ResponseBuilder(req, "Watson says hello from the GET /foo with one child dynamic route!");
        }

        static HttpResponse GetFoo(HttpRequest req)
        { 
            return ResponseBuilder(req, "Watson says hello from the GET /foo dynamic route!");
        }

        static HttpResponse DefaultRoute(HttpRequest req)
        { 
            return ResponseBuilder(req, "Watson says hello from the default route!");
        }

        static HttpResponse ResponseBuilder(HttpRequest req, string text)
        { 
            return new HttpResponse(req, 200, null, "text/plain", Encoding.UTF8.GetBytes(text));
        }
    }
}
