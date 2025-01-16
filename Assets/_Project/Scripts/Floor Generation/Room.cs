using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System;
using _Project.Scripts;
using _Project.Scripts.Floor_Generation;
using Cinemachine;
using _Project.Scripts.GlobalHandlers;
using _Project.Scripts.Weapons;

public enum RoomType
{
    Base,
    Treasure,
    Shop,
    Boss
}

public enum RoomSize
{
    Small,
    Medium,
    Large
}

public class Room : MonoBehaviour
{
    [SerializeField]
    private RoomType roomType = RoomType.Base;
    public RoomSize roomSize = RoomSize.Small;

    [SerializeField]
    private RoomConnector[] doors;

    [SerializeField]
    private RandomizedRoomObject[] randomObjects;

    private RoomManager roomManager;

    [SerializeField, Header("Minimap variables")]
    private MinimapRoom minimapRoom;

    [SerializeField]
    private BoxCollider[] overlapColliders;

    [SerializeField, Header("Enemies")]
    private List<EnemySpawner> spawners = new List<EnemySpawner>();

    [SerializeField]
    private List<EnemyCharacter> spawnedEnemies;

    [SerializeField]
    private bool isInitialized = false;

    [SerializeField]
    private bool isBeaten = false;
    public bool IsBeaten => isBeaten;

    [SerializeField]
    private Transform spawnPoint;
    [SerializeField]
    private List<GameObject> respawnableObjects;
    private List<Respawnable> respawnables = new List<Respawnable>();
    private List<GameObject> spawnedRespawnables = new List<GameObject>();

    [Header("Fog")]
    private bool isControlPerkActivated = false;
    private bool isControlPerkThreeActivated = false;
    [SerializeField]
    private GameObject fog;
    [SerializeField]
    private GameObject fogBlockerPrefab;
    [SerializeField]
    private int fogBlockerAmount = 1;
    [SerializeField]
    private int fogBlockerUpgradeAmount = 2;
    [SerializeField]
    private float fogBlockerSize = 1f;
    [SerializeField]
    private Transform[] fogPoints;
    [SerializeField]
    private List<GameObject> activefogBlockers = new List<GameObject>();
    [SerializeField]
    private CinemachineVirtualCamera peekCamera;

    private BlakeHeroCharacter blakeHeroCharacter;
    private List<RoomTrigger> triggers = new List<RoomTrigger>();
    private List<RoomOverlapTrigger> fogTriggers = new List<RoomOverlapTrigger>();
    private RoomsDoneCounter roomsDoneCounter;
    private List<WeaponPickup> instantiatedWeapons;
    private bool anyEnemyAlive = false;

    [HideInInspector]
    public int gCost;
    [HideInInspector]
    public int hCost;
    [HideInInspector]
    public int fCost;

    [HideInInspector]
    public Room cameFromRoom;

    public List<EnemyCharacter> SpawnedEnemiesList => spawnedEnemies;

    public delegate void OnEnemySpawn(EnemyCharacter character);
    public event OnEnemySpawn enemySpawned;
    public event Action OnResetEnemies;

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }

    [Serializable]
    public struct EnemySpawner
    {
        public Transform EnemySpawnPoint;
        public GameObject EnemyToSpawn;
        public Waypoints EnemyWaypoints;
    }

    private void Awake()
    {
        roomsDoneCounter = FindObjectOfType<RoomsDoneCounter>();
        instantiatedWeapons = new List<WeaponPickup>();
    }

    private void SpawnRespawnables()
    {
        respawnables.Clear();
        foreach (var respawnable in respawnableObjects)
        {
            respawnables.Add(new Respawnable(respawnable, respawnable.transform));
            respawnable.SetActive(false);
        }

        foreach(var respawnable in respawnables)
        {
            var go = Instantiate(respawnable.original, respawnable.transform.position, respawnable.transform.rotation, respawnable.transform.parent);
            go.SetActive(true);
        }
    }

    public void SetupDoorConnectors()
    {
        foreach (RoomConnector door in doors)
        {
            door.SetRoom(this);
            door.SetDoor();
        }
    }

    public void SetControlPerkOne(bool value)
    {
        isControlPerkActivated = value;
    }
    public void SetControlPerkThree(bool value)
    {
        isControlPerkThreeActivated = value;
    }

    public void Peek()
    {
        if (peekCamera == null) return;
        ReferenceManager.PlayerInputController.onPeekingCancel += StopPeek;
        roomManager.GetComponent<FloorManager>().GetMainCamera().enabled = false;
        ReferenceManager.PlayerInputController.GetComponent<PlayerMovement>().Peek(this);
        for (int i = 0; i < ((isControlPerkThreeActivated) ? fogBlockerUpgradeAmount : fogBlockerAmount); i++)
        {
            activefogBlockers[i].gameObject.SetActive(true);
        }
        peekCamera.gameObject.SetActive(true);
        fog.GetComponent<Fog>().Peek();
    }

    public void StopPeek()
    {
        if (peekCamera == null) return;
        roomManager.GetComponent<FloorManager>().GetMainCamera().enabled = true;
        peekCamera.gameObject.SetActive(false);
        fog.GetComponent<Fog>().TurnOffFog();
        foreach (var fogBlocker in activefogBlockers)
        {
            fogBlocker.SetActive(false);
        }
        ReferenceManager.PlayerInputController.onPeekingCancel -= StopPeek;
    }

    public bool HavePeekingCam()
    {
        return peekCamera != null;
    }

    public bool CanBePeeked()
    {
        if (!HavePeekingCam()) return false;
        if (isBeaten) return false;
        if (spawnedEnemies.Count > 0 && IsPlayerInside()) return false;

        return true;
    }
    public void SetupFogBlockers()
    {
        if (fogBlockerPrefab == null) return;
        if (fogPoints.Length == 0) return;
        List<Transform> avaiblePoints = new List<Transform>(fogPoints);
        for (int i = 0; i < fogBlockerUpgradeAmount; i++)
        {
            var point = avaiblePoints[UnityEngine.Random.Range(0, avaiblePoints.Count)];
            var fogBlocker = Instantiate(fogBlockerPrefab, point.position, Quaternion.identity, this.transform);
            fogBlocker.transform.localScale *= fogBlockerSize;
            activefogBlockers.Add(fogBlocker);
            avaiblePoints.Remove(point);

            fogBlocker.SetActive(false);

            if (avaiblePoints.Count == 0) break;
        }
    }

    public void AddToBlockerScale(float scale)
    {
        fogBlockerSize += scale;

        foreach (var blocker in activefogBlockers)
        {
            blocker.transform.localScale = Vector3.one;
            blocker.transform.localScale *= fogBlockerSize;
        }
    }

    public void ActivateAllBlockers()
    {
        isControlPerkThreeActivated = true;
    }

    public void InitializeRoom(RoomManager rm)
    {
        roomManager = rm;
        foreach (RandomizedRoomObject randomObject in randomObjects)
        {
            randomObject.InitializeRandomObject();
        }


        if (minimapRoom != null && roomManager.GetMinimapFloor() != null)
        {
            minimapRoom.transform.parent = roomManager.GetMinimapFloor();
        }

        if (fog != null)
        {
            fog.SetActive(true);
        }

        //Build NavMesh
        NavMeshSurface[] surfaces = GetComponentsInChildren<NavMeshSurface>();
        foreach (NavMeshSurface surface in surfaces)
        {
            surface.BuildNavMesh();
        }


        foreach (RoomConnector roomConnector in doors)
        {
            roomConnector.UnlockDoor();
            roomConnector.CloseDoor();
        }

        SetupFogBlockers();
        SpawnEnemies();
        if(!isBeaten)
        {
            SpawnRespawnables();
        }
        isInitialized = true;
    }

    private void SpawnEnemies()
    {
        if (spawners.Count <= 0)
        {
            return;
        }

        anyEnemyAlive = true;

        //Spawn enemies
        foreach (EnemySpawner enemy in spawners)
        {
            var spawnedEnemyCharacter =
                Instantiate(enemy.EnemyToSpawn.gameObject, enemy.EnemySpawnPoint.transform.position,
                    enemy.EnemySpawnPoint.rotation, this.transform).GetComponent<EnemyCharacter>();
            spawnedEnemyCharacter.GetComponent<AIController>().SetWaypoints(enemy.EnemyWaypoints);
            spawnedEnemyCharacter.SpawnedInRoom = this;
            spawnedEnemyCharacter.onDeath += RemoveEnemyFromList;
            spawnedEnemies.Add(spawnedEnemyCharacter);
            enemySpawned?.Invoke(spawnedEnemyCharacter);
        }
    }

    public RoomType GetRoomType()
    {
        return roomType;
    }

    private void BeatLevel()
    {
        blakeHeroCharacter.onRespawn += ResetRoom;
        roomsDoneCounter.AddBeatenRoom();
        isBeaten = true;
        minimapRoom.CompleteRoom();
    }

    public RoomConnector[] GetDoors()
    {
        return doors;
    }

    public RoomConnector[] GetFreeDoors()
    {
        List<RoomConnector> freeDoors = new List<RoomConnector>();
        foreach (RoomConnector door in doors)
        {
            if (door.GetConnector() == null) freeDoors.Add(door);
        }
        return freeDoors.ToArray();
    }

    public void SeeRoom()
    {
        minimapRoom.ShowRoom();
    }

    public void DisableFog()
    {
        if (IsPlayerInsideFog()) return;
        fog.SetActive(false);
    }

    public void EnableFog()
    {
        if (IsPlayerInsideFog()) return;
        fog.SetActive(true);
    }

    public void DisableRoom()
    {
        gameObject.SetActive(false);
        var toDelete = new List<WeaponPickup>();
        foreach (var weapon in instantiatedWeapons)
        {
            if (weapon != null)
            {
                weapon.gameObject.SetActive(false);
            }
            else
            {
                toDelete.Add(weapon);
            }
        }

        foreach (var delete in toDelete)
        {
            instantiatedWeapons.Remove(delete);
        }
    }

    public void EnterRoom()
    {
        if (IsPlayerInside()) return;
        Room activeRoom = roomManager.GetActiveRoom();
        if (activeRoom == this) return;
        minimapRoom.VisitRoom();
        fog.SetActive(false);
        if (fog.GetComponent<Fog>() != null)
        {
            fog.GetComponent<Fog>().DisableFog();
        }

        List<Room> roomsToActivate = GetNeigbours();
        if (roomsToActivate.Contains(activeRoom)) roomsToActivate.Remove(activeRoom);


        foreach (Room room in roomsToActivate)
        {
            room.SeeRoom();
            room.gameObject.SetActive(true);
        }

        roomManager.SetPreviousRoom(roomManager.GetActiveRoom());
        roomManager.SetActiveRoom(this);

        if (!isBeaten)
        {
            if (spawnedEnemies.Count == 0)
            {
                BeatLevel();
            }
            else
            {
                foreach (RoomConnector roomConnector in doors)
                {
                    if (roomConnector.GetConnector() != null)
                    {
                        roomConnector.GetConnector().LockDoor();
                    }
                    roomConnector.LockDoor();
                }
            }
        }
        if (isBeaten && blakeHeroCharacter != null)
        {
            blakeHeroCharacter.SetRespawnPosition(GetSpawnPointPosition());
        }

        foreach (var enemy in spawnedEnemies)
        {
            if (enemy == null) continue;
            enemy.GetComponent<AIController>().UpdatePlayerRef();
        }

        var toDelete = new List<WeaponPickup>();
        foreach (var weapon in instantiatedWeapons)
        {
            if (weapon != null)
            {
                weapon.gameObject.SetActive(true);
            }
            else
            {
                toDelete.Add(weapon);
            }
        }

        foreach (var delete in toDelete)
        {
            instantiatedWeapons.Remove(delete);
        }
    }

    public void SetPlayer(GameObject player)
    {
        blakeHeroCharacter = player.GetComponent<BlakeHeroCharacter>();
        blakeHeroCharacter.onRespawn += ResetRoom;
    }

    private void ResetRoom()
    {
        if (roomManager.GetActiveRoom() != this) return;
        roomManager.SetActiveRoom(roomManager.GetPreviousRoom());
        if (isBeaten) return;
        foreach (RoomConnector roomConnector in doors)
        {
            roomConnector.UnlockDoor();
            if (roomConnector.GetConnector() != null)
            {
                roomConnector.GetConnector().UnlockDoor();
            }
        }
        minimapRoom.ForgetRoom();

        foreach (RoomTrigger rt in triggers)
        {
            rt.Reset();
        }

        foreach (RoomOverlapTrigger rt in fogTriggers)
        {
            rt.Reset();
        }

        Invoke("ResetEnemies", 0.5f);
        
        foreach (var weapon in instantiatedWeapons.ToArray())
        {
            Destroy(weapon.gameObject);
            instantiatedWeapons.Remove(weapon);
        }

        foreach(var go in spawnedRespawnables)
        {
            if(go != null)
            {
                Destroy(go);
            }
        }

        if (blakeHeroCharacter != null)
        {
            blakeHeroCharacter.onRespawn -= ResetRoom;
        }
        fog.SetActive(true);
        fog.GetComponent<Fog>().TurnOffFog();
    }

    private void ResetEnemies()
    {
        foreach (var enemy in spawnedEnemies)
        {
            Destroy(enemy.gameObject);
        }
        SpawnRespawnables();
        OnResetEnemies?.Invoke();
        spawnedEnemies.Clear();
        SpawnEnemies();
    }

    private void Update()
    {
        if (!isBeaten && isInitialized && (IsPlayerInsideFog() || IsPlayerInside()) && roomManager.GetActiveRoom() == this)
        {
            if (spawnedEnemies.Count == 0)
            {
                if (blakeHeroCharacter == null) return;
                BeatLevel();
                foreach (RoomConnector roomConnector in doors)
                {
                    roomConnector.UnlockDoor();
                    if (roomConnector.GetConnector() != null)
                    {
                        roomConnector.GetConnector().UnlockDoor();
                    }
                }
                if (blakeHeroCharacter != null)
                {
                    blakeHeroCharacter.SetRespawnPosition(GetSpawnPointPosition());
                }
            }
        }

        if (roomManager != null)
        {
            if (!isBeaten && roomManager.GetActiveRoom() == this && (!IsPlayerInside() && !IsPlayerInsideFog()) && spawnedEnemies.Count > 0)
            {
                ResetRoom();
            }
        }
    }

    public void ExitRoom()
    {
        if (blakeHeroCharacter != null)
        {
            blakeHeroCharacter.onRespawn -= ResetRoom;
        }
        if (IsPlayerInside()) return;
        /*if(roomManager.GetActiveRoom() == this)
        {
            roomManager.SetActiveRoom(null);

        }*/

    }

    public List<Room> GetNeigbours()
    {
        List<Room> neigbours = new List<Room>();
        foreach (RoomConnector door in doors)
        {
            RoomConnector neighbour = door.GetConnector();
            if (neighbour == null) continue;

            neigbours.Add(neighbour.GetRoom());
        }
        return neigbours;
    }

    public bool IsPlayerInside()
    {
        foreach (RoomTrigger rt in triggers)
        {
            if (rt.IsPlayerInside()) return true;
        }
        return false;
    }

    public bool IsPlayerInsideFog()
    {
        foreach (RoomOverlapTrigger rt in fogTriggers)
        {
            if (rt.IsPlayerInside()) return true;
        }
        return false;
    }

    public void AddTrigger(RoomTrigger rt)
    {
        triggers.Add(rt);
    }

    public void AddFogTrigger(RoomOverlapTrigger rt)
    {
        fogTriggers.Add(rt);
    }

    public BoxCollider[] GetOverlapColliders()
    {
        return overlapColliders;
    }

    public RoomManager GetRoomManager()
    {
        return roomManager;
    }

    public Vector3 GetSpawnPointPosition()
    {
        if (spawnPoint != null)
        {
            return spawnPoint.position;
        }
        else return transform.position;
    }

    private void RemoveEnemyFromList(BlakeCharacter blakeCharacter)
    {
        spawnedEnemies.Remove(blakeCharacter as EnemyCharacter);

        if (spawnedEnemies.Count <= 0)
        {
            anyEnemyAlive = false;
        }
    }

    public void AddSpawnedWeapon(WeaponPickup weapon)
    {
        instantiatedWeapons.Add(weapon);
    }

    private void OnDestroy()
    {
        if (blakeHeroCharacter == null) return;
        blakeHeroCharacter.onRespawn -= ResetRoom;
    }

    ~Room()
    {
        blakeHeroCharacter.onRespawn -= ResetRoom;
    }
}
