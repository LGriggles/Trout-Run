using UnityEngine;
using System.Collections;


// Base class for all weapons that require particle system bullets or other projectiles
public abstract class ProjectileWeapon : Weapon 
{
    Vector2 _desiredDirection = new Vector2();
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

    public override void Step()
    {
        //_weaponDirection = Vector2.Lerp(_weaponDirection, _desiredDirection, 0.3f);
    }

	public override void SetDirection(Vector2 direction)
	{
        //MiniProfiler.AddMessage("wepdir3" + direction);
        _desiredDirection = direction;
        _mySprite.transform.localScale = new Vector3(1, _owner.facingDirection, 1);
        /*
        if (_owner != null) _mySprite.transform.localScale = new Vector3(1, _owner.facingDirection, 1);
        else
        {
            _mySprite.transform.localScale = new Vector3(1, Mathf.Sign(direction.x), 1);
            return;
        }
         * */

		switch(base.possibleDirections)
		{
		case Weapon.WeaponDir.HORIZONTAL:
            _desiredDirection.y = 0;
			break;

		case Weapon.WeaponDir.FOUR:
            if (Mathf.Abs(_desiredDirection.y) > 0.5f) _desiredDirection.x = 0; // force vertical only
                    else {_desiredDirection.y = 0; _desiredDirection.x = _owner.facingDirection;} // force horizontal only
			
			break;
		case Weapon.WeaponDir.EIGHT:
            // Sensitivity lets us define how much diagonal on analogue stick registers as diagonal (margin of error, don't want to accidentally press diagonal all the time)
            float sens = 0.1f;
            if (Mathf.Abs(_desiredDirection.x) >= sens && Mathf.Abs(_desiredDirection.y) >= sens) // then diagonal
			{
                _desiredDirection.x = 0.7f * Mathf.Sign(_desiredDirection.x);
                _desiredDirection.y = 0.7f * Mathf.Sign(_desiredDirection.y);
			}
			break;
		}
        
        _weaponDirection = Vector2.Lerp(_weaponDirection, _desiredDirection, 0.3f);
        //MiniProfiler.AddMessage("wepdir2" + _weaponDirection);
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
