using UnityEngine;

namespace RiseOfWar
{
    public class PlayerInteractions : MonoBehaviour
    {
        private KeyCode _whistle = KeyCode.F;
        private KeyCode _interract = KeyCode.E;

        private float _lastWhistleTime = 0;

        private void Update()
        {
            if (Input.GetKeyDown(_whistle) && _lastWhistleTime <= Time.time)
            {
                Whistle();
            }

            if (Input.GetKeyDown(_interract))
            {
                DroppedWeapon _weapon = DroppedWeaponRegistry.GetClosestActiveDroppedWeapon();
                if (_weapon != null)
                {
                    _weapon.Pickup(ActorManager.instance.player);
                }
            }

            if (GameConfiguration.isDebugModeEnabled)
            {
                if (Input.GetKeyDown(KeyCode.M))
                {
                    RaycastHit _hit;
                    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out _hit))
                    {
                        ActorManager.ActorsOnTeam(1)[0].SetPositionAndRotation(_hit.point, ActorManager.ActorsOnTeam(1)[0].transform.rotation);
                    }
                }
            }
        }

        public void Whistle()
        {
            _lastWhistleTime = Time.time + GameConfiguration.whistleDelay;
            SoundManager.instance.ForcePlaySound(ResourceManager.Instance.whistleSounds[Random.Range(0, ResourceManager.Instance.whistleSounds.Length)]);
        }
    }
}