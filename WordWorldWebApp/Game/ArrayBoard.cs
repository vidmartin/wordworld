using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordWorldWebApp.Game
{
    public class ArrayBoard : Board
    {
        private readonly char[,] _array;

        public ArrayBoard(int w, int h)
        {
            _array = new char[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    _array[x, y] = ' ';
                }
            }
        }

        public override int Width => _array.GetLength(0);

        public override int Height => _array.GetLength(1);

        public override bool WriteX(XY pos, string s)
        {
            if (pos.x < 0 || pos.x + s.Length > Width || pos.y < 0 || pos.y + 1 > Height)
            {
                throw new IndexOutOfRangeException();
            }

            for (int i = 0; i < s.Length; i++)
            {
                _array[pos.x + i, pos.y] = s[i];
            }

            return true;
        }

        public override bool WriteY(XY pos, string s)
        {
            if (pos.x < 0 || pos.x + 1 > Width || pos.y < 0 || pos.y + s.Length > Height)
            {
                throw new IndexOutOfRangeException();
            }
            
            for (int i = 0; i < s.Length; i++)
            {
                _array[pos.x, pos.y + i] = s[i];
            }

            return true;
        }

        public override bool DeleteX(XY pos)
        {
            if (At(pos) == ' ')
            {
                return false;
            }

            for (int i = 0; true; i++)
            {
                var curr = new XY(pos.x + i, pos.y);

                if (curr.x >= Width || At(curr) == ' ')
                {
                    return true;
                }

                if (StartOfY(curr) != curr || EndOfY(curr) != curr)
                {
                    // pokud je písmeno součástí slova v druhém směru, nemazat ho
                    continue;
                }

                _array[curr.x, curr.y] = ' ';
            }
        }

        public override bool DeleteY(XY pos)
        {
            if (At(pos) == ' ')
            {
                return false;
            }

            for (int i = 0; true; i++)
            {
                var curr = new XY(pos.x, pos.y + i);

                if (curr.y >= Width || At(curr) == ' ')
                {
                    return true;
                }

                if (StartOfX(curr) != curr || EndOfX(curr) != curr)
                {
                    // pokud je písmeno součástí slova v druhém směru, nemazat ho
                    continue;
                }

                _array[curr.x, curr.y] = ' ';
            }
        }

        public override string ReadX(XY pos)
        {
            var builder = new StringBuilder();

            while(pos.x < Width && At(pos) != ' ')
            {
                builder.Append(At(pos));
                pos.x += 1;
            }

            return builder.ToString();
        }

        public override string ReadY(XY pos)
        {
            var builder = new StringBuilder();

            while (pos.y < Width && At(pos) != ' ')
            {
                builder.Append(At(pos));
                pos.y += 1;
            }

            return builder.ToString();
        }

        public override XY StartOfX(XY pos)
        {
            if (At(pos) == ' ')
            {
                throw new ArgumentException();
            }

            for (int i = 0; true; i++)
            {
                var curr = new XY(pos.x - i, pos.y);

                if (curr.x < 0 || At(curr) == ' ')
                {
                    return new XY(pos.x - i + 1, pos.y);
                }
            }
        }

        public override XY StartOfY(XY pos)
        {
            if (At(pos) == ' ')
            {
                throw new ArgumentException();
            }

            for (int i = 0; true; i++)
            {
                var curr = new XY(pos.x, pos.y - i);

                if (curr.y < 0 || At(curr) == ' ')
                {
                    return new XY(pos.x, pos.y - i + 1);
                }
            }
        }

        public override char At(XY pos)
        {
            return _array[pos.x, pos.y];
        }

        public override void Set(XY pos, char ch)
        {
            _array[pos.x, pos.y] = ch;
        }

        public override XY EndOfX(XY pos)
        {
            if (At(pos) == ' ')
            {
                throw new ArgumentException();
            }

            for (int i = 0; true; i++)
            {
                var curr = new XY(pos.x + i, pos.y);

                if (curr.x >= Width || At(curr) == ' ')
                {
                    return new XY(pos.x + i - 1, pos.y);
                }
            }
        }

        public override XY EndOfY(XY pos)
        {
            if (At(pos) == ' ')
            {
                throw new ArgumentException();
            }

            for (int i = 0; true; i++)
            {
                var curr = new XY(pos.x, pos.y + i);

                if (curr.y >= Height || At(curr) == ' ')
                {
                    return new XY(pos.x, pos.y + i - 1);
                }
            }
        }
    }
}
