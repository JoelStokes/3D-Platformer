using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //Jump
    private float jumpStartForce = 27.5f;
    private float jumpReleaseMult = .5f;
    private bool jumpStarting = false;
    private bool jumpEnding = false;
    private float jumpPressedTimer = 0;
    private float jumpPressedCount = .2f;

    //Move
    private float accelerationSpeed = .6f;
    private float maxSpeed = 14.7f;
    private float currentMove = 0;
    private float deadZone = .6f;
    private float dampingGround = .8f;
    private float dampingAir = .9f;
    private float dampingTurn = .7f;
    private float dampingTurnAir = .76f;
    private float maxSpeedDampen = 1.25f;
    private float maxSpeedDampenTurn = 1.3f;

    //Ground Checks
    private bool isGrounded = false;
    private LayerMask groundLayerMask;
    private float groundedHeight = .1f;
    private float groundedTimer = 0;
    private float groundedCount =.1f;
    private Vector2 groundPos;

    //Walljump
    private bool onLeftWall = false;
    private bool onRightWall = false;
    private float wallSlowMax = 5;   //Only applied moving downwards
    private float wallJumpOutwardsForce = 14;
    private float wallJumpUpwardsForce = 25;
    private float wallJumpTimer = 0;
    private float wallJumpCount = .1f;
    private LayerMask wallJumpLayerMask;
    private float wallSlideLeftTimer = 0;
    private float wallSlideRightTimer = 0;
    private float wallSlideCount = .15f;

    //Tube
    private bool inTube = false;
    private float tubeSpeed = 25f;
    private float tubeExitForce = 25f;
    private string tubeDirection;
    private LayerMask tubeLayerMask;

    //Walk
    private float walkSpeed;    //Set in start, MaxSpeed / 2
    private bool walking = false;

    //Fall
    private float maxVelocity = 35; //Prevent player from moving too quickly

    //Spring
    private bool springJump = false;    //used to prevent jump release for quicker descent (fixed spring jump height)
    private float springTimer = 0;
    private float springCount = .1f;
    private float sideSpringCount = .25f;

    //Stun Mode (Hurt Knockback)
    private bool hurt = false;
    private float hurtCurve = 0;
    private Vector2 hurtPos;  //3 locations on hurt Bezier Curve
    private float hurtHeight = 10f;
    private bool hurtRight = false;
    private float stunTimer = 0;
    private float stunCount = .25f;
    private float hurtForceUp = 8;
    private float hurtForceBack = 2.5f;

    //Components
    private Rigidbody2D rigi;
    private BoxCollider2D boxCollider;
    private ParticleSystem particles;
    private float particlePosition = .55f;
    private TrailRenderer trail;

    void Start()
    {
        rigi = GetComponent<Rigidbody2D>();
        groundLayerMask = LayerMask.GetMask("Ground") | LayerMask.GetMask("One-Way");
        wallJumpLayerMask = LayerMask.GetMask("Ground");
        tubeLayerMask = LayerMask.GetMask("Tube Trigger");
        boxCollider = GetComponent<BoxCollider2D>();
        particles = GetComponent<ParticleSystem>();
        trail = GetComponent<TrailRenderer>();

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

        if (wallSlideLeftTimer > 0){
            wallSlideLeftTimer -= Time.deltaTime;
        }

        if (wallSlideRightTimer > 0){
            wallSlideRightTimer -= Time.deltaTime;
        }

        if (springTimer > 0){
            springTimer -= Time.deltaTime;
        }

        if (stunTimer > 0){
            stunTimer -= Time.deltaTime;
        }

        if (hurt){
            if (hurtCurve < 1){
                ApplyHurtCurve();
            } else {
                hurt = false;
                trail.emitting = true;

                if (hurtRight){
                    rigi.velocity = new Vector2(hurtForceBack, hurtForceUp);
                } else {
                    rigi.velocity = new Vector2(-hurtForceBack, hurtForceUp);
                }

                stunTimer = stunCount;
            }
        }

        if (inTube){    //Apply Tube Transform movement
            switch (tubeDirection){
                case "up":
                    transform.position = new Vector3(transform.position.x, transform.position.y + (tubeSpeed * Time.deltaTime));
                    break;
                case "down":
                    transform.position = new Vector3(transform.position.x, transform.position.y - (tubeSpeed * Time.deltaTime));
                    break;
                case "left":
                    transform.position = new Vector3(transform.position.x - (tubeSpeed * Time.deltaTime), transform.position.y);
                    break;
                default:
                    transform.position = new Vector3(transform.position.x + (tubeSpeed * Time.deltaTime), transform.position.y);
                    break;
            }
        }
    }

    void FixedUpdate()
    {
        if (!hurt && stunTimer <= 0 && !inTube){
            if (CheckGrounded()){   //Set Grounded
                groundedTimer = groundedCount;
                isGrounded = true;
                springJump = false;
                groundPos = new Vector2(transform.position.x, transform.position.y);
            }

            if (!isGrounded){   //Set Walls
                CheckWalls();
            } else {
                onLeftWall = false;
                onRightWall = false;
            }

            if (wallJumpTimer <= 0 && springTimer <= 0){     //Move
                if (currentMove > deadZone){
                    ApplyMove(1);
                } else if (currentMove < -deadZone){
                    ApplyMove(-1);
                } else {
                    SlowMovement();
                }
            }

            if (jumpStarting && isGrounded && springTimer <= 0){    //Jump
                ApplyJump(jumpStartForce);
                jumpStarting = false;
                jumpPressedTimer = 0;
                groundedTimer = 0;
            } else if (jumpStarting && ((onLeftWall || onRightWall) || (wallSlideLeftTimer > 0 || wallSlideRightTimer > 0)) && wallJumpTimer <= 0 && springTimer <= 0){
                ApplyWallJump();
                jumpPressedTimer = 0;
            } else if (jumpEnding && !springJump && !inTube){
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

            if (springTimer <= 0){
                rigi.velocity = Vector2.ClampMagnitude(rigi.velocity, maxVelocity); //Prevent falling too fast, avoid clipping through walls
            }
        } else if (inTube){ //Use raycast to check for bends / exit in tube
            Vector2 raycastDirection;

            switch (tubeDirection){
                case "up":
                    raycastDirection = Vector2.up;
                    break;
                case "down":
                    raycastDirection = Vector2.down;
                    break;
                case "left":
                    raycastDirection = Vector2.left;
                    break;
                default:
                    raycastDirection = Vector2.right;
                    break;
            }
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y) - (raycastDirection * .4f), raycastDirection, .4f, tubeLayerMask);

            if (hit.collider != null && hit.transform.gameObject.tag == "Tube")
            {                
                Tube tubeScript = hit.transform.gameObject.GetComponent<Tube>();
                
                if (tubeDirection != tubeScript.direction.ToString()){
                    transform.position = new Vector3(hit.transform.position.x, hit.transform.position.y, transform.position.z); //Recenter onto tube turn if different hit
                }

                if (inTube && tubeScript.isExit){
                    ExitTube();
                } else if (inTube){
                    tubeDirection = tubeScript.direction.ToString();
                }
            }
        }
    }

    private void HandleParticles(){
        if (!particles.isEmitting){
            ParticleSystem.ShapeModule shape = particles.shape;

            if (onLeftWall){
                shape.position = new Vector3(-particlePosition, 0, -1);
            } else {
                shape.position = new Vector3(particlePosition, 0, -1);
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

        if (leftRaycastHit.collider != null && currentMove < -deadZone){
            onLeftWall = true;
        } else {
            if (onLeftWall){    //Was previously in wall slide, give grace period
                wallSlideLeftTimer = wallSlideCount;
            }
            onLeftWall = false;
        }

        RaycastHit2D rightRaycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0f, Vector2.right, groundedHeight, wallJumpLayerMask);

        if (rightRaycastHit.collider != null && currentMove > deadZone){
            onRightWall = true;
        } else {
            if (onRightWall){
                wallSlideRightTimer = wallSlideCount;
            }
            onRightWall = false;
        }
    }

    private void ApplyMove(float value){
        float dampenedVelocity = rigi.velocity.x;
        if (isGrounded && ((rigi.velocity.x > 0 && value < 0) || (rigi.velocity.x < 0 && value > 0))){  //Used to only apply to isGrounded turning, but feels better jumping like this
            dampenedVelocity *= dampingTurn;
        } else if ((rigi.velocity.x > 0 && value < 0) || (rigi.velocity.x < 0 && value > 0)){
            dampenedVelocity *= dampingTurnAir;
        }

        float maxCheck;
        if (walking){
            maxCheck = walkSpeed;
        } else {
            maxCheck = maxSpeed;
        }

        float newSpeed = ((value * accelerationSpeed) + dampenedVelocity);

        if (newSpeed > maxCheck){  //Fast move right & hold right, retain some momentum. If fast right & trying to slow, immediately drop to maxSpeed
            if (value > 0){
                 newSpeed = newSpeed - maxSpeedDampen;    //Slow degrade to max speed when matching direction
            } else {
                newSpeed = newSpeed - maxSpeedDampenTurn;   //Sharper degrade due to holding against the speed
            }
        } else if (newSpeed < -maxCheck){
            if (value < 0){
                newSpeed = newSpeed + maxSpeedDampen;
            } else {
                newSpeed = newSpeed + maxSpeedDampenTurn;
            }
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
        if (!inTube){
            rigi.velocity = new Vector2(rigi.velocity.x, value);
        }
    }

    private void ApplyWallJump(){
        if (onLeftWall || wallSlideLeftTimer > 0){
            rigi.velocity = new Vector2(wallJumpOutwardsForce, wallJumpUpwardsForce);
        } else if (onRightWall || wallSlideRightTimer > 0){
            rigi.velocity = new Vector2(-wallJumpOutwardsForce, wallJumpUpwardsForce);
        }

        onLeftWall = false;
        onRightWall = false;
        wallJumpTimer = wallJumpCount;
        wallSlideLeftTimer = 0;
        wallSlideRightTimer = 0;
        springJump = false;
    }

    private void ApplyWallSlow(){
        if (rigi.velocity.y < -wallSlowMax){
            rigi.velocity = new Vector2(rigi.velocity.x, -wallSlowMax);
        }
    }

    public void Jump(InputAction.CallbackContext context){
        if (context.phase == InputActionPhase.Started){
            if (!inTube){
                jumpStarting = true;
                jumpPressedTimer = jumpPressedCount;
            }
        } else if (context.phase == InputActionPhase.Canceled){
            if (!inTube){
                jumpEnding = true;
            }
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

    private void ExitTube(){
        rigi.isKinematic = false;
        inTube = false;

        switch (tubeDirection){
            case "up":
                rigi.velocity = new Vector2(0, tubeExitForce);
                break;
            case "down":
                rigi.velocity = new Vector2(0, -tubeExitForce);
                break;
            case "right":
                rigi.velocity = new Vector2(tubeExitForce, 0);
                break;
            default:
                rigi.velocity = new Vector2(-tubeExitForce, 0);
                break;
        }
    }

    private void ApplyHurtCurve(){
        hurtCurve += 1.0f *Time.deltaTime;

        Vector2 midPos = new Vector2((groundPos.x + hurtPos.x)/2, 0);
        if (groundPos.y > hurtPos.y){
            midPos.y = groundPos.y + hurtHeight;
        } else {
            midPos.y = hurtPos.y + hurtHeight;
        }

        Vector2 m1 = Vector2.Lerp(hurtPos, midPos, hurtCurve);
        Vector2 m2 = Vector2.Lerp(midPos, groundPos, hurtCurve);
        Vector2 updatePos = Vector2.Lerp(m1, m2, hurtCurve);

        transform.position = new Vector3(updatePos.x, updatePos.y, transform.position.z);
    }

    //TEMPORARY CLOSE FOR ALPHA PHYSICS TEST!!! Change to Pause Menu later
    public void Pause(InputAction.CallbackContext context){
        if (context.phase == InputActionPhase.Started){
            Application.Quit();
        }
    }

    void OnTriggerEnter2D(Collider2D other){
        if (!hurt && stunTimer <= 0 && !inTube){
            if (other.gameObject.tag == "Death"){
                SceneManager.LoadScene("AlphaStart");
            } else if (other.gameObject.tag == "Switch"){
                other.gameObject.GetComponent<SwitchButton>().ActivateSwitch();
            } else if (other.gameObject.tag == "Spring"){
                if (other.gameObject.GetComponent<Spring>().diagonal){
                    springTimer = sideSpringCount;
                } else {
                    springTimer = springCount;
                }
                springJump = true;

                other.gameObject.GetComponent<Spring>().ApplyForce(rigi);
            } else if (other.gameObject.tag == "Hurt"){
                hurt = true;
                hurtCurve = 0;
                trail.emitting = false;

                onLeftWall = false;
                onRightWall = false;
                if (particles.isEmitting){
                    particles.Stop();
                }

                hurtPos = new Vector2(transform.position.x, transform.position.y);

                //if (hurtPos.x < groundPos.x + .5f && hurtPos.x > groundPos.x - .5f)
                //Add extra buffer if hurt & grounded spots are very close?

                if (groundPos.x > hurtPos.x){
                    hurtRight = true;
                } else {
                    hurtRight = false;
                }
            } else if (other.gameObject.tag == "Tube"){
                Tube tubeScript = other.gameObject.GetComponent<Tube>();

                if (!inTube && tubeScript.isEntrance){
                    inTube = true;
                    onLeftWall = false;
                    onRightWall = false;
                    isGrounded = false;
                    rigi.isKinematic = true;
                    tubeDirection = tubeScript.direction.ToString();
                    rigi.velocity = Vector2.zero;
                    transform.position = new Vector3(other.gameObject.transform.position.x, other.gameObject.transform.position.y, transform.position.z);
                }
            }
        }
    }
}
