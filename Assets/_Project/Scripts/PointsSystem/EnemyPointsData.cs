using UnityEngine;

namespace _Project.Scripts.PointsSystem
{
    public class EnemyPointsData : MonoBehaviour
    {
        [SerializeField] 
        private EnemyTypeEnum enemyTypeEnum;
        
        [SerializeField, HideInInspector] 
        private int pointsForKill;
        
        private EnemyCharacter enemyCharacter;

        public EnemyTypeEnum EnemyTypeEnum => enemyTypeEnum;

        private void Awake()
        {
            enemyCharacter = GetComponent<EnemyCharacter>();
            enemyCharacter.onDeath += EnemyCharacterOnDeath;
        }

        public void SetPointsForKill(int value)
        {
            pointsForKill = value;
        }

        private void EnemyCharacterOnDeath(BlakeCharacter blakeCharacter)
        {
            EnemyDeathMediator.Instance.RegisterEnemyDeath(pointsForKill, enemyTypeEnum);
            UnregisterEvent();
        }

        private void UnregisterEvent()
        {
            enemyCharacter.onDeath -= EnemyCharacterOnDeath;
        }

        private void OnDestroy()
        {
            UnregisterEvent();
        }
        
    }
}
