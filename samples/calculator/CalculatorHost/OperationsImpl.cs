using System;
using System.Collections.Generic;
using HostappWorld.wit.imports.example.calculator;

namespace HostappWorld.wit.imports.example.calculator
{
    public class OperationsImpl : IOperations
    {
        public static int Add(int left, int right) => left + right;

        public static string ToUpper(string input) => input.ToUpperInvariant();

        public static string GetPrivateClouds(string subscription, string token)
        {
            // Real HTTP implementation using WASI HTTP client
            try
            {
                var url = $"https://management.azure.com/subscriptions/{subscription}/providers/Microsoft.AVS/privateClouds?api-version=2024-09-01";
                var (status, body) = WasiHttpClient.Get(url, token);
                if (status >= 200 && status < 300)
                    return body;
                return $"{{\"error\":\"http {status}\",\"body\":{Escape(body)} }}";
            }
            catch (Exception ex)
            {
                return $"{{\"error\":\"exception\",\"message\":\"{Escape(ex.Message)}\"}}";
            }
        }

        // Minimal WASI HTTP client based on Adder's implementation
        public static class WasiHttpClient
        {
            public static (int status, string body) Get(string url, string bearerToken)
            {
                // This assumes generated types are available
                var headers = new ITypes.Fields();
                headers.Set("authorization", new List<byte[]>(new[] { System.Text.Encoding.UTF8.GetBytes($"Bearer {bearerToken}") }));
                headers.Set("accept", new List<byte[]>(new[] { System.Text.Encoding.UTF8.GetBytes("application/json") }));

                var req = new ITypes.OutgoingRequest(headers);
                req.SetMethod(ITypes.Method.Get());

                var uri = new Uri(url);
                req.SetScheme(uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? ITypes.Scheme.Https() : ITypes.Scheme.Http());
                req.SetAuthority(uri.Authority);
                req.SetPathWithQuery(uri.PathAndQuery);

                ITypes.RequestOptions? options = null;
                var future = OutgoingHandlerInterop.Handle(req, options);

                ITypes.IncomingResponse? incoming = null;
                for (int i = 0; i < 10; i++)
                {
                    var resultOpt = future.Get();
                    if (resultOpt != null)
                    {
                        var outer = resultOpt.Value;
                        if (outer.IsOk)
                        {
                            var inner = outer.AsOk;
                            if (inner.IsOk)
                            {
                                incoming = inner.AsOk;
                            }
                            else
                            {
                                var err = inner.AsErr;
                                throw new Exception($"HTTP transport error: {err.Tag}");
                            }
                        }
                        break;
                    }
                    System.Threading.Thread.Sleep(10);
                }

                if (incoming == null)
                {
                    throw new TimeoutException("Timed out waiting for HTTP response");
                }

                int status = incoming.Status();
                string body = string.Empty;
                try
                {
                    var bodyRes = incoming.Consume();
                    var stream = bodyRes.Stream();
                    var bytes = new List<byte>();
                    while (true)
                    {
                        var chunk = stream.Read(1024);
                        if (chunk.Length == 0)
                            break;
                        bytes.AddRange(chunk);
                    }
                    body = System.Text.Encoding.UTF8.GetString(bytes.ToArray());
                    stream.Dispose();
                    bodyRes.Dispose();
                }
                finally
                {
                    incoming.Dispose();
                    future.Dispose();
                }

                return (status, body);
            }
        }

        private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
