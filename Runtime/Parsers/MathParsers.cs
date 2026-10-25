using System.Text.RegularExpressions;
using UnityEngine;

namespace Game.GM
{
    internal sealed class Vector2ArgParser : ArgParser<Vector2>
    {
        private static readonly Regex _template = ArgText.Tuple(string.Empty, ArgText.FloatComponent, 2);

        public override Regex Template => _template;

        public override string Hint => "(x,y)，例如 (1,2)、(1.5, -2)、(-1f, 0.5)";

        public override bool TryParse(string arg, out object argObj)
        {
            if (ArgText.TrySplitTuple(arg, out string[] parts)
                && parts.Length == 2
                && ArgText.TryParseFloat(parts[0], out float x)
                && ArgText.TryParseFloat(parts[1], out float y))
            {
                argObj = new Vector2(x, y);
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class Vector3ArgParser : ArgParser<Vector3>
    {
        private static readonly Regex _template = ArgText.Tuple(string.Empty, ArgText.FloatComponent, 3);

        public override Regex Template => _template;

        public override string Hint => "(x,y,z)，例如 (1,9,0)、(1.5, -2, 0f)";

        public override bool TryParse(string arg, out object argObj)
        {
            if (ArgText.TrySplitTuple(arg, out string[] parts)
                && parts.Length == 3
                && ArgText.TryParseFloat(parts[0], out float x)
                && ArgText.TryParseFloat(parts[1], out float y)
                && ArgText.TryParseFloat(parts[2], out float z))
            {
                argObj = new Vector3(x, y, z);
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class Vector4ArgParser : ArgParser<Vector4>
    {
        private static readonly Regex _template = ArgText.Tuple(string.Empty, ArgText.FloatComponent, 4);

        public override Regex Template => _template;

        public override string Hint => "(x,y,z,w)，例如 (1,0,0,1)";

        public override bool TryParse(string arg, out object argObj)
        {
            if (ArgText.TrySplitTuple(arg, out string[] parts)
                && parts.Length == 4
                && ArgText.TryParseFloat(parts[0], out float x)
                && ArgText.TryParseFloat(parts[1], out float y)
                && ArgText.TryParseFloat(parts[2], out float z)
                && ArgText.TryParseFloat(parts[3], out float w))
            {
                argObj = new Vector4(x, y, z, w);
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class Vector2IntArgParser : ArgParser<Vector2Int>
    {
        private static readonly Regex _template = ArgText.Tuple("i", ArgText.IntComponent, 2);

        public override Regex Template => _template;

        public override string Hint => "i(x,y)，只接受整数，例如 i(3,4)、i(-1, 2)";

        public override bool TryParse(string arg, out object argObj)
        {
            if (ArgText.TrySplitTuple(arg, out string[] parts)
                && parts.Length == 2
                && ArgText.TryParseInt(parts[0], out int x)
                && ArgText.TryParseInt(parts[1], out int y))
            {
                argObj = new Vector2Int(x, y);
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class Vector3IntArgParser : ArgParser<Vector3Int>
    {
        private static readonly Regex _template = ArgText.Tuple("i", ArgText.IntComponent, 3);

        public override Regex Template => _template;

        public override string Hint => "i(x,y,z)，只接受整数，例如 i(1,2,3)";

        public override bool TryParse(string arg, out object argObj)
        {
            if (ArgText.TrySplitTuple(arg, out string[] parts)
                && parts.Length == 3
                && ArgText.TryParseInt(parts[0], out int x)
                && ArgText.TryParseInt(parts[1], out int y)
                && ArgText.TryParseInt(parts[2], out int z))
            {
                argObj = new Vector3Int(x, y, z);
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class QuaternionArgParser : ArgParser<Quaternion>
    {
        private static readonly Regex _template = ArgText.Tuple("q", ArgText.FloatComponent, 4);

        public override Regex Template => _template;

        public override string Hint => "q(x,y,z,w)，例如 q(0,0,0,1)";

        public override bool TryParse(string arg, out object argObj)
        {
            if (ArgText.TrySplitTuple(arg, out string[] parts)
                && parts.Length == 4
                && ArgText.TryParseFloat(parts[0], out float x)
                && ArgText.TryParseFloat(parts[1], out float y)
                && ArgText.TryParseFloat(parts[2], out float z)
                && ArgText.TryParseFloat(parts[3], out float w))
            {
                argObj = new Quaternion(x, y, z, w);
                return true;
            }

            argObj = null;
            return false;
        }
    }

    internal sealed class ColorArgParser : ArgParser<Color>
    {
        private static readonly Regex _template = ArgText.Tuple("c", ArgText.FloatComponent, 3, 4);

        public override Regex Template => _template;

        public override string Hint => "c(r,g,b) 或 c(r,g,b,a)，分量按 0~1，例如 c(1,0,0)、c(1,0,0,1)";

        public override bool TryParse(string arg, out object argObj)
        {
            if (!ArgText.TrySplitTuple(arg, out string[] parts) || (parts.Length != 3 && parts.Length != 4))
            {
                argObj = null;
                return false;
            }

            if (!ArgText.TryParseFloat(parts[0], out float r)
                || !ArgText.TryParseFloat(parts[1], out float g)
                || !ArgText.TryParseFloat(parts[2], out float b))
            {
                argObj = null;
                return false;
            }

            float a = 1f;
            if (parts.Length == 4 && !ArgText.TryParseFloat(parts[3], out a))
            {
                argObj = null;
                return false;
            }

            argObj = new Color(r, g, b, a);
            return true;
        }
    }

    internal sealed class RectArgParser : ArgParser<Rect>
    {
        private static readonly Regex _template = ArgText.Tuple("rect", ArgText.FloatComponent, 4);

        public override Regex Template => _template;

        public override string Hint => "rect(x,y,width,height)，例如 rect(0,0,100,50)";

        public override bool TryParse(string arg, out object argObj)
        {
            if (ArgText.TrySplitTuple(arg, out string[] parts)
                && parts.Length == 4
                && ArgText.TryParseFloat(parts[0], out float x)
                && ArgText.TryParseFloat(parts[1], out float y)
                && ArgText.TryParseFloat(parts[2], out float width)
                && ArgText.TryParseFloat(parts[3], out float height))
            {
                argObj = new Rect(x, y, width, height);
                return true;
            }

            argObj = null;
            return false;
        }
    }
}
