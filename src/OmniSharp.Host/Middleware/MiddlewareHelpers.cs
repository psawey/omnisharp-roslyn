using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace OmniSharp.Middleware
{
    public static class MiddlewareHelpers
    {
        private static readonly Encoding _encoding = new System.Text.UTF8Encoding(false);
        private const int _bufferSize = 1024;

        public static void WriteTo(HttpResponse response, object value)
        {
            using (var writer = new StreamWriter(response.Body, _encoding, _bufferSize, true))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                jsonWriter.CloseOutput = false;
                var jsonSerializer = JsonSerializer.Create(/*TODO: SerializerSettings*/);
                jsonSerializer.Serialize(jsonWriter, value);
            }
        }

        public static void WriteToRaw(HttpResponse response, object value)
        {
            using (var writer = new StreamWriter(response.Body, _encoding, _bufferSize, true))
            {
                writer.Write(value);
            }
        }
    }
}