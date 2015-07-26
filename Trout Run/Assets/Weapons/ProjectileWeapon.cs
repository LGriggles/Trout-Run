using UnityEngine;
using System.Collections;


// Base class for all weapons that require particle system bullets or other projectiles
public abstract class ProjectileWeapon : Weapon 
{
    public Bullets _bulletSystem;
    public float BulletSpeed;
    Vector2 _desiredDirection = new Vector2();
    private SpriteRenderer _mySprite;

    //Bullet
    public GameObject prefabBase;

	protected override void Awake()
	{
        _mySprite = GetComponentInChildren<SpriteRenderer>();
		Transform[] children = GetComponentsInChildren<Transform>();
        _bulletSystem = this.gameObject.AddComponent<Bullets>();
        if (prefabBase != null && _bulletSystem != null)
        {
            _bulletSystem.InstantiatePool(200, prefabBase);
        }
		base.Awake ();
	}

    void Start()
    {

    }

	public override void SetDirection(Vector2 direction)
	{
        _desiredDirection = direction;
        _mySprite.transform.localScale = new Vector3(1, _owner.facingDirection, 1);

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
		transform.localPosition = new Vector2 (_owner.facingDirection / 3, 0.0f);
		float thatsRad = Mathf.Atan2(_weaponDirection.y, _weaponDirection.x);
		float ang = thatsRad * Mathf.Rad2Deg;
		
		transform.rotation = Quaternion.Euler(0, 0, ang);
		transform.localPosition += transform.right * _offset.x;
		transform.localPosition += Vector3.up * _offset.y;
	}

}
