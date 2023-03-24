using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheSTAR.Utility
{
    public static class ColorUtility
    {
        private static readonly Color MaxFaintColor = new Color(0.5f, 0.5f, 0.5f, 1);

        public static Color Gray(Color initialColor)
        {
            float value = (initialColor.r + initialColor.g + initialColor.b) / 3;
            return new Color(value, value, value, initialColor.a);
        }

        public static Color Light(Color color, float force)
        {
            float a = color.a;
            color *= force;
            color.a = a;
            return color;
        }

        public static Color Faint(Color color) => color - (color - MaxFaintColor) / 2;

        public static Color Contrast(Color color) => color + (color - MaxFaintColor) / 2;
    }
}