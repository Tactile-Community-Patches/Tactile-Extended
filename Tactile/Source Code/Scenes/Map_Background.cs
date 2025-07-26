using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Tactile
{
    enum Background_Mode
    {
        Scrolling,
        Static
    }
	class Map_Background : Stereoscopic_Graphic_Object
    {
        private Background_Mode Mode;
        private Texture2D Texture;
        private string Filename;
        private double Parallax_Factor;
        private int X_velocity;
        private int Y_velocity;
        private double X_displacement;
        private double Y_displacement;
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
            writer.Write(X_displacement);
            writer.Write(Y_displacement);
            writer.Write((int)Mode);
        }
        public void read(BinaryReader reader)
        {
            Filename = reader.ReadString();
            Parallax_Factor = reader.ReadDouble();
            X_velocity = reader.ReadInt32();
            Y_velocity = reader.ReadInt32();
            X_displacement = reader.ReadDouble();
            Y_displacement = reader.ReadDouble();
            Mode = (Background_Mode)reader.ReadInt32();
        }
		#endregion
		public Map_Background(string filename, int x, int y, int parallax_factor, Background_Mode mode)
        {
            Filename = filename;
            Mode = mode;
            Parallax_Factor = (double)parallax_factor / 100d;
            Texture = Global.Content.Load<Texture2D>(@"Graphics/Pictures/" + filename);

            switch (Mode)
            {
                case Background_Mode.Scrolling:
                    X_velocity = x;
                    Y_velocity = y;
                    X_displacement = 0d;
                    Y_displacement = 0d;
                    break;
                case Background_Mode.Static:
                    X_velocity = 0;
                    Y_velocity = 0;
                    X_displacement = (double)x;
                    Y_displacement = (double)y;
                    break;
            }
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
            X_displacement += X_velocity / 1000d;
            Y_displacement += Y_velocity / 1000d;
            if ((int)X_displacement % Texture.Width == 0 && (int)X_displacement != 0)
            {
                X_displacement = 0d;
            }
            if ((int)Y_displacement % Texture.Height == 0 && (int)Y_displacement != 0)
            {
                Y_displacement = 0d;
            }
        }
        private void refresh_source()
        {
            Source.X = (int)(Global.game_map.display_x / 2 * Parallax_Factor) + (int)X_displacement;
            Source.Y = (int)(Parallax_Factor * Global.game_map.display_y / 2 * Config.WINDOW_WIDTH / Config.WINDOW_HEIGHT) + (int)Y_displacement;
        }
        public void draw(SpriteBatch spriteBatch)
        {
            switch(Mode)
            {
                case Background_Mode.Scrolling:
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone);
                    break;
                case Background_Mode.Static:
                    spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
                    break;
            }
            spriteBatch.Draw(this.Texture, Destination, Source, Color.White);
            spriteBatch.End();
        }
    }
}