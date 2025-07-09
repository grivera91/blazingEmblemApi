using ApiReconocimientoVoz.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class GenderDetectorController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GenderDetectorController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> DetectGender([FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("Archivo no válido.");

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
            string result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode, $"Error en el servicio de género: {result}");
            }

            var genderResponse = JsonSerializer.Deserialize<GenderResponse>(result);
            return Ok(genderResponse);
        }
        catch (Exception)
        {
            throw;
        }
    }
}