using UnityEngine;
using System.Collections;

public class Flyer : Enemy
{
    protected override void Initialize()
    {
        // Define abilities of this enemy type
        _abilities = (EnemyProps.FLYS);

        _flySpeed = 10;
    }

    protected override void Spawn()
    {

    }

	

   
}
