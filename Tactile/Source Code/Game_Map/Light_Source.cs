using Microsoft.Xna.Framework;
using System.IO;
using ArrayExtension;
using HashSetExtension;
using ListExtension;
using RectangleExtension;
using TactileArrayExtension;
using TactileVector2Extension;
using TactileDictionaryExtension;
using TactileListExtension;
using TactileColorExtension;

namespace Tactile
{
    public class Light_Source
    {
        private Color @Color;
        private Vector2 Loc;
        private int Brightness;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            Color.write(writer);
            Loc.write(writer);
            writer.Write(Brightness);
        }
        public void read(BinaryReader reader)
        {
            Color.read(reader);
            Loc = Loc.read(reader);
            Brightness = reader.ReadInt32();
        }
        #endregion

        #region Accessors
        public Color color { get { return Color; } }
        public Vector2 loc { get { return Loc; } }
        public int brightness { get { return Brightness; } }
        #endregion
        public Light_Source()
        {
            Color = Color.White;
            Loc = Vector2.Zero;
            Brightness = 0;
        }
        public Light_Source(Color color, Vector2 loc, int brightness)
        {
            Color = color;
            Loc = loc;
            Brightness = brightness;
            Color.A = (byte)Brightness;
        }
    }
}
