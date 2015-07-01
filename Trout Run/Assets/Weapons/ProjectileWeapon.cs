using UnityEngine;
using System.Collections;


// Base class for all weapons that require particle system bullets or other projectiles
public abstract class ProjectileWeapon : Weapon 
{
    private SpriteRenderer _mySprite;
	public int bulletCount { get { return _bullets.bulletCount; } }
	protected Bullets _bullets // the particle system for bullets
	{ get
		{
            if(_owner == null)
                return null;
			switch(_owner.myTeam)
			{
                case WeaponHolder.Team.PLAYER: return _playerBulletParticles;
                case WeaponHolder.Team.ENEMY: return _enemyBulletsParticles;
			default: return null;
			}
		}
	}
	protected Bullets _playerBulletParticles;
	protected Bullets _enemyBulletsParticles;


	protected override void Awake()
	{
        _mySprite = GetComponentInChildren<SpriteRenderer>();
		Transform[] children = GetComponentsInChildren<Transform>();
		foreach(Transform child in children)
		{
			if(child.gameObject.name == "PlayerBullets") _playerBulletParticles = child.GetComponent<Bullets>();
			else if(child.gameObject.name == "EnemyBullets") _enemyBulletsParticles = child.GetComponent<Bullets>();
		}
		if(_playerBulletParticles == null) Debug.LogError (name + " needs a player bullet particle system in a child object called 'PlayerBullets'");
		if(_playerBulletParticles == null) Debug.LogError (name + " needs an enemy bullet particle system in a child object called 'EnemyBullets'");

		base.Awake ();
	}

	public override void SetDirection(Vector2 direction)
	{
		_weaponDirection = direction;
        _mySprite.transform.localScale = new Vector3 (1, _owner.facingDirection, 1);
                if (Input.GetAxis("Vertical") < 0.2 && _owner.mover.isGrounded)
                {
                    _weaponDirection.y = 0;
                    _weaponDirection.x = _owner.facingDirection;
                }
        else {
		switch(base.possibleDirections)
		{
		case Weapon.WeaponDir.TWO:
            //dis shit blank yo
			break;

		case Weapon.WeaponDir.FOUR:
			if(Mathf.Abs (_weaponDirection.y) > 0.5f) _weaponDirection.x = 0; // force vertical only
                    else {_weaponDirection.y = 0; _weaponDirection.x = _owner.facingDirection;} // force horizontal only
			
			break;
		case Weapon.WeaponDir.EIGHT:
            // Sensitivity lets us define how much diagonal on analogue stick registers as diagonal (margin of error, don't want to accidentally press diagonal all the time)
            float sens = 0.1f;
			if(Mathf.Abs(_weaponDirection.x) >= sens && Mathf.Abs (_weaponDirection.y) >= sens) // then diagonal
			{
				_weaponDirection.x = 0.7f * Mathf.Sign(_weaponDirection.x);
				_weaponDirection.y = 0.7f * Mathf.Sign(_weaponDirection.y);
			}
			break;
		}
        }
		
		transform.localPosition = new Vector2 (_owner.facingDirection / 3, 0.0f);
		float thatsRad = Mathf.Atan2(_weaponDirection.y, _weaponDirection.x);
		float ang = thatsRad * Mathf.Rad2Deg;
		
		transform.rotation = Quaternion.Euler(0, 0, ang);
		transform.localPosition += transform.right * _offset.x;
		transform.localPosition += Vector3.up * _offset.y;
	}

	public override void Reset ()
	{
		_playerBulletParticles.Reset();
		_enemyBulletsParticles.Reset();
	}

}
