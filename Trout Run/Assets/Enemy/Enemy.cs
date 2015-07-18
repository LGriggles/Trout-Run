using UnityEngine;
using System.Collections;

public abstract class Enemy : WeaponHolder 
{
    public enum Type { DRONE, FLYER, NUM_ENEMIES }

    // State
    public enum EnemyState { OK, HIT, DEAD }
    private EnemyState _enemyState = EnemyState.OK;
    public EnemyState CurrentState { get { return _enemyState; } }

    protected uint _abilities = 0; //!< Set this to define what things this enemy can do
    protected float _walkSpeed;
    protected float _flySpeed;
    protected float _jumpForce;

    // To do with moving, probably will be different when merged with Lee's physics
    Vector2 _moveVector;
    protected Transform _transform;
    protected EnemyAnimation _animation;

    // Imported from old enemy
    protected EnemyDirector _enemyDirector;
    protected WeaponManager _weaponManager;
    protected TicketManager _ticketManager;
    protected SpriteRenderer _myRenderer;
    private Transform _graphicsTrans;
    private WeaponName _startingWeapon = WeaponName.NONE; // Weapon. The initWeapon is weapon he will spawn with if unarmed at spawn time

    protected int _maxHealth = 10;
    protected int _health;
    protected float _ticketAmount = 0; //For best results, stick to even numbers

    
    // Getters / Setters
    public uint Abilities { get { return _abilities; } }
    public Vector2 Position { get { return _transform.position; } set { _transform.position = value; } }
    public bool OnGround { get { return _mover.isGrounded; } }
    public WeaponName startingWeapon { set { _startingWeapon = value; } }
    public bool visible { get { return _myRenderer.gameObject.activeSelf; } set { _myRenderer.gameObject.SetActive(value); } }
    public bool collidable { get { return GetComponent<Rigidbody2D>().simulated; } set { GetComponent<Rigidbody2D>().simulated = value; } }
    public void SetDirector(EnemyDirector director) { _enemyDirector = director; }
    
    
    protected abstract void Initialize(); //!< Override to define enemy specific functions that would normally be in Awake
    protected abstract void Spawn(); //!< Override to define enemy specific functions that are called when first spawn


    void Awake()
    {
        _transform = this.transform;
        _mover = GetComponent<Mover>();
        _animation = new EnemyAnimation(GetComponentInChildren<Animator>());
        _myRenderer = GetComponentInChildren<SpriteRenderer>();
        _graphicsTrans = _myRenderer.transform;
        _weaponManager = GameObject.FindGameObjectWithTag("WeaponManager").GetComponent<WeaponManager>();
        _ticketManager = GameObject.FindGameObjectWithTag("TicketManager").GetComponent<TicketManager>();
        myTeam = Team.ENEMY;

        Initialize();
    }

    

    // Update is called once per frame
    void Update()
    {
        // Set Direction
        if (Mathf.Sign(_graphicsTrans.localScale.x) != Mathf.Sign(_facingDirection))
        {
            _graphicsTrans.localScale = new Vector3(_graphicsTrans.localScale.x * -1, _graphicsTrans.localScale.y, _graphicsTrans.localScale.z);
        }
        if (_weapon != null) _weapon.SetDirection(new Vector2(_facingDirection, 0));


        _mover.Move(_moveVector); // move vector controlled by our friend the behaviour
        _moveVector = Vector2.zero;
    }


    // Called whenever enemy is launched from a pool and also on start
    public void Spawn(Vector3 position, float facingDirection)
    {
        _enemyState = EnemyState.OK;
        visible = true;
        collidable = true;
        _mover.Reset(); // Reset Mover so you ain't moving no more

        _transform.position = position;
        _facingDirection = Mathf.Sign(facingDirection);
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


    

    public void DropWeapon()
    {
        if (_weapon == null) return;
        _weapon.Throw(Vector2.up * 12, true);
        _weapon = null;
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

    // Get hit
    public void LoseHealth(int health)
    {
        // Only want to die once!!
        if (_health <= 0) return;

        // Lose health and then die if none left
        _health -= health;
        if (_health <= 0)
        {
            StartCoroutine(Die());
        }
        else
        {
            StartCoroutine(HitFlash());
        }
    }

    // Get Hit - routine
    IEnumerator HitFlash()
    {
        _enemyState = EnemyState.HIT;
        _animation.SetAnim(AnimationHashIDs.Anim.HIT);
        for (int i = 0; i < 5; i++)
        {
            _myRenderer.color = new Color(1.0f, 0.4f, 0.4f, 0.7f);
            yield return new WaitForSeconds(0.08f);
            _myRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            yield return new WaitForSeconds(0.08f);
        }
        _enemyState = EnemyState.OK;
    }



    // Die!
    private IEnumerator Die()
    {
        // Set state and drop your trousers I mean weapon
        _enemyState = EnemyState.DEAD;
        _animation.SetAnim(AnimationHashIDs.Anim.DIE);
        DropWeapon();

        // Sort them lovely tickets out blud
        float kingOfDreamland = 0;
        for (int i = 0; i < _ticketAmount; i++)
        {
            GameObject johnBervage = _ticketManager.SpawnTicket(new Vector2(transform.position.x, transform.position.y));
            if (johnBervage != null)
            {
                //johnBervage.GetComponent<Rigidbody2D>().AddForce(new Vector2 (-((ticketAmount/2) * 100) + (i * 100)+50, Mathf.Abs(-((ticketAmount/2) * 75) + (i * 100)+50)));
                johnBervage.GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Cos(kingOfDreamland), Mathf.Sin(kingOfDreamland)) * 400);
                kingOfDreamland += (360 / _ticketAmount) * Mathf.Deg2Rad;
            }
        }

        // Start die animation, handled in children
        yield return StartCoroutine(DeathSequence());

        // Clear up
        if (_weapon != null) _weapon.Reset();
        //if (_enemyDirector != null) _enemyDirector.EnemyDied();
        gameObject.SetActive(false);
        
        yield break;
    }

    protected abstract IEnumerator DeathSequence();


    public bool HasAbilities(uint abilities)
    {
        return (_abilities & abilities) == abilities;
    }


    
  

    //------------------------------------------------//
    //----            ABILITY FUNCTIONS           ----//
    //  All are virtual so children can override and  //
    //  change implementation if they need to.        //
    //------------------------------------------------//
    public virtual void DoNothing()
    {
        _animation.SetAnim(AnimationHashIDs.Anim.IDLE);
    }


    public virtual void Walk(float dir)
    {
        _animation.SetAnim(AnimationHashIDs.Anim.WALK);
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