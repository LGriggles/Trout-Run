using UnityEngine;
using System.Collections;


// Base class for all blue-haired swordies
public class MeleeWeapon : Weapon 
{
    // TEMP....
    // Stuck this in for now but when this is finished MeleeWeapon should be abstract class and this can be removed
    public override WeaponName GetName() { return WeaponName.NONE;}
    // ...end temp

    Collider2D[] collisionResults = new Collider2D[10];

    GameObject _damageSpace;
    Vector2 _damageArea; //Area that the melee weapon is doing damage to
    Vector2 _damageAreaOffset; //Offsets for the direction in which the weapon is attacking
    bool _attacking = false;
	protected override void Awake()
	{
		base.Awake ();
	}

    protected override void Init()
    {
        base.Init();
    }

	public override void SetDirection(Vector2 direction)
	{
        _weaponDirection.x = _owner.facingDirection;
		transform.localPosition = Vector2.zero;
		float thatsRad = Mathf.Atan2(_weaponDirection.y, _weaponDirection.x);
		float ang = thatsRad * Mathf.Rad2Deg;
		
		transform.rotation = Quaternion.Euler(0, 0, ang);
		transform.localPosition += transform.right * _offset.x;
		transform.localPosition += Vector3.up * _offset.y;
	}

	public override void ShootTap()
	{
		if(Input.GetAxis("Horizontal") > 0.35 || Input.GetAxis("Horizontal") < -0.35)
		{
			SideAttack();
		}
		else if (Input.GetAxis("Vertical") > 0.10)
		{
			UpAttack();
		}
		else if (Input.GetAxis("Vertical") < -0.10)
		{
			DownAttack();
		}
		else if (Input.GetAxis("Horizontal") == 0 && Input.GetAxis("Vertical") == 0)
		{
			NeutralAttack();
		}
	}

	protected virtual void NeutralAttack()
	{
        int size = Physics2D.OverlapCircleNonAlloc(transform.position,2,collisionResults,1 << LayerMask.NameToLayer("Enemy"));
        for(int i = 0; i < size; i++)
        {
            Collider2D colliderIter = collisionResults[i];
            Mover mover = colliderIter.GetComponentInParent<Mover>();
            WeaponHolder holder = colliderIter.GetComponentInParent<WeaponHolder>();
            EnemyOld enemy = colliderIter.GetComponentInParent<EnemyOld>();
            if(mover != null)
            {
                if(holder.myTeam != _owner.myTeam)
                {
                    enemy.LoseHealth(2);
                    mover.RemoveAllMomentum();
                    if(_weaponDirection.x > 0 && _owner.transform.position.x < mover.transform.position.x)
                    {
                        mover.AddKnockBackImpact(
                            new Vector2(0.2f,
                                     1
                                     ),300);
                    }
                    else if(_weaponDirection.x < 0 && _owner.transform.position.x > mover.transform.position.x)
                    {
                        mover.AddKnockBackImpact(
                            new Vector2(-0.2f,
                                    1
                                    ),300);
                    }
                }
            }
        }
		Debug.Log ("NeutralAttack");
	}

	protected virtual void SideAttack()
	{
        int size = Physics2D.OverlapCircleNonAlloc(transform.position,2,collisionResults,1 << LayerMask.NameToLayer("Enemy"));
        for(int i = 0; i < size; i++)
        {
            Collider2D colliderIter = collisionResults[i];
            Mover mover = colliderIter.GetComponentInParent<Mover>();
            WeaponHolder holder = colliderIter.GetComponentInParent<WeaponHolder>();
            EnemyOld enemy = colliderIter.GetComponentInParent<EnemyOld>();
            if(mover != null)
            {
                if(holder.myTeam != _owner.myTeam)
                {
                    enemy.LoseHealth(2);
                    mover.RemoveAllMomentum();
                    if(_weaponDirection.x > 0 && _owner.transform.position.x < mover.transform.position.x)
                    {
                        mover.AddKnockBackImpact(
                            new Vector2(1,
                                    0
                                    ),50);
                    }
                    else if(_weaponDirection.x < 0 && _owner.transform.position.x > mover.transform.position.x)
                    {
                        mover.AddKnockBackImpact(
                            new Vector2(-1,
                                    0
                                    ),50);
                    }
                }
            }
        }
	    Debug.Log ("SideAttack");
	}

	protected virtual void UpAttack()
	{
        int size = Physics2D.OverlapCircleNonAlloc(transform.position,2,collisionResults,1 << LayerMask.NameToLayer("Enemy"));
        for(int i = 0; i < size; i++)
        {
            Collider2D colliderIter = collisionResults[i];
            Mover mover = colliderIter.GetComponentInParent<Mover>();
            WeaponHolder holder = colliderIter.GetComponentInParent<WeaponHolder>();
            EnemyOld enemy = colliderIter.GetComponentInParent<EnemyOld>();
            if(mover != null)
            {
                if(holder.myTeam != _owner.myTeam)
                {
                    enemy.LoseHealth(0);
                    mover.RemoveAllMomentum();
                        mover.AddKnockBackImpact(
                            new Vector2(0,
                                    1
                                    ),300);
                }
            }
        }
		Debug.Log ("UpAttack");
	}
	
	protected virtual void DownAttack()
	{
        int size = Physics2D.OverlapCircleNonAlloc(transform.position,2,collisionResults,1 << LayerMask.NameToLayer("Enemy"));
        for(int i = 0; i < size; i++)
        {
            Collider2D colliderIter = collisionResults[i];
            Mover mover = colliderIter.GetComponentInParent<Mover>();
            WeaponHolder holder = colliderIter.GetComponentInParent<WeaponHolder>();
            EnemyOld enemy = colliderIter.GetComponentInParent<EnemyOld>();
            if(mover != null)
            {
                if(holder.myTeam != _owner.myTeam)
                {
                    enemy.LoseHealth(1);
                    mover.RemoveAllMomentum();
                    if(_weaponDirection.x > 0 && _owner.transform.position.x < mover.transform.position.x)
                    {
                        mover.AddKnockBackImpact(
                            new Vector2(0.2f,
                                    -1
                                    ),300);
                    }
                    else if(_weaponDirection.x < 0 && _owner.transform.position.x > mover.transform.position.x)
                    {
                        mover.AddKnockBackImpact(
                            new Vector2(-0.2f,
                                    -1
                                    ),300);
                    }
                }
            }
        }
		Debug.Log ("DownAttack");
	}

    IEnumerator AttackingDone()
    {
        yield return new WaitForSeconds(1);
        _attacking = false;
    }
}
