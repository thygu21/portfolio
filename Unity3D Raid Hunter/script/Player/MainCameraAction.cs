using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCameraAction : MonoBehaviour
{
    public GameObject player;
    float offsetX = 3f;
    public float offsetY;
    float offsetZ = -3f;
    public float followSpeed = 5f;
    Vector3 cameraPosition;

    private void Start()
    {
        //선택한 플레이어 정보 대입
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            cameraPosition.x = player.transform.position.x + offsetX;
            cameraPosition.y = player.transform.position.y + offsetY;
            cameraPosition.z = player.transform.position.z + offsetZ;
            transform.position = cameraPosition;
        }
    }
}
