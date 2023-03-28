using UnityEngine;

namespace RiseOfWar
{
    public class PlayerInteractions : MonoBehaviour
    {
        private KeyCode _whistle = KeyCode.F;
        private float _lastWhistleTime = 0;

        private void Update()
        {
            if (Input.GetKeyDown(_whistle) && _lastWhistleTime <= Time.time)
            {
                Whistle();
            }

            if (GameConfiguration.isDebugModeEnabled)
            {
                if (Input.GetKeyDown(KeyCode.M))
                {
                    RaycastHit _hit;
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit))
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