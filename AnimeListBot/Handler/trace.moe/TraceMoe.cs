using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Drawing;
using AnimeListBot.Handler.trace.moe.Objects;
using Newtonsoft.Json;

namespace AnimeListBot.Handler.trace.moe
{
    public class TraceMoe
    {
        public const string API_LINK = "https://trace.moe/api/";

        public static async Task<(bool, string, ITraceResult)> Search(Uri link)
        {
            byte[] buffer = new byte[1024];
            byte[] imgData;

            WebRequest req = WebRequest.Create(link);
            WebResponse imgResponse = await req.GetResponseAsync();
            using (Stream imgStream = imgResponse.GetResponseStream())
            {
                using (MemoryStream memStream = new MemoryStream())
                {
                    while (true)
                    {
                        int bytesRead = imgStream.Read(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                        {
                            break;
                        }
                        else
                        {
                            memStream.Write(buffer, 0, bytesRead);
                        }
                    }
                    imgData = memStream.ToArray();

                    imgStream.Close();
                    memStream.Close();
                }
            }

            if(imgData.Length <= 0)
            {
                return (false, "Failed to load image.", null);
            }


            string base64image = Convert.ToBase64String(imgData);

            HttpClient httpClient = Program.httpClient;
            var values = new Dictionary<string, string>
            {
                { "image", base64image }
            };

            var stringPayload = JsonConvert.SerializeObject(values);
            using (var content = new StringContent(stringPayload, Encoding.UTF8, "application/json"))
            {
                var response = await httpClient.PostAsync(API_LINK + "search", content);
                var responseString = await response.Content.ReadAsStringAsync();

                try
                {
                    TraceResult result = JsonConvert.DeserializeObject<TraceResult>(responseString);
                    if (result != null)
                    {
                        return (true, responseString, result);
                    }
                    else
                    {
                        return (false, "Failed to load.", null);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(responseString);
                    await Program._logger.LogError(e);
                    return (false, e.Message, null);
                }
            }
        }
    }
}
