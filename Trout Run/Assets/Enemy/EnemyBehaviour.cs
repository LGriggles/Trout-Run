using UnityEngine;
using System.Collections;

public class EnemyBehaviour 
{
    Enemy _myEnemy;
    EnemyTask _myTask;

    public bool IsAssigned { get { return _myEnemy != null; } }

    public EnemyBehaviour()
    {
    }

    public void Reset(Enemy enemy)
    {
        _myEnemy = enemy;
        _myTask.Reset(enemy);
    }

    public void SetTasks(EnemyTask tasks) // should be a list of tasks but ho hum
    {
        _myTask = tasks;
    }


    


	
	// Update is called from enemy once per frame
    public EnemyTask.State Update()
    {
        if (_myEnemy == null) return EnemyTask.State.ENEMY_DEAD;

        if (_myTask == null)
        {
            _myEnemy.DoNothing();
            return EnemyTask.State.COMPLETE;
        }


        switch (_myEnemy.CurrentState)
        {
            case Enemy.EnemyState.OK: 
                return _myTask.DoTask();

            case Enemy.EnemyState.HIT:
                return EnemyTask.State.FAILED; // this is because may have been knocked off course and need to recalculate paths etc

            case Enemy.EnemyState.DEAD:
                _myEnemy = null;
                return EnemyTask.State.ENEMY_DEAD;
        }

        return EnemyTask.State.ONGOING;
	}



    /*
    // Call once to set the current task to move to x - returns false if impossible
    public bool MoveToPosition(Vector2 target)
    {
        if (_myEnemy.HasAbilities(EnemyProps.FLYS))
        {
            _myTask = new ETFlyTo(_myEnemy, target);
            return true;
        }
        else if (_myEnemy.HasAbilities(EnemyProps.WALKS))
        {
            EnemyPath path = EnemyPathfinder.FindPath(_myEnemy, target);

            if (path == null) return false;

            _myTask = new ETWalkTo(_myEnemy, path, target);
            return true;
        }
        else
        {
            return false;
        }
    }


    // Helo moto
    public bool ChargeAtPlayer()
    {
        _myTask = new ETChargeAtPlayer(_myEnemy, Random.Range(3, 7));
        return true;
    }
    */






}
