using System.Net.Http.Headers;
using System.Text.Json;

namespace ApiReconocimientoVoz.Services
{
    public class GenderService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GenderService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        public async Task<string?> DetectGender(IFormFile file)
        {
            string? apiKey = _configuration["GenderRecognition:ApiKey"];
            string? url = _configuration["GenderRecognition:Endpoint"];

            using MultipartFormDataContent content = new MultipartFormDataContent();
            using Stream stream = file.OpenReadStream();
            StreamContent streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
            content.Add(streamContent, "file", file.FileName);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = content;
            request.Headers.Add("apiKey", apiKey);

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            string resultJson = await response.Content.ReadAsStringAsync();

            var jsonDoc = JsonDocument.Parse(resultJson);
            return jsonDoc.RootElement.GetProperty("gender").GetString();
        }
    }
}
