using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathWheel : MonoBehaviour
{
    public float acceleration = .0001f;
    public float timeInterval = .5f;
    public float timeIntervalAdd = .5f;
    private float speed = 0;
    private float maxSpeed = 10;
    private float speedTimer = 0;

    private float moveTimer = 0;
    private float moveCounter = .017f;  //60fps move

    // Update is called once per frame
    void Update()
    {
        speedTimer += Time.deltaTime;
        moveTimer += Time.deltaTime;

        if (speedTimer > timeInterval){
            speed += acceleration;
            
            speedTimer = 0;
            speed += acceleration;
            timeInterval += timeIntervalAdd;
        }

        if (moveTimer > moveCounter){
            transform.position = new Vector3(transform.position.x, transform.position.y + speed, transform.position.z);
            moveTimer = 0;
        }
    }
}
