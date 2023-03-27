using HarmonyLib;
using UnityEngine;

namespace RiseOfWar
{
    public static class DecalManagerPatcher
    {
        [HarmonyPatch(typeof(DecalManager), "AddDecal")]
        [HarmonyPrefix]
        static void AddDecalPatch()
        {
        }

        [HarmonyPatch(typeof(DecalManager), "CreateBloodDrop")]
        [HarmonyPrefix]
        static bool CreateBloodDrop(Vector3 point, Vector3 baseVelocity, int team)
        {
            Color color = DecalManager.instance.decalDrawers[team + 1].GetComponent<Renderer>().material.GetColor("_Color");

            BloodParticle _bloodParticle = GameObject.Instantiate(DecalManager.instance.bloodDropPrefab, point, Quaternion.identity).GetComponent<BloodParticle>();
            _bloodParticle.transform.localScale.Scale(Vector3.one * UnityEngine.Random.Range(2f, 3f));

            switch (BloodParticle.BLOOD_PARTICLE_SETTING)
            {
                case BloodParticle.BloodParticleType.BloodExplosions:
                    _bloodParticle.velocity = baseVelocity + (UnityEngine.Random.insideUnitSphere + new Vector3(0f, 1.4f, 0f)) * 8f;
                    break;

                case BloodParticle.BloodParticleType.DecalOnly:
                    _bloodParticle.velocity = baseVelocity * 2f + (UnityEngine.Random.insideUnitSphere + Vector3.down) * 2f;
                    break;

                default:
                    _bloodParticle.velocity = baseVelocity + (UnityEngine.Random.insideUnitSphere + Vector3.up) * 2f;
                    break;
            }

            _bloodParticle.team = team;

            Renderer _particleRenderer = _bloodParticle.GetComponent<Renderer>();
            if (BloodParticle.BLOOD_PARTICLE_SETTING == BloodParticle.BloodParticleType.DecalOnly)
            {
                _particleRenderer.enabled = false;
                return false;
            }

            _particleRenderer.material.color = color;

            return false;
        }
    }
}