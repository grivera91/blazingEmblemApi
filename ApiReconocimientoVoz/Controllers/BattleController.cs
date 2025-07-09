using ApiReconocimientoVoz.Services;
using ApiReconocimientoVoz.Utilities;
using ApiReconocimientoVoz.Utilities.ApiReconocimientoVoz.Utilities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BattleController : ControllerBase
{
    private readonly GenderService _genderService;
    private readonly TranscriptionService _transcriptionService;
    private readonly TextToSpeechService _ttsService;

    public BattleController(GenderService genderService, TranscriptionService transcriptionService, TextToSpeechService ttsService)
    {
        _genderService = genderService;
        _transcriptionService = transcriptionService;
        _ttsService = ttsService;
    }

    [HttpPost]
    public async Task<IActionResult> ProcessBattle([FromForm] IFormFile file, [FromForm] string gender)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Archivo inválido");

        if (string.IsNullOrWhiteSpace(gender))
            return BadRequest("Género no especificado");

        try
        {
            // Aquí ya NO llamas a _genderService; usas el género recibido
            string? transcript = await _transcriptionService.Transcribe(file);
            byte[]? ttsAudio = null;

            if (!string.IsNullOrEmpty(transcript))
            {
                string? selectedVoice = gender.ToLower() == "male" ? "es-ES-Standard-B" : "es-ES-Standard-A";
                ttsAudio = await _ttsService.GenerateAudio(transcript, selectedVoice);
            }

            AttackInfo? attackInfo = AttackResolver.GetAttack(transcript, gender);

            return Ok(new
            {
                success = true,
                gender,
                transcript,
                attack_name = attackInfo?.AttackName ?? null,
                attack_animation = attackInfo?.AnimationKey ?? null,
                attack_effect = attackInfo?.HitEffectKey ?? null,
                attack_sound = attackInfo?.HitSoundKey ?? null,
                attack_damage = attackInfo?.Damage ?? 0,
                message = attackInfo == null ? "No se reconoció ningún comando. Intenta nuevamente." : null,
                audioBase64 = ttsAudio != null ? Convert.ToBase64String(ttsAudio) : null,
                cast_animation = attackInfo?.CastAnimationKey ?? null
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }
}