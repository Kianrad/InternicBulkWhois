using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace InternicBulkWhois
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var domains = File.ReadAllLines(args[0]);
            var sb = new StringBuilder();            
            foreach (var domain in domains)
            {
                var webRequest =
                    WebRequest.CreateHttp("https://reports.internic.net/cgi/whois?whois_nic=" + domain +
                                          "&type=domain");
                webRequest.Headers.Add("Host", "reports.internic.net");
                webRequest.Headers.Add("Referer", "https://www.internic.net/");
                webRequest.Headers.Add("User-Agent",
                    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36");
                webRequest.AllowAutoRedirect = false;

                try
                {
                    using (var stream = webRequest.GetResponse().GetResponseStream())
                    {
                        var sr = new StreamReader(stream);
                        var res = sr.ReadToEnd();

                        var pattern = @"(Domain Name.*)DNSSEC";

                        var options = RegexOptions.Singleline;

                        foreach (Match m in Regex.Matches(res, pattern, options))
                            try
                            {
                                var lines = m.Groups[1].Value.Split(Environment.NewLine);
                                var reslist = new List<string>();
                                foreach (var line in lines)
                                {
                                    var startindex = line.IndexOf(":") + 2;
                                    var linedata = "";
                                    if (startindex < line.Length && line.Length - line.IndexOf(":") - 2 > 0)
                                    {
                                        linedata = line.Substring(startindex,
                                            line.Length - line.IndexOf(":") - 2);
                                        linedata = linedata.Replace("\r", "");
                                    }

                                    reslist.Add("\"" + linedata + "\"");
                                }

                                sb.AppendLine(string.Join(",", reslist.ToArray()));
                            }
                            catch
                            {
                                sb.AppendLine("Error:" + domain);
                            }


                        stream.Close();
                       
                    }
                }
                catch
                {
                    sb.AppendLine("Error:" + domain);
                }
            }


            File.WriteAllText("result.csv", sb.ToString());
        }
    }
}