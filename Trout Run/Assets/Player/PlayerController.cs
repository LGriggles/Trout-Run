using UnityEngine;
using System.Collections;


public class PlayerController : WeaponHolder
{

    Collider2D[] collisionResults = new Collider2D[100];

    //Player hollistic state
    enum PlayerStates
    {
        NORMAL,
        AIRDODGING,
        GROUNDDODGING,
        LEVITATION,
        GROUNDLOCK
    }
    PlayerStates _playerState;
    
    //Levitation
    public bool _levitatedThisJump;



    struct DodgingStruct
    {
        //Dodging
        public bool _inputPeriod; //Set to true if you press dodge - gives you about a fifth of a second to input a direction
        public bool _executedDodge; //Have you done a dodge?
        public float _curDodgeTime; //Decreases to zero then can dodge again
        //Ground dodging
        public KnockBack groundDodgeKnockBack;
    }
    
    DodgingStruct _dodging;
    
    //Sprite stuff
    private Animator _anim;
    private SpriteRenderer _myRenderer;
    public GameObject sprite;
    private GameObject _arm;
    
    /* SLAMMIN CONTROL STICK STUFF :LEE
    float moveDir;
    bool _readyToSlamStick = true;
    public bool _running = false;*/
    Vector2 _inputMovementAxis = new Vector2();
    Vector2 _moveVector = new Vector2();
    
    public float normalSpeed = 32;
    public float gimpedSpeed = 24;
    private float _speed;
    public float speed { get { return _speed; } }

    private float _tempDrag, _tempGrav, _tempGrdAtt, _tempUngrdSpeed;

    bool _hasDoubleJump = false;
    public float jumpForce = 0.5f;
    private bool _pickup = false; // flag for when pressed "pickup" button
    private bool _trig = false; // flag for when trigger buttons are pressed in
    private bool _resurrectDeadOnPlanetJupiter = false; // flag for the levitate co-routine to stop touching itself at night
    private bool _immunity = false; // flag for temporary immunity upon being hit
    public bool immunity { get { return _immunity; } }

    private int _layPickup;
    private int _layEnemy;
    
    private Vector2 _weaponDirection = Vector2.right;
    public Vector2 weaponDirection { get { return _weaponDirection; } }
    private Vector2 throwForce;
    

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        _arm = GameObject.Find("Arm");
        _anim = GetComponentInChildren<Animator>();
        _mover = GetComponent<Mover>();
        _layPickup = LayerMask.NameToLayer("IgnoreAllButPlayer");
        _layEnemy = LayerMask.NameToLayer("Enemy");
        myTeam = Team.PLAYER;
        _myRenderer = sprite.GetComponent<SpriteRenderer>();
        //Set up dodging
        _dodging._curDodgeTime = 0;
        _dodging._executedDodge = false;
        _dodging._inputPeriod = false;
    }

    public void reAwaken()
    {
        _tempDrag = _mover.drag;
        _tempGrav = _mover.gravity;
        _tempGrdAtt = _mover.groundAttraction;
        _tempUngrdSpeed = _mover.minUngroundingSpeed;
        Awake();
        Destroy(_mover);
        _mover = gameObject.AddComponent<Mover>();
        _mover.drag = _tempDrag;
        _mover.gravity = _tempGrav;
        _mover.groundAttraction = _tempGrdAtt;
        _mover.minUngroundingSpeed = _tempUngrdSpeed;
        //LevelController.Reset();
    }
    
    //When this is called the player can shoot
    void UpdateAllowShooting()
    {
        // SHOOTING THINGS
        if( _weapon != null)
        {
            // Aim the gun based on direction pressed (temporary - will need some way of making this generic for all weapons)
            if(Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                if(!_trig)
                {
                    _weaponDirection = new Vector2(Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical"));
                    _weapon.SetDirection(_weaponDirection);
                    _arm.transform.localRotation = _weapon.transform.localRotation;
                }
            }
            
            // Shoot the weapon
            if(Input.GetButtonDown("Shoot")) _weapon.ShootTap();
            if(Input.GetButton ("Shoot")) _weapon.ShootHold();
            if(Input.GetButtonUp ("Shoot")) _weapon.ShootRelease();
        }
        else
        {
            // aint got no weapon :(
            if(Input.GetButtonDown("Shoot")) DisarmEnemy();
        }
    }
    
    void UpdateStateNormal()
    {
        UpdateAllowShooting();
        if(_mover.isGrounded)
        {
            _speed = normalSpeed;
            _dodging._curDodgeTime = 0;
            _levitatedThisJump = false;
        }
        // Few little stuff for debugging
        // ESCAPE!!!!
        if(Input.GetKeyDown(KeyCode.Escape))Application.Quit();
        //if(Input.GetKeyDown(KeyCode.R)) MiniProfiler.Reset();
        //MiniProfiler.AddMessage ("Velocity " + _mover.velocity + "\nGround " + _mover.isGrounded);
        
        
        // ACTUAL UPDATE BEGINS HERE!!!!
        // Movement
        // Player control here!
        
        _moveVector.x = Input.GetAxis("Horizontal") * _speed;
        _mover.Move(_moveVector); // move
        
        if(_moveVector.x > 0.05f)
        {

            if(!_trig)
            {
                _facingDirection = 1;
            }
            _arm.transform.localPosition = new Vector3(Mathf.Abs(_arm.transform.localPosition.x), 
            _arm.transform.localPosition.y, _arm.transform.localPosition.z);
        }
        else if(_moveVector.x < -0.05f)
        {
            if(!_trig)
            {
                _facingDirection = -1;
            }
            _arm.transform.localPosition = new Vector3(-Mathf.Abs(_arm.transform.localPosition.x), 
            _arm.transform.localPosition.y, _arm.transform.localPosition.z);
        }

        if(Input.GetAxis("Vertical") > 0.25f && !_mover.isGrounded)
        {
            print("fall");
            GetComponent<Rigidbody2D>().AddForce(new Vector2(0,5));
        }

        if(Input.GetButtonDown("Jump"))
        {
            if(Input.GetAxis("Vertical") <= -0.5f && _mover.onPlatform) _mover.DropThroughPlatform();
            else 
            {
                if(_mover.isGrounded)
                {
                    _mover.Jump (jumpForce); // jump
                }
                else
                {
                    //Pressed jump in the air
                }
            }
        }
        if(Input.GetButtonUp ("Jump"))
        {
            _mover.endExtendedJump();
        }
        
        
        if(Input.GetButtonDown ("Pickup") && !_pickup)
        {
            if(_weapon == null) StartCoroutine(PickupWeapon());
            else DropWeapon ();
        }

        else if(Input.GetButtonDown("Special") && !_dodging._executedDodge && !_dodging._inputPeriod)
        {
            StartCoroutine(DodgeLeniency(0.2f));
        }

        //DODGING STUFF
        if(!_mover.isGrounded && _dodging._inputPeriod == true)
        {
            //Do air dodging, this must be before the levitation stuff as that's for if you press specially neaturally
            if(Input.GetAxis("Horizontal") > 0.30f)
            {
                //Right
                _mover.RemoveAllMomentum();
                _mover.AddKnockBackImpact(new Vector2(1,0.4f),200);
                _playerState = PlayerStates.AIRDODGING;
                _dodging._curDodgeTime = 50;
                _dodging._executedDodge = true;
            }
            else if(Input.GetAxis("Horizontal") < -0.30f)
            {
                //Left
                _mover.RemoveAllMomentum();
                _mover.AddKnockBackImpact(new Vector2(-1,0.4f),200);
                _playerState = PlayerStates.AIRDODGING;
                _dodging._curDodgeTime = 50;
                _dodging._executedDodge = true;
            }
        }
        else if(_mover.isGrounded && _dodging._inputPeriod == true)
        {
        if(Input.GetAxis("Horizontal") > 0.20f)
        {
            //Right
            _playerState = PlayerStates.GROUNDDODGING;
            _mover.AddKnockBackImpact(new Vector2(1,0),200);
            _dodging._curDodgeTime = 25;
        }
        else if(Input.GetAxis("Horizontal") < -0.20f)
        {
            //Left
            _playerState = PlayerStates.GROUNDDODGING;
            _mover.AddKnockBackImpact(new Vector2(-1,0),200);
            _dodging._curDodgeTime = 25;
        }
        }

        if(Input.GetAxis("Trigger") < 0.5f && Input.GetAxis("Trigger") > -0.5f )
        {
            _trig = false;
        }

        if(!_mover.isGrounded) //Isn't on the floor
        {
            if(!_levitatedThisJump && (Input.GetAxis("Trigger") >= 0.5f || Input.GetAxis("Trigger") <= -0.5f))
            {
                if (_trig == false)
                {
                    _playerState = PlayerStates.LEVITATION;
                    _trig = true;
                    _mover.RemoveAllMomentum();
                    _levitatedThisJump = true;
                }
            }
        }
        else if((Input.GetAxis("Trigger") >= 0.5f || Input.GetAxis("Trigger") <= -0.5f)) //Is on the floor
        {
            if (_trig == false)
            {
                _trig = true;
            }
        }
        
        //Time until can dodge again
        if(_dodging._curDodgeTime > 0)
        {
            if(_mover.isGrounded)
            {
                _dodging._curDodgeTime -= 50 * Time.deltaTime;
            }
        }
        else if (_mover.isGrounded)
        {
            _dodging._curDodgeTime = 0;
            _dodging._executedDodge = false;
        }
    }
    
    void UpdateStateLevitation()
    {
        //_myRenderer.color = new Color (1.0f, 0.5f, 1.0f, 1.0f);
        UpdateAllowShooting();
        _levitatedThisJump = true;
        if (!_resurrectDeadOnPlanetJupiter)
        StartCoroutine(LevitationStateOver());
        _mover.RemoveAllMomentum();
        if(Input.GetAxis("Horizontal") > 0.30f && Input.GetButtonDown("Special"))
        {
            //Right
            _mover.AddKnockBackImpact(new Vector2(1,0.4f),200);
            _playerState = PlayerStates.AIRDODGING;
            _dodging._curDodgeTime = 50;
            _dodging._executedDodge = true;
        }
        else if(Input.GetAxis("Horizontal") < -0.30f && Input.GetButtonDown("Special"))
        {
            //Left
            _mover.AddKnockBackImpact(new Vector2(-1,0.4f),200);
            _playerState = PlayerStates.AIRDODGING;
            _dodging._curDodgeTime = 50;
            _dodging._executedDodge = true;
        }
        else if(Input.GetButtonDown("Special") || (Input.GetAxis("Trigger") >= 0.5f || Input.GetAxis("Trigger") <= -0.5f) && !_trig)
        {
            //It's over player
            _playerState = PlayerStates.NORMAL;
        }
    }
    
    IEnumerator LevitationStateOver()
    {
        if(_playerState == PlayerStates.LEVITATION && !_resurrectDeadOnPlanetJupiter)
        {
            _resurrectDeadOnPlanetJupiter = true;
            yield return new WaitForSeconds(0.5f);
            _playerState = PlayerStates.NORMAL;
        }
        _resurrectDeadOnPlanetJupiter = false;
    }
    
    void UpdateStateGroundDodging()
    {
        //UpdateAllowShooting();
        bool isItOverMummy = false;
        UpdateAllowShooting();
        if(Input.GetButtonDown ("Pickup") && !_pickup)
        {
            if(_weapon == null) StartCoroutine(PickupWeapon());
            else DropWeapon ();
        }
        
        if(_dodging._curDodgeTime > 0)
        {
            _dodging._curDodgeTime -= 50 * Time.deltaTime;
        }
        else
        {
            if(!isItOverMummy)
            {
                StartCoroutine(GroundDodgeOver());
                isItOverMummy = true;
                _dodging._curDodgeTime = 10;
            }
        }
    }

    void UpdateStateGroundLock()
    {
        _speed = gimpedSpeed;
        UpdateAllowShooting();
        if(!_mover.isGrounded || (Input.GetAxis("Trigger") < 0.5f && Input.GetAxis("Trigger") > -0.5f))
        {
            _playerState = PlayerStates.NORMAL;
        }
        if(Input.GetButtonDown("Jump"))
        {
            if(Input.GetAxis("Vertical") <= -0.5f && _mover.onPlatform) 
            {
                _mover.DropThroughPlatform();
                _playerState = PlayerStates.NORMAL;
            }
            else 
            {
                _mover.Jump(jumpForce);
                _playerState = PlayerStates.NORMAL;
            }
        }
    }
    
    IEnumerator GroundDodgeOver()
    {
        yield return new WaitForSeconds(0.05f);
        if(_playerState == PlayerStates.GROUNDDODGING)
        {
            _playerState = PlayerStates.NORMAL;
        }
    }
    
    void UpdateStateAirDodging()
    {
        UpdateAllowShooting();
        if(((Input.GetAxis("Trigger") >= 0.5f || Input.GetAxis("Trigger") <= -0.5f) && !_trig) && !_mover.isGrounded && !_levitatedThisJump)
        {
            _playerState = PlayerStates.LEVITATION;
            _mover.RemoveAllMomentum();
             _levitatedThisJump = true;
            _trig = true;
        }
        
        
        ////Still can pick up and throw weapons because you're hot shit
        //if(Input.GetButtonDown ("Pickup") && !_pickup)
        //{
            if(_weapon == null) StartCoroutine(PickupWeapon());
            //else DropWeapon ();
        //}
        
        //Hit the land, you gotta wait to move again bruh
        if(_mover.isGrounded)
        {
            _dodging._curDodgeTime = 0;
            _mover.RemoveAllMomentum();
            StartCoroutine(AirDodgingHitGround());
        }
        else //Can influence direction if in the air
        {
            _moveVector.x = Input.GetAxis("Horizontal") * _speed;
            _mover.Move(_moveVector); // move
        }
        
        //Recover the dodging powerrrrrr
        if(_dodging._curDodgeTime > 0)
        {
            if(_mover.isGrounded)
            {
                _dodging._curDodgeTime -= 50 * Time.deltaTime;
            }
        }
        else //You're done, switch to normal state
        {
            _levitatedThisJump = false;
            _playerState = PlayerStates.NORMAL;
            _dodging._curDodgeTime = 0;
        }
    }
    
    
    //Hit the ground, delay until can do anything again
    IEnumerator AirDodgingHitGround()
    {
        _levitatedThisJump = false;
        yield return new WaitForSeconds(0.02f);
        _dodging._curDodgeTime = 0;
        if(_playerState == PlayerStates.AIRDODGING)
        {
            _playerState = PlayerStates.NORMAL;
        }
    }
    
    void FixedUpdate()
    {
        
    }
    // Update is called once per frame
    void Update ()
    {
        switch(_playerState)
        {
            case PlayerStates.NORMAL:
                UpdateStateNormal();
                break;
                
            case PlayerStates.AIRDODGING:
                UpdateStateAirDodging();
                break;
                
            case PlayerStates.LEVITATION:
                UpdateStateLevitation();
                break;
                
            case PlayerStates.GROUNDDODGING:
                UpdateStateGroundDodging();
                break;

            case PlayerStates.GROUNDLOCK:
                UpdateStateGroundLock();
                break;
        }
        UpdateAnimation();
        MiniProfiler.AddMessage("Stick" + Input.GetAxis("Horizontal"));
        MiniProfiler.AddMessage("Direction " + _facingDirection);
        MiniProfiler.AddMessage("Player state" + _playerState);
    }
    
    void DropWeapon()
    {
        throwForce = _mover.velocity + new Vector2(Input.GetAxis ("Horizontal") * 12, Input.GetAxis ("Vertical") * 12);
        _weapon.Throw (throwForce, true);
        _weapon = null;
    }
    
    void LoseWeapon()
    {
        _weapon.Lose ();
        _weapon = null;
    }
    
    public void GetHit()
    {
        _immunity = true;
        StartCoroutine(HitFlash());
        if(_weapon != null) LoseWeapon();
    }

    IEnumerator HitFlash()
    {
        for (int i = 0; i < 5; i++)
        {
        _myRenderer.color = new Color (1.0f, 0.4f, 0.4f, 0.7f);
        yield return new WaitForSeconds(0.1f);
        _myRenderer.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
        yield return new WaitForSeconds(0.1f);
        }
        _immunity = false;
    }

    IEnumerator DodgeLeniency(float leniencyTime)
    {
        _dodging._inputPeriod = true;
        yield return new WaitForSeconds(leniencyTime);
        _dodging._inputPeriod = false;
    }

    private IEnumerator PickupWeapon()
    {
        _pickup = true;
        yield return new WaitForFixedUpdate();
        _pickup = false;
    }

    private void DisarmEnemy()
    {
        int size = Physics2D.OverlapCircleNonAlloc(transform.position,1,collisionResults,1 << LayerMask.NameToLayer("Enemy"));
        for(int i = 0; i < size; i++)
        {
            Collider2D colliderIter = collisionResults[i];
            Mover mover = colliderIter.GetComponentInParent<Mover>();
            WeaponHolder holder = colliderIter.GetComponentInParent<WeaponHolder>();
            EnemyOld enemy = colliderIter.GetComponentInParent<EnemyOld>();
            if(mover != null)
            {
                int enemySide = (int)(Mathf.Sign(mover.transform.position.x - transform.position.x));
                if(_facingDirection != enemySide) return;


                enemy.DropWeapon();
                Vector2 knockBackVector = new Vector2(enemy.transform.position.x - transform.position.x,0);
                enemy.mover.AddKnockBackImpact(knockBackVector, 40);
            }
        }
    }
    
    
    void OnTriggerStay2D(Collider2D other)
    {
        if(other.gameObject.layer == _layPickup)
        {
            if(_pickup) // if pressed pickup during last update...
            {
                // Check for Weapon component in other to see if pickup is a weapon
                Weapon weaponPickup = other.GetComponentInParent<Weapon>();
                if(weaponPickup != null && _weapon == null)
                {
                    weaponPickup.Pickup(this, new Vector2(0.3f, 0.1f));
                    weaponPickup.SetDirection(new Vector2(_facingDirection, 0));
                }
                StartCoroutine(PickupWeapon());
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "CollectibleTrigger")
        {
            Pickup pickup = other.gameObject.GetComponentInParent<Pickup>();
            pickup.Collected(this);
        }
        else if (other.gameObject.name == "DoorTrigger")
        {
            if (Input.GetButton("Jump"))
            {
                Door door = other.gameObject.GetComponentInParent<Door>();
                door.Enter();
            }
        }
    }
    
    private void UpdateAnimation ()
    {
        if(Mathf.Sign(_anim.transform.localScale.x) != Mathf.Sign(_facingDirection))
        {
            _anim.transform.localScale = new Vector3(_anim.transform.localScale.x * -1, _anim.transform.localScale.y, _anim.transform.localScale.z);
        }
        
        if (_mover.isGrounded)
        {
            if (_mover.velocity.x < 0.3 && _mover.velocity.x > -0.3)
            {
                if (Input.GetAxis("Vertical") > 0.3)
                {
                    _anim.Play("IdleUp");
                    _anim.speed = 1;
                }
                else 
                {
                    _anim.Play("Idle1");
                    _anim.speed = 1;
                }
            }
            else if (Input.GetAxis("Vertical") > 0.3)
            {
                _anim.Play("WalkUp");
                _anim.speed = 0.5f + Mathf.Abs(_mover.velocity.x / 4);
            }
            else
            {
                _anim.Play("Walk");
                _anim.speed = 0.5f + Mathf.Abs(_mover.velocity.x / 4);
            }
        }
        else if (_mover.velocity.y > 0)
        {
            _anim.Play("Jump");
            _anim.speed = 1;
        }
        else if (_mover.velocity.y < 0)
        {
            _anim.Play("Fall");
            _anim.speed = 1;
        }
    }
}
