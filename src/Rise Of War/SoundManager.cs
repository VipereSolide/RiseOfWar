using System.Collections.Generic;

using UnityEngine;

namespace RiseOfWar
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance { get; private set; }

        private List<AudioClip> _scheduledSounds = new List<AudioClip>();
        private bool _isPlayingSound = false;

        private AudioSource _source;
        private float _playingSoundCooldown = 0;

        public AudioSource Source { get { return _source; } }

        private void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = false;
            _source.playOnAwake = false;
        }

        public void ResetAudioMixerGroup()
        {
            _source.outputAudioMixerGroup = GameManager.instance.ingameMixerGroup;
        }

        public void PlaySound(AudioClip sound)
        {
            AddSoundToQueue(sound);
        }

        public void ForcePlaySound(AudioClip sound)
        {
            _source.PlayOneShot(sound);
        }

        private void Update()
        {
            if (_scheduledSounds.Count <= 0)
            {
                return;
            }

            if (_isPlayingSound == true)
            {
                _playingSoundCooldown -= Time.deltaTime;

                if (_playingSoundCooldown > 0)
                {
                    return;
                }

                _playingSoundCooldown = _scheduledSounds[0].length;
                _isPlayingSound = false;
            }

            ForcePlaySound(_scheduledSounds[0]);
            _scheduledSounds.RemoveAt(0);
            _isPlayingSound = true;
        }

        private void AddSoundToQueue(AudioClip sound)
        {
            _scheduledSounds.Add(sound);
        }
    }
}