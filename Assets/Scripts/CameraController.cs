using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject FollowObj;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(FollowObj.transform.position.x, FollowObj.transform.position.y, transform.position.z);
    }
}
