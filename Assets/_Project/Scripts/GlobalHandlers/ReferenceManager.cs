using System;
using System.Threading.Tasks;
using _Project.Scripts.Patterns;
using _Project.Scripts.Player;
using _Project.Scripts.PointsSystem;
using _Project.Scripts.SceneHandlers;
using _Project.Scripts.VirtualCamera;
using _Project.Scripts.Weapons.Upgrades;
using Unity.Services.Core;

namespace _Project.Scripts.GlobalHandlers
{
    public class ReferenceManager : Singleton<ReferenceManager>
    {
        private BlakeHeroCharacter blakeHeroCharacter;
        private PlayerInputController playerInputController;
        private SceneHandler sceneHandler;
        private LevelHandler levelHandler;
        private MessageRouter messageRouter = new();
        private RoomManager roomManager;
        private MainVirtualCameraController mainVirtualCameraController;
        private WeaponUpgradeManager weaponUpgradeManager;
        private PlayerCurrencyController playerCurrencyController;

        public event Action OnFloorLoad;

        public static BlakeHeroCharacter BlakeHeroCharacter
        {
            get => Instance != null ? Instance.blakeHeroCharacter : null;
            set
            {
                if (Instance == null) return;
                Instance.blakeHeroCharacter = value;
            }
        }
    
        public static PlayerInputController PlayerInputController
        {
            get => Instance != null ? Instance.playerInputController : null;
            set
            {
                if (Instance == null) return;   
                Instance.playerInputController = value;
            }
        }
    
        public static SceneHandlers.SceneHandler SceneHandler
        {
            get => Instance != null ? Instance.sceneHandler : null;
            set
            {
                if (Instance == null) return;
                Instance.sceneHandler = value;
            }
        }

        public static LevelHandler LevelHandler
        {
            get => Instance != null ? Instance.levelHandler : null;
            set
            {
                if (Instance == null) return;
                Instance.levelHandler = value;
            }
        }

        public static MessageRouter MessageRouter
        {
            get => Instance != null ? Instance.messageRouter : null;
        }

        public static RoomManager RoomManager
        {
            get => Instance != null ? Instance.roomManager : null;
            set
            {
                if (Instance == null) return;
                Instance.roomManager = value;
            }
        }
        
        public static MainVirtualCameraController MainVirtualCameraController
        {
            get => Instance != null ? Instance.mainVirtualCameraController : null;
            set
            {
                if (Instance == null) return;
                Instance.mainVirtualCameraController = value;
            }
        }
        
        public static WeaponUpgradeManager WeaponUpgradeManager
        {
            get => Instance != null ? Instance.weaponUpgradeManager : null;
            set
            {
                if (Instance == null) return;
                Instance.weaponUpgradeManager = value;
            }
        }
        
        public static PlayerCurrencyController PlayerCurrencyController
        {
            get => Instance != null ? Instance.playerCurrencyController : null;
            set
            {
                if (Instance == null) return;
                Instance.playerCurrencyController = value;
            }
        }

        protected override void Awake()
        {
            base.Awake();
#if ENABLE_CLOUD_SERVICES_ANALYTICS
            Task initializationTask = UnityServices.InitializeAsync();
#endif
        }

        public void OnFloorGenEnd()
        {
            OnFloorLoad?.Invoke();
        }
    }
}
