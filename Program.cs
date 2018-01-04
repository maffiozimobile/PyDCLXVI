using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks;

namespace GitConsole
{
    // Класс переопределяет метод WebClient для изменения параметра Timeout запроса
    class WebClientWithShortTimeout : WebClient
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest request = base.GetWebRequest(uri);
            request.Timeout =  1000; // в миллисекундах
            return request;
        }
    }

    public class GitExecute
    {
        public static void MainClass(string filename)
        {
            string filedata = null;
            try
            {
                filedata = File.ReadAllText(filename);
            }
            catch (FileNotFoundException ex)
            {
                //Console.WriteLine(ex.GetType().FullName);
                Console.WriteLine(ex.Message);
                return;
            }
            
                var temparr = new List<string>();
                var pattern = @"([\w]+\.[a-z]{2,6})";
                foreach (Match m in Regex.Matches(filedata, pattern))
                {
                    temparr.Add("https://" + m.Value);
                    temparr.Add("http://" + m.Value);
                }
                var distinctUrls = new List<string>(temparr.Distinct());
                temparr.Clear();
                Console.WriteLine("Всего объектов " + distinctUrls.Count());
                Parallel.ForEach(distinctUrls, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 20 }, CheckUrl);
                distinctUrls.Clear();
        }

        private static void CheckUrl(string url)
        {
            var uri = new Uri(url + "/.git/HEAD");
            const string pattern = @"([a-z]{3}[:]\s[r][e][f][s]/)";
            try
            {
                var html = new WebClientWithShortTimeout();
                string data = html.DownloadString(uri);
                foreach (Match m in Regex.Matches(data, pattern))
                {
                    try
                    {
                        //File.WriteAllText("result.txt", url + "/.git/");
                        File.AppendAllText("result.txt", url + "/.git/\n");
                    } catch (IOException ex)
                    {
                        //Console.WriteLine(ex.GetType().FullName);
                        Console.WriteLine(ex.Message);
                        return;
                    }
                    Console.WriteLine(url + " Уязвимый");
                }
            }
            catch (InvalidOperationException ex)
            {
                //Console.WriteLine(ex.GetType().FullName);
                //Console.WriteLine(url + " " + ex.Message);
            }
        }
    }
    class Program
    {
        public static void Main(string[] args)
        {
            GitExecute.MainClass("test1.txt");

            Console.WriteLine("Работа завершена.");
            Console.ReadKey();

        }
    }
}
