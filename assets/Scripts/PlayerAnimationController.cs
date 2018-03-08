using UnityEngine;
using System.Collections;

//extend player, get player state info
public class PlayerAnimationController : MonoBehaviour {

    public StateInfo stateInfo;

    const string kIdleAnim = "Idle";
    const string kRunAnim = "Run";
    const string kJumpAnim = "Jump";
    const string kFallAnim = "Fall";
    const string kLandAnim = "Land";
    const string kJumpTransAnim = "Jump_Transition";
    const string kCrouchAnim = "Crouch";
    const string kCrouchIdleAnim = "Crouch Idle";
    const string kCrawlAnim = "Crawl";
    const string kAttack1Anim = "Attack1";
    const string kAttack1FinAnim = "Attack1 Finish";
    const string kAttack2Anim = "Attack2";
    const string kAttack2FinAnim = "Attack2 Finish";
    const string kAttack3Anim = "Attack3";
    const string kAttack3FinAnim = "Attack3 Finish";
    const string kWallSlideAnim = "Wall Cling";
    const string kWallJumpAnim = "Wall Jump";
    const string kBackJumpAnim = "Back Jump";

    enum State
    {
        Idle,
        Running,
        Jumping,
        Falling,
        Landing,
        Jump_Transitioning,
        Crouching,
        Crouch_Idle,
        Crawling,
        Attack1_Finishing,
        Attacking1,
        Attack2_Finishing,
        Attacking2,
        Attack3_Finishing,
        Attacking3,
        WallSliding,
        WallJumping,
        BackJumping
    }

    State state;
    private Animator animator;
    private bool inAttack = false;
    [HideInInspector]
    public bool inBackJump = false;

    // Use this for initialization
    void Start()
    {
        animator = GetComponent<Animator>();
        stateInfo.count = 0;
    }

   public void Flip(float horizontal)
    {
        if (inBackJump)
        {
            return;
        }
        else
        {
            if (horizontal > 0 && stateInfo.faceRight || horizontal < 0 && !stateInfo.faceRight)
            {
                if (stateInfo.faceRight)
                {
                    stateInfo.faceRight = false;
                }
                else
                {
                    stateInfo.faceRight = true;
                }
                Vector3 theScale = transform.localScale;

                theScale.x *= -1;

                transform.localScale = theScale;
            }
        }
    }

    private void EnterState(State state)
    {
        switch (state)
        {
            case State.Idle:
                animator.Play(kIdleAnim);
                break;
            case State.Running:
                animator.Play(kRunAnim);
                break;
            case State.Jumping:
                animator.Play(kJumpAnim);
                break;
            case State.Falling:
                animator.Play(kFallAnim);
                break;
            case State.Landing:
                animator.Play(kLandAnim);
                break;
            case State.Jump_Transitioning:
                animator.Play(kJumpTransAnim);
                break;
            case State.Crouching:
                animator.Play(kCrouchAnim);
                break;
            case State.Crouch_Idle:
                animator.Play(kCrouchIdleAnim);
                break;
            case State.Crawling:
                animator.Play(kCrawlAnim);
                break;
            case State.Attacking1:
                animator.Play(kAttack1Anim);
                break;
            case State.Attack1_Finishing:
                animator.Play(kAttack1FinAnim);
                break;
            case State.Attacking2:
                animator.Play(kAttack2Anim);
                break;
            case State.Attack2_Finishing:
                animator.Play(kAttack2FinAnim);
                break;
            case State.Attacking3:
                animator.Play(kAttack3Anim);
                break;
            case State.Attack3_Finishing:
                animator.Play(kAttack3FinAnim);
                break;
            case State.WallSliding:
                animator.Play(kWallSlideAnim);
                break;
            case State.WallJumping:
                animator.Play(kWallJumpAnim);
                break;
            case State.BackJumping:
                animator.Play(kBackJumpAnim);
                break;

        }

        this.state = state;
    }

    public void ContinueState()
    {
        switch (state)
        {
            case State.Idle:
                inBackJump = false;
                if (stateInfo.attack)
                    EnterState(State.Attacking1);
                else if (stateInfo.backJump)
                    EnterState(State.BackJumping);
                else if (stateInfo.crouch)
                    EnterState(State.Crouching);
                else if (stateInfo.jump)
                    EnterState(State.Jumping);
                else if (stateInfo.input.x != 0)
                    EnterState(State.Running);
                break;
            case State.Running:
                Debug.Log("velocity.x: " + stateInfo.input.x);
                if (stateInfo.jump)
                    EnterState(State.Jumping);
                else if (stateInfo.input.x == 0)
                    EnterState(State.Idle);
                else if (stateInfo.backJump)
                    EnterState(State.BackJumping);
                break;
            case State.Jumping:
                if (stateInfo.wallSliding)
                    EnterState(State.WallSliding);
                else if (stateInfo.velocity.y <= 0)
                    EnterState(State.Jump_Transitioning);
                break;
            case State.Jump_Transitioning:
                if (stateInfo.wallSliding)
                    EnterState(State.WallSliding);
                else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                        EnterState(State.Falling);
                break;
            case State.Falling:
                if (stateInfo.wallSliding)
                    EnterState(State.WallSliding);
                else if (stateInfo.grounded)
                    EnterState(State.Landing);
                break;
            case State.Landing:
                if (stateInfo.crouch)
                    EnterState(State.Crouching);
                else if (stateInfo.jump)
                    EnterState(State.Jumping);
                else if (stateInfo.input.x != 0)
                    EnterState(State.Running);
                else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                    EnterState(State.Idle);
                break;
            case State.Crouching:
                if (!stateInfo.crouch)
                    EnterState(State.Idle);
                else if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                    EnterState(State.Crouch_Idle);
                break;
            case State.Crouch_Idle:
                if (!stateInfo.crouch)
                    EnterState(State.Idle);
                else if (stateInfo.input.x != 0)
                    EnterState(State.Crawling);
                break;
            case State.Crawling:
                if (!stateInfo.crouch)
                    EnterState(State.Idle);
                else if (stateInfo.input.x == 0)
                    EnterState(State.Crouch_Idle);
                break;
            case State.Attacking1:
                if (stateInfo.attack)
                {
                    inAttack = true;
                }
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    if (inAttack)
                    {
                        EnterState(State.Attacking2);
                        inAttack = false;
                    }
                    else
                        EnterState(State.Attack1_Finishing);
                }
                break;
            case State.Attack1_Finishing:
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                    EnterState(State.Idle);
                break;
            case State.Attacking2:
                if (stateInfo.attack)
                {
                    inAttack = true;
                }
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    if (inAttack)
                    {
                        EnterState(State.Attacking3);
                        inAttack = false;
                    }
                    else
                        EnterState(State.Attack2_Finishing);
                }
                break;
            case State.Attack2_Finishing:
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                    EnterState(State.Idle);
                break;
            case State.Attacking3:
                if (stateInfo.attack)
                {
                    inAttack = true;
                }
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    if (inAttack)
                    {
                        EnterState(State.Attacking1);
                        inAttack = false;
                    }
                    else
                        EnterState(State.Attack3_Finishing);
                }
                break;
            case State.Attack3_Finishing:
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                    EnterState(State.Idle);
                break;
            case State.WallSliding:
                if (stateInfo.jump)
                    EnterState(State.WallJumping);
                else if (stateInfo.grounded)
                    EnterState(State.Idle);
                break;
            case State.WallJumping:
                if (stateInfo.wallSliding)
                    EnterState(State.WallSliding);
                else if (stateInfo.velocity.y <= 0)
                    EnterState(State.Jump_Transitioning);
                break;
            case State.BackJumping:
                
                if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
                {
                    EnterState(State.Idle);
                    
                }
                break;

        }
    }

    public struct StateInfo
    {
        public Vector2 input;
        public Vector3 velocity;
        public bool faceRight;
        public bool jump, grounded, crouch, attack;
        public bool wallSliding, backJump;
        public float count;
        

        public void Reset()
        {
            jump = false;
            grounded = false;
            attack = false;
            wallSliding = false;
            backJump = false;
        }
    }
}
