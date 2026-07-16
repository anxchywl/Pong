using UnityEngine;
using UnityEngine.UIElements;

namespace Pong
{
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
        [SerializeField] private StyleSheet styleSheet;
        [SerializeField] private string[] preferredFontNames;

        [Header("Interface language")]
        [SerializeField] private string menuEyebrow;
        [SerializeField] private string menuTitle;
        [SerializeField] private string menuSubtitle;
        [SerializeField] private string pauseEyebrow;
        [SerializeField] private string pauseTitle;
        [SerializeField] private string victoryTitle;
        [SerializeField] private string defeatTitle;
        [SerializeField] private string playerScoreLabel;
        [SerializeField] private string opponentScoreLabel;
        [SerializeField] private string playIcon;
        [SerializeField] private string pauseIcon;
        [SerializeField] private string backIcon;
        [SerializeField] private string gameModeIcon;
        [SerializeField] private string skinsIcon;
        [SerializeField] private string backgroundIcon;
        [SerializeField] private string themeIcon;
        [SerializeField] private string settingsIcon;
        [SerializeField] private string creditsIcon;
        [SerializeField] private string quitIcon;
        [SerializeField] private string restartIcon;
        [SerializeField] private string homeIcon;

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
        public StyleSheet StyleSheet => styleSheet;
        public string[] PreferredFontNames => preferredFontNames;
        public string MenuEyebrow => menuEyebrow;
        public string MenuTitle => menuTitle;
        public string MenuSubtitle => menuSubtitle;
        public string PauseEyebrow => pauseEyebrow;
        public string PauseTitle => pauseTitle;
        public string VictoryTitle => victoryTitle;
        public string DefeatTitle => defeatTitle;
        public string PlayerScoreLabel => playerScoreLabel;
        public string OpponentScoreLabel => opponentScoreLabel;
        public string PlayIcon => playIcon;
        public string PauseIcon => pauseIcon;
        public string BackIcon => backIcon;
        public string GameModeIcon => gameModeIcon;
        public string SkinsIcon => skinsIcon;
        public string BackgroundIcon => backgroundIcon;
        public string ThemeIcon => themeIcon;
        public string SettingsIcon => settingsIcon;
        public string CreditsIcon => creditsIcon;
        public string QuitIcon => quitIcon;
        public string RestartIcon => restartIcon;
        public string HomeIcon => homeIcon;
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
