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
            var url = String.Format("{0}{1}{2}", configuration["AddImageUrl"], "?text=", text);

            if (imageFile != null)
            {
                var byteImg = msToByte(imageFile.OpenReadStream());

                using (var myClient = new HttpClient())
                using (var myRequest = new HttpRequestMessage(HttpMethod.Post, url))
                using (var httpContent = new StreamContent(imageFile.OpenReadStream()))
                {
                    myRequest.Content = httpContent;

                    using (var newResponse = await myClient.SendAsync(myRequest, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                    {
                        Stream stream = newResponse.Content.ReadAsStream();
                        var fileBytes = msToByte(stream);
                        var base64 = Convert.ToBase64String(fileBytes);
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
