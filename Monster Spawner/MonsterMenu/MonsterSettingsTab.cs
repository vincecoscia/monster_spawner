﻿using StardewValley;
using StardewValley.Menus;

namespace Monster_Spawner
{
    internal class MonsterSettingsTab : IClickableMenu
    {
        public MonsterSettingsTab(int x, int y, int width, int height) : base(x, y, width, height)
        {

            if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
            {
                return;
            }

            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }
    }
}
