using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Globalization;

namespace TemperatureLogger
{
    class DownloadXmlFile
    {
        public static string localFileName = Path.GetFullPath("KEWR.xml");

        static void LastModifiedXml()
        {
             Uri myUri = new Uri(@"http://w1.weather.gov/xml/current_obs/KEWR.xml");
				// Creates an HttpWebRequest for the specified URL. 
				HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(myUri);
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                DateTime today = DateTime.Now;
                DateTime lastModified = File.Exists(localFileName) ? File.GetLastWriteTime(localFileName) : DateTime.Now.AddDays(0.0);

                if (!File.Exists(localFileName))
                {
                    DownloadXml();
                    WriteXmlToDB();
                }
                else
                {
                    try
                    {
                        //myHttpWebRequest.Timeout = 10000;
                        //myHttpWebRequest.AllowWriteStreamBuffering = false;
                        myHttpWebRequest.IfModifiedSince = lastModified;
                        if (lastModified <= myHttpWebResponse.LastModified)
                        {
                            DownloadXml();
                            WriteXmlToDB();
                            Console.WriteLine("The file has been modified on {0}", myHttpWebResponse.LastModified);
                            myHttpWebResponse.Close();
                        }
                        else
                        {
                            Console.WriteLine("\nThe file has not been modified since " + myHttpWebResponse.LastModified);
                        }

                    }
                    catch (WebException e)
                    {
                        if (e.Response != null)
                        {
                            if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.NotModified)
                                Console.WriteLine("\nThe page has not been modified since " + lastModified);
                            else
                                Console.WriteLine("\nUnexpected status code = " + ((HttpWebResponse)e.Response).StatusCode);
                        }
                        else
                            Console.WriteLine("\nUnexpected Web Exception " + e.Message);
                    }
                }
        }

        static void DownloadXml()
        {
            string remoteUrl = @"http://w1.weather.gov/xml/current_obs/KEWR.xml";
            WebClient myWebClient = new WebClient(); // Create a new WebClient instance.
            myWebClient.UseDefaultCredentials = true;
            myWebClient.DownloadFile(remoteUrl, localFileName); // Download the Web resource and save it into the specific folder.
            //Console.WriteLine("Successfully Downloaded File \"{0}\" from \"{1}\"", localFileName, remoteUrl);
            Console.WriteLine("Successfully Downloaded Xml File \n\n");
        }

        
        static void WriteXmlToDB()
        {
            XDocument xdoc = XDocument.Load(Path.GetFullPath("KEWR.xml"));
            XmlDataInserttoDBDataContext dbContext = new XmlDataInserttoDBDataContext();
            var data = from item in xdoc.Descendants("current_observation")
                       //where (item!= null)
                       select new SensorReading
                       {
                           //observation_time = DateTime.ParseExact((item.Element("observation_time_rfc822").Value).Remove((item.Element("observation_time_rfc822").Value).IndexOf("Last Updated on ")), "ddd dd MMM yyyy h:mm tt zzz", null),
                           observation_time = DateTime.Parse((item.Element("observation_time_rfc822").Value)),
                           temp_f = Double.Parse(item.Element("temp_f").Value),
                           temp_c = Double.Parse(item.Element("temp_c").Value),
                           relative_humidity = Double.Parse(item.Element("relative_humidity").Value),
                           wind_mph = Double.Parse(item.Element("wind_mph").Value),
                           wind_kt = Double.Parse(item.Element("wind_kt").Value),
                           pressure_mb = Double.Parse(item.Element("pressure_mb").Value),
                           pressure_in = Double.Parse(item.Element("pressure_in").Value),
                           dewpoint_f = Double.Parse(item.Element("dewpoint_f").Value),
                           dewpoint_c = Double.Parse(item.Element("dewpoint_c").Value),
                           windchill_f = Double.Parse(item.Element("windchill_f").Value),
                           windchill_c = Double.Parse(item.Element("windchill_c").Value),
                           visibility_mi = Double.Parse(item.Element("visibility_mi").Value)
                       };
            dbContext.SensorReadings.InsertAllOnSubmit(data);
            dbContext.SubmitChanges();
            Console.WriteLine("Successfully inserted into Database, Please check the database.\n\n");
        }

         static void Main(string[] args)
        {
           // DownloadXml();
            LastModifiedXml();
            Console.ReadLine();
        
        }
    }
}
