using UnityEngine;
using System.Collections;
using System.Collections.Generic;

class KnockBack
{
	float _startTime; // the time in seconds to apply the force. Set to zero to apply single impulse force
	float _time; // current time, we reduce this to zero from start time
	bool _reduceOverTime; // should the force be reduced over time or should it be constant?
	public bool impactKnockback { get { return _startTime == 0; } }
	public bool finished { get { return _time <= 0; } }

	private Vector2 _velocity; // changed this from direction because it's not just direction, it's direction and force (velocity? couldn't think of better word..)
	public Vector2 velocity 
	{ get 
		{ 
			if(!_reduceOverTime || _startTime == 0) return _velocity;
			return _velocity * (_time / _startTime); // velocity scaled by time passed for if you wanna reduce over time
		} 
	}

	public KnockBack (Vector2 direction, float force , float time, bool reduceOverTime)
	{
		direction.Normalize(); // ensure direction is normalized
		_velocity = direction * force * 0.1f; // scale direction by force to create velocity
		_startTime = time;
		_time = time;
		_reduceOverTime = reduceOverTime;
	}

	public void Process()
	{
		_time = Mathf.Max(0, _time - Time.deltaTime); // reduce time
	}
}

public class Mover : MonoBehaviour 
{
	// ALL THESE VARS AT THE TOP ARE FOR TINKERING WITH TO CHANGE HOW THE MOVER FEELS
	// Public vars for tinkering with delightful explanations to help you figure out wtf going on
	public float drag = 45; // determnines how long it will take to reach 0 velocity if Move() if not being called
	public float gravity = 32; // use with jump force (dictated by Jump(jumpForce) called from Player etc) to change how jumps feel, the numbers are arbitrary so just change to random values till feels right
	public float groundAttraction = 2; // value determines additional downward force whilst on ground. If the character jitters when going downhill then increase this value
	public float minUngroundingSpeed = 5.5f; // value determnines how fast you must be moving up or down to be "ungrounded". Minor falls being ignored helps with slopes, lifts etc
	private float _termVel = 14; // terminal velocity (private because 14 feels about right in all cases but feel free to alter)
    Vector2 totalForce;

	// THESE VARS ARE THE MOVER'S USE ONLY, NOT FOR TINKERING!
	Rigidbody2D _rigid; // dependancy...
	//Collider2D _feetCollider;
    //Collider2D _bodyCollider;

	// For moving / setting flags for FixedUpdate
	Vector2 _standardMovement; // For walking left right and shit
	private float _jump = 0; // amount he should jump next fixed frame? (set via Jump(jumpForce))
	private float _jumpExtra = 0; // for short-hopping, faggots

	// Stuff to tell us additional useful info about the mover
	private Vector2 _relativeVelocity = Vector2.zero; // this is always relative to floor, e.g. up is floor normal
	public Vector2 velocity // this is in world space, so will be the one we use for moving the rigidbody
	{ get
		{
			if(!_collidedBottom || _jumped || _floorNormal == Vector2.up)
			{
				return _relativeVelocity;
			}
			else
			{
				Vector2 worldVel = Vector2.zero;
				Vector2 walkRight = Vector3.Cross(_floorNormal, Vector3.forward);
				worldVel += walkRight * _relativeVelocity.x;
				worldVel += _floorNormal * _relativeVelocity.y;
				return worldVel;
			}
		} 
	}

	private bool _jumped = false; // did he just jump?
	public bool isGrounded { get { return _grounded; } } // is he on the ground?
	private bool _grounded = false; 
	public bool onPlatform { get { return _onPlatform; } } // is he on a platform?
	bool _onPlatform = false; 
	//bool _collideWithPlatforms = true; // should we collide with platforms or not?
    bool _fallingThroughPlatform = false; // are we currently falling through a platform due to pressing down and jump?
	bool _continueJump = false;


    public bool IsIgnoringPlatforms()
    {
        return _fallingThroughPlatform;
    }



	// Stuff for knockbacks
	List<KnockBack> _knockBacks = new List<KnockBack> ();

	// Shit to do with collisions
	Vector2 _position { get { return transform.position; } } // position as a vector2 makes math easier
	Vector2 _floorNormal = Vector2.zero; // normal of floor collisions
	bool _collidedTop = false;
	bool _collidedBottom  = false;
	bool _collidedLeft  = false;
	bool _collidedRight  = false;
	bool _collidedSides { get { return _collidedLeft || _collidedRight; } }

	//private int _layFeet;
	//private int _layIgnorePlat;
	private int _layPlatforms;
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~


	// NOW ALL THE INTERFACE FUCNTIONS SO OWNER OF THIS MOVER CAN TELL IT WHAT TO DO AND WHATNOT
	// Add knockback impact adds an instant impact force (e.g. alters velocity instantly)
	public void AddKnockBackImpact(Vector3 direction, float force) { AddKnockBackForce(direction, force, 0, false); }
	public void AddKnockBackImpact(Vector3 direction, float force, float delayInSecs) { AddKnockBackForce(direction, force, delayInSecs, 0, false); }
	
	// Add knockback force adds a constant or gradually reduced force over time
	public void AddKnockBackForce(Vector3 direction, float force, float duration, bool reduceForceGradually) { AddKnockBackForce(direction, force, 0, duration, reduceForceGradually); }
	public void AddKnockBackForce(Vector3 direction, float force, float delayInSecs, float duration, bool reduceForceGradually) { StartCoroutine(AddKnockBack(direction, force, delayInSecs, duration, reduceForceGradually)); }

	// The main knockback function called from all the public knockback functions. Waits for delay (if any) then adds the knockback
	private IEnumerator AddKnockBack(Vector3 direction, float force, float delayInSecs, float duration, bool reduceForceGradually)
	{
		yield return new WaitForSeconds(delayInSecs);
        _knockBacks.Add (new KnockBack(direction.normalized, force, duration, reduceForceGradually));
	}

	// MOVEMENT FUNCTIONS
	public void Move(Vector2 force) { _standardMovement = force; } // Move character]
    public void MoveInstant(Vector2 velocity) { _relativeVelocity = velocity; } // Move character
    public void Jump(float jumpForce) {	_jump = jumpForce; } // Jump
	public void endExtendedJump() { _continueJump = false; }
	public void DropThroughPlatform()
	{
        //CollideWithPlatforms(false);
        _fallingThroughPlatform = true;
	}

    /*
    void CollideWithPlatforms(bool collide)
    {
        LevelController.CollideWithPlatforms(_feetCollider, collide);
        _collideWithPlatforms = collide; // set bool to reflect current state
    }
     * */
	
	// Reset
	public void Reset()
	{
		_knockBacks.Clear ();
		_relativeVelocity = Vector3.zero;
        _layPlatforms = LayerMask.NameToLayer("Platform");
        //LevelController.CollideWithPlatforms(_bodyCollider, false);
        //LevelController.CollideWithPlatforms(_feetCollider, false);
	}
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~




	// THE MAIN EVENT!!!!
	//AWAKE
	void Awake()
	{
		_rigid = GetComponent<Rigidbody2D> ();

        /*
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach(Transform child in children)
        {
            if(child.gameObject.name == "FeetCollider") _feetCollider = child.gameObject.GetComponent<Collider2D>();
            else if(child.gameObject.name == "BodyCollider") _bodyCollider = child.gameObject.GetComponent<Collider2D>();
        }
        if(_feetCollider == null) Debug.LogError(name + " has no child object called 'FeetCollider' with a collider2D in it, it needs it's feet for platforms!");
        if(_bodyCollider == null) Debug.LogError(name + " has no child object called 'BodyCollider' with a collider2D in it, it needs this for main collisions");
         * */


		// Layers - these need to be different if we're an Enemy or Player :/
		//_layFeet = LayerMask.NameToLayer("Feet");
		//_layPlatforms = LayerMask.NameToLayer("Platform");
		//_layIgnorePlat = LayerMask.NameToLayer("Ignore Platforms");
	}

    void Start()
    {
        //LevelController.CollideWithPlatforms(_bodyCollider, false);
        Reset();
    }
	

	// UPDATE
	void Update () 
	{
		//MiniProfiler.AddMessage("Velocity " + velocity.ToString() + "\nRelative Velocity " + _relativeVelocity);
		//MiniProfiler.AddMessage("Top " + _collidedTop + "\nBottom " + _collidedBottom + "\nSides " + _collidedSides);
		//MiniProfiler.AddMessage("Jumped = " + _jumped);
		//MiniProfiler.AddMessage("Grounded = " + _grounded);
		MiniProfiler.AddMessage("On Platform = " + _onPlatform);
        //MiniProfiler.AddMessage("Collide Plats " + _collideWithPlatforms + "\nFalling Through " + _fallingThroughPlatform);
	}
	

	// FIXED UPDATE (teh meat and potatas)
	void FixedUpdate()
	{
		// APPLY VARIOUS FORCE
		totalForce = Vector2.zero; // will be combined force of everything
		totalForce += _standardMovement; // add the standard movement specified with the public Move() function
		totalForce.y -= gravity; // add the force of gravity
		if(_grounded) _relativeVelocity.y -= groundAttraction; // this is an option



		//Handle knockback
		for (int i = _knockBacks.Count-1; i >= 0; i--) // reverse iterate so if we remove things it doesn't go mental
		{
			// Add the force of the knockback
			if(_knockBacks [i].impactKnockback) _relativeVelocity += _knockBacks [i].velocity; // impact so add to velocity
			else totalForce += _knockBacks [i].velocity; // force so add to total force

			// Discard knockback if time is 0
			if (_knockBacks [i].finished) _knockBacks.RemoveAt(i); // remove if time to die
			else _knockBacks [i].Process (); // else process
		}




        if (_fallingThroughPlatform && velocity.y <= -6)
        {
            _fallingThroughPlatform = false;
        }

		// Switch layer so we don't collide with platforms when going up
        /*
        if(velocity.y >= 0 && _collideWithPlatforms) CollideWithPlatforms(false); // if going up turn off collisions with platforms
        else
        {
            if(_fallingThroughPlatform && velocity.y <= -6)
            {
                CollideWithPlatforms(true);
                _fallingThroughPlatform = false;
            }
            else if(velocity.y < 0 && !_collideWithPlatforms && !_fallingThroughPlatform)
            {
                CollideWithPlatforms(true);
            }
        }
         * */



        /*
		if (velocity.y >= 0 || _collideWithPlatforms) // going up
			_feetCollider.layer = _layIgnorePlat;
		else
			_feetCollider.layer = _layFeet;
        */         
		





		// THIS IS WHERE WE ADD THE FORCE!!!!! <can u feel da force?>
		_relativeVelocity += totalForce * Time.deltaTime; // add the total force to velocity
		_relativeVelocity.x *= Mathf.Min(drag * Time.deltaTime, 1); // butters drag for now
		_relativeVelocity.y = Mathf.Clamp (_relativeVelocity.y, -_termVel, _termVel); // Cap velocity.y so it don't go crazy
		
		if(_jump > 0.1f && _grounded && !_jumped)// if jump then jump!
		{ 
            AddKnockBackImpact(new Vector2(0,1),10);
			_relativeVelocity.y = _jump;
			_jumpExtra = _jump/2;
			_grounded = false;
			_continueJump = true;
			StartCoroutine(StartJump());
		}
		else if (!_grounded && _continueJump)
		{
			_relativeVelocity.y += _jumpExtra;
		}
		else
		{
			_continueJump = false;
		}

		// Slope landing correction
		if(Mathf.Abs (_relativeVelocity.x) < 0.2f && totalForce.x == 0)
			StartCoroutine(SlopeResolve());


		// MOVEEEE!
		_rigid.MovePosition(_position + (velocity * Time.deltaTime));


		// COLLISION INFO
		if(_collidedLeft && _relativeVelocity.x < 0) _relativeVelocity.x = 0;
		else if(_collidedRight && _relativeVelocity.x > 0) _relativeVelocity.x = 0;

		if(_collidedTop) _relativeVelocity.y = 0; // hit head
		
		if(_collidedBottom) // hitting floor
		{
			if(velocity.y < 0)
			{
				_relativeVelocity.y = 0;
				_grounded = true;
				_continueJump = true;
				_jumpExtra = 0;
			}
		}
		else if(Mathf.Abs (_relativeVelocity.y) >= minUngroundingSpeed) // in air and faster than 3 to account for slight movement when still "grounded" (lifts / slopes etc)
		{
			_grounded = false;
		}

		
		// RESET ALL FLAGS ETC
		_collidedTop = false;
		_collidedBottom  = false;
		_collidedLeft  = false;
		_collidedRight  = false;
		_onPlatform = false;
		
		_standardMovement = Vector2.zero; // set to zero so if Move() not called next frame it doesn't carry on forever!
		_jump = 0;
		if (_jumpExtra > 24 * Time.deltaTime){
		_jumpExtra = _jumpExtra - (24 * Time.deltaTime);
		}
	}


	// This is the only way I've found for stopping slight "slide" when landing on slopes
	// It is like 99% perfect, still a tiny amount of movement but barely noticeable
	private IEnumerator SlopeResolve()
	{
		Vector2 oldPos = _position;
		yield return new WaitForFixedUpdate();

		Vector2 newPos = _position;
		if(newPos.x != oldPos.x)
		{
			newPos.x = oldPos.x;
			transform.position = newPos;
			//_grounded = false;
		}
	}

	// The _jumped property let's us transform relative velocity the moment we press jump rather than wait for
	// collision detection  to kick in a couple of frames too late
	private IEnumerator StartJump()
	{
		_jumped = true;
		yield return new WaitForSeconds(0.2f);
		_jumped = false;
	}


    //Removes ALL momentum from the mover, including knockbacks
    public void RemoveAllMomentum()
    {
        _knockBacks.Clear();
        _relativeVelocity = Vector2.zero;
        totalForce = Vector2.zero;
    }


	
	void OnCollisionStay2D(Collision2D col)
	{
		// Put this in a coroutine that waits for end of fixed update
		// I think it gives more accurate results because the MovePosition function
		// doesn't come into effect until the end of fixed update, so the collision flags
		// are always 1 frame ahead of what they should be
		StartCoroutine(SetCollisionFlags(col));
	}


	IEnumerator SetCollisionFlags(Collision2D col)
	{
		yield return new WaitForFixedUpdate();

		for(int i = 0; i < col.contacts.Length; i++)
		{
			Vector3 normal = col.contacts[i].normal;
			if(Mathf.Abs (normal.x) >= 0.9f) // hit sides
			{
				if(Mathf.Sign(normal.x) == 1) _collidedLeft = true; // normal pointing right, so you're pushed out right from a LEFT wall
				else _collidedRight = true;
			}
			else // hit top or bottom
			{
				if(Mathf.Sign(normal.y) == 1) // normal pointing up, so you're pushed out up from the floor (BOTTOM)
				{
					_collidedBottom = true;
					_floorNormal = col.contacts[i].normal;
					if(col.gameObject.layer == _layPlatforms) _onPlatform = true;
				} 
				else _collidedTop = true;
			}
		}
	}
}
