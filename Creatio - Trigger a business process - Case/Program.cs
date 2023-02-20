using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Xml;
using System.Web.Script.Serialization;
using System.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Configuration;


namespace QueryAgentCaseDistribution
{

    class ResponseStatus
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public object Exception { get; set; }
        public object PasswordChangeUrl { get; set; }
        public object RedirectUrl { get; set; }
    }
    class Program
    {
        /*
         Case direct routing
         39c25750-511c-4e98-9e10-2c340c69e5ea
         C
         Case Internal routing
         f10d54bd-06cc-40e8-b20a-9c7233d1b5c5 
         I
        */
        public static string user = ConfigurationManager.AppSettings["username"];
        public static  string pass = ConfigurationManager.AppSettings["password"];
        public static string stat = ConfigurationManager.AppSettings["status"];
        private static string baseUri = ConfigurationManager.AppSettings["base"];

        private static string authServiceUri = baseUri + @"/ServiceModel/AuthService.svc/Login";
        private static string processServiceUri = baseUri + @"/0/ServiceModel/ProcessEngineService.svc/";
        private static ResponseStatus status = null;
        public static CookieContainer AuthCookie = new CookieContainer();
        public static bool TryLogin(string userName, string userPassword)
        {

            var authRequest = HttpWebRequest.Create(authServiceUri) as HttpWebRequest;
            authRequest.Method = "POST";
            authRequest.ContentType = "application/json";
            authRequest.CookieContainer = AuthCookie;
            using (var requesrStream = authRequest.GetRequestStream())
            {
                using (var writer = new StreamWriter(requesrStream))
                {
                    writer.Write(@"{
                    ""UserName"":""" + userName + @""",
                    ""UserPassword"":""" + userPassword + @"""
                    }");
                }
            }
            using (var response = (HttpWebResponse)authRequest.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = reader.ReadToEnd();
                    status = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<ResponseStatus>(responseText);
                }
            }
            if (status != null)
            {
                if (status.Code == 0)
                {
                    return true;
                }
                Console.WriteLine(status.Message);
            }
            return false;
        }

        public static void GetListOfAgentCases(string queueid, string routingtype,string status)
        {
            //Direct routing examples below
            //39c25750-511c-4e98-9e10-2c340c69e5ea
            //C

            //Internal routing examples below
            //f10d54bd-06cc-40e8-b20a-9c7233d1b5c5
            //I
            //if ((queueid == "39c25750-511c-4e98-9e10-2c340c69e5ea" || queueid == "f10d54bd-06cc-40e8-b20a-9c7233d1b5c5") && (routingtype == "C" || routingtype == "I"))

            int startIndex;
                int endIndex;
                string jsonString = "";
                string requestString = processServiceUri +
                                   "UsrProcess_80fe930/Execute?ResultParameterName=CaseList&QueueId=" + queueid + "&RoutingType=" + routingtype + "&status=" + status;
                HttpWebRequest request = HttpWebRequest.Create(requestString) as HttpWebRequest;
                request.Method = "GET";
                request.CookieContainer = AuthCookie;
            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = reader.ReadToEnd();
                    startIndex = responseText.IndexOf("[{");
                    if (startIndex > 0)
                    {
                        endIndex = responseText.LastIndexOf("}]") + 2;
                        jsonString = responseText.Substring(startIndex, endIndex - startIndex);
                        Console.WriteLine("Response as follow");
                        Console.WriteLine(jsonString);
                    }
                    else
                    {
                        var data = new { Message = "No Data Found" };
                        var json = JsonConvert.SerializeObject(data);
                        Console.WriteLine(json);
                        //Console.ReadLine();
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            if (!TryLogin(user, pass))
            {
                var data = new { Message = "Wrong login or password" };
                var json = JsonConvert.SerializeObject(data);
                Console.WriteLine(json);
            }
            else
            {
                Console.Write("Enter The Queue Id: ");
                string queue = Console.ReadLine();

                Console.Write("Enter The Routing Type,'C' for direct case routing and 'I' for internal case routing: ");
                string routingtype = Console.ReadLine();

                Console.WriteLine();

                try
                {
                    GetListOfAgentCases(queue, routingtype, stat);
                }
                catch (Exception er)
                {
                    Console.WriteLine(er);
                    throw;
                }

            };
            Console.ReadLine();
        }
    }
}
