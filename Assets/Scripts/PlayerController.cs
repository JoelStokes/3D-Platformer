using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Jump
    private float jumpStartForce = 31;
    private float jumpReleaseMult = .5f;
    private bool jumpStarting = false;
    private bool jumpEnding = false;
    private float jumpPressedTimer = 0;
    private float jumpPressedCount = .2f;

    //Move
    private float accelerationSpeed = .6f;
    private float maxSpeed = 15;
    private float currentMove = 0;
    private float deadZone = .6f;
    private float dampingGround = .8f;
    private float dampingAir = .9f;
    private float dampingTurn = .7f;

    //Ground Checks
    private bool isGrounded = false;
    private LayerMask groundLayerMask;
    private float groundedHeight = .1f;
    private float groundedTimer = 0;
    private float groundedCount =.1f;

    //Walljump
    private bool onLeftWall = false;
    private bool onRightWall = false;
    private float wallSlowMax = 5;   //Only applied moving downwards
    private float wallJumpOutwardsForce = 15;
    private float wallJumpUpwardsForce = 27;
    private float wallJumpTimer = 0;
    private float wallJumpCount = .1f;

    //Fall
    private float maxVelocity = 35; //Prevent player from moving too quickly

    //Components
    private Rigidbody2D rigi;
    private BoxCollider2D boxCollider;

    void Start()
    {
        rigi = GetComponent<Rigidbody2D>();
        groundLayerMask = LayerMask.GetMask("Ground");
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update(){  //Handle Timers
        if (jumpPressedTimer > 0){     //Jump & Grounded buffer windows to allow jump
            jumpPressedTimer -= Time.deltaTime;
        } else {
            jumpStarting = false;
        }

        if (groundedTimer > 0){
            groundedTimer -= Time.deltaTime;
        } else{
            isGrounded = false;
        }

        if (wallJumpTimer > 0){     //Walljump prevents turning around to reach same wall & mashing to double wall jump
            wallJumpTimer -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        if (CheckGrounded()){   //Set Grounded
            groundedTimer = groundedCount;
            isGrounded = true;
        }

        if (!isGrounded){   //Set Walls
            CheckWalls();
        } else {
            onLeftWall = false;
            onRightWall = false;
        }

        if (wallJumpTimer <= 0){     //Move
            if (currentMove > deadZone){
                ApplyMove(1);
            } else if (currentMove < -deadZone){
                ApplyMove(-1);
            } else {
                SlowMovement();
            }
        }

        if (jumpStarting && isGrounded){    //Jump
            ApplyJump(jumpStartForce);
            jumpStarting = false;
            jumpPressedTimer = 0;
            groundedTimer = 0;
        } else if (jumpStarting && (onLeftWall || onRightWall) && wallJumpTimer <= 0){
            ApplyWallJump();
        } else if (jumpEnding){
            if (rigi.velocity.y > 0){   //Don't apply jump reduction if apex of jump already hit
                ApplyJump(rigi.velocity.y * jumpReleaseMult);
            }
            
            jumpEnding = false;
        }

        if (onLeftWall || onRightWall){
            ApplyWallSlow();
        }

        rigi.velocity = Vector2.ClampMagnitude(rigi.velocity, maxVelocity);
    }

    private bool CheckGrounded(){
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, groundedHeight, groundLayerMask);

        return raycastHit.collider != null;
    }

    private void CheckWalls(){
        RaycastHit2D leftRaycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.left, groundedHeight, groundLayerMask);

        if (leftRaycastHit.collider != null){
            onLeftWall = true;
        } else {
            onLeftWall = false;
        }

        RaycastHit2D rightRaycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.right, groundedHeight, groundLayerMask);

        if (rightRaycastHit.collider != null){
            onRightWall = true;
        } else {
            onRightWall = false;
        }
    }

    private void ApplyMove(float value){
        float dampenedVelocity = rigi.velocity.x;
        if (isGrounded && (rigi.velocity.x > 0 && value < 0) || (rigi.velocity.x < 0 && value > 0)){  //Check if turning
            dampenedVelocity *= dampingTurn;
        }

        float newSpeed = ((value * accelerationSpeed) + dampenedVelocity);

        if (newSpeed > maxSpeed){
            newSpeed = maxSpeed;
        } else if (newSpeed < -maxSpeed){
            newSpeed = -maxSpeed;
        }

        rigi.velocity = new Vector2(newSpeed, rigi.velocity.y);
    }

    private void SlowMovement(){
        float newSpeed = rigi.velocity.x;

        if (isGrounded){
            newSpeed *= dampingGround;
        } else {
            newSpeed *= dampingAir;
        }
        rigi.velocity = new Vector2(newSpeed, rigi.velocity.y);
    }

    private void ApplyJump(float value){
        rigi.velocity = new Vector2(rigi.velocity.x, value);
    }

    private void ApplyWallJump(){
        if (onLeftWall){
            rigi.velocity = new Vector2(wallJumpOutwardsForce, wallJumpUpwardsForce);
        } else if (onRightWall){
            rigi.velocity = new Vector2(-wallJumpOutwardsForce, wallJumpUpwardsForce);
        }

        onLeftWall = false;
        onRightWall = false;
        wallJumpTimer = wallJumpCount;
    }

    private void ApplyWallSlow(){
        if (rigi.velocity.y < -wallSlowMax){
            rigi.velocity = new Vector2(rigi.velocity.x, -wallSlowMax);
        }
    }

    public void Jump(InputAction.CallbackContext context){
        if (context.phase == InputActionPhase.Started){
            jumpStarting = true;
            jumpPressedTimer = jumpPressedCount;
        } else if (context.phase == InputActionPhase.Canceled){
            jumpEnding = true;
        }
    }

    public void Move(InputAction.CallbackContext context){
        currentMove = context.ReadValue<float>();
    }
}
