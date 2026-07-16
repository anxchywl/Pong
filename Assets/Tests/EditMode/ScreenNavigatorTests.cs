using NUnit.Framework;

namespace Pong.Tests
{
    public sealed class ScreenNavigatorTests
    {
        [Test]
        public void Back_ReturnsToPreviousScreen()
        {
            ScreenNavigator navigator = new ScreenNavigator(AppScreen.MainMenu);
            navigator.Open(AppScreen.GameMode);
            navigator.Open(AppScreen.Settings);

            bool navigated = navigator.Back();

            Assert.That(navigated, Is.True);
            Assert.That(navigator.Current, Is.EqualTo(AppScreen.GameMode));
        }

        [Test]
        public void Home_ClearsNavigationHistory()
        {
            ScreenNavigator navigator = new ScreenNavigator(AppScreen.MainMenu);
            navigator.Open(AppScreen.Customization);

            navigator.Home();

            Assert.That(navigator.Current, Is.EqualTo(AppScreen.MainMenu));
            Assert.That(navigator.Back(), Is.False);
        }
    }
}
