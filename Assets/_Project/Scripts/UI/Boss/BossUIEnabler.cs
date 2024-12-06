using System;
using _Project.Scripts.EnemyBoss.Shield;
using _Project.Scripts.GlobalHandlers;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Project.Scripts.UI.Boss
{
    public class BossUIEnabler : MonoBehaviour
    {
        [SerializeField] private GameObject healthBar;
        
        private bool _subscribedToRoomManager = false;
        private bool _subscribedToLevelHandler = false;
        private bool _subscribedToBoss = false;
        private bool _subscribedToPlayer = false;

        private bool EventsSubscribed => _subscribedToLevelHandler && _subscribedToRoomManager && _subscribedToBoss && _subscribedToPlayer;
        
        public Room BossRoom { get; set; }
        public BossCharacter BossCharacter { get; set; }

        private void Update()
        {
           TrySubscribeToEvents();
        }

        private void TrySubscribeToEvents()
        {
            if (EventsSubscribed)
            {
                return;
            }

            if (!_subscribedToRoomManager && ReferenceManager.RoomManager != null)
            {
                var manager = ReferenceManager.RoomManager;
                manager.onRoomEnter += TryEnableHealthBar;
                manager.onRoomLeave += TryDisableHealthBar;
                _subscribedToRoomManager = true;
            }

            if (!_subscribedToLevelHandler && ReferenceManager.LevelHandler != null)
            {
                ReferenceManager.LevelHandler.onNextLevel += RefreshRoomManagerEvents;
                _subscribedToLevelHandler = true;
            }

            if (!_subscribedToBoss && BossCharacter != null)
            {
                BossCharacter.onDeath += OnBossKilled;
                _subscribedToBoss = true;
            }
            
            if (!_subscribedToPlayer && ReferenceManager.BlakeHeroCharacter != null)
            {
                ReferenceManager.BlakeHeroCharacter.onDeath += OnPlayerDeath;
                _subscribedToPlayer = true;
            }
        }

        private void TryEnableHealthBar(Room room)
        {
            if (room == null)
            {
                return;
            }
            
            if (room.GetRoomType() == RoomType.Boss && !room.IsBeaten)
            {
                BossRoom = room;
                BossCharacter = room.SpawnedEnemiesList[0] as BossCharacter;
                healthBar.SetActive(true);
            }
        }

        private void TryDisableHealthBar(Room room)
        {
            if (room.GetRoomType() == RoomType.Boss)
            {
                healthBar.SetActive(false);
            }
        }

        private void ForceDisableHeathBar()
        {
            healthBar.SetActive(false);
        }

        private void RefreshRoomManagerEvents()
        {
            var manager = ReferenceManager.RoomManager;
            if (manager != null)
            {
                manager.onRoomEnter -= TryEnableHealthBar;
                manager.onRoomLeave -= TryDisableHealthBar;
            }

            BossRoom = null;
            BossCharacter = null;
            _subscribedToRoomManager = false;
        }

        private void OnPlayerDeath(BlakeCharacter _)
        {
            OnBossKilled(null);
        }
        
        private void OnBossKilled(BlakeCharacter _)
        {
            BossKilled().Forget();

            if (BossCharacter != null)
            {
                BossCharacter.onDeath -= OnBossKilled;
            }

            BossCharacter = null;
        }

        private async UniTaskVoid BossKilled()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            ForceDisableHeathBar();
        }

        private void OnDestroy()
        {
            if (ReferenceManager.RoomManager != null)
            {
                var manager = ReferenceManager.RoomManager;
                manager.onRoomEnter -= TryEnableHealthBar;
                manager.onRoomLeave -= TryDisableHealthBar;
            }

            if (ReferenceManager.LevelHandler != null)
            {
                ReferenceManager.LevelHandler.onNextLevel -= RefreshRoomManagerEvents;
            }

            if (BossCharacter != null)
            {
                BossCharacter.onDeath -= OnBossKilled;
            }
            
            if (ReferenceManager.BlakeHeroCharacter != null)
            {
                ReferenceManager.BlakeHeroCharacter.onDeath -= OnPlayerDeath;
            }
        }
    }
}
