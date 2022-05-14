using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spring : MonoBehaviour
{
    public bool diagonal = false;
    private bool inverted = false;   //For diagonal, if x scale flipped
    private int direction = 0;   //0 = North, 1 East, 2 South, 3 West

    private Animator anim;

    private float springUpForce = 40;
    private float springOutForce = 29;

    void Start()
    {
        if (transform.localScale.x < 0){
            inverted = true;
        }

        Debug.Log("z: " + transform.eulerAngles.z + ", x scale: " + transform.localScale.x);
        if (transform.eulerAngles.z == 90){
            direction = 1;
        } else if (transform.eulerAngles.z == 180){
            direction = 2;
        } else if (transform.eulerAngles.z == 270){
            direction = 3;
        }

        anim = GetComponent<Animator>();
    }

    public void ApplyForce(Rigidbody2D rigi){
        anim.SetTrigger("Activate");

        float xForce = 0;
        float yForce = 0;
        
        if (diagonal){
            yForce = springUpForce - 5;
            if (inverted){
                xForce = springOutForce;
            } else {
                xForce = -springOutForce;
            }
        } else {
            yForce = springUpForce;
        }

        rigi.velocity = new Vector2(xForce, yForce);
    }
}
