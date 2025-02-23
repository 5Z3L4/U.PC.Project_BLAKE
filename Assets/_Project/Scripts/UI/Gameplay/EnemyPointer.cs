using System.Collections;
using System.Collections.Generic;
using _Project.Scripts;
using _Project.Scripts.GlobalHandlers;
using UnityEngine;

public class EnemyPointer : MonoBehaviour
{
    [SerializeField]
    private GameObject arrowPrefab;
    private List<GameObject> arrows = new List<GameObject>();
    private List<Transform> offScreenEnemies = new List<Transform>();
    private Transform player;
    private RoomManager roomManager;

    private void Start()
    {
        roomManager = ReferenceManager.RoomManager;
        player = ReferenceManager.BlakeHeroCharacter?.transform;
    }

    private void Update()
    {
        if(player == null)
        {
            player = ReferenceManager.BlakeHeroCharacter?.transform;
        }
        if (player == null || roomManager == null) return;
        offScreenEnemies.Clear();
        var list = roomManager.GetActiveRoom()?.SpawnedEnemiesList;
        if (list == null) return;
        foreach (var enemy in list)
        {
            if (enemy == null) continue;
            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(enemy.transform.position);

            if (viewportPosition.x <= 0 || viewportPosition.x >= 1 || viewportPosition.y <= 0 || viewportPosition.y >= 1)
            {
                offScreenEnemies.Add(enemy.transform);
            }
        }
        while (arrows.Count < offScreenEnemies.Count)
        {
            arrows.Add(Instantiate(arrowPrefab));
        }
        while (arrows.Count > offScreenEnemies.Count)
        {
            Destroy(arrows[0]);
            arrows.RemoveAt(0);
        }

        for (int i = 0; i < offScreenEnemies.Count; i++)
        {
            Vector3 direction = offScreenEnemies[i].position - player.position;
            direction.y = 0; // Keep the direction in 2D plane
            arrows[i].transform.position = player.position + direction.normalized * 10f + Vector3.up * 2.5f;
            arrows[i].transform.rotation = Quaternion.FromToRotation(Vector3.forward, direction);
        }
    }
}
