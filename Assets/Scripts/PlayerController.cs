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
    private LayerMask wallJumpLayerMask;

    //Walk
    private float walkSpeed;    //Set in start, MaxSpeed / 2
    private bool walking = false;

    //Fall
    private float maxVelocity = 35; //Prevent player from moving too quickly

    //Components
    private Rigidbody2D rigi;
    private BoxCollider2D boxCollider;
    private ParticleSystem particles;
    private float particlePosition = .35f;

    void Start()
    {
        rigi = GetComponent<Rigidbody2D>();
        groundLayerMask = LayerMask.GetMask("Ground") | LayerMask.GetMask("One-Way");
        wallJumpLayerMask = LayerMask.GetMask("Ground");
        boxCollider = GetComponent<BoxCollider2D>();
        particles = GetComponent<ParticleSystem>();

        walkSpeed = maxSpeed / 2;
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
            jumpPressedTimer = 0;
        } else if (jumpEnding){
            if (rigi.velocity.y > 0){   //Don't apply jump reduction if apex of jump already hit
                ApplyJump(rigi.velocity.y * jumpReleaseMult);
            }
            
            jumpEnding = false;
        }

        if (onLeftWall || onRightWall){
            ApplyWallSlow();
            if (rigi.velocity.y < 0){   //Only emit particles if sliding downwards
                HandleParticles();
            }
        } else {
            if (particles.isEmitting){
                particles.Stop();
            }
        }

        rigi.velocity = Vector2.ClampMagnitude(rigi.velocity, maxVelocity);
    }

    private void HandleParticles(){
        if (!particles.isEmitting){
            ParticleSystem.ShapeModule shape = particles.shape;

            if (onLeftWall){
                shape.position = new Vector3(-particlePosition, 0, 0);
            } else {
                shape.position = new Vector3(particlePosition, 0, 0);
            }

            particles.Play();
        }
    }
    private bool CheckGrounded(){
        if (rigi.velocity.y <= 0.1f && rigi.velocity.y > -.1f){  //Prevent rising grounded state through semi-solid platforms
            RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.down, groundedHeight, groundLayerMask);

            return raycastHit.collider != null;
        } else {
            return false;
        }
    }

    private void CheckWalls(){
        RaycastHit2D leftRaycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.left, groundedHeight, wallJumpLayerMask);

        if (leftRaycastHit.collider != null){
            onLeftWall = true;
        } else {
            onLeftWall = false;
        }

        RaycastHit2D rightRaycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.right, groundedHeight, wallJumpLayerMask);

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

        float maxCheck;
        if (walking){
            maxCheck = walkSpeed;
        } else {
            maxCheck = maxSpeed;
        }

        if (newSpeed > maxCheck){
            newSpeed = maxCheck;
        } else if (newSpeed < -maxCheck){
            newSpeed = -maxCheck;
        }

        rigi.velocity = new Vector2(newSpeed, rigi.velocity.y);
    }

    private void SlowMovement(){    //Dampen current horizontal movement
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

    public void Walk(InputAction.CallbackContext context){
        if (context.phase == InputActionPhase.Started){
            walking = true;
        } else if (context.phase == InputActionPhase.Canceled){
            walking = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other){
        if (other.gameObject.tag == "Death"){
            Debug.Log("Die");
        }
    }
}
