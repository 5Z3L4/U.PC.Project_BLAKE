using System;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Patterns;
using UnityEngine;

namespace _Project.Scripts.PointsSystem
{
    public class PlayerCurrencyController : Singleton<PlayerCurrencyController>
    {
        public delegate void AddedPointsDeleagte(float addedPoints);
        public event AddedPointsDeleagte onAddPoints;
        public event Action<float> OnPointsChanged;
        
        private float points = 0;

        [SerializeField]
        private float lostPointOnDeathPercentage = 20;
        [SerializeField]
        private float deathPointsModifier = 0;
        public float Points => points;

#if UNITY_EDITOR
        private bool _debugMode;
#endif
        
        protected override void Awake()
        {
            base.Awake();
            
            ReferenceManager.PlayerCurrencyController = this;
        }

        public void ResetValues()
        {
            points = 0;
            deathPointsModifier = 0;
            lostPointOnDeathPercentage = 20;
        }

        public void RegisterEnemyDeath(int pointsForKill)
        {
            float pointsToAdd = pointsForKill * EnemyDeathMediator.Instance.ComboController.ComboCounter;
            AddPoints(pointsToAdd);
            onAddPoints?.Invoke(pointsToAdd);
        }

        public void AddPoints(float points)
        {
            this.points += points;
            OnPointsChanged?.Invoke(this.points);
        }

        public void RemovePoints(float points)
        {
            this.points -= points;
            OnPointsChanged?.Invoke(this.points);
        }

        public bool HasPlayerEnoughPoints(float pointsToSpend)
        {
#if UNITY_EDITOR
            if (_debugMode)
            {
                return true;
            }
#endif
            return points >= pointsToSpend;
        }

        public void LosePointsOnDeath(BlakeCharacter blakeCharacter)
        {
            var value = Mathf.Max((lostPointOnDeathPercentage + deathPointsModifier) / 100f, 0f);
            RemovePoints(Mathf.Round(points * value));
            EnemyDeathMediator.Instance.Refresh();
        }

        public void AddDeathModifierPercentage(float percentage)
        {
            deathPointsModifier += percentage;
        }

        public void RefreshPoints()
        {
            OnPointsChanged?.Invoke(this.points);
        }


#if UNITY_EDITOR
        public void SetDebugMode(bool isEnabled)
        {
            if (isEnabled)
            {
                points = 10000;
                _debugMode = true;
            }
            else
            {
                _debugMode = false;
            }
        }
#endif
    }
}
