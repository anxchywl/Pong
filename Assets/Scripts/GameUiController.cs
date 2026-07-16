using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Pong
{
    [RequireComponent(typeof(UIDocument), typeof(AudioSource))]
    public sealed class GameUiController : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField] private SeatDirector seats;
        [SerializeField] private GameModeCatalog gameModes;
        [SerializeField] private ThemeCatalog themes;
        [SerializeField] private CosmeticCatalog cosmetics;
        [SerializeField] private GamePresentation presentation;

        private UIDocument document;
        private VisualElement root;
        private VisualElement screenLayer;
        private VisualElement quitConfirmation;
        private VisualElement winPanel;
        private VisualElement debugPanel;
        private Label debugFps;
        private ScreenNavigator navigator;
        private ScreenHost screenHost;
        private PlayerPreferences preferences;
        private GameSettings settings;
        private UiAudioFeedback audioFeedback;
        private UiThemePresenter themePresenter;
        private MainMenuView mainMenu;
        private PlayersView playersView;
        private Button playButton;
        private GameModeView gameModeView;
        private SettingsView settingsView;
        private CustomizationView customizationView;
        private MatchHud hud;
        private PauseMenuView pauseMenu;
        private bool settingsOpenedFromPause;
        private bool debugVisible;
        private float nextFpsUpdate;

        public string ActiveThemeId => preferences?.SelectedThemeId ?? string.Empty;

        public void SelectTheme(string themeId)
        {
            if (themes.Find(themeId) == null)
            {
                return;
            }

            preferences.SelectTheme(themeId);
            ApplyThemeAndCosmetics();
        }

        private void OnEnable()
        {
            // a missing seat director means the scene predates the court seats, and every later
            // failure would be a confusing null rather than the one fact that matters
            if (seats == null)
            {
                Debug.LogError(
                    "GameUiController has no SeatDirector, so the court has no seats. " +
                    "Run Pong > Setup Game UI to author them, then enter play mode again.",
                    this
                );
                enabled = false;
                return;
            }

            document = GetComponent<UIDocument>();
            root = document.rootVisualElement;
            preferences = new PlayerPreferences();
            preferences.Load();
            settings = GameSettings.Load();
            settings.Sanitize();
            audioFeedback = new UiAudioFeedback(GetComponent<AudioSource>());
            themePresenter = new UiThemePresenter(root);

            BuildViews();
            ApplySettings();
            ApplyThemeAndCosmetics();
            match.StateChanged += HandleMatchStateChanged;
            navigator.Changed += HandleScreenChanged;
            seats.Roster.Changed += HandleRosterChanged;
            HandleRosterChanged();
            HandleModeChanged();
            HandleScreenChanged(AppScreen.MainMenu);
        }

        private void Start()
        {
            HandleMatchStateChanged(match.State);
        }

        private void Update()
        {
            HandleShortcuts();
            UpdateDebugDisplay();
        }

        private void OnDisable()
        {
            if (match != null)
            {
                match.StateChanged -= HandleMatchStateChanged;
            }

            if (navigator != null)
            {
                navigator.Changed -= HandleScreenChanged;
            }

            if (seats != null)
            {
                seats.Roster.Changed -= HandleRosterChanged;
            }
        }

        private void HandleRosterChanged()
        {
            mainMenu.RenderLineup();
            playersView.Render();
            playButton.SetEnabled(seats.Roster.IsPlayable);
        }

        private void BuildViews()
        {
            screenLayer = root.Q<VisualElement>("screen-layer");
            quitConfirmation = root.Q<VisualElement>("quit-confirmation");
            winPanel = root.Q<VisualElement>("win-panel");
            debugPanel = root.Q<VisualElement>("debug-panel");
            debugFps = root.Q<Label>("debug-fps");

            navigator = new ScreenNavigator(AppScreen.MainMenu);
            screenHost = new ScreenHost(root);
            hud = new MatchHud(root);
            mainMenu = new MainMenuView(
                root,
                seats.Roster,
                seats.Profiles,
                StartSelectedMode,
                navigator.Open,
                ShowQuitConfirmation,
                PlayClick
            );
            playersView = new PlayersView(root, seats.Roster, seats.Profiles, PlayClick);
            playButton = root.Q<Button>("play-button");
            gameModeView = new GameModeView(root, gameModes, preferences, HandleModeChanged, PlayClick);
            customizationView = new CustomizationView(
                root,
                themes,
                cosmetics,
                preferences,
                ApplyThemeAndCosmetics,
                PlayClick
            );
            settingsView = new SettingsView(root, settings, ApplySettings, ApplyResolution, PlayClick);
            pauseMenu = new PauseMenuView(
                root,
                match.ResumeMatch,
                RestartFromPause,
                OpenPauseSettings,
                ReturnToMainMenu,
                ShowQuitConfirmation,
                PlayClick
            );

            root.Q<Button>("hud-pause-button").clicked += () =>
            {
                PlayClick();
                match.TogglePause();
            };
            root.Q<Button>("players-back-button").clicked += NavigateBack;
            root.Q<Button>("mode-back-button").clicked += NavigateBack;
            root.Q<Button>("customization-back-button").clicked += NavigateBack;
            root.Q<Button>("settings-back-button").clicked += CloseSettings;
            root.Q<Button>("credits-back-button").clicked += NavigateBack;
            root.Q<Button>("win-restart-button").clicked += RestartFromWin;
            root.Q<Button>("win-main-menu-button").clicked += () =>
            {
                PlayClick();
                ReturnToMainMenu();
            };
            root.Q<Button>("quit-cancel-button").clicked += HideQuitConfirmation;
            root.Q<Button>("quit-confirm-button").clicked += QuitGame;
        }

        private void HandleShortcuts()
        {
            bool keyboardCancel = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            bool gamepadCancel = Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
            if (keyboardCancel || gamepadCancel)
            {
                HandleEscape();
            }

            if (Debug.isDebugBuild && Keyboard.current != null && Keyboard.current.f10Key.wasPressedThisFrame)
            {
                debugVisible = !debugVisible;
                debugPanel.EnableInClassList("is-hidden", !debugVisible);
            }
        }

        private void HandleEscape()
        {
            if (!quitConfirmation.ClassListContains("is-hidden"))
            {
                HideQuitConfirmation();
                return;
            }

            if (settingsOpenedFromPause)
            {
                CloseSettings();
                return;
            }

            switch (match.State.Phase)
            {
                case MatchPhase.Serving:
                case MatchPhase.Playing:
                    match.TogglePause();
                    break;
                case MatchPhase.Paused:
                    match.ResumeMatch();
                    break;
                case MatchPhase.FrontEnd:
                    navigator.Back();
                    break;
            }
        }

        private void HandleMatchStateChanged(MatchState state)
        {
            hud.Render(state);
            bool inFrontEnd = state.Phase == MatchPhase.FrontEnd || settingsOpenedFromPause;
            screenLayer.EnableInClassList("is-hidden", !inFrontEnd);
            pauseMenu.SetVisible(state.Phase == MatchPhase.Paused && !settingsOpenedFromPause);
            winPanel.EnableInClassList("is-hidden", state.Phase != MatchPhase.Won);

            if (state.Phase == MatchPhase.Won)
            {
                Label title = root.Q<Label>("win-title");
                GameTheme theme = GetActiveTheme();
                title.text = state.Winner == PlayerSide.Left ? theme.VictoryTitle : theme.DefeatTitle;
                root.Q<Label>("win-score").text = $"{state.LeftScore}  —  {state.RightScore}";
                winPanel.schedule.Execute(() => root.Q<Button>("win-restart-button").Focus());
            }
        }

        private void HandleScreenChanged(AppScreen screen)
        {
            screenHost.Show(screen);
        }

        private void HandleModeChanged()
        {
            GameModeDefinition selected = gameModes.Find(preferences.SelectedModeId);
            if (selected == null || !selected.Available)
            {
                selected = gameModes.Find("classic");
                preferences.SelectMode(selected.Id);
            }

            mainMenu.SetSelectedMode(selected.DisplayName);
            hud.SetMode(selected.DisplayName);
        }

        private void StartSelectedMode()
        {
            settingsOpenedFromPause = false;
            match.StartMatch(settings.PointsToWin, settings.GameSpeed);
        }

        private void OpenPauseSettings()
        {
            settingsOpenedFromPause = true;
            settingsView.ShowFirstCategory();
            screenLayer.RemoveFromClassList("is-hidden");
            pauseMenu.SetVisible(false);
            screenHost.Show(AppScreen.Settings);
        }

        private void CloseSettings()
        {
            PlayClick();
            if (settingsOpenedFromPause)
            {
                settingsOpenedFromPause = false;
                screenLayer.AddToClassList("is-hidden");
                pauseMenu.SetVisible(true);
                return;
            }

            navigator.Back();
        }

        private void NavigateBack()
        {
            PlayClick();
            navigator.Back();
        }

        private void RestartFromPause()
        {
            settingsOpenedFromPause = false;
            match.RestartMatch();
        }

        private void RestartFromWin()
        {
            PlayClick();
            match.RestartMatch();
        }

        private void ReturnToMainMenu()
        {
            settingsOpenedFromPause = false;
            navigator.Home();
            match.EnterFrontEnd();
        }

        private void ApplyThemeAndCosmetics()
        {
            GameTheme theme = GetActiveTheme();
            CosmeticDefinition background = cosmetics.FindSelected(
                theme.Id,
                CosmeticCategory.Background,
                preferences.GetCosmetic(theme.Id, CosmeticCategory.Background)
            );
            float effectIntensity = background?.EffectIntensity ?? 0.5f;
            presentation.Apply(theme, cosmetics, preferences);
            themePresenter.Apply(theme, effectIntensity);
            audioFeedback.ApplyTheme(theme);
            mainMenu.SetSelectedTheme(theme.DisplayName);
        }

        private GameTheme GetActiveTheme()
        {
            GameTheme theme = themes.Find(preferences.SelectedThemeId);
            if (theme != null)
            {
                return theme;
            }

            theme = themes.Default;
            preferences.SelectTheme(theme.Id);
            return theme;
        }

        private void ApplySettings()
        {
            settings.Sanitize();
            AudioListener.volume = settings.MasterVolume;
            QualitySettings.vSyncCount = settings.VSync ? 1 : 0;
            Screen.fullScreen = settings.Fullscreen;
            document.panelSettings.scale = settings.UiScale;
            root.EnableInClassList("reduced-motion", settings.ReducedMotion);
            audioFeedback.Volume = settings.SfxVolume;
            presentation.SetSfxVolume(settings.SfxVolume);
        }

        private void ApplyResolution(Resolution resolution)
        {
            Screen.SetResolution(resolution.width, resolution.height, settings.Fullscreen);
        }

        private void ShowQuitConfirmation()
        {
            quitConfirmation.RemoveFromClassList("is-hidden");
            quitConfirmation.schedule.Execute(() => root.Q<Button>("quit-cancel-button").Focus());
        }

        private void HideQuitConfirmation()
        {
            PlayClick();
            quitConfirmation.AddToClassList("is-hidden");
        }

        private void QuitGame()
        {
            PlayClick();
            Application.Quit();
        }

        private void PlayClick()
        {
            audioFeedback.PlayClick();
        }

        private void UpdateDebugDisplay()
        {
            if (!debugVisible || Time.unscaledTime < nextFpsUpdate)
            {
                return;
            }

            nextFpsUpdate = Time.unscaledTime + 0.25f;
            float framesPerSecond = Time.unscaledDeltaTime > 0f ? 1f / Time.unscaledDeltaTime : 0f;
            debugFps.text = $"{framesPerSecond:0} FPS";
        }
    }
}
