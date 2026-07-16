using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pong
{
    public sealed class SettingsView
    {
        private readonly GameSettings settings;
        private readonly Action settingsChanged;
        private readonly Action<Resolution> resolutionChanged;
        private readonly List<Resolution> resolutions = new List<Resolution>();
        private readonly Label pointsValue;
        private readonly Label speedValue;
        private readonly Label masterValue;
        private readonly Label musicValue;
        private readonly Label sfxValue;
        private readonly Label scaleValue;

        public SettingsView(
            VisualElement root,
            GameSettings settings,
            Action settingsChanged,
            Action<Resolution> resolutionChanged
        )
        {
            this.settings = settings;
            this.settingsChanged = settingsChanged;
            this.resolutionChanged = resolutionChanged;

            pointsValue = root.Q<Label>("points-value");
            speedValue = root.Q<Label>("speed-value");
            masterValue = root.Q<Label>("master-value");
            musicValue = root.Q<Label>("music-value");
            sfxValue = root.Q<Label>("sfx-value");
            scaleValue = root.Q<Label>("scale-value");

            BindGameplay(root);
            BindAudio(root);
            BindGraphics(root);
            BindAccessibility(root);
            RefreshLabels();
        }

        private void BindGameplay(VisualElement root)
        {
            SliderInt points = root.Q<SliderInt>("points-slider");
            points.SetValueWithoutNotify(settings.PointsToWin);
            points.RegisterValueChangedCallback(change =>
            {
                settings.PointsToWin = change.newValue;
                Commit();
            });

            Slider speed = root.Q<Slider>("speed-slider");
            speed.SetValueWithoutNotify(settings.GameSpeed);
            speed.RegisterValueChangedCallback(change =>
            {
                settings.GameSpeed = change.newValue;
                Commit();
            });
        }

        private void BindAudio(VisualElement root)
        {
            BindVolume(root.Q<Slider>("master-slider"),
                settings.MasterVolume,
                value => settings.MasterVolume = value);
            BindVolume(root.Q<Slider>("music-slider"),
                settings.MusicVolume,
                value => settings.MusicVolume = value);
            BindVolume(root.Q<Slider>("sfx-slider"),
                settings.SfxVolume,
                value => settings.SfxVolume = value);
        }

        private void BindGraphics(VisualElement root)
        {
            Toggle fullscreen = root.Q<Toggle>("fullscreen-toggle");
            fullscreen.SetValueWithoutNotify(settings.Fullscreen);
            fullscreen.RegisterValueChangedCallback(change =>
            {
                settings.Fullscreen = change.newValue;
                Commit();
            });

            Toggle vsync = root.Q<Toggle>("vsync-toggle");
            vsync.SetValueWithoutNotify(settings.VSync);
            vsync.RegisterValueChangedCallback(change =>
            {
                settings.VSync = change.newValue;
                Commit();
            });

            DropdownField resolution = root.Q<DropdownField>("resolution-dropdown");
            HashSet<string> seen = new HashSet<string>();
            foreach (Resolution available in Screen.resolutions)
            {
                string label = $"{available.width} × {available.height}";
                if (seen.Add(label))
                {
                    resolutions.Add(available);
                    resolution.choices.Add(label);
                }
            }

            if (resolutions.Count == 0)
            {
                resolution.choices.Add($"{Screen.width} × {Screen.height}");
                resolution.SetEnabled(false);
            }
            else
            {
                int index = settings.ResolutionIndex;
                if (index < 0 || index >= resolutions.Count)
                {
                    index = FindCurrentResolution();
                }

                settings.ResolutionIndex = index;
                resolution.SetValueWithoutNotify(resolution.choices[index]);
                resolution.RegisterValueChangedCallback(change =>
                {
                    int selectedIndex = resolution.choices.IndexOf(change.newValue);
                    if (selectedIndex < 0)
                    {
                        return;
                    }

                    settings.ResolutionIndex = selectedIndex;
                    Commit();
                    resolutionChanged(resolutions[selectedIndex]);
                });
            }
        }

        private void BindAccessibility(VisualElement root)
        {
            Slider scale = root.Q<Slider>("ui-scale-slider");
            scale.SetValueWithoutNotify(settings.UiScale);
            scale.RegisterValueChangedCallback(change =>
            {
                settings.UiScale = change.newValue;
                Commit();
            });

            Toggle reducedMotion = root.Q<Toggle>("reduced-motion-toggle");
            reducedMotion.SetValueWithoutNotify(settings.ReducedMotion);
            reducedMotion.RegisterValueChangedCallback(change =>
            {
                settings.ReducedMotion = change.newValue;
                Commit();
            });
        }

        private void BindVolume(Slider slider, float value, Action<float> setter)
        {
            slider.SetValueWithoutNotify(value);
            slider.RegisterValueChangedCallback(change =>
            {
                setter(change.newValue);
                Commit();
            });
        }

        private int FindCurrentResolution()
        {
            for (int index = 0; index < resolutions.Count; index++)
            {
                if (resolutions[index].width == Screen.width && resolutions[index].height == Screen.height)
                {
                    return index;
                }
            }

            return resolutions.Count - 1;
        }

        private void Commit()
        {
            settings.Save();
            settingsChanged();
            RefreshLabels();
        }

        private void RefreshLabels()
        {
            pointsValue.text = settings.PointsToWin.ToString();
            speedValue.text = $"{settings.GameSpeed:0.00}×";
            masterValue.text = $"{settings.MasterVolume * 100f:0}%";
            musicValue.text = $"{settings.MusicVolume * 100f:0}%";
            sfxValue.text = $"{settings.SfxVolume * 100f:0}%";
            scaleValue.text = $"{settings.UiScale * 100f:0}%";
        }
    }
}
