using UnityEngine;
using System.Collections;

public class ProfilerLauncher : MonoBehaviour 
{
	public bool runProfilerOnStart = false;
	bool profilerRunning = false;

	// Use this for initialization
	void Start () 
	{
		if(runProfilerOnStart) 
		{
			MiniProfiler.Start();
			profilerRunning = true;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(profilerRunning)
		{
			if(Input.GetKeyDown(KeyCode.R)) MiniProfiler.Reset ();
			if(Input.GetKeyDown(KeyCode.Return)) 
			{
				if(MiniProfiler.stopped)
					MiniProfiler.Start();
				else
					MiniProfiler.Stop ();
			}
		}
	
	}
}
