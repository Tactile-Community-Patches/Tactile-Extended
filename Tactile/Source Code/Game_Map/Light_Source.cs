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
    class Light_Source
    {
        private Color @Color;
        private Vector2 Loc;

        public void write(BinaryWriter writer)
        {
            Color.write(writer);
            Loc.write(writer);
        }
        public void read(BinaryReader reader)
        {
            Color.read(reader);
            Loc.read(reader);
        }

        public Color color { get { return Color; } }
        public Vector2 loc { get { return Loc; } }
        public Light_Source()
        {
            Color = Color.White;
            Loc = Vector2.Zero;
        }
        public Light_Source(Color color, Vector2 loc)
        {
            Color = color;
            Loc = loc;
        }
    }
}
