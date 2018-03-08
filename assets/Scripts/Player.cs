using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(PlayerAnimationController))]

public class Player : MonoBehaviour {
    Controller2D controller;
    PlayerAnimationController animationController;

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    public float accelerationTimeAirBorne = .2f;
    public float accelerationTimeGrounded = .1f;
    public float moveSpeed = 12;
    public float backJumpDistance = 4f;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    float timetoWallUnstick;

    Vector2 input;
    bool wallSliding;
    int wallDirX = 0;

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector3 velocity;
    float velocityXsmoothing;

    // Use this for initialization
    void Start () {
        controller = GetComponent<Controller2D>();
        animationController = GetComponent<PlayerAnimationController>();
        animationController.stateInfo.Reset();
        animationController.stateInfo.faceRight = true;
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
        print("Gravity: " + gravity + " jump velocity: " + maxJumpVelocity);
    }

    // Update is called once per frame
    void Update () {
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        wallDirX = (controller.collisions.left) ? -1 : 1;

        if (!controller.edgeDet.edge)
        {
            float targetVelocityX = input.x * moveSpeed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXsmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirBorne);
        }
        

        HandleWallSliding();
        
        if (controller.collisions.above || controller.collisions.below)
        {
            velocity.y = 0;
        }

        HandleInput();

        if (!controller.edgeDet.edge)
            velocity.y += gravity * Time.deltaTime;


        animationController.stateInfo.velocity = velocity;
        animationController.stateInfo.input = input;
        if (controller.collisions.below)
        {
            animationController.stateInfo.grounded = true;
        }

        if (animationController.stateInfo.grounded || animationController.inBackJump)
            animationController.Flip(input.x);
        else
            animationController.Flip(velocity.x);
        //Debug.Log("velocity.x after damp: " + velocity.x);
        
       
        animationController.ContinueState();

        controller.Move(velocity * Time.deltaTime, input);

        animationController.stateInfo.Reset();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            animationController.stateInfo.crouch = true;
        }

        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            animationController.stateInfo.crouch = false;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            animationController.stateInfo.attack = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            animationController.stateInfo.jump = true;
            if (wallSliding)
            {
                if (wallDirX == (int)input.x)
                {
                    Debug.Log("test jump climb");
                    velocity.x = -wallDirX * wallJumpClimb.x;
                    velocity.y = wallJumpClimb.y;
                }
                else if (input.x == 0)
                {
                    Debug.Log("test jump off");

                    velocity.x = -wallDirX * wallJumpOff.x;
                    velocity.y = wallJumpOff.y;
                }
                else
                {
                    Debug.Log("test jump leap");
                    velocity.x = -wallDirX * wallLeap.x;
                    velocity.y = wallLeap.y;
                }
            }
            if (controller.collisions.below)
            {
                velocity.y = maxJumpVelocity;
            }
        }
        if(Input.GetKeyUp(KeyCode.Space))
        {

            if (velocity.y > minJumpVelocity)
            {
                velocity.y = minJumpVelocity;
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            animationController.stateInfo.backJump = true;
            animationController.inBackJump = true;
            if (animationController.stateInfo.faceRight)
            {
                velocity.x = backJumpDistance;
            }
            else
            {
                velocity.x = -backJumpDistance;
            }
        }
    }

    void HandleWallSliding()
    {
        wallSliding = false;

        if (controller.edgeDet.edge)//edge detection
        {
            velocity.y = 0;
        }

        else if ((controller.collisions.left || controller.collisions.right) && (!controller.collisions.below && velocity.y < 0))
        {
            wallSliding = true;
            animationController.stateInfo.wallSliding = true;

            if (velocity.y < -wallSlideSpeedMax)
            {
                velocity.y = -wallSlideSpeedMax;
            }
            if (timetoWallUnstick > 0)
            {
                velocityXsmoothing = 0;
                if (input.x != wallDirX && input.x != 0)
                {
                    timetoWallUnstick -= Time.deltaTime;
                }
                else
                {
                    timetoWallUnstick = wallStickTime;
                }
            }
            else
            {
                timetoWallUnstick = wallStickTime;
            }
        }
    }
}
