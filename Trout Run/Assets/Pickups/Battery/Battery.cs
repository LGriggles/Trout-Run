using UnityEngine;
using System.Collections;

public class Battery : Pickup {

    private PlayerController _tempPC; //temp storage for whichever character touched the battery

	// Use this for initialization
	void Start () {
	
	}
	
    void OnEnable () {
        gameObject.GetComponent<Renderer>().enabled = true;
        gameObject.GetComponentInChildren<Collider2D>().enabled = true;
    }

	// Update is called once per frame
    public override void Collected (PlayerController pc) {
        if (pc.weapon != null) {
            _tempPC = pc;
            gameObject.GetComponent<Renderer>().enabled = false;
            gameObject.GetComponentInChildren<Collider2D>().enabled = false;
            StartCoroutine(IncrementDurability());
        }
    }

    IEnumerator IncrementDurability () {
        Debug.Log ("Weenies");
        int _toAdd = _tempPC.weapon.maxDurability / 3;
        for (int i = 0; i < _toAdd; i++)
        {
            yield return new WaitForSeconds (1.0f / _tempPC.weapon.maxDurability);
            _tempPC.weapon.AddDurability(1);
        }
        gameObject.SetActive(false);
    }
}
