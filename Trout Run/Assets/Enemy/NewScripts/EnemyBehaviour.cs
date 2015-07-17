using UnityEngine;
using System.Collections;

public class EnemyBehaviour : MonoBehaviour 
{
    Enemy _myEnemy;
    EnemyTask _myTask;

    void Awake()
    {
        _myEnemy = GetComponent<Enemy>();

    }

	// Use this for initialization
	void Start ()
    {

	}
	
	// Update is called once per frame
	void Update ()
    {
        switch (_myEnemy.CurrentState)
        {
            case Enemy.EnemyState.OK:
                if (_myTask == null)
                {
                    _myEnemy.DoNothing();
                }
                else
                {
                    EnemyTask.State taskState = _myTask.DoTask();
                    if (taskState != EnemyTask.State.ONGOING)
                    {
                        _myTask = null;
                    }
                }
                
                break;

            case Enemy.EnemyState.HIT:
            case Enemy.EnemyState.DEAD:
                _myTask = null;
                
                break;
        }



        
        

	
	}




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







}
