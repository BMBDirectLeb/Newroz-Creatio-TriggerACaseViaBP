using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Xml;
using System.Web.Script.Serialization;


namespace Creatio___Trigger_a_business_process___Case
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
        private const string baseUri = "http://10.10.23.141:82";
        private const string authServiceUri = baseUri + @"/ServiceModel/AuthService.svc/Login";
        private const string processServiceUri = baseUri + @"/0/ServiceModel/ProcessEngineService.svc/";
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

        public static void GetListOfAgentDesktop(string queueid, string routingtype)
        {
            //39c25750-511c-4e98-9e10-2c340c69e5ea
            //C
            string requestString = processServiceUri +
                               "UsrProcess_80fe930/Execute?ResultParameterName=CaseList&QueueId=" + queueid + "&RoutingType=" + routingtype;
        // ttp://10.10.23.141:82/0/ServiceModel/ProcessEngineService.svc/UsrProcess_80fe930/Execute?ResultParameterName=CaseList
            HttpWebRequest request = HttpWebRequest.Create(requestString) as HttpWebRequest;
            request.Method = "GET";
            request.CookieContainer = AuthCookie;
            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    string responseText = reader.ReadToEnd();
                    Console.WriteLine(responseText);
                }
            }
        }

        static void Main(string[] args)
        {
            Console.Write("Enter Username: ");
            string user = Console.ReadLine();
            Console.Write("Enter Password: ");
            string pass = Console.ReadLine();
            if (!TryLogin(user, pass))
            {
                Console.WriteLine("Wrong login or password. Application will be terminated.");
            }
            else
            {
                Console.WriteLine("Login Successfull");
                Console.Write("Enter Queue: ");
                string queue = Console.ReadLine();
                Console.Write("Enter Routing Type: ");
                string routingtype = Console.ReadLine();
                try
                {
                    GetListOfAgentDesktop(queue, routingtype);// "39c25750-511c-4e98-9e10-2c340c69e5ea","I");
                }
                catch (Exception)
                {
                    // Process exception here. Or throw it further.
                    throw;
                }

            };

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}
