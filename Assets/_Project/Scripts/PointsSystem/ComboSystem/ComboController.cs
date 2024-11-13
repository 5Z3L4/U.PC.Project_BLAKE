using System;
using UnityEngine;

namespace _Project.Scripts.PointsSystem.ComboSystem
{
    public class ComboController : MonoBehaviour
    {
        private const float MAX_TIMER_VALUE = 3f;
        private const float MAX_COMBO_COUNT = 5f;
        private float MIN_COMBO_COUNT = 1f;
        private const float COMBO_INCREASE_STEP = .1f;
        private const int MIN_KILLS_TO_START_COMBO = 2;
    
        private float comboCounter = 1f;
        private float timer = 0f;
        private int killsCounter = 0;
        private bool isComboActive = false;

        public event Action OnComboTimerEnd;

        public float ComboCounter => comboCounter;
        public int KillsCounter => killsCounter;
        public bool ShouldComboStart => killsCounter >= MIN_KILLS_TO_START_COMBO;

        public event Action<float> comboShieldEvent;

        private void Update()
        {
            CountdownTime();
        }

        public void RegisterEnemyDeath()
        {
            timer = MAX_TIMER_VALUE;
            killsCounter++;

            isComboActive = true;

            if (comboCounter < MAX_COMBO_COUNT && ShouldComboStart)
            {
                comboCounter += COMBO_INCREASE_STEP;
            }
            comboShieldEvent?.Invoke(comboCounter);

        }

        public void SetMaxCombo()
        {
            timer = MAX_TIMER_VALUE;
            isComboActive = true;
            comboCounter = MAX_COMBO_COUNT;

        }

        public void AddMinCombo(float value)
        {
            MIN_COMBO_COUNT += value;
            comboCounter = MIN_COMBO_COUNT;
        }

        private void CountdownTime()
        {
            if (!isComboActive)
            {
                return;
            }
        
            timer -= Time.deltaTime;
        
            if (timer < 0)
            {
                ResetValues();
            }
        }

        private void ResetValues()
        {
            OnComboTimerEnd?.Invoke();
            timer = 0f;
            killsCounter = 0;
            comboCounter = MIN_COMBO_COUNT;
            isComboActive = false;
        }
    }
}
