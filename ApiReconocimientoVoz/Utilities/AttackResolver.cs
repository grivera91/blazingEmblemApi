using ApiReconocimientoVoz.Utilities.ApiReconocimientoVoz.Utilities;

namespace ApiReconocimientoVoz.Utilities
{
    public static class AttackResolver
    {
        private static readonly Dictionary<string, (string[] Keywords, AttackInfo Info)> MaleCommands = new()
        {
            { "ataque normal", (new[] { "ataque normal", "golpe normal", "puño normal" }, new AttackInfo {
                AttackName = "Ataque Normal",
                AnimationKey = "hector_normal_attack",
                HitEffectKey = "normal_hit",
                HitSoundKey = "hit_sound_normal",
                Damage = 20,
                CastAnimationKey = "hector_cast"
            })},
            { "ataque fuerte", (new[] { "ataque fuerte", "golpe fuerte", "puño fuerte" }, new AttackInfo {
                AttackName = "Ataque Fuerte",
                AnimationKey = "hector_strong_attack",
                HitEffectKey = "strong_hit",
                HitSoundKey = "hit_sound_strong",
                Damage = 40,
                CastAnimationKey = "hector_cast"
            })},
            { "ataque a distancia", (new[] { "ataque a distancia", "ataque lejano", "ataque volador" }, new AttackInfo {
                AttackName = "Ataque a Distancia",
                AnimationKey = "hector_distant_attack",
                HitEffectKey = "distant_hit",
                HitSoundKey = "hit_sound_distant",
                Damage = 30,
                CastAnimationKey = "hector_cast"
            })},
            { "ataque especial", (new[] { "especial", "ataque especial","super ataque", "ataque final" }, new AttackInfo {
                AttackName = "Ataque Especial",
                AnimationKey = "hector_special_attack",
                HitEffectKey = "special_hit",
                HitSoundKey = "hit_sound_special",
                Damage = 60 ,
                CastAnimationKey = "hector_cast"
            })}
        };

        private static readonly Dictionary<string, (string[] Keywords, AttackInfo Info)> FemaleCommands = new()
        {
            { "ataque de fuego", (new[] { "fuego", "ataque de fuego", "llamas" }, new AttackInfo {
                AttackName = "Ataque de Fuego",
                AnimationKey = "serra_fire_attack",
                HitEffectKey = "fire_hit",
                HitSoundKey = "hit_sound_fire",
                Damage = 50,
                CastAnimationKey = "serra_cast"
            }) },
            { "ataque de trueno", (new[] { "trueno", "ataque de trueno", "truenos" }, new AttackInfo {
                AttackName = "Ataque de Trueno",
                AnimationKey = "serra_thunder_attack",
                HitEffectKey = "thunder_hit",
                HitSoundKey = "hit_sound_thunder",
                Damage = 40,
                CastAnimationKey = "serra_cast"
            }) },
            { "ataque de viento", (new[] { "viento", "ataque de viento", "vientos" }, new AttackInfo {
                AttackName = "Ataque de Viento",
                AnimationKey = "serra_wind_attack",
                HitEffectKey = "wind_hit",
                HitSoundKey = "hit_sound_wind",
                Damage = 30,
                CastAnimationKey = "serra_cast"
            }) },
            { "ataque divino", (new[] { "divino", "ataque divino", "divinidad", "un ataque divino" }, new AttackInfo {
                AttackName = "Ataque divino",
                AnimationKey = "serra_divine_attack",
                HitEffectKey = "divine_hit",
                HitSoundKey = "hit_sound_divine",
                Damage = 60,
                CastAnimationKey = "serra_cast"
            }) },
        };

        public static AttackInfo? GetAttack(string transcript, string gender)
        {
            if (string.IsNullOrWhiteSpace(transcript) || string.IsNullOrWhiteSpace(gender))
                return null;

            transcript = transcript.ToLower().Trim();
            gender = gender.ToLower().Trim();

            var commandMap = gender switch
            {
                "male" => MaleCommands,
                "female" => FemaleCommands,
                _ => null
            };

            if (commandMap == null)
                return null;

            foreach (var (_, (keywords, info)) in commandMap)
            {
                foreach (var phrase in keywords)
                {
                    if (transcript.Contains(phrase))
                        return info;
                }
            }

            return null;
        }
    }
}