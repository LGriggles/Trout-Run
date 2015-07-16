using UnityEngine;
using System.Collections;

public class ChargerShooter : ProjectileWeapon
{
    private Mover _mover; // mover of whoever is holding this gun
    float _currentCharge; //The amount of charge the weapon is currently holding
    float _naturalCharge = 5; //The charge the gun gets to even whilst the charge button isn't held
    float _desiredCharge = 20; //The maximum charge level

    public override WeaponName GetName() { return WeaponName.CHARGE_CANNON;} // get name as enum (can do .ToString() to get as a string if needed)
    protected override void OnAwake(){}
    ParticleSystem _chargingParticles;

    // Use this for initialization
    protected override void Init () // called from base class?
    {
        _maxDurability = 8;
        ResetDurability();
        _directions = WeaponDir.HORIZONTAL;
        ParticleSystem[] children = GetComponentsInChildren<ParticleSystem>();
        foreach(ParticleSystem child in children)
        {
            if(child.gameObject.name == "ChargeParticles") _chargingParticles = child.GetComponent<ParticleSystem>();
        }
        // Deactivate bullets till weapon picked up
        _playerBulletParticles.gameObject.SetActive(false);
        _enemyBulletsParticles.gameObject.SetActive(false);
    }
    
    
    public override void ApplyKnockback (Mover mover,Vector2 impactDirection)
    {
        mover.AddKnockBackImpact(impactDirection, 100);
    }
    
    
    void Update()
    {
    }
    
    // Use this for whenever picked up
    protected override void OnPickup()
    {       
        _mover = GetComponentInParent<Mover>();
        
        if(_owner.myTeam == WeaponHolder.Team.PLAYER)
        {
            _playerBulletParticles.gameObject.SetActive(true);
            _enemyBulletsParticles.gameObject.SetActive(false);
        }
        else if(_owner.myTeam == WeaponHolder.Team.ENEMY)
        {
            _playerBulletParticles.gameObject.SetActive(false);
            _enemyBulletsParticles.gameObject.SetActive(true);
        }
        
        _bullets.Reset ();
    }
    
    public override void ShootHold()
    {
        // if no durability, you can't charge
        if(durability == 0)
        {
            _chargingParticles.Stop();
            return;
        }

        _chargingParticles.startSize = _currentCharge;
        if (!_chargingParticles.isPlaying)
        {
            _chargingParticles.Play();
        }
        if (_currentCharge < _desiredCharge) 
        {
            _currentCharge += 0.5f * Time.deltaTime;
        }
        if (_currentCharge > 1)
        {

            _chargingParticles.startColor= new Color(50,50,50);
        } else
        {
            _chargingParticles.startColor = new Color(0,255,0);
        }
    } 
    
    // Virtual function in base class to interface with all weapons (via override)
    public override void ShootRelease()
    {
        if (_chargingParticles.isPlaying)
        {
            _chargingParticles.Stop();
        }

        // if no durability, you can't shoot
        if(durability == 0) return;

        if (_currentCharge > 1)
        {
            _strength = ((int)_currentCharge * 20);
            Vector3 velo;
        
            // Velocity
            velo = transform.right * (_bullets.startSpeed);
        
            // Inherit x axis velocity if more than velocity on it's own (reduce weird effect where when you move bullets seem more spaced out)
            if (Mathf.Abs(velo.x + _mover.velocity.x) > Mathf.Abs(velo.x))
                velo.x += _mover.velocity.x + (_currentCharge);

            Vector3 reversed = new Vector3();
            reversed = -velo * _currentCharge;
            Mover temp = _owner.GetComponent<Mover>();
            ApplyKnockback(temp,reversed);

            _bullets.GetComponent<ParticleSystem>().startSize = _currentCharge * 0.5f;
            // Acually emit the particle
            _bullets.Emit(_bullets.transform.position, velo, _owner);
            LoseDurability(1);
            //_bullets.Emit(1);
            _currentCharge = 0;
        }
    }
}
