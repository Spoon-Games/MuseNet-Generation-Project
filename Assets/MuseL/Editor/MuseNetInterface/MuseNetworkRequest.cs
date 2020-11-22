using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MuseL
{
    public static class MuseNetworkRequest
    {
        public static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        public static readonly HttpClient client;

        public const int TIME_OUT = 5 * 60;

        static MuseNetworkRequest()
        { 
            client = GetClient();
        }

        public static HttpClient GetClient() 
        {
            HttpClient client =  new HttpClient(new HttpClientHandler() { Proxy = GetProxy() })
            {
                Timeout = TimeSpan.FromSeconds(TIME_OUT)
            };
            client.DefaultRequestHeaders.ConnectionClose = true;
            return client;
        }

        private static IWebProxy GetProxy()
        {
            HttpWebRequest myWebRequest = (HttpWebRequest)WebRequest.Create("http://www.microsoft.com");

            // Obtain the 'Proxy' of the  Default browser.
            IWebProxy proxy = myWebRequest.Proxy;
            // Print the Proxy Url to the console.
            return proxy;
        }

        public static async Task<string[]> MakeMuseRequest(string[] encoding, string genre, Instruments instruments, int temperature, int trunication, CancellationToken cancellation)
        {
            await semaphore.WaitAsync(cancellation);
            if (cancellation.IsCancellationRequested)
                return null;

            MuseInput museInput = new MuseInput(genre, encoding, instruments, temperature, trunication);
            string request = JsonUtility.ToJson(museInput, true);
            //string request = "{\"genre\":\"video\",\"instrument\":{ \"piano\":true,\"strings\":true,\"winds\":true,\"drums\":true,\"harp\":false,\"guitar\":true,\"bass\":true},\"encoding\":\"\",\"temperature\":1,\"truncation\":27,\"generationLength\":225,\"audioFormat\":\"\"}";
            Task<string> requestTask = MakeHTTPRequest(request, cancellation);

            string response = "";

            if (await Task.WhenAny(requestTask, Task.Delay((5 * 60 +10) * 1000, cancellation)) == requestTask)
            {
                response = requestTask.Result;
            }

            if (string.IsNullOrEmpty(response))
            {
                semaphore.Release();
                return null;
            }else if (Equals(response, "noresponse"))
            {
                semaphore.Release();
                return new string[] { "noresponse" };
            }

            MuseOutput output = JsonUtility.FromJson<MuseOutput>(response);

            semaphore.Release();

            return output.completions[0].encoding.Split(' ');

        }

        private static async Task<string> MakeHTTPRequest(string jsonContent, CancellationToken token)
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                //HttpClient client = await ProxyHandler.GetClient(token);
                //if (client == null)
                  //  return "noresponse";

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                using (HttpResponseMessage response = await client.PostAsync("https://musenet.openai.com/sample", content, token))
                {
                    ;
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(responseBody))
                    {
                        return "noresponse";
                    }
                    // Above three lines can be replaced with new helper method below
                    // string responseBody = await client.GetStringAsync(uri);
                    return responseBody;
                }
                
            }
            catch (Exception e)
            { 
                Debug.Log(e.ToString());
                return "";
            }
        }

        private class MuseOutput
        {
            public Completion[] completions;
        }

        [System.Serializable]
        private class Completion
        {
            public string encoding;
        }

        [System.Serializable]
        private class MuseInput
        {
            public string genre;
            public InstrumentStruct instrument;
            public string encoding;
            public int temperature;
            public int truncation;

            public int generationLength;
            public string audioFormat;

            public MuseInput(string genre, string[] encoding, Instruments instruments, int temperature, int trunication)
            {
                this.genre = genre;
                StringBuilder builder = new StringBuilder();
                foreach (var e in encoding)
                {
                    builder.Append(e);
                    builder.Append(' ');
                }
                this.encoding = builder.ToString();

                instrument = new InstrumentStruct(instruments);
                this.temperature = temperature;
                truncation = trunication;
                generationLength = 255;
                audioFormat = "";
            }
        }

        [System.Serializable]
        public class InstrumentStruct
        {
            public bool piano;
            public bool strings;
            public bool winds;
            public bool drums;
            public bool harp;
            public bool guitar;
            public bool bass;

            public InstrumentStruct(Instruments instruments)
            {
                piano = instruments.HasFlag(Instruments.piano);
                strings = instruments.HasFlag(Instruments.strings);
                winds = instruments.HasFlag(Instruments.winds);
                drums = instruments.HasFlag(Instruments.drums);
                harp = instruments.HasFlag(Instruments.harp);
                guitar = instruments.HasFlag(Instruments.guitar);
                bass = instruments.HasFlag(Instruments.bass);
            }
        }
    }
}
