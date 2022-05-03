using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject FollowObj;
    public Vector2 offset;
    public bool lockX = false;
    public bool lockY = false;

    // Update is called once per frame
    void Update()
    {
        if (lockX){
            transform.position = new Vector3(transform.position.x, FollowObj.transform.position.y + offset.y, transform.position.z);
        } else if (lockY){
            transform.position = new Vector3(FollowObj.transform.position.x + offset.x, transform.position.y, transform.position.z);
        } else {
            transform.position = new Vector3(FollowObj.transform.position.x + offset.x, FollowObj.transform.position.y + offset.y, transform.position.z);
        }
    }
}
