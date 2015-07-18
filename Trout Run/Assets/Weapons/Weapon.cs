using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour
{
    public void DebugPrint()
    {
        Debug.Log("Mein owner is " +_owner.gameObject.name);

    }




    WeaponManager _weaponManager;

    protected int _strength; // damage weapon will do
    public int strength { get { return _strength; } }
    protected int _layEnemy;
    private int _layIgnorePlayer;
    private int _layIgnoreCollisions;
    private int _layIgnoreEntities;
    private GameObject _pickupTrigger;
    private Rigidbody2D _rigid;
    private bool _spinning = false; // for making it spin round crazy when throwing
    private const float _DORMANT_LIFESPAN = 6; // how many secs till disappears when not owned?
    private float _lifetime;
    private bool _lost = false; // once lost it can never be retreived...
    protected Vector2 _weaponDirection;
    public Vector2 curWeaponDir { get { return _weaponDirection; } }
    private bool _collideWithPlatforms = true;
    private Collider2D _collider;

    protected WeaponHolder _owner = null;

    protected Vector2 _offset = Vector2.zero; // the offset from origin of owner to the handle of this weapon - this will be based on where the "hand" of the owner is

    public enum WeaponDir { EIGHT, FOUR, HORIZONTAL }
    protected WeaponDir _directions;
    public WeaponDir possibleDirections { get { return _directions; } }

    // For durability and name
    private int _durability = 100; // made this private as it forces children to use LoseDurability and ResetDurability, which cap durabilty at zero when reducing
    protected int _maxDurability = 100;
    public int durability { get { return _durability; } }
    public int maxDurability { get { return _maxDurability; } }
    protected void ResetDurability() { _durability = _maxDurability; }
    protected void LoseDurability(int ammount)
    {
        int lowest = 0;
        if (_owner != null && _owner.myTeam == WeaponHolder.Team.ENEMY) // if enemy lowest is half durability
            lowest = (int)(_maxDurability * 0.5f);
        _durability = Mathf.Max(lowest, _durability - ammount);
    }
    public void AddDurability(int amount /*with one M, you filthy scrubs*/)
    {
        _durability += amount;
        if (_durability > _maxDurability) _durability = _maxDurability;
    }

    public abstract WeaponName GetName(); // force implementation

    // Overrides for children
    protected virtual void OnAwake() { }
    protected virtual void Init() { }
    protected virtual void OnPickup() { }

    protected virtual void Awake()
    {
        // Inits
        _rigid = GetComponent<Rigidbody2D>();

        // Lifetime
        _lifetime = _DORMANT_LIFESPAN;

        // Weapon manager
        _weaponManager = GameObject.FindGameObjectWithTag("WeaponManager").GetComponent<WeaponManager>();

        // Layers
        _layEnemy = LayerMask.NameToLayer("Enemy");
        _layIgnoreCollisions = LayerMask.NameToLayer("IgnoreCollisions"); // ignore all collisions
        _layIgnoreEntities = LayerMask.NameToLayer("IgnoreEntities"); // collide with scene but ignore entity collisions (for dormant weapons)
        _layIgnorePlayer = LayerMask.NameToLayer("IgnorePlayer"); // ignore player only, for when thrown and just want to hit enemies and the floor

        // Collider
        _collider = GetComponent<Collider2D>();

        // Find Pickup Trigger
        Transform[] children = GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.gameObject.name == "PickupTrigger") _pickupTrigger = child.gameObject;
        }
        if (_pickupTrigger == null) Debug.LogError(name + " has no child called 'PickupTrigger'. Needed to allow player to pick up weapon");

        
        gameObject.layer = _layIgnorePlayer;

        OnAwake();
        Init();
    }

    // Shoot me baby, into your heart
    public virtual void ShootTap() { }
    public virtual void ShootHold() { }
    public virtual void ShootRelease() { }
    public virtual void ApplyKnockback(Mover mover, Vector2 impactDirection) { } // called when bullets hit, melee weapons make contact etc

    public void Pickup(Enemy enemy) { Pickup(enemy, Vector2.zero); }
    public void Pickup(PlayerController player) { Pickup(player, Vector2.zero); }
    public void Pickup(Enemy enemy, Vector2 offset)
    {
        // Set Enemy
        enemy.weapon = this;
        transform.parent = enemy.transform;
        _owner = enemy; //Set the holder of the weapon
        Pickup(offset);
        OnPickup();
    }

    public void Pickup(WeaponHolder weaponHolder, Vector2 offset)
    {
        // Set Enemy
        weaponHolder.weapon = this;
        transform.parent = weaponHolder.transform;
        _owner = weaponHolder; //Set the holder of the weapon
        Pickup(offset);
        OnPickup();
    }

    public void Pickup(PlayerController player, Vector2 offset)
    {
        //player.weapon = this;
        // Set Player
        transform.parent = player.transform;
        //transform.position += new Vector3 (player.facingDirection, 0, 0);
        _owner = player;
        Pickup(offset);
        OnPickup();
    }

    // Called from Pickup(Player) and Pickup(Enemy), e.g. the generic logic for both
    private void Pickup(Vector2 offset)
    {
        _lifetime = _DORMANT_LIFESPAN;
        _lost = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(Vector3.zero);

        gameObject.layer = _layIgnoreCollisions;
        _rigid.isKinematic = true;
        _pickupTrigger.SetActive(false);
        // Set offset and direction
        _offset = offset;
        _spinning = false;
        SetDirection(Vector3.right);
    }

    public void DropMe()
    {
        transform.parent = null;
        _owner = null;
        gameObject.layer = _layIgnorePlayer;
        _rigid.isKinematic = false;
        _pickupTrigger.SetActive(true);
    }

    // Dropping and throwing weapon
    public void Drop() { Throw(Vector2.up * 5, false); }
    public void Throw(Vector2 force) { Throw(force, false); }
    public void Throw(Vector2 force, bool spin)
    {
        transform.parent = null;
        _owner = null;
        gameObject.layer = _layIgnorePlayer;
        _rigid.isKinematic = false;
        _pickupTrigger.SetActive(true);

        _rigid.AddForce(force, ForceMode2D.Impulse);
        _spinning = spin;
    }

    // Like drop and throw except the weapon is lost forever - for when you get hit and lose weapon etc
    public void Lose()
    {
        Throw(Vector2.up * 12, true);
        _lost = true;
        gameObject.layer = _layIgnoreCollisions;
        StartCoroutine(DestroyInTime(3));
    }

    IEnumerator DestroyInTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (_lost == true)
        {
            _weaponManager.DestroyWeapon(this);
        }
    }

    public virtual void SetDirection(Vector2 direction) { }

    public void SetPositionOffset(Vector2 offset)
    {
        _offset = offset;
        SetDirection(Vector2.right);
    }

    public virtual void Step() { }

    void FixedUpdate()
    {
        Step();
        // if belongs to nobody we need to switch layers to handle collisions properly
        if (_owner == null && !_lost)
        {
            // Countdown timer to death
            if (_weaponManager.weaponCount > 4)
            {
                _lifetime -= Time.deltaTime;
                if (_lifetime <= 0) _weaponManager.DestroyWeapon(this);
            }

            // To do with colliding with enemies when thrown but not dormant
            // Probably want to add "collide with platforms only if going down" here as well
            if (gameObject.layer == _layIgnorePlayer) // being thrown
            {
                // Don't hit plats when going up
                if (_collideWithPlatforms && _rigid.velocity.y > 0)
                {
                    LevelController.CollideWithPlatforms(_collider, false);
                    _collideWithPlatforms = false;
                }

                // If not moving switch to non-collidable layer
                if (Mathf.Abs(_rigid.velocity.x) < 0.1f && Mathf.Abs(_rigid.velocity.y) < 0.1f)
                    gameObject.layer = _layIgnoreEntities;
            }
            else if (gameObject.layer == _layIgnoreEntities) // on floor / dormant
            {
                // If moving switch to colliable layer
                if (Mathf.Abs(_rigid.velocity.x) >= 0.1f || Mathf.Abs(_rigid.velocity.y) >= 0.1f)
                    gameObject.layer = _layIgnorePlayer;
            }
            else gameObject.layer = _layIgnorePlayer; // default layer

            // Hit plats if going down or dormant
            if (!_collideWithPlatforms && _rigid.velocity.y <= 0)
            {
                LevelController.CollideWithPlatforms(_collider, true);
                _collideWithPlatforms = true;
            }
        }
    }

    // Specifically for resetting bullets but may be useful for other weapons
    public virtual void Reset()
    {

    }
}
