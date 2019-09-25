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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace AnimeListBot.Handler.trace.moe
{
    public class TraceMoe
    {
        public const string API_LINK = "https://trace.moe/api/";

        public static async Task<ITraceResult> Search(Uri link)
        {
            Image<Rgba32> searchImage;
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
                    searchImage = Image.Load(imgStream);
                    imgStream.Close();
                }
            }catch(Exception e)
            {
                return new TraceResult()
                {
                    failed = true,
                    errorMessage = e.Message
                };
            }

            byte[] imageData = CompressImageToArray(searchImage);
            string base64image = Convert.ToBase64String(imageData);

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

        public static byte[] CompressImageToArray(Image<Rgba32> img)
        {
            byte[] byteArray = new byte[0];
            using (MemoryStream stream = new MemoryStream())
            {
                JpegEncoder encoder = new JpegEncoder();
                encoder.Quality = 75;
                img.Save(stream, encoder);

                byteArray = stream.ToArray();
                Console.WriteLine("Data Length: " + byteArray.Length);

                stream.Close();
            }
            return byteArray;
        }
    }
}
