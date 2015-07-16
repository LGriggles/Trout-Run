using UnityEngine;
using System.Collections;

public class Door : MonoBehaviour {

    private bool _locked = false; // Can this door/warptile be used at the current point in time??
    public int DoorID; // Used as a reference point - Door 1 in this scene would lead to Door 1 in another scene, for example
    public string SceneName; // What Scene does this door take you to?
    GameObject _HUD;
    Fade _fade;

	// Use this for initialization
	void Start () {
        _HUD = GameObject.FindGameObjectWithTag("HUD");
        _fade = _HUD.GetComponent<Fade>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void ToggleLock (){ // Switch door from locked to unlocked and vice-versa
        _locked = !_locked;
    }

    public void Enter (){
        if (!_locked){
        DoorManager _manager = GetComponentInParent<DoorManager>();
        _manager.SetDoorID(DoorID);
        _fade.FadeOut();
        StartCoroutine(SwitchScene());
        }
    }

    private IEnumerator SwitchScene(){
        yield return new WaitForSeconds (0.9f);
        Application.LoadLevel(SceneName);
    }
}
