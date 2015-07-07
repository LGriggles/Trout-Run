using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour 
{
    protected uint _abilities = 0; //!< Set this to define what things this enemy can do
    protected float _walkSpeed;
    protected float _flySpeed;

    // To do with moving, probably will be different when merged with Lee's physics
    Vector2 _moveVector;
    protected Transform _transform;
    protected Rigidbody2D _rigidbody;

    // Getters / Setters
    public uint Abilities { get { return _abilities; } }
    public Vector2 Position { get { return _transform.position; } set { _transform.position = value; } }
    
    
    protected abstract void Initialize(); //!< Override to define enemy specific functions that would normally be in Awake


    void Awake()
    {
        _transform = this.transform;
        _rigidbody = GetComponent<Rigidbody2D>();
        Initialize();
    }


    void FixedUpdate()
    {
        _rigidbody.MovePosition((Vector2)(_transform.position) + (_moveVector * Time.fixedDeltaTime));
        _moveVector = Vector2.zero;
    }


    public bool HasAbilities(uint abilities)
    {
        return (_abilities & abilities) == abilities;
    }




    //------------------------------------------------//
    //----            ABILITY FUNCTIONS           ----//
    //  All are virtual so children can override and  //
    //  change implementation if they need to.        //
    //------------------------------------------------//
    public virtual void Walk(float dir)
    {
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






}












public static class EnemyProps
{
    public const uint WALKS =   0x00000001;
    public const uint JUMPS =   0x00000002;
    public const uint FLYS =    0x00000004;
    public const uint SHIELDS = 0x00000008;
    public const uint LADDERS = 0x00000010;






}