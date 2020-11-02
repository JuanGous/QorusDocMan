using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QorusShareLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

namespace DocMan.Data
{
    public class DocManContext : DbContext
    {
        public DocManContext(DbContextOptions<DocManContext> options)
            : base(options)
        {
        }

        public DbSet<QorusShareLib.Models.DocumentModel> DocumentModel { get; set; }

        internal static void SeedData(DocManContext context)
        {
            try
            {
                Uri baseAddress = new Uri("https://localhost:44336/api/");
                using (var client = new HttpClient())
                {
                    client.BaseAddress = baseAddress;

                    HttpResponseMessage response = client.GetAsync(baseAddress + "AzureDoc/List").Result;

                    if (response.Content is object && response.Content.Headers.ContentType.MediaType == "application/json")
                    {
                        var contentStream = response.Content.ReadAsStreamAsync().Result;

                        using (var streamReader = new StreamReader(contentStream))
                        {
                            using (var jsonReader = new JsonTextReader(streamReader))
                            {
                                JsonSerializer serializer = new JsonSerializer();
                                try
                                {
                                    var ls = serializer.Deserialize<IEnumerable<DocumentModel>>(jsonReader);

                                    context.DocumentModel.AddRange(ls);
                                }
                                catch (JsonReaderException e)
                                {
                                    throw new Exception("Invalid JSON.", e);
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("HTTP Response was invalid and cannot be deserialised.");
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine("HTTP Request timed out. QorusAzureAPI migh be down.");
            }
        }
    }
}