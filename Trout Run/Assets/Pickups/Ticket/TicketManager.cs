using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TicketManager : MonoBehaviour {

    public int maxTickets; //Maximum amount of tickets that can be in pool at any given time
    public GameObject ticket;
    List<GameObject> tickets;
	// Use this for initialization
	void Start () {
        tickets = new List<GameObject>();
        for(int i = 0; i < maxTickets; i++)
        {
            GameObject obj = (GameObject)Instantiate(ticket);
            obj.transform.parent = gameObject.transform;
            obj.SetActive(false);
            tickets.Add(obj);
        }
	}
	
    public GameObject SpawnTicket(Vector2 spawnPos)
    {
        for(int i = 0; i < tickets.Count; i++)
        {
            if(!tickets[i].activeInHierarchy)
            {
                tickets[i].SetActive(true);
                tickets[i].transform.position = spawnPos;
                return tickets[i];
            }
        }
        return null;
    }
	// Update is called once per frame
	void Update () {
	
	}
}
