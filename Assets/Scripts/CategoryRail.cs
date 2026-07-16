using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Pong
{
    /// A rail of categories showing exactly one panel at a time. Settings and the customization
    /// workshop share it, so both behave identically and neither grows its own tab logic.
    public sealed class CategoryRail
    {
        private readonly List<Button> tabs = new List<Button>();
        private readonly List<VisualElement> panels = new List<VisualElement>();
        private readonly List<string> ids = new List<string>();

        /// Tabs and panels are found by name, so a new category is markup plus a theme copy
        /// entry: no code changes here.
        public CategoryRail(
            VisualElement root,
            string tabPrefix,
            string panelPrefix,
            IReadOnlyList<string> categoryIds,
            Action clickFeedback
        )
        {
            foreach (string id in categoryIds)
            {
                Button tab = root.Q<Button>(tabPrefix + id);
                VisualElement panel = root.Q<VisualElement>(panelPrefix + id);
                if (tab == null || panel == null)
                {
                    continue;
                }

                string captured = id;
                tab.clicked += () =>
                {
                    clickFeedback();
                    Select(captured);
                };

                tabs.Add(tab);
                panels.Add(panel);
                ids.Add(id);
            }

            if (ids.Count > 0)
            {
                Select(ids[0]);
            }
        }

        public string Selected { get; private set; }

        public void Select(string id)
        {
            Selected = id;
            for (int index = 0; index < ids.Count; index++)
            {
                bool active = ids[index] == id;
                tabs[index].EnableInClassList("is-selected", active);
                panels[index].EnableInClassList("is-hidden", !active);
            }
        }
    }
}
