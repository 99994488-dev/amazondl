using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AmazonDL.UtilLib
{
    public class Requests
    {
        public static HttpClient Client { get; set; } = new HttpClient();
        public static bool UseCache { get; set; } = false;

        public static HttpResponseMessage Send(HttpRequestMessage request)
        {
            return Client.SendAsync(request).Result;
        }

        public static string Request(string URL, Dictionary<string, string> headers)
        {
            HttpResponseMessage response = Get(URL, headers);
            byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            return Encoding.UTF8.GetString(bytes);
        }
        public static byte[] RequestBinary(string URL, Dictionary<string, string> headers)
        {
            HttpResponseMessage response = Get(URL, headers);
            byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            return bytes;
        }

        public static HttpResponseMessage Get(string URL, Dictionary<string, string> headers)
        {
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(URL),
                Method = HttpMethod.Get
            };

            if (headers != null)
                foreach (KeyValuePair<string, string> header in headers)
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return Send(request);
        }

        public static string Request(string URL, Dictionary<string, string> headers, string postData)
        {
            StringContent content = new StringContent(postData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = Post(URL, headers, content);
            byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Request(string URL, Dictionary<string, string> headers, Dictionary<string, string> postData)
        {
            FormUrlEncodedContent content = new FormUrlEncodedContent(postData);

            HttpResponseMessage response = Post(URL, headers, content);
            byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            return Encoding.UTF8.GetString(bytes);
        }

        public static string Request(string URL, Dictionary<string, string> headers, byte[] postData)
        {
            ByteArrayContent content = new ByteArrayContent(postData);

            HttpResponseMessage response = Post(URL, headers, content);
            byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            return Encoding.UTF8.GetString(bytes);
        }

        public static byte[] RequestBinary(string URL, Dictionary<string, string> headers, string postData)
        {
            StringContent content = new StringContent(postData, Encoding.UTF8, "application/json");

            HttpResponseMessage response = Post(URL, headers, content);
            byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            return bytes;
        }

        public static byte[] RequestBinary(string URL, Dictionary<string, string> headers, byte[] postData)
        {
            ByteArrayContent content = new ByteArrayContent(postData);

            HttpResponseMessage response = Post(URL, headers, content);
            byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            return bytes;
        }

        public static byte[] RequestBinary(string URL, Dictionary<string, string> headers, Dictionary<string, string> postData)
        {
            FormUrlEncodedContent content = new FormUrlEncodedContent(postData);

            HttpResponseMessage response = Post(URL, headers, content);
            byte[] bytes = response.Content.ReadAsByteArrayAsync().Result;
            return bytes;
        }

        static HttpResponseMessage Post(string URL, Dictionary<string, string> headers, HttpContent content)
        {
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(URL),
                Method = HttpMethod.Post,
                Content = content
            };

            if (headers != null)
                foreach (KeyValuePair<string, string> header in headers)
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            return Send(request);
        }

        public static bool Download(string URL, string filename)
        {
            int retries = 0;
            while (retries < 10)
            {
                HttpResponseMessage response = Client.GetAsync(new Uri(URL)).Result;
                if (response.IsSuccessStatusCode)
                {
                    Stream stream = response.Content.ReadAsStreamAsync().Result;
                    FileStream fileStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    stream.CopyTo(fileStream);

                    fileStream.Close();
                    response.Dispose();
                    stream.Close();

                    return true;
                }
                else
                {
                    retries++;
                    response.Dispose();
                    continue;
                }
            }
            return false;
        }

        public static bool DownloadAsync(string URL, string filename, bool progress)
        {
            return DownloadAsync_(URL, filename, progress).Result;
        }

        public static int GetFilesize(string url)
        {
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(url),
                Method = HttpMethod.Head
            };

            return Convert.ToInt32(Math.Round(Client.SendAsync(request).Result.Content.Headers.ContentLength.Value / 1024.0 / 1024.0, 2));
        }

        static long totalRead = 0L;
        static long totalLength = 0L;

        public static void ResetCounter()
        {
            totalRead = 0L;
            totalLength = 0L;
        }

        static async Task<bool> DownloadAsync_(string URL, string filename, bool progress)
        {
            string oldTitle = Console.Title;
            using (HttpResponseMessage response = Client.GetAsync(URL, HttpCompletionOption.ResponseHeadersRead).Result)
            {
                response.EnsureSuccessStatusCode();
                long length = response.Content.Headers.ContentLength.Value;
                totalLength += length;

                using (Stream contentStream = await response.Content.ReadAsStreamAsync(), fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        if (progress)
                            Utils.TitleProgress(totalRead, totalLength);

                        var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await fileStream.WriteAsync(buffer, 0, read);

                            totalRead += read;
                        }
                    }
                    while (isMoreToRead);
                }
            }

            Console.Title = oldTitle;
            return true;
        }

        public static int ResponseCode(string URL, Dictionary<string, string> headers)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(URL),
                Method = HttpMethod.Get
            };

            if (headers != null)
                foreach (KeyValuePair<string, string> header in headers)
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            using HttpClient httpClient = new HttpClient(httpClientHandler);
            HttpResponseMessage response = httpClient.SendAsync(request).Result;

            return (int)response.StatusCode;
        }

        public static (string, string) ResponseDetails(string URL)
        {
            HttpResponseMessage response = Client.GetAsync(new Uri(URL)).Result;

            string authority = response.RequestMessage.RequestUri.Authority;
            string query = response.RequestMessage.RequestUri.Query;
            return (authority, query);
        }

        public static string ParseNetscapeCookies(string cookies)
        {
            string parsedCookies = null;
            foreach (string line in cookies.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
            {
                if (line.StartsWith("#"))
                    continue;

                try
                {
                    parsedCookies += $"{line.Split("	")[5]}={line.Split("	")[6]}; ";
                }
                catch
                {
                    continue;
                }
            }
            return parsedCookies;
        }

        public static JObject RequestJson(Dictionary<string, dynamic> requestData)
        {
            return JObject.Parse(RequestJsonInternal(requestData));
        }

        public static JArray RequestJsonArray(Dictionary<string, dynamic> requestData)
        {
            return JArray.Parse(RequestJsonInternal(requestData));
        }

        static string RequestJsonInternal(Dictionary<string, dynamic> requestData)
        {
            string response;
            if (requestData.ContainsKey("data") && !requestData.ContainsKey("headers"))
                response = Request(requestData["url"], null);
            else if (!requestData.ContainsKey("headers"))
                response = Request(requestData["url"], null);
            else if (!requestData.ContainsKey("data"))
                response = Request(requestData["url"], requestData["headers"]);
            else if (requestData["data"].GetType().Name == "String")
                response = Request(requestData["url"], requestData["headers"], (string)requestData["data"]);
            else if (requestData["data"].GetType().Name.Contains("Dictionary"))
                response = Request(requestData["url"], requestData["headers"], (Dictionary<string, string>)requestData["data"]);
            else
                response = Request(requestData["url"], requestData["headers"], (byte[])requestData["data"]);

            return response;
        }
    }
}
