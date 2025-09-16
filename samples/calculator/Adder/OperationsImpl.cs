using System;
using ComputerWorld.wit.imports.wasi.http.v0_2_0; // May be generated if wasi:http present; harmless if unused

namespace ComputerWorld.wit.exports.example.calculator;

// Implementation of operations interface generated from WIT
public class OperationsImpl : IOperations
{
    public static int Add(int left, int right) => left + right;

    public static string ToUpper(string input) => input.ToUpperInvariant();

    // Real implementation using WASI HTTP client
    public static string GetPrivateClouds(string subscription)
    {
        if (string.IsNullOrWhiteSpace(subscription)) return "{\"error\":\"missing-subscription\"}";
        string? token = Environment.GetEnvironmentVariable("AZURE_TOKEN");
        if (string.IsNullOrWhiteSpace(token)) return "{\"error\":\"missing-token\"}";
        try
        {
            var url = $"https://management.azure.com/subscriptions/{subscription}/providers/Microsoft.AVS/privateClouds?api-version=2024-09-01";
            var headers = new ITypes.Fields();
            headers.Set("authorization", new System.Collections.Generic.List<byte[]>(new[] { System.Text.Encoding.UTF8.GetBytes($"Bearer {token}") }));
            headers.Set("accept", new System.Collections.Generic.List<byte[]>(new[] { System.Text.Encoding.UTF8.GetBytes("application/json") }));

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
                            return $"{{\"error\":\"http-transport\",\"code\":\"{err.Tag}\"}}";
                        }
                    }
                    break;
                }
                System.Threading.Thread.Sleep(10);
            }

            if (incoming == null)
            {
                return "{\"error\":\"timeout\"}";
            }

            int status = incoming.Status();
            string body = string.Empty;
            try
            {
                var bodyRes = incoming.Consume();
                var stream = bodyRes.Stream();
                var bytes = new System.Collections.Generic.List<byte>();
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

            return body;
        }
        catch (Exception ex)
        {
            return $"{{\"error\":\"exception\",\"message\":\"{Escape(ex.Message)}\"}}";
        }
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
