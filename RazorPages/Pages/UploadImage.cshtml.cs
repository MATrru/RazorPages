using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace RazorPages.Pages
{
    public class UploadImageModel : PageModel
    {
        private readonly ILogger<UploadImageModel> _logger;
        private readonly IConfiguration configuration;

        public UploadImageModel(ILogger<UploadImageModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }

        public void OnGet()
        {
            ViewData["imgBase64"] = "";
        }

        public async Task<IActionResult> OnPostAsync()
        {

            IFormFile imageFile = Request.Form.Files["Image"];
            string text = Request.Form["text"];
            var url = String.Format("{0}&text={1}", configuration["AddImageUrl"], text);

            if (imageFile != null)
            {
                var ms = new MemoryStream();
                imageFile.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);

                using (var myClient = new HttpClient())
                using (var myRequest = new HttpRequestMessage(HttpMethod.Post, url))
                using (var httpContent = new StreamContent(ms))
                {
                    myRequest.Content = httpContent;

                    using (var newResponse = await myClient.PostAsync(url, httpContent))
                    {
                        var base64 = Convert.ToBase64String(await newResponse.Content.ReadAsByteArrayAsync());
                        ViewData["imgBase64"] = String.Format("data:image/jpeg;base64,{0}", base64);
                        return Page();
                    }
                }
            }
            return Page();
        }

        public static byte[] msToByte(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
