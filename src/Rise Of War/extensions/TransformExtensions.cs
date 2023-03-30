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

        public static Vector3 ToGroundPosition(this Transform transform)
        {
            Vector3 _output = transform.position;
            RaycastHit _hit;

            if (Physics.Raycast(transform.position + Vector3.up * 0.25f, Vector3.down, out _hit))
            {
                _output = _hit.point;
            }

            return _output;
        }

        public static Vector3 ToGroundPosition(this Transform transform, Vector3 offset)
        {
            Vector3 _output = transform.position;
            RaycastHit _hit;

            if (Physics.Raycast(transform.position + Vector3.up * 0.25f + offset, Vector3.down, out _hit))
            {
                _output = _hit.point;
            }

            return _output;
        }

        public static void SetLayerRecursively(this Transform transform, int layer)
        {
            transform.gameObject.layer = layer;

            foreach (Transform _child in transform)
            {
                _child.gameObject.layer = layer;
                _child.SetLayerRecursively(layer);
            }
        }
    }
}