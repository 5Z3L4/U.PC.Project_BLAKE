using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Patterns;
using UnityEngine;
using UnityEngine.Audio;

namespace _Project.Scripts.SoundEffects
{
    [RequireComponent(typeof(SFXPool))]
    public class SoundEffectsManager : Singleton<SoundEffectsManager>
    {
        private SFXPool _sfxPool;
        private Dictionary<string, AudioClip> _isClipPlaying;
        private Dictionary<string, AudioSource> _frequentAudioSources;

        protected override void Awake()
        {
            base.Awake();
            
            _sfxPool = GetComponent<SFXPool>();
            _isClipPlaying = new Dictionary<string, AudioClip>();
            _frequentAudioSources = new Dictionary<string, AudioSource>();
        }

        public void PlaySFX(SoundData soundData, Vector3 position, bool isRandomVolume = false, bool isRandomPitch = false)
        {
            if (!_isClipPlaying.TryAdd(soundData.AudioClip.name, soundData.AudioClip) && !soundData.IsFrequent) return;

            if (!_frequentAudioSources.TryGetValue(soundData.AudioClip.name, out var audioSource))
            {
                audioSource = _sfxPool.GetObject();
            }
            
            audioSource.transform.position = position;
            audioSource.outputAudioMixerGroup = soundData.AudioMixerGroup;
            audioSource.clip = soundData.AudioClip;

            if (isRandomPitch)
            {
                audioSource.pitch += Random.Range(-0.05f, 0.05f);
            }

            if (isRandomVolume)
            {
                audioSource.volume = Random.Range(0.45f, 1f);
            }
            
            audioSource.PlayOneShot(soundData.AudioClip);
            //audioSource.Play();

            StartCoroutine(ClipReset(audioSource, soundData.AudioClip, soundData.IsFrequent));
        }
    
        private IEnumerator ClipReset(AudioSource audioSource, AudioClip clip, bool isFrequent)
        {
            yield return new WaitUntil(() => !audioSource.isPlaying);
            audioSource.Stop();
            
            _isClipPlaying.Remove(clip.name);

            if (isFrequent)
            {
                _frequentAudioSources.TryAdd(clip.name, audioSource);
                yield break;
            }
            _sfxPool.ReturnObject(audioSource);
        }
    }
}
