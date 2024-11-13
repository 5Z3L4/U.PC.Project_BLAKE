using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Floor_Generation;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.PointsSystem;
using _Project.Scripts.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI.Gameplay
{
    public class PlayerGameplayUIManager : MonoBehaviour
    {
        private const string INFINITY_SYMBOL = "∞";
    
        [SerializeField] 
        private FloorManager floorManager;

        [SerializeField] 
        private MinimapCameraFollow minimapCamera;

        [SerializeField, Space] 
        private TextMeshProUGUI weaponName;

        [SerializeField] 
        private TextMeshProUGUI bulletsLeft;

        [SerializeField] 
        private TextMeshProUGUI healthLeft;
        
        [SerializeField] 
        private TextMeshProUGUI respawnsLeft;
    
        [SerializeField] 
        private TextMeshProUGUI pointsCounter;
    
        [SerializeField] 
        private TextMeshProUGUI killsCounter;
    
        [SerializeField] 
        private TextMeshProUGUI comboCounter;

        [SerializeField] 
        private GameObject interactUI;
        [SerializeField]
        private GameObject altInteractUI;

        [SerializeField]
        private GameObject mapUI;

        [SerializeField] 
        private GameObject dashCooldownUI;
        [SerializeField]
        private GameObject dashCooldownSprite;
        private Dictionary<GameObject, Dash> dashSprites = new Dictionary<GameObject, Dash>();

        [SerializeField]
        private GameObject controlOnePerkObject;
        [SerializeField]
        private GameObject controlOnePerkText;

        private GameObject player;
        private WeaponsManager weaponsManager;
        private PlayerInteractables playerInteractables;
        private BlakeHeroCharacter blakeHeroCharacter;
        private PlayerMovement playerMovement;
        private Image dashCooldownImage;

        private void Start()
        {
            floorManager.FloorGeneratorEnd += FloorManagerOnFloorGeneratorEnd;
        }

        private void FloorManagerOnFloorGeneratorEnd(Transform playerTransform, Transform cameraFollowTransform)
        {
            ReferenceManager.PlayerInputController.onMapPressEvent += ShowMap;
            ReferenceManager.PlayerInputController.onMapReleaseEvent += HideMap;
            floorManager.FloorGeneratorEnd -= FloorManagerOnFloorGeneratorEnd;
        
            if (player == null || weaponsManager == null)
            {
                player = playerTransform.gameObject;
                weaponsManager = player.GetComponent<WeaponsManager>();
                weaponsManager.OnPrimaryAttack += RefreshUI;
                weaponsManager.OnPlayerPickupWeaponEvent += RefreshUI;
                weaponsManager.OnWeaponChangedEvent += RefreshUI;
                RefreshUI(weaponsManager.Weapons[weaponsManager.ActiveWeaponIndex]);
            }
        
            minimapCamera.SetPlayer(playerTransform);
        
            playerInteractables = player.GetComponent<PlayerInteractables>();
            blakeHeroCharacter = player.GetComponent<BlakeHeroCharacter>();
            playerMovement = player.GetComponent<PlayerMovement>();

            playerInteractables.SetInteractUIReference(interactUI, altInteractUI);
            //dashCooldownImage = dashCooldownUI.transform.GetChild(1).GetComponent<Image>();

            EnemyDeathMediator.Instance.OnRegisteredEnemyDeath += UpdatePointsAndCombo;
            ReferenceManager.PlayerCurrencyController.OnPointsChanged += RefreshPoints;
            EnemyDeathMediator.Instance.ComboController.OnComboTimerEnd += HideComboTexts;
            HideComboTexts();
        
            playerMovement.OnDashPerformed += StartDashCooldownUI;
            playerMovement.OnDashAdded += OnAddDash;
            playerMovement.OnDashRemoved += OnRemoveDash;
            blakeHeroCharacter.OnDamageTaken += HealthLeftUI;
            blakeHeroCharacter.onRespawn += OnRespawnUIUpdate;

            playerMovement.OnPeek += ShowText;
            ReferenceManager.PlayerInputController.onPeekingCancel += HideText;
            OnRespawnUIUpdate();
            RefreshDashUI();
        }

        private void RefreshDashUI()
        {
            foreach(var dash in dashSprites)
            {
                Destroy(dash.Key);
            }
            dashSprites.Clear();
            foreach(var dash in playerMovement.Dashes)
            {
                GameObject sprite = Instantiate(dashCooldownSprite, dashCooldownUI.transform);
                dashSprites.Add(sprite, dash);
            }
        }

        private void ShowText(Room room)
        {
            if (!ReferenceManager.RoomManager.isControlOneActivated) return;
            controlOnePerkText.GetComponent<TMP_Text>().text = "Enemies inside: " + room.SpawnedEnemiesList.Count.ToString();
            controlOnePerkText.SetActive(true);
        }

        private void HideText()
        {
            controlOnePerkText.SetActive(false);
        }

        private void ShowMap()
        {
            mapUI.SetActive(true);
        }

        private void HideMap()
        {
            mapUI.SetActive(false);
        }

        private void RefreshUI(Weapon weapon)
        {
            WeaponNameUI(weapon);
            BulletsLeftUI(weapon);
            RefreshPoints(PlayerCurrencyController.Instance.Points);
        }

        private void WeaponNameUI(Weapon weapon)
        {
            weaponName.text = weapon.WeaponDefinition.WeaponName;
        }

        private void BulletsLeftUI(Weapon weapon)
        {
            var rangedWeapon = weapon as RangedWeapon;

            bulletsLeft.text = rangedWeapon != null ? rangedWeapon.BulletsLeft.ToString() : INFINITY_SYMBOL;
        }

        private void HealthLeftUI(GameObject instigator)
        {
            healthLeft.text = blakeHeroCharacter.Health.ToString();
        }

        private void OnRespawnUIUpdate()
        {
            HealthLeftUI(null);
            respawnsLeft.text = blakeHeroCharacter.RespawnsLeft.ToString();
        }

        private void UpdatePointsAndCombo(ComboAndPointsValues comboAndPointsValues)
        {
            RefreshPoints(comboAndPointsValues.Points);
            
            if (!comboAndPointsValues.ShouldComboStart)
            {
                return;
            }
        
            var killsCounterActive = killsCounter.gameObject.activeInHierarchy;
            var comboCounterActive = comboCounter.gameObject.activeInHierarchy;
        
            if (!killsCounterActive || !comboCounterActive)
            {
                killsCounter.gameObject.SetActive(true);
                comboCounter.gameObject.SetActive(true);
            }
        
            killsCounter.text = $"x{comboAndPointsValues.KillsCounter} KILLS";
            comboCounter.text = $"x{comboAndPointsValues.ComboCounter} Points";
        }

        private void RefreshPoints(float points)
        {
            pointsCounter.text = $"Points: {points}";
        }

        private void HideComboTexts()
        {
            var killsCounterActive = killsCounter.gameObject.activeInHierarchy;
            var comboCounterActive = comboCounter.gameObject.activeInHierarchy;
            if (killsCounterActive || comboCounterActive)
            {
                killsCounter.gameObject.SetActive(false);
                comboCounter.gameObject.SetActive(false);
            }
        }

        private void OnAddDash(Dash dash)
        {
            GameObject sprite = Instantiate(dashCooldownSprite, dashCooldownUI.transform);
            dashSprites.Add(sprite, dash);
        }

        private void OnRemoveDash(Dash dash)
        {
            List<GameObject> toRemove = new List<GameObject>();
            foreach(var keyValue in dashSprites)
            {
                if(keyValue.Value == dash)
                {
                    toRemove.Add(keyValue.Key);
                }
            }
            foreach(var keyValue in toRemove)
            {
                dashSprites.Remove(keyValue);
                Destroy(keyValue);
            }
        }

        private void StartDashCooldownUI()
        {
            StopCoroutine(DashCooldownUI());
            StartCoroutine(DashCooldownUI());
        }

        private IEnumerator DashCooldownUI()
        {
            dashCooldownUI.SetActive(true);
            bool showUI = true;
            while(showUI)
            {
                bool allOffCooldown = false;
                foreach(var dash in dashSprites)
                {
                    dash.Key.GetComponent<Image>().fillAmount = (playerMovement.DashCooldown - dash.Value.dashTimer) / playerMovement.DashCooldown;
                    if(dash.Value.dashTimer > 0)
                    {
                        allOffCooldown = true;
                    }
                }
                dashCooldownUI.transform.position = player.transform.position - Camera.main.transform.forward * 2f;
                dashCooldownUI.transform.LookAt(Camera.main.transform);
                if (!allOffCooldown) showUI = false;
                yield return new WaitForEndOfFrame();

            }

            /*while(playerMovement.DashCooldownCountdown > 0)
            {
                dashCooldownImage.fillAmount = playerMovement.DashCooldownCountdown/playerMovement.DashCooldown;
                dashCooldownUI.transform.position = player.transform.position + Vector3.up * 0.2f;
                dashCooldownUI.transform.LookAt(Camera.main.transform);

                yield return new WaitForEndOfFrame();
            }
        
            dashCooldownImage.fillAmount = 0;*/
            dashCooldownUI.SetActive(false);
        }

        private void OnDestroy()
        {
            playerMovement.OnDashPerformed -= StartDashCooldownUI;

            playerMovement.OnDashAdded -= OnAddDash;
            playerMovement.OnDashRemoved -= OnRemoveDash;
            blakeHeroCharacter.OnDamageTaken -= HealthLeftUI;
            blakeHeroCharacter.onRespawn -= OnRespawnUIUpdate;

            ReferenceManager.PlayerInputController.onMapPressEvent -= ShowMap;
            ReferenceManager.PlayerInputController.onMapReleaseEvent -= HideMap;

            EnemyDeathMediator.Instance.OnRegisteredEnemyDeath -= UpdatePointsAndCombo;
            ReferenceManager.PlayerCurrencyController.OnPointsChanged -= RefreshPoints;
            EnemyDeathMediator.Instance.ComboController.OnComboTimerEnd -= HideComboTexts;


            playerMovement.OnPeek -= ShowText;
            ReferenceManager.PlayerInputController.onPeekingCancel -= HideText;
        }
    }
}
