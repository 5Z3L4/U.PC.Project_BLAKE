using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Player.GFX
{
    public class WhiteAlphaColorBlender : MonoBehaviour
    {
        private const string SHADER_NAME = "BLAKE_CellShading_WhiteAlphaColor";
        
        private readonly int _alpha = Shader.PropertyToID("_Alpha");
        private readonly int _colorHDR = Shader.PropertyToID("_Color");

        [SerializeField, Range(1f,2f)] private float intensity = 1.5f;
        
        private Material _whiteAlphaColorMaterial;

        private Vector4 ShineColor => new (intensity, intensity, intensity, 1f);
        
        private void Awake()
        {
            foreach (var material in GetComponent<SkinnedMeshRenderer>().materials)
            {
                if (material.shader.name.EndsWith(SHADER_NAME))
                {
                    _whiteAlphaColorMaterial = material;
                    break;
                }
            }

            if (_whiteAlphaColorMaterial == null)
            {
                Debug.LogError($"White Alpha Color shader not found in skinned mesh renderer!", this);
            }
        }

        private void OnEnable()
        {
            Reset();
        }

        private void OnDisable()
        {
            Reset();
        }

        public void LightUpPlayer(float animationDuration)
        {
            DOVirtual.Float(1f, 0f, animationDuration, SetFloat).SetTarget(this);
        }

        private void SetFloat(float value)
        {
            _whiteAlphaColorMaterial.SetFloat(_alpha, value);
        }

        private void Reset()
        {
            SetFloat(0f);
            _whiteAlphaColorMaterial.SetColor(_colorHDR, ShineColor);
        }
    }
}
