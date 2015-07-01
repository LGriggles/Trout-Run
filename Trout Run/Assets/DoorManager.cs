using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoorManager : MonoBehaviour {

    List<Door> _doors;
    protected static int lastDoor = 0; // What was the DoorID of the last door used?

	// Use this for initialization
	void Start () {
        _doors = new List<Door>();
        GameObject[] doors = GameObject.FindGameObjectsWithTag("Door");
        foreach(GameObject door in doors)
        {
            _doors.Add(door.GetComponent<Door>());
            door.transform.parent = gameObject.transform;
        }
        if (lastDoor != 0){
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach(GameObject player in players)
            {
                for(int i = 0; i < _doors.Count; i++)
                {
                    if (_doors[i].DoorID == lastDoor) 
                    {
                        PlayerController _playa = player.GetComponent<PlayerController>();
                        _playa.reAwaken();
                        player.transform.position = new Vector3 (doors[i].transform.position.x,
                                                                 doors[i].transform.position.y + 2,
                                                                 doors[i].transform.position.z);
                        break;
                    }
                }
            }
        }
	}
	
    public void SetDoorID (int doorID)
    {
        lastDoor = doorID;
    }

	// Update is called once per frame
	void Update () {
	
	}
}
