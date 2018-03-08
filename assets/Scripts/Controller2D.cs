using UnityEngine;
using System.Collections;


public class Controller2D : RaycastController {
    
    public float maxClimbAngle = 80;
    public float maxDescendAngle = 80;

    public CollisionInfo collisions;
    public EdgeDetection edgeDet;

    Vector2 playerInput; 

	// Use this for initialization
	public override void Start () {
        base.Start();
        collisions.faceDir = 1;
    }

    public void Move(Vector3 velocity, bool standingOnPlatform)
    {
        Move(velocity, Vector2.zero, standingOnPlatform);
    }
	
    public void Move(Vector3 velocity, Vector2 input, bool standingOnPlatform = false)
    {
        //handle collisions that affect the final velocity
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.velocityOld = velocity;
        playerInput = input;

        if (velocity.x != 0)
        {
            collisions.faceDir = (int)Mathf.Sign(velocity.x);
        }

        if (velocity.y < 0)
        {
            DescendSlope(ref velocity);
        }

        HorizontalCollisions(ref velocity);

        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }

        transform.Translate(velocity); //move object according to velocity
    }

    void HorizontalCollisions(ref Vector3 velocity)
    {
        float directionX = collisions.faceDir; //right = 1, left = -1
        float rayLength = Mathf.Abs(velocity.x) + skinWidth; //absolute value of ray = length

        if (Mathf.Abs(velocity.x) < skinWidth)
        {
            rayLength = 2 * skinWidth;
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight; //if direction is left start bottomleft else start bottomright
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                if (i == 0)
                    edgeDet.hit1 = true;
                if (i == 1)
                    edgeDet.hit2 = true;
                if (i == 2)
                    edgeDet.hit3 = true;

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                if (i == 0 && slopeAngle <= maxClimbAngle) //climbing slope
                {
                    if (collisions.descendingSlope) //if starting to climb a slope while descending
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngleOld) //close distance of collisionbox to slope
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    ClimbSlope(ref velocity, slopeAngle);
                    velocity.x += distanceToSlopeStart * directionX;
                }

                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }

                    collisions.left = directionX == -1; //if direction = left, collisions.left = true
                    collisions.right = directionX == 1; //if direction = right, collisions.right = true
                }
            }
            else if (i == 3 && edgeDet.hit1 && edgeDet.hit2 && edgeDet.hit3) //trying to detect edge
            {

                edgeDet.edge = true;

            }
            else
            {
                Debug.Log("test");
                //edgeDet.edge = false;
            }
        }
        edgeDet.Reset();
    }

    void VerticalCollisions(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y); //up = 1, down = -1
        float rayLength = Mathf.Abs(velocity.y) + skinWidth; //absolute value of ray = length

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft; //if direction is down start bottomleft else start topleft
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                if (hit.collider.tag == "Through")
                {
                    if (directionY == 1 || hit.distance == 0)
                    {
                        continue;
                    }
                    if (playerInput.y == -1)
                    {
                        continue;
                    }
                }
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;

                if(collisions.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                collisions.below = directionY == -1; //if direction = down, collisions.below = true
                collisions.above = directionY == 1; //if direction = up, collisions.above = true
            }
        }

        //adjusting for curving slope
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y; //if statement
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }

    void ClimbSlope(ref Vector3 velocity, float slopeAngle)
    {
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance; //setting the slope move velocity to the same as ground velocity
        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }

    void DescendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if ((hit.distance - skinWidth) <= (Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }

    }

    

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool edgeHang;
        public float slopeAngle, slopeAngleOld;
        public Vector3 velocityOld;
        public int faceDir;
        

        public void Reset()
        {
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendingSlope = false;
            edgeHang = false;

            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }

    public struct EdgeDetection
    {
        public bool hit1, hit2, hit3;
        public bool edge;

        public void Reset()
        {
            hit1 = hit2 = hit3 = false;
        }
    }
}
