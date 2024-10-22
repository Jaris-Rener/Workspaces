namespace Howl.Workspaces
{
    using System;
    using UnityEngine;

    public static class Util
    {
        public static int Round(this float val, double factor = 1f)
        {
            return (int)(Math.Round(val / factor, MidpointRounding.AwayFromZero) * factor);
        }

        public static Vector2 SnapToGrid(this Vector2 vector, float factor = 0)
        {
            vector.x = Round(vector.x, factor);
            vector.y = Round(vector.y, factor);
            return vector;
        }
    }
}