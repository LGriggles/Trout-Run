using UnityEngine;
using System.Collections;

public class Drone : Enemy
{
    // Death and shooting
    public ParticleSystem explosion;
    private float ticketAmount = 14; //For best results, stick to even numbers

    // Stuff for this guy's specific behaviour
    private Vector2 _weaponOffset = new Vector2(0.07f, 0.03f);
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
        _myRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }

    protected override void Spawn()
    {
        explosion.time = 0;
        if (_weapon != null) _weapon.SetPositionOffset(_weaponOffset);
    }



    private IEnumerator IdleDelay()
    {
        yield return new WaitForSeconds(2.0f);
        //ChangeState(State.RUN);
    }

    protected override IEnumerator GetHurt()
    {
        //ChangeState(State.HIT);
        yield return new WaitForSeconds(1.0f);
        //ChangeState(State.RUN);
        _anim.Play("Walk");
    }

    protected override IEnumerator Die()
    {
        DropWeapon();
        float kingOfDreamland = 0;
        for (int i = 0; i < ticketAmount; i++)
        {
            GameObject johnBervage = _ticketManager.SpawnTicket(new Vector2(transform.position.x, transform.position.y));
            if (johnBervage != null)
            {
                //johnBervage.GetComponent<Rigidbody2D>().AddForce(new Vector2 (-((ticketAmount/2) * 100) + (i * 100)+50, Mathf.Abs(-((ticketAmount/2) * 75) + (i * 100)+50)));
                johnBervage.GetComponent<Rigidbody2D>().AddForce(new Vector2(Mathf.Cos(kingOfDreamland), Mathf.Sin(kingOfDreamland)) * 400);
                kingOfDreamland += (360 / ticketAmount) * Mathf.Deg2Rad;
            }
        }
        //ChangeState(State.DEAD);
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

        SetAsDead();
    }

    












   
}
