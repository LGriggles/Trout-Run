using UnityEngine;
using System.Collections;

public class CollisionTestWall : MonoBehaviour 
{
	int _hit = 0;

	public void ResetHits() { _hit = 0; }

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		MiniProfiler.AddMessage("Wall hit " + _hit + " times");
	}


	void OnParticleCollision(GameObject other)
	{
		//Die ();
		BulletTester bullets = other.GetComponentInParent<BulletTester>();
		if(bullets == null) return;
		bullets.HitEnemy();
	}

	public void Die()
	{
		_hit += 1;
	}

}
