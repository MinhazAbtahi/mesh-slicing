
using UnityEngine;

public enum MegaWeightChannel
{
	Red,
	Green,
	Blue,
	Alpha,
	None,
}

public enum MegaModChannel
{
	None		= 0,
	Verts		= 1,
	UV			= 2,
	UV1			= 4,
	UV2			= 8,
	Normals		= 16,
	Tris		= 32,
	Col			= 64,
	Selection	= 128,
	All			= 32767,
}

[RequireComponent(typeof(MeshModifyObject))]
public class MeshModifier : MonoBehaviour
{
	[HideInInspector]
	public bool			ModEnabled		= true;
	[HideInInspector]
	public bool			DisplayGizmo	= true;
	[HideInInspector]
	public int			Order			= -1;
	[HideInInspector]
	public Vector3		Offset			= Vector3.zero;
	[HideInInspector]
	public Vector3		gizmoPos		= Vector3.zero;
	[HideInInspector]
	public Vector3		gizmoRot		= Vector3.zero;
	[HideInInspector]
	public Vector3		gizmoScale		= Vector3.one;
	[HideInInspector]
	public Color		gizCol1			= Color.yellow;
	[HideInInspector]
	public Color		gizCol2			= Color.green;
	[HideInInspector]
	[System.NonSerialized]
	public Matrix4x4	tm				= new Matrix4x4();
	[System.NonSerialized]
	public Matrix4x4	invtm			= new Matrix4x4();
	//[HideInInspector]
	public MeshBoundingBox		bbox			= new MeshBoundingBox();
	[HideInInspector]
	public Vector3[]	corners			= new Vector3[8];

	//[HideInInspector]
	//public bool					useWeights = false;
	//[HideInInspector]
	//public MegaWeightChannel	weightChannel	= MegaWeightChannel.Red;

	[HideInInspector]
	public int steps = 50;	// How many steps for the gizmo boxes

	// new for mt
	[HideInInspector]
	public Vector3[]	verts;
	[HideInInspector]
	public Vector3[]	sverts;
	[HideInInspector]
	public bool			valid;

	[HideInInspector]
	public float[]	selection;

	[HideInInspector]
	public MeshModifier	instance;	// For groups this is the mod to use will be same type

	public bool limitchandisplay = false;
	public int startchannel = 0;
	public int displaychans = 10;

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5_0 || UNITY_5_1 || UNITY_5 || UNITY_2017 || UNITY_2018 || UNITY_2019
	public bool	useUndo = false;
#else
	public bool	useUndo = true;
#endif

	[HideInInspector]
	public string       Label            = "";
	[HideInInspector]
	public int          MaxLOD           = 0;

	public virtual MegaModChannel	ChannelsReq()		{ return MegaModChannel.Verts; }
	public virtual MegaModChannel	ChannelsChanged()	{ return MegaModChannel.Verts; }

	public virtual float	GizmoSize()					{ return bbox.Radius() * 0.05f; }
	public virtual void		ModStart(MeshModifiers ms)	{ }
	public virtual void		ModUpdate()					{ }
	public virtual bool		ModLateUpdate(ModContext mc) { return true; }	// TODO: Do we need mc now?
	public virtual Vector3	Map(int i, Vector3 p)		{ return p; }
	public virtual void		ShowGUI()					{ }
	public virtual string	ModName()					{ return "Missing Name"; }
	public virtual bool		InitMod(MeshModifiers mc)	{ return true; }
	public virtual bool		Prepare(ModContext mc)	{ return true; }
	public virtual void		ModEnd(MeshModifiers ms)	{ }
	public virtual string	GetHelpURL() { return "?page_id=377"; }
	
	public virtual void		PrepareMT(MeshModifiers mc, int cores)	{ }

	public virtual void		DoneMT(MeshModifiers mc)	{}

	public virtual void SetValues(MeshModifier mod)	{}

	public virtual bool		CanThread()	{ return true; }

	// Used for copying and prefabs
	public virtual void	Copy(MeshModifier dst)
	{
		dst.Label			= Label;
		dst.MaxLOD			= MaxLOD;
		dst.ModEnabled		= ModEnabled;
		dst.DisplayGizmo	= DisplayGizmo;
		dst.Order			= Order;
		dst.Offset			= Offset;
		dst.gizmoPos		= gizmoPos;
		dst.gizmoRot		= gizmoRot;
		dst.gizmoScale		= gizmoScale;
		dst.gizCol1			= gizCol1;
		dst.gizCol2			= gizCol2;
	}

	public virtual void PostCopy(MeshModifier dst)
	{
	}

	public virtual void DoWork(MeshModifiers mc, int index, int start, int end, int cores)
	{
		//if ( useWeights )

		if ( selection != null )
		{
			DoWorkWeighted(mc, index, start, end, cores);
			return;
		}

		for ( int i = start; i < end; i++ )
			sverts[i] = Map(i, verts[i]);
	}

	public virtual void DoWorkWeighted(MeshModifiers mc, int index, int start, int end, int cores)
	{
		for ( int i = start; i < end; i++ )
		{
			Vector3 p = verts[i];

			float w = selection[i];	//[(int)weightChannel];

			if ( w > 0.001f )
			{
				Vector3 mp = Map(i, verts[i]);

				sverts[i].x = p.x + (mp.x - p.x) * w;
				sverts[i].y = p.y + (mp.y - p.y) * w;
				sverts[i].z = p.z + (mp.z - p.z) * w;
			}
			else
				sverts[i] = p;	//verts[i];
		}
	}

	// This is never called
	void Awake()
	{
		MeshModifyObject modobj = (MeshModifyObject)gameObject.GetComponent<MeshModifyObject>();

		if ( modobj != null )
			modobj.ModReset(this);
	}

	void Reset()
	{
		MeshModifyObject modobj = (MeshModifyObject)gameObject.GetComponent<MeshModifyObject>();

		if ( modobj != null )
			modobj.ModReset(this);
	}

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/" + GetHelpURL());
	}

	[ContextMenu("Reset Offset")]
	public void ResetOffset()
	{
		Offset = Vector3.zero;
	}

	Vector3 GetCentre()
	{
		MeshModifyObject modobj = (MeshModifyObject)gameObject.GetComponent<MeshModifyObject>();

		if ( modobj != null && modobj.cachedMesh != null )
			return modobj.cachedMesh.bounds.center;

		return Vector3.zero;
	}

	[ContextMenu("Reset GizmoPos")]
	public void ResetGizmoPos()
	{
		gizmoPos = Vector3.zero;
	}

	[ContextMenu("Reset GizmoRot")]
	public void ResetGizmoRot()
	{
		gizmoRot = Vector3.zero;
	}

	[ContextMenu("Reset GizmoScale")]
	public void ResetGizmoScale()
	{
		gizmoScale = Vector3.one;
	}

	[ContextMenu("Center Offset")]
	public void CentreOffset()
	{
		Offset = -GetCentre();
	}

	[ContextMenu("Center GizmoPos")]
	public void CentreGizmoPos()
	{
		gizmoPos = -GetCentre();
	}

	// TODO: This is wrong, Offset should be 0
	public void SetModMesh(Mesh ms)
	{
		if ( ms != null )
		{
			Bounds b = ms.bounds;
			//Offset = -b.center;
			bbox.min = b.center - b.extents;
			bbox.max = b.center + b.extents;
			verts = ms.vertices;
			MeshChanged();
		}
	}

	public virtual void MeshChanged()
	{
	}

	public void SetTM()
	{
		tm = Matrix4x4.identity;
		Quaternion rot = Quaternion.Euler(-gizmoRot);

		tm.SetTRS(gizmoPos + Offset, rot, gizmoScale);
		invtm = tm.inverse;
	}

	public void SetTM(Vector3 off)
	{
		tm = Matrix4x4.identity;
		Quaternion rot = Quaternion.Euler(-gizmoRot);

		tm.SetTRS(gizmoPos + off, rot, gizmoScale);
		invtm = tm.inverse;
	}

	public void SetAxis(Matrix4x4 tmAxis)
	{
		Matrix4x4 itm = tmAxis.inverse;
		tm = tmAxis * tm;
		invtm = invtm * itm;
	}

	public virtual void Modify(ref Vector3[] sverts, ref Vector3[] verts)
	{
		for ( int i = 0; i < verts.Length; i++ )
			sverts[i] = Map(i, verts[i]);
	}

	public virtual void Modify(Vector3[] sverts, Vector3[] verts)
	{
		for ( int i = 0; i < verts.Length; i++ )
			sverts[i] = Map(i, verts[i]);
	}

	public virtual void Modify(MeshModifiers mc)
	{
		if ( verts != null )
		{
			for ( int i = 0; i < verts.Length; i++ )
				sverts[i] = Map(i, verts[i]);
		}
	}

	// Weighted version
	// Only be here if weights are being used
	public virtual void ModifyWeighted(MeshModifiers mc)
	{
		for ( int i = 0; i < verts.Length; i++ )
		{
			Vector3 p = verts[i];

			float w = mc.selection[i];

			if ( w > 0.001f )
			{
				Vector3 mp = Map(i, verts[i]);

				sverts[i].x = p.x + (mp.x - p.x) * w;
				sverts[i].y = p.y + (mp.y - p.y) * w;
				sverts[i].z = p.z + (mp.z - p.z) * w;
			}
			else
				sverts[i] = verts[i];
		}
	}

	public void DrawEdge(Vector3 p1, Vector3 p2)
	{
		Vector3 last = Map(-1, p1);
		Vector3 pos = Vector3.zero;
		for ( int i = 1; i <= steps; i++ )
		{
			pos = p1 + ((p2 - p1) * ((float)i / (float)steps));

			pos = Map(-1, pos);
			if ( (i & 4) == 0 )
				Gizmos.color = gizCol1;
			else
				Gizmos.color = gizCol2;

			Gizmos.DrawLine(last, pos);
			last = pos;
		}
		Gizmos.color = gizCol1;
	}

	public void DrawEdgeCol(Vector3 p1, Vector3 p2)
	{
		Vector3 last = Map(-1, p1);
		Vector3 pos = Vector3.zero;
		for ( int i = 1; i <= steps; i++ )
		{
			pos = p1 + ((p2 - p1) * ((float)i / (float)steps));

			pos = Map(-1, pos);

			Gizmos.DrawLine(last, pos);
			last = pos;
		}
	}

	// TODO: If we draw like warps do we know if we are the current edited script?
	public virtual void DrawGizmo(ModContext context)
	{
		tm = Matrix4x4.identity;
		MeshMatrix.Translate(ref tm, context.Offset);
		invtm = tm.inverse;

		if ( !Prepare(context) )
			return;

		Vector3 min = context.bbox.min;
		Vector3 max = context.bbox.max;

		Matrix4x4 gtm = Matrix4x4.identity;
		Vector3 pos = gizmoPos;
		pos.x = -pos.x;
		pos.y = -pos.y;
		pos.z = -pos.z;

		Vector3 scl = gizmoScale;
		scl.x = 1.0f - (scl.x - 1.0f);
		scl.y = 1.0f - (scl.y - 1.0f);
		gtm.SetTRS(pos, Quaternion.Euler(gizmoRot), scl);

		// put sourceObj into context
		if ( context.mod.sourceObj != null )
			Gizmos.matrix = context.mod.sourceObj.transform.localToWorldMatrix * gtm;
		else
			Gizmos.matrix = context.go.transform.localToWorldMatrix * gtm;

		//Gizmos.color = ModCol();	//Color.yellow;
		corners[0] = new Vector3(min.x, min.y, min.z);
		corners[1] = new Vector3(min.x, max.y, min.z);
		corners[2] = new Vector3(max.x, max.y, min.z);
		corners[3] = new Vector3(max.x, min.y, min.z);

		corners[4] = new Vector3(min.x, min.y, max.z);
		corners[5] = new Vector3(min.x, max.y, max.z);
		corners[6] = new Vector3(max.x, max.y, max.z);
		corners[7] = new Vector3(max.x, min.y, max.z);

		DrawEdge(corners[0], corners[1]);
		DrawEdge(corners[1], corners[2]);
		DrawEdge(corners[2], corners[3]);
		DrawEdge(corners[3], corners[0]);

		DrawEdge(corners[4], corners[5]);
		DrawEdge(corners[5], corners[6]);
		DrawEdge(corners[6], corners[7]);
		DrawEdge(corners[7], corners[4]);

		DrawEdge(corners[0], corners[4]);
		DrawEdge(corners[1], corners[5]);
		DrawEdge(corners[2], corners[6]);
		DrawEdge(corners[3], corners[7]);

		ExtraGizmo(context);
	}

	public virtual void ExtraGizmo(ModContext mc)
	{
	}

	public void DrawFromTo(BendAxis axis, float from, float to, ModContext mc)
	{
		Vector3 min = mc.bbox.min;
		Vector3 max = mc.bbox.max;

		switch ( axis )
		{
			case BendAxis.X:
				corners[0] = new Vector3(-from, min.y, min.z);
				corners[1] = new Vector3(-from, max.y, min.z);
				corners[2] = new Vector3(-from, max.y, max.z);
				corners[3] = new Vector3(-from, min.y, max.z);

				corners[4] = new Vector3(-to, min.y, min.z);
				corners[5] = new Vector3(-to, max.y, min.z);
				corners[6] = new Vector3(-to, max.y, max.z);
				corners[7] = new Vector3(-to, min.y, max.z);
				break;

			case BendAxis.Y:
				corners[0] = new Vector3(min.x, min.y, -from);
				corners[1] = new Vector3(min.x, max.y, -from);
				corners[2] = new Vector3(max.x, max.y, -from);
				corners[3] = new Vector3(max.x, min.y, -from);

				corners[4] = new Vector3(min.x, min.y, -to);
				corners[5] = new Vector3(min.x, max.y, -to);
				corners[6] = new Vector3(max.x, max.y, -to);
				corners[7] = new Vector3(max.x, min.y, -to);
				break;

			case BendAxis.Z:
				corners[0] = new Vector3(min.x, from, min.z);
				corners[1] = new Vector3(min.x, from, max.z);
				corners[2] = new Vector3(max.x, from, max.z);
				corners[3] = new Vector3(max.x, from, min.z);

				corners[4] = new Vector3(min.x, to, min.z);
				corners[5] = new Vector3(min.x, to, max.z);
				corners[6] = new Vector3(max.x, to, max.z);
				corners[7] = new Vector3(max.x, to, min.z);
				break;
		}

		Color c = Color.red;
		c.a = gizCol1.a;
		Gizmos.color = c;

		Vector3 offset = Vector3.zero;	//mc.Offset;

		DrawEdgeCol(corners[0] - offset, corners[1] - offset);
		DrawEdgeCol(corners[1] - offset, corners[2] - offset);
		DrawEdgeCol(corners[2] - offset, corners[3] - offset);
		DrawEdgeCol(corners[3] - offset, corners[0] - offset);

		c = Color.green;
		c.a = gizCol1.a;
		Gizmos.color = c;

		DrawEdgeCol(corners[4] - offset, corners[5] - offset);
		DrawEdgeCol(corners[5] - offset, corners[6] - offset);
		DrawEdgeCol(corners[6] - offset, corners[7] - offset);
		DrawEdgeCol(corners[7] - offset, corners[4] - offset);
	}

	void OnDrawGizmosSelected()
	{
	}
}