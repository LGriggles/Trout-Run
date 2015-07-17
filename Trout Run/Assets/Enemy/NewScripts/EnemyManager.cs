using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour 
{
    List<EnemyBehaviour> _enemyBehavs;
    BoxCollider2D _boundary;



    void Awake()
    {
        _boundary = GetComponent<BoxCollider2D>();
    }

	// Use this for initialization
	void Start ()
    {
        FindContainedEnemyBehavs();

        for (int i = 0; i < _enemyBehavs.Count; ++i)
        {
            _enemyBehavs[i].MoveToPosition(new Vector2(9.3f, 7.1f));
        }
	
	}

    // Update is called once per frame
    void Update() 
    {
        //foreach(
	
	}


    public void EnemySpawned(EnemyBehaviour enemy)
    {
        enemy.ChargeAtPlayer();
    }



    //! Repopulate the enemy behaviour list with all behaviours of all enemies in the game. Slow so only do at start to determine existing enemies (populate dynamically with in-game created enemies)
    void FindAllEnemyBehavs()
    {
        _enemyBehavs = new List<EnemyBehaviour>(FindObjectsOfType<EnemyBehaviour>());
    }

    //! Repopulate the enemy behaviour list with all behaviours of all enemies within boundary. Slow so only do at start to determine existing enemies (populate dynamically with in-game created enemies)
    void FindContainedEnemyBehavs()
    {
        _enemyBehavs = new List<EnemyBehaviour>();
        Collider2D[] colliders = Physics2D.OverlapAreaAll(_boundary.bounds.min, _boundary.bounds.max);


        foreach (Collider2D c in colliders)
        {
            EnemyBehaviour b = c.GetComponent<EnemyBehaviour>();
            if (b != null && !_enemyBehavs.Contains(b))
            {
                _enemyBehavs.Add(b);
            }
        }


    }







}
