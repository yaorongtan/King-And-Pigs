using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerState
{
    None,
    Movement,
    Jump,
    Attack,
    Dead,
    InDoor,
    OutDoor,
}

public class State
{
    protected PlayerController player;
    public PlayerState StateType;

    public State(PlayerController _player)
    {
        player = _player;
        
    }


    public virtual void Enter()
    {
        switch(StateType)
        {
            case PlayerState.Attack:
                Debug.Log("Player 进入 Attack 状态");
                break;
            case PlayerState.Dead:
                Debug.Log("Player 进入 Dead 状态");
                break;
            case PlayerState.InDoor:
                Debug.Log("Player 进入 InDoor 状态");
                break;
            case PlayerState.Jump:
                Debug.Log("Player 进入 Jump 状态");
                break;
            case PlayerState.Movement:
                Debug.Log("Player 进入 Movement 状态");
                break;
            case PlayerState.OutDoor:
                Debug.Log("Player 进入 OutDoor 状态");
                break;
        }
    }

    public virtual void Exit()
    {
        switch (StateType)
        {
            case PlayerState.Attack:
                Debug.Log("Player 退出 Attack 状态");
                break;
            case PlayerState.Dead:
                Debug.Log("Player 退出 Dead 状态");
                break;
            case PlayerState.InDoor:
                Debug.Log("Player 退出 InDoor 状态");
                break;
            case PlayerState.Jump:
                Debug.Log("Player 退出 Jump 状态");
                break;
            case PlayerState.Movement:
                Debug.Log("Player 退出 Movement 状态");
                break;
            case PlayerState.OutDoor:
                Debug.Log("Player 退出 OutDoor 状态");
                break;
        }
    }

   public virtual void UpdateInput()
    {

    }

    public virtual void UpdateLogic()
    {

    }

    public virtual void UpdatePhysicLogic()
    {

    }

    public virtual PlayerState ChangeState()
    {
        return PlayerState.None;
    }

}

public class Movement:State
{
    public Movement(PlayerController _player):base(_player)
    {
        this.StateType = PlayerState.Movement;
    }

    public override void Enter()
    {
        base.Enter();
        this.player.IsGround = true;
    }

    public override void UpdateInput()
    {
        float x = Input.GetAxis("Horizontal");
        //float y = Input.GetAxis("Vertical");

        float desiredSpeed =x * this.player.MaxSpeed;
        this.player.DesiredSpeed = desiredSpeed;

        Vector2 speed = this.player.Rig2D.velocity;

        speed.x = Mathf.MoveTowards(speed.x,desiredSpeed,this.player.MaxAcceleration);
        
        this.player.Rig2D.velocity = speed;
        if(speed.x!=0)
            this.player.transform.rotation = Quaternion.Euler(0,speed.x>0?0:180,0);


    }

    public override void UpdateLogic()
    {
        //水平上有速度，设置人物的动画
        this.player.Anim.SetFloat("speed", Mathf.Abs(this.player.Rig2D.velocity.x));
        this.player.Anim.SetBool("ground",true);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override PlayerState ChangeState()
    {

        if (Input.GetKeyDown(KeyCode.K)) return PlayerState.Jump;
        if (!this.player.isOnGround(this.player.GroundTextLayerMask)) return PlayerState.Jump;
        if (Input.GetKeyDown(KeyCode.J)) return PlayerState.Attack;

        return PlayerState.None;
    }

}

public class Jump:State
{
    public Jump(PlayerController _player) : base(_player)
    {
        this.StateType = PlayerState.Jump;
    }

    private float maxJumpSpeed;
    private float minJumpSpeed;
    private int jumpAllow = 1;
    private bool groundText = false;
    private float timer;
    private float spawn = 0.2f;
    private bool isFall;

    public override void Enter()
    {
        base.Enter();
        maxJumpSpeed = Mathf.Sqrt(-2.0f*player.Rig2D.gravityScale*Physics2D.gravity.y*player.MaxJumpHeight);
        minJumpSpeed = Mathf.Sqrt(-2.0f * player.Rig2D.gravityScale * Physics2D.gravity.y * player.MinJumpHeight);
        this.player.IsGround = false;
        jumpAllow = 1;
        timer = 0;

        //Debug.Log(maxJumpSpeed);
        //Debug.Log(minJumpSpeed);
    }


    public override void UpdateInput()
    {

        Vector2 speed = this.player.Rig2D.velocity;

        if (jumpAllow>0 && Input.GetKeyDown(KeyCode.K))
        {
            jumpAllow--;
            this.player.Rig2D.velocity = new Vector2();
            speed.y += minJumpSpeed;
           
        }

       
        if (this.player.Rig2D.velocity.y>0&&timer<spawn&&Input.GetKey(KeyCode.K))
        {
            timer += Time.deltaTime;
           
            speed.y = Mathf.Lerp(speed.y,maxJumpSpeed,timer/spawn);
        }


        float x = Input.GetAxis("Horizontal");

        this.player.DesiredSpeed = x*this.player.MaxSpeed;

        speed.x = Mathf.MoveTowards(speed.x,this.player.DesiredSpeed,this.player.MaxAirAcceleration);

        
        this.player.Rig2D.velocity = speed;
    }

    public override void UpdateLogic()
    {
        this.player.Anim.SetBool("ground", groundText&&this.player.Rig2D.velocity.y<0);
        this.player.Anim.SetBool("jump",player.Rig2D.velocity.y>=0);
        this.player.Anim.SetBool("fall",player.Rig2D.velocity.y<0);
    }

    public override void UpdatePhysicLogic()
    {
        //更新速度
        Vector2 speed = this.player.Rig2D.velocity;
       
        if(this.player.Rig2D.velocity.y<0)
        {
            speed.y = speed.y * Mathf.Pow(100, Time.fixedDeltaTime);
        }

        this.player.Rig2D.velocity = speed;
    }

    public override void Exit()
    {
        base.Exit();
        this.player.Anim.SetFloat("speed",0.0f);
    }

    public override PlayerState ChangeState()
    {
        groundText = this.player.isOnGround(this.player.GroundTextLayerMask);

        if (groundText && player.Rig2D.velocity.y <= 0) return PlayerState.Movement;

        if(Input.GetKeyDown(KeyCode.J))
        {
            if (this.player.Rig2D.velocity.y > 0) return PlayerState.Attack;

            float dist = this.player.DistToGround(this.player.GroundTextLayerMask);

            //Debug.Log("地面距离为"+ dist+",阈值为 "+ this.player.MinAirAttackThreshold);
            //Debug.Break();

            if (this.player.DistToGround(this.player.GroundTextLayerMask) > this.player.MinAirAttackThreshold)
             return PlayerState.Attack; 
        }

        return PlayerState.None;
    }

}

public class Attack:State
{
    public Attack(PlayerController player):base(player)
    {
        this.StateType = PlayerState.Attack;
        attackAnimationNameHash = Animator.StringToHash("Attack");
    }

    private int allowAttack;
    private int attackAnimationNameHash;

    public override void Enter()
    {
        base.Enter();
        allowAttack = 1;
    }

   


    public override void UpdateInput()
    {
        if(Input.GetKeyDown(KeyCode.J) && allowAttack>0)
        {
            allowAttack--;

            this.player.Anim.SetTrigger("attack");
        }
       
    }

    public override void UpdateLogic()
    {
        
    }

    public override PlayerState ChangeState()
    {
        var info = this.player.Anim.GetCurrentAnimatorStateInfo(0);

        if (info.shortNameHash != attackAnimationNameHash)
            return this.player.IsGround ? PlayerState.Movement : PlayerState.Jump;
        return PlayerState.None;
    }

    public override void Exit()
    {
        base.Exit();

    }

}

//TODO 
public class Dead:State
{
    public Dead(PlayerController player):base(player)
    {

    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
    }

    public override void Exit()
    {
        base.Exit();
    }


}




public class PlayerController : MonoBehaviour
{
    [Range(0,20)]
    [SerializeField]
    private float maxSpeed = 0.0f;//最大速度
    [Range(0, 20)]
    [SerializeField]
    private float maxAcceleration = 0.0f;//最大加速度
    [SerializeField, Range(0, 20f)]
    private float minJumpHeight;
    [SerializeField,Range(0.5f,20f)]
    private float maxJumpHeight;
    [SerializeField,Range(0,20)]
    private float maxAirAcceleratin;//跳跃的时候允许的最大加速度

    [Header("Attack")]
    [SerializeField,Range(0,2f)]
    private float minAirAttackDistThreshold = 0.0f;

    [SerializeField]
    private Vector2 groundCircle = new Vector2();
    [SerializeField,Range(0,5)]
    private float groundCircleRadius = 0;
    [SerializeField]
    private LayerMask groundTextLayerMask;


    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Rigidbody2D rig2D;
    private bool isGround = true;
    private float desiredSpeed;//期望速度
    private PlayerStateMachine stateMachine;


    //公有属性
    [SerializeField]
    public Animator Anim { get { return this.animator; } }
    [SerializeField]
    public Rigidbody2D Rig2D { get { return this.rig2D; } }
    public bool IsGround { get { return isGround; } set { this.isGround = value; } }
    public float DesiredSpeed { get { return this.desiredSpeed; } set { desiredSpeed = value; } }//期望速度 
    public float MaxSpeed { get { return maxSpeed; } }
    public float MaxAcceleration { get { return maxAcceleration; } }
    public float MaxJumpHeight { get { return maxJumpHeight; } }
    public float MinJumpHeight { get { return minJumpHeight; } }
    public float MaxAirAcceleration { get { return maxAirAcceleratin; } }
    public LayerMask GroundTextLayerMask { get { return this.groundTextLayerMask; } }
    public float MinAirAttackThreshold { get { return minAirAttackDistThreshold; } }


    public Dictionary<PlayerState, State> playerStates;


    private void Awake()
    {
        playerStates = new Dictionary<PlayerState, State>();
        stateMachine = this.gameObject.GetComponent<PlayerStateMachine>();

        //初始化状态和状态机
        var moveMentState = new Movement(this);
        var jumpState = new Jump(this);
        var attackState = new Attack(this);

        playerStates[PlayerState.Movement] = moveMentState;
        playerStates[PlayerState.Jump] = jumpState;
        playerStates[PlayerState.Attack] = attackState;


        stateMachine.Initialize(this,moveMentState);

    }

    private void Start()
    {
        
    }


    public void Update()
    {
        //防止在要进行状态改变的情况下还更新当前状态.
        stateMachine.ChangeState();

        

        stateMachine.currentState.UpdateInput();
        stateMachine.currentState.UpdateLogic();

    }


    public void FixedUpdate()
    {
        stateMachine.currentState.UpdatePhysicLogic();
    }


    /// <summary>
    /// 检测玩家是否在地面
    /// </summary>
    /// <returns></returns>
    public bool isOnGround(LayerMask layerMask)
    {
        Collider2D colliders = Physics2D.OverlapCircle(groundCircle +
            new Vector2(this.transform.position.x, this.transform.position.y), groundCircleRadius, layerMask);

        if (colliders == null) return false;

        //TODO 添加
        return true;
    }

    /// <summary>
    /// 人物距离地面的距离
    /// </summary>
    /// <returns></returns>
    public float DistToGround(LayerMask layerMask)
    {
        RaycastHit2D hit2D =  Physics2D.Raycast(this.transform.position,Vector2.down,float.MaxValue,layerMask);

        if (hit2D.collider == null) return float.MaxValue;
        
        return Vector2.Distance(hit2D.point,this.transform.position);
    }

    public void OnDrawGizmos()
    {
        if (IsGround)
            Gizmos.color = Color.blue;
        else Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCircle+new Vector2(this.transform.position.x, this.transform.position.y), groundCircleRadius);
        Gizmos.color = Color.white;
    }

}


