using UnityEngine;
using System.Collections;

public class MachineGun : ProjectileWeapon 
{
	private float _freqPlayer = 0.028f; // seconds
	private float _freqEnemy = 0.05f; // seconds
	private bool _shooting = false;
	private Mover _mover; // mover of whoever is holding this gun


    public override WeaponName GetName() { return WeaponName.MACHINE_GUN;} // get name as enum (can do .ToString() to get as a string if needed)
	protected override void OnAwake(){}

	// Use this for initialization
	protected override void Init () // called from base class?
	{
		_strength = 5;
        _maxDurability = 160;
        ResetDurability();
		_directions = WeaponDir.EIGHT;
		// Deactivate bullets till weapon picked up
	}

	public override void ApplyKnockback (Mover mover, Vector2 impactDirection)
	{
		Vector3 direction = new Vector3(mover.transform.position.x - transform.position.x, 0);
		mover.AddKnockBackImpact(impactDirection, 40);
	}

	// Use this for whenever picked up (so if need reference to owner, e.g. Mover in this case)
	protected override void OnPickup()
	{
		_mover = GetComponentInParent<Mover>();
	}

	// Virtual function in base class to interface with all weapons (via override)
	public override void ShootHold()
	{
        if(durability == 0) return;
		// Demonstrates that it is possible to define different behaviours depending on if shot by player or enemy
		// In this case, player shoots a single round as player will hold button to shoot. However, enemy currently
		// only presses "shoot" once, so in that behaviour we make him shoot for a few secs
		if(!_shooting)
		{
            if(_owner.myTeam == WeaponHolder.Team.PLAYER) StartCoroutine(PlayerShootBullets());
            else if(_owner.myTeam == WeaponHolder.Team.ENEMY) StartCoroutine(EnemyShootBullets());
		}
	}
	

	IEnumerator PlayerShootBullets()
	{
		_shooting = true; // so we can't set off coroutine multiple times
		ShootRound(); // Shoot a bullet
		yield return new WaitForSeconds(_freqPlayer); // Wait for frequency (cool down) until allowing another shot
		_shooting = false;
	}

	IEnumerator EnemyShootBullets()
	{
		_shooting = true; // so we can't set off coroutine multiple times
		float t = 0; // time
		while(t < 0.5f) // keep shooting till time is over 0.5 seconds
		{
			ShootRound();  // Shoot a bullet
			t+=_freqEnemy; // by adding freq each time it loops the time will be accurate (because we wait for freqEnemy seconds when we yield)
			yield return new WaitForSeconds(_freqEnemy); // Wait for frequency (cool down) until allowing another shot
		}
		_shooting = false;
	}
	

	// Shoot a single round of bullets. Called from PlayerShootBullets and EnemyShootBullets
	void ShootRound()
	{
		// Sometimes the enemy is still firing when dies (in coroutine) so bullets in null
		// Perhaps this indicates that the logic of "how long to fire for" should be in the enemy?
		// But this means he'd need separate logic for all different types of weapons
        if (_bulletSystem == null) return;

		Vector3 pos;
		Vector3 velo;

		// Position
        pos = _bulletSystem.transform.position;
		pos += new Vector3(0, Random.Range(-0.1f, 0.1f), 0); // randominate the starting pos slightly
		
		// Velocity
        velo = transform.right * (BulletSpeed);

		// Inherit x axis velocity if more than velocity on it's own (reduce weird effect where when you move bullets seem more spaced out)
		if(Mathf.Abs (velo.x + _mover.velocity.x) > Mathf.Abs (velo.x))
			velo.x += _mover.velocity.x;

		// Acually emit the particle
        _bulletSystem.Shoot(pos, velo, _owner);
		//_bullets.Emit(pos, velo, _owner);
        LoseDurability(1);
	}
}
