using System.Collections.Generic;

using UnityEngine;

namespace RiseOfWar
{
    public static class AnimationHelper
    {
        public static string[] GetClipNames(this Animation animation)
        {
            List<string> _animationClips = new List<string>();

            foreach (AnimationClip _clip in animation)
            {
                _animationClips.Add(_clip.name);
            }

            return _animationClips.ToArray();
        }

        public static void Clear(this Animation animation)
        {
            string[] _names = GetClipNames(animation);

            foreach (string _name in _names)
            {
                animation.RemoveClip(_name);
            }
        }

        public static AnimationClip GetClip(this Animation animation, int index)
        {
            string[] _names = GetClipNames(animation);
            AnimationClip _clip = animation.GetClip(_names[index]);

            return _clip;
        }
    }
}