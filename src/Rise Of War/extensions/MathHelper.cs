using System;

using UnityEngine;

namespace RiseOfWar
{
    public static class MathHelper
    {
        public static class FloatHelper
        {
            public static float Parse(string value)
            {
                try
                {
                    return float.Parse(value);
                }
                catch
                {
                    return float.Parse(value.Replace(".", ","));
                }
            }
        }
    }
}