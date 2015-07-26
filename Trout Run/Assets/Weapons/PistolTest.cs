using UnityEngine;
using System.Collections;

public class PistolTest : ProjectileWeapon
{
    private Mover _mover; // mover of whoever is holding this gun

    public override WeaponName GetName() { return WeaponName.PISTOL; } // get name as enum (can do .ToString() to get as a string if needed)
    protected override void OnAwake() { }

    // Use this for initialization
    protected override void Init() // called from base class?
	{
		_strength = 4;
        _maxDurability = 20;
        ResetDurability();
		_directions = WeaponDir.EIGHT;
		// Deactivate bullets till weapon picked up
	}


    public override void ApplyKnockback(Mover mover, Vector2 impactDirection)
    {
        mover.AddKnockBackImpact(impactDirection, 100);
    }

    // Use this for whenever picked up
    protected override void OnPickup()
    {
        _mover = GetComponentInParent<Mover>();

    }

    // Virtual function in base class to interface with all weapons (via override)
    public override void ShootTap()
    {
        //if(_owner == Owner.NOBODY) return;
        if (durability == 0) return;
        Vector3 velo;

        // Velocity
        velo = transform.right * (5);

        // Inherit x axis velocity if more than velocity on it's own (reduce weird effect where when you move bullets seem more spaced out)
        if (Mathf.Abs(velo.x + _mover.velocity.x) > Mathf.Abs(velo.x))
            velo.x += _mover.velocity.x;
        // Acually emit the particle
        _bulletSystem.Shoot(_bulletSystem.transform.position, velo, _owner);
        LoseDurability(1);
    }
}
