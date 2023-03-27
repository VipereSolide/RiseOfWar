using UnityEngine;

namespace RiseOfWar
{
    public static class Vector3Extensions
    {
        public static Vector3 SetX(this Vector3 _value, float _x)
        {
            return new Vector3(_x, _value.y, _value.z);
        }

        public static Vector3 SetY(this Vector3 _value, float _y)
        {
            return new Vector3(_value.x, _y, _value.z);
        }

        public static Vector3 SetZ(this Vector3 _value, float _z)
        {
            return new Vector3(_value.x, _value.y, _z);
        }

        public static Vector3 Parse(string _value)
        {
            string[] _parts = _value.Split(',');
            return new Vector3(MathHelper.FloatHelper.Parse(_parts[0]), MathHelper.FloatHelper.Parse(_parts[1]), MathHelper.FloatHelper.Parse(_parts[2]));
        }
    }
}