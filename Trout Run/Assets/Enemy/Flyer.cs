using UnityEngine;
using System.Collections;

public class Flyer : Enemy
{
    public override Type EnemyType { get { return Type.FLYER; } }

    protected override void Initialize()
    {
        // Define abilities of this enemy type
        _abilities = (EnemyProps.FLYS);

        _flySpeed = 10;
    }

    protected override void Spawn()
    {

    }

    protected override IEnumerator DeathSequence()
    {
        yield break;
    }
	

   
}
