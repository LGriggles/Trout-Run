using UnityEngine;
using System.Collections;

public class Bullets : MonoBehaviour 
{
    WeaponHolder _owner;
	Weapon _myWeapon; // ref to weapon that shoots these bullets
	ParticleSystem _bullets; // ref to particle system
	int _sceneMask; // layer mask for solid scenery
	int _targetMask; // layer mask for thing we want to shoot (player for enemy and enemy for player)
    int _shootableMask; // layer mask for 'shootable' objects that are neither movers nor traditional scenery

	// NOTE: Size of _particles array is dictated by max particles so can save memory by reducing that number in particle system properties
	ParticleSystem.Particle[] _particles; // array to write particles to when modifying them
	public int maxHitsPerBullet = 3; // max number of collisions we bother checking per particle
	Collider2D[] _collisions; // array to write collisions to

	public int bulletCount { get { return _bullets.particleCount; } }
	public void Reset() { _bullets.Stop(); }
	public float startSpeed { get { return _bullets.startSpeed; } }


	void Awake()
	{
		_myWeapon = GetComponentInParent<Weapon>();
		_bullets = this.GetComponent<ParticleSystem>();
		_sceneMask = 1 << LayerMask.NameToLayer("SolidScenery");
        _shootableMask = 1 << LayerMask.NameToLayer("ShootableObject");
	}

	void Start()
	{
		_particles = new ParticleSystem.Particle[_bullets.maxParticles];
		_collisions = new Collider2D[maxHitsPerBullet];
	}

	public void Emit(Vector2 position, Vector2 velocity, WeaponHolder owner)
	{
        _owner = owner;
        switch (_owner.myTeam)
        {
            case WeaponHolder.Team.PLAYER:
                _targetMask = 1 << LayerMask.NameToLayer("Enemy");
                break;
            case WeaponHolder.Team.ENEMY:
                _targetMask = 1 << LayerMask.NameToLayer("Player");
                break;
       }


		// Before emitting a bullet we check to see if the shnozzle of this gun is colliding with anything
		// First, cast against target
        BoxCollider2D weaponCol = _myWeapon.GetComponent<BoxCollider2D>();
        Vector2 topLeft = (Vector2)(weaponCol.bounds.center) - (weaponCol.size * 0.5f);
        Vector2 bottomRight = topLeft + weaponCol.size;

        int count = Physics2D.OverlapAreaNonAlloc(topLeft, bottomRight, _collisions, _targetMask);
        //int count = Physics2D.OverlapPointNonAlloc(position, _collisions, _targetMask);
		
        if(count > maxHitsPerBullet) count  = maxHitsPerBullet;
		bool hitSummat = false;
		
		if(count != 0) // then we hit something
		{
			for(int j = 0; j < count; j++) // for every collision of this particle...
			{
				switch(owner.myTeam)
				{
				case WeaponHolder.Team.PLAYER:
					Enemy enemy = _collisions[j].gameObject.GetComponentInParent<Enemy>();
					enemy.LoseHealth(_myWeapon.strength);
					_myWeapon.ApplyKnockback(enemy.GetComponent<Mover>(), velocity);
					
					break;
				}
				
				hitSummat = true;
			}
		}
		else // try casting against scenery
		{
			count = Physics2D.OverlapPointNonAlloc(position, _collisions, _sceneMask);
			if(count > 0) hitSummat = true;
		}

		// Return straight away if hit something (e.g. shnozzle is over enemy or scenery or whatnot)
        if(hitSummat) return;

		// Acually emit bullet
		_bullets.Emit(position, velocity, _bullets.startSize, _bullets.startLifetime, _bullets.startColor);
	}



	void FixedUpdate()
	{
		bool hitSummat = false;
		int numParts = _bullets.GetParticles(_particles);
		if(numParts == 0) return; // no bullets here!

		// For each bullet, check if it collides with something
		for(int i = 0; i < numParts; i++)
		{
			// First, cast against target
			int count = Physics2D.OverlapPointNonAlloc(_particles[i].position, _collisions, _targetMask);
			if(count > maxHitsPerBullet) count  = maxHitsPerBullet;

			if(count != 0) // then we hit something
			{
				for(int j = 0; j < count; j++) // for every collision of this particle...
				{
					switch(_owner.myTeam)
					{
					case WeaponHolder.Team.PLAYER:
						Enemy enemy = _collisions[j].gameObject.GetComponentInParent<Enemy>();
						enemy.LoseHealth(_myWeapon.strength);
						_myWeapon.ApplyKnockback(enemy.GetComponent<Mover>(), _particles[i].velocity);

						break;

                        case WeaponHolder.Team.ENEMY:
						PlayerController player = _collisions[j].gameObject.GetComponentInParent<PlayerController>();
						if (player.immunity == false)
                            {
                        player.GetHit();
						_myWeapon.ApplyKnockback(player.GetComponent<Mover>(), _particles[i].velocity);
                            }
						break;
					}
					
					hitSummat = true;
					_particles[i].lifetime = 0; // kill particle
				}
			}
            else // try casting against shootableobjects
            {
                count = Physics2D.OverlapPointNonAlloc(_particles[i].position, _collisions, _sceneMask);
                if(count > 0)
                {
                    hitSummat = true;
                    _particles[i].lifetime = 0; // kill particle
                }
                if (hitSummat == false)
                {
                    count = Physics2D.OverlapPointNonAlloc(_particles[i].position, _collisions, _shootableMask);
                    if(count != 0)
                    {
                        for(int j = 0; j < count; j++) // for every collision of this particle...
                        {
                            hitSummat = true;
                            ReflectiveSurface reflet = _collisions[j].gameObject.GetComponentInParent<ReflectiveSurface>();
                            _particles[i] = reflet.Hit(_particles[i], _myWeapon);
                        }
                    }
                }
            }
		}
		
		if(hitSummat)
			_bullets.SetParticles(_particles, numParts);
	}
}
