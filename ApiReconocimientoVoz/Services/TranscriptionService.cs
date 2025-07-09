using System.Net.Http.Headers;
using System.Text.Json;

namespace ApiReconocimientoVoz.Services
{
    public class TranscriptionService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TranscriptionService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();

            string? apiKey = _configuration["AssemblyAI:ApiKey"];
            _httpClient.DefaultRequestHeaders.Add("authorization", apiKey);
        }

        public async Task<string?> Transcribe(IFormFile file)
        {
            string? baseUrl = _configuration["AssemblyAI:BaseUrl"];

            // 1. Subir audio
            using var fileStream = file.OpenReadStream();
            var content = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var uploadResponse = await _httpClient.PostAsync($"{baseUrl}/upload", content);
            if (!uploadResponse.IsSuccessStatusCode)
                throw new Exception("Error al subir audio a AssemblyAI");

            var uploadJson = await uploadResponse.Content.ReadFromJsonAsync<JsonDocument>();
            var audioUrl = uploadJson?.RootElement.GetProperty("upload_url").GetString();

            if (string.IsNullOrEmpty(audioUrl))
                throw new Exception("URL de audio inválida");

            // 2. Crear transcripción
            var requestData = new { audio_url = audioUrl, speech_model = "universal", language_code = "es" };
            var json = JsonSerializer.Serialize(requestData);
            var jsonContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var transcriptResponse = await _httpClient.PostAsync($"{baseUrl}/transcript", jsonContent);
            var transcriptJson = await transcriptResponse.Content.ReadFromJsonAsync<JsonDocument>();
            var transcriptId = transcriptJson?.RootElement.GetProperty("id").GetString();

            // 3. Polling hasta finalizar
            string pollingUrl = $"{baseUrl}/transcript/{transcriptId}";
            while (true)
            {
                var pollingResponse = await _httpClient.GetAsync(pollingUrl);
                var pollingResult = await pollingResponse.Content.ReadFromJsonAsync<JsonDocument>();
                var status = pollingResult?.RootElement.GetProperty("status").GetString();

                if (status == "completed")
                    return pollingResult?.RootElement.GetProperty("text").GetString();
                if (status == "error")
                    throw new Exception(pollingResult?.RootElement.GetProperty("error").GetString());

                await Task.Delay(3000);
            }
        }
    }
}
