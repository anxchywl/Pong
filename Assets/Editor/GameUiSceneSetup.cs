using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UIElements;

namespace Pong.Editor
{
    public static class GameUiSceneSetup
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string PanelSettingsPath = "Assets/UI/GameUiPanelSettings.asset";
        private const string GameModesPath = "Assets/UI/GameModes.asset";
        private const string ThemesPath = "Assets/UI/Themes.asset";
        private const string RetroThemePath = "Assets/UI/RetroTheme.asset";
        private const string FuturisticThemePath = "Assets/UI/FuturisticTheme.asset";
        private const string RetroParticleMaterialPath = "Assets/UI/RetroParticles.mat";
        private const string FuturisticParticleMaterialPath = "Assets/UI/FuturisticParticles.mat";
        private const string CosmeticsPath = "Assets/UI/Cosmetics.asset";
        private const string InputProfilesPath = "Assets/UI/InputProfiles.asset";
        private const string ControlsPath = "Assets/UI/PongControls.inputactions";
        private const string RetroThemeSheetPath = "Assets/UI/Themes/Retro.tss";
        private const string FuturisticThemeSheetPath = "Assets/UI/Themes/Futuristic.tss";

        // the goalkeepers sit at +/-7.5; attackers stand ahead of them with the centre left open
        private const float AttackerColumn = 4.2f;

        // the square sprite is 2.56 world units per unit of local scale, so 0.55 is a paddle about
        // 1.4 units tall: roughly a sixth of the arena, as Pong has always been
        private const float PaddleLength = 0.55f;
        private const float ArenaHalfHeight = 4.44f;

        // the ball meets a paddle's face, so a paddle's width costs nothing in balance. About 1:4
        // against its height reads as a solid bar; much wider and a short paddle turns into a square
        private const float PaddleWidth = 0.13f;

        // the arena filled 98% of the frame's width, leaving the HUD nowhere to live. This leaves a
        // band above the wall deep enough for the score to sit clear of the court. Framing is a
        // camera change only: no world geometry moves and nothing plays differently
        private const float CameraSize = 6.9f;

        [MenuItem("Pong/Setup Game UI")]
        public static void Run()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(ScenePath);
            InputActionAsset controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
            PanelSettings panelSettings = CreatePanelSettings();
            InputProfileCatalog inputProfiles = CreateInputProfiles();
            GameModeCatalog gameModes = CreateGameModes();
            GameTheme futuristic = CreateFuturisticTheme();
            GameTheme retro = CreateRetroTheme();
            ThemeCatalog themes = CreateThemeCatalog(futuristic, retro);
            CosmeticCatalog cosmetics = CreateCosmetics();
            AssetDatabase.SaveAssets();

            RemoveLegacyUi();
            FrameArena();
            SeatDirector seats = CreateSeats(inputProfiles, controls, out PaddleSeat[] paddles);
            CreateMatchShortcuts(controls);
            CreateSeatDeviceWatcher(seats);
            GameObject uiObject = CreateUiObject(panelSettings);
            CreateEventSystem(controls);
            WirePresentation(uiObject.GetComponent<GamePresentation>(), paddles);
            WireController(uiObject.GetComponent<GameUiController>(), seats, gameModes, themes, cosmetics, controls);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static PanelSettings CreatePanelSettings()
        {
            PanelSettings settings = LoadOrCreate<PanelSettings>(PanelSettingsPath);
            settings.name = "GameUiPanelSettings";

            // scaling from a reference resolution made every screen the same size in points, so a
            // phone reported itself as wide as a desktop and no breakpoint could tell them apart.
            // Physical sizing means a point is about a hundredth of an inch everywhere, so lengths
            // are real, touch targets are the size they claim, and the room a layout has is the room
            // it actually has. A 1920x1080 desktop window is unchanged: it was already the reference
            settings.scaleMode = PanelScaleMode.ConstantPhysicalSize;
            settings.referenceDpi = 96f;
            settings.fallbackDpi = 96f;
            settings.sortingOrder = 10;

            // a theme's sheet is assigned at runtime; this is only what the editor shows before
            // play, so the panel is never left with the bare default theme
            settings.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(FuturisticThemeSheetPath);
            EditorUtility.SetDirty(settings);
            return settings;
        }

        private static InputProfileCatalog CreateInputProfiles()
        {
            InputProfileCatalog catalog = LoadOrCreate<InputProfileCatalog>(InputProfilesPath);
            catalog.name = "InputProfiles";
            SerializedObject serialized = new SerializedObject(catalog);
            SerializedProperty list = serialized.FindProperty("profiles");
            list.arraySize = 3;
            // the layout belongs in the name: two players sharing a keyboard must tell their
            // seats apart at a glance, not by reading the subtitle
            SetProfile(list.GetArrayElementAtIndex(0), "keyboard-wasd", "Keyboard W/S",
                "Left of the board", InputProfileKind.Keyboard, "KeyboardLeft");
            SetProfile(list.GetArrayElementAtIndex(1), "keyboard-arrows", "Keyboard Arrows",
                "Right of the board", InputProfileKind.Keyboard, "KeyboardRight");
            SetProfile(list.GetArrayElementAtIndex(2), "gamepad", "Gamepad",
                "Left stick or D-pad", InputProfileKind.Gamepad, "Gamepad");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static void SetProfile(
            SerializedProperty property,
            string id,
            string displayName,
            string hint,
            InputProfileKind kind,
            string controlScheme
        )
        {
            property.FindPropertyRelative("id").stringValue = id;
            property.FindPropertyRelative("displayName").stringValue = displayName;
            property.FindPropertyRelative("hint").stringValue = hint;
            property.FindPropertyRelative("kind").enumValueIndex = (int)kind;
            property.FindPropertyRelative("controlScheme").stringValue = controlScheme;
        }

        private static GameModeCatalog CreateGameModes()
        {
            GameModeCatalog catalog = LoadOrCreate<GameModeCatalog>(GameModesPath);
            SerializedProperty modes = new SerializedObject(catalog).FindProperty("modes");

            // Local Multiplayer is gone as a mode: the seats decide who plays, so a mode that only
            // meant "two humans" would contradict the players screen rather than add anything
            modes.arraySize = 3;
            SetMode(modes.GetArrayElementAtIndex(0), "classic", "Classic",
                "The original duel, played by whoever takes a seat.", "1-4 PLAYERS", true);
            SetMode(modes.GetArrayElementAtIndex(1), "practice", "Practice",
                "A pressure-free court for learning timing and angles.", "SOLO", false);
            SetMode(modes.GetArrayElementAtIndex(2), "ai", "AI League",
                "Face distinct opponents with readable play styles.", "1 PLAYER", false);
            modes.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
        }

        private static GameTheme CreateRetroTheme()
        {
            GameTheme theme = LoadOrCreate<GameTheme>(RetroThemePath);
            theme.name = "RetroTheme";
            SerializedObject serialized = new SerializedObject(theme);
            SetString(serialized, "id", "retro");
            SetString(serialized, "displayName", "Retro");
            SetString(serialized, "description",
                "A cabinet-built world of crisp geometry, warm phosphor, scanlines, and immediate arcade feedback.");
            SetReference(serialized, "themeStyleSheet",
                AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(RetroThemeSheetPath));
            SetStringArray(serialized, "preferredFontNames", "Menlo", "Consolas", "Courier New");
            SetString(serialized, "victoryTitle", "PLAYER ONE WINS");
            SetString(serialized, "defeatTitle", "CPU TAKES THE ROUND");
            SetCopy(
                serialized,
                ("menu-eyebrow", "", "INSERT COIN // QUICK MATCH"),
                ("menu-title", "", "READY PLAYER ONE"),
                ("menu-subtitle", "", "First to the target score takes the cabinet."),
                ("match-card-eyebrow", "", "NEXT MATCH"),
                ("lineup-eyebrow", "", "AT THE CABINET"),
                ("mode-caption", "", "MODE"),
                ("world-caption", "", "WORLD"),
                ("menu-footer-hint", "", "ENTER OR A TO CONFIRM"),
                ("menu-footer-mark", "", "PONG  •  LOCAL BUILD"),
                ("players-eyebrow", "", "CABINET SEATING"),
                ("players-title", "", "PLAYERS"),
                ("left-side-caption", "", "PLAYER ONE SIDE"),
                ("right-side-caption", "", "PLAYER TWO SIDE"),
                ("players-hint", "", "GOALKEEPER HOLDS THE LINE. ATTACKER MEETS THE BALL EARLY."),
                ("wordmark", "", "PONG"),
                ("mode-eyebrow", "", "SELECT YOUR GAME"),
                ("mode-title", "", "GAME MODE"),
                ("mode-description", "", "Choose a ruleset. New games slot into the cabinet without rewiring the menu."),
                ("customization-eyebrow", "", "CABINET WORKSHOP"),
                ("customization-title", "", "CUSTOMIZATION"),
                ("customization-summary", "", "PREVIEW BEFORE YOU APPLY"),
                ("settings-eyebrow", "", "SERVICE MENU"),
                ("settings-title", "", "SETTINGS"),
                ("settings-summary", "", "SAVED AUTOMATICALLY"),
                ("credits-eyebrow", "", "THE NAMES ON THE CABINET"),
                ("credits-title", "", "CREDITS"),
                ("credits-mark", "", "PONG"),
                ("credits-role", "", "DESIGN, ENGINEERING AND GAME FEEL"),
                ("shop-tab-theme", "", "THEME"),
                ("shop-tab-arena", "", "ARENA"),
                ("shop-tab-paddle", "", "PADDLE"),
                ("shop-tab-ball", "", "BALL"),
                ("shop-tab-hud", "", "HUD"),
                ("shop-tab-effects", "", "EFFECTS"),
                ("shop-tab-audio", "", "AUDIO"),
                ("collection-eyebrow", "", "IN THE CABINET"),
                ("collection-owned-caption", "", "UNLOCKED"),
                ("collection-theme-caption", "", "BELONGS TO"),
                ("collection-note", "", "Parts belong to their cabinet and cannot be mixed across worlds."),
                ("workshop-preview-caption", "", "LIVE PREVIEW"),
                ("workshop-reset-button", "", "RESET"),
                ("workshop-randomize-button", "[?]", "RANDOMIZE"),
                ("workshop-apply-button", "[>]", "APPLY"),
                ("settings-tab-gameplay", "", "GAMEPLAY"),
                ("settings-tab-audio", "", "AUDIO"),
                ("settings-tab-graphics", "", "GRAPHICS"),
                ("settings-tab-controls", "", "CONTROLS"),
                ("settings-tab-accessibility", "", "ACCESSIBILITY"),
                ("settings-gameplay-title", "", "MATCH RULES AND PACING"),
                ("settings-audio-title", "", "CABINET VOLUME"),
                ("settings-graphics-title", "", "DISPLAY AND SYNC"),
                ("settings-controls-title", "", "CABINET CONTROLS"),
                ("settings-accessibility-title", "", "COMFORT AND READABILITY"),
                ("pause-eyebrow", "", "SYSTEM HOLD"),
                ("pause-title", "", "GAME PAUSED"),
                ("left-score-caption", "", "P1"),
                ("right-score-caption", "", "CPU"),
                ("play-button", "[>]", "PLAY"),
                ("players-button", "[P]", "PLAYERS"),
                ("game-mode-button", "[M]", "GAME MODE"),
                ("customization-button", "[C]", "CUSTOMIZATION"),
                ("settings-button", "[*]", "SETTINGS"),
                ("quit-button", "[X]", "QUIT"),
                ("hud-pause-button", "[II]", ""),
                ("resume-button", "[>]", "RESUME"),
                ("pause-restart-button", "[R]", "RESTART"),
                ("pause-settings-button", "[*]", "SETTINGS"),
                ("pause-main-menu-button", "[H]", "MAIN MENU"),
                ("pause-quit-button", "[X]", "QUIT"),
                ("win-restart-button", "[R]", "PLAY AGAIN"),
                ("win-main-menu-button", "[H]", "MAIN MENU"),
                ("back-button", "[<]", "BACK")
            );
            SetColor(serialized, "primaryAccent", Hex("F5B942"));
            SetColor(serialized, "secondaryAccent", Hex("6FA25C"));
            SetColor(serialized, "playerColor", Hex("F5B942"));
            SetColor(serialized, "opponentColor", Hex("6FA25C"));
            SetColor(serialized, "ballColor", Hex("F6EECB"));
            SetColor(serialized, "arenaColor", Hex("D8C97C"));
            SetColor(serialized, "centerLineColor", new Color(0.85f, 0.79f, 0.49f, 0.42f));
            SetColor(serialized, "backgroundColor", Hex("050704"));
            SetEnum(serialized, "overlayPattern", ThemeOverlayPattern.Scanlines);
            SetFloat(serialized, "overlayIntensity", 0.18f);
            SetFloat(serialized, "glowIntensity", 0f);
            SetFloat(serialized, "transitionDuration", 0.055f);
            SetInt(serialized, "impactParticleCount", 3);
            SetFloat(serialized, "particleSize", 0.09f);
            SetFloat(serialized, "particleSpeed", 0.55f);
            SetColor(serialized, "particleColor", Hex("F5B942"));
            SetEnum(serialized, "particleStyle", ThemeParticleStyle.Pixel);
            SetReference(serialized, "particleMaterial", CreateParticleMaterial(RetroParticleMaterialPath));
            SetEnum(serialized, "audioWaveform", ThemeAudioWaveform.Square);
            SetFloat(serialized, "clickFrequency", 440f);
            SetFloat(serialized, "clickDuration", 0.035f);
            SetFloat(serialized, "bounceFrequency", 220f);
            SetFloat(serialized, "bounceDuration", 0.045f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return theme;
        }

        private static GameTheme CreateFuturisticTheme()
        {
            GameTheme theme = LoadOrCreate<GameTheme>(FuturisticThemePath);
            theme.name = "FuturisticTheme";
            SerializedObject serialized = new SerializedObject(theme);
            SetString(serialized, "id", "futuristic");
            SetString(serialized, "displayName", "Futuristic");
            SetString(serialized, "description",
                "A quiet competition space with layered glass, precise motion, ceramic surfaces, and restrained light.");
            SetReference(serialized, "themeStyleSheet",
                AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(FuturisticThemeSheetPath));
            SetStringArray(serialized, "preferredFontNames", "Avenir Next", "Segoe UI", "Arial");
            SetString(serialized, "victoryTitle", "VICTORY CONFIRMED");
            SetString(serialized, "defeatTitle", "MATCH COMPLETE");
            SetCopy(
                serialized,
                ("menu-eyebrow", "", "MATCH LINK READY"),
                ("menu-title", "", "Enter the arena."),
                ("menu-subtitle", "", "Precision play in a calm, responsive competition space."),
                ("match-card-eyebrow", "", "NEXT MATCH"),
                ("lineup-eyebrow", "", "LINEUP"),
                ("mode-caption", "", "MODE"),
                ("world-caption", "", "WORLD"),
                ("menu-footer-hint", "", "ENTER OR A TO CONFIRM"),
                ("menu-footer-mark", "", "PONG  •  LOCAL BUILD"),
                ("players-eyebrow", "", "WHO IS PLAYING"),
                ("players-title", "", "Players"),
                ("left-side-caption", "", "LEFT SIDE"),
                ("right-side-caption", "", "RIGHT SIDE"),
                ("players-hint", "", "A goalkeeper defends the goal. An attacker meets the ball early."),
                ("wordmark", "", "PONG"),
                ("mode-eyebrow", "", "PLAY YOUR WAY"),
                ("mode-title", "", "Game Mode"),
                ("mode-description", "", "Choose a ruleset. New modes slot into this library without changing the menu."),
                ("customization-eyebrow", "", "MAKE IT YOURS"),
                ("customization-title", "", "Customization"),
                ("customization-summary", "", "PREVIEW BEFORE YOU APPLY"),
                ("settings-eyebrow", "", "TUNE THE EXPERIENCE"),
                ("settings-title", "", "Settings"),
                ("settings-summary", "", "SAVED AUTOMATICALLY"),
                ("credits-eyebrow", "", "THE PEOPLE BEHIND THE PADDLES"),
                ("credits-title", "", "Credits"),
                ("credits-mark", "", "PONG"),
                ("credits-role", "", "Design, engineering and game feel"),
                ("shop-tab-theme", "", "THEME"),
                ("shop-tab-arena", "", "ARENA"),
                ("shop-tab-paddle", "", "PADDLE"),
                ("shop-tab-ball", "", "BALL"),
                ("shop-tab-hud", "", "HUD"),
                ("shop-tab-effects", "", "EFFECTS"),
                ("shop-tab-audio", "", "AUDIO"),
                ("collection-eyebrow", "", "COLLECTION"),
                ("collection-owned-caption", "", "UNLOCKED"),
                ("collection-theme-caption", "", "BELONGS TO"),
                ("collection-note", "", "Cosmetics belong to their world and cannot be mixed across themes."),
                ("workshop-preview-caption", "", "LIVE PREVIEW"),
                ("workshop-reset-button", "", "RESET"),
                ("workshop-randomize-button", "~", "RANDOMIZE"),
                ("workshop-apply-button", ">", "APPLY"),
                ("settings-tab-gameplay", "", "GAMEPLAY"),
                ("settings-tab-audio", "", "AUDIO"),
                ("settings-tab-graphics", "", "GRAPHICS"),
                ("settings-tab-controls", "", "CONTROLS"),
                ("settings-tab-accessibility", "", "ACCESSIBILITY"),
                ("settings-gameplay-title", "", "Match rules and pacing"),
                ("settings-audio-title", "", "Independent volume channels"),
                ("settings-graphics-title", "", "Display and synchronisation"),
                ("settings-controls-title", "", "Keyboard and controller input"),
                ("settings-accessibility-title", "", "Comfort and readability"),
                ("pause-eyebrow", "", "SIMULATION SUSPENDED"),
                ("pause-title", "", "Awaiting your return."),
                ("left-score-caption", "", "YOU"),
                ("right-score-caption", "", "RIVAL"),
                ("play-button", ">", "PLAY"),
                ("players-button", "&&", "PLAYERS"),
                ("game-mode-button", "//", "GAME MODE"),
                ("customization-button", "::", "CUSTOMIZATION"),
                ("settings-button", "<>", "SETTINGS"),
                ("quit-button", "X", "QUIT"),
                ("hud-pause-button", "II", ""),
                ("resume-button", ">", "RESUME"),
                ("pause-restart-button", "R", "RESTART"),
                ("pause-settings-button", "<>", "SETTINGS"),
                ("pause-main-menu-button", "H", "MAIN MENU"),
                ("pause-quit-button", "X", "QUIT"),
                ("win-restart-button", "R", "PLAY AGAIN"),
                ("win-main-menu-button", "H", "MAIN MENU"),
                ("back-button", "<", "BACK")
            );
            SetColor(serialized, "primaryAccent", Hex("D6A8FF"));
            SetColor(serialized, "secondaryAccent", Hex("FFB07E"));
            SetColor(serialized, "playerColor", Hex("D6A8FF"));
            SetColor(serialized, "opponentColor", Hex("FFB07E"));
            SetColor(serialized, "ballColor", Hex("F6F3FA"));
            SetColor(serialized, "arenaColor", Hex("C5C1DB"));
            SetColor(serialized, "centerLineColor", new Color(0.77f, 0.75f, 0.86f, 0.24f));
            SetColor(serialized, "backgroundColor", Hex("080A11"));
            SetEnum(serialized, "overlayPattern", ThemeOverlayPattern.Circuit);
            SetFloat(serialized, "overlayIntensity", 0.14f);
            SetFloat(serialized, "glowIntensity", 0.55f);
            SetFloat(serialized, "transitionDuration", 0.18f);
            SetInt(serialized, "impactParticleCount", 7);
            SetFloat(serialized, "particleSize", 0.07f);
            SetFloat(serialized, "particleSpeed", 0.8f);
            SetColor(serialized, "particleColor", Hex("D6A8FF"));
            SetEnum(serialized, "particleStyle", ThemeParticleStyle.Soft);
            SetReference(serialized, "particleMaterial", CreateParticleMaterial(FuturisticParticleMaterialPath));
            SetEnum(serialized, "audioWaveform", ThemeAudioWaveform.Sine);
            SetFloat(serialized, "clickFrequency", 680f);
            SetFloat(serialized, "clickDuration", 0.065f);
            SetFloat(serialized, "bounceFrequency", 510f);
            SetFloat(serialized, "bounceDuration", 0.075f);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return theme;
        }

        private static ThemeCatalog CreateThemeCatalog(GameTheme futuristic, GameTheme retro)
        {
            ThemeCatalog catalog = LoadOrCreate<ThemeCatalog>(ThemesPath);
            SerializedObject serialized = new SerializedObject(catalog);
            SerializedProperty themes = serialized.FindProperty("themes");
            themes.arraySize = 2;
            themes.GetArrayElementAtIndex(0).objectReferenceValue = futuristic;
            themes.GetArrayElementAtIndex(1).objectReferenceValue = retro;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return catalog;
        }

        private static CosmeticCatalog CreateCosmetics()
        {
            CosmeticCatalog catalog = LoadOrCreate<CosmeticCatalog>(CosmeticsPath);
            SerializedObject serialized = new SerializedObject(catalog);
            SerializedProperty items = serialized.FindProperty("cosmetics");
            items.arraySize = 30;
            int index = 0;

            // retro
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-arena-upright", "Upright Cabinet",
                "Brass rails and a low dotted divider.", CosmeticCategory.Arena,
                Hex("D8C97C"), Hex("8A7F4E"), 0.4f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-arena-phosphor", "Phosphor Court",
                "A disciplined green monochrome court.", CosmeticCategory.Arena,
                Hex("91B96C"), Hex("5C7444"), 0.5f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-arena-vector", "Vector Monitor",
                "Thin bright strokes on true black.", CosmeticCategory.Arena,
                Hex("9BE3C8"), Hex("2F6B57"), 0.65f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-paddle-cabinet", "Cabinet Pair",
                "Warm amber against muted phosphor.", CosmeticCategory.Paddle,
                Hex("F5B942"), Hex("6FA25C"), 0.35f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-paddle-terminal", "Terminal Pair",
                "A monochrome set from a service terminal.", CosmeticCategory.Paddle,
                Hex("8DBB63"), Hex("C7D89A"), 0.45f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-paddle-amber", "Amber Deluxe",
                "Both sides in amber, told apart by weight.", CosmeticCategory.Paddle,
                Hex("FFD36B"), Hex("B37A1F"), 0.5f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-ball-white", "Pixel White",
                "The high-contrast arcade standard.", CosmeticCategory.Ball,
                Hex("F6EECB"), Hex("F6EECB"), 0.3f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-ball-amber", "Amber Dot",
                "A warmer point with a clear motion read.", CosmeticCategory.Ball,
                Hex("FFD36B"), Hex("F5B942"), 0.5f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-hud-plate", "Score Plate",
                "A bolted plate above the court.", CosmeticCategory.Hud,
                Hex("F5B942"), Hex("6B5A24"), 0.4f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-hud-seven-seg", "Seven Segment",
                "Digits cut from a segment display.", CosmeticCategory.Hud,
                Hex("FFB43C"), Hex("5C3E10"), 0.6f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-effects-soft-crt", "Soft CRT",
                "Gentle scanlines, charcoal interior.", CosmeticCategory.Effects,
                Hex("050704"), Hex("161B11"), 0.3f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-effects-sharp-crt", "Sharp CRT",
                "Denser scanlines and deeper blacks.", CosmeticCategory.Effects,
                Hex("030403"), Hex("10140C"), 0.7f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-effects-bezel", "Cabinet Bezel",
                "A heavy bezel closing in the picture.", CosmeticCategory.Effects,
                Hex("070806"), Hex("1D2115"), 0.5f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-audio-square", "Square Wave",
                "The original two-tone bleep.", CosmeticCategory.Audio,
                Hex("F5B942"), Hex("6FA25C"), 0.5f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-audio-cabinet", "Cabinet Speaker",
                "Lower, boxier, a little further away.", CosmeticCategory.Audio,
                Hex("C79A38"), Hex("4E7040"), 0.3f);
            // futuristic
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-arena-glass", "Glass Court",
                "Layered glass over a quiet ceramic floor.", CosmeticCategory.Arena,
                Hex("C5C1DB"), Hex("6E6A85"), 0.45f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-arena-carbon", "Carbon Court",
                "Matte carbon with a machined edge.", CosmeticCategory.Arena,
                Hex("8E93A8"), Hex("43485C"), 0.35f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-arena-ceramic", "Ceramic Court",
                "Warm white ceramic, softly lit.", CosmeticCategory.Arena,
                Hex("E8E4F0"), Hex("9A94AE"), 0.55f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-paddle-alloy", "Alloy Pair",
                "Anodised violet against warm bronze.", CosmeticCategory.Paddle,
                Hex("D6A8FF"), Hex("FFB07E"), 0.45f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-paddle-ceramic", "Ceramic Pair",
                "Bright ceramic with a soft falloff.", CosmeticCategory.Paddle,
                Hex("F0EAFF"), Hex("FFD9BE"), 0.3f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-paddle-graphite", "Graphite Pair",
                "Restrained graphite, lit only at the edge.", CosmeticCategory.Paddle,
                Hex("A9A3BE"), Hex("6F6A82"), 0.25f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-ball-pearl", "Pearl",
                "A clean neutral sphere.", CosmeticCategory.Ball,
                Hex("F6F3FA"), Hex("D8D3E6"), 0.4f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-ball-ion", "Ion",
                "A cool core with a short trail.", CosmeticCategory.Ball,
                Hex("CFE6FF"), Hex("8FB6E6"), 0.7f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-hud-pane", "Glass Pane",
                "The score floating on layered glass.", CosmeticCategory.Hud,
                Hex("D6A8FF"), Hex("3A3550"), 0.4f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-hud-minimal", "Minimal",
                "No frame. Type and nothing else.", CosmeticCategory.Hud,
                Hex("F2EFF8"), Hex("2A2738"), 0.15f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-effects-clean", "Clean",
                "No overlay. The court, precisely lit.", CosmeticCategory.Effects,
                Hex("080A11"), Hex("121522"), 0.2f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-effects-lattice", "Lattice",
                "A faint structural grid behind play.", CosmeticCategory.Effects,
                Hex("080A11"), Hex("141830"), 0.5f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-effects-bloom", "Bloom",
                "Light lifts a little off every surface.", CosmeticCategory.Effects,
                Hex("090B14"), Hex("1A1E33"), 0.85f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-audio-sine", "Sine",
                "Soft, tuned, almost musical.", CosmeticCategory.Audio,
                Hex("D6A8FF"), Hex("FFB07E"), 0.5f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "futuristic-audio-chamber", "Chamber",
                "The same tones with air around them.", CosmeticCategory.Audio,
                Hex("B79AE0"), Hex("E6A98A"), 0.75f);

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static GameObject CreateUiObject(PanelSettings panelSettings)
        {
            GameObject uiObject = new GameObject("Game UI");
            UIDocument document = uiObject.AddComponent<UIDocument>();
            document.panelSettings = panelSettings;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI/GameUi.uxml");
            document.sortingOrder = 10;

            AudioSource audioSource = uiObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            uiObject.AddComponent<GamePresentation>();
            uiObject.AddComponent<GameUiController>();
            CreateParticleSystem(uiObject.transform);
            CreateGameplayAudio(uiObject.transform);
            return uiObject;
        }

        private static void CreateParticleSystem(Transform parent)
        {
            GameObject effects = new GameObject("Theme Impact Particles");
            effects.transform.SetParent(parent, false);
            ParticleSystem particles = effects.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.maxParticles = 64;
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystemRenderer renderer = effects.GetComponent<ParticleSystemRenderer>();
            renderer.sortingOrder = 8;
        }

        private static void CreateGameplayAudio(Transform parent)
        {
            GameObject audioObject = new GameObject("Theme Gameplay Audio");
            audioObject.transform.SetParent(parent, false);
            AudioSource source = audioObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
        }

        /// Adding the module binds it to the Input System package's own default actions, which sit
        /// in a package and cannot be edited. Points it at ours instead.
        /// Pads arriving and leaving are a device concern, so this watches them beside the seats
        /// rather than inside them.
        private static void CreateSeatDeviceWatcher(SeatDirector seats)
        {
            SeatDeviceWatcher watcher = Ensure<SeatDeviceWatcher>(seats.gameObject);
            SerializedObject serialized = new SerializedObject(watcher);
            SetReference(serialized, "seats", seats);
            SetReference(serialized, "match", Object.FindAnyObjectByType<MatchController>());
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// Pause and restart read input, and the match must not. They live beside it instead.
        private static void CreateMatchShortcuts(InputActionAsset controls)
        {
            MatchController match = Object.FindAnyObjectByType<MatchController>();
            MatchShortcuts shortcuts = Ensure<MatchShortcuts>(match.gameObject);
            SerializedObject serialized = new SerializedObject(shortcuts);
            SetReference(serialized, "match", match);
            SetReference(serialized, "controls", controls);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateEventSystem(InputActionAsset controls)
        {
            GameObject eventSystemObject = new GameObject("Event System");
            EventSystem eventSystem = eventSystemObject.AddComponent<EventSystem>();
            eventSystem.sendNavigationEvents = true;
            InputSystemUIInputModule module = eventSystemObject.AddComponent<InputSystemUIInputModule>();

            // the module's setters copy each reference into the scene instead of pointing at the
            // asset, and drop the assignment outright once the action already matches
            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(ControlsPath);
            SerializedObject serialized = new SerializedObject(module);
            SetReference(serialized, "m_ActionsAsset", controls);
            SetReference(serialized, "m_PointAction", FindActionReference(subAssets, "Point"));
            SetReference(serialized, "m_MoveAction", FindActionReference(subAssets, "Navigate"));
            SetReference(serialized, "m_LeftClickAction", FindActionReference(subAssets, "Click"));
            SetReference(serialized, "m_RightClickAction", FindActionReference(subAssets, "RightClick"));
            SetReference(serialized, "m_MiddleClickAction", FindActionReference(subAssets, "MiddleClick"));
            SetReference(serialized, "m_ScrollWheelAction", FindActionReference(subAssets, "ScrollWheel"));
            SetReference(serialized, "m_SubmitAction", FindActionReference(subAssets, "Submit"));
            SetReference(serialized, "m_CancelAction", FindActionReference(subAssets, "Cancel"));
            SetReference(serialized, "m_TrackedDevicePositionAction",
                FindActionReference(subAssets, "TrackedDevicePosition"));
            SetReference(serialized, "m_TrackedDeviceOrientationAction",
                FindActionReference(subAssets, "TrackedDeviceOrientation"));
            serialized.ApplyModifiedProperties();
        }

        /// Finds the importer's reference for one UI action, skipping the hidden duplicate it keeps
        /// for backwards compatibility.
        private static InputActionReference FindActionReference(Object[] subAssets, string actionName)
        {
            foreach (Object candidate in subAssets)
            {
                if (candidate is not InputActionReference reference ||
                    (reference.hideFlags & HideFlags.HideInHierarchy) != 0)
                {
                    continue;
                }

                InputAction action = reference.action;
                if (action != null && action.actionMap.name == "UI" && action.name == actionName)
                {
                    return reference;
                }
            }

            // the module matches actions by name and leaves a null rather than failing, so a rename
            // here would cost a kind of menu input in silence
            Debug.LogError(
                $"{ControlsPath} has no UI/{actionName} action. Its UI map must name every action " +
                "exactly as the Input System default does, or the UI module cannot bind to it.");
            return null;
        }

        private static void RemoveLegacyUi()
        {
            foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name is "UI" or "Game UI" or "Event System")
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        /// Pulls the camera back so the court no longer runs edge to edge and the HUD has margin
        /// to live in. Nothing in the world moves, so the match plays exactly as before.
        ///
        /// ArenaFraming takes it from here at runtime, where the window's shape is known. The size
        /// set here is what a 16:9 window keeps, and what the editor shows before play.
        private static void FrameArena()
        {
            Camera camera = FindComponent<Camera>("Main Camera");
            camera.orthographicSize = CameraSize;
            Ensure<ArenaFraming>(camera.gameObject);
            EditorUtility.SetDirty(camera);
        }

        /// The court gains an attacker column ahead of each goalkeeper. The goalkeepers stay exactly
        /// where they were, so a one-per-side lineup is the same match it has always been.
        private static SeatDirector CreateSeats(
            InputProfileCatalog profiles,
            InputActionAsset controls,
            out PaddleSeat[] seats
        )
        {
            GameObject keeperLeft = GameObject.Find("Player Paddle");
            GameObject keeperRight = GameObject.Find("Computer Paddle");

            seats = new[]
            {
                ConfigureSeat(keeperLeft, PlayerSide.Left, SeatRole.Goalkeeper),
                ConfigureSeat(
                    ClonePaddle(keeperLeft, "Left Attacker", AttackerColumn * -1f),
                    PlayerSide.Left,
                    SeatRole.Attacker
                ),
                ConfigureSeat(keeperRight, PlayerSide.Right, SeatRole.Goalkeeper),
                ConfigureSeat(
                    ClonePaddle(keeperRight, "Right Attacker", AttackerColumn),
                    PlayerSide.Right,
                    SeatRole.Attacker
                )
            };

            GameObject directorObject = GameObject.Find("Seats");
            if (directorObject == null)
            {
                directorObject = new GameObject("Seats");
            }

            directorObject.transform.SetParent(GameObject.Find("Gameplay").transform, false);
            SeatDirector director = Ensure<SeatDirector>(directorObject);

            SerializedObject serialized = new SerializedObject(director);
            SetReference(serialized, "profiles", profiles);
            SetReference(serialized, "controls", controls);
            SerializedProperty seatProperty = serialized.FindProperty("seats");
            seatProperty.arraySize = seats.Length;
            for (int index = 0; index < seats.Length; index++)
            {
                seatProperty.GetArrayElementAtIndex(index).objectReferenceValue = seats[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return director;
        }

        private static GameObject ClonePaddle(GameObject source, string name, float column)
        {
            Transform existing = source.transform.parent.Find(name);
            GameObject clone = existing == null
                ? Object.Instantiate(source, source.transform.parent)
                : existing.gameObject;
            clone.name = name;
            clone.transform.localPosition = new Vector3(column, 0f, source.transform.localPosition.z);
            return clone;
        }

        private static PaddleSeat ConfigureSeat(GameObject paddle, PlayerSide side, SeatRole role)
        {
            PlayerPaddleInput human = Ensure<PlayerPaddleInput>(paddle);
            ComputerPaddleController computer = Ensure<ComputerPaddleController>(paddle);

            SerializedObject computerSerialized = new SerializedObject(computer);
            SetReference(computerSerialized, "ball", FindComponent<BallController>("Ball"));
            computerSerialized.ApplyModifiedPropertiesWithoutUndo();

            PaddleMovement movement = paddle.GetComponent<PaddleMovement>();
            SerializedObject movementSerialized = new SerializedObject(movement);
            SetFloat(movementSerialized, "fullLength", PaddleLength);
            SetFloat(movementSerialized, "arenaHalfHeight", ArenaHalfHeight);
            movementSerialized.ApplyModifiedPropertiesWithoutUndo();

            // author the size into the scene too, so the editor shows the paddle the game will use
            Vector3 scale = paddle.transform.localScale;
            paddle.transform.localScale = new Vector3(PaddleWidth, PaddleLength, scale.z);

            // the attackers were cloned from the goalkeepers and carry a copy of their glow child
            RemoveChild(paddle, "Player Glow");
            RemoveChild(paddle, "Computer Glow");

            SpriteRenderer renderer = paddle.GetComponent<SpriteRenderer>();
            PaddleSeat seat = Ensure<PaddleSeat>(paddle);
            SerializedObject serialized = new SerializedObject(seat);
            SetEnum(serialized, "side", side);
            SetEnum(serialized, "role", role);
            SetReference(serialized, "humanInput", human);
            SetReference(serialized, "computerInput", computer);
            SetReference(serialized, "paddleRenderer", renderer);
            SetReference(serialized, "glowRenderer",
                CreateGlow(renderer, "Paddle Glow", new Vector3(1.85f, 1.16f, 1f)));
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return seat;
        }

        private static void WirePresentation(GamePresentation presentation, PaddleSeat[] seats)
        {
            SpriteRenderer ball = FindComponent<SpriteRenderer>("Ball");
            SerializedObject serialized = new SerializedObject(presentation);
            SetReference(serialized, "gameplayCamera", FindComponent<Camera>("Main Camera"));
            SetReference(serialized, "ballController", FindComponent<BallController>("Ball"));
            SetReference(serialized, "ballRenderer", ball);
            SetReference(serialized, "ballGlow", CreateGlow(ball, "Ball Glow", new Vector3(1.9f, 1.9f, 1f)));

            SerializedProperty paddleProperty = serialized.FindProperty("paddles");
            paddleProperty.arraySize = seats.Length;
            for (int index = 0; index < seats.Length; index++)
            {
                paddleProperty.GetArrayElementAtIndex(index).objectReferenceValue = seats[index];
            }
            SetReference(serialized, "topWall", FindComponent<SpriteRenderer>("Top Wall"));
            SetReference(serialized, "bottomWall", FindComponent<SpriteRenderer>("Bottom Wall"));
            SetReference(serialized, "impactParticles", presentation.GetComponentInChildren<ParticleSystem>());
            SetReference(serialized, "gameplayAudioSource",
                presentation.transform.Find("Theme Gameplay Audio").GetComponent<AudioSource>());

            SpriteRenderer[] dashes = GameObject.Find("Center Line")
                .GetComponentsInChildren<SpriteRenderer>()
                .OrderBy(renderer => renderer.name)
                .ToArray();
            SerializedProperty dashProperty = serialized.FindProperty("centerDashes");
            dashProperty.arraySize = dashes.Length;
            for (int index = 0; index < dashes.Length; index++)
            {
                dashProperty.GetArrayElementAtIndex(index).objectReferenceValue = dashes[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SpriteRenderer CreateGlow(SpriteRenderer source, string name, Vector3 scale)
        {
            Transform existing = source.transform.Find(name);
            GameObject glowObject = existing == null ? new GameObject(name) : existing.gameObject;
            glowObject.transform.SetParent(source.transform, false);
            glowObject.transform.localPosition = Vector3.zero;
            glowObject.transform.localRotation = Quaternion.identity;
            glowObject.transform.localScale = scale;
            SpriteRenderer glow = glowObject.GetComponent<SpriteRenderer>();
            if (glow == null)
            {
                glow = glowObject.AddComponent<SpriteRenderer>();
            }

            glow.sprite = source.sprite;
            glow.sharedMaterial = source.sharedMaterial;
            glow.sortingOrder = source.sortingOrder - 1;
            glow.enabled = false;
            return glow;
        }

        private static void WireController(
            GameUiController controller,
            SeatDirector seats,
            GameModeCatalog gameModes,
            ThemeCatalog themes,
            CosmeticCatalog cosmetics,
            InputActionAsset controls
        )
        {
            SerializedObject serialized = new SerializedObject(controller);
            SetReference(serialized, "match", Object.FindAnyObjectByType<MatchController>());
            SetReference(serialized, "seats", seats);
            SetReference(serialized, "gameModes", gameModes);
            SetReference(serialized, "themes", themes);
            SetReference(serialized, "cosmetics", cosmetics);
            SetReference(serialized, "presentation", controller.GetComponent<GamePresentation>());
            SetReference(serialized, "controls", controls);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void RemoveChild(GameObject parent, string name)
        {
            Transform child = parent.transform.Find(name);
            if (child != null)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }

        // explicit null check: Unity's == operator, not ??, is what understands a missing component
        private static T Ensure<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component == null ? target.AddComponent<T>() : component;
        }

        private static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Material CreateParticleMaterial(string path)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ??
                    Shader.Find("Sprites/Default");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.name = System.IO.Path.GetFileNameWithoutExtension(path);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetMode(
            SerializedProperty property,
            string id,
            string displayName,
            string description,
            string playerSummary,
            bool available
        )
        {
            property.FindPropertyRelative("id").stringValue = id;
            property.FindPropertyRelative("displayName").stringValue = displayName;
            property.FindPropertyRelative("description").stringValue = description;
            property.FindPropertyRelative("playerSummary").stringValue = playerSummary;
            property.FindPropertyRelative("available").boolValue = available;
        }

        private static void SetCosmetic(
            SerializedProperty property,
            string themeId,
            string id,
            string displayName,
            string description,
            CosmeticCategory category,
            Color primary,
            Color secondary,
            float effectIntensity
        )
        {
            property.FindPropertyRelative("themeId").stringValue = themeId;
            property.FindPropertyRelative("id").stringValue = id;
            property.FindPropertyRelative("displayName").stringValue = displayName;
            property.FindPropertyRelative("description").stringValue = description;
            property.FindPropertyRelative("category").enumValueIndex = (int)category;
            property.FindPropertyRelative("primaryColor").colorValue = primary;
            property.FindPropertyRelative("secondaryColor").colorValue = secondary;
            property.FindPropertyRelative("unlocked").boolValue = true;
            property.FindPropertyRelative("effectIntensity").floatValue = effectIntensity;
        }

        private static void SetCopy(
            SerializedObject serialized,
            params (string Element, string Icon, string Text)[] entries
        )
        {
            SerializedProperty list = serialized.FindProperty("copy");
            list.arraySize = entries.Length;
            for (int index = 0; index < entries.Length; index++)
            {
                SerializedProperty entry = list.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("element").stringValue = entries[index].Element;
                entry.FindPropertyRelative("icon").stringValue = entries[index].Icon;
                entry.FindPropertyRelative("text").stringValue = entries[index].Text;
            }
        }

        private static void SetString(SerializedObject serialized, string propertyName, string value)
        {
            serialized.FindProperty(propertyName).stringValue = value;
        }

        private static void SetStringArray(SerializedObject serialized, string propertyName, params string[] values)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            property.arraySize = values.Length;
            for (int index = 0; index < values.Length; index++)
            {
                property.GetArrayElementAtIndex(index).stringValue = values[index];
            }
        }

        private static void SetColor(SerializedObject serialized, string propertyName, Color value)
        {
            serialized.FindProperty(propertyName).colorValue = value;
        }

        private static void SetFloat(SerializedObject serialized, string propertyName, float value)
        {
            serialized.FindProperty(propertyName).floatValue = value;
        }

        private static void SetInt(SerializedObject serialized, string propertyName, int value)
        {
            serialized.FindProperty(propertyName).intValue = value;
        }

        private static void SetEnum<T>(SerializedObject serialized, string propertyName, T value) where T : System.Enum
        {
            serialized.FindProperty(propertyName).enumValueIndex = System.Convert.ToInt32(value);
        }

        private static void SetReference(SerializedObject serialized, string propertyName, Object value)
        {
            serialized.FindProperty(propertyName).objectReferenceValue = value;
        }

        private static T FindComponent<T>(string objectName) where T : Component
        {
            return GameObject.Find(objectName).GetComponent<T>();
        }

        private static Color Hex(string value)
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color color);
            return color;
        }
    }
}
