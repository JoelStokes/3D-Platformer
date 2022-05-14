using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject FollowObj;
    public Vector2 offset;
    public float xMult = 1;
    public float yMult = 1;

    public bool useEdges = false;   //Set edges that the camera cannot scroll past. Level start / end areas
    public Vector2 topLeftEdge;
    public Vector2 bottomRightEdge;

    private GameObject LoadHandler;

    void Awake(){
        LoadHandler = GameObject.Find("Load Handler");   //See if load overlay being used, then center to camera position
    }
    
    void Update()
    {
        //SET EDGES HERE ONCE DOING ROOM-STYLED OR TUTORIAL LEVELS
        transform.position = new Vector3((FollowObj.transform.position.x * xMult) + offset.x, (FollowObj.transform.position.y * yMult) + offset.y, transform.position.z);

        if (LoadHandler != null){
            LoadHandler.transform.position = new Vector3(transform.position.x, transform.position.y, LoadHandler.transform.position.z);
        }
    }
}
