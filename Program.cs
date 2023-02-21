using Microsoft.AspNetCore.Builder;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace API
{
    public class Program
    {
        private static string api_url = "http://localhost:8009";

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            app.MapGet("/br-small", async (HttpContext context) =>
            {
                await GET_BROTLI_FILE(context, CompressionLevel.SmallestSize);
            });

            app.MapGet("/br-fast", async (HttpContext context) =>
            {
                await GET_BROTLI_FILE(context, CompressionLevel.Fastest);
            });

            app.MapGet("/", () => {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<html>");
                sb.AppendLine("<body>");
                sb.AppendLine("<script type=\"module\">");
                sb.AppendLine("{");
                sb.AppendLine($"let request = await fetch(\"{api_url}/br-fast\");");
                sb.AppendLine("let response = await request.text();");
                sb.AppendLine("}");
                sb.AppendLine("{");
                sb.AppendLine($"let request = await fetch(\"{api_url}/br-small\");");
                sb.AppendLine("let response = await request.text();");
                sb.AppendLine("}");
                sb.AppendLine("</script>");
                sb.AppendLine("</body>");
                sb.AppendLine("</html>");
                return Results.Text(sb.ToString(), "text/html", Encoding.UTF8);
            });

            await app.RunAsync();
        }

        public static async Task GET_BROTLI_FILE(HttpContext context, CompressionLevel compression_level)
        {
            var bytes = await File.ReadAllBytesAsync("test.txt");

            context.Response.Headers.Add("Content-Encoding", "br");
            context.Response.StatusCode = 200;

            var compression_stream = new BrotliStream(context.Response.Body, compression_level, true);
            await compression_stream.WriteAsync(bytes);
            await compression_stream.FlushAsync();
            await context.Response.Body.FlushAsync();
        }
    }
}
