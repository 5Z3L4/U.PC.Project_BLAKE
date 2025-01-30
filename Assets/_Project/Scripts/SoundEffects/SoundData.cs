using System;
using UnityEngine;
using UnityEngine.Audio;

namespace _Project.Scripts.SoundEffects
{
    [Serializable]
    public class SoundData
    {
        [SerializeField] private AudioClip audioClip;
        [SerializeField] private AudioMixerGroup audioMixerGroup;
        [SerializeField] private bool isFrequent;
    
        public AudioClip AudioClip => audioClip;
        public AudioMixerGroup AudioMixerGroup => audioMixerGroup;
        public bool IsFrequent => isFrequent;
    }
}
