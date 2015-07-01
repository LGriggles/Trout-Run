using UnityEngine;
using System.Collections;

public class EnemyLeeFlyer : Enemy
{
    // State
    public enum State { IDLE = 0, RUN = 1, SHOOT = 2, HIT = 3, DEAD = 4 }; // critical these match animator component, hence exposed values
    private State _currentState; // DON'T SET DIRECTLY!!! Use ChangeState();
    public State currentState { get { return _currentState; } }
    
    // How fast he moves
    public float speed = 32;
    
    // Death and shooting
    public ParticleSystem explosion;
    
    // Stuff for this guy's specific behaviour
    private Vector2 _weaponOffset = new Vector2(0.07f, 0.03f);
    private float _distanceToChange = 5; // must be at least this distance to change facingDirection
    private int _shotFrequency = 240;
    private float _shotCoolDown = 2; // in seconds
    private float _coolDownTimer = 0;
    private bool _canShoot = true;
    
    
    // Dependancies
    protected override void OnAwake () 
    {
        _maxHealth = 10;
        _myRenderer.color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
    }
    
    // OnInit is called whenever enemy is created from a pool (activated basically, rather than instantiating over and over)
    protected override void OnInit()
    {
        _mover.gravity = 0;
        _currentState = State.IDLE; // Can't ChangeState() from DEAD so we set directly, only time we do this
        explosion.time = 0;
        if(_weapon != null) _weapon.SetPositionOffset(_weaponOffset);
    }
    
    
    // Update is called once per frame
    void Update ()
    {
        //Debug.Log (_currentState);
        if(_currentState != State.DEAD && _currentState != State.HIT) // No update if dead
        {
            // Update Behaviour
            UpdateBehaviour();
            
            // Set Direction
            if(Mathf.Sign(_graphicsTrans.localScale.x) != Mathf.Sign(_facingDirection))
            {
                _graphicsTrans.localScale = new Vector3(_graphicsTrans.localScale.x * -1, _graphicsTrans.localScale.y, _graphicsTrans.localScale.z);
            }
            
            // Update based  on state
            switch(_currentState)
            {
                case State.RUN:
                    //Move
                    _mover.Move(Vector2.right * _facingDirection * speed);
                    break;
            }
        }
    }
    
    
    void UpdateBehaviour()
    {
        if(_currentState == State.IDLE) IdleDelay();
        
        if(_playerTrans.position.x <= _myTrans.position.x - _distanceToChange) // if he's to my left...
            _facingDirection = -1;
        else if(_playerTrans.position.x >= _myTrans.position.x + _distanceToChange)
            _facingDirection = 1;
        
        // Set weapon facingDirection
        if(_weapon != null) _weapon.SetDirection(new Vector2(_facingDirection, 0));
        
        // Every random x shoot bullet
        if(_canShoot && _mover.isGrounded)
        {
            int rand = Random.Range(0, _shotFrequency);
            if(rand == 0)
            {
                ChangeState(State.SHOOT);
                _coolDownTimer = _shotCoolDown;
                _canShoot = false;
            }
        }
        
        if(_coolDownTimer > 0) _coolDownTimer -= Time.deltaTime;
        else _canShoot = true;
    }
    
    
    private IEnumerator IdleDelay()
    {
        yield return new WaitForSeconds (2.0f);
        ChangeState(State.RUN);
    }
    
    protected override IEnumerator GetHurt()
    {
        ChangeState (State.HIT);
        yield return new WaitForSeconds (1.0f);
        ChangeState(State.RUN);
        _anim.Play("Walk");
    }
    
    protected override IEnumerator Die()
    {
        DropWeapon();
        ChangeState(State.DEAD);
        explosion.Play();
        _mover.Reset();
        collidable = false;
        
        // Wait half a sec til sprite becomes invisible
        yield return new WaitForSeconds(0.25f); 
        visible = false;
        
        while(explosion.isPlaying)
        {
            yield return new WaitForEndOfFrame();
        }
        explosion.Stop ();
        explosion.Clear();
        explosion.time = 0;
        
        /*
        if(_weapon != null && _weapon.GetComponent<ProjectileWeapon>())
        {
            ProjectileWeapon wep = _weapon.GetComponent<ProjectileWeapon>();
            while(wep.bulletCount > 0)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        */
        
        SetAsDead();
    }
    
    // Always use instead of setting state directly
    public void ChangeState(State newState)
    {
        if(_currentState == State.DEAD) return; // can't change state if dying
        _currentState = newState;
        
        // Can't shoot if he ain't got no weapon!
        if(_currentState == State.SHOOT && _weapon == null) _currentState = State.IDLE;
        
        //_anim.SetInteger("state", (int)_currentState);
        
        switch(_currentState)
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
    
    private IEnumerator Shoot()
    {
        yield return new WaitForSeconds(0.5f);
        
        // Acually shoot
        if(_currentState != State.DEAD && _weapon != null)
        {
            _weapon.ShootTap();
            _weapon.ShootHold();
            yield return new WaitForSeconds(0.5f);
            ChangeState(State.RUN);
        }
    }
    
}
