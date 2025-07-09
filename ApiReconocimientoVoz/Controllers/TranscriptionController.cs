using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class TranscriptionController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public TranscriptionController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();

        string? apiKey = _configuration["AssemblyAI:ApiKey"];
        _httpClient.DefaultRequestHeaders.Add("authorization", apiKey);
    }

    [HttpPost]
    public async Task<IActionResult> TranscribeAudio([FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "Archivo no válido" });

            // Obtener el endpoint base desde la config
            string? baseUrl = _configuration["AssemblyAI:BaseUrl"];

            // 1. Subir archivo a AssemblyAI
            using var fileStream = file.OpenReadStream();
            var content = new StreamContent(fileStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var uploadResponse = await _httpClient.PostAsync($"{baseUrl}/upload", content);
            if (!uploadResponse.IsSuccessStatusCode)
                return StatusCode(500, new { error = "Error al subir audio a AssemblyAI" });

            var uploadJson = await uploadResponse.Content.ReadFromJsonAsync<JsonDocument>();
            var audioUrl = uploadJson?.RootElement.GetProperty("upload_url").GetString();

            if (string.IsNullOrEmpty(audioUrl))
                return StatusCode(500, new { error = "URL de audio inválida" });

            // 2. Enviar para transcripción
            var requestData = new
            {
                audio_url = audioUrl,
                speech_model = "universal",
                language_code = "es"
            };

            var json = JsonSerializer.Serialize(requestData);
            var jsonContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var transcriptResponse = await _httpClient.PostAsync($"{baseUrl}/transcript", jsonContent);
            var transcriptJson = await transcriptResponse.Content.ReadFromJsonAsync<JsonDocument>();
            var transcriptId = transcriptJson?.RootElement.GetProperty("id").GetString();

            // 3. Hacer polling
            string pollingUrl = $"{baseUrl}/transcript/{transcriptId}";
            while (true)
            {
                var pollingResponse = await _httpClient.GetAsync(pollingUrl);
                var pollingResult = await pollingResponse.Content.ReadFromJsonAsync<JsonDocument>();
                var status = pollingResult?.RootElement.GetProperty("status").GetString();

                if (status == "completed")
                {
                    var text = pollingResult?.RootElement.GetProperty("text").GetString();
                    return Ok(new { text });
                }
                else if (status == "error")
                {
                    var error = pollingResult?.RootElement.GetProperty("error").GetString();
                    return StatusCode(500, new { error });
                }

                await Task.Delay(3000);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
