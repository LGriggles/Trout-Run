using UnityEngine;
using System.Collections;

public class Fade : MonoBehaviour {

    //public stuff yo
    public float fadeSpeed;
    public bool wait;
    public Color fadeColor;

    private Texture2D _fadeTexture;
    private int _depth;
    private float _alpha;
    private float _dir;
    private Color _tempColors;

	void Start () {
        _fadeTexture = new Texture2D (1,1);
        _fadeTexture.SetPixel (0, 0, fadeColor);
        _fadeTexture.Apply();
        _depth = -1000;
        _alpha = 1;
        _dir = -1;
        FadeIn();
	}
	
    void OnGUI(){
        
        if(!wait){
           _alpha += _dir * fadeSpeed * Time.deltaTime;
        }
        _alpha = Mathf.Clamp01(_alpha);
        _tempColors = GUI.color;
        _tempColors.a = _alpha;
        GUI.color = _tempColors;
        GUI.depth = _depth;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _fadeTexture);
    }

    public void FadeIn(){
        wait = false;
        _dir = -1;  
    }
   
    public void FadeOut(){
        _dir = 1;  
    }
}