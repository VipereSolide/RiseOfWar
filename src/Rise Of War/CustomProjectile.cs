using System.Collections.Generic;
using System.Collections;
using System;

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

            if (_projectile == null)
            {
                Plugin.LogError("CustomProjectile: Instance of custom projectile could not be initialized! A custom projectile has to have a \"projectile\" component attached!");
                Destroy(this);
                return;
            }

            _projectileGraphics = transform.GetChild(0);

            if (_projectileGraphics == null)
            {
                Plugin.LogError("CustomProjectile: Instance of custom projectile could not be initialized! A custom projectile has to have a \"graphic\" children!");
                Destroy(this);
                return;
            }

            _tailScale = Random.Range(100f, 250f);
        }

        private void Update()
        {
            if (_projectileGraphics == null)
            {
                Plugin.LogError("CustomProjectile: Instance of custom projectile could not be initialized! A custom projectile has to have a \"graphic\" children!");
                Destroy(gameObject);
                return;
            }

            _projectileGraphics.localScale = Vector3.one + Vector3.forward * _projectile.velocity.magnitude / _tailScale;
        }
    }
}