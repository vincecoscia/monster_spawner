using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monster_Spawner.Common;
using Monster_Spawner.Common.UI;
using Monster_Spawner.Monsters;
using Monster_Spawner.MonsterSpawning;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;

namespace Monster_Spawner.MonsterMenu
{
    /// <summary>
    /// Represents a menu for selecting a monster to spawn.
    /// </summary>
    internal class MonsterMenu : IClickableMenu
    {
        private readonly List<TabComponent> tabComponents;
        private readonly List<IClickableMenu> tabs;
        private int current;
        private readonly List<OptionsElement> Options = new();

        private const int menuWidth = 800;
        private const int menuHeight = 600;
        private ClickableTextureComponent UpArrow;
        private ClickableTextureComponent DownArrow;
        private ClickableTextureComponent Scrollbar;
        private bool IsScrolling;
        private Rectangle ScrollbarRunner;
        private int CurrentItemIndex;
        private readonly IModHelper Helper;
        private ClickableComponent Title;
        private ClickableComponent DropdownText;
        private Dropdown<int> quantityDropdown;
        private ClickableComponent clearMonstersButton;


        public MonsterMenu(IModHelper helper)
            : base(
                  // Postion the menu in the center of the screen
                  (int)(Game1.uiViewport.Width / 2 - ((menuWidth + IClickableMenu.borderWidth * 0.5f) / 2)),
                  Game1.uiViewport.Height / 2 - (menuHeight + IClickableMenu.borderWidth * 2) / 2,
                  // Menu Height and Width
                  menuWidth + IClickableMenu.borderWidth * 2,
                  menuHeight + IClickableMenu.borderWidth * 2, true)
        {
            this.Helper = helper;

            Game1.playSound("bigSelect");

            int dropdownX = this.xPositionOnScreen - 180; // Offset to the left
            int dropdownY = this.yPositionOnScreen + 160; // Slightly below the top of the menu

            // Initialize the dropdown
            var quantityOptions = new int[] { 1, 5, 10, 25, 100 };
            this.quantityDropdown = new Dropdown<int>(
                x: dropdownX,
                y: dropdownY,
                font: Game1.dialogueFont,
                selectedItem: 1,
                items: quantityOptions,
                getLabel: value => value.ToString()
            );

            this.clearMonstersButton = new ClickableComponent(new Rectangle(this.xPositionOnScreen - 90, this.yPositionOnScreen + menuHeight -40, Game1.tileSize * 4, Game1.tileSize), "Clear All");


            tabs = new List<IClickableMenu>();
            tabComponents = new List<TabComponent>();

            // add title
            this.Title = new ClickableComponent(new Rectangle(this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen, Game1.tileSize * 4, Game1.tileSize), "Monster Spawner");

            // add quantity dropdown
            this.DropdownText = new ClickableComponent(new Rectangle(this.xPositionOnScreen - 90, this.yPositionOnScreen + 80, Game1.tileSize * 4, Game1.tileSize), "Quantity");

            // add scroll UI
            int scrollbarOffset = Game1.tileSize * (4) / 16;
            this.UpArrow = new ClickableTextureComponent("up-arrow", new Rectangle(this.xPositionOnScreen + this.width + scrollbarOffset, this.yPositionOnScreen + Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 459, 11, 12), Game1.pixelZoom);
            this.DownArrow = new ClickableTextureComponent("down-arrow", new Rectangle(this.xPositionOnScreen + this.width + scrollbarOffset, this.yPositionOnScreen + this.height - Game1.tileSize, 11 * Game1.pixelZoom, 12 * Game1.pixelZoom), "", "", Game1.mouseCursors, new Rectangle(421, 472, 11, 12), Game1.pixelZoom);
            
            tabs.Add(new MonsterSelectionTabNew(xPositionOnScreen, yPositionOnScreen, width, height, this.Helper, this.GetCurrentQuantity));

            //tabs.Add(new MonsterSettingsTab(xPositionOnScreen, yPositionOnScreen, width, height));

            tabComponents.Add(new TabComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "Monsters"));
            //tabComponents.Add(new TabComponent(new Rectangle(this.xPositionOnScreen + 128, this.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "Settings"));

            current = 0;
            initializeUpperRightCloseButton();
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);




            if (!Game1.options.showMenuBackground)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            }

            CommonHelper.DrawTab(this.Title.bounds.X, this.Title.bounds.Y, Game1.dialogueFont, this.Title.name, 1);

            CommonHelper.DrawTab(this.DropdownText.bounds.X, this.DropdownText.bounds.Y, Game1.dialogueFont, this.DropdownText.name, 1);

            this.quantityDropdown.Draw(b, 1f);

            // Draw the clear monsters button
            CommonHelper.DrawTab(this.clearMonstersButton.bounds.X, this.clearMonstersButton.bounds.Y, Game1.dialogueFont, this.clearMonstersButton.name, 1);

            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, tabs[current].width, tabs[current].height, false, true);
            tabs[current].draw(b);

            MonsterSelectionTabNew currentTab = tabs[current] as MonsterSelectionTabNew;
            int maxPages = currentTab != null ? currentTab.CalculateMaxPages() : 1;

            // Draw the UpArrow only if not on the first page
            if (this.CurrentItemIndex > 0)
            {
                this.UpArrow.draw(b);
            }

            // Draw the DownArrow only if not on the last page
            if (this.CurrentItemIndex < maxPages - 1)
            {
                this.DownArrow.draw(b);
            }

            drawMouse(b);
        }



        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {

            // Handle clicks on the dropdown first
            if (this.quantityDropdown.TryClick(x, y, out bool itemClicked, out bool dropdownToggled))
            {
                // If an item was clicked or the dropdown was toggled, return early to consume the click.
                // You may also handle the change in selection or dropdown state here if needed.
                if (itemClicked || dropdownToggled)
                {
                    // Example: Update something based on the new selection
                    // Remember, dropdown selection changes are already handled within the dropdown itself.
                    return;
                }

            }

            // Check if the clear monsters button was clicked
            if (this.clearMonstersButton.containsPoint(x, y))
            {
                // Call the ClearMonsters method
                Spawner.GetInstance().ClearMonsters();

                if (playSound)
                {
                    Game1.playSound("bigDeSelect"); // Play a sound for feedback
                }

                // Close the menu
                Game1.exitActiveMenu();

                Game1.addHUDMessage(new HUDMessage("Cleared all monsters!", 2));

                return; // Consume the click to prevent further processing
            }


            MonsterSelectionTabNew currentTab = tabs[current] as MonsterSelectionTabNew;
            int totalItems = currentTab != null ? MonsterData.ToClickableMonsterComponents().Count : 0;
            int itemsPerPage = currentTab?.itemsPerPage ?? 10; // Make sure this matches the logic in drawing and updating pages

            // Replace this.Options.Count with totalItems
            if (totalItems > itemsPerPage)
            {
                if (this.UpArrow.containsPoint(x, y))
                {
                    this.UpArrowPressed();
                    return; // Consume the click, so it doesn't close the menu
                }
                else if (this.DownArrow.containsPoint(x, y))
                {
                    this.DownArrowPressed();
                    return; // Consume the click, so it doesn't close the menu
                }
                // Add logic here if you want to handle clicking on the scrollbar itself
            }

            // Then handle other clicks
            for (int i = 0; i < tabComponents.Count; i++)
            {
                if (tabComponents[i].containsPoint(x, y))
                {
                    if (i != current)
                    {
                        current = i;
                        Game1.playSound("smallSelect");
                    }
                    return;
                }
            }

            if (isWithinBounds(x, y))
            {
                tabs[current].receiveLeftClick(x, y, playSound);
            }
            else
            {
                // If the click is outside menu bounds but not on arrows, don't close the menu; just ignore it.
                return;
            }

            // Call base method only if the click is not on any specific interactive components
            base.receiveLeftClick(x, y, playSound);
        }

        public int GetCurrentQuantity()
        {
            return this.quantityDropdown.Selected;
        }



        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            tabs[current].performHoverAction(x, y);
        }

        private void UpArrowPressed()
        {
            MonsterSelectionTabNew currentTab = tabs[current] as MonsterSelectionTabNew;
            if (currentTab == null) return;

            int maxPages = currentTab.CalculateMaxPages();
            if (this.CurrentItemIndex > 0)
            {
                this.CurrentItemIndex--;
                currentTab.ChangePage(this.CurrentItemIndex);
            }
        }


        private void DownArrowPressed()
        {
            MonsterSelectionTabNew currentTab = tabs[current] as MonsterSelectionTabNew;
            if (currentTab == null) return;

            int maxPages = currentTab.CalculateMaxPages();
            if (this.CurrentItemIndex < maxPages - 1)
            {
                this.CurrentItemIndex++;
                currentTab.ChangePage(this.CurrentItemIndex);
            }
        }


    }
}
