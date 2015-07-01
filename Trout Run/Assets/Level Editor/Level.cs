// Script on Level Segment allows it to be edited

/* IMPORTANT 1!
   I use ReadPixels() from the tile sheet texture to render tiles in the tile picker
   Unity is FUCKING retarded and forces you to click a box in texture to allow editting which
   apparantly makes a copy of the texture wasting memory EVEN THOUGH I'M ONLY READING PIXELS AND
   NOT WRITING THEM. Fucking ridiculous. Anyway, unless I figure out a better way of doing it it
   means you will have to set texture back to not allowing editing once level is finished.
*/

/* IMPORTANT 2!
   There are a number of issues that affect rendering of textures that are to do with texture import
   settings, not my shader as first thought. 'Filter Mode - Point' ensures blocky textures, no attempt
   at smooth filtering. The 'Format' setting at the bottom (compressed etc) greatly affect how the texture
   appears. Mip Map setting also create weird artifacts when rendered, I simply unchecked 'Generate Mip Maps'
   and these artifacts were eradicated. However a more sophisticated soloution may be better for performance
   reasons later on (e.g. create our own mip maps if texture will be viewed from a variety of distances)
*/


#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct LE_Mode
{
	public const int FACE = 0;
	public const int EDGE = 1;
	public const int BLOCK = 2;

	static public string[] levelLabels { get { return new string[3] { "Face", "Edge", "Block" }; } }
	static public string[] planeLabels { get { return new string[2] { "Face", "Edge" }; } }
}

public class Level : MonoBehaviour 
{
	public List<Face> faces; // list of all faces - so we don't have to recalcualte them every time we click on the mesh
	public float snap = 1.0f;
	
	// Mode and tool - what are we doing to the face?
	public int currentMode = LE_Mode.FACE;

	// Face Tools
	public enum FaceTool { MOVE, EXTRUDE, SPLIT }
	public FaceTool _currentFaceTool = FaceTool.MOVE;
	public bool faceMove { set { if(value) _currentFaceTool = FaceTool.MOVE; } get { if(_currentFaceTool == FaceTool.MOVE && currentMode == LE_Mode.FACE) return true; return false; } }
	public bool faceExtrude { set { if(value) _currentFaceTool = FaceTool.EXTRUDE; } get { if(_currentFaceTool == FaceTool.EXTRUDE && currentMode == LE_Mode.FACE) return true; return false; } }
	public bool faceSplit { set { if(value) _currentFaceTool = FaceTool.SPLIT; } get { if(_currentFaceTool == FaceTool.SPLIT && currentMode == LE_Mode.FACE) return true; return false; } }


	// Edge Tools
	public enum EdgeTool { MOVE, MOVE_LOOP }
	public EdgeTool _currentEdgeTool = EdgeTool.MOVE;
	public bool edgeMove { set { if(value) _currentEdgeTool = EdgeTool.MOVE; } get { if(_currentEdgeTool == EdgeTool.MOVE && currentMode == LE_Mode.EDGE) return true; return false; } }
	public bool edgeLoopMove { set { if(value) _currentEdgeTool = EdgeTool.MOVE_LOOP; } get { if(_currentEdgeTool == EdgeTool.MOVE_LOOP && currentMode == LE_Mode.EDGE) return true; return false; } }




	// Tile
	public int tilesAcross = 2;
	public int tilesDown = 2;
}
#endif