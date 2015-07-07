using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public sealed class LevelController
{
    //Singleton yo ass off
    static LevelController _instance;
    static LevelController controller // returns the instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new LevelController();
            }
            return _instance;
        }
    }

    // My delicate lovely variables
    List<Collider2D> _playerColliders;
    List<Collider2D> _platformColliders;


    public LevelController()
    {
        // Variable inits
        _playerColliders = new List<Collider2D>();
        _platformColliders = new List<Collider2D>();

        // Obtain list of all colliders in every player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in players)
        {
            foreach(Collider2D col in player.GetComponentsInChildren<Collider2D>())
            {
                _playerColliders.Add(col);
            }
        }

        // Obtain list of all colliders in every platform
        GameObject[] platforms = GameObject.FindGameObjectsWithTag("Platform");
        foreach(GameObject platform in platforms)
        {
            foreach(Collider2D col in platform.GetComponentsInChildren<Collider2D>())
            {
                _platformColliders.Add(col);
            }
        }
    }



    /*
    // Marvelish, now some functions for altering collisions
    static public void CollideWithPlayers(Collider2D other, bool collide)
    {
        foreach(Collider2D col in controller._playerColliders)
        {
            Physics2D.IgnoreCollision(other, col, !collide);
        }
    }

    static public void CollideWithPlatforms(Collider2D other, bool collide)
    {
        foreach(Collider2D col in controller._platformColliders)
        {
            Physics2D.IgnoreCollision(other, col, !collide);
        }
    }
     * */

    /*
    static public void Reset()
    {
        _instance = null;
        _instance = new LevelController();
    }
     * */




}
