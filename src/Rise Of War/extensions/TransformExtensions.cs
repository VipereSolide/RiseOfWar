using UnityEngine;

namespace RiseOfWar
{
    public static class TransformExtensions
    {
        public static void ResetLocalTransform(this Transform _transform)
        {
            _transform.localPosition = Vector3.zero;
            _transform.localRotation = Quaternion.identity;
            _transform.localScale = Vector3.one;
        }

        public static void ResetGlobalTransform(this Transform _transform)
        {
            _transform.position = Vector3.zero;
            _transform.rotation = Quaternion.identity;
            _transform.localScale = Vector3.one;
        }
    }
}