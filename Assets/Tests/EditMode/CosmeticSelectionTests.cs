using NUnit.Framework;

namespace Pong.Tests
{
    public sealed class CosmeticSelectionTests
    {
        [Test]
        public void Get_ReturnsEmptyForAnUnchosenCategory()
        {
            CosmeticSelection selection = new CosmeticSelection("retro");

            Assert.That(selection.Get("retro", CosmeticCategory.Paddle), Is.Empty);
        }

        [Test]
        public void Set_KeepsCategoriesAndThemesApart()
        {
            CosmeticSelection selection = new CosmeticSelection("retro");

            selection.Set("retro", CosmeticCategory.Paddle, "retro-paddle-cabinet");
            selection.Set("retro", CosmeticCategory.Ball, "retro-ball-amber");
            selection.Set("futuristic", CosmeticCategory.Paddle, "futuristic-paddle-alloy");

            Assert.That(selection.Get("retro", CosmeticCategory.Paddle), Is.EqualTo("retro-paddle-cabinet"));
            Assert.That(selection.Get("retro", CosmeticCategory.Ball), Is.EqualTo("retro-ball-amber"));
            Assert.That(selection.Get("futuristic", CosmeticCategory.Paddle), Is.EqualTo("futuristic-paddle-alloy"));
        }

        /// The workshop's whole premise: editing the draft must not touch what is saved.
        [Test]
        public void Clone_DoesNotShareStateWithItsSource()
        {
            CosmeticSelection saved = new CosmeticSelection("retro");
            saved.Set("retro", CosmeticCategory.Paddle, "retro-paddle-cabinet");

            CosmeticSelection draft = saved.Clone();
            draft.Set("retro", CosmeticCategory.Paddle, "retro-paddle-terminal");
            draft.ThemeId = "futuristic";

            Assert.That(saved.Get("retro", CosmeticCategory.Paddle), Is.EqualTo("retro-paddle-cabinet"));
            Assert.That(saved.ThemeId, Is.EqualTo("retro"));
        }

        [Test]
        public void Matches_IsTrueForAnUntouchedClone()
        {
            CosmeticSelection saved = new CosmeticSelection("retro");
            saved.Set("retro", CosmeticCategory.Arena, "retro-arena-vector");

            Assert.That(saved.Clone().Matches(saved), Is.True);
        }

        [Test]
        public void Matches_NoticesAChangedCosmetic()
        {
            CosmeticSelection saved = new CosmeticSelection("retro");
            saved.Set("retro", CosmeticCategory.Arena, "retro-arena-vector");
            CosmeticSelection draft = saved.Clone();

            draft.Set("retro", CosmeticCategory.Arena, "retro-arena-phosphor");

            Assert.That(draft.Matches(saved), Is.False);
        }

        [Test]
        public void Matches_NoticesAChangedTheme()
        {
            CosmeticSelection saved = new CosmeticSelection("retro");
            CosmeticSelection draft = saved.Clone();

            draft.ThemeId = "futuristic";

            Assert.That(draft.Matches(saved), Is.False);
        }

        /// A cosmetic chosen for a theme the player is not wearing is still a change.
        [Test]
        public void Matches_NoticesAnAddedCategory()
        {
            CosmeticSelection saved = new CosmeticSelection("retro");
            CosmeticSelection draft = saved.Clone();

            draft.Set("futuristic", CosmeticCategory.Ball, "futuristic-ball-ion");

            Assert.That(draft.Matches(saved), Is.False);
        }

        [Test]
        public void TryParseKey_RoundTripsAKey()
        {
            string key = CosmeticSelection.Key("retro", CosmeticCategory.Effects);

            bool parsed = CosmeticSelection.TryParseKey(key, out string themeId, out CosmeticCategory category);

            Assert.That(parsed, Is.True);
            Assert.That(themeId, Is.EqualTo("retro"));
            Assert.That(category, Is.EqualTo(CosmeticCategory.Effects));
        }

        [Test]
        public void TryParseKey_RejectsRubbish()
        {
            Assert.That(CosmeticSelection.TryParseKey("nonsense", out _, out _), Is.False);
            Assert.That(CosmeticSelection.TryParseKey("retro.notacategory", out _, out _), Is.False);
        }
    }
}
