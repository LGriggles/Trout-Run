// An enemy is specifically a standard enemy that can be armed with a standard weapon that can be
// taken by the player. Bosses or irregular enemies / static enemies etc may be better to not
// inherit from this class

// Important to note that start up logic (Awake, Start, Init) is handled in children using OnStart(), OnAwake()
// and OnInit() to allow base logic to execute.

using UnityEngine;
using System.Collections;

public class EnemyOld : WeaponHolder
{
    protected WeaponManager _weaponManager;
    protected TicketManager _ticketManager;
    
    public Transform _graphicsTrans; // link graphics (should be separate transform parented to Enemy object)
    protected Animator _anim;
    protected SpriteRenderer _myRenderer;

    protected Transform _myTrans;
    protected Transform _playerTrans;
    
    // Weapon. The initWeapon is weapon he will spawn with if unarmed at spawn time
    public WeaponName startingWeapon { set { _startingWeapon = value; } }
    private WeaponName _startingWeapon = WeaponName.NONE;
    
    protected int _maxHealth = 10;
    protected int _health;

    // Bools for setting visible and collidable
    public bool visible { get { return _graphicsTrans.gameObject.activeSelf; } set { _graphicsTrans.gameObject.SetActive(value); } }
    public bool collidable  { get { return GetComponent<Rigidbody2D>().simulated; } set { GetComponent<Rigidbody2D>().simulated = value; } }

    //protected Mover _mover;
    //public Mover mover { get { return _mover; } }  // accessing like this is probably faster and easier than using GetComponent
    
    // These are called from base init functions / messages so children can have specific logic
    protected virtual void OnStart(){} // Called at the end of Start() for additional child logic
    protected virtual void OnAwake () {} // Called at the end of Awake() for additional child logic
    protected virtual void OnInit() {} // Called at the end of Init() for additional child logic
    
    void Awake()
    {
        _anim = _graphicsTrans.gameObject.GetComponent<Animator>();
        _myTrans = transform;
        _myRenderer = _graphicsTrans.gameObject.GetComponent<SpriteRenderer>();
        _playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        _weaponManager = GameObject.FindGameObjectWithTag ("WeaponManager").GetComponent<WeaponManager>();
        _ticketManager = GameObject.FindGameObjectWithTag ("TicketManager").GetComponent<TicketManager>();
        myTeam = Team.ENEMY;
        _mover = GetComponent<Mover>();
        OnAwake();
    }
    
    // Given a weapon prefab will instantiate a new instance of it and pick it up
    public void PickupWeapon(Weapon weapon)
    {
        if(_weapon != null) return; // currently refuse if has weapon, later though we could make him drop current if already has one
        weapon.Pickup(this);
        weapon.SetDirection(new Vector2(_facingDirection, 0));
    }
    
    void Start() { OnStart(); }
    
    // Called whenever enemy is launched from a pool and also on start
    public void Init(Vector3 position, float facingDirection) 
    {
        visible = true;
        collidable = true;
        _mover.Reset(); // Reset Mover so you ain't moving no more
        
        
        if(facingDirection < 0 ) facingDirection = -1; // Ensure facingDirection is either 1 or -1
        else facingDirection = 1;
        
        _myTrans.position = position;
        _facingDirection = facingDirection;
        _health = _maxHealth;
        
        // Create init weapon if unarmed.
        // Note that it might be better to have a "weapon pool" later rather than instantating new weapon
        // every time.
        if(_weapon == null && (int)_startingWeapon < (int)WeaponName.NUMBER_OF_WEAPONS )
        {
            // Request starting weapon from weapon manager and pick up
            PickupWeapon(_weaponManager.GetWeapon(_startingWeapon));
        }
        
        //For children
        OnInit ();
    } 
    
    public void LoseHealth(int health)
    {
        // Only want to die once!!
        if(_health <= 0) return;
        
        // Lose health and then die if none left
        _health -= health;
        if(_health <= 0) StartCoroutine(Die ());
        else{
            StartCoroutine(HitFlash());
            StartCoroutine(GetHurt());
        }
    }

    IEnumerator HitFlash()
    {
        for (int i = 0; i < 5; i++)
        {
            _myRenderer.color = new Color (1.0f, 0.4f, 0.4f, 0.7f);
            yield return new WaitForSeconds(0.08f);
            _myRenderer.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
            yield return new WaitForSeconds(0.08f);
        }
        
    }

    protected virtual IEnumerator GetHurt(){
        yield break;
    }
    
    public void DropWeapon()
    {
        if(_weapon == null) return;
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
        if(_weapon != null) _weapon.Reset();
        EnemySpawner spawner = GetComponentInParent<EnemySpawner>();
        if(spawner != null) spawner.EnemyDied();
        gameObject.SetActive(false);
    }
}
