using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;



namespace SimpleCrawler
{
    //һЩע�͵��Ĵ�������֮ǰд���������Ҫ��İ汾������������ʦ�Ĵ�������˸Ľ���
    public class Crawler
    {
        //public Hashtable urls = new Hashtable();
        //private int count = 0;
        //public string startUrl;

        public event Action<Crawler> CrawlerStopped;
        public event Action<Crawler, string, string> PageDownloaded;

        //�����ص�URL��key��URL��value��ʾ�Ƿ����سɹ�
        private Dictionary<string, bool> done = new Dictionary<string, bool>();

        //�����ض���
        private Queue<string> pending = new Queue<string>();

        //URL�����ʽ��������HTML�ı��в���URL
        private readonly string urlDetectRegex = @"(href|HREF)[]*=[]*[""'](?<url>[^""'#>]+)[""']";

        //URL�������ʽ
        public static readonly string urlParseRegex = @"^(?<site>https?://(?<host>[\w.-]+)(:\d+)?($|/))(\w+/)*(?<file>[^#?]*)";

        public string HostFilter { get; set; } //�������˹���
        public string FileFilter { get; set; } //�ļ����˹���
        public int MaxPage { get; set; } //�����������
        public string StartURL { get; set; } //��ʼ��ַ
        public Encoding HtmlEncoding { get; set; } //��ҳ����
        public Dictionary<string, bool> DownloadedPages { get; } //��������ҳ

        public Crawler()
        {
            MaxPage = 100;
            HtmlEncoding = Encoding.UTF8;
        }

        public void Excute()
        {
            //this.startUrl = "http://www.cnblogs.com/dstang2000/";
            ////if (args.Length >= 1) myCrawler.startUrl = args[0];
            //this.urls.Add(this.startUrl, false);//�����ʼҳ��
            //new Thread(this.Crawl).Start();

            done.Clear();
            pending.Clear();
            pending.Enqueue(StartURL);
            Crawl();

        }


        public string DownLoad(string url)
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.Encoding = Encoding.UTF8;
                string html = webClient.DownloadString(url);
                string fileName = done.Count.ToString();
                File.WriteAllText(fileName, html, Encoding.UTF8);
                return html;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "";
            }
        }

        private void Parse(string html, string startUrl)
        {
            //string strRef = @"(href|HREF)[]*=[]*[""'][^""'#>]+[""']";
            MatchCollection matches = new Regex(urlDetectRegex).Matches(html);
            foreach (Match match in matches)
            {
                //strRef = match.Value.Substring(match.Value.IndexOf('=') + 1)
                //          .Trim('"', '\"', '#', '>');
                //if (strRef.Length == 0) continue;
                //if (Regex.IsMatch(strRef, @"^/"))//��Ϊ���Ե�ַ
                //{
                //    strRef = startUrl.Substring(0,startUrl.Length-1) + strRef;
                //}
                //if (urls[strRef] == null) urls[strRef] = false;
                string linkUrl = match.Groups["url"].Value;
                if (linkUrl == null || linkUrl == "") continue;
                linkUrl = FixUrl(linkUrl, startUrl);//ת����·��
                                                    //������host��file�������֣����й���
                Match linkUrlMatch = Regex.Match(linkUrl, urlParseRegex);
                string host = linkUrlMatch.Groups["host"].Value;
                string file = linkUrlMatch.Groups["file"].Value;
                if (Regex.IsMatch(host, HostFilter) && Regex.IsMatch(file, FileFilter)
                  && !done.ContainsKey(linkUrl))
                {
                    pending.Enqueue(linkUrl);
                }

            }
        }

        private void Crawl()
        {

            //��ȡ
            while (done.Count < MaxPage && pending.Count > 0)
            {
                string url = pending.Dequeue();
                try
                {
                    string html = DownLoad(url); // ����
                    done[url] = true;
                    PageDownloaded(this, url, "success");
                    Parse(html, url);//����,�������µ�����
                }
                catch (Exception ex)
                {
                    PageDownloaded(this, url, "  Error:" + ex.Message);
                }
            }
            CrawlerStopped(this);

            //Console.WriteLine("��ʼ������.... ");
            //while (true)
            //{
            //    string current = null;
            //    foreach (string url in urls.Keys)
            //    {
            //        if ((bool)urls[url]) continue;
            //        current = url;
            //    }
            //    if (!Regex.IsMatch(current, @"^(https?://www.cnblogs.com/dstang2000)")) continue;
            //    if (current == null || count > 10) break;
            //    Console.WriteLine("����" + current + "ҳ��!");


            //    string html = DownLoad(current); // ����
            //    urls[current] = true;
            //    count++;
            //    Regex pthtml = new Regex(@"^(<!DOCTYPE\shtml>)");
            //    if (!pthtml.IsMatch(html))
            //    {
            //        Console.WriteLine("���н���");
            //        continue;
            //    }

            //    Parse(html,startUrl);//����,�������µ�����
            //    Console.WriteLine("���н���");

            //}
        }


        //�����·��תΪ����·��
        private string FixUrl(string url, string pageUrl)
        {
            if (url.Contains("://"))
            {
                return url;
            }
            if (url.StartsWith("/"))
            {
                Match urlMatch = Regex.Match(pageUrl, urlParseRegex);
                String site = urlMatch.Groups["site"].Value;
                return site.EndsWith("/") ? site + url.Substring(1) : site + url;
            }

            if (url.StartsWith("../"))
            {
                url = url.Substring(3);
                int idx = pageUrl.LastIndexOf('/');
                return FixUrl(url, pageUrl.Substring(0, idx));
            }

            if (url.StartsWith("./"))
            {
                return FixUrl(url.Substring(2), pageUrl);
            }
            if (url.Contains("://"))
            {
                return url;
            }
            if (url.StartsWith("//"))
            {
                return "http:" + url;
            }

            int end = pageUrl.LastIndexOf("/");
            return pageUrl.Substring(0, end) + "/" + url;
        }
    }
}