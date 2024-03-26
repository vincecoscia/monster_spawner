using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monster_Spawner.Monsters;
using Monster_Spawner.MonsterSpawning;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Monster_Spawner
{
    /// <summary>
    /// Represents a menu for placing monsters in SDV.
    /// </summary>
    internal class MonsterPlaceMenu : IClickableMenu
    {
        private readonly ClickableTextureComponent ok;
        private readonly Texture2D placementTile;
        private readonly MonsterData.Monster monster;
        private readonly MonsterData monsterData;

        private readonly AnimatedSprite? monsterTexture;
        private readonly IModHelper Helper;

        public MonsterPlaceMenu(MonsterData.Monster monster, AnimatedSprite texture, IModHelper helper)
            : base(0, 0, Game1.viewport.Width, Game1.viewport.Height)
        {
            this.Helper = helper;

            if (monster != MonsterData.Monster.CursedDoll)
            {
                monsterTexture = texture;
                monsterTexture.CurrentFrame = 0;
            }

            this.monster = monster;
            monsterData = MonsterData.GetMonsterData(monster);
            ok = new ClickableTextureComponent(new Rectangle(16, 16, 60, 60), Game1.mouseCursors, new Rectangle(128, 256, 63, 63), 1f, false);
            placementTile = Game1.content.Load<Texture2D>("LooseSprites\\buildingPlacementTiles");

            //Warn users
            if (monster == MonsterData.Monster.ArmoredBug)
            {
              Game1.addHUDMessage(new HUDMessage("Be aware that armored bugs are unkillable.", 2));
            }
            else if (monster == MonsterData.Monster.Duggy || monster == MonsterData.Monster.MagmaDuggy)
            {
              if (monsterTexture != null)
              {
                monsterTexture.CurrentFrame = 5;
              }
              Game1.addHUDMessage(new HUDMessage("Duggies can only be spawned on diggable tiles.", 2));
            }

            Game1.playSound("bigSelect");
            Game1.addHUDMessage(new HUDMessage($"Click anywhere to spawn a {monsterData.Displayname}", 0));
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (ok.containsPoint(x, y))
            {
                Game1.exitActiveMenu();
                Game1.playSound("bigDeSelect");
                return;
            }
            if (Spawner.GetInstance().SpawnMonster(monster, WhereToPlace()))
            {
                Game1.playSound("axe");
            }
            base.receiveLeftClick(x, y, playSound);
        }


        private Vector2 WhereToPlace()
        {
            ICursorPosition cursorPosition = this.Helper.Input.GetCursorPosition();

            if (monster == MonsterData.Monster.Duggy || monster == MonsterData.Monster.WildernessGolem || monster == MonsterData.Monster.MagmaDuggy)
            {
                // These specific monsters use the grab tile position, which is already in tile coordinates.
                // return cursorPosition.GrabTile * new Vector2(Game1.tileSize, Game1.tileSize);
                // return cursorPosition.Tile * new Vector2(Game1.tileSize, Game1.tileSize);
                return Game1.currentCursorTile;
            }
            else
            {
                // For other types of monsters, we also convert tile coordinates back to pixels.
                return cursorPosition.Tile * new Vector2(Game1.tileSize, Game1.tileSize);
            }
        }


        private Vector2 WhereToDraw() {
            if (monster == MonsterData.Monster.Duggy || monster == MonsterData.Monster.WildernessGolem || monster == MonsterData.Monster.MagmaDuggy) {
                return new Vector2(Game1.getMouseX() - monsterData.Texturewidth, Game1.getMouseY() - monsterData.Textureheight * 2.2f);
            } else {
                return new Vector2(Game1.getMouseX() - monsterData.Texturewidth, Game1.getMouseY() - monsterData.Textureheight * 2.2f);
            }
        }

        public override void draw(SpriteBatch b)
        {
            ICursorPosition cursorPosition = this.Helper.Input.GetCursorPosition();

            Vector2 basePosition = cursorPosition.Tile * new Vector2(Game1.tileSize, Game1.tileSize);

            float scale = Game1.options.zoomLevel;


            Rectangle tileSourceRect = Spawner.IsOkToPlace(monster, Game1.currentCursorTile) ? new Rectangle(0, 0, 64, 64) : new Rectangle(64, 0, 64, 64);

            // Draw the placement tile
            b.Draw(
                placementTile,
                Utility.ModifyCoordinatesForUIScale(Game1.GlobalToLocal(Utility.ModifyCoordinatesForUIScale(basePosition / scale))),
                tileSourceRect,
                Color.White,
                0f, // Rotation
                Vector2.Zero, // Origin
                scale, // Scale
                SpriteEffects.None, // Effects
                0f // Layer depth
            );
            if (monster == MonsterData.Monster.CursedDoll)
            {
              Vector2 vector2 = WhereToDraw();
              b.Draw(Game1.objectSpriteSheet, new Rectangle((int)vector2.X, (int)vector2.Y, 16 * 4, 16 * 4), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 103, 16, 16)), new Color(255, 50, 50));
            }
            else if (monsterTexture != null)
            {
              monsterTexture.draw(b, WhereToDraw(), 1, 0, 0, (monsterData.TextureColor == default ? Color.White : monsterData.TextureColor) * 0.7f, false, 4);
            }

            ok.draw(b);
            drawMouse(b);
        }
    }
}
