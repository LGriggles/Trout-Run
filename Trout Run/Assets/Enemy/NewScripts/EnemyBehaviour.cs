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
        if (_myTask == null) return;
        EnemyTask.State taskState = _myTask.DoTask();
        if(taskState != EnemyTask.State.ONGOING)
        {
            _myTask = null;
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







}
