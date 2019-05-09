using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WatsonWebserver;
using System.IO;

namespace TestDefault
{
    static class Program
    {
        public const string dir = "C:\\Users\\jsayres\\Source\\Repos\\WatsonWebserver\\TestDefault\\webserver\\webserver";

        static void Main()
        {
            List<string> hostnames = new List<string>();
            hostnames.Add("127.0.0.1");

            Server server = new Server(hostnames, 9000, false, GetIndexRoute);

            // add static routes
            server.StaticRoutes.Add(HttpMethod.GET, "/", GetIndexRoute);
            server.StaticRoutes.Add(HttpMethod.GET, "/Status", GetStatusRoute);
            server.StaticRoutes.Add(HttpMethod.GET, "/Monitor", GetMonitorRoute);
            server.StaticRoutes.Add(HttpMethod.GET, "/Health", GetHealthRoute);
            server.StaticRoutes.Add(HttpMethod.GET, "/Configuration", GetConfigurationRoute);
            server.StaticRoutes.Add(HttpMethod.GET, "/VersionControl", GetVersionControlRoute); 
            server.StaticRoutes.Add(HttpMethod.GET, "/SystemTest", GetSystemTestRoute);
            server.StaticRoutes.Add(HttpMethod.GET, "/LogFiles", GetLogFilesRoute);

            // add dynamic routes
            server.DynamicRoutes.Add(HttpMethod.GET, new Regex("(.*?).csv?$"), GetSpecificLogFile);
            server.DynamicRoutes.Add(HttpMethod.GET, new Regex("(.*?).xml?$"), GetUpdate);
            // server.AccessControl.Mode = AccessControlMode.DefaultDeny;
            // server.AccessControl.Whitelist.Add("127.0.0.1", "255.255.255.255");
            // server.AccessControl.Whitelist.Add("127.0.0.1", "255.255.255.255");

            bool runForever = true;
            while (runForever)
            {
                string userInput = WatsonCommon.InputString("Command [? for help] >", null, false);
                switch (userInput.ToLower())
                {
                    case "?":
                        Menu();
                        break;

                    case "q":
                        runForever = false;
                        break;

                    case "c":
                    case "cls":
                        Console.Clear();
                        break;

                    case "state":
                        Console.WriteLine("Listening: " + server.IsListening);
                        break;

                    case "dispose":
                        server.Dispose();
                        break;
                }
            }
        }

        private static HttpResponse GetUpdate(HttpRequest req)
        {
            return XmlResponseBuilder(req, WIFI_SendDynamicPage("/webserver" + req.RawUrlWithoutQuery));
        }

        private static HttpResponse GetSpecificLogFile(HttpRequest req)
        {
            return ResponseBuilder(req, SendRequestedLogFile("/logs" + req.RawUrlWithoutQuery));
        }

        private static HttpResponse GetLogFilesRoute(HttpRequest req)
        {
            //return ResponseBuilder(req, SendLogFileElements());
            return ResponseBuilder(req, SendIndexPage(req.RawUrlWithoutQuery));
        }

        private static HttpResponse GetHealthRoute(HttpRequest req)
        {
            return ResponseBuilder(req, SendIndexPage(req.RawUrlWithoutQuery));
        }

        private static HttpResponse GetSystemTestRoute(HttpRequest req)
        {
            return ResponseBuilder(req, SendIndexPage(req.RawUrlWithoutQuery));
        }

        private static HttpResponse GetVersionControlRoute(HttpRequest req)
        {
            return ResponseBuilder(req, SendIndexPage(req.RawUrlWithoutQuery));
        }

        private static HttpResponse GetConfigurationRoute(HttpRequest req)
        {
            return ResponseBuilder(req, SendIndexPage(req.RawUrlWithoutQuery));
        }

        private static HttpResponse GetMonitorRoute(HttpRequest req)
        {
            return ResponseBuilder(req, SendIndexPage(req.RawUrlWithoutQuery));
        }

        private static HttpResponse GetStatusRoute(HttpRequest req)
        {
            return ResponseBuilder(req, SendIndexPage(req.RawUrlWithoutQuery));
        }

        private static HttpResponse GetIndexRoute(HttpRequest req)
        {
            return ResponseBuilder(req, SendIndexPage(""));
        }

        static void Menu()
        {
            Console.WriteLine("---");
            Console.WriteLine("  ?        help, this menu");
            Console.WriteLine("  q        quit the application");
            Console.WriteLine("  cls      clear the screen");
            Console.WriteLine("  state    indicate whether or not the server is listening");
            Console.WriteLine("  dispose  dispose the server object");
        }

        static HttpResponse RequestReceived(HttpRequest req)
        { 
            return new HttpResponse(req, 200, null, "text/plain", Encoding.UTF8.GetBytes("Watson says hello from the default route!"));
        }

        //void WIFI_ParsePageName(char* pageName)
        //{
        //    SYS_PRINT("page:%s\n\r", pageName);
        //    char* args;
        //    char* argName;
        //    char* argValue;
        //    args = strchr(pageName, '?');
        //    if (args != NULL)
        //    {
        //        argValue = strchr(pageName, '=');
        //        if (argValue != NULL)
        //        {
        //            argName = args + 1;
        //            *argValue = '\0';
        //            argValue++;
        //            SYS_PRINT("Name:%s\n\r", argName);
        //            SYS_PRINT("Value:%s\n\r", argValue);
        //            WIFI_ParseArgs(argName, argValue);
        //        }
        //        *args = '\0';
        //    }

        //    if (strstr(pageName, ".csv"))
        //    {
        //        SendRequestedLogFile(pageName);
        //    }
        //    else if (strstr(pageName, ".xml"))
        //    {
        //        SendStatusUpdateXML(pageName);
        //    }
        //    else
        //    {
        //        if (strstr(pageName, ".") == 0)              // Should we send the index page
        //            strcat(pageName, HTTP_DEFAULT_FILE);

        //        WIFI_SendHttpResponseOk();

        //        WIFI_SendStaticPage("/style.htm");
        //        WIFI_SendStaticPage("/header.htm");
        //        WIFI_SendDynamicPage(pageName);
        //        WIFI_SendStaticPage("/footer.htm");
        //    }
        //}


        static string SendIndexPage(string pageName)
        {
            string message = "";

            message += WIFI_SendStaticPage("/webserver/style.htm");
            message += WIFI_SendStaticPage("/webserver/header.htm");
            //message += WIFI_SendStaticPage("/webserver" + pageName + "/index.htm");

            message += WIFI_SendDynamicPage("/webserver" + pageName + "/index.htm");
            message += WIFI_SendStaticPage("/webserver/footer.htm");

            return message;
        }

        static string WIFI_SendStaticPage(string pageName)
        {
            string localBuffer = "";
            string smallBuffer = "";
            string filePath = pageName;
            if (!String.IsNullOrEmpty(filePath) && filePath.StartsWith("/")) filePath = filePath.Substring(1);
            filePath = AppDomain.CurrentDomain.BaseDirectory + filePath;
            filePath = filePath.Replace("+", " ").Replace("%20", " ");
            
            if (File.Exists(filePath))
            {
                FileStream fs = File.OpenRead(filePath);
                while (fs.Position < fs.Length)
                {
                    localBuffer += Convert.ToChar(fs.ReadByte());
                }
                return localBuffer;
            }
            else
            {
                localBuffer =  "<div id=\"content\"><p>Please insert an SD Card with the proper web server files into the EOT and try again.</p></div>";
                return localBuffer;
            }
        }

        static string WIFI_SendDynamicPage(string pageName)
        {
            string localBuffer = "";

            string filePath = pageName;
            if (!String.IsNullOrEmpty(filePath) && filePath.StartsWith("/")) filePath = filePath.Substring(1);
            filePath = AppDomain.CurrentDomain.BaseDirectory + filePath;
            filePath = filePath.Replace("+", " ").Replace("%20", " ");

            if (File.Exists(filePath))
            {
                return ParseDynamicVariable(filePath);
            }
            else
            {
                localBuffer = "<div id=\"content\"><p>Please insert an SD Card with the proper web server files into the EOT and try again.</p></div>";
                return localBuffer;
            }
        }

        static string ParseDynamicVariable(string file)
        {
            char currentByte;
            string tmpString = "";
            char variableSymbol = '~';
            string message = "";
            bool symbolFound = false;
            bool isStatusPage = false;

            FileStream fs = File.OpenRead(file);
            if (file.Contains("/status.xml"))
                isStatusPage = true;
            if(isStatusPage)
                message += "<response>";
            while (fs.Position < fs.Length)
            {
                symbolFound = false;
                currentByte = Convert.ToChar(fs.ReadByte());
                if (currentByte.Equals(variableSymbol))
                {
                    tmpString = "";
                    currentByte = Convert.ToChar(fs.ReadByte());
                    tmpString += currentByte;
                    while (!symbolFound)
                    {
                        currentByte = Convert.ToChar(fs.ReadByte());
                        if (!currentByte.Equals(variableSymbol))
                        {
                            tmpString += currentByte;
                        }
                        else
                        {
                            symbolFound = true;
                        }
                    }
                    message += "<" + tmpString + ">";
                    message += GetVariableValue(tmpString, file); //parse variables here//Convert.ToString(random_generator_float());
                    message += "</" + tmpString + ">";
                }
                else
                {
                    if (!isStatusPage)
                        message += currentByte;
                }
            }
            if (isStatusPage)
                message += "</response>";
            return message;
        }

        static string GetVariableValue(string variableName, string pageName)
        {
            float tempFloat;

            if ((pageName.Contains("/Monitor/index.htm")) || (pageName.Contains("/Monitor/status.xml")))
            {
                if (variableName.Equals("v3p3"))
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("v3p8"))
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("v5"))
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("v12"))
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("vExtCharge"))
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("vExtReg"))
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("vGen"))
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("vReg"))
                {
                    tempFloat = random_generator_float();
                    if (tempFloat > 100)
                    {
                        tempFloat = 0;
                    }
                    variableName = string.Format("{0:P2}V", tempFloat);
                }
                else if (variableName.Equals("vIn")) //charging voltage
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("iIn")) //charging current
                {
                    variableName = string.Format("{0:P2}A", random_generator_float());
                }
                else if (variableName.Equals("vBatt"))
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("iBatt"))
                {
                    variableName = string.Format("{0:P2}A", random_generator_float());
                    //            if (CHARGER_GetIbat() < 0)  //#issue #474
                    //            {
                    //                variableName = string.Format("{0}A", 0.0);
                    //            } else {
                    //                variableName = string.Format("{0}A", CHARGER_GetIbat());
                    //            }
                }
                else if (variableName.Equals("vSys"))
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("iLoad"))
                {
                    variableName = string.Format("{0:P2}A", random_generator_float());
                }
                else if (variableName.Equals("vUHF1"))
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("vUHF2"))
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("temp"))
                {
                    variableName = string.Format("{0:P2}F", (int)random_generator_float());
                }
                else if (variableName.Equals("chargeTemp"))
                {
                    variableName = string.Format("{0:P2}F", (random_generator_float() * 1.8 + 32.00));
                }
                else if (variableName.Equals("chargeCount"))
                {
                    variableName = string.Format("{0:P2}", (int)random_generator_float());
                }
                else if (variableName.Equals("maxCount"))
                {
                    variableName = string.Format("{0:P2}", (int)random_generator_float());
                }
                else if (variableName.Equals("minCount"))
                {
                    variableName = string.Format("{0:P2}", (int)random_generator_float());
                }
                else if (variableName.Equals("accelX"))
                {
                    variableName = string.Format("{0:P2}", (int)random_generator_float());
                }
                else if (variableName.Equals("accelY"))
                {
                    variableName = string.Format("{0:P2}", (int)random_generator_float());
                }
                else if (variableName.Equals("accelZ"))
                {
                    variableName = string.Format("{0:P2}", (int)random_generator_float());
                }
            }
            else if ((pageName.Contains("/Health/index.htm")) || (pageName.Contains("/Health/status.xml")))
            {
                if (variableName.Equals("gEOT"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gAntDet"))
                {
                    variableName = FormatYN(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gAntShort"))
                {
                    variableName = FormatYN(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gGNSS"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gCell"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gXDCR"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gBrake"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gValve"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gGen"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gOverspin"))
                {
                    variableName = FormatYN(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gHVM"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gButt"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gSD"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gAccel"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gRTC"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gTemp"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gDLS"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gCharge"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gChargeExt"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gChargeGen"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gRPM"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gWifi"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("gBT"))
                {
                    variableName = FormatGB(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
            }
            else if ((pageName.Contains("/Status/index.htm")) || (pageName.Contains("/Status/status.xml")))
            {
                //        GPGGA_t gpgga = GNSS_GetGPGGA();
                //        GPVTG_t gpvtg = GNSS_GetGPVTG();
                //        GPRMC_t gprmc = GNSS_GetGPRMC();

                if (variableName.Equals("BPP"))
                {
                    variableName = string.Format("{0}psi", random_generator_uint16());
                }
                else if (variableName.Equals("MRP"))
                {
                    variableName = string.Format("{0}psi", random_generator_uint16());
                }
                else if (variableName.Equals("vBatt"))
                {
                    variableName = string.Format("{0:P2}V", random_generator_float());
                }
                else if (variableName.Equals("pBatt"))
                {
                    if (random_generator_uint16() < 5.0)
                    {
                        variableName = string.Format("*{0}%", random_generator_uint16());
                    }
                    else
                    {
                        variableName = string.Format("{0}%", random_generator_uint16());
                    }
                }
                else if (variableName.Equals("pGNSS"))
                {
                    if ((0x0001 & random_generator_uint16()) > 0)
                    {
                        variableName = string.Format("{0}", "OFF");
                    }
                    else
                    {
                        variableName = string.Format("{0}", "ON");
                    }
                }
                else if (variableName.Equals("lat"))
                {
                    //            if (gpgga.latitude == 0 || gpgga.latitude == (-2))
                    //            {
                    //                variableName = string.Format("{0}", "NO DATA");
                    //            } else {
                    variableName = string.Format("{0:P2}", (random_generator_float() / 3600000));
                    //            }
                }
                else if (variableName.Equals("long"))
                {
                    //            if (gpgga.longitude == 0 || gpgga.longitude == (-2))
                    //            {
                    //                variableName = string.Format("{0}", "NO DATA");
                    //            } else {
                    variableName = string.Format("{0:P2}", (random_generator_float() / 3600000));
                    //            }
                }
                else if (variableName.Equals("UTC"))
                {
                    //            if (gpgga.hour <= 0 || gpgga.min <= 0 || gpgga.sec < 0)
                    //            {
                    variableName = string.Format("{0}", "NO DATA");
                    //            } else {
                    //                if (gpgga.min < 10)
                    //                {
                    //                    variableName = string.Format("{0}:%0d:{0}", gpgga.hour, gpgga.min, gpgga.sec);
                    //                } else {
                    //                    variableName = string.Format("{0}:{0}:{0}", gpgga.hour, gpgga.min, gpgga.sec);
                    //                }
                    //            }       
                }
                else if (variableName.Equals("fix"))
                {
                    if ((0x0001 & random_generator_uint16()) > 0)
                    {
                        variableName = string.Format("{0}", "NO");
                    }
                    else
                    {
                        variableName = string.Format("{0}", "YES");
                    }
                }
                else if (variableName.Equals("HDOP"))
                {
                    if ((0x0001 & random_generator_uint16()) > 0)
                    {
                        variableName = string.Format("{0}", "NO DATA");
                    }
                    else
                    {
                        variableName = string.Format("{0:P2}", random_generator_float());
                    }
                }
                else if (variableName.Equals("sSpeed"))
                {
                    tempFloat = random_generator_float();
                    if (tempFloat <= 0)
                    {
                        variableName = string.Format("{0:P2}mph", tempFloat);
                    }
                    else
                    {
                        tempFloat = tempFloat * (float).6214; //convert to mph
                        variableName = string.Format("{0:P2}mph", tempFloat);
                    }
                }
                else if (variableName.Equals("sMot"))
                {
                    variableName = FormatYN(variableName, (ushort)(0x0001 & random_generator_uint16()));
                }
                else if (variableName.Equals("sBrake"))
                {
                    if ((0x0001 & random_generator_uint16()) > 0)
                    {
                        variableName = string.Format("{0}", "YES");
                    }
                    else
                    {
                        variableName = string.Format("{0}", "NO");
                    }
                }
                else if (variableName.Equals("sGen"))
                {
                    if ((0x0001 & random_generator_uint16()) > 0 )
                    //Somewhere the logic for the GenValve being open/closed is backwards, so it returns the opposite of what's true.
                    //This is a bandaid fix, reversed the NO and YES so it returns the truth on WiFi.
                    {
                        variableName = string.Format("{0}", "NO");
                    }
                    else
                    {
                        variableName = string.Format("{0}", "YES");
                    }
                }
                else if (variableName.Equals("sDark"))
                {
                    if ((0x0001 & random_generator_uint16()) > 0 ) //light
                    {
                        variableName = string.Format("{0}", "LIGHT");
                    }
                    else //Dark
                    {
                        variableName = string.Format("{0}", "DARK");
                    }
                }
                else if (variableName.Equals("sHVM"))
                {
                    if ((0x0001 & random_generator_uint16()) > 0)
                    {
                        variableName = string.Format("{0}", "OFF");
                    }
                    else
                    {
                        variableName = string.Format("{0}", "ON");
                    }
                }
                else if (variableName.Equals("sEmer"))
                {
                    variableName = string.Format("{0}", ((0x0001 & random_generator_uint16()) > 0 ) ? "TRUE" : "FALSE");
                }
                else if (variableName.Equals("sRPM"))
                {
                    variableName = string.Format("{0}", random_generator_uint16());
                }
                else if (variableName.Equals("sCell"))
                {
                    if ((0x0001 & random_generator_uint16()) > 0)
                    {
                        variableName = string.Format("{0}", "OFF");
                    }
                    else
                    {
                        variableName = string.Format("{0}", "ON");
                    }
                }
                else if ((0x0001 & random_generator_uint16()) > 0)
                {
                    if ((0x0001 & random_generator_uint16()) > 0) //IS UPRIGHT
                    {
                        variableName = string.Format("{0}", "YES");
                    }
                    else
                    {
                        variableName = string.Format("{0}", "NO");
                    }
                }
            }
            else if (pageName.Contains("/VersionControl/index.htm"))
            {
                if (variableName.Equals("fwVer"))
                {
                    variableName = "33";
                }
                else if (variableName.Equals("bootVer"))
                {
                    variableName = "2";
                }
                else if (variableName.Equals("mdlNum"))
                {
                    variableName = "1";
                }
                else if (variableName.Equals("wifiModuleName"))
                {
                    variableName = WIFI_GetProductName();
                }
                else if (variableName.Equals("wifiModuleID"))
                {
                    variableName = "EOT25";
                }
                else if (variableName.Equals("wifiModuleFW"))
                {
                    variableName = WIFI_GetFirmware();
                }
                else if (variableName.Equals("lpupFW"))
                {
                    variableName = string.Format("9VD81A-{0}", 1);  //#issue #466
                }
                else if (variableName.Equals("lpupBoot"))
                {
                    variableName = string.Format("9VD80A-{0}", 1); //#issue #466
                }
                else if (variableName.Equals("rrId"))
                {
                    variableName = "25";
                }
                else if (variableName.Equals("eotId"))
                {
                    variableName = "26";
                }
            }
            else if (pageName.Contains("/Configuration/index.htm"))
            {
                if (variableName.Equals("calDate"))
                {
                    variableName = "5-26-2019";
                }
                else if (variableName.Equals("calLoc"))
                {
                    variableName = "Durham";
                }
                else if (variableName.Equals("calTech"))
                {
                    variableName = "JAS";
                }
                else if (variableName.Equals("battRplcDate"))
                {
                    variableName = "5-26-2019";
                }
                else if (variableName.Equals("battRplcLoc"))
                {
                    variableName = "Durham";
                }
                else if (variableName.Equals("battRplcTech"))
                {
                    variableName = "JAS";
                }
                else if (variableName.Equals("eotEnbl"))
                {
                    variableName = string.Format("{0}", ((0x0001 & random_generator_uint16()) > 0) ? "True" : "False");
                }
                else if (variableName.Equals("callWakeTime"))
                {
                    variableName = string.Format("{0}", random_generator_uint16());
                }
                else if (variableName.Equals("callSlpTime"))
                {
                    variableName = string.Format("{0}", random_generator_uint16());
                }
                else if (variableName.Equals("brdSerial"))
                {
                    variableName = "111";
                }
                else if (variableName.Equals("mdlNum"))
                {
                    variableName = "01";
                }
                else if (variableName.Equals("rrId"))
                {
                    variableName = "25";
                }
                else if (variableName.Equals("eotId"))
                {
                    variableName = "26";
                }
                // <editor-fold defaultstate="collapsed" desc="Factory Config Variables">
                else if (variableName.Equals("pipeCfg"))
                {
                    if (((0x0001 & random_generator_uint16()) > 0))
                        variableName = string.Format("Dual Pipe");
                    else
                        variableName = string.Format("Single Pipe");
                }
                else if (variableName.Equals("liteToDrkThrsh"))
                {
                    variableName = string.Format("{0}", random_generator_uint16());
                }
                else if (variableName.Equals("drkToLiteThrsh"))
                {
                    variableName = string.Format("{0}", random_generator_uint16());
                }
                else if (variableName.Equals("shwBattRdy"))
                {
                    if (((0x0001 & random_generator_uint16()) > 0))
                    {
                        variableName = string.Format("True");
                    }
                    else
                    {
                        variableName = string.Format("False");
                    }
                }
                else if (variableName.Equals("battRdyTxt"))
                {
                    if (((0x0001 & random_generator_uint16()) > 0))
                    {
                        variableName = string.Format("YES");
                    }
                    else
                    {
                        variableName = string.Format("NO");
                    }
                }
                else if (variableName.Equals("battNotRdyTxt"))
                {
                    if (((0x0001 & random_generator_uint16()) > 0))
                    {
                        variableName = string.Format("True");
                    }
                    else
                    {
                        variableName = string.Format("False");
                    }
                }
                else if (variableName.Equals("calEnbl"))
                {
                    variableName = string.Format("{0}", ((0x0001 & random_generator_uint16()) > 0) ? "True" : "False");
                }
                else if (variableName.Equals("wifiPass"))
                {
                    variableName = "SiemensEOTwifi";
                }

                else if (variableName.Equals("wifiEnbl"))
                {
                    variableName = string.Format("{0}", ((0x0001 & random_generator_uint16()) > 0) ? "True" : "False");
                }

                else if (variableName.Equals("genExst"))
                {
                    variableName = string.Format("{0}", ((0x0001 & random_generator_uint16()) > 0) ? "True" : "False");
                }
                else if (variableName.Equals("genVlvExst"))
                {
                    variableName = string.Format("{0}", ((0x0001 & random_generator_uint16()) > 0) ? "True" : "False");
                }
                else if (variableName.Equals("hvmExst"))
                {
                    variableName = string.Format("{0}", ((0x0001 & random_generator_uint16()) > 0) ? "True" : "False");
                }
                else if (variableName.Equals("calDateEdit"))
                {
                    //CFG_GetCalibrationDate(tempBuffer);
                    //FormatDate(variableName, tempBuffer);
                }
                else if (variableName.Equals("battRplcDateEdit"))
                {
                    //CFG_GetBatteryReplacementDate(tempBuffer);
                    //FormatDate(variableName, tempBuffer);
                }
                else if (variableName.Equals("calEnblEdit"))
                {
                    //variableName = string.Format("{0}", CFG_GetCalibrationEnabled() ? "Checked" : "");
                }
                else if (variableName.Equals("eotEnblEdit"))
                {
                    //variableName = string.Format("{0}", CFG_GetEotEnabled() ? "Checked" : "");
                }
            }

            else if (pageName.Contains("/LogFiles/index.htm"))
            {
                if (variableName.Equals("Logs"))
                {
                    variableName = SendLogFileElements();
                }
            }
            else
            {
                return "";
            }
            return variableName;
        }

        static string SendLogFileElements()
        {
            UInt16 i = 0;
            bool done = false;

            string localBuffer;
            string filePath = "/logs";
            if (!String.IsNullOrEmpty(filePath) && filePath.StartsWith("/")) filePath = filePath.Substring(1);
            filePath = AppDomain.CurrentDomain.BaseDirectory + filePath;
            filePath = filePath.Replace("+", " ").Replace("%20", " ");
            string[] fileList = Directory.GetFiles(filePath);

            localBuffer = "<div id=\"content\"><h2>Log Files</h2><div class=\"scroll\">";
            if (fileList.Length > 0)
            {
                for (i = 0; i < fileList.Length; i++)
                {
                    localBuffer += "<p><a href=\"";
                    localBuffer += string.Format("{0}", fileList[i].Split('\\').Last());
                    localBuffer += "\" download>";
                    localBuffer += string.Format("{0}", fileList[i].Split('\\').Last());
                    localBuffer += "</a>";
                }
                localBuffer += "</div></div><p>&nbsp;</p>";
            }
            else
            {
                localBuffer += "</div></div><p>&nbsp;</p>";
            }
            return localBuffer;
        }

        static string SendRequestedLogFile(string logFile)
        {
            string logData = "";
            byte[] buffer;

            string filePath = logFile;
            if (!String.IsNullOrEmpty(filePath) && filePath.StartsWith("/")) filePath = filePath.Substring(1);
            filePath = AppDomain.CurrentDomain.BaseDirectory + filePath;
            filePath = filePath.Replace("+", " ").Replace("%20", " ");

            FileStream fileHandle = File.OpenRead(filePath);
            if (File.Exists(filePath))
            {
                buffer = new byte[fileHandle.Length];
                while (fileHandle.Position < fileHandle.Length)
                    logData += Convert.ToChar(fileHandle.ReadByte());
            }
            return logData;
        }

        static string WIFI_GetFirmware()
        {
            return "33.4";
        }

        static string WIFI_GetProductName()
        {
            return "Invetek";
        }

        static string FormatYN(string variableName, UInt16 variableVal)
        {
            variableName = "";
            if (variableVal == 0)
            {
                variableName="NO";
            }
            else
            {
                variableName = "YES";
            }
            return variableName;
        }

        static string FormatGB(string variableName, UInt16 variableVal)
        {
            variableName = "";
            if (variableVal == 0) //Not a failure
            {
                variableName = "GOOD";
            }
            else if (variableVal == 1) //Failure
            {
                variableName = "BAD";
            }
            else
            { //Should never get here but just in case
                variableName = "ERROR";
            }
            return variableName;
        }

        private static float random_generator_float()
        {
            Random r = new Random();
            return (float)r.NextDouble();
        }

        private static int random_generator_uint16()
        {
            Random r = new Random();
            return r.Next();
        }

        static HttpResponse XmlResponseBuilder(HttpRequest req, string text)
        {
            req.ContentType = "text/xml";
            return new HttpResponse(req, 200, null, "text/xml", Encoding.UTF8.GetBytes(text));
        }

        static HttpResponse ResponseBuilder(HttpRequest req, string text)
        {
            return new HttpResponse(req, 200, null, "text/plain", Encoding.UTF8.GetBytes(text));
        }
    }
}
