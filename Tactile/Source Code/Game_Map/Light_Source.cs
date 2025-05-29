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
        private int Intensity;

        #region Serialization
        public void write(BinaryWriter writer)
        {
            Color.write(writer);
            Loc.write(writer);
            writer.Write(Intensity);
        }
        public void read(BinaryReader reader)
        {
            Color.read(reader);
            Loc.read(reader);
            Intensity = reader.ReadInt32();
        }
        #endregion

        #region Accessors
        public Color color { get { return Color; } }
        public Vector2 loc { get { return Loc; } }
        public int intensity { get { return Intensity; } }
        #endregion
        public Light_Source()
        {
            Color = Color.White;
            Loc = Vector2.Zero;
            Intensity = 0;
        }
        public Light_Source(Color color, Vector2 loc, int intensity)
        {
            Color = color;
            Loc = loc;
            Intensity = intensity;
        }
    }
}
