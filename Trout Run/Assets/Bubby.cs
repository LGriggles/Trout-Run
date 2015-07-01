using UnityEngine;
using System.Collections;

public class Bubby : MonoBehaviour
{
	Vector3 _velocity;
	SphereCollider sphere;

	// Use this for initialization
	void Awake () 
	{
		sphere = GetComponent<SphereCollider>();
	}

	void Update()
	{
		_velocity.x = Input.GetAxis ("Horizontal");
		_velocity.y = Input.GetAxis ("Vertical");
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		RaycastHit hit;// = new RaycastHit();
		if(Physics.SphereCast(transform.position, sphere.radius, _velocity, out hit, _velocity.magnitude))
		{
			if(hit.distance <=  0.02f)//sphere.radius)
			{
				transform.position = hit.point;//_velocity.normalized * (hit.distance - sphere.radius);
				transform.position += hit.normal * (sphere.radius + 0.02f);
			}
			else
				transform.position += _velocity * Time.deltaTime;
		}
		else
			transform.position += _velocity * Time.deltaTime;
	}
}
