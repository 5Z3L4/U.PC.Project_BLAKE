using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Patterns;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _Project.Scripts.VFX_Controllers
{
    public class PlayerVignetteDamageEffect : Singleton<PlayerVignetteDamageEffect>
    {
        [SerializeField] private float targetVignetteIntensity = .6f;
        [SerializeField] private float recoverySpeed = 1f;
        
        private Vignette _vignette;

        protected override void Awake()
        {
            base.Awake();
            
            if (GetComponent<Volume>().profile.TryGet(out _vignette))
            {
                _vignette.intensity.overrideState = true;
                _vignette.intensity.value = 0f;
                
                _vignette.smoothness.overrideState = true;
                _vignette.smoothness.value = .75f;
                
                _vignette.color.overrideState = true;
                _vignette.color.value = Color.red;
            }
            
            ReferenceManager.Instance.OnFloorLoad += OnPlayerSpawned;
        }

        private void Update()
        {
            if (_vignette != null)
            {
                _vignette.intensity.value = _vignette.intensity.value < 0.1f ? 0f : Mathf.Lerp(_vignette.intensity.value, 0f, Time.deltaTime * recoverySpeed);
            }
        }

        private void OnPlayerSpawned()
        {
            ReferenceManager.BlakeHeroCharacter.OnDamageTaken += TakeDamage;
        }

        private void TakeDamage(GameObject _)
        {
            if (_vignette != null)
            {
                _vignette.intensity.value = targetVignetteIntensity;
            }
        }

        private void OnDestroy()
        {
            if (ReferenceManager.Instance != null)
            {
                ReferenceManager.Instance.OnFloorLoad -= OnPlayerSpawned;
            }

            if (ReferenceManager.BlakeHeroCharacter != null)
            {
                ReferenceManager.BlakeHeroCharacter.OnDamageTaken -= TakeDamage;
            }
        }
    }
}