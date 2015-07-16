using UnityEngine;
using System.Collections;

public class ScrollingText : MonoBehaviour {
    
    private GUIManager _gui;
    private GUIText dialogGUI;
    private GUIText dialogGUI2;
    public string [] dialogLines;
    private bool talking;
    private bool textIsScrolling;
    private int currentLine;
    private string displayText;
    private int startLine;
    
    void Start () {
        _gui = GameObject.Find("GUI").GetComponent<GUIManager>();
        dialogGUI = _gui.dialogGUI;
        dialogGUI2 = _gui.dialogGUI2;
        talking = false;
        _gui.enabled = false;
    }
    
    void OnTriggerStay2D (Collider2D col) {
        if(/*col.gameObject.tag == "Player" &&*/ !talking && Input.GetButtonDown ("Jump")) {
            //playerScript = col.GetComponent(CharacterMotor);
            //yield WaitForSeconds (0.01);
            currentLine = 0;
            StartCoroutine(StartScrolling ());
            Debug.Log ("Joseph Coonsley");
            //playerScript.enabled = false;
        }
    }
    
    void Update () {
        if (talking)
        {
            _gui.enabled = true;
            if (Input.GetButtonDown ("Jump"))
            {
                if (textIsScrolling){
                    if (currentLine == 0){dialogGUI2.text = dialogLines[currentLine];}
                    else {dialogGUI.text = dialogLines[currentLine];}
                    textIsScrolling = false;
                }
                else {
                    //display next line
                    if(currentLine < dialogLines.Length - 1) {
                        if (currentLine > 0) {
                            dialogGUI2.text = dialogGUI.text;
                        }
                        currentLine++;
                        StartCoroutine(StartScrolling());
                    }
                    else {
                        //end conversation
                        currentLine = 0;
                        dialogGUI.text = "";
                        dialogGUI2.text = "";
                        talking = false;
                        //playerScript.enabled = true;
                        _gui.enabled = false;
                    }
                }
            }
        }
    }
    
    private IEnumerator StartScrolling(){
        Debug.Log ("Frederique Von Cucumbre");
        talking = true;
        textIsScrolling = true;
        displayText = "";
        startLine = currentLine;
        for (int i = 0; i < dialogLines[currentLine].Length; i++)
        {
            if (textIsScrolling && currentLine == startLine)
            {
                displayText += dialogLines[currentLine][i];
                if (currentLine == 0){dialogGUI2.text = displayText;}
                else {dialogGUI.text = displayText;}
                yield return new WaitForSeconds (0.03f);
            }
        }
        textIsScrolling = false;
    }
}
