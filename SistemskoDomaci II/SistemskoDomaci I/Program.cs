using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WebServer
{
    class Program
    {
        static readonly string RootDirectory = Directory.GetCurrentDirectory();
        static HttpListener listener;
        static ConcurrentDictionary<string, byte[]> cache = new ConcurrentDictionary<string, byte[]>(); 

        static async Task Main(string[] args)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:5050/");
            listener.Start();

            Console.WriteLine("Web server pokrenut");

            while (true)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    _ = ProcessRequestAsync(context); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static async Task ProcessRequestAsync(HttpListenerContext context)
        {
            string requestUrl = context.Request.Url.LocalPath;

            if (requestUrl == "/")
            {
                await SendFileListResponseAsync(context);
            }
            else
            {
                await SendFileResponseAsync(context, requestUrl);
            }
        }

        static async Task SendFileListResponseAsync(HttpListenerContext context)
        {
            string[] files = Directory.GetFiles(RootDirectory);
            string responseHtml = "<html><body><h1>Dostupni fajlovi u root direktorijumu:</h1><ul>";

            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                responseHtml += $"<li><a href=\"/{fileName}\">{fileName}</a></li>";
            }

            responseHtml += "</ul></body></html>";

            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseHtml);
            context.Response.ContentType = "text/html";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        static async Task SendFileResponseAsync(HttpListenerContext context, string requestUrl)
        {
            string filePath = Path.Combine(RootDirectory, requestUrl.TrimStart('/'));

            if (File.Exists(filePath))
            {
                byte[] fileBytes = await GetFileFromCacheOrDiskAsync(filePath);

                context.Response.ContentType = "application/octet-stream";
                context.Response.ContentLength64 = fileBytes.Length;
                context.Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(filePath));
                await context.Response.OutputStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                context.Response.OutputStream.Close();
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.OutputStream.Close();
            }
        }

        static async Task<byte[]> GetFileFromCacheOrDiskAsync(string filePath)
        {
            if (cache.TryGetValue(filePath, out byte[] cachedFileBytes))
            {
                 Console.WriteLine($"Pribavljanje {filePath} iz cache-a");
                return cachedFileBytes;
            }
            else
            {
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
                cache[filePath] = fileBytes; 
                Console.WriteLine($"Čita se  {filePath} sa diska i dodaje se u cache");
                return fileBytes;
            }
        }
    }
}