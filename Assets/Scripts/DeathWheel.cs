using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathWheel : MonoBehaviour
{
    public float acceleration;
    public float timeInterval;
    public float timeIntervalAdd;
    private float speed = 0;
    private float maxSpeed = 12;
    private float speedTimer = 0;

    // Update is called once per frame
    void Update()
    {
        speedTimer += Time.deltaTime;

        if (speedTimer >= timeInterval && speed < maxSpeed){
            speed += acceleration;
            
            speedTimer = 0;
            speed += acceleration;
            timeInterval += timeIntervalAdd;
        }

        transform.position = new Vector3(transform.position.x, transform.position.y + (speed * Time.deltaTime), transform.position.z);
    }
}
