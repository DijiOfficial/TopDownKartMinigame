using System;
using UnityEngine;

namespace Battlekart
{
    [Serializable]
    public class SoundSetting
    {
        [SerializeField] private AudioClip _clip;
        [SerializeField, Range(0.0f, 1.0f)] private float _volume = 1.0f;

        public AudioClip Clip => _clip;
        public float Volume => _volume;
    }
}
