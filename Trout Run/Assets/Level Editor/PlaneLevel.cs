// Script on Plane Segment allows it to be edited

#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlaneLevel : MonoBehaviour 
{
	public List<Face> faces; // list of all faces - so we don't have to recalcualte them every time we click on the mesh
	public float snap = 1.0f;
	
	// Mode and tool - what are we doing to the face?
	public int currentMode = LE_Mode.FACE;
	
	// Face Tools
	public enum FaceTool { MOVE, SPLIT }
	public FaceTool _currentFaceTool = FaceTool.MOVE;
	public bool faceMove { set { if(value) _currentFaceTool = FaceTool.MOVE; } get { if(_currentFaceTool == FaceTool.MOVE && currentMode == LE_Mode.FACE) return true; return false; } }
	public bool faceSplit { set { if(value) _currentFaceTool = FaceTool.SPLIT; } get { if(_currentFaceTool == FaceTool.SPLIT && currentMode == LE_Mode.FACE) return true; return false; } }
	
	
	// Edge Tools
	public enum EdgeTool { MOVE, EXTRUDE }
	public EdgeTool _currentEdgeTool = EdgeTool.MOVE;
	public bool edgeMove { set { if(value) _currentEdgeTool = EdgeTool.MOVE; } get { if(_currentEdgeTool == EdgeTool.MOVE && currentMode == LE_Mode.EDGE) return true; return false; } }
	public bool edgeExtrude { set { if(value) _currentEdgeTool = EdgeTool.EXTRUDE; } get { if(_currentEdgeTool == EdgeTool.EXTRUDE && currentMode == LE_Mode.EDGE) return true; return false; } }
	
	
	
	
	// Tile
	public int tilesAcross = 2;
	public int tilesDown = 2;
}
#endif