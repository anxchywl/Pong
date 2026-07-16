using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
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

        [MenuItem("Pong/Setup Game UI")]
        public static void Run()
        {
            UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(ScenePath);
            PanelSettings panelSettings = CreatePanelSettings();
            GameModeCatalog gameModes = CreateGameModes();
            GameTheme futuristic = CreateFuturisticTheme();
            GameTheme retro = CreateRetroTheme();
            ThemeCatalog themes = CreateThemeCatalog(futuristic, retro);
            CosmeticCatalog cosmetics = CreateCosmetics();
            AssetDatabase.SaveAssets();

            RemoveLegacyUi();
            GameObject uiObject = CreateUiObject(panelSettings);
            CreateEventSystem();
            WirePresentation(uiObject.GetComponent<GamePresentation>());
            WireController(uiObject.GetComponent<GameUiController>(), gameModes, themes, cosmetics);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        private static PanelSettings CreatePanelSettings()
        {
            PanelSettings settings = LoadOrCreate<PanelSettings>(PanelSettingsPath);
            settings.name = "GameUiPanelSettings";
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;
            settings.sortingOrder = 10;
            settings.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(
                "Assets/UI Toolkit/UnityThemes/UnityDefaultRuntimeTheme.tss"
            );
            EditorUtility.SetDirty(settings);
            return settings;
        }

        private static GameModeCatalog CreateGameModes()
        {
            GameModeCatalog catalog = LoadOrCreate<GameModeCatalog>(GameModesPath);
            SerializedProperty modes = new SerializedObject(catalog).FindProperty("modes");
            modes.arraySize = 4;
            SetMode(modes.GetArrayElementAtIndex(0), "classic", "Classic",
                "The original duel: you against a focused computer rival.", "1 PLAYER", true);
            SetMode(modes.GetArrayElementAtIndex(1), "practice", "Practice",
                "A pressure-free court for learning timing and angles.", "SOLO", false);
            SetMode(modes.GetArrayElementAtIndex(2), "ai", "AI League",
                "Face distinct opponents with readable play styles.", "1 PLAYER", false);
            SetMode(modes.GetArrayElementAtIndex(3), "local-multiplayer", "Local Multiplayer",
                "Share the court and settle it on one screen.", "2 PLAYERS", false);
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
            SetReference(serialized, "styleSheet", AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/Retro.uss"));
            SetStringArray(serialized, "preferredFontNames", "Menlo", "Consolas", "Courier New");
            SetString(serialized, "menuEyebrow", "INSERT COIN // QUICK MATCH");
            SetString(serialized, "menuTitle", "READY PLAYER ONE");
            SetString(serialized, "menuSubtitle", "First to the target score takes the cabinet.");
            SetString(serialized, "pauseEyebrow", "SYSTEM HOLD");
            SetString(serialized, "pauseTitle", "GAME PAUSED");
            SetString(serialized, "victoryTitle", "PLAYER ONE WINS");
            SetString(serialized, "defeatTitle", "CPU TAKES THE ROUND");
            SetString(serialized, "playerScoreLabel", "P1");
            SetString(serialized, "opponentScoreLabel", "CPU");
            SetString(serialized, "playIcon", "[>]");
            SetString(serialized, "pauseIcon", "[II]");
            SetString(serialized, "backIcon", "[<]");
            SetString(serialized, "gameModeIcon", "[M]");
            SetString(serialized, "skinsIcon", "[S]");
            SetString(serialized, "backgroundIcon", "[B]");
            SetString(serialized, "themeIcon", "[W]");
            SetString(serialized, "settingsIcon", "[*]");
            SetString(serialized, "creditsIcon", "[i]");
            SetString(serialized, "quitIcon", "[X]");
            SetString(serialized, "restartIcon", "[R]");
            SetString(serialized, "homeIcon", "[H]");
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
            SetReference(serialized, "styleSheet",
                AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/UI/Futuristic.uss"));
            SetStringArray(serialized, "preferredFontNames", "Avenir Next", "Segoe UI", "Arial");
            SetString(serialized, "menuEyebrow", "MATCH LINK READY");
            SetString(serialized, "menuTitle", "Enter the arena.");
            SetString(serialized, "menuSubtitle", "Precision play in a calm, responsive competition space.");
            SetString(serialized, "pauseEyebrow", "SIMULATION SUSPENDED");
            SetString(serialized, "pauseTitle", "Awaiting your return.");
            SetString(serialized, "victoryTitle", "VICTORY CONFIRMED");
            SetString(serialized, "defeatTitle", "MATCH COMPLETE");
            SetString(serialized, "playerScoreLabel", "YOU");
            SetString(serialized, "opponentScoreLabel", "RIVAL");
            SetString(serialized, "playIcon", ">");
            SetString(serialized, "pauseIcon", "II");
            SetString(serialized, "backIcon", "<");
            SetString(serialized, "gameModeIcon", "//");
            SetString(serialized, "skinsIcon", "[]");
            SetString(serialized, "backgroundIcon", "::");
            SetString(serialized, "themeIcon", "||");
            SetString(serialized, "settingsIcon", "<>");
            SetString(serialized, "creditsIcon", "..");
            SetString(serialized, "quitIcon", "X");
            SetString(serialized, "restartIcon", "R");
            SetString(serialized, "homeIcon", "H");
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
            items.arraySize = 16;
            int index = 0;

            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-paddle-cabinet", "Cabinet Pair",
                "Warm player amber against a muted phosphor rival.", CosmeticCategory.Paddle,
                Hex("F5B942"), Hex("6FA25C"), 0.35f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-paddle-terminal", "Terminal Pair",
                "A monochrome green set inspired by service terminals.", CosmeticCategory.Paddle,
                Hex("8DBB63"), Hex("C7D89A"), 0.45f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-ball-white", "Pixel White",
                "The clean high-contrast arcade standard.", CosmeticCategory.Ball,
                Hex("F6EECB"), Hex("F6EECB"), 0.3f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-ball-amber", "Amber Dot",
                "A warmer phosphor point with clear motion reads.", CosmeticCategory.Ball,
                Hex("FFD36B"), Hex("F5B942"), 0.5f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-arena-cabinet", "Upright Cabinet",
                "Brass rails and a low-intensity dotted divider.", CosmeticCategory.Arena,
                Hex("D8C97C"), new Color(0.85f, 0.79f, 0.49f, 0.42f), 0.4f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-arena-phosphor", "Phosphor Court",
                "A disciplined green monochrome arena.", CosmeticCategory.Arena,
                Hex("91B96C"), new Color(0.57f, 0.72f, 0.42f, 0.38f), 0.5f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-background-soft-crt", "Soft CRT",
                "Subtle scanlines and a charcoal cabinet interior.", CosmeticCategory.Background,
                Hex("050704"), Hex("161B11"), 0.35f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "retro", "retro-background-sharp-crt", "Sharp CRT",
                "Denser scanlines and deeper black levels.", CosmeticCategory.Background,
                Hex("020302"), Hex("0C100A"), 0.78f);

            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "future-paddle-ceramic", "Ceramic Pair",
                "Soft violet and warm alloy surfaces.", CosmeticCategory.Paddle,
                Hex("D6A8FF"), Hex("FFB07E"), 0.55f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "future-paddle-graphite", "Graphite Pair",
                "Cool white ceramic against quiet graphite.", CosmeticCategory.Paddle,
                Hex("EDF0F7"), Hex("8B91A6"), 0.4f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "future-ball-ion", "Ion Core",
                "A restrained violet core with a soft edge.", CosmeticCategory.Ball,
                Hex("E1C4FF"), Hex("D6A8FF"), 0.65f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "future-ball-pearl", "Pearl",
                "Neutral ceramic white for maximum clarity.", CosmeticCategory.Ball,
                Hex("F6F3FA"), Hex("C9C6D3"), 0.4f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "future-arena-glass", "Glass Circuit",
                "Pale rails above a restrained violet circuit.", CosmeticCategory.Arena,
                Hex("D4D0E4"), new Color(0.84f, 0.66f, 1f, 0.28f), 0.65f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "future-arena-quiet", "Quiet Alloy",
                "Low-glare metal with a nearly invisible center guide.", CosmeticCategory.Arena,
                Hex("A8ADBC"), new Color(0.66f, 0.68f, 0.74f, 0.2f), 0.3f);
            SetCosmetic(items.GetArrayElementAtIndex(index++), "futuristic", "future-background-orbital", "Orbital Dusk",
                "Layered violet depth with sparse interface lines.", CosmeticCategory.Background,
                Hex("080A11"), Hex("21192D"), 0.45f);
            SetCosmetic(items.GetArrayElementAtIndex(index), "futuristic", "future-background-deep-field", "Deep Field",
                "A darker field with more visible ambient circuitry.", CosmeticCategory.Background,
                Hex("05060B"), Hex("15111F"), 0.82f);

            serialized.ApplyModifiedPropertiesWithoutUndo();
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

        private static void CreateEventSystem()
        {
            GameObject eventSystemObject = new GameObject("Event System");
            EventSystem eventSystem = eventSystemObject.AddComponent<EventSystem>();
            eventSystem.sendNavigationEvents = true;
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
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

        private static void WirePresentation(GamePresentation presentation)
        {
            SpriteRenderer player = FindComponent<SpriteRenderer>("Player Paddle");
            SpriteRenderer computer = FindComponent<SpriteRenderer>("Computer Paddle");
            SpriteRenderer ball = FindComponent<SpriteRenderer>("Ball");
            SerializedObject serialized = new SerializedObject(presentation);
            SetReference(serialized, "gameplayCamera", FindComponent<Camera>("Main Camera"));
            SetReference(serialized, "ballController", FindComponent<BallController>("Ball"));
            SetReference(serialized, "playerPaddle", player);
            SetReference(serialized, "computerPaddle", computer);
            SetReference(serialized, "ballRenderer", ball);
            SetReference(serialized, "playerGlow", CreateGlow(player, "Player Glow", new Vector3(1.85f, 1.16f, 1f)));
            SetReference(serialized, "computerGlow", CreateGlow(computer, "Computer Glow", new Vector3(1.85f, 1.16f, 1f)));
            SetReference(serialized, "ballGlow", CreateGlow(ball, "Ball Glow", new Vector3(1.9f, 1.9f, 1f)));
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
            GameModeCatalog gameModes,
            ThemeCatalog themes,
            CosmeticCatalog cosmetics
        )
        {
            SerializedObject serialized = new SerializedObject(controller);
            SetReference(serialized, "match", Object.FindAnyObjectByType<MatchController>());
            SetReference(serialized, "gameModes", gameModes);
            SetReference(serialized, "themes", themes);
            SetReference(serialized, "cosmetics", cosmetics);
            SetReference(serialized, "presentation", controller.GetComponent<GamePresentation>());
            serialized.ApplyModifiedPropertiesWithoutUndo();
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
