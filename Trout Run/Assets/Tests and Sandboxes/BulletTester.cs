using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletTester : MonoBehaviour 
{
	enum State { IDLE, PARTICLES, OBJECTS, CUSTOM };
	State _currentState = State.IDLE;

	public int bulletCount = 100;
	ParticleSystem _particleBullets;

	public SpriteBullet spriteBulletPrefab;
	List<SpriteBullet> _spriteBullets;
	public int _numSprites = 0;

	public CollisionTestWall wall;

	private int enemyLayer;

	public MeinParticles customPart;
	

	void Awake()
	{
		_particleBullets = GetComponentInChildren<ParticleSystem>();
		_spriteBullets = new List<SpriteBullet>();

		GameObject bullets = new GameObject();
		bullets.name = "Bullets";

		for(int i =0; i < bulletCount; i++)
		{
			SpriteBullet newBullet = Object.Instantiate(spriteBulletPrefab) as SpriteBullet;
			newBullet.name = "Bullet_" + i;
			newBullet.transform.parent = bullets.transform;
			newBullet.gameObject.SetActive(false);
			newBullet.SetParent(this);
			_spriteBullets.Add (newBullet);
		}

		enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
	}

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		//CheckCollision();

		switch(_currentState)
		{
		case State.IDLE:
			if(Input.GetKeyDown(KeyCode.P))LaunchParticles();
			else if(Input.GetKeyDown(KeyCode.O))LaunchObjects();
			else if(Input.GetKeyDown(KeyCode.Space)) 
			{
				_currentState = State.CUSTOM;
				ResetTest();
				MiniProfiler.Start ();
			}
			break;


		case State.PARTICLES:
			if(_particleBullets.particleCount == 0) EndTest();
			break;

		case State.OBJECTS:
			if(_numSprites == 0) EndTest ();
			break;

		case State.CUSTOM:
			if(customPart.numParts == 0) EndTest ();
			break;
		}

		if(Input.GetKeyDown(KeyCode.R)) ResetTest();

		MiniProfiler.AddMessage("Particles = " + _particleBullets.particleCount);
		MiniProfiler.AddMessage("Sprites = " + _numSprites);
	}

	void LaunchParticles()
	{
		_currentState = State.PARTICLES;

		// Launch them particles!!
		/*
		for(int i = 0; i < bulletCount; i++)
		{
			_particleBullets.Emit(
		}
		*/
		_particleBullets.Emit(bulletCount);

		////endLaunch

		ResetTest();
		MiniProfiler.Start ();
	}

	void LaunchObjects()
	{
		_currentState = State.OBJECTS;

		// Launch them objects!!
		for(int i = 0; i < _spriteBullets.Count; i++)
		{
			_spriteBullets[i].gameObject.SetActive(true);
			_spriteBullets[i].transform.position = transform.position;
			_numSprites += 1;
		}
		
		
		////endLaunch

		ResetTest ();
		MiniProfiler.Start ();
	}

	void EndTest()
	{
		MiniProfiler.Stop();
		_currentState = State.IDLE;
	}

	void ResetTest()
	{
		wall.ResetHits();
		MiniProfiler.Reset ();
	}



	public void HitEnemy()
	{
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleBullets.particleCount];
		int numParts = _particleBullets.GetParticles(particles);

		for(int i = 0; i < particles.Length; i++)
		{
			Collider[] hit = Physics.OverlapSphere(particles[i].position, particles[i].size * 0.5f, enemyLayer);
			
			if(hit.Length == 0) continue; // next iter if no collision
			
			foreach( Collider col in hit) // hit enemy
			{
				CollisionTestWall wall = col.gameObject.GetComponent<CollisionTestWall>();
				if(wall != null) wall.Die();
			}
			
			particles[i].lifetime = 0; // kill particle
		}

		_particleBullets.SetParticles(particles, numParts);
	}


	// An alternative collision system from within the particle system's script
	// It's butters slow!!
	void CheckCollision()
	{
		bool hitSummat = false;
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleBullets.maxParticles];
		int numParts = _particleBullets.GetParticles(particles);
		
		if(numParts == 0) return;
		
		for(int i = 0; i < numParts; i++)
		{
			Collider[] hit = Physics.OverlapSphere(particles[i].position, particles[i].size * 0.5f, enemyLayer);
			
			if(hit.Length == 0) continue; // next iter if no collision
			
			foreach( Collider col in hit) // hit enemy
			{
				CollisionTestWall wall = col.gameObject.GetComponent<CollisionTestWall>();
				if(wall != null) wall.Die();
			}
			
			particles[i].lifetime = 0; // kill particle
			hitSummat = true;
		}
		
		if(hitSummat)
			_particleBullets.SetParticles(particles, numParts);
	}

	/*
	void OnDrawGizmos()
	{
		if(_particleBullets != null)
		{
			ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_particleBullets.maxParticles];
			int numParts = _particleBullets.GetParticles(particles);
			
			for(int i = 0; i < numParts; i++)
			{
				Gizmos.DrawSphere(particles[i].position, particles[i].size * 0.5f);
			}
		}

	}
	*/


}
