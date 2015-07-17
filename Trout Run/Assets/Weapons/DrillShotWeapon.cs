using UnityEngine;
using System.Collections;

public class DrillShotWeapon : ProjectileWeapon
{
    public override WeaponName GetName() { return WeaponName.NONE; }
    Collider2D[] _collisionResults = new Collider2D[10];

    public GameObject _drillShotHead;
    GameObject _damageSpace;
    Vector2 _damageArea; //Area that the melee weapon is doing damage to
    Vector2 _damageAreaOffset; //Offsets for the direction in which the weapon is attacking
    bool _attacking = false;

    protected override void Init()
    {
        _directions = WeaponDir.EIGHT;
    }

    public override void ShootTap()
    {
        DrillShotHead drillShotHead = _drillShotHead.GetComponent<DrillShotHead>();
        drillShotHead._owner = _owner;
        drillShotHead.ShootMe();
    }

    IEnumerator AttackingDone()
    {
        yield return new WaitForSeconds(1);
        _attacking = false;
    }
}
