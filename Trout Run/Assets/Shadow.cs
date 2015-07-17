using UnityEngine;
using System.Collections;

public class Shadow : MonoBehaviour {

    //Sayonara, Shadow The Hedgehog

    private float distance;
    private RaycastHit2D hit;
    private Projector shadowTheHedgehog;

	void Start () {
        shadowTheHedgehog = GetComponent<Projector>();
	}
	
	// Let's go home, to the planet as COOL AND BLUE AS ME!
	void Update () {
        hit = Physics2D.Raycast(transform.position, -Vector2.up);
        if (hit.collider != null)
        {
            distance = Mathf.Abs(hit.point.y - transform.position.y);
            shadowTheHedgehog.farClipPlane = distance + 4;
        }
	}
}
