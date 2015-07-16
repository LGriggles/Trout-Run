using UnityEngine;
using System.Collections;

public class DrillShotHead : WeaponHolder
{
    public WeaponHolder _owner;
    int _sceneMask; // layer mask for solid scenery
    int _targetMask; // layer mask for thing we want to shoot (player for enemy and enemy for player)
    int _shootableMask; // layer mask for 'shootable' objects that are neither movers nor traditional scenery
    Collider2D[] _collisionResults = new Collider2D[10];
    bool _collidedTop = false;
    bool _collidedBottom = false;
    bool _collidedLeft = false;
    bool _collidedRight = false;
    Vector2 colNorm = Vector2.zero;
    Vector3 _shootingVelocity = new Vector3();

    enum ShootingState
    {
        NONE,
        SHOOTING,
        RETRACT,
    }
    ShootingState shootingState = ShootingState.NONE;

    public void Awake()
    {
        _sceneMask = 1 << LayerMask.NameToLayer("SolidScenery");
        _shootableMask = 1 << LayerMask.NameToLayer("ShootableObject");
    }

    public void ShootMe()
    {
        transform.localPosition = new Vector3(0, 0, 0);
        _shootingVelocity = new Vector3(1, 0, 0);
        _targetMask = 1 << LayerMask.NameToLayer("Enemy");
        //switch (_owner.myTeam)
        //{
        //    case WeaponHolder.Team.PLAYER:
        //        _targetMask = 1 << LayerMask.NameToLayer("Enemy");
        //        break;
        //    case WeaponHolder.Team.ENEMY:
        //        _targetMask = 1 << LayerMask.NameToLayer("Player");
        //        break;
        //}
        shootingState = ShootingState.SHOOTING;
    }

    // Use this for initialization
    void Start()
    {

    }

    void ShootingUpdate()
    {
        bool hitSummat = false;
        int count = Physics2D.OverlapCircleNonAlloc(transform.position,1, _collisionResults, _targetMask);

        if (count != 0) // then we hit something
        {
            for (int j = 0; j < count; j++) // for every collision of this particle...
            {
                Enemy enemy = _collisionResults[j].gameObject.GetComponentInParent<Enemy>();
                if (enemy.weapon != null)
                {
                    weapon = enemy.weapon;
                    enemy.DropWeapon();
                    weapon.Pickup(this, new Vector2(0, 0));
                }
                //switch (_owner.myTeam)
                //{
                //    case WeaponHolder.Team.PLAYER:
                //        print("collided with enemy");
                //        Enemy enemy = _collisionResults[j].gameObject.GetComponentInParent<Enemy>();
                //        enemy.LoseHealth(1);
                //        break;
                //    case WeaponHolder.Team.ENEMY:
                //        break;
                //}
            }
            hitSummat = true;
            shootingState = ShootingState.RETRACT;
        }
        else // try casting against shootableobjects
        {
            count = Physics2D.OverlapPointNonAlloc(transform.position, _collisionResults, _sceneMask);
            if (count > 0)
            {
                hitSummat = true;
                shootingState = ShootingState.RETRACT;
            }
        }
    }

    void FixedUpdate()
    {
        switch (shootingState)
        {
            case ShootingState.SHOOTING:
                (_owner as PlayerController).LockDirection();
                ShootingUpdate();
                transform.localPosition = transform.localPosition + _shootingVelocity;
                _shootingVelocity.x -= 0.07f;
                if (_shootingVelocity.x < 0.0f)
                {
                    shootingState = ShootingState.RETRACT;
                }
                break;

            case ShootingState.RETRACT:
                transform.localPosition = new Vector3(0, 0, 0);

                (_owner as PlayerController).UnlockDirection();
                if (weapon != null)
                {
                    Weapon tempWep = weapon;
                    DropWeapon();
                    tempWep.Pickup(_owner, new Vector2(0, 0));
                }
                shootingState = ShootingState.NONE;
                break;
            case ShootingState.NONE:

                break;
        }

    }

    // Update is called once per frame
    void Update()
    {

    }


}
