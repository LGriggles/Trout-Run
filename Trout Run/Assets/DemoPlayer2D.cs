using UnityEngine;
using System.Collections;

public class DemoPlayer2D : MonoBehaviour 
{
	public float speed;
	Vector2 _velocity;
	Vector2 _position { get { return transform.position; } }
	public float gravity = 9;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		MiniProfiler.AddMessage("Velocity " + _velocity + "\nRB Velocity " + GetComponent<Rigidbody2D>().velocity);
		_velocity.x = Input.GetAxis("Horizontal") *  speed;
		//rigidbody2D.AddForce(new Vector2(Input.GetAxis("Horizontal") * speed, 0));
	}

	void FixedUpdate()
	{
		/*
		if(_velocity.y > -14 && !_collidedBottom) _velocity.y -= gravity * Time.fixedDeltaTime;

		if(!_collidedBottom)
			Move (_velocity * Time.fixedDeltaTime);
		else
		{
			Vector2 transformedVelocity = Vector2.zero;
			Vector2 walkRight = Vector3.Cross(colNorm, Vector3.forward);
			transformedVelocity += walkRight * _velocity.x;
			transformedVelocity += colNorm * _velocity.y;
			Move (transformedVelocity * Time.fixedDeltaTime);
		}

		if(_collidedBottom) _velocity.y = 0;
		*/


		Vector2 moveVec = Vector2.zero;
		moveVec.x = _velocity.x;
		moveVec.y = -gravity;

		
		if(!_collidedBottom)
			GetComponent<Rigidbody2D>().AddForce(moveVec);
		else
		{
			Vector2 transformedMoveVec = Vector2.zero;
			Vector2 walkRight = Vector3.Cross(colNorm, Vector3.forward);
			transformedMoveVec += walkRight * moveVec.x;
			transformedMoveVec += colNorm * moveVec.y;
			GetComponent<Rigidbody2D>().AddForce(transformedMoveVec);
		}
		
		//if(_collidedBottom) _velocity.y = 0;


		_collidedTop = false;
		_collidedBottom  = false;
		_collidedLeft  = false;
		_collidedRight  = false;
	}

	void Move(Vector2 movement)
	{
		GetComponent<Rigidbody2D>().MovePosition(_position + movement);
	}



	bool _collidedTop = false;
	bool _collidedBottom  = false;
	bool _collidedLeft  = false;
	bool _collidedRight  = false;
	bool _collidedSides { get { return _collidedLeft || _collidedRight; } }
	Vector2 colNorm = Vector2.zero;
	void OnCollisionStay2D(Collision2D col)
	{
		for(int i = 0; i < col.contacts.Length; i++)
		{
			Vector3 normal = col.contacts[i].normal;
			//MiniProfiler.AddMessage("Contact " + i + ": Normal = " + col.contacts[i].normal);
			if(Mathf.Abs (normal.x) >= 0.9f) // hit sides
			{
				if(Mathf.Sign(normal.x) == 1) _collidedLeft = true; // normal pointing right, so you're pushed out right from a LEFT wall
				else _collidedRight = true;
			}
			else // hit top or bottom
			{
				if(Mathf.Sign(normal.y) == 1) {_collidedBottom = true; colNorm = col.contacts[i].normal; } // normal pointing up, so you're pushed out up from the floor (BOTTOM)
				else _collidedTop = true;
			}
		}
	}
}
