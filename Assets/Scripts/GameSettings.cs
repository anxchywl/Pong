using UnityEngine;

namespace Pong
{
    public sealed class GameSettings
    {
        private const string Prefix = "pong.settings.";

        public int PointsToWin { get; set; } = 5;
        public float GameSpeed { get; set; } = 1f;
        public float MasterVolume { get; set; } = 0.8f;
        public float MusicVolume { get; set; } = 0.65f;
        public float SfxVolume { get; set; } = 0.8f;
        public bool Fullscreen { get; set; }
        public bool VSync { get; set; } = true;
        public float UiScale { get; set; } = 1f;
        public bool ReducedMotion { get; set; }
        public int ResolutionIndex { get; set; } = -1;

        public static GameSettings Load()
        {
            return new GameSettings
            {
                PointsToWin = PlayerPrefs.GetInt(Prefix + "points-to-win", 5),
                GameSpeed = PlayerPrefs.GetFloat(Prefix + "game-speed", 1f),
                MasterVolume = PlayerPrefs.GetFloat(Prefix + "master-volume", 0.8f),
                MusicVolume = PlayerPrefs.GetFloat(Prefix + "music-volume", 0.65f),
                SfxVolume = PlayerPrefs.GetFloat(Prefix + "sfx-volume", 0.8f),
                Fullscreen = PlayerPrefs.GetInt(Prefix + "fullscreen", Screen.fullScreen ? 1 : 0) == 1,
                VSync = PlayerPrefs.GetInt(Prefix + "vsync", 1) == 1,
                UiScale = PlayerPrefs.GetFloat(Prefix + "ui-scale", 1f),
                ReducedMotion = PlayerPrefs.GetInt(Prefix + "reduced-motion", 0) == 1,
                ResolutionIndex = PlayerPrefs.GetInt(Prefix + "resolution", -1)
            };
        }

        public void Sanitize()
        {
            PointsToWin = Mathf.Clamp(PointsToWin, 1, 21);
            GameSpeed = Mathf.Clamp(GameSpeed, 0.75f, 1.5f);
            MasterVolume = Mathf.Clamp01(MasterVolume);
            MusicVolume = Mathf.Clamp01(MusicVolume);
            SfxVolume = Mathf.Clamp01(SfxVolume);
            UiScale = Mathf.Clamp(UiScale, 0.85f, 1.2f);
            ResolutionIndex = Mathf.Max(-1, ResolutionIndex);
        }

        public void Save()
        {
            Sanitize();
            PlayerPrefs.SetInt(Prefix + "points-to-win", PointsToWin);
            PlayerPrefs.SetFloat(Prefix + "game-speed", GameSpeed);
            PlayerPrefs.SetFloat(Prefix + "master-volume", MasterVolume);
            PlayerPrefs.SetFloat(Prefix + "music-volume", MusicVolume);
            PlayerPrefs.SetFloat(Prefix + "sfx-volume", SfxVolume);
            PlayerPrefs.SetInt(Prefix + "fullscreen", Fullscreen ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "vsync", VSync ? 1 : 0);
            PlayerPrefs.SetFloat(Prefix + "ui-scale", UiScale);
            PlayerPrefs.SetInt(Prefix + "reduced-motion", ReducedMotion ? 1 : 0);
            PlayerPrefs.SetInt(Prefix + "resolution", ResolutionIndex);
            PlayerPrefs.Save();
        }
    }
}
