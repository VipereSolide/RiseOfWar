
using UnityEngine;
using Random = UnityEngine.Random;

namespace RiseOfWar
{
    public class CustomProjectile : MonoBehaviour
    {
        protected Transform _projectileGraphics;
        protected Projectile _projectile;
        protected float _tailScale = 250f;

        private void Start()
        {
            _projectile = GetComponent<Projectile>();
            _projectile.transform.localScale = Vector3.one;

            if (_projectile == null)
            {
                Plugin.LogError("CustomProjectile: Instance of custom projectile could not be initialized! A custom projectile has to have a \"projectile\" component attached!");
                return;
            }

            _projectileGraphics = transform.GetChild(0);

            if (_projectileGraphics == null)
            {
                Plugin.LogError("CustomProjectile: Instance of custom projectile could not be initialized! A custom projectile has to have a \"graphic\" children!");
                return;
            }

            MeshRenderer _renderer = _projectileGraphics.GetComponent<MeshRenderer>();
            Material _material = ResourceManager.Instance.ProjectileTracerMaterials[_projectile.source.team + 1];
            _renderer.material = _material;

            _tailScale = Random.Range(25f, 100f);
        }

        private void Update()
        {
            if (_projectileGraphics == null)
            {
                Plugin.LogError("CustomProjectile: Instance of custom projectile could not be initialized! A custom projectile has to have a \"graphic\" children!");
                return;
            }

            _projectileGraphics.localScale = (Vector3.one + Vector3.up * _projectile.velocity.magnitude / _tailScale) / 8;
        }
    }
}