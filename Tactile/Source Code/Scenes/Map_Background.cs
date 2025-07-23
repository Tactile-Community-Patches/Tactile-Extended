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
        private string Filename;
        private double Parallax_Factor;
        private int X_velocity;
        private int Y_velocity;
        private int X_displacement = 0;
        private int Y_displacement = 0;
        private int X_timer = 0;
        private int Y_timer = 0;
        private Rectangle Destination = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);
        private Rectangle Source = new Rectangle(0, 0, Config.WINDOW_WIDTH, Config.WINDOW_HEIGHT);

        public Texture2D texture { get { return Texture; } }
        public double depth { get { return Parallax_Factor; } }
		#region Serialization
		public void write(BinaryWriter writer)
        {
            writer.Write(Filename);
            writer.Write(Parallax_Factor);
            writer.Write(X_velocity);
            writer.Write(Y_velocity);
        }
        public void read(BinaryReader reader)
        {
            Filename = reader.ReadString();
            Parallax_Factor = reader.ReadDouble();
            X_velocity = reader.ReadInt32();
            Y_velocity = reader.ReadInt32();
        }
		#endregion
		public Map_Background(string filename, int x_velocity, int y_velocity, int parallax_factor)
        {
            Filename = filename;
            Parallax_Factor = (double)parallax_factor / 100d;
            X_velocity = x_velocity;
            Y_velocity = y_velocity;

            Texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + filename);
        }
        public Map_Background(BinaryReader reader)
        {
            read(reader);

            Texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + Filename);
            refresh_source();
        }

        public void update()
        {
            update_velocity();
            refresh_source();
        }

        private void update_velocity()
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
        private void refresh_source()
        {
            Source.X = (int)(Global.game_map.display_x / 2 * Parallax_Factor) + X_displacement;
            Source.Y = (int)(Parallax_Factor * Global.game_map.display_y / 2 * Config.WINDOW_WIDTH / Config.WINDOW_HEIGHT) + Y_displacement;
        }
        public void draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(this.Texture, Destination, Source, Color.White);
        }
    }
}