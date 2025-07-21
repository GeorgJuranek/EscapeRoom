using System;
using EscapeRoom;

namespace EscapeRoom
{
    public struct Tile
    {
        private ETile type = ETile.free;

        private char layerVerseInit;

        private EKey tileKey = EKey.none;

        private Dictionary<int, (char, char)> verses = new Dictionary<int, (char, char)>();

        public Tile(ETile _type, char _individualInit, Dictionary<int, (char, char)> _verses) // for walkable Tiles
        {
            type = _type;
            layerVerseInit = _individualInit;
            verses = _verses;
        }

        public Tile(ETile _type, char _individual) //for wall Tiles
        {
            type = _type;
            layerVerseInit = _individual;
        }

        public char InitialIndividual
        {
            get
            {
                return layerVerseInit;
            }
        }

        public char WalkingOnNewIndividual(bool wasLastInputVertical, EKey key)
        {
            if (wasLastInputVertical)
            {
                return verses[(int)key].Item2;
            }
            return verses[(int)key].Item1;
        }

        public EKey TileKey
        {
            get
            {
                return tileKey;
            }

            set
            {
                tileKey = value;
            }
        }

        public ETile Type
        {
            get
            {
                return type;
            }
        }

    }
}

