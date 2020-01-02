using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;
using System.Web;

namespace vimeoDownloader
{
    class Program
    {
        struct odinvideo
        {
            public int idn;
            public string title;
        }
        static Queue<odinvideo> videos = new Queue<odinvideo>();
        //очередь адресов для закачки
        //static Queue<string> URLs = new Queue<string>();
        //список скачанных страниц
        static List<string> HTMLs = new List<string>();
        //локер для очереди адресов
        static object URLlocker = new object();
        //локер для списка скачанных страниц
        static object HTMLlocker = new object();
        static object errorlocker = new object();
        static List<string> csvInf = new List<string>();
        static void Main(string[] args)
        {
            string line;
            StreamReader file = new StreamReader(@"result.txt");

            if (File.Exists(@"errors.txt"))
            {
                File.Delete(@"errors.txt");
            }
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "video/upload.csv"))
            {
                File.Delete(AppDomain.CurrentDomain.BaseDirectory + "video/upload.csv");
            }

            while ((line = file.ReadLine()) != null)
            {
                Console.WriteLine(line);
                odinvideo vid = new odinvideo();
                string[] lines = Regex.Split(line, ";;;;;");
                vid.idn = Convert.ToInt32(lines[0]); //Console.WriteLine(lines[0]); Console.ReadKey();
                vid.title = lines[1].ToString();
                videos.Enqueue(vid);
            }


            //создаем и запускаем 3 потока
            for (int i = 0; i < 10; i++)
                (new Thread(new ThreadStart(Download))).Start();
            //ожидаем нажатия Enter
            //Console.ReadLine();


            Console.WriteLine(videos.Count);
            //Console.ReadKey();
        }

        public static void Download()
        {
            //будем крутить цикл, пока не закончатся ULR в очереди
            while (true)
            {
                odinvideo URL;
                //блокируем очередь URL и достаем оттуда один адрес
                lock (URLlocker)
                {
                    if (videos.Count == 0)
                        break;//адресов больше нет, выходим из метода, завершаем поток
                    else
                    {
                        Console.WriteLine("************");
                        Console.WriteLine("****  " + videos.Count);
                        Console.WriteLine("************");
                        URL = videos.Dequeue();
                    }
                }
                Console.WriteLine(URL.idn + " - " + URL.title + " - start downloading ...");
                //скачиваем страницу
                //Console.WriteLine(URL.idn);
                getURldownloadvid(URL.idn.ToString(), URL.title);
                
                //блокируем список скачанных страниц, и заносим туда свою страницу
                //lock (HTMLlocker)
                    //HTMLs.Add(HTML);
                //
                //Console.WriteLine(URL + " - downloaded (" + HTML.Length+" bytes)");
            }
        }
        public static string getPlayerLink(int idn)
        {
            string url = "";
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(@"http://vimeo.com/" + idn + @"/");
            myRequest.Method = "GET";
            myRequest.UserAgent = "MSIE 6.0";
            WebResponse myResponse = myRequest.GetResponse();
            StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            string HTML = sr.ReadToEnd();
            //System.IO.File.WriteAllText(@"WriteText.txt", HTML);
            Regex newReg = new Regex("data-config-url=\"(?<val>.*?)\" data-fallback-url");
            MatchCollection matches = newReg.Matches(HTML);
            if (matches.Count > 0)
            {
                //Console.WriteLine("++++++++++++++++");
                foreach (Match mat in matches)
                {
                    url = mat.Groups["val"].Value;
                    break;
                }
            }
            //Console.WriteLine("****"+url);
            //Console.ReadKey();
            return url.Replace("&amp;", "&");
        }
        public static String code(string Url)
        {
            int count = 0;
            lab33:
            try
            {
                count++;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(Url);
                myRequest.Method = "GET";
                WebResponse myResponse = myRequest.GetResponse();
                StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
                string result = sr.ReadToEnd();
                sr.Close();
                myResponse.Close();
                return result;
            }
            catch(Exception ex)
            {
                if(count<3)
                {
                    Console.WriteLine("Ожидание 5 сек " + ex.Message + "(((((" + Url);
                    Thread.Sleep(25000);
                    goto lab33;
                }
                
            }

            return null;
        }
        public static string getURldownloadvid(string idn, string title)
        {
            string HTML="";
            int counter = 0;
            lock (HTMLlocker)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "video\\upload.csv", true))
                {
                    file.WriteLine(title + "05452" + title + "05452" + title + "05452" + "Comedy" + "05452" + "TRUE" + "05452" + AppDomain.CurrentDomain.BaseDirectory + "video\\" + title.Replace(@"\", "").Replace(@"/", "").Replace(@":", "").Replace(@"*", "").Replace(@"?", "").Replace("\"", "").Replace(@"<", "").Replace(@">", "").Replace(@"|", "").Replace(@".", "") + ".mp4");
                }
            }
            string url="";
            string jsonUrl = "";
            lab1:
            try
            {

                jsonUrl = getPlayerLink(Convert.ToInt32(idn)); Console.WriteLine("xxxxxxx");

            }catch(Exception ex)
            {
                counter++;
                Console.WriteLine("--1" + idn + " " + ex.Message);
                if (ex.Message.IndexOf("404") > -1) { return ""; }

                lock (errorlocker)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"errors.txt", true))
                    {
                        //file.WriteLine("--3" + ex.Message + "::" + remoteFilename);
                        file.WriteLine("--1 " + idn + ";;;;;" + title);

                    }
                }

                if (counter < 3)
                {
                    Console.WriteLine("Ожидание 5 с");
                    Thread.Sleep(25000);
                    goto lab1;
                }
                else
                {
                    

                }
            }
            osh403:
            if (jsonUrl.Length > 0)
            {
                HTML = code(jsonUrl);
            }

            //mobile hd sd
            Regex newReg = new Regex("sd\":{\"profile\":(.*?),\"origin\":\"ns(.*?).(.*?)\",\"url\":\"(?<val>.*?)\",\"height");
            MatchCollection matches = newReg.Matches(HTML);
            if (matches.Count > 0)
            {
                Console.WriteLine(@"http://player.vimeo.com/v2/video/" + idn + @"/" + " - downloaded (" + HTML.Length + " bytes)");
                foreach (Match mat in matches)
                {
                    //
                    Console.WriteLine(mat.Groups["val"].Value);
                    //WebClient webClient = new WebClient();
                    try
                    {

                        string filename = title.Replace(@"\", "").Replace(@"/", "").Replace(@":", "").Replace(@"*", "").Replace(@"?", "").Replace("\"", "").Replace(@"<", "").Replace(@">", "").Replace(@"|", "").Replace(@".", "");
                        
                        //webClient.DownloadFile(mat.Groups["val"].Value, AppDomain.CurrentDomain.BaseDirectory + "video\\" + title + ".mp4");
                        int bitt = DownloadFile(mat.Groups["val"].Value, AppDomain.CurrentDomain.BaseDirectory + "video\\" + filename + ".mp4", idn, title);
                        if (bitt == 403) { goto osh403; }
                        if (bitt == 404) { return ""; }
                        Console.WriteLine("Скачан клип {0} bytes written", bitt);
                        
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("--2" + "EXCEPTION" + ex.Message + "--" + mat.Groups["val"].Value);
                        lock (errorlocker)
                        {
                            using (System.IO.StreamWriter file = new StreamWriter("errors.txt", true, Encoding.Default))
                            {
                                file.WriteLine("--2 " + ex.Message + "--" + mat.Groups["val"].Value);
                            }
                        }
                        //Console.ReadKey();

                    }
                    break;
                }
                //Console.ReadKey();
            }
            else
            {
                Console.WriteLine("EROR");
                Console.WriteLine("http://player.vimeo.com/v2/video/" + idn + @"/" + "------");
                //Console.ReadKey();
            }

            return url;
        }
        public static int Download(String remoteFilename,
                               String localFilename)
        {
            // Function will return the number of bytes processed
            // to the caller. Initialize to 0 here.
            int bytesProcessed = 0;

            // Assign values to these objects here so that they can
            // be referenced in the finally block
            Stream remoteStream = null;
            Stream localStream = null;
            WebResponse response = null;

            // Use a try/catch/finally block as both the WebRequest and Stream
            // classes throw exceptions upon error
            
                // Create a request for the specified remote file name
                WebRequest request = WebRequest.Create(remoteFilename);
                if (request != null)
                {
                    // Send the request to the server and retrieve the
                    // WebResponse object 
                    response = request.GetResponse();
                    if (response != null)
                    {
                        // Once the WebResponse object has been retrieved,
                        // get the stream object associated with the response's data
                        remoteStream = response.GetResponseStream();

                        // Create the local file
                        localStream = File.Create(localFilename);

                        // Allocate a 1k buffer
                        byte[] buffer = new byte[1024];
                        int bytesRead;

                        // Simple do/while loop to read from stream until
                        // no bytes are returned
                        do
                        {
                            // Read data (up to 1k) from the stream
                            bytesRead = remoteStream.Read(buffer, 0, buffer.Length);

                            // Write the data to the local file
                            localStream.Write(buffer, 0, bytesRead);

                            // Increment total bytes processed
                            bytesProcessed += bytesRead;
                        } while (bytesRead > 0);
                    }
               
            }
            
            // Return total bytes processed to caller.
            return bytesProcessed;
        } 
        public static int DownloadFile(String remoteFilename,
                               String localFilename, string idn, string title)
        {
            int count = 0;
            lab2:
            try
            {
                /*using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(remoteFilename, localFilename);
                }*/
                Download(remoteFilename, localFilename);
            }
            catch(Exception ex)
            {
                if (ex.Message.IndexOf("403") > -1) { return 403; }
                
                count++;
                Console.WriteLine("--3" + idn + " " + ex.Message + "::" + remoteFilename);

                lock (errorlocker)
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"errors.txt", true))
                    {
                        //file.WriteLine("--3" + ex.Message + "::" + remoteFilename);
                        file.WriteLine("--3 " + idn + ";;;;;" + title + ";;;;;" + ex.Message);

                    }
                }
                
                if (count < 3)
                {
                    Console.WriteLine("Ожидание 5 с");
                    Thread.Sleep(25000);
                    goto lab2;
                }
                else
                {
                    
                }
            }
            return 0;
            
        }
    }
}
