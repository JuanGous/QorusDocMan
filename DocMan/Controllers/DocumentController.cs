using DocMan.Data;
using DocMan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using QorusShareLib.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DocMan.Controllers
{
    public class DocumentController : Controller
    {
        private readonly DocManContext _context;
        private Uri baseAddress = new Uri("https://localhost:44336/api/");
        private HttpClient client;

        public DocumentController(DocManContext context)
        {
            _context = context;
            client = new HttpClient();
            client.BaseAddress = baseAddress;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.DocumentModel.ToListAsync());
        }

        public IActionResult RefreshDocumentList()
        {
            _context.DocumentModel.RemoveRange(_context.DocumentModel.Select(x => x));
            DocManContext.SeedData(_context);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FileName,Size,Category,LastReviewed,FormFile")] DocumentCreateModel documentModel)
        {
            if (ModelState.IsValid)
            {
                documentModel.Id = Guid.NewGuid();
                using (var ms = new MemoryStream())
                {
                    documentModel.FormFile.CopyTo(ms);
                    documentModel.Content = ms.ToArray();
                }
                documentModel.ContentType = documentModel.FormFile.ContentType;

                _context.Add(documentModel);

                UploadDocument(documentModel);

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(documentModel);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var documentModel = await _context.DocumentModel.FindAsync(id);
            if (documentModel == null)
            {
                return NotFound();
            }

            DocumentCreateModel dcm = JsonConvert.DeserializeObject<DocumentCreateModel>(JsonConvert.SerializeObject(documentModel));
            return View(dcm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,FileName,Size,Category,LastReviewed,FormFile")] DocumentCreateModel documentModel)
        {
            if (id != documentModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (documentModel.FormFile != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            documentModel.FormFile.CopyTo(ms);
                            documentModel.Content = ms.ToArray();
                        }
                        documentModel.ContentType = documentModel.FormFile.ContentType;
                    }

                    _context.Update(documentModel);

                    UploadDocument(documentModel);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocumentModelExists(documentModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(documentModel);
        }

        public IActionResult DownloadDocument(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var documentModel = _context.DocumentModel.FindAsync(id).Result;

            HttpResponseMessage response = client.GetAsync(baseAddress + "AzureDoc/Download/" + documentModel.Id.ToString()).Result;

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
            }

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
                            DocumentModel doc = serializer.Deserialize<DocumentModel>(jsonReader);

                            return File(doc.Content, doc.ContentType, doc.FileName);
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
                ModelState.AddModelError(string.Empty, "HTTP Response was invalid and cannot be deserialised.");
            }

            return Ok();
        }

        private bool DocumentModelExists(Guid id)
        {
            return _context.DocumentModel.Any(e => e.Id == id);
        }

        private void UploadDocument(DocumentModel documentModel)
        {
            StringContent content = new StringContent(JsonConvert.SerializeObject(documentModel), Encoding.UTF8, "application/json");

            HttpResponseMessage response = client.PostAsync(baseAddress + "AzureDoc/Upload", content).Result;
            if (!response.IsSuccessStatusCode) //web api sent error response
            {
                ModelState.AddModelError(string.Empty, "Server error. Please contact administrator.");
            }
        }
    }
}