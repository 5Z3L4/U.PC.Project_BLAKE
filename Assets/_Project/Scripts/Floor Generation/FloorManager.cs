using System;
using Cinemachine;
using UnityEngine;

[RequireComponent(typeof(FloorGenerator))]
public class FloorManager : MonoBehaviour
{
    public event Action<Transform,Transform> FloorGeneratorEnd;
    
    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private GameObject virtualCameraPrefab;

    [SerializeField] 
    private GameObject cameraFollowPrefab;

    private FloorGenerator floorGenerator;
    private GameObject player;
    private CinemachineVirtualCamera virtualCamera;
    private GameObject cameraFollow;
    

    private void Awake()
    {
        floorGenerator = GetComponent<FloorGenerator>();
    }

    private void Start()
    {
        if (ReferenceManager.PlayerInputController != null)
        {
            ReferenceManager.PlayerInputController.gameObject.SetActive(false);
        }
        StartCoroutine(floorGenerator.GenerateFloor());
    }

    public void OnFloorGeneratorEnd(Vector3 startingRoomTransform)
    {
        if (ReferenceManager.PlayerInputController == null)
        {
            player = Instantiate(playerPrefab, startingRoomTransform, Quaternion.identity);
        } else
        {
            player = ReferenceManager.PlayerInputController.gameObject;
            player.transform.position = startingRoomTransform;
            player.transform.rotation = Quaternion.identity;
            ReferenceManager.PlayerInputController.gameObject.SetActive(true);
        }
        virtualCamera = Instantiate(virtualCameraPrefab).GetComponent<CinemachineVirtualCamera>();
        cameraFollow = Instantiate(cameraFollowPrefab);
        
        virtualCamera.Follow = cameraFollow.transform;
        cameraFollow.GetComponent<CameraFollowScript>().SetPlayerReference(player.transform);
        
        FloorGeneratorEnd?.Invoke(player.transform, cameraFollow.transform);
    }
}
