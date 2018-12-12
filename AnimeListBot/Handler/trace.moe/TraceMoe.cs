using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using AnimeListBot.Handler.trace.moe.Objects;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace AnimeListBot.Handler.trace.moe
{
    public class TraceMoe
    {
        public const string API_LINK = "https://trace.moe/api/";

        public static async Task<ITraceResult> Search(Uri link)
        {
            byte[] buffer = new byte[1024];
            byte[] imgData;

            try
            {
                WebRequest req = WebRequest.Create(link);
                WebResponse imgResponse = await req.GetResponseAsync();

                if (!imgResponse.ContentType.StartsWith("image/"))
                {
                    return new TraceResult()
                    {
                        failed = true,
                        errorMessage = "File/Link is not an image."
                    };
                }

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

                if (imgData.Length <= 0)
                {
                    return new TraceResult()
                    {
                        failed = true,
                        errorMessage = "Failed to load data."
                    };
                }
            }catch(Exception e)
            {
                return new TraceResult()
                {
                    failed = true,
                    errorMessage = e.Message
                };
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

                string errorMessage = Regex.Match(responseString, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                string errorDescription = string.Empty;

                if (errorMessage == string.Empty && response.StatusCode != HttpStatusCode.OK)
                {
                    errorMessage = response.ReasonPhrase;
                    errorDescription = responseString;
                }

                if(errorMessage != string.Empty)
                {
                    if (response.StatusCode == HttpStatusCode.RequestEntityTooLarge)
                    {
                        errorDescription = "Please use an image that is less than 1MB"; 
                    }

                    return new TraceResult()
                    {
                        failed = true,
                        errorMessage = errorMessage,
                        errorDescription = errorDescription
                    };
                }

                try
                {
                    TraceImage result = JsonConvert.DeserializeObject<TraceImage>(responseString);
                    if (result != null)
                    {
                        return new TraceResult()
                        {
                            trace = result
                        };
                    }
                    else
                    {
                        return new TraceResult()
                        {
                            failed = true,
                            errorMessage = errorMessage != string.Empty ? errorMessage : "Failed to load trace data."
                        };
                    }
                }
                catch(Exception e)
                {
                    await Program._logger.LogError(e);
                    return new TraceResult()
                    {
                        failed = true,
                        errorMessage = e.Message
                    };
                }
            }
        }
    }
}
