using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace MuseL
{
    public static class ProxyHandler
    {
        private const string REQUEST_PROXY_URL = "http://pubproxy.com/api/proxy?post=true";

        private static ConcurrentBag<string> proxies = new ConcurrentBag<string>();
        private static BlockingCollection<HttpClient> clients = new BlockingCollection<HttpClient>();

        private static int lastRequestedClient = -1;
        private static bool hasAllProxies = false;

        private static readonly HttpClient requestClient = new HttpClient();

        private static object threadLock = new object();

        static ProxyHandler()
        {
            lock (threadLock)
            {
                clients.Add(new HttpClient() { Timeout = TimeSpan.FromSeconds(MuseNetworkRequest.TIME_OUT) });
                AppDomain.CurrentDomain.ProcessExit += Dispose;
            }
        }

        private static void Dispose(object sender, EventArgs e)
        {
            foreach(var c in clients)
            {
                c?.Dispose();
            }

            clients?.Dispose();

            requestClient?.Dispose();
        }

        public static async Task<HttpClient> GetClient(CancellationToken cancellation)
        {
            //clients.Add(new HttpClient() { Timeout = TimeSpan.FromSeconds(MuseNetworkRequest.TIME_OUT) });
            await TryGetProxy(cancellation);

            lock (threadLock)
            {
                Debug.Log("CurrentProxies: " + clients.Count);
                lastRequestedClient = (lastRequestedClient + 1) % clients.Count;
                return clients.FirstOrDefault();
            }
        }

        private static async Task TryGetProxy(CancellationToken cancellation)
        {
            if (hasAllProxies)
                return;

            string response = await TryMakeRequest(cancellation);

            if (string.IsNullOrEmpty(response))
            {
                return;
            }

            ProxyOutput output = JsonUtility.FromJson<ProxyOutput>(response);

            if (output == null || output.data.Length == 0)
            {
                hasAllProxies = true;
                return;
            }

            for (int i = 0; i < output.data.Length; i++)
            {
                if (proxies.Contains(output.data[i].ipPort))
                {
                    if (i == output.data.Length - 1)
                    {
                        hasAllProxies = true;
                        return;
                    }
                }
                else
                {
                    clients.Add(new HttpClient(new HttpClientHandler() { Proxy = new WebProxy(output.data[i].ipPort) })
                    {
                        Timeout = TimeSpan.FromSeconds(MuseNetworkRequest.TIME_OUT)
                    });
                }
            }
        }

        private static async Task<string> TryMakeRequest(CancellationToken cancellation)
        {
            try
            {
                using (var responseMessage = await requestClient.GetAsync(REQUEST_PROXY_URL, cancellation))
                {
                    responseMessage.EnsureSuccessStatusCode();
                    string response = await responseMessage.Content.ReadAsStringAsync();
                    return response;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return null;
            }
        }

        private class ProxyOutput
        {
            public ProxyData[] data;
        }

        [System.Serializable]
        private class ProxyData
        {
            public string ipPort;
        }

    }
}
