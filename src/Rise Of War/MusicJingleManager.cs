
using UnityEngine;

namespace RiseOfWar
{
    using Events;

    public class MusicJingleManager : MonoBehaviour
    {
        public static MusicJingleManager instance { get; private set; }

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            EventManager.onCapturePointInteraction += OnCapturePointInteractionListener;
        }

        private AudioClip GetJingleFromTeam(int team)
        {
            if (team == 0)
            {
                return ResourceManager.Instance.CaptureJingleGermany;
            }

            return ResourceManager.Instance.CaptureJingleSovietUnion;
        }

        private void OnCapturePointInteractionListener(OnCapturePointInteractionEvent _event)
        {
            switch (_event.type)
            {
                case OnCapturePointInteractionEvent.InteractionType.Captured:
                    SoundManager.instance.ForcePlaySound(GetJingleFromTeam(0));
                    break;
                case OnCapturePointInteractionEvent.InteractionType.Neutralized:
                    SoundManager.instance.ForcePlaySound(ResourceManager.Instance.CaptureJinglePointNeutralized);
                    break;
                case OnCapturePointInteractionEvent.InteractionType.Lost:
                    SoundManager.instance.ForcePlaySound(ResourceManager.Instance.CaptureJinglePointLost);
                    break;
                case OnCapturePointInteractionEvent.InteractionType.EnemyCapture:
                    SoundManager.instance.ForcePlaySound(GetJingleFromTeam(1));
                    break;
            }
        }
    }
}