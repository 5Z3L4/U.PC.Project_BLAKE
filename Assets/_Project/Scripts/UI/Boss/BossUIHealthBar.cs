using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.Boss
{
    public class BossUIHealthBar : MonoBehaviour
    {
        [SerializeField] private BossUIEnabler bossUIEnabler;
        
        private readonly Color TARGET_COLOR = Color.red;
        private readonly Color START_COLOR = Color.white;
        
        [SerializeField] private TextMeshProUGUI enemyName;
        [SerializeField] private Image progressFill;
        [SerializeField] private ContentSizeFitter progressBarSizeFitter;

        private Sequence _sequence;
        private float _animationDuration = .5f;

        private void OnEnable()
        {
            enemyName.text = bossUIEnabler.BossCharacter.BossName.ToUpper();
            progressFill.color = TARGET_COLOR;
            progressFill.fillAmount = 1f;
            RefreshSizeFitter().Forget();
            bossUIEnabler.BossCharacter.OnDamageTaken += RefreshBar;
        }

        private void OnDisable()
        {
            if (bossUIEnabler.BossCharacter != null)
            {
                bossUIEnabler.BossCharacter.OnDamageTaken -= RefreshBar;
            }
        }

        private void RefreshBar(GameObject _)
        {
            _sequence?.Kill();

            var currentAmount = progressFill.fillAmount;
            var boss = bossUIEnabler.BossCharacter;
            if (boss == null)
            {
                return;
            }
            
            var targetAmount = (float)boss.Health / boss.DefaultHealth;
            
            _sequence = DOTween.Sequence(this);
            var t1 = DOVirtual.Float(currentAmount, targetAmount, _animationDuration, 
                x => progressFill.fillAmount = x);
            var t2 = DOVirtual.Color(START_COLOR, TARGET_COLOR, _animationDuration,
                c => progressFill.color = c);

            _sequence.Insert(0, t1);
            _sequence.Insert(0, t2);
            _sequence.OnComplete(() => _sequence = null);
        }

        private async UniTaskVoid RefreshSizeFitter()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(.1f));
            progressBarSizeFitter.enabled = false;
            progressBarSizeFitter.enabled = true;
        }
    }
}
