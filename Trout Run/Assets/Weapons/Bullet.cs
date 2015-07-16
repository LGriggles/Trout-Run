using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    Rigidbody2D _rigidBody;
    BoxCollider2D boxCollider;
    Bullets ownerFiringSystem;
    WeaponHolder _owner;
    Weapon _firedFrom; //The weapon the bullet was fired from

    int _enemyLayer = LayerMask.NameToLayer("Enemy");
    int _playerLayer = LayerMask.NameToLayer("Player");
    int _sceneryLayer = LayerMask.NameToLayer("SolidScenery");

    // Use this for initialization
    void Start()
    {
    }

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void  Shot(Bullets bulletsManager, Vector2 velocity, WeaponHolder owner)
    {
        _owner = owner;
        _firedFrom = _owner.weapon;
        switch (owner.myTeam)
        {
            case WeaponHolder.Team.PLAYER:
                //_targetMask = 1 << LayerMask.NameToLayer("Enemy");
                //print(_targetMask);
                //gameObject.layer = _targetMask;
                break;
            case WeaponHolder.Team.ENEMY:
                ////_targetMask = 1 << LayerMask.NameToLayer("Player");
                //gameObject.layer = _targetMask;
                break;
        }


        ownerFiringSystem = bulletsManager;
        _rigidBody.velocity = velocity;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.layer == _enemyLayer)
        {
            if (_owner.myTeam == WeaponHolder.Team.ENEMY)
                return;
            Enemy enemy = collider.GetComponentInParent<Enemy>();
            if (enemy != null)
            {
                _firedFrom.ApplyKnockback(enemy.mover, _rigidBody.velocity);
                enemy.LoseHealth(_firedFrom.strength);
                ownerFiringSystem.DestroyObject(gameObject);
            }
        }
        else if (collider.gameObject.layer == _sceneryLayer)
        {
            ownerFiringSystem.DestroyObject(gameObject);
        }
        else if (collider.gameObject.layer == _playerLayer)
        {
            PlayerController player = collider.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                _firedFrom.ApplyKnockback(player.mover, _rigidBody.velocity);
                player.GetHit();
                ownerFiringSystem.DestroyObject(gameObject);
            }
        }
    }
}
