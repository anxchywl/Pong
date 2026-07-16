using System;
using System.Collections.Generic;

namespace Pong
{
    public sealed class ScreenNavigator
    {
        private readonly Stack<AppScreen> history = new Stack<AppScreen>();

        public ScreenNavigator(AppScreen initialScreen)
        {
            Current = initialScreen;
        }

        public event Action<AppScreen> Changed;

        public AppScreen Current { get; private set; }

        public void Open(AppScreen screen)
        {
            if (screen == Current)
            {
                return;
            }

            history.Push(Current);
            Current = screen;
            Changed?.Invoke(Current);
        }

        public void Home()
        {
            history.Clear();
            SetCurrent(AppScreen.MainMenu);
        }

        public bool Back()
        {
            if (history.Count == 0)
            {
                return false;
            }

            SetCurrent(history.Pop());
            return true;
        }

        private void SetCurrent(AppScreen screen)
        {
            if (screen == Current)
            {
                return;
            }

            Current = screen;
            Changed?.Invoke(Current);
        }
    }
}
