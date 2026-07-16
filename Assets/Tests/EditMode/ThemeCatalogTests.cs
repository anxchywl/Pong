using System;
using NUnit.Framework;
using UnityEditor;

namespace Pong.Tests
{
    public sealed class ThemeCatalogTests
    {
        [Test]
        public void ThemeAssets_DefineDistinctVisualAndInteractionLanguages()
        {
            GameTheme retro = AssetDatabase.LoadAssetAtPath<GameTheme>("Assets/UI/RetroTheme.asset");
            GameTheme futuristic = AssetDatabase.LoadAssetAtPath<GameTheme>("Assets/UI/FuturisticTheme.asset");

            Assert.That(retro, Is.Not.Null);
            Assert.That(futuristic, Is.Not.Null);
            Assert.That(retro.StyleSheet, Is.Not.EqualTo(futuristic.StyleSheet));
            Assert.That(retro.PreferredFontNames[0], Is.Not.EqualTo(futuristic.PreferredFontNames[0]));
            Assert.That(retro.OverlayPattern, Is.Not.EqualTo(futuristic.OverlayPattern));
            Assert.That(retro.AudioWaveform, Is.Not.EqualTo(futuristic.AudioWaveform));
            Assert.That(retro.ParticleStyle, Is.Not.EqualTo(futuristic.ParticleStyle));
            Assert.That(retro.GlowIntensity, Is.Not.EqualTo(futuristic.GlowIntensity));
            Assert.That(retro.TransitionDuration, Is.Not.EqualTo(futuristic.TransitionDuration));
        }

        [Test]
        public void CosmeticCatalog_ProvidesIndependentSelectionsForEveryThemeCategory()
        {
            CosmeticCatalog catalog = AssetDatabase.LoadAssetAtPath<CosmeticCatalog>("Assets/UI/Cosmetics.asset");
            string[] themeIds = { "retro", "futuristic" };

            foreach (string themeId in themeIds)
            {
                foreach (CosmeticCategory category in Enum.GetValues(typeof(CosmeticCategory)))
                {
                    CosmeticDefinition cosmetic = catalog.FindSelected(themeId, category, string.Empty);
                    Assert.That(cosmetic, Is.Not.Null, $"Missing {category} cosmetic for {themeId}");
                    Assert.That(cosmetic.ThemeId, Is.EqualTo(themeId));
                }
            }
        }
    }
}
