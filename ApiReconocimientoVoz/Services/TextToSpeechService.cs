using System.Text.Json;

namespace ApiReconocimientoVoz.Services
{
    public class TextToSpeechService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TextToSpeechService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
        }

        public async Task<byte[]> GenerateAudio(string text, string? voice = "es-ES-Standard-A")
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Texto vacío o inválido.", nameof(text));

            string? apiKey = _configuration["TTS:ApiKey"];
            string? ttsUrl = _configuration["TTS:Endpoint"];

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(ttsUrl))
                throw new Exception("No se encontró configuración de TTS en appsettings.");

            // Construir el payload correcto como el controlador original
            var ttsPayload = new
            {
                input = new { text },
                voice = new
                {
                    languageCode = "es-ES",
                    name = voice ?? "es-ES-Standard-A"
                },
                audioConfig = new
                {
                    audioEncoding = "LINEAR16",
                    pitch = -5.0,
                    speakingRate = 0.9
                }
            };

            var json = JsonSerializer.Serialize(ttsPayload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var ttsRequest = new HttpRequestMessage(HttpMethod.Post, $"{ttsUrl}?key={apiKey}")
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(ttsRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error en el servicio TTS: {response.StatusCode}, Respuesta: {responseContent}");

            var responseData = JsonDocument.Parse(responseContent);
            var audioContent = responseData.RootElement.GetProperty("audioContent").GetString();

            if (string.IsNullOrEmpty(audioContent))
                throw new Exception("Respuesta inválida: audioContent vacío.");

            return Convert.FromBase64String(audioContent);
        }
    }
}
