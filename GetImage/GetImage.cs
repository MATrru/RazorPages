using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using Azure.Storage.Blobs;

namespace GetImage
{
    public static class GetImage
    {
        [FunctionName("GetImage")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            BlobContainerClient blobContainerClient = new BlobContainerClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "sample-container");
            blobContainerClient.CreateIfNotExists();

            MemoryStream ms = new MemoryStream();
            req.Body.CopyToAsync(ms);            
            string text = req.Query["text"];

            {
                using (var img = Image.FromStream(ms))
                {
                    using (var graphic = Graphics.FromImage(img))
                    {

                        var font = new Font(FontFamily.GenericSansSerif, 38, FontStyle.Bold, GraphicsUnit.Pixel);
                        var color = Color.FromArgb(128, 255, 255, 255);
                        var brush = new SolidBrush(color);
                        var point = new Point(0, (int)(img.Height / 2));
                        //graphic.DrawString(text, font, brush, point);
                        graphic.DrawString(text, font, brush, new RectangleF(point, new SizeF(img.Width, img.Height)));
                        img.Save(ms, ImageFormat.Jpeg);

                        ms.Position = 0;
                        string fileName = Guid.NewGuid().ToString() + "_" + "image.jpg";
                        await blobContainerClient.UploadBlobAsync(fileName, new BinaryData(msToByte(ms)));

                        return new FileContentResult(imageToByteArray(img), "image/jpeg");
                    }
                }
            }

            return new BadRequestResult();
        }

        public static byte[] msToByte(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private static byte[] imageToByteArray(Image image)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }
    }
}


