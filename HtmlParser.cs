using System;
using System.Net;
using System.IO;
using System.Text;

namespace ParseHtml
{
    public class HtmlParser
    {
        public string Url {get; private set;} = "https://html.com/resources/free-html-templates/";
        public string PathToWrite {get; private set;}

        public string DefaultPath {get; private set;} = String.Format("{0}\\index.html", System.Environment.CurrentDirectory);

        public HtmlParser(string url, string pathToWrite)
        {
            this.Url = url;
            this.PathToWrite = pathToWrite;
        }

        public HtmlParser(string url)
        {
            this.Url = url;
            this.PathToWrite = this.DefaultPath;
        }

        public HtmlParser() { this.PathToWrite = this.DefaultPath; }

        static void Main(string[] args)
        {
            HtmlParser parser = null;
            (string url, string path) paths = HtmlParser.GetArgs(args);
            if (paths.url == null) parser = new HtmlParser();
            if (paths.url != null & paths.path == null) parser = new HtmlParser(paths.url);
            if (paths.url != null & paths.path != null) parser = new HtmlParser(paths.url, paths.path);

            parser.Start();
            
            Console.Write("Press any key to exit... ");
            Console.ReadKey();
            
        }

        public void Start()
        {
            Uri uri = new Uri(this.Url);
            Console.WriteLine("Start reading HTML...");

            string data = this.DownloadString(uri);
            if (data.Equals(string.Empty)) throw new ArgumentException("Can not read data from input Url or Html code is Empty!");

            (bool isSuccess, string fileName) resultsOfWrite = this.WriteToFile(data, this.PathToWrite);

            // Пользователь подал невалидный путь - меняем на путь по умолчанию
            if (resultsOfWrite.isSuccess == false & resultsOfWrite.fileName == null)
            {
                this.PathToWrite = DefaultPath;
                Console.WriteLine("Not validate path as argument - changed for default path!");
                resultsOfWrite = this.WriteToFile(data, this.PathToWrite);
            }
            
            // Если такой файл уже существует - меняем имя пока не будет новый
            int count = 1;
            while (resultsOfWrite.isSuccess == false)
            {
                StringBuilder builder = new StringBuilder();
                char separator = Path.DirectorySeparatorChar;
                builder.Append(this.PathToWrite.Substring(0, this.PathToWrite.LastIndexOf(separator) + 1));
                builder.Append("index").Append(count).Append(".html");
                count += 1;

                if (count.Equals(Int32.MaxValue)) 
                    throw new ApplicationException("Algorithm is dead, can not generate name that is not already exists!");

                this.PathToWrite = builder.ToString();
                resultsOfWrite = this.WriteToFile(data, this.PathToWrite);
            }
            
            Console.WriteLine($"Html file [{resultsOfWrite.fileName}] created!");
        }
        private void PrintSomeInfoAboutUrl(Uri uri)
        {
            Console.WriteLine("AbsolutePath=" + uri.AbsolutePath);
            Console.WriteLine("AbsoluteUri=" + uri.AbsoluteUri);
            Console.WriteLine("LocalPath=" + uri.LocalPath);
            Console.WriteLine("PathAndQuery=" + uri.PathAndQuery);
            Console.WriteLine("OriginalString=" + uri.OriginalString);
            Console.WriteLine();
        }  

        public static (string, string) GetArgs(string[] args)
        {
            if (args.Length < 1) return (null, null);
            if (args.Length == 1) return (args[0], null);
            return (args[0], args[1]);
        }

        private string DownloadString(Uri uri)
        {
            string data = string.Empty;
            try
            {
                using (WebClient web = new WebClient())
                {
                    data = web.DownloadString(uri);
                    Console.WriteLine("Reading complete. Start writing to file...");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("--- Error ---\nMessage:\n{0}\nStackTrace:{1}\n", ex.Message, ex.StackTrace));
            }

            return data;
        }

        /// <summary>
        /// Комбинации:
        /// [false, FILE_NAME] - такой файл уже существует;
        /// [false, null] - ошибка в пути к файлу;
        /// [true, FILE_NAME] - файл успешно создан и записан.
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private (bool isSuccess, string fileName) WriteToFile(string data, string pathToWrite)
        {
            string path = pathToWrite;
            try
            {
                if (File.Exists(path)) return (false, path);

                using (StreamWriter w = new StreamWriter(File.Create(path)))
                {
                    w.WriteLine(data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"--- Error!!!---\nMessage:\n{e.Message}\nStarckTrace:\n");
                return (false, null);
            }
            
            return (true, path);
        }
    }
}
