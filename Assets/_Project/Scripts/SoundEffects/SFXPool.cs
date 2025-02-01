using System.Collections.Generic;
using _Project.Scripts.Patterns;
using UnityEngine;
using UnityEngine.Pool;

namespace _Project.Scripts.SoundEffects
{
    public class SFXPool : MonoBehaviour
    {
        [SerializeField] private GameObject poolPrefab;
        [SerializeField] private int defaultPoolCapacity = 10;
        [SerializeField] private int maxPoolCapacity = 100;
        [SerializeField] private bool collectionCheck = true;
        
        private ObjectPool<AudioSource> _audioSourcePool;

        private void Awake()
        {
            _audioSourcePool = new ObjectPool<AudioSource>(
                CreateAudioSource, 
                OnGetAudioSource, 
                OnReleaseAudioSource,
                OnDestroyAudioSource,
                collectionCheck,
                defaultPoolCapacity,
                maxPoolCapacity);
        }

        private AudioSource CreateAudioSource()
        {
            var audioSourceGO = Instantiate(poolPrefab, transform, true);
            var audioSource = audioSourceGO.GetComponent<AudioSource>();
            audioSource.gameObject.SetActive(false);
            
            return audioSource;
        }

        private void OnGetAudioSource(AudioSource obj)
        {
            obj.gameObject.SetActive(true);
        }

        private void OnReleaseAudioSource(AudioSource obj)
        {
            obj.gameObject.SetActive(false);
        }

        private void OnDestroyAudioSource(AudioSource obj)
        {
            Destroy(obj.gameObject);
        }

        public AudioSource GetObject()
        {
            return _audioSourcePool.Get();
        }

        public void ReturnObject(AudioSource obj)
        {
            _audioSourcePool.Release(obj);
        }
    }
}
