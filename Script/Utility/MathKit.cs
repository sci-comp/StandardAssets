using Godot;
using System;
using System.Collections.Generic;

namespace Toolbox
{
    public enum CharacterLocation
    {
        Above,
        Behind,
        Center,
        Dynamic,
        Feet,
        Front,
        Head,
        WaterLine
    }

    public enum AxisDirection
    {
        Up,
        Down,
        Left,
        Right,
        Forward,
        Back
    }

    public static class MathKit
    {
        public static readonly Vector3[] VectorDirection = new Vector3[]
        {
            Vector3.Up,
            Vector3.Down,
            Vector3.Left,
            Vector3.Right,
            Vector3.Forward,
            Vector3.Back,
        };

        public static Vector3 RotateAroundUp(Vector3 characterForward, float degrees)
        {
            float radians = Mathf.DegToRad(degrees);
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            Vector3 newForward = new Vector3(
                cos * characterForward.X + sin * characterForward.Z,
                characterForward.Y,
                cos * characterForward.Z - sin * characterForward.X
            );

            return newForward;
        }

        public static Vector3 GetAxisDirection(AxisDirection axis)
        {
            return VectorDirection[(int)axis];
        }

        public static bool XOR3(bool a, bool b, bool c) => (a && !b && !c) || (!a && b && !c) || (!a && !b && c);

        public static float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
        {
            Vector3 perp = fwd.Cross(targetDir);
            float dir = perp.Dot(up);

            if (dir > 0.0f)
            {
                return 1.0f;
            }
            else if (dir < 0.0f)
            {
                return -1.0f;
            }
            else
            {
                return 0.0f;
            }
        }

        public static string Choice(IEnumerable<string> choices, IEnumerable<int> weights)
        {
            var cumulativeWeight = new List<int>();
            int last = 0;
            foreach (var cur in weights)
            {
                last += cur;
                cumulativeWeight.Add(last);
            }
            int choice = new Random().Next(0, last);
            int i = 0;
            foreach (var cur in choices)
            {
                if (choice < cumulativeWeight[i])
                {
                    return cur;
                }
                i++;
            }
            return null;
        }

        public static float Remap(float x, float A, float B, float C, float D)
        {
            float remappedValue = C + (x - A) / (B - A) * (D - C);
            return remappedValue;
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            var rng = new Random();
            for (int i = 0; i < list.Count; i++)
            {
                int j = rng.Next(i, list.Count);
                (list[j], list[i]) = (list[i], list[j]);
            }
        }
    }
}
