using UnityEngine;
using System.Collections;

public abstract class Enemy : WeaponHolder 
{
    // State
    //public enum State { IDLE = 0, RUN = 1, SHOOT = 2, HIT = 3, DEAD = 4 }; // critical these match animator component, hence exposed values
    //private State _currentState; // DON'T SET DIRECTLY!!! Use ChangeState();
    //public State currentState { get { return _currentState; } }


    protected uint _abilities = 0; //!< Set this to define what things this enemy can do
    protected float _walkSpeed;
    protected float _flySpeed;
    protected float _jumpForce;

    // To do with moving, probably will be different when merged with Lee's physics
    Vector2 _moveVector;
    protected Transform _transform;

    // Imported from old enemy
    protected WeaponManager _weaponManager;
    protected TicketManager _ticketManager;
    protected Animator _anim;
    protected SpriteRenderer _myRenderer;
    private Transform _graphicsTrans;
    private WeaponName _startingWeapon = WeaponName.NONE; // Weapon. The initWeapon is weapon he will spawn with if unarmed at spawn time

    protected int _maxHealth = 10;
    protected int _health;

    
    // Getters / Setters
    public uint Abilities { get { return _abilities; } }
    public Vector2 Position { get { return _transform.position; } set { _transform.position = value; } }
    public bool OnGround { get { return _mover.isGrounded; } }
    public WeaponName startingWeapon { set { _startingWeapon = value; } }
    public bool visible { get { return _myRenderer.gameObject.activeSelf; } set { _myRenderer.gameObject.SetActive(value); } }
    public bool collidable { get { return GetComponent<Rigidbody2D>().simulated; } set { GetComponent<Rigidbody2D>().simulated = value; } }
    
    
    protected abstract void Initialize(); //!< Override to define enemy specific functions that would normally be in Awake
    protected abstract void Spawn(); //!< Override to define enemy specific functions that are called when first spawn


    void Awake()
    {
        _transform = this.transform;
        _mover = GetComponent<Mover>();
        _anim = GetComponentInChildren<Animator>();
        _myRenderer = GetComponentInChildren<SpriteRenderer>();
        _graphicsTrans = _myRenderer.transform;
        _weaponManager = GameObject.FindGameObjectWithTag("WeaponManager").GetComponent<WeaponManager>();
        _ticketManager = GameObject.FindGameObjectWithTag("TicketManager").GetComponent<TicketManager>();
        myTeam = Team.ENEMY;

        Initialize();
    }

    // Given a weapon prefab will instantiate a new instance of it and pick it up
    public void PickupWeapon(Weapon weapon)
    {
        if (_weapon != null) return; // currently refuse if has weapon, later though we could make him drop current if already has one
        weapon.Pickup(this);
        weapon.SetDirection(new Vector2(_facingDirection, 0));
    }


    // Update is called once per frame
    void Update()
    {
        //Debug.Log (_currentState);
        //if (_currentState != State.DEAD && _currentState != State.HIT) // No update if dead
        //{
            // Set Direction
            if (Mathf.Sign(_graphicsTrans.localScale.x) != Mathf.Sign(_facingDirection))
            {
                _graphicsTrans.localScale = new Vector3(_graphicsTrans.localScale.x * -1, _graphicsTrans.localScale.y, _graphicsTrans.localScale.z);
            }


            _mover.Move(_moveVector); // move vector controlled by our friend the behaviour
        //}


        _moveVector = Vector2.zero;
    }





    // Called whenever enemy is launched from a pool and also on start
    public void Spawn(Vector3 position, float facingDirection)
    {
        //_currentState = State.IDLE; // Can't ChangeState() from DEAD so we set directly, only time we do this
        visible = true;
        collidable = true;
        _mover.Reset(); // Reset Mover so you ain't moving no more


        if (facingDirection < 0) facingDirection = -1; // Ensure facingDirection is either 1 or -1
        else facingDirection = 1;

        _transform.position = position;
        _facingDirection = facingDirection;
        _health = _maxHealth;

        // Create init weapon if unarmed.
        // Note that it might be better to have a "weapon pool" later rather than instantating new weapon
        // every time.
        if (_weapon == null && (int)_startingWeapon < (int)WeaponName.NUMBER_OF_WEAPONS)
        {
            // Request starting weapon from weapon manager and pick up
            PickupWeapon(_weaponManager.GetWeapon(_startingWeapon));
        }

        //For children
        Spawn();
    }


    private IEnumerator Shoot()
    {
        yield return new WaitForSeconds(0.5f);

        // Acually shoot
        //if (_currentState != State.DEAD && _weapon != null)
        //{
            _weapon.ShootTap();
            _weapon.ShootHold();
            yield return new WaitForSeconds(0.5f);
            //ChangeState(State.RUN);
        //}
    }


    // Always use instead of setting state directly
    /*
    public void ChangeState(State newState)
    {
        if (_currentState == State.DEAD) return; // can't change state if dying
        _currentState = newState;

        // Can't shoot if he ain't got no weapon!
        if (_currentState == State.SHOOT && _weapon == null) _currentState = State.IDLE;

        //_anim.SetInteger("state", (int)_currentState);

        switch (_currentState)
        {
            case State.SHOOT:
                StartCoroutine(Shoot());
                break;
            case State.IDLE:
                _anim.Play("Idle");
                break;
            case State.HIT:
                _anim.Play("Hit");
                break;
            case State.RUN:
                _anim.Play("Walk");
                break;
        }
    }
     * */

    public void LoseHealth(int health)
    {
        // Only want to die once!!
        if (_health <= 0) return;

        // Lose health and then die if none left
        _health -= health;
        if (_health <= 0) StartCoroutine(Die());
        else
        {
            StartCoroutine(HitFlash());
            StartCoroutine(GetHurt());
        }
    }

    IEnumerator HitFlash()
    {
        SetAnim(Anim.HIT); ;
        for (int i = 0; i < 5; i++)
        {
            _myRenderer.color = new Color(1.0f, 0.4f, 0.4f, 0.7f);
            yield return new WaitForSeconds(0.08f);
            _myRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            yield return new WaitForSeconds(0.08f);
        }

    }

    protected virtual IEnumerator GetHurt()
    {
        yield break;
    }

    public void DropWeapon()
    {
        if (_weapon == null) return;
        _weapon.Throw(Vector2.up * 12, true);
        _weapon = null;
    }

    // Override with unique death sequence! MAKE SURE SET AS DEAD AFTER!!
    protected virtual IEnumerator Die()
    {
        SetAsDead();
        yield break;
    }

    // This needs to be called at the end of die
    protected void SetAsDead()
    {
        if (_weapon != null) _weapon.Reset();
        EnemySpawner spawner = GetComponentInParent<EnemySpawner>();
        if (spawner != null) spawner.EnemyDied();
        gameObject.SetActive(false);
    }


    public bool HasAbilities(uint abilities)
    {
        return (_abilities & abilities) == abilities;
    }


    // Hath been functionated because probably wanna change stuff later
    enum Anim { IDLE, WALK, HIT }
    void SetAnim(Anim anim)
    {
        switch (anim)
        {
            case Anim.IDLE:     _anim.Play("Idle");     return;
            case Anim.WALK:     _anim.Play("Walk");     return;
            case Anim.HIT:      _anim.Play("Hit");      return;
        }
    }


  

    //------------------------------------------------//
    //----            ABILITY FUNCTIONS           ----//
    //  All are virtual so children can override and  //
    //  change implementation if they need to.        //
    //------------------------------------------------//
    public virtual void Walk(float dir)
    {
        SetAnim(Anim.WALK);
        _facingDirection = dir;

        _moveVector = new Vector2(dir * _walkSpeed, 0);
    }

    public virtual void UseLadder(float dir, LevelNode ladder)
    {
        // Note should be something here to check if on ladder but just for testing now
        Vector3 p = _transform.position;
        p.x = ladder.Centre.x;
        _transform.position = p;
        _moveVector = new Vector2(0, dir * _walkSpeed);
    }

    public virtual void Fly(Vector2 dir)
    {
        _moveVector = dir * _flySpeed;
    }

    public virtual void Jump()
    {
        _mover.Jump(_jumpForce);
    }

    public virtual void DropThroughPlatform()
    {
        _mover.DropThroughPlatform();
    }






}












public static class EnemyProps
{
    public const uint WALKS =   0x00000001;
    public const uint JUMPS =   0x00000002;
    public const uint FLYS =    0x00000004;
    public const uint SHIELDS = 0x00000008;
    public const uint LADDERS = 0x00000010;






}