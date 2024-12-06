using System.Reflection;
using _Project.Scripts.ConsoleCommands;
using _Project.Scripts.Patterns;
using SickDev.DevConsole;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Project.Scripts.GlobalHandlers
{
    public class GlobalStartSettings : Singleton<GlobalStartSettings>
    {
        [SerializeField] private bool _enableDeveloperConsole;
        [SerializeField] private bool _removeURPDebug;
        [SerializeField] private bool _enableEnemies;
        
        [Space]
        [SerializeField] private BaseCommand _devConsoleCommandsPrefab;

        public bool EnableEnemies => _enableEnemies;
        
        private void Start()
        {
            if (_enableDeveloperConsole)
            {
                EnableDeveloperConsole();
            }

            if (_removeURPDebug)
            {
                RemoveURPDebug();
            }
        }

        private void EnableDeveloperConsole()
        {
            var xd = DevConsole.singleton;
            Instantiate(_devConsoleCommandsPrefab);
        }

        private void RemoveURPDebug()
        {
            var fieldInfo = typeof(UnityEngine.Rendering.DebugManager).GetField("debugActionMap",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var debugActionMap = (InputActionMap)fieldInfo.GetValue(UnityEngine.Rendering.DebugManager.instance);
            debugActionMap.Disable();
        }
    }
}
