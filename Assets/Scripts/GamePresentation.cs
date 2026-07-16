using System.Collections.Generic;
using UnityEngine;

namespace Pong
{
    public sealed class GamePresentation : MonoBehaviour
    {
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private BallController ballController;

        [Tooltip("Every paddle on the court. Addressed by the side it defends rather than " +
            "individually, so a court with four paddles themes as readily as one with two.")]
        [SerializeField] private PaddleSeat[] paddles;

        [SerializeField] private SpriteRenderer ballRenderer;
        [SerializeField] private SpriteRenderer ballGlow;
        [SerializeField] private SpriteRenderer topWall;
        [SerializeField] private SpriteRenderer bottomWall;
        [SerializeField] private SpriteRenderer[] centerDashes;
        [SerializeField] private ParticleSystem impactParticles;
        [SerializeField] private AudioSource gameplayAudioSource;

        private GameTheme activeTheme;
        private AudioClip bounceClip;
        private readonly Dictionary<string, Material> particleMaterials = new Dictionary<string, Material>();
        private readonly Dictionary<string, AudioClip> bounceClips = new Dictionary<string, AudioClip>();

        private void OnEnable()
        {
            ballController.Bounced += HandleBallBounced;
        }

        private void OnDisable()
        {
            ballController.Bounced -= HandleBallBounced;
        }

        public void Apply(
            GameTheme theme,
            CosmeticCatalog catalog,
            CosmeticSelection selection
        )
        {
            activeTheme = theme;
            CosmeticDefinition paddle = GetSelection(theme, catalog, selection, CosmeticCategory.Paddle);
            CosmeticDefinition ball = GetSelection(theme, catalog, selection, CosmeticCategory.Ball);
            CosmeticDefinition arena = GetSelection(theme, catalog, selection, CosmeticCategory.Arena);
            CosmeticDefinition background = GetSelection(theme, catalog, selection, CosmeticCategory.Effects);

            Color leftColor = paddle?.PrimaryColor ?? theme.PlayerColor;
            Color rightColor = paddle?.SecondaryColor ?? theme.OpponentColor;
            foreach (PaddleSeat seat in paddles)
            {
                seat.Renderer.color = seat.Side == PlayerSide.Left ? leftColor : rightColor;
            }

            ballRenderer.color = ball?.PrimaryColor ?? theme.BallColor;
            topWall.color = arena?.PrimaryColor ?? theme.ArenaColor;
            bottomWall.color = arena?.PrimaryColor ?? theme.ArenaColor;

            Color centerColor = arena?.SecondaryColor ?? theme.CenterLineColor;
            foreach (SpriteRenderer dash in centerDashes)
            {
                dash.color = centerColor;
            }

            gameplayCamera.backgroundColor = background?.PrimaryColor ?? theme.BackgroundColor;
            float effectIntensity = background?.EffectIntensity ?? 0.5f;
            ApplyGlow(theme, effectIntensity);
            ApplyParticles(theme);
            if (!bounceClips.TryGetValue(theme.Id, out bounceClip))
            {
                bounceClip = CreateBounceClip(theme);
                bounceClips.Add(theme.Id, bounceClip);
            }
        }

        public void SetSfxVolume(float value)
        {
            gameplayAudioSource.volume = Mathf.Clamp01(value) * 0.18f;
        }

        private void ApplyGlow(GameTheme theme, float effectIntensity)
        {
            float intensity = theme.GlowIntensity * Mathf.Lerp(0.7f, 1.3f, effectIntensity);
            bool glowVisible = intensity > 0.01f;

            foreach (PaddleSeat seat in paddles)
            {
                seat.Glow.enabled = glowVisible;
                Color glow = seat.Renderer.color;
                glow.a = intensity * 0.34f;
                seat.Glow.color = glow;
            }

            ballGlow.enabled = glowVisible;
            Color currentBallColor = ballRenderer.color;
            currentBallColor.a = intensity * 0.42f;
            ballGlow.color = currentBallColor;
        }

        private void ApplyParticles(GameTheme theme)
        {
            ParticleSystem.MainModule main = impactParticles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = theme.OverlayPattern == ThemeOverlayPattern.Scanlines ? 0.22f : 0.48f;
            main.startSize = theme.ParticleSize;
            main.startSpeed = theme.ParticleSpeed;
            main.startColor = theme.ParticleColor;

            ParticleSystem.EmissionModule emission = impactParticles.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = impactParticles.shape;
            shape.enabled = false;
            impactParticles.GetComponent<ParticleSystemRenderer>().sharedMaterial = GetParticleMaterial(theme);
        }

        private Material GetParticleMaterial(GameTheme theme)
        {
            if (particleMaterials.TryGetValue(theme.Id, out Material material))
            {
                return material;
            }

            material = new Material(theme.ParticleMaterial)
            {
                name = $"{theme.DisplayName} Particle Material",
                mainTexture = CreateParticleTexture(theme.ParticleStyle)
            };
            particleMaterials.Add(theme.Id, material);
            return material;
        }

        private static Texture2D CreateParticleTexture(ThemeParticleStyle style)
        {
            const int size = 16;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = $"{style} Particle",
                filterMode = style == ThemeParticleStyle.Pixel ? FilterMode.Point : FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = 1f;
                    if (style == ThemeParticleStyle.Soft)
                    {
                        Vector2 offset = new Vector2(x + 0.5f, y + 0.5f) / size * 2f - Vector2.one;
                        alpha = Mathf.Clamp01(1f - offset.magnitude);
                        alpha *= alpha;
                    }

                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }

        private void HandleBallBounced(Vector2 position)
        {
            if (activeTheme == null)
            {
                return;
            }

            if (activeTheme.ImpactParticleCount > 0)
            {
                ParticleSystem.EmitParams parameters = new ParticleSystem.EmitParams
                {
                    position = position,
                    applyShapeToPosition = false
                };
                impactParticles.Emit(parameters, activeTheme.ImpactParticleCount);
            }

            if (bounceClip != null)
            {
                gameplayAudioSource.PlayOneShot(bounceClip);
            }
        }

        private static AudioClip CreateBounceClip(GameTheme theme)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * theme.BounceDuration);
            float[] samples = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                float time = (float)index / sampleRate;
                float progress = (float)index / sampleCount;
                float phase = 2f * Mathf.PI * theme.BounceFrequency * time;
                float wave = theme.AudioWaveform == ThemeAudioWaveform.Square
                    ? Mathf.Sign(Mathf.Sin(phase))
                    : Mathf.Sin(phase) + Mathf.Sin(phase * 2f) * 0.12f;
                samples[index] = wave * (1f - progress) * 0.8f;
            }

            AudioClip clip = AudioClip.Create($"{theme.DisplayName} Ball Impact", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static CosmeticDefinition GetSelection(
            GameTheme theme,
            CosmeticCatalog catalog,
            CosmeticSelection selection,
            CosmeticCategory category
        )
        {
            return catalog.FindSelected(theme.Id, category, selection.Get(theme.Id, category));
        }
    }
}
