
using UnityEngine;

public enum BendAxis
{
    X = 0,
    Y = 1,
    Z = 2,
};

public class MeshBend : MeshModifier
{
	//[HideInInspector]
	public float	angle		= 0.0f;
	[HideInInspector]
	public float	dir			= 0.0f;
	//[HideInInspector]
	public BendAxis	axis		= BendAxis.X;
	[HideInInspector]
	public bool		doRegion	= false;
	[HideInInspector]
	public float	from		= 0.0f;
	[HideInInspector]
	public float	to			= 0.0f;
	Matrix4x4		mat			= new Matrix4x4();
	Matrix4x4		tmAbove		= new Matrix4x4();
	Matrix4x4		tmBelow		= new Matrix4x4();
	float			r			= 0.0f;
	float			oor			= 0.0f;

	public override string ModName()	{ return "Bend"; }
	public override string GetHelpURL() { return "?page_id=41"; }

	public override void SetValues(MeshModifier mod)
	{
		MeshBend bm = (MeshBend)mod;
		angle = bm.angle;
		dir = bm.dir;
		axis = bm.axis;
		doRegion = bm.doRegion;
		from = bm.from;
		to = bm.to;
	}

	void CalcR(BendAxis axis, float ang)
	{
		float len = 0.0f;

		if ( !doRegion )
		{
			switch ( axis )
			{
				case BendAxis.X: len = bbox.max.x - bbox.min.x; break;
				case BendAxis.Z: len = bbox.max.y - bbox.min.y; break;
				case BendAxis.Y: len = bbox.max.z - bbox.min.z; break;
			}
		}
		else
			len = to - from;

		if ( Mathf.Abs(ang) < 0.000001f )
			r = 0.0f;
		else
			r = len / ang;

		oor = 1.0f / r;
	}

	public override Vector3 Map(int i, Vector3 p)
	{
		if ( r == 0.0f && !doRegion )
			return p;

		p = tm.MultiplyPoint3x4(p);	// tm may have an offset gizmo etc

		if ( doRegion )
		{
			if ( p.y <= from )
				return invtm.MultiplyPoint3x4(tmBelow.MultiplyPoint3x4(p));
			else
			{
				if ( p.y >= to )
					return invtm.MultiplyPoint3x4(tmAbove.MultiplyPoint3x4(p));
			}
		}

		if ( r == 0.0f )
			return invtm.MultiplyPoint3x4(p);

		float x = p.x;
		float y = p.y;
		//float yr = y / r;
		float yr = Mathf.PI - (y * oor);

		//float c = Mathf.Cos(Mathf.PI - yr);
		//float s = Mathf.Sin(Mathf.PI - yr);
		float c = Mathf.Cos(yr);
		float s = Mathf.Sin(yr);
		//float px = r * c + r - x * c;
		p.x = r * c + r - x * c;

		//p.x = px;
		//float pz = r * s - x * s;
		p.y = r * s - x * s;
		//p.y = pz;
		return invtm.MultiplyPoint3x4(p);
		//return p;
	}

	void Calc()
	{
		if ( from > to)	from = to;
		if ( to < from ) to = from;

		mat = Matrix4x4.identity;

		switch ( axis )
		{
			case BendAxis.X: MeshMatrix.RotateZ(ref mat, Mathf.PI * 0.5f); break;
			case BendAxis.Y: MeshMatrix.RotateX(ref mat, -Mathf.PI * 0.5f); break;
			case BendAxis.Z: break;
		}

		MeshMatrix.RotateY(ref mat, Mathf.Deg2Rad * dir);
		SetAxis(mat);

		CalcR(axis, Mathf.Deg2Rad * -angle);

		if ( doRegion )
		{
			doRegion = false;
			float len  = to - from;
			float rat1, rat2;

			if ( len == 0.0f )
				rat1 = rat2 = 1.0f;
			else
			{
				rat1 = to / len;
				rat2 = from / len;
			}

			Vector3 pt;
			tmAbove = Matrix4x4.identity;
			MeshMatrix.Translate(ref tmAbove, 0.0f, -to, 0.0f);
			MeshMatrix.RotateZ(ref tmAbove, -Mathf.Deg2Rad * angle * rat1);
			MeshMatrix.Translate(ref tmAbove, 0.0f, to, 0.0f);
			pt = new Vector3(0.0f, to, 0.0f);
			MeshMatrix.Translate(ref tmAbove, tm.MultiplyPoint3x4(Map(0, invtm.MultiplyPoint3x4(pt))) - pt);

			tmBelow = Matrix4x4.identity;
			MeshMatrix.Translate(ref tmBelow, 0.0f, -from, 0.0f);
			MeshMatrix.RotateZ(ref tmBelow, -Mathf.Deg2Rad * angle * rat2);
			MeshMatrix.Translate(ref tmBelow, 0.0f, from, 0.0f);
			pt = new Vector3(0.0f, from, 0.0f);
			MeshMatrix.Translate(ref tmBelow, tm.MultiplyPoint3x4(Map(0, invtm.MultiplyPoint3x4(pt))) - pt);

			doRegion = true;
		}
	}

	public override bool ModLateUpdate(ModContext mc)
	{
		return Prepare(mc);
	}

	public override bool Prepare(ModContext mc)
	{
		Calc();
		return true;
	}

	public override void ExtraGizmo(ModContext mc)
	{
		if ( doRegion )
			DrawFromTo(axis, from, to, mc);
	}
}
