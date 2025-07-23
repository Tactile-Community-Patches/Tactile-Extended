using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Tactile
{
	class Map_Background : Stereoscopic_Graphic_Object
    {
        private Texture2D Texture;
        private float Parallax_Factor;
        private int X_velocity;
        private int Y_velocity;
        private int X_displacement = 0;
        private int Y_displacement = 0;
        private int X_timer = 0;
        private int Y_timer = 0;

        public Texture2D texture { get { return Texture; } }
		#region Serialization
		public void write(BinaryWriter writer)
        {

        }
        public void read(BinaryReader reader)
        {

        }
		#endregion
		public Map_Background(string filename, int x_velocity, int y_velocity, int parallax_factor)
        {
            Texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + filename);
            Parallax_Factor = (float)parallax_factor / 100f;
            X_velocity = x_velocity;
            Y_velocity = y_velocity;
        }

        public void update()
        {
            X_timer++;
            Y_timer++;
            if (X_timer == X_velocity)
            {
                X_timer = 0;
                X_displacement += 1;
            }
            if (X_displacement % Texture.Width == 0)
            {
                X_displacement = 0;
            }
            if (Y_timer == Y_velocity)
            {
                Y_timer = 0;
                Y_displacement += 1;
            }
            if (Y_displacement % Texture.Height == 0)
            {
                Y_displacement = 0;
            }
        }
        public void draw(SpriteBatch spriteBatch)
        {
            update(); // fix this shit

            spriteBatch.Draw(this.Texture, new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT), new Rectangle((int)(Global.game_map.display_x/2* Parallax_Factor) + X_displacement, (int)(Parallax_Factor * Global.game_map.display_y/2 * Config.WINDOW_WIDTH/ Config.WINDOW_HEIGHT) + Y_displacement, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT), Color.White);
        }
    }
}