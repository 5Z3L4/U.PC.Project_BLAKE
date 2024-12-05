
using UnityEngine;

namespace _Project.Scripts.SceneHandlers
{
    [CreateAssetMenu(fileName = "LevelList", menuName = "Scene Management/Level List")]
    public class LevelList : ScriptableObject
    {
        public string[] levelNames;
    }
}
