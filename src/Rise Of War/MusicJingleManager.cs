using System;

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

        private void OnCapturePointInteractionListener(OnCapturePointInteractionEvent _event)
        {
            switch (_event.type)
            {
                case OnCapturePointInteractionEvent.InteractionType.Captured:
                    SoundManager.instance.PlaySound(ResourceManager.Instance.captureJingleGe);
                    break;
                case OnCapturePointInteractionEvent.InteractionType.Neutralized:
                    SoundManager.instance.PlaySound(ResourceManager.Instance.captureJingleCapturePointNeutralized);
                    break;
                case OnCapturePointInteractionEvent.InteractionType.Lost:
                    SoundManager.instance.PlaySound(ResourceManager.Instance.captureJingleCapturePointLost);
                    break;
            }
        }
    }
}