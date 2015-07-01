using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public class Face
{
	private Mesh _mesh;

	public const int TP_LFT = 0, TP_RGT = 1, BT_LFT = 2, BT_RGT = 3; // for referencing corners in an array of 4 verts
	public const int TOP = 0, RIGHT = 1, BOTTOM = 2, LEFT = 3; // for referencing edges
	
	private int[] _triIndices = new int[2]; // The 2 triangles that make up this (square) face
	private int[] _mainInds = new int[4]; // 4 Main indices of the face, one at each corner
	private int[] _dupInds; // duplicate indices, same Vector3 values as main but relate to joined side verts with different texture coordinates
	
	// Constructinator
	public Face(Mesh mesh, int triA, int triB)
	{
		_mesh = mesh;
		_triIndices[0] = triA;
		_triIndices[1] = triB;

		// Set main inds
		for(int i = 0; i < 3; i++) _mainInds[i] = _mesh.triangles[(_triIndices[0] * 3) + i]; // First 3 are first tri in order
		_mainInds[3] = _mesh.triangles[_triIndices[1] * 3]; // Last one is first in second tri
	}


	// For tiles
	public Vector2 tileOffset
	{
		get { return _mesh.uv2[_mainInds[TP_LFT]]; }
		
		set
		{
			Vector2[] offsets = _mesh.uv2;
			for(int i = 0; i < 4; i++)
			{
				offsets[_mainInds[i]] = value;
			}
			_mesh.uv2 = offsets;
		}
	}

	// Get edges etc
	public Edge[] edges 
	{ get
		{
			return new Edge[4]
			{
				new Edge(Vert(TP_LFT), Vert (TP_RGT)), // TOP
				new Edge(Vert(TP_RGT), Vert (BT_RGT)), // RIGHT
				new Edge(Vert(BT_RGT), Vert (BT_LFT)), // BOTTOM
				new Edge(Vert(BT_LFT), Vert (TP_LFT)) // LEFT
			};
		}
	}

	// Mainly for debug
	private int _faceNumber;
	public int faceNumber { set { _faceNumber = value; } get { return _faceNumber; }}
	public void PrintNumber() { Debug.Log ("Face " + _faceNumber); }

	public void PrintInfo()
	{
		PrintNumber();
		int[] triVertsA = new int[] { _mesh.triangles[_triIndices[0] * 3], _mesh.triangles[(_triIndices[0] * 3) + 1], _mesh.triangles[(_triIndices[0] * 3) + 2] };
		int[] triVertsB = new int[] { _mesh.triangles[_triIndices[1] * 3], _mesh.triangles[(_triIndices[1] * 3) + 1], _mesh.triangles[(_triIndices[1] * 3) + 2] };
		Debug.Log ("TriA = " + triVertsA[0] + ", " + triVertsA[1] + ", " + triVertsA[2]);
		Debug.Log ("TriB = " + triVertsB[0] + ", " + triVertsB[1] + ", " + triVertsB[2]);

		Debug.Log ("UVS: tl " + _mesh.uv[_mainInds[TP_LFT]] + ", tr " + _mesh.uv[_mainInds[TP_RGT]] + ", bl " + _mesh.uv[_mainInds[BT_LFT]] + ", br " + _mesh.uv[_mainInds[BT_RGT]]);
		Debug.Log ("UV2: tl " + _mesh.uv2[_mainInds[TP_LFT]] + ", tr " + _mesh.uv2[_mainInds[TP_RGT]] + ", bl " + _mesh.uv2[_mainInds[BT_LFT]] + ", br " + _mesh.uv2[_mainInds[BT_RGT]]);
	}

	// Does this face use the specified triangle?
	public bool UsesTri(int triIndex)
	{
		if(_triIndices[0] == triIndex || _triIndices[1] == triIndex) return true;
		return false;
	}

	// Getters
	public Vector3 Vert(int corner) { return _mesh.vertices[_mainInds[corner]]; } // specified vert
	public Vector3 normal { get { return _mesh.normals[_mainInds[0]]; } } // normal
	public int[] indices { get { return _mainInds; } } // indices
	public int[] dupIndices // duplicate indices
	{ get 
		{ 
			if(_dupInds == null) CalcDups(); // work em out if never calculated them
			return _dupInds;
		} 
	}




	public int[] allIndices // indices and duplicates as one array
	{ get
		{
			int[] returnArray = new int[4 + dupIndices.Length];
			_mainInds.CopyTo(returnArray, 0);
			dupIndices.CopyTo(returnArray, 4);
			return returnArray;
		}
	}

	public Vector3[] verts
	{ get 
		{
			Vector3[] returnArray = new Vector3[4];

			for(int i = 0; i < 4; i++)
			{
				int v = _mainInds[i];
				returnArray[i] = _mesh.vertices[v];
			}

			return returnArray;
		} 
	}



	public Vector3 centre 
	{ get 
		{
			Vector3 dir = Vector3.Normalize(Vert(BT_RGT) - Vert(TP_LFT));
			float dist = Vector3.Distance(Vert(TP_LFT), Vert(BT_RGT)) * 0.5f;
			
			return Vert(TP_LFT) + (dir * dist);
		}
	}

	public int[] GetAllEdgeIndices(int edge)
	{
		if(edge < 0 || edge >= 4) return null;
		Vector3[] edgeVerts = edges[edge].verts;

		List<int> inds = new List<int>();
		for(int i = 0; i < _mesh.vertices.Length; i++)
		{
			if(_mesh.vertices[i] == edgeVerts[0] || _mesh.vertices[i] == edgeVerts[1]) // if vert is same value as j
			{
				inds.Add (i);
			}
		}

		return inds.ToArray();
	}

	// Get correct uvs, taking into account scaling / tiling and flipping etc
	/*
	public Vector2[] correctUvs
	{ get
		{
			Vector2 tl = tileUvs[Face.TP_LFT];
			Vector2 tr = tileUvs[Face.TP_RGT];
			Vector2 bl = tileUvs[Face.BT_LFT];
			Vector2 br = tileUvs[Face.BT_RGT];
			
			// TO DO - This is the bit where you can alter depending on if face has "flipped" UVs or whatnot
			// At the moment though just regular (tl = tl etc)
			float leftScalar = 0, rightScalar = _tileX, topScalar = _tileY, bottomScalar = 0;
			
			if(_currentFace.FlipHoriUVs)
			{
				leftScalar = _tileX;
				rightScalar = 0;
			}
			if(_currentFace.FlipVertUVs)
			{
				topScalar = 0;
				bottomScalar = _tileY;
			}
			
			
			tl.Scale(new Vector2(leftScalar, topScalar));
			tr.Scale(new Vector2(rightScalar, topScalar));
			bl.Scale(new Vector2(leftScalar, bottomScalar));
			br.Scale(new Vector2(rightScalar, bottomScalar));
			
			
			
			
			// Now set to UVs in newUvs array
			int u = i * 4; // tl UV of this face
			newUvs[u+0] = tl;
			newUvs[u+1] = tr;
			newUvs[u+2] = bl;
			newUvs[u+3] = br;

		}
	}
	*/

	// Dis a complex one homey
	public Vector2[] tileUvs
	{ get
		{
			Vector2[] newUvs = new Vector2[4];

			float topX;
			float botX;
			float leftY;
			float rightY;

			if(Mathf.Abs (normal.x) >= 0.9f) // left or right
			{
				topX = Mathf.Abs (Vert (TP_RGT).z - Vert (TP_LFT).z);
				botX = Mathf.Abs (Vert (BT_RGT).z - Vert (BT_LFT).z);
				leftY = Mathf.Abs (Vert (BT_LFT).y - Vert (TP_LFT).y);
				rightY= Mathf.Abs (Vert (BT_RGT).y - Vert (TP_RGT).y);
			}
			else if(Mathf.Abs (normal.z) >= 0.9f) // forward or back
			{
				topX = Mathf.Abs (Vert (TP_RGT).x - Vert (TP_LFT).x);
				botX = Mathf.Abs (Vert (BT_RGT).x - Vert (BT_LFT).x);
				leftY = Mathf.Abs (Vert (BT_LFT).y - Vert (TP_LFT).y);
				rightY= Mathf.Abs (Vert (BT_RGT).y - Vert (TP_RGT).y);
			}
			else
			{
				topX = Mathf.Abs (Vert (TP_RGT).x - Vert (TP_LFT).x);
				botX = Mathf.Abs (Vert (BT_RGT).x - Vert (BT_LFT).x);
				leftY = Mathf.Abs (Vert (BT_LFT).z - Vert (TP_LFT).z);
				rightY= Mathf.Abs (Vert (BT_RGT).z - Vert (TP_RGT).z);
			}

			newUvs[TP_LFT] = new Vector2(topX, leftY);
			newUvs[TP_RGT] = new Vector2(topX, rightY);
			newUvs[BT_LFT] = new Vector2(botX, leftY);
			newUvs[BT_RGT] = new Vector2(botX, rightY);

			return newUvs;
		}
	}

	// Get if UVs have been flipped
	public bool UVflippedHoriz
	{ get
		{
			// If the left is less than the right then it is NOT flipped
			if(_mesh.uv[_mainInds[TP_LFT]].x < _mesh.uv[_mainInds[TP_RGT]].x ||
			   _mesh.uv[_mainInds[BT_LFT]].x < _mesh.uv[_mainInds[BT_RGT]].x) 
			{
				return false;
			}
			
			return true;
		}
	}
	public bool UVflippedVert
	{ get
		{
			// If the bottom is less than the top then it is NOT flipped
			if(_mesh.uv[_mainInds[BT_LFT]].y < _mesh.uv[_mainInds[TP_LFT]].y ||
			   _mesh.uv[_mainInds[BT_RGT]].y < _mesh.uv[_mainInds[TP_RGT]].y) 
			{
				return false;
			}

			return true;
		}
	}

	// Get if UVs have been flipped
	public bool UVoriginFlippedHoriz // default is left, so left = false, right = true
	{ get
		{
			bool flipped = UVflippedHoriz;

			// if both rights have the same value, origin is right so return true
			//if(_mesh.uv[_mainInds[TP_RGT]].x == _mesh.uv[_mainInds[BT_RGT]].x) return true;

			if(_mesh.uv[_mainInds[TP_RGT]].x == _mesh.uv[_mainInds[BT_RGT]].x && !flipped) return true;
			else if(_mesh.uv[_mainInds[TP_LFT]].x == _mesh.uv[_mainInds[BT_LFT]].x && flipped) return true;
			return false;
		}
	}
	public bool UVoriginFlippedVert // default is bottom, so bottom = false, top = true
	{ get
		{
			bool flipped = UVflippedVert;

			// if both tops have the same value, origin is top so return true
			//if(_mesh.uv[_mainInds[TP_LFT]].y == _mesh.uv[_mainInds[TP_RGT]].y) return true;
			if(_mesh.uv[_mainInds[TP_LFT]].y == _mesh.uv[_mainInds[TP_RGT]].y && !flipped) return true;
			else if(_mesh.uv[_mainInds[BT_LFT]].y == _mesh.uv[_mainInds[BT_RGT]].y && flipped) return true;
			return false;
		}
	}


	// Delightful functions
	/*
	public void FlipHoriUVs()
	{
		Vector2[] uvs = _mesh.uv;

		// Flip top uvs
		Vector2 temp = uvs[_mainInds[TP_LFT]];
		uvs[_mainInds[TP_LFT]] = uvs[_mainInds[TP_RGT]]; // top left = top right
		uvs[_mainInds[TP_RGT]] = temp; // top right = top left

		// Flip bottom uvs
		temp = uvs[_mainInds[BT_LFT]];
		uvs[_mainInds[BT_LFT]] = uvs[_mainInds[BT_RGT]]; // bot left = bot right
		uvs[_mainInds[BT_RGT]] = temp; // bot right = bot left

		_mesh.uv = uvs;
	}

	public void FlipVertUVs()
	{
		Vector2[] uvs = _mesh.uv;
		
		// Left top uvs
		Vector2 temp = uvs[_mainInds[TP_LFT]];
		uvs[_mainInds[TP_LFT]] = uvs[_mainInds[BT_LFT]]; // top left = bot left
		uvs[_mainInds[BT_LFT]] = temp; // bot left = top left
		
		// Right top uvs
		temp = uvs[_mainInds[TP_RGT]];
		uvs[_mainInds[TP_RGT]] = uvs[_mainInds[BT_RGT]]; // top right = bot right
		uvs[_mainInds[BT_RGT]] = temp; // bot right = top right
		
		_mesh.uv = uvs;
	}
	*/






	public Vector3[] GetExtrudeNormals()
	{
		Vector3[] faceNorms = new Vector3[4];

		if(Mathf.Abs (normal.x) >= 0.9f) // left or right
		{
			faceNorms[0] = Vector3.back;
			faceNorms[1] = Vector3.forward;
			faceNorms[2] = Vector3.up;
			faceNorms[3] = Vector3.down;
		}
		else if(normal.z >= 0.9f) // forward
		{
			faceNorms[0] = Vector3.right;
			faceNorms[1] = Vector3.left;
			faceNorms[2] = Vector3.up;
			faceNorms[3] = Vector3.down;
		}
		else if(normal.z <= -0.9f) // back
		{
			faceNorms[0] = Vector3.left;
			faceNorms[1] = Vector3.right;
			faceNorms[2] = Vector3.up;
			faceNorms[3] = Vector3.down;
		}
		else // Up or down
		{
			faceNorms[0] = Vector3.back;
			faceNorms[1] = Vector3.right;
			faceNorms[2] = Vector3.forward;
			faceNorms[3] = Vector3.left;
		}

		Vector3[] norms = new Vector3[16];

		for(int i = 0; i < norms.Length; i++)
		{
			int j = (int)(Mathf.Floor(i/4));
			norms[i] = faceNorms[j];
		}

		return norms;
	}

	public Vector3[] GetExtrudeVerts(Vector3 extrude)
	{
		// Note when I say "front face" I mean the one facing the camera. Unity would consider this the back face as we are "looking forward"
		Vector3[] extrudeVerts = new Vector3[16];

		// LEFT EXTRUDE
		if(normal == Vector3.left)
		{
			// FRONT FACE
			extrudeVerts[0] = Vert (TP_RGT) + extrude; // top left
			extrudeVerts[1] = Vert (TP_RGT); // top right
			extrudeVerts[2] = Vert (BT_RGT) + extrude; // bottom left
			extrudeVerts[3] = Vert (BT_RGT); // bottom right

			// BACK FACE
			extrudeVerts[4] = Vert (TP_LFT); // top left
			extrudeVerts[5] = Vert (TP_LFT) + extrude; // top right
			extrudeVerts[6] = Vert (BT_LFT); // bottom left
			extrudeVerts[7] = Vert (BT_LFT) + extrude; // bottom right

			// TOP FACE
			extrudeVerts[8] = Vert (TP_LFT) + extrude; // top left
			extrudeVerts[9] = Vert (TP_LFT); // top right
			extrudeVerts[10] = Vert (TP_RGT) + extrude; // bottom left
			extrudeVerts[11] = Vert (TP_RGT); // bottom right

			// BOTTOM FACE
			extrudeVerts[12] = Vert (BT_RGT) + extrude; // top left
			extrudeVerts[13] = Vert (BT_RGT); // top right
			extrudeVerts[14] = Vert (BT_LFT) + extrude; // bottom left
			extrudeVerts[15] = Vert (BT_LFT); // bottom right

			return extrudeVerts;
		}


		// RIGHT EXTRUDE
		if(normal == Vector3.right)
		{
			// FRONT FACE
			extrudeVerts[0] = Vert (TP_LFT); // top left
			extrudeVerts[1] = Vert (TP_LFT) + extrude; // top right
			extrudeVerts[2] = Vert (BT_LFT) ; // bottom left
			extrudeVerts[3] = Vert (BT_LFT) + extrude; // bottom right
			
			// BACK FACE
			extrudeVerts[4] = Vert (TP_RGT) + extrude; // top left
			extrudeVerts[5] = Vert (TP_RGT); // top right
			extrudeVerts[6] = Vert (BT_RGT) + extrude; // bottom left
			extrudeVerts[7] = Vert (BT_RGT); // bottom right
			
			// TOP FACE
			extrudeVerts[8] = Vert (TP_RGT); // top left
			extrudeVerts[9] = Vert (TP_RGT) + extrude; // top right
			extrudeVerts[10] = Vert (TP_LFT); // bottom left
			extrudeVerts[11] = Vert (TP_LFT) + extrude; // bottom right
			
			// BOTTOM FACE
			extrudeVerts[12] = Vert (BT_LFT); // top left
			extrudeVerts[13] = Vert (BT_LFT) + extrude; // top right
			extrudeVerts[14] = Vert (BT_RGT); // bottom left
			extrudeVerts[15] = Vert (BT_RGT) + extrude; // bottom right
			
			return extrudeVerts;
		}


		// FORWARD (into screen!) EXTRUDE
		if(normal == Vector3.forward)
		{
			// RIGHT FACE
			extrudeVerts[0] = Vert (TP_LFT); // top left
			extrudeVerts[1] = Vert (TP_LFT) + extrude; // top right
			extrudeVerts[2] = Vert (BT_LFT); // bottom left
			extrudeVerts[3] = Vert (BT_LFT) + extrude; // bottom right
			
			// LEFT FACE
			extrudeVerts[4] = Vert (TP_RGT) + extrude; // top left
			extrudeVerts[5] = Vert (TP_RGT); // top right
			extrudeVerts[6] = Vert (BT_RGT) + extrude; // bottom left
			extrudeVerts[7] = Vert (BT_RGT); // bottom right
			
			// TOP FACE
			extrudeVerts[8] = Vert (TP_RGT) + extrude; // top left
			extrudeVerts[9] = Vert (TP_LFT) + extrude; // top right
			extrudeVerts[10] = Vert (TP_RGT); // bottom left
			extrudeVerts[11] = Vert (TP_LFT); // bottom right
			
			// BOTTOM FACE
			extrudeVerts[12] = Vert (BT_RGT); // top left
			extrudeVerts[13] = Vert (BT_LFT); // top right
			extrudeVerts[14] = Vert (BT_RGT) + extrude; // bottom left
			extrudeVerts[15] = Vert (BT_LFT) + extrude; // bottom right
			
			return extrudeVerts;
		}
		
		
		// BACK (towards screen!) EXTRUDE
		if(normal == Vector3.back)
		{
			// RIGHT FACE
			extrudeVerts[4] = Vert (TP_RGT) + extrude; // top left
			extrudeVerts[5] = Vert (TP_RGT); // top right
			extrudeVerts[6] = Vert (BT_RGT) + extrude; // bottom left
			extrudeVerts[7] = Vert (BT_RGT); // bottom right

			// LEFT FACE
			extrudeVerts[0] = Vert (TP_LFT); // top left
			extrudeVerts[1] = Vert (TP_LFT) + extrude; // top right
			extrudeVerts[2] = Vert (BT_LFT); // bottom left
			extrudeVerts[3] = Vert (BT_LFT) + extrude; // bottom right
			
			// TOP FACE
			extrudeVerts[8] = Vert (TP_LFT); // top left
			extrudeVerts[9] = Vert (TP_RGT); // top right
			extrudeVerts[10] = Vert (TP_LFT) + extrude; // bottom left
			extrudeVerts[11] = Vert (TP_RGT) + extrude; // bottom right
			
			// BOTTOM FACE
			extrudeVerts[12] = Vert (BT_LFT) + extrude; // top left
			extrudeVerts[13] = Vert (BT_RGT) + extrude; // top right
			extrudeVerts[14] = Vert (BT_LFT); // bottom left
			extrudeVerts[15] = Vert (BT_RGT); // bottom right
			
			return extrudeVerts;
		}
		
		
		// UP EXTRUDE
		if(normal == Vector3.up)
		{
			// FRONT FACE
			extrudeVerts[0] = Vert (BT_LFT) + extrude; // top left
			extrudeVerts[1] = Vert (BT_RGT) + extrude; // top right
			extrudeVerts[2] = Vert (BT_LFT); // bottom left
			extrudeVerts[3] = Vert (BT_RGT); // bottom right
			
			// RIGHT FACE
			extrudeVerts[4] = Vert (BT_RGT) + extrude; // top left
			extrudeVerts[5] = Vert (TP_RGT) + extrude; // top right
			extrudeVerts[6] = Vert (BT_RGT); // bottom left
			extrudeVerts[7] = Vert (TP_RGT); // bottom right
			
			// BACK FACE
			extrudeVerts[8] = Vert (TP_RGT) + extrude; // top left
			extrudeVerts[9] = Vert (TP_LFT) + extrude; // top right
			extrudeVerts[10] = Vert (TP_RGT); // bottom left
			extrudeVerts[11] = Vert (TP_LFT); // bottom right
			
			// LEFT FACE
			extrudeVerts[12] = Vert (TP_LFT) + extrude; // top left
			extrudeVerts[13] = Vert (BT_LFT) + extrude; // top right
			extrudeVerts[14] = Vert (TP_LFT); // bottom left
			extrudeVerts[15] = Vert (BT_LFT); // bottom right
			
			return extrudeVerts;
		}
		
		
		// DOWN EXTRUDE
		if(normal == Vector3.down)
		{
			// FRONT FACE
			extrudeVerts[0] = Vert (TP_LFT); // top left
			extrudeVerts[1] = Vert (TP_RGT); // top right
			extrudeVerts[2] = Vert (TP_LFT) + extrude; // bottom left
			extrudeVerts[3] = Vert (TP_RGT) + extrude; // bottom right
			
			// RIGHT FACE
			extrudeVerts[4] = Vert (TP_RGT); // top left
			extrudeVerts[5] = Vert (BT_RGT); // top right
			extrudeVerts[6] = Vert (TP_RGT) + extrude; // bottom left
			extrudeVerts[7] = Vert (BT_RGT) + extrude; // bottom right
			
			// BACK FACE
			extrudeVerts[8] = Vert (BT_RGT); // top left
			extrudeVerts[9] = Vert (BT_LFT); // top right
			extrudeVerts[10] = Vert (BT_RGT) + extrude; // bottom left
			extrudeVerts[11] = Vert (BT_LFT) + extrude; // bottom right
			
			// LEFT FACE
			extrudeVerts[12] = Vert (BT_LFT); // top left
			extrudeVerts[13] = Vert (TP_LFT); // top right
			extrudeVerts[14] = Vert (BT_LFT) + extrude; // bottom left
			extrudeVerts[15] = Vert (TP_LFT) + extrude; // bottom right
			
			return extrudeVerts;
		}


		Debug.LogError("Normal not axis alligned when trying to extrude!");
		return extrudeVerts;
	}




	void CalcDups() // calcs duplicate indices, the ones that don't make this face but share a vert pos with one of the verts
	{
		List<int> dups = new List<int>();
		for(int i = 0; i < _mesh.vertices.Length; i++)
		{
			for(int j = 0; j < _mainInds.Length; j++)
			{
				if(i == _mainInds[j]) continue; // if it literally is this vert then continue
				if(_mesh.vertices[i] == Vert (j)) // if vert is same value as j
				{
					dups.Add (i);
					continue;
				}
			}
		}
		_dupInds = new int[dups.Count];
		dups.CopyTo(_dupInds, 0);
	}

	// EDGES = oo, edgy!
	public int NearestEdge(Vector3 point)
	{
		Edge[] myEdges = edges;
		int nearestEdge = 0;
		float dist = Edge.DistanceToEdge(myEdges[nearestEdge].verts[0], myEdges[nearestEdge].verts[1], point);

		for(int i = 1; i < 4; i++)
		{
			float thisDist = Edge.DistanceToEdge(myEdges[i].verts[0], myEdges[i].verts[1], point);

			if(thisDist < dist)
			{
				dist = thisDist;
				nearestEdge = i;
			}
		}

		return nearestEdge;
	}


	// Split face based on edge passed as arg
	public void SplitFace(Edge newEdge, bool horizontally) { SplitFace (newEdge.verts[0], newEdge.verts[1], horizontally); }
	public void SplitFace(Vector3 vertA, Vector3 vertB, bool horizontally)//only vertical split atm
	{
		int newVertsStart = _mesh.vertexCount;
		int newTrisStart = _mesh.triangles.Length;
		int newVertCount = newVertsStart + 4;
		int newIndCount =  newTrisStart + 6;

		// Get verts with +4 new verts on end
		Vector3[] verts = new Vector3[newVertCount];
		_mesh.vertices.CopyTo(verts, 0);

		// EDIT EXISTING FACE SO RIGHT HAND SIDE IS NEW EDGE
		// Edit right verts for this face
		if(horizontally)
		{
			verts[indices[BT_LFT]] = vertA;
			verts[indices[BT_RGT]] = vertB;
		}
		else
		{
			verts[indices[TP_RGT]] = vertA;
			verts[indices[BT_RGT]] = vertB;
		}


		// CREATE NEW FACE FOR RIGHT FACE IN SPLIT
		Vector3[] normals = new Vector3[newVertCount];
		//Vector2[] uv = new Vector2[newVertCount];
		Vector2[] uv2 = new Vector2[newVertCount];
		int[] tris = new int[newIndCount];

		_mesh.normals.CopyTo(normals, 0);
		//_mesh.uv.CopyTo(uv, 0);
		_mesh.uv2.CopyTo(uv2, 0);
		_mesh.triangles.CopyTo(tris, 0);

		// New verts
		int tl = newVertsStart+0;
		int tr = newVertsStart+1;
		int bl = newVertsStart+2;
		int br = newVertsStart+3;

		if(horizontally)
		{
			verts[tl] = vertA;
			verts[tr] = vertB;
			verts[bl] = Vert (BT_LFT);
			verts[br] = Vert(BT_RGT);
		}
		else
		{
			verts[tl] = vertA;
			verts[tr] = Vert(TP_RGT);
			verts[bl] = vertB;
			verts[br] = Vert(BT_RGT);
		}

		// New norms and uv2s
		for(int i = 0; i < 4; i++)
		{
			normals[newVertsStart + i] = normal;
			uv2[newVertsStart + i] = tileOffset;
		}

		// Unt finallys ve doing ze trianges
		tris[newTrisStart+0] = tl;
		tris[newTrisStart+1] = tr;
		tris[newTrisStart+2] = bl;
		tris[newTrisStart+3] = br;
		tris[newTrisStart+4] = bl;
		tris[newTrisStart+5] = tr;


		// Set the new values!
		_mesh.vertices = verts;
		_mesh.normals = normals;
		_mesh.uv2 = uv2;
		_mesh.triangles = tris;
	}





















	// Old code... not very nice...
	/*
	void CalcMainIndices()
	{
		int[] tris = _mesh.triangles;
		int[] topInds = new int[3];
		int[] botInds = new int[3];


		for(int i = 0; i < 3; i++)
		{
			topInds[i] = tris[(_triIndices[0] * 3) + i];
			botInds[i] = tris[(_triIndices[1] * 3) + i];
		}

		// Which is top left? And save remaining remaining indices so we know they are top right and bottom left
		int remainA = -1;
		int remainB = -1;

		for(int i = 0; i < 3; i++)
		{
			bool iNotInBotInds = true;
			for(int j = 0; j < 3; j++)
			{
				if(topInds[i] == botInds[j]) iNotInBotInds = false;
			}

			if(iNotInBotInds == true) // then i is top left!
			{
				_mainInds[TP_LFT] = i;
				remainA = (i+1)%3;
				remainB = (i+2)%3;
			}

		}

		// Which is bottom right?
		for(int i = 0; i < 3; i++)
		{
			bool iNotInTopInds = true;
			for(int j = 0; j < 3; j++)
			{
				if(botInds[i] == topInds[j]) iNotInTopInds = false;
			}
			
			if(iNotInTopInds == true) // then i is bottom right!
				_mainInds[BT_RGT] = i;
		}

		// Other remaining - we know they are top right and bottom left but which is which??
		Vector3 norm = _mesh.normals[_mainInds[TP_LFT]];
		Vector3 vertTL = _mesh.vertices[_mainInds[TP_LFT]];
		Vector3 vertA = _mesh.vertices[remainA];
		Vector3 vertB = _mesh.vertices[remainB];

		if(Mathf.Abs (norm.x) == 1 || Mathf.Abs (norm.z) == 1)
		{
			// Top right is the one closest to topLeft in the "y"
			if(vertA.y - vertTL.y < vertB.y - vertTL.y)
			{
				_mainInds[TP_RGT] = remainA;
				_mainInds[BT_LFT] = remainB;
			}
			else
			{
				_mainInds[TP_RGT] = remainB;
				_mainInds[BT_LFT] = remainA;
			}
		}
		else
		{
			// Top right is the one closest to topLeft in the "x"
			if(vertA.x - vertTL.x < vertB.x - vertTL.x)
			{
				_mainInds[TP_RGT] = remainA;
				_mainInds[BT_LFT] = remainB;
			}
			else
			{
				_mainInds[TP_RGT] = remainB;
				_mainInds[BT_LFT] = remainA;
			}
		}
	}
	*/
}
