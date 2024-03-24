using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monster_Spawner.Monsters;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace Monster_Spawner.MonsterMenu
{
    internal class MonsterSelectionTabNew : IClickableMenu
    {
        private readonly List<ClickableComponent> clickableComponents = new List<ClickableComponent>();
        private readonly int maxColums = 20;
        private readonly IModHelper Helper;
        private readonly int maxRows = 20;
        public int itemsPerPage = 35;
        private int currentPageIndex = 0;
        private readonly int defaultTextureWidth = 8 * 4; //Account for texture scaling
        private readonly int defaultTextureHeight = 8 * 4;
        private readonly int clearing = 8;

        public MonsterSelectionTabNew(int x, int y, int width, int height, IModHelper helper) :
            base(x, y, width, height)
        {
            LayoutMonsters(MonsterData.ToClickableMonsterComponents());
            this.Helper = helper;
        }

        private void LayoutMonsters(List<ClickableMonsterComponent> components)
        {
            // Clear existing clickable components to prepare for new page layout
            clickableComponents.Clear();

            // Calculate the start and end indices for the monsters to display on the current page
            int start = currentPageIndex * itemsPerPage;
            int end = Math.Min(start + itemsPerPage, components.Count);

            // Reset layout positions
            int colum = 0;
            int row = 0;

            // Create a new layout for the current page
            ClickableMonsterComponent[,] newPageLayout = new ClickableMonsterComponent[maxColums, maxRows];

            // Adjust layout for the current page
            for (int index = start; index < end; index++)
            {
                // Get the current monster component from the list
                ClickableMonsterComponent component = components[index];

                // Find the next available space
                while (SpaceOccupied(newPageLayout, colum, row, component))
                {
                    ++colum;
                    if (colum >= maxColums)
                    {
                        colum = 0;
                        ++row;
                        if (row >= maxRows)
                        {
                            throw new Exception("Ran out of space while laying out monsters on the current page!");
                        }
                    }
                }

                // Assign the monster to the layout and mark the occupied space
                MarkSpaceOccupied(newPageLayout, colum, row, component);

                // Set the bounds for this component based on its position
                int sideBorderClearance = xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth - 16;
                int topBorderClearance = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 16;
                component.bounds = new Rectangle(sideBorderClearance + colum * (defaultTextureWidth + clearing), topBorderClearance + row * (defaultTextureHeight + clearing), component.bounds.Width, component.bounds.Height);

                // Add the component to the list of clickable components for this page
                clickableComponents.Add(component);
            }
        }

        private void MarkSpaceOccupied(ClickableMonsterComponent[,] pageLayout, int x, int y, ClickableMonsterComponent component)
        {
            // Mark the space as occupied based on the size of the monster (similar to your existing logic)
            pageLayout[x, y] = component;
            if (component.HeightLevel >= 2)
            {
                pageLayout[x + 1, y] = component;
                pageLayout[x, y + 1] = component;
                pageLayout[x + 1, y + 1] = component;
                if (component.HeightLevel >= 3)
                {
                    pageLayout[x, y + 2] = component;
                    pageLayout[x + 1, y + 2] = component;
                    if (component.HeightLevel >= 4)
                    {
                        pageLayout[x, y + 3] = component;
                        pageLayout[x + 1, y + 3] = component;
                        if (component.WidthLevel >= 4)
                        {
                            // Extend to the right for wider components
                            for (int i = 0; i < 4; i++) // 4x4 block for 32x32 size
                            {
                                for (int j = 0; j < 4; j++)
                                {
                                    if (x + i < maxColums && y + j < maxRows)
                                    {
                                        pageLayout[x + i, y + j] = component;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        private bool SpaceOccupied(ClickableMonsterComponent[,] pageLayout, int x, int y, ClickableMonsterComponent component)
        {
            //Height Level 1: 8x8
            if (pageLayout[x, y] != null)
            {
                return true;
            }

            //Height Level 2: 16x16
            if (component.HeightLevel >= 2)
            {
                if (y < maxRows - 1 && x < maxColums - 1)
                {
                    if (pageLayout[x + 1, y] != null || pageLayout[x, y + 1] != null || pageLayout[x + 1, y + 1] != null)
                    {
                        return true;
                    }
                    else
                    {
                        //Height Level 3: 16x24
                        if (component.HeightLevel >= 3)
                        {
                            if (y < maxRows - 2)
                            {
                                if (pageLayout[x, y + 2] != null || pageLayout[x + 1, y + 2] != null)
                                {
                                    return true;
                                }
                                else
                                {
                                    //Height Level 4: 16x32
                                    if (component.HeightLevel >= 4)
                                    {
                                        if (y < maxRows - 3)
                                        {
                                            if (pageLayout[x, y + 3] != null || pageLayout[x + 1, y + 3] != null)
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                //Width Level 4: 32x32
                                                if (component.WidthLevel >= 4)
                                                {
                                                    if (x < maxColums - 3)
                                                    {
                                                        if (pageLayout[x + 2, y] != null || pageLayout[x + 2, y + 1] != null || pageLayout[x + 2, y + 2] != null || pageLayout[x + 2, y + 3] != null
                                                            || pageLayout[x + 3, y] != null || pageLayout[x + 3, y + 1] != null || pageLayout[x + 3, y + 2] != null || pageLayout[x + 3, y + 3] != null)
                                                        {
                                                            return true;
                                                        }
                                                        else
                                                        {
                                                            return false;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        return true;
                                                    }
                                                } //Break here, component is 16x32
                                            }
                                        }
                                        else
                                        {
                                            return true;
                                        }
                                    } // Break here, component is 16x24
                                }
                            }
                            else
                            {
                                return true;
                            }
                        } //Break here, component is 16x16
                    }
                }
                else
                {
                    return true;
                }
            } //Break here, component is 8x8 (doesnt exist, but just to be consistent)

            return false;
        }

        public void ChangePage(int pageIndex)
        {
            this.currentPageIndex = pageIndex;
            LayoutMonsters(MonsterData.ToClickableMonsterComponents()); // Assuming this is how you get your monsters
        }



        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            foreach (ClickableMonsterComponent component in clickableComponents)
            {
                component.Draw(b);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            foreach (ClickableComponent component in clickableComponents)
            {
                if (component.GetType() == typeof(ClickableMonsterComponent))
                {
                    ClickableMonsterComponent m = (ClickableMonsterComponent)component;
                    m.PerformHoverAction(x, y);
                }
            }
        }

        public int CalculateMaxPages()
        {
            int totalItems = MonsterData.ToClickableMonsterComponents().Count;
            return (int)Math.Ceiling((double)totalItems / itemsPerPage);
        }



        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (isWithinBounds(x, y))
            {

                foreach (ClickableComponent monster in clickableComponents)
                {
                    if (monster.GetType() == typeof(ClickableMonsterComponent) && monster.containsPoint(x, y))
                    {
                        ClickableMonsterComponent m = (ClickableMonsterComponent)monster;
                        Game1.activeClickableMenu = new MonsterPlaceMenu(m.Monster, m.Sprite, this.Helper);
                    }
                }
                base.receiveLeftClick(x, y, true);

            }
            else
            {
                Game1.exitActiveMenu();
            }
        }
    }
}
