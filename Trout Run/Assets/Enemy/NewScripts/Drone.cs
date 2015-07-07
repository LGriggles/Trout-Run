using UnityEngine;
using System.Collections;

public class Drone : Enemy
{
    protected override void Initialize()
    {
        // Define abilities of this enemy type
        _abilities = (EnemyProps.WALKS | EnemyProps.JUMPS | EnemyProps.LADDERS);

        _walkSpeed = 2;
    }





   
}
