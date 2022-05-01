using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Jump
    private float jumpStartForce = 100;
    private float jumpHoldForce = 4f;
    private bool jumpStarting = false;
    private bool jumping = false;

    //Move
    private float moveSpeed = 100;
    private float currentMove = 0;
    private float deadZone = .6f;

    //Fall
    private float maxVelocity = 35; //Prevent player from moving too quickly

    //Components
    private Rigidbody2D rigi;

    void Start()
    {
        rigi = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (currentMove > deadZone){
            ApplyMove(1);
        } else if (currentMove < -deadZone){
            ApplyMove(-1);
        }

        if (jumpStarting){
            ApplyJump(jumpStartForce);
            jumpStarting = false;
        } else if (jumping){
            ApplyJump(jumpHoldForce);
        }

        rigi.velocity = Vector2.ClampMagnitude(rigi.velocity, maxVelocity);
    }

    private void ApplyMove(float value){
        Debug.Log(value);
        rigi.AddForce(new Vector2(value, 0) * moveSpeed, ForceMode2D.Force);
    }

    private void ApplyJump(float value){
        rigi.AddForce(Vector2.up * value, ForceMode2D.Impulse);
    }

    public void Jump(InputAction.CallbackContext context){
        if (context.phase == InputActionPhase.Started){
            jumpStarting = true;
            jumping = true;
        } else if (context.phase == InputActionPhase.Canceled){
            jumping = false;
        }
    }

    public void Move(InputAction.CallbackContext context){
        currentMove = context.ReadValue<float>();
    }
}
