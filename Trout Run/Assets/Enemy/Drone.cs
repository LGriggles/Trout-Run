using UnityEngine;
using System.Collections;

public class Drone : Enemy
{
    public override Type EnemyType { get { return Type.DRONE; } }

    // Death and shooting
    public ParticleSystem explosion;

    // Stuff for this guy's specific behaviour
    //private float _distanceToChange = 5; // must be at least this distance to change facingDirection
    //private int _shotFrequency = 240;
    //private float _shotCoolDown = 2; // in seconds
    //private float _coolDownTimer = 0;
    //private bool _canShoot = true;




    // When he done acually be maded
    protected override void Initialize()
    {
        // Define abilities of this enemy type
        _abilities = (EnemyProps.WALKS | EnemyProps.JUMPS | EnemyProps.LADDERS);

        _walkSpeed = 120;
        _jumpForce = 6;

        _maxHealth = 10;
        _ticketAmount = 14;

        _weaponOffset = new Vector2(0.07f, 0.03f);
        _weaponOffset = new Vector2(0.4f, 0.25f);
    }

    protected override void Spawn()
    {
        explosion.time = 0;
        _myRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }



    private IEnumerator IdleDelay()
    {
        yield return new WaitForSeconds(2.0f);
        //ChangeState(State.RUN);
    }



    protected override IEnumerator DeathSequence()
    {
        explosion.Play();
        _mover.Reset();
        collidable = false;

        // Wait half a sec til sprite becomes invisible
        yield return new WaitForSeconds(0.25f);
        visible = false;

        while (explosion.isPlaying)
        {
            yield return new WaitForEndOfFrame();
        }
        explosion.Stop();
        explosion.Clear();
        explosion.time = 0;

        /*
        if(_weapon != null && _weapon.GetComponent<ProjectileWeapon>())
        {
            ProjectileWeapon wep = _weapon.GetComponent<ProjectileWeapon>();
            while(wep.bulletCount > 0)
            {
                yield return new WaitForEndOfFrame();
            }
        }
        */

    }

    












   
}
