#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[ExecuteInEditMode]
[CustomEditor(typeof(PlaneLevel))]
public class PlaneEditor : Editor
{
	PlaneLevel _level;
	Mesh _mesh;
	MeshCollider _collider;
	
	//Face[] _faces; // Array of face - each face represents 2 tris that form a square.
	Face _currentFace;
	int _faceCount { get { return _mesh.triangles.Length / 6; } } // will give you number of square (6 indices in 2 tris, or 1 square)
	Edge _currentEdge { get { if(_currentFace==null) return null; return _currentFace.edges[_currentEdgeIndex]; } }
	int _currentEdgeIndex = 0; // top, right, bottom or left? (0-3 respectively)
	
	
	bool _faceSelected = false;
	bool _retainFocus = true; // should Unity keep focussing on this object if you click off it, e.g. click on a handle
	Vector3 _normalMask { get { return new Vector3(Mathf.Abs(_currentFace.normal.x), Mathf.Abs(_currentFace.normal.y), Mathf.Abs(_currentFace.normal.z)); } } // return abs normal, for use with scaling vector to match normal



	Tool _lastTool; // use this to monitor last tool
	bool _toolsOverriden = false; // are we currently overriding tools?
	
	bool _extruding = false; // so we can ensure new verts are created only when the extrude first starts
	
	// Edges
	bool _insertEdgeHoriz = true; // should new edges be horiz or vert?
	Edge _splitEdge = new Edge(); // the edge that will be created if you decide to split
	Vector3 _splitEdgePOI = Vector3.zero; // the point in 3D space clicked when split edge was last calculated
	Vector3 _splitEdgeRayDir = Vector3.zero; // the direction of the ray cast to determine POI
	
	// Tiles
	float _tileX = 0.5f; // how much of texture is one tile as a decimal (horiozntal)
	float _tileY = 0.5f; // how much of texture is one tile as a decimal (vertical)
	int _currentTile = 0; // current selected tile
	GUIContent[] tileIcons; // GUIContent each with an image of one tile
	bool showTiles = false; // Should tile picker be shown?
	
	// Menu Items - For creating new level segments and whatnot
	[MenuItem("Level Editor/New Plane")]
	static public void CreateLevel()
	{
		// Create new Level
		GameObject newLevel = new GameObject();
		newLevel.name = "Plane Segment";
		newLevel.AddComponent<PlaneLevel>();
		Selection.activeGameObject = newLevel;
		// note will generate other components when first selected...
	}
	
	void OnEnable()
	{
		// Note that non static vars are uninitialized whenever the editor is deselected.
		// That's why I've taken out "if(not init...)" because always null values when this function called. Will have to use static for undos etc later
		_level = (PlaneLevel)target;
		_mesh = GetMesh();
		_collider = _level.GetComponent<MeshCollider>();
		_lastTool = Tools.current;
		
		if(_level.faces == null) CalculateFaces(); // calc them faces if never been done before
		InitTiles();
	}
	
	void OnUndoRedo()
	{
		CalculateFaces(); // because we have no idea of knowing how many changed
		UpdateMeshCollider(); // because Unity is a bit thick
	}
	
	
	
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	// THIS IS ALL THE VISUAL SHIT IN INSPECTOR
	public override void OnInspectorGUI()
	{
		if (Event.current.commandName == "UndoRedoPerformed") { OnUndoRedo(); } // UNDO!
		
		if(tileIcons == null) return; // not ready yet!
		// GENERAL OPTIONS
		// TILES
		// Current Tile / Face Tile
		EditorGUILayout.BeginHorizontal(GUILayout.Width (240), GUILayout.Width (88));
		GUIStyle tileLabStyle = new GUIStyle(EditorStyles.boldLabel);
		tileLabStyle.padding.top = 32;
		
		// Show current tile
		EditorGUILayout.LabelField("Current Tile", tileLabStyle, GUILayout.Width (85), GUILayout.Height (58)); // Label saying "Current Tile" or whatever
		if(GUILayout.Button(tileIcons[_currentTile], GUILayout.Width(48), GUILayout.Height (48))) // Icon of current tile
			showTiles = !showTiles;
		EditorGUILayout.EndHorizontal();
		
		
		// Tile Picker - pick a tile!
		if(showTiles) // if yes, here is tile logic!
		{
			for(int i = 0; i < tileIcons.Length; i+=5)
			{
				EditorGUILayout.BeginHorizontal(GUILayout.Width (240));
				int rowLength = Mathf.Min (tileIcons.Length - i, 5);
				for(int j = 0; j < rowLength; j++)
				{
					// Button for each tile in row
					int t = i+j; // tile index
					if(GUILayout.Button(tileIcons[t], GUILayout.Width (42), GUILayout.Height (42)))
					{
						_currentTile = t; // set current tile
						if(_faceSelected) _currentFace.tileOffset = tileToOffset(t); // set face tile (if appropriate)
						showTiles = false;
						break;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		
		// Specify how many tiles across and down in tile sheet
		GUILayout.BeginVertical();
		int ta = EditorGUILayout.IntField("TilesX", _level.tilesAcross);
		int td = EditorGUILayout.IntField("TilesY", _level.tilesDown);
		UpdateTiles(ta, td);
		GUILayout.EndVertical();
		
		// What to snap to - probably should always be 1 as 1 Unity metre = 1 tile
		_level.snap = EditorGUILayout.FloatField("Snap", _level.snap);
		
		// Reset mesh and recalc faces (only if it starts freaking out - will take potentially a LONG time to recalculate!)
		GUILayout.BeginHorizontal();
		if(GUILayout.Button("Reset Mesh"))
		{
			//ResetMesh();
			if(EditorUtility.DisplayDialog("Reset Mesh?", "You can undo after this but it sometimes flips out, if it does then modify mesh (e.g. extrude) then undo again!", "Reset", "Cancel"))
				ResetMesh();
		}
		if(GUILayout.Button("Recalc Faces")) CalculateFaces();
		GUILayout.EndHorizontal();
		
		// Drop down menu fore selecting face, edge etc
		_level.currentMode = EditorGUILayout.Popup(_level.currentMode, LE_Mode.planeLabels);
		
		// MODE SPECIFIC OPTIONS
		if(_faceSelected)
		{
			GUIStyle _styBox = new GUIStyle(); // style box
			_styBox.normal.background = ColourTex(new Color(0.5f, 0.5f, 0.5f, 0.2f));
			_styBox.margin = new RectOffset(0, 0, 5, 5);
			
			
			// Begin Area
			GUILayout.BeginVertical(_styBox);
			
			// Label
			EditorStyles.boldLabel.padding.top = 0;
			GUILayout.Label (LE_Mode.planeLabels[_level.currentMode] + " Tools", EditorStyles.boldLabel);
			
			// TOOLS SPECIFIC TO MODE
			switch(_level.currentMode)
			{
			case LE_Mode.FACE:
				// Flip UVs
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("Flip U")) { FlipHoriUVs(); }
				if(GUILayout.Button("Flip V")) { FlipVertUVs(); }
				GUILayout.EndHorizontal();
				
				// Flip Origins
				GUILayout.BeginHorizontal();
				if(GUILayout.Button("Flip U Origin")) { FlipOriginHoriUVs(); }
				if(GUILayout.Button("Flip V Origin")) { FlipOriginVertUVs(); }
				GUILayout.EndHorizontal();
				
				// Tool
				_level.faceMove = EditorGUILayout.Toggle ("Move", _level.faceMove);
				_level.faceSplit = EditorGUILayout.Toggle ("Split", _level.faceSplit);
				
				if(_level.faceSplit)
				{
					GUILayout.BeginHorizontal();
					_insertEdgeHoriz = EditorGUILayout.Toggle ("Split Horiz", _insertEdgeHoriz);
					_insertEdgeHoriz = !EditorGUILayout.Toggle ("Split Vert", !_insertEdgeHoriz);
					GUILayout.EndHorizontal();
				}
				
				break; // end face tools
				
			case LE_Mode.EDGE:
				// Tool
				_level.edgeMove = EditorGUILayout.Toggle ("Move", _level.edgeMove);
				_level.edgeExtrude = EditorGUILayout.Toggle ("Extrude", _level.edgeExtrude);
				
				break; // end edge tools
			}
			
			// End Area
			GUILayout.EndVertical();
		}
		else
		{
			GUILayout.Label ("Ctrl Click to select a " + LE_Mode.planeLabels[_level.currentMode].ToLower());
		}
		
		if (GUI.changed)
		{
			EditorUtility.SetDirty (target);
			CalcSplitEdge(Vector3.zero);
		}
	}
	//<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
	
	
	//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
	// THIS IS ALL THE VISUAL SHIT IN INSPECTOR
	void OnSceneGUI()
	{
		if (Event.current.commandName == "UndoRedoPerformed") { OnUndoRedo(); } // UNDO!
		
		// MOUSE UP EVENT - ALL "ON RELEASE OF HANDLE" etc LOGIC HERE!!!!
		if(Event.current.type == EventType.MouseUp) // has to be at top for some reason..
		{
			_extruding = false;
			CalcAllUvs();
		}
		//-------------
		
		
		
		
		// FACE SELECTED..... ALL MANIPULATION LOGIC HERE!
		if(_faceSelected)
		{
			// Ensure default tool is turned off
			if(Tools.current != Tool.None)
			{
				_lastTool = Tools.current;
				Tools.current = Tool.None;
				_toolsOverriden = true;
			}
			
			// Get values in global coordinates rather than local
			Vector3[] globalVerts; // verts
			Vector3 globalCentre = Vector2.zero; // centre of verts
			
			// How this will look depends on mode
			switch(_level.currentMode)
			{
			case LE_Mode.FACE:
				globalVerts	= new Vector3[4];
				globalVerts[0] = _level.transform.TransformPoint(_currentFace.Vert(Face.TP_LFT));
				globalVerts[1] = _level.transform.TransformPoint(_currentFace.Vert(Face.TP_RGT));
				globalVerts[2] = _level.transform.TransformPoint(_currentFace.Vert(Face.BT_RGT));
				globalVerts[3] = _level.transform.TransformPoint(_currentFace.Vert(Face.BT_LFT));
				globalCentre = _level.transform.TransformPoint(_currentFace.centre);
				
				// Visual representation of face
				Handles.DrawSolidRectangleWithOutline(globalVerts, new Color(0.5f, 0.5f, 0.5f, 0.3f), Color.black);
				
				break;
				
			case LE_Mode.EDGE:
				globalVerts = new Vector3[2];
				globalVerts[0] = _level.transform.TransformPoint(_currentEdge.verts[0]);
				globalVerts[1] = _level.transform.TransformPoint(_currentEdge.verts[1]);
				globalCentre = globalVerts[0] + (Vector3.Normalize(globalVerts[1] - globalVerts[0]) * (Vector3.Distance(globalVerts[0], globalVerts[1]) * 0.5f));
				
				// Visual representation of edges
				Handles.color = new Color(0.8f, 0, 0.5f);
				Handles.DrawAAPolyLine(8, globalVerts);
				
				break;
			}
			
			// MOVE MANIPULATION TOOL - MOVE OR EXTRUDE GEOM
			Vector3 vertOffset = Vector3.zero;
			Vector3 offsetVector = Vector3.zero;
			if(_level.faceMove || _level.edgeMove)
			{
				// Sliders, that's how we roll here at Wonga(tm)
				Handles.color = Color.red;
				Vector3 xSlider = Handles.Slider(globalCentre, Mathf.Sign (_currentFace.normal.x) *  _level.transform.right, HandleUtility.GetHandleSize(globalCentre) * 0.8f, Handles.ArrowCap, 1);
				Handles.color = Color.green;
				Vector3 ySlider = Handles.Slider(globalCentre, Mathf.Sign (_currentFace.normal.y) * _level.transform.up, HandleUtility.GetHandleSize(globalCentre) * 0.8f, Handles.ArrowCap, 1);
				Handles.color = Color.blue;
				Vector3 zSlider = Handles.Slider(globalCentre, Mathf.Sign (_currentFace.normal.z) * _level.transform.forward, HandleUtility.GetHandleSize(globalCentre) * 0.8f, Handles.ArrowCap, 1);


				// Work out offset by adding offset of each slider
				xSlider = _level.transform.InverseTransformDirection(xSlider - globalCentre);
				ySlider = _level.transform.InverseTransformDirection(ySlider - globalCentre);
				zSlider = _level.transform.InverseTransformDirection(zSlider - globalCentre);
				offsetVector = xSlider + ySlider + zSlider;

				// If that mag is mag enough then do some lovely things and stuff
				float mag = offsetVector.magnitude;
				if(mag >= _level.snap)
				{
					Undo.RecordObject(_mesh, "Edit Level");
					MoveSelectedVerts(offsetVector);
				}
				
			}
			
			// EXTRUDING!
			if(_level.edgeExtrude)
			{
				// Sliders, that's how we roll here at Wonga(tm)
				Handles.color = new Color(1, 0.6f, 0.05f);
				vertOffset = Handles.Slider(globalCentre, Edge.GetEdgeNormal(_currentFace.normal, _currentEdgeIndex, _level.transform), HandleUtility.GetHandleSize(globalCentre) * 0.8f, Handles.ArrowCap, 1);
				offsetVector = _level.transform.InverseTransformDirection(vertOffset - globalCentre);



				float mag = offsetVector.magnitude;
				if(mag >= _level.snap)
				{
					Undo.RecordObject(_mesh, "Edit Level");
					
					if(!_extruding)
					{
						BeginExtrude(offsetVector); // duplicates selected verts and then moves them by offset
						_extruding = true;
					}
					else
						MoveSelectedVerts(offsetVector);
					
				}
			}
			
			// SPLIT AND EDGE LOOP INSERTION
			if(_level.faceSplit)
			{
				Vector3[] globSplits = new Vector3[2];
				globSplits[0] = _level.transform.TransformPoint(_splitEdge.verts[0]);
				globSplits[1] = _level.transform.TransformPoint(_splitEdge.verts[1]);
				Handles.DrawAAPolyLine (12, globSplits);
				
				if(Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
				{
					// First of all, work out the ray needed to get opposite side of face
					_currentFace.SplitFace(_splitEdge, _insertEdgeHoriz);
					float offset = _level.snap * 0.2f;
					Vector3 backOrigin = _splitEdgePOI + (_splitEdgeRayDir * offset);
					Vector3 backDirection = -_splitEdgeRayDir;

					CalculateFaces();
					CalcAllUvs();
					UpdateMeshCollider();
					_faceSelected = false;

					SelectFaceByRay(backOrigin, backDirection, 1.0f);
					_currentFace.SplitFace(_splitEdge, _insertEdgeHoriz);

					CalculateFaces();
					CalcAllUvs();
					UpdateMeshCollider();
					_faceSelected = false;

					Event.current.Use();
				}
			}
			
			
			
		}
		else // FACE NOT SELECTED!!!
		{
			// Reset tool
			if(_toolsOverriden)
			{
				Tools.current = _lastTool;
				_toolsOverriden = false;
			}
		}
		//----------------------------------
		
		// SELECTING A FACE
		if(Event.current.control) // use control for all editing
		{
			// Mouse events
			if(Event.current.type == EventType.MouseDown)
			{
				switch(Event.current.button)
				{
				case 0:
					Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
					SelectFaceByRay(mouseRay.origin, mouseRay.direction, Mathf.Infinity);
					
					break; // end mouse down events
				}
			}
		}
		//-----------------------------------
		
		
		
		// UNSELECTING A FACE
		if(_faceSelected)
		{
			if(Event.current.type == EventType.MouseDown && !Event.current.alt)
			{
				switch(Event.current.button)
				{
				case 0: // Basically if clicked on other object or nothing at all then unselect
					Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
					RaycastHit hit;
					if (Physics.Raycast(mouseRay.origin, mouseRay.direction, out hit, Mathf.Infinity))
					{
						if(!hit.collider.gameObject == _level.gameObject) _faceSelected = false; 
					}
					else _faceSelected = false; 
					break;
				}
			}
		}
		//-----------------------------
		
		
		// CLEAR UP!
		if(_retainFocus) // only do this if trying to actually edit otherwise you can never select anything!!
			Selection.activeTransform = _level.transform; // loses focus when click in scene without this, may cause problems laster.. idk
		
		// DOESNT WORK
		_retainFocus = _faceSelected; // must be done before unselect as we still want to retain this once after face unselected
		
		
		if (GUI.changed)
			EditorUtility.SetDirty (target);
	}
	//<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
	//--phew!


	void SelectFaceByRay(Vector3 rayStart, Vector3 rayDirection, float rayDistance)
	{
		RaycastHit hit;
		int triIndex;
		
		// Raycast till we hit something
		if (Physics.Raycast(rayStart, rayDirection, out hit, rayDistance))
		{
			// Unselect if not level
			if(!hit.collider.gameObject == _level.gameObject) _faceSelected = false; 
			
			// The three verts of this tri
			triIndex = hit.triangleIndex;
			_splitEdgePOI = hit.point;
			_splitEdgeRayDir = rayDirection;
			
			// Set face and edge
			CalculateCurrentFaceFromTri(triIndex); // set current face
			_currentEdgeIndex = _currentFace.NearestEdge(_level.transform.InverseTransformPoint(_splitEdgePOI)); // nearest edge to click
			
			// Set split edge if in split mode
			if(_level.faceSplit) CalcSplitEdge(_splitEdgePOI);
			
			// And all that other stuff...
			_faceSelected = true;
			_currentTile = offsetToTile(_currentFace.tileOffset); // set current tile to this face's tile
			EditorUtility.SetDirty (target); // this updates GUI to match "face selected" menu items
			Event.current.Use();
		}
		else // Unselect
		{
			_faceSelected = false;
		}
	}
	
	
	// Calc splitable edge on current face that is nearest to point
	void CalcSplitEdge(Vector3 point)
	{
		if(_currentEdge == null ) return;
		
		// Transform point to local space as edges are all in local space
		point = _level.transform.InverseTransformPoint(point);
		
		// Work out direction across top and bottom edges
		// Edge A = Top or Left
		// Edge B = Bottom or Right
		Vector3 edgeDirA;
		Vector3 edgeDirB;
		float edgeMagA;
		float edgeMagB;
		
		if(_insertEdgeHoriz)
		{
			edgeDirA = _currentFace.Vert(Face.BT_LFT) - _currentFace.Vert(Face.TP_LFT);
			edgeDirB = _currentFace.Vert(Face.BT_RGT) - _currentFace.Vert(Face.TP_RGT);
		}
		else
		{
			edgeDirA = _currentFace.Vert(Face.TP_RGT) - _currentFace.Vert(Face.TP_LFT);
			edgeDirB = _currentFace.Vert(Face.BT_RGT) - _currentFace.Vert(Face.BT_LFT);
		}
		
		edgeMagA = edgeDirA.magnitude;
		edgeMagB = edgeDirB.magnitude;
		
		edgeDirA = Vector3.Normalize(edgeDirA);
		edgeDirB = Vector3.Normalize(edgeDirB);
		
		int numDivs = Mathf.FloorToInt(Mathf.Min (edgeMagA, edgeMagB) / _level.snap); // number of possible divisions
		float minDist = 0; // minimum dist to a prospective edge
		
		Vector3 edgeBStartVert = _currentFace.Vert (Face.BT_LFT);
		if(_insertEdgeHoriz) edgeBStartVert = _currentFace.Vert (Face.TP_RGT);
		
		for(int i = 0; i < numDivs; i++) // for each division
		{
			float edgeOffset = (i+1) * _level.snap;
			Vector3 vertA = _currentFace.Vert (Face.TP_LFT) + (edgeDirA * edgeOffset);
			Vector3 vertB = edgeBStartVert + (edgeDirB * edgeOffset);
			
			float dist = Edge.DistanceToEdge(vertA, vertB, point);
			if(i==0 || dist < minDist)
			{
				minDist = dist;
				_splitEdge.Set(vertA, vertB);
			}
		}
	}
	
	
	
	//------------------------------------------------------------TILE STUFF!
	// _tileY Functions
	// Update tiles once new values have been picked (ensures UVs and icons update and that tiles no less than 1X1)
	void InitTiles() { UpdateTiles(_level.tilesAcross, _level.tilesDown, true); }
	void UpdateTiles(int tilesAcross, int tilesDown, bool init = false)
	{
		if(!init && tilesAcross == _level.tilesAcross && tilesDown == _level.tilesDown) return;
		
		_level.tilesAcross = tilesAcross;
		_level.tilesDown = tilesDown;
		_tileX = (float)1/_level.tilesAcross;
		_tileY = (float)1/_level.tilesDown;
		GetMaterial().mainTextureScale = new Vector2(_tileX, _tileY);
		CalcAllUvs();
		
		
		// Now set tile icons
		int tileCount = _level.tilesDown * _level.tilesAcross;
		tileIcons = new GUIContent[tileCount];
		for(int i = 0; i < tileIcons.Length; i++)
		{
			tileIcons[i] = new GUIContent(TileTex(i));
		}
		
		if(_currentTile >= tileCount) _currentTile = 0;
	}
	
	
	
	// Get a texture based on a tile
	private Texture2D TileTex(int tileNumber)
	{
		// Get row and col of tile based on stupid shitty inverted y axis pixel space bullshit grr
		int col = tileNumber%_level.tilesAcross;
		int row = Mathf.FloorToInt(tileNumber/_level.tilesAcross);
		int bottomRow = _level.tilesDown - 1;
		row = bottomRow - row; // invert row
		
		// Now do texturey things
		Texture2D tileSheet = GetMaterial().mainTexture as Texture2D;
		if(tileSheet == null) return ColourTex(Color.white, 32, 32);
		int width = Mathf.FloorToInt(tileSheet.width * _tileX);
		int height = Mathf.FloorToInt(tileSheet.height * _tileY);
		
		// Get pixels from main texture for tiles
		Color[] pix = new Color[width * height];
		pix = tileSheet.GetPixels (col * width, row * height, width, height);
		
		// Create Tile from pixels
		Texture2D tile = new Texture2D(width, height);
		tile.SetPixels(pix);
		tile.Apply();
		
		return tile;
	}
	//------------------------------------------------------------
	
	
	
	//------------------------------------------------------------MESH STUFF!
	// Meshly FUNctions (hint: not fun)
	// Return mesh if exists, create cube if not
	private Mesh GetMesh()
	{
		if(_level.gameObject.GetComponent<MeshFilter>() && _level.gameObject.GetComponent<MeshRenderer>() && _level.gameObject.GetComponent<MeshCollider>()) // already has mesh filter and therefore presumably mesh unless some toerag has fucked around with it
		{
			return _level.gameObject.GetComponent<MeshFilter>().sharedMesh;
		}
		else
		{
			MeshFilter filter; // ref to filter
			MeshRenderer renderer; // ref to filter
			Mesh mesh = GenerateMesh(); // make a cube and save to asset folder
			
			if(!_level.GetComponent<MeshFilter>())filter = _level.gameObject.AddComponent<MeshFilter>(); // give it a filter
			else filter = _level.GetComponent<MeshFilter>();
			
			if(!_level.GetComponent<MeshCollider>()) _collider = _level.gameObject.AddComponent<MeshCollider>(); // give it a collider
			else _collider = _level.GetComponent<MeshCollider>();
			
			if(!_level.GetComponent<MeshRenderer>()) renderer = _level.gameObject.AddComponent<MeshRenderer>(); // give it a renderer
			else renderer = _level.GetComponent<MeshRenderer>();
			
			filter.sharedMesh = mesh;
			_collider.sharedMesh = mesh;
			renderer.material = GetMaterial();
			
			return mesh;
		}
	}
	
	Material GetMaterial()
	{
		Material myMat = _level.GetComponent<MeshRenderer>().sharedMaterial;
		if(myMat == null) myMat = GenerateDefaultMat();
		//else if(myMat.HasProperty() myMat = GenerateDefaultMat();
		
		_level.GetComponent<Renderer>().material = myMat;
		
		return _level.GetComponent<MeshRenderer>().sharedMaterial;
	}
	
	// Generate or get default material (first one created). If you rename it a new one will be generated each time
	Material GenerateDefaultMat()
	{
		// Ensure Level Editor Folder exists and create if not
		string path = Application.dataPath + "/Level Editor";
		if(!Directory.Exists(path)) AssetDatabase.CreateFolder("Assets", "Level Editor");
		
		// Ensure Materials Folder exists and create if not
		path = path + "/Materials";
		if(!Directory.Exists(path)) AssetDatabase.CreateFolder("Assets/Level Editor", "Materials");
		
		// Check all files in Meshes folder and ensure we have a unique name (so don't overwrite any existing meshes)
		string filePath = "Assets/Level Editor/Materials/Tile Default.mat";
		if(!File.Exists(filePath))
		{
			// create
			Material tileDefault = new Material(Shader.Find("Alex's Banging Shaders/Tileable"));
			if(tileDefault == null) Debug.LogError ("Can't find shader 'Alex's Banging Shaders/Tileable' - please add to project!");
			tileDefault.name = "Tile Default";
			AssetDatabase.CreateAsset(tileDefault, filePath);
		}
		
		return AssetDatabase.LoadAssetAtPath(filePath, typeof(Material)) as Material;
	}
	
	void ResetMesh()
	{
		Undo.RecordObject(_mesh, "Reset Mesh");
		Mesh cube = CreatePlane();
		_mesh.Clear();
		_mesh.vertices = cube.vertices;
		_mesh.triangles = cube.triangles;
		_mesh.normals = cube.normals;
		_mesh.uv = cube.uv;
		_mesh.uv2 = cube.uv2;
		UpdateMeshCollider();
		CalculateFaces (); // this needs to be done here!
		CalcAllUvs ();
		_faceSelected = false;
		_currentFace = null;
	}
	
	
	
	// Generate new cube mesh and save to asset folder as LevelMesh_1 etc to correct folder (Level Editor/Meshes). Returns the mesh
	private Mesh GenerateMesh()
	{
		// Ensure Level Editor Folder exists and create if not
		string path = Application.dataPath + "/Level Editor";
		if(!Directory.Exists(path)) AssetDatabase.CreateFolder("Assets", "Level Editor");
		
		// Ensure Meshes Folder exists and create if not
		path = path + "/Meshes";
		if(!Directory.Exists(path)) AssetDatabase.CreateFolder("Assets/Level Editor", "Meshes");
		
		// Check all files in Meshes folder and ensure we have a unique name (so don't overwrite any existing meshes)
		string[] meshPaths = Directory.GetFiles (path);
		int meshNumber = 0;
		for(int i = 0; i < meshPaths.Length; i++)
		{
			if(meshPaths[i].Contains("LevelMesh_" + meshNumber.ToString()))
			{
				meshNumber += 1;
				i = 0;
			}
		}
		
		// Create mesh
		string fileName = "Assets/Level Editor/Meshes/LevelMesh_" + meshNumber.ToString() + ".asset";
		AssetDatabase.CreateAsset(CreatePlane(), fileName);
		
		return AssetDatabase.LoadAssetAtPath(fileName, typeof(Mesh)) as Mesh;
	}
	
	// Create a single 1*1 cube as starting point
	private Mesh CreatePlane() 
	{
		Mesh planeMesh = new Mesh();
		
		
		/* HERE COMES THE PLANEY BIT! */
		Vector3[] verts = new Vector3[8];
		Vector2[] uvs = new Vector2[8];
		Vector2[] offsetUvs = new Vector2[8];
		int[] tris = new int[12];

		// Verts - notice tris corrsepond to order of verts for a face
		verts[0] = new Vector3(-0.5f, 0.5f, 0); // Front Top Left
		verts[1] = new Vector3(0.5f, 0.5f, 0); // Front Top Right
		verts[2] = new Vector3(-0.5f, -0.5f, 0); // Front Bottom Left
		verts[3] = new Vector3(0.5f, -0.5f, 0); // Front Bottom Right
		
		verts[5] = new Vector3(-0.5f, 0.5f, 0); // Back Top Left
		verts[4] = new Vector3(0.5f, 0.5f, 0); // Back Top Right
		verts[7] = new Vector3(-0.5f, -0.5f, 0); // Back Bottom Left
		verts[6] = new Vector3(0.5f, -0.5f, 0); // Back Bottom Right


		// Tris and UVs can be done in for loop as similar - verts a little bit more complex so done em 1 by 1
		for(int i = 0; i < 2; i++)
		{
			// Calc tris
			int tl = i * 4; // top left of this face
			int tr = tl + 1; // top right of this face
			int bl = tl + 2; // bottom left of this face
			int br = tl + 3; // bottom right of this face
			int tri = i * 6; // tri index
			
			// ALways make in this order because it means verts 0, 1, 2, 3 (tl, tr, bl, br) are in that order!
			// 0 -----1 // x -----5
			// |     /| // |     /|
			// |   /  | // |   /  |
			// | /    | // | /    |
			// 2------x // 4------3
			
			tris[tri] = tl;
			tris[tri + 1] = tr;
			tris[tri + 2] = bl;
			tris[tri + 3] = br;
			tris[tri + 4] = bl;
			tris[tri + 5] = tr;
			
			// Calc UVs
			uvs[tl] = new Vector2(0, 1); // top left
			uvs[tr] = new Vector2(1, 1); // top right
			uvs[bl] = new Vector2(0, 0); // bottom left
			uvs[br] = new Vector2(1, 0); // bottom right
		}
		
		
		for(int i = 0; i < offsetUvs.Length; i++)
		{
			offsetUvs[i] = tileToOffset(_currentTile);
		}
		

		/* --------cube end...--------- */
		
		// Now make dat mesh
		planeMesh.vertices = verts;
		planeMesh.triangles = tris;
		planeMesh.uv = uvs;
		planeMesh.uv2 = offsetUvs;
		
		planeMesh.RecalculateNormals(); // might do this by hand later.. (please don't...)
		
		
		
		return planeMesh;
	}
	
	
	
	// Move the selected verts (and duplicates for UV coordinates)
	void MoveSelectedVerts(float dist) { MoveSelectedVerts(_currentFace.normal * dist); } // Move based on a dist (float) in dir of normal
	void MoveSelectedVerts(Vector3 moveOffset) // Move based on an offset vector
	{
		if(_mesh == null) return;
		moveOffset = RoundToSnap(moveOffset); // ensure offset is divisable by snap
		float gridError = 0.01f; // round values to this to avoid tiny changes due to float inaccuracy

		//moveOffset = _level.transform.TransformDirection(moveOffset);
		//Debug.Log (moveOffset);
		
		Vector3[] verts = _mesh.vertices;
		int[] indsToMove;
		if(_level.currentMode == LE_Mode.FACE) indsToMove = _currentFace.allIndices;
		else if(_level.currentMode == LE_Mode.EDGE) indsToMove = _currentFace.GetAllEdgeIndices(_currentEdgeIndex);
		else indsToMove = new int[0]; // temp - no "block mode" yet
		
		// Move all dem lovely indices baby
		for(int i = 0; i < indsToMove.Length; i++)
		{


			int v = indsToMove[i]; // index of vert we're about to move
			verts[v] = RoundTo(verts[v] + moveOffset, gridError); // round might not be needed?

			//Debug.Log ("Adding " + moveOffset + " to vert " + indsToMove[i] + " at pos " + verts[v]);

			//verts[v] = verts[v] + moveOffset; // round might not be needed?

			//.Log ("Added " + moveOffset + " to vert " + indsToMove[i] + "! New pos is " + verts[v]);
		}
		
		_mesh.vertices = verts;
		
		UpdateMeshCollider();
	}
	
	
	// Duplicates selected verts and then moves them by offset
	void BeginExtrude(Vector3 moveOffset)
	{
		// Faff at the start
		if(_mesh == null) return;
		moveOffset = RoundToSnap(moveOffset); // ensure offset is divisable by snap
		//float gridError = 0.01f; // round values to this to avoid tiny changes due to float inaccuracy
		
		
		int vertCount = _mesh.vertexCount; // verts before extrude
		int triCount = _mesh.triangles.Length;
		
		// Arrays
		Vector3[] verts = new Vector3[vertCount + 8];
		Vector3[] norms = new Vector3[vertCount + 8];
		int[] tris = new int[_mesh.triangles.Length + 12]; // 24 new tris (4 faces = 6*4)
		Vector2[] uvs = new Vector2[vertCount + 8];
		Vector2[] tileUvs = new Vector2[vertCount + 8];
		
		// Add new verts
		_mesh.vertices.CopyTo(verts, 0); // copy old verts

		// verts for new faces
		switch(_currentEdgeIndex)
		{
		case 0: // TOP
			verts[vertCount+0] = _currentFace.Vert(Face.TP_LFT) + moveOffset;
			verts[vertCount+1] = _currentFace.Vert(Face.TP_RGT) + moveOffset;
			verts[vertCount+2] = _currentFace.Vert(Face.TP_LFT); // BottomLeft = TopLeft
			verts[vertCount+3] = _currentFace.Vert(Face.TP_RGT); // BottomRight = TopRight
			break;

		case 1: // RIGHT
			verts[vertCount+0] = _currentFace.Vert(Face.TP_RGT); // TopLeft = TopRight
			verts[vertCount+1] = _currentFace.Vert(Face.TP_RGT) + moveOffset;
			verts[vertCount+2] = _currentFace.Vert(Face.BT_RGT); // BottomLeft = BottomRight
			verts[vertCount+3] = _currentFace.Vert(Face.BT_RGT) + moveOffset;
			break;

		case 2: // BOTTOM
			verts[vertCount+0] = _currentFace.Vert(Face.BT_LFT); // top = bottom
			verts[vertCount+1] = _currentFace.Vert(Face.BT_RGT); // top = bottom
			verts[vertCount+2] = _currentFace.Vert(Face.BT_LFT) + moveOffset;
			verts[vertCount+3] = _currentFace.Vert(Face.BT_RGT) + moveOffset;
			break;

		case 3: // LEFT
			verts[vertCount+0] = _currentFace.Vert(Face.TP_LFT) + moveOffset;
			verts[vertCount+1] = _currentFace.Vert(Face.TP_LFT); // right = left
			verts[vertCount+2] = _currentFace.Vert(Face.BT_LFT) + moveOffset;
			verts[vertCount+3] = _currentFace.Vert(Face.BT_LFT); // right = left
			break;
		}

		// Verts for backface
		verts[vertCount+5] = verts[vertCount+0];
		verts[vertCount+4] = verts[vertCount+1];
		verts[vertCount+7] = verts[vertCount+2];
		verts[vertCount+6] = verts[vertCount+3];

		
		// Add new norms
		_mesh.normals.CopyTo(norms, 0);

		for(int i = 0; i < 4; i++)
		{
			norms[vertCount+i] = _currentFace.normal;
			norms[vertCount+4+i]= -_currentFace.normal;
		}

		// Add tris
		_mesh.triangles.CopyTo(tris, 0);

		tris[triCount+0] = vertCount+0; // topLeft
		tris[triCount+1] = vertCount+1; // topRight
		tris[triCount+2] = vertCount+2; // bottomLeft
		tris[triCount+3] = vertCount+3; // bottomRight
		tris[triCount+4] = vertCount+2; // bottomLeft
		tris[triCount+5] = vertCount+1; // topRight

		tris[triCount+6] = vertCount+4; // topLeft
		tris[triCount+7] = vertCount+5; // topRight
		tris[triCount+8] = vertCount+6; // bottomLeft
		tris[triCount+9] = vertCount+7; // bottomRight
		tris[triCount+10] = vertCount+6; // bottomLeft
		tris[triCount+11] = vertCount+5; // topRight
		
		// Add UVs
		_mesh.uv.CopyTo(uvs, 0);

		uvs[vertCount+0] = new Vector2(0, 1); // top left
		uvs[vertCount+1] = new Vector2(1, 1); // top right
		uvs[vertCount+2] = new Vector2(0, 0); // bottom left
		uvs[vertCount+3] = new Vector2(1, 0); // bottom right

		uvs[vertCount+4] = new Vector2(0, 1); // top left
		uvs[vertCount+5] = new Vector2(1, 1); // top right
		uvs[vertCount+6] = new Vector2(0, 0); // bottom left
		uvs[vertCount+7] = new Vector2(1, 0); // bottom right
		
		// Add uv2s
		_mesh.uv2.CopyTo(tileUvs, 0);
		for(int i = _mesh.uv2.Length; i < tileUvs.Length; i++)
		{
			tileUvs[i] = _currentFace.tileOffset;
		}
		
		
		// Set mesh
		_mesh.vertices = verts;
		_mesh.triangles = tris;
		_mesh.uv = uvs;
		_mesh.normals = norms;
		_mesh.uv2 = tileUvs;
		
		
		UpdateMeshCollider();

		// Update shit to do with what you've selected
		_currentFace = null;
		CalculateFaces();
		int faceIndex = _level.faces.Count - 2;
		_currentFace = _level.faces[faceIndex];
		CalcAllUvs ();

		//Debug.Log ("Faces: " + _faceCount + ", Verts: " + _mesh.vertexCount);
	}
	
	
	
	//---------------------UTILITY / HELPER FUNCTONS ------------------------------//
	// Calculate tile uvs
	Vector2[] CalcUvs(int faceIndex)
	{
		return CalcUvs (faceIndex, _level.faces[faceIndex].UVflippedHoriz, _level.faces[faceIndex].UVflippedVert, _level.faces[faceIndex].UVoriginFlippedHoriz, _level.faces[faceIndex].UVoriginFlippedVert);
	}
	
	Vector2[] CalcUvs(int faceIndex, bool flipU, bool flipV, bool flipOriginU, bool flipOriginV)
	{
		Vector2 tl = _level.faces[faceIndex].tileUvs[Face.TP_LFT];
		Vector2 tr = _level.faces[faceIndex].tileUvs[Face.TP_RGT];
		Vector2 bl = _level.faces[faceIndex].tileUvs[Face.BT_LFT];
		Vector2 br = _level.faces[faceIndex].tileUvs[Face.BT_RGT];
		
		// TO DO - This is the bit where you can alter depending on if face has "flipped" UVs or whatnot
		// At the moment though just regular (tl = tl etc)
		
		Vector2 biggestSides = new Vector2(Mathf.Max (tl.x, tr.x), Mathf.Max (tl.y, bl.y));
		
		// 'orizontal
		if(!flipOriginU)
		{
			if(!flipU) // default
			{
				tl.x = 0;
				tr.x *= _tileX;
				bl.x = 0;
				br.x*= _tileX;
			}
			else // flipped, normal origin
			{
				tl.x *= _tileX;
				tr.x = 0;
				bl.x *= _tileX;
				br.x = 0;
			}
		}
		else
		{
			if(!flipU) // inverse origin
			{
				tl.x = (biggestSides.x - tl.x) * _tileX;
				tr.x = biggestSides.x * _tileX;
				bl.x = (biggestSides.x - bl.x) * _tileX;
				br.x= biggestSides.x * _tileX;
			}
			else // flipped and inverse origin
			{
				tl.x = biggestSides.x * _tileX;
				tr.x = (biggestSides.x - tr.x) * _tileX;
				bl.x = biggestSides.x * _tileX;
				br.x = (biggestSides.x - br.x) * _tileX;
			}
		}
		
		
		
		
		// Vertical Dood
		if(!flipOriginV)
		{
			if(!flipV) // default
			{
				tl.y *= _tileY;
				tr.y *= _tileY;
				bl.y = 0;
				br.y = 0;
			}
			else // flipped, normal origin
			{
				tl.y = 0;
				tr.y = 0;
				bl.y *= _tileY;
				br.y *= _tileY;
			}
		}
		else
		{
			if(!flipV) // inverse origin
			{
				tl.y = biggestSides.y * _tileY;
				tr.y = biggestSides.y *_tileY;
				bl.y = (biggestSides.y - bl.y) * _tileY;
				br.y = (biggestSides.y - br.y) * _tileY;
			}
			else // flipped and inverse origin
			{
				tl.y = (biggestSides.y - tl.y) * _tileY;
				tr.y = (biggestSides.y - tr.y) * _tileY;
				bl.y = biggestSides.y * _tileY;
				br.y = biggestSides.y * _tileY;
			}
		}
		
		
		/*
		if(_currentFace != null)
		{
			Debug.Log ("Top Left " + _mesh.uv[_currentFace.indices[Face.TP_LFT]].ToString());
			Debug.Log ("Top Right " + _mesh.uv[_currentFace.indices[Face.TP_RGT]].ToString());
			Debug.Log ("Bottom Left " + _mesh.uv[_currentFace.indices[Face.BT_LFT]].ToString());
			Debug.Log ("Bottom Right " + _mesh.uv[_currentFace.indices[Face.BT_RGT]].ToString());
		}
		*/
		
		return new Vector2[4] { tl, tr, bl, br };
	}
	
	void CalcAllUvs()
	{
		Vector2[] newUvs = new Vector2[_mesh.vertexCount];
		
		for(int i = 0; i < _faceCount; i++)
		{
			Vector2[] faceVerts = CalcUvs(i);
			
			// Now set to UVs in newUvs array
			int u = i * 4; // tl UV of this face
			newUvs[u+0] = faceVerts[0];
			newUvs[u+1] = faceVerts[1];
			newUvs[u+2] = faceVerts[2];
			newUvs[u+3] = faceVerts[3];
		}
		_mesh.uv = newUvs;
		
		
		
		
	}
	
	void ModCurrFaceUvs(bool flipU, bool flipV, bool flipOriginU, bool flipOriginV)
	{
		if(_currentFace == null) return;
		
		Vector2[] uvs = _mesh.uv;
		Vector2[] modUvs = CalcUvs (_currentFace.faceNumber, flipU, flipV, flipOriginU, flipOriginV);
		
		uvs[_currentFace.indices[Face.TP_LFT]] = modUvs[Face.TP_LFT];
		uvs[_currentFace.indices[Face.TP_RGT]] = modUvs[Face.TP_RGT];
		uvs[_currentFace.indices[Face.BT_LFT]] = modUvs[Face.BT_LFT];
		uvs[_currentFace.indices[Face.BT_RGT]] = modUvs[Face.BT_RGT];
		
		_mesh.uv = uvs;
	}
	
	void FlipHoriUVs() { ModCurrFaceUvs(!_currentFace.UVflippedHoriz, _currentFace.UVflippedVert, _currentFace.UVoriginFlippedHoriz, _currentFace.UVoriginFlippedVert); }
	void FlipVertUVs() { ModCurrFaceUvs(_currentFace.UVflippedHoriz, !_currentFace.UVflippedVert, _currentFace.UVoriginFlippedHoriz, _currentFace.UVoriginFlippedVert); }
	void FlipOriginHoriUVs() { ModCurrFaceUvs(_currentFace.UVflippedHoriz, _currentFace.UVflippedVert, !_currentFace.UVoriginFlippedHoriz, _currentFace.UVoriginFlippedVert); }
	void FlipOriginVertUVs() { ModCurrFaceUvs(_currentFace.UVflippedHoriz, _currentFace.UVflippedVert, _currentFace.UVoriginFlippedHoriz, !_currentFace.UVoriginFlippedVert); }
	
	
	// Rounding
	float RoundToSnap(float value){ return RoundTo (value, _level.snap); } // float to snap
	Vector3 RoundToSnap(Vector3 value){	return RoundTo(value, _level.snap); } // v3 to snap
	
	float RoundTo(float value, float roundTo) // float to whatever you specify
	{
		if(roundTo <= 0) return value;
		
		value /= roundTo;
		value = Mathf.Round(value);
		value *= roundTo;
		
		return value;
		
	}
	
	Vector2 Floor(Vector2 value, int decPlaces){ return new Vector2(Floor (value.x, decPlaces), Floor (value.y, decPlaces)); }
	float Floor(float value, int decPlaces) // float to whatever you specify
	{
		//if(decPlaces == 0) return value;
		int multiplier = 1;
		
		for(int i = 0; i < decPlaces; i++) 
			multiplier *= 10;
		
		value *= multiplier;
		value = Mathf.Floor(value);
		value /= multiplier;
		
		return value;
		
	}
	
	Vector3 RoundTo(Vector3 value, float roundTo) // v3 to whatever you specify
	{
		value.x = RoundTo(value.x, roundTo);
		value.y = RoundTo(value.y, roundTo);
		value.z = RoundTo(value.z, roundTo);
		return value;
	}
	
	// Can haz plain bg texture from color plz plz thx bai
	private Texture2D ColourTex(Color col) { return ColourTex (col, 1, 1); }
	private Texture2D ColourTex(Color col, int width, int height)
	{
		Color[] pix = new Color[width * height];
		
		for(int i = 0; i < pix.Length; i++)
			pix[i] = col;
		
		Texture2D result = new Texture2D(width, height);
		result.SetPixels(pix);
		result.Apply();
		return result;
	}
	
	
	// Calc normal of a triangle
	Vector3 CalcTriNorm(Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 v1 = b-a; // vector from a to b
		Vector3 v2 = c-a; // vector from a to c
		return Vector3.Cross(v1, v2).normalized; // normalized cross product
	}
	
	// When mesh changes, call this to recalculate mesh collider
	void UpdateMeshCollider()
	{
		_mesh.RecalculateBounds();
		_collider.sharedMesh = null; // have to set to null first or else doesn't bother to check that mesh has changed / been updated
		_collider.sharedMesh = _mesh;
	}
	
	
	
	//-----------------------------------------------------------------------------
	// FANCY NEW FUNCTIONS FOR FANCY FACE CLASS, OO CHECK HIM OUT!
	
	// Calculates Faces by going through mesh - note squares must be made from adjacent triangles (e.g. 1+2, 3+4, etc)
	void CalculateFaces()
	{
		if(_level.faces == null)
			_level.faces = new List<Face>();
		else
			_level.faces.Clear ();
		
		for(int i = 0; i < _faceCount; i++)
		{
			int triA = i * 2;
			int triB = triA + 1;
			Face newFace = new Face(_mesh, triA, triB);
			newFace.faceNumber = i;
			_level.faces.Add(newFace);
		}
	}
	
	void CalculateCurrentFaceFromTri(int triIndex)
	{
		for(int i = 0; i < _level.faces.Count; i++)
		{
			if(_level.faces[i].UsesTri(triIndex))
			{
				_currentFace = _level.faces[i];
				return;
			}
		}
	}
	
	// Convert between offsetUvs and tile numbers
	Vector2 tileToOffset(int tileNumber)
	{
		int across = tileNumber % _level.tilesAcross;
		int down = Mathf.FloorToInt(tileNumber / _level.tilesAcross);
		
		return new Vector2(across * _tileX, 1-(down * _tileY) - _tileY);
	}
	
	int offsetToTile(Vector2 offset)
	{
		Vector2 inverseOffset = offset;
		inverseOffset.y = 1 - (inverseOffset.y + _tileY);
		
		int across = Mathf.RoundToInt(inverseOffset.x/_tileX);
		int down = Mathf.RoundToInt(inverseOffset.y/_tileY);
		
		return (down * _level.tilesAcross) + across;
		
	}	
}
#endif