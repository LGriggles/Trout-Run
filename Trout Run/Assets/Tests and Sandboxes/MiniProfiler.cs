using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MiniProfiler : MonoBehaviour
{
	bool _isRecording = false;
	static public bool stopped { get { return !Instance._isRecording; } }
	float _timePassed = 0;
	int _framesPassed = 0;

	float _FPS = 0;
	float _avFPS = 0;
	float _highFPS = 0;
	float _lowFPS = 0;

	List<float> _runningFPSs = new List<float>();
	float _runningTimer = 0;

	// Add additional messages and whatnot
	string _msg = ""; // concat of list of messages
	List<string> _msgs = new List<string>();
	const int MAX_MSGS = 6;
	static public void AddMessage(string msg) { if(Instance._msgs.Count < MAX_MSGS) Instance._msgs.Add (msg); }

	//Singleton yo ass off
	static MiniProfiler _instance;
	static MiniProfiler Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType(typeof(MiniProfiler)) as MiniProfiler;
			if (!_instance)
				_instance = new GameObject("Mini Profiler").AddComponent<MiniProfiler>();
			return _instance;
		}
	}

	static public void Start()
	{
		Instance._isRecording = true;
	}

	static public void Stop()
	{
		Instance._isRecording = false;
	}

	static public void Reset()
	{
		Instance._timePassed = 0;
		Instance._framesPassed = 0;

		Instance._FPS = 0;
		Instance._avFPS = 0;
		Instance._highFPS = 0;
		Instance._lowFPS = 0;

		Instance._runningFPSs.Clear();
	}

	void OnGUI()
	{
		GUI.Box(new Rect(0, 0, 160, 120), 
		        "Time Passed " + _timePassed +
		        "\nFrames Passed " + _framesPassed +
		        "\nCurrent FPS " + _FPS +
		        "\nAverage FPS " + _avFPS +
		        "\nHighest FPS " + _highFPS +
		        "\nLowest FPS " + _lowFPS);

		GUI.Box(new Rect(160, 0, 160, 120), _msg);

	}




	void Update()
	{
		if(_isRecording)
		{
			_timePassed += Time.deltaTime;
			_runningTimer += Time.deltaTime;
			_framesPassed += 1;

			// Work out teh fps
			if(_timePassed > 0)
			{
				_FPS = _framesPassed / _timePassed;

				if(_lowFPS == 0) _lowFPS = _FPS;

				if(_FPS > _highFPS) _highFPS = _FPS;
				else if(_FPS < _lowFPS) _lowFPS = _FPS;
			}

			// Work out average
			if(_runningTimer >= 0.2f)
			{
				_runningTimer = 0;
				_runningFPSs.Add (_FPS);
				_avFPS = 0;

				for(int i = 0; i < _runningFPSs.Count; i++)
				{
					_avFPS += _runningFPSs[i];
				}

				_avFPS = _avFPS / _runningFPSs.Count;
			}
		}

		// Update msgs
		_msg = "";
		for(int i = 0; i < _msgs.Count; i++)
		{
			_msg += _msgs[i] + "\n";
		}
		_msgs.Clear();
	}
}
