using UnityEngine;
using System.Collections;

public class InputSetter : MonoBehaviour
{

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		/*
		int padID = 0; // 0 = unknown
		int button = -1; // -1 = none / unknown

		// This hideous if else monster is probably only way of doing this in Unity i cri
		if(Input.GetKeyDown(KeyCode.Joystick1Button0)) { padID = 1; button = 0; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button1)) { padID = 1; button = 1; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button2)) { padID = 1; button = 2; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button3)) { padID = 1; button = 3; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button4)) { padID = 1; button = 4; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button5)) { padID = 1; button = 5; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button6)) { padID = 1; button = 6; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button7)) { padID = 1; button = 7; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button8)) { padID = 1; button = 8; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button9)) { padID = 1; button = 9; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button10)) { padID = 1; button = 10; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button11)) { padID = 1; button = 11; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button12)) { padID = 1; button = 12; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button13)) { padID = 1; button = 13; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button14)) { padID = 1; button = 14; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button15)) { padID = 1; button = 15; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button16)) { padID = 1; button = 16; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button17)) { padID = 1; button = 17; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button18)) { padID = 1; button = 18; }
		else if(Input.GetKeyDown(KeyCode.Joystick1Button19)) { padID = 1; button = 19; }
	

		if(padID != 0 && button != -1)
			Debug.Log ("Pressed button " + button + " on joystick " + padID);

		*/


		if(Input.anyKeyDown)
		{
			KeyCode keyPressed;
			for(int i = 0; i < 1000; i++)
			{
				keyPressed = (KeyCode)i;
				// Continue if not valid keycode
				if(keyPressed.ToString() == i.ToString() && i >= 10)
					continue;


				if(Input.GetKeyDown(keyPressed))
					Debug.Log ("Key " + i + " = " + keyPressed.ToString ());
			}
		}


		/*
		KeyCode joyz = KeyCode.Joystick1Button0;
		if(Input.GetKeyDown (joyz))
		{
			Debug.Log ("Button is = " + joyz.ToString() + ", code is " + (int)joyz);
		}
		*/



	}
}
