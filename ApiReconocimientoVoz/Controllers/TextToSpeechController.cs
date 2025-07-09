using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class TextToSpeechController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public TextToSpeechController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Generate([FromBody] TextRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { error = "Texto vacío o inválido." });

        try
        {
            string? apiKey = _configuration["TTS:ApiKey"];
            string? ttsUrl = _configuration["TTS:Endpoint"];

            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(ttsUrl))
                return StatusCode(500, new { error = "No se encontró configuración de TTS en appsettings." });

            var ttsPayload = new
            {
                input = new { text = request.Text },
                voice = new
                {
                    languageCode = request.Voice ?? "es-ES",
                    name = (request.Voice == "es-ES" ? "es-ES-Standard-A" : request.Voice),
                },
                audioConfig = new { audioEncoding = "LINEAR16" } // LINEAR16 → WAV
            };

            var json = JsonSerializer.Serialize(ttsPayload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var ttsRequest = new HttpRequestMessage(HttpMethod.Post, $"{ttsUrl}?key={apiKey}");
            ttsRequest.Content = content;

            var response = await _httpClient.SendAsync(ttsRequest);
            if (!response.IsSuccessStatusCode)
                return StatusCode(500, new { error = "Error al comunicarse con el servicio TTS" });

            var responseJson = await response.Content.ReadAsStringAsync();
            var responseData = JsonDocument.Parse(responseJson);

            var audioContent = responseData.RootElement.GetProperty("audioContent").GetString();
            if (string.IsNullOrEmpty(audioContent))
                return StatusCode(500, new { error = "Respuesta inválida del servicio TTS" });

            var audioBytes = Convert.FromBase64String(audioContent);
            return File(audioBytes, "audio/wav");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    public class TextRequest
    {
        public string? Text { get; set; }
        public string? Voice { get; set; }
    }
}
