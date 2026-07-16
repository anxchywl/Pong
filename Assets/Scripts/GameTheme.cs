using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pong
{
    /// One fixed string in the interface, addressed by the UXML name or class it belongs to.
    [Serializable]
    public sealed class ThemeCopyEntry
    {
        [Tooltip("A UXML name or class. Every element matching either is updated.")]
        [SerializeField] private string element;
        [SerializeField] private string icon;
        [SerializeField] private string text;

        public string Element => element;

        public string Compose()
        {
            if (string.IsNullOrEmpty(icon))
            {
                return text;
            }

            return string.IsNullOrEmpty(text) ? icon : $"{icon}  {text}";
        }
    }

    public enum ThemeOverlayPattern
    {
        Scanlines,
        Circuit
    }

    public enum ThemeAudioWaveform
    {
        Square,
        Sine
    }

    public enum ThemeParticleStyle
    {
        Pixel,
        Soft
    }

    [CreateAssetMenu(menuName = "Pong/UI/Game Theme")]
    public sealed class GameTheme : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;

        [Tooltip("The theme's whole interface language. It imports the shared structure and this " +
            "theme's own sheet, so swapping it swaps every screen at once.")]
        [SerializeField] private ThemeStyleSheet themeStyleSheet;

        [SerializeField] private string[] preferredFontNames;

        [Header("Interface language")]
        [Tooltip("Every fixed string in the interface, addressed by UXML name or class. A theme " +
            "rewrites the interface's whole voice here, and a new menu entry needs no code.")]
        [SerializeField] private List<ThemeCopyEntry> copy = new List<ThemeCopyEntry>();

        [Tooltip("Chosen at runtime by the result, so these cannot bind to a fixed element")]
        [SerializeField] private string victoryTitle;
        [SerializeField] private string defeatTitle;

        [Header("Presentation")]
        [SerializeField] private Color primaryAccent = Color.white;
        [SerializeField] private Color secondaryAccent = Color.white;
        [SerializeField] private Color playerColor = Color.white;
        [SerializeField] private Color opponentColor = Color.white;
        [SerializeField] private Color ballColor = Color.white;
        [SerializeField] private Color arenaColor = Color.white;
        [SerializeField] private Color centerLineColor = Color.white;
        [SerializeField] private Color backgroundColor = Color.black;
        [SerializeField] private ThemeOverlayPattern overlayPattern;
        [SerializeField, Range(0f, 1f)] private float overlayIntensity;
        [SerializeField, Range(0f, 1f)] private float glowIntensity;

        [Header("Motion and particles")]
        [SerializeField, Min(0f)] private float transitionDuration = 0.12f;
        [SerializeField, Min(0)] private int impactParticleCount = 5;
        [SerializeField, Min(0.01f)] private float particleSize = 0.08f;
        [SerializeField, Min(0f)] private float particleSpeed = 0.7f;
        [SerializeField] private Color particleColor = Color.white;
        [SerializeField] private ThemeParticleStyle particleStyle;
        [SerializeField] private Material particleMaterial;

        [Header("Audio feedback")]
        [SerializeField] private ThemeAudioWaveform audioWaveform;
        [SerializeField, Min(20f)] private float clickFrequency = 560f;
        [SerializeField, Range(0.01f, 0.2f)] private float clickDuration = 0.04f;
        [SerializeField, Min(20f)] private float bounceFrequency = 320f;
        [SerializeField, Range(0.01f, 0.2f)] private float bounceDuration = 0.05f;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public ThemeStyleSheet ThemeStyleSheet => themeStyleSheet;
        public string[] PreferredFontNames => preferredFontNames;
        public IReadOnlyList<ThemeCopyEntry> Copy => copy;
        public string VictoryTitle => victoryTitle;
        public string DefeatTitle => defeatTitle;
        public Color PrimaryAccent => primaryAccent;
        public Color SecondaryAccent => secondaryAccent;
        public Color PlayerColor => playerColor;
        public Color OpponentColor => opponentColor;
        public Color BallColor => ballColor;
        public Color ArenaColor => arenaColor;
        public Color CenterLineColor => centerLineColor;
        public Color BackgroundColor => backgroundColor;
        public ThemeOverlayPattern OverlayPattern => overlayPattern;
        public float OverlayIntensity => overlayIntensity;
        public float GlowIntensity => glowIntensity;
        public float TransitionDuration => transitionDuration;
        public int ImpactParticleCount => impactParticleCount;
        public float ParticleSize => particleSize;
        public float ParticleSpeed => particleSpeed;
        public Color ParticleColor => particleColor;
        public ThemeParticleStyle ParticleStyle => particleStyle;
        public Material ParticleMaterial => particleMaterial;
        public ThemeAudioWaveform AudioWaveform => audioWaveform;
        public float ClickFrequency => clickFrequency;
        public float ClickDuration => clickDuration;
        public float BounceFrequency => bounceFrequency;
        public float BounceDuration => bounceDuration;
    }
}
