using UnityEngine;
using System.Collections;

public class Edge
{
	private Vector3[] _verts = new Vector3[2];
	public Vector3[] verts { get { return _verts; } }

	public Edge() { Set(Vector3.zero, Vector3.zero); } // default constructor sets both verts to zero
	public Edge(Vector3 vertA, Vector3 vertB) { Set (vertA, vertB); } // ...or set them as args

	public void Set(Vector3 vertA, Vector3 vertB) { _verts[0] = vertA; _verts[1] = vertB; }

	public static float DistanceToEdge(Edge edge, Vector3 point)
	{
		return DistanceToEdge(edge.verts[0], edge.verts[1], point);
	}

	public static float DistanceToEdge(Vector3 start, Vector3 end, Vector3 point)
	{
		Vector3 lineDirection = Vector3.Normalize(end - start);
		Vector3 startToPoint = point - start;

		return Vector3.Cross(lineDirection, startToPoint).magnitude;
	}


	// Get an "edge normal" based on side
	// NOTE Only works for front and back fzces becasue that's all that is needed for plane editor, if you need other faces add later
	public static Vector3 GetEdgeNormal(Vector3 faceNormal, int side)
	{
		int zDir = -Mathf.RoundToInt(faceNormal.z);
		if(zDir == 0)
		{
			Debug.LogError("Tried to get Edge normal for face other than front or back - you need to implement that shit first!");
			return Vector3.zero;
		}

		// top, right, bottom or left? (0-3 respectively)
		switch(side)
		{
		case 0: return Vector3.up;
		case 1: return Vector3.right * zDir;
		case 2: return Vector3.down;
		case 3: return Vector3.left * zDir;
		default: return Vector3.zero;
		}
	}

	// Get an "edge normal" based on side relative to transform
	public static Vector3 GetEdgeNormal(Vector3 faceNormal, int side, Transform relativeTo)
	{
		int zDir = -Mathf.RoundToInt(faceNormal.z);
		if(zDir == 0)
		{
			Debug.LogError("Tried to get Edge normal for face other than front or back - you need to implement that shit first!");
			return Vector3.zero;
		}

		// top, right, bottom or left? (0-3 respectively)
		switch(side)
		{
		case 0: return relativeTo.up;
		case 1: return relativeTo.right * zDir;
		case 2: return -relativeTo.up;
		case 3: return -relativeTo.right * zDir;
		default: return Vector3.zero;
		}
	}
	
}
