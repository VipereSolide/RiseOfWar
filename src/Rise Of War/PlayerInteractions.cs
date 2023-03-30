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
                RaycastHit _hit;
                PlayerFpParent _origin = PlayerFpParent.instance;

                if (Physics.Raycast(
                    _origin.fpCamera.transform.position,
                    _origin.fpCamera.transform.forward,
                    out _hit,
                    GameConfiguration.weaponPickupDistance
                ))
                {
                    DroppedWeapon _weapon = _hit.transform.GetComponent<DroppedWeapon>();

                    if (_weapon != null)
                    {
                        _weapon.Pickup(FpsActorController.instance.actor);
                    }
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