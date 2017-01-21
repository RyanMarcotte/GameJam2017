using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    private Vector3 offset;

    void Start()
    {
        //Initialize camera offset
        offset = transform.position - player.transform.position;
    }

    //Use LateUpdate since it is guaranteed to run AFTER Update
    void LateUpdate()
    {
        //Move the camera
        transform.position = player.transform.position + offset;
    }
}