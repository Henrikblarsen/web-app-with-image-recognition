using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Company.Function
{
    public static class AnalyseImage
    {
        [FunctionName("AnalyseImage")]
        public static async Task<IActionResult>  Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest
            req, ILogger log, System.Threading.ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
         .SetBasePath(context.FunctionAppDirectory)
         .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
         .AddEnvironmentVariables()
         .Build();

        // Add your Computer Vision subscription key and endpoint
        string subscriptionKey = config["ComputerVisionKey"];
        string endpoint = "https://web-app-with-image-recognition-hbla.cognitiveservices.azure.com/";

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);

        string imageUrl = data?.imageUrl;

        // Create a client
        ComputerVisionClient client = Authenticate(endpoint, subscriptionKey);
        var analysisResult = await
        // Analyze an image to get features and other properties.
        AnalyzeImageUrl(client, imageUrl);

            return new OkObjectResult(analysisResult);
        }

        /*AUTHENTICATE - Creates a Computer Vision client used by each example.*/
        public static ComputerVisionClient Authenticate(string endpoint, string key)
        {
            ComputerVisionClient client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(key))
            { Endpoint = endpoint };
            return client;
        }
             
            /* ANALYZE IMAGE - URL IMAGE / Analyze URL image. Extracts captions, categories, tags, objects, faces, racy/adult/gory content,
            * brands, celebrities, landmarks, color scheme, and image types.*/
            
            public static async Task<ImageAnalysis> AnalyzeImageUrl(ComputerVisionClient client, string imageUrl)
            {

                List<VisualFeatureTypes?> features = new List<VisualFeatureTypes?>()
                {
                    VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
                    VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
                    VisualFeatureTypes.Tags, VisualFeatureTypes.Adult,
                    VisualFeatureTypes.Color, VisualFeatureTypes.Brands,
                    VisualFeatureTypes.Objects
                };
                // Analyze the URL image 
                ImageAnalysis results = await client.AnalyzeImageAsync(imageUrl, visualFeatures: features);
                return results;
            }
    }
}
