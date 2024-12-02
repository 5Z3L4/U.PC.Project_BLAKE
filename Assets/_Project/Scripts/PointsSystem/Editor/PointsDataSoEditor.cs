using System.Linq;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.PointsSystem
{
    [CustomEditor(typeof(PointsDataSo))]
    public class PointsDataSoEditor : Editor
    {
        private const string DESTINATION_FOLDER = "Assets/_Project/Prefabs/Enemy";
        
        public override void OnInspectorGUI()
        {
            if(GUILayout.Button("Refresh points on enemies", GUILayout.Height(40)))
            {
                RefreshDataOnEnemies();
            }
        
            base.OnInspectorGUI();
        }

        private void RefreshDataOnEnemies()
        {
            var guids = AssetDatabase.FindAssets("t:prefab", new[] { DESTINATION_FOLDER });
            
            if (guids.Length <= 0)
            {
                Debug.LogError($"Found {guids.Length} files! ABORTING!");
                return;
            }

            var pointsDataSo = (PointsDataSo)target;
            var paths = guids.Select(AssetDatabase.GUIDToAssetPath).ToList();
            var enemies = paths.Select(AssetDatabase.LoadAssetAtPath<GameObject>).ToList();

            foreach (var enemy in enemies)
            {
                var enemyPointsData = enemy.GetComponent<EnemyPointsData>();

                if (enemyPointsData == null)
                {
                    Debug.Log($"GameObject: |||{DESTINATION_FOLDER}/{enemy.name}||| doesn't have enemyPointsData! If it's not a enemy, ignore that message");
                }
                else if (enemyPointsData.EnemyTypeEnum == EnemyTypeEnum.Undefined)
                {
                    Debug.Log($"Enemy from: |||{DESTINATION_FOLDER}/{enemyPointsData.name}||| has Undefined EnemyTypeEnum! If it's base prefab, ignore that message");
                }
                else
                {
                    SetValues(pointsDataSo, enemyPointsData);
                }
            }
        }

        private void SetValues(PointsDataSo pointsDataSo, EnemyPointsData enemyPointsData)
        {
            foreach (var pointsData in pointsDataSo.enemyData)
            {
                if (pointsData.enemyTypeEnum == enemyPointsData.EnemyTypeEnum)
                {
                    enemyPointsData.SetPointsForKill(pointsData.pointsForKill);
                    EditorUtility.SetDirty(enemyPointsData.gameObject);
                    return;
                }
            }
        }
    }
}
