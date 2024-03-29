﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BzKovSoftSlice.ObjectSlicer.MeshGenerator
{
	public class CapsuleTests
	{
		[Test]
		public void IntersectsPlane1()
		{
			var transform = new GameObject().transform;
			var result = Capsule.IntersectsPlane(transform, 10, 100, 1, Vector3.zero, new Plane(Vector3.up, -59f));

			Assert.AreEqual(SliceResult.Sliced, result);
		}

		[Test]
		public void IntersectsPlane2()
		{
			var transform = new GameObject().transform;	
			var result = Capsule.IntersectsPlane(transform, 10, 100, 1, Vector3.zero, new Plane(Vector3.up, 59f));

			Assert.AreEqual(SliceResult.Sliced, result);
		}

		[Test]
		public void IntersectsPlane3()
		{
			var transform = new GameObject().transform;
			var result = Capsule.IntersectsPlane(transform, 10, 100, 1, Vector3.zero, new Plane(Vector3.up, 61f));

			Assert.AreEqual(SliceResult.Pos, result);
		}
	}
}