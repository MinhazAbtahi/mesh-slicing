﻿using BzKovSoftSlice.ObjectSlicer;
using BzKovSoftSlice.ObjectSlicer.MeshGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;
using DG.Tweening;
using System.Collections;

namespace BzKovSoftSlice.ObjectSlicer
{
  
    class StaticComponentManager : IComponentManager
	{
		protected readonly GameObject _originalObject;
		protected readonly Plane _plane;
		protected readonly ColliderSliceResult[] _colliderResults;

       


        //sakib modify list for rigidbody objects
        public List<GameObject> slicePieces;
        public GameObject objectManager;

        public bool Success { get { return _colliderResults != null; } }

		public StaticComponentManager(GameObject go, Plane plane, Collider[] colliders)
		{
			_originalObject = go;
			_plane = plane;

			_colliderResults = SliceColliders(plane, colliders);
		}

		public void OnSlicedWorkerThread(SliceTryItem[] items)
		{
			for (int i = 0; i < _colliderResults.Length; i++)
			{
				var collider = _colliderResults[i];

				if (collider.SliceResult == SliceResult.Sliced)
				{
					collider.SliceResult = collider.meshDissector.Slice();
				}
			}
		}

		public void OnSlicedMainThread(GameObject resultObjNeg, GameObject resultObjPos, Renderer[] renderersNeg, Renderer[] renderersPos)
		{
			var cldrsA = new List<Collider>();
			var cldrsB = new List<Collider>();
           
            RepairColliders(resultObjNeg, resultObjPos, cldrsA, cldrsB);
            RepairRigidbody(resultObjNeg);
            //AddModifier(resultObjNeg);

            //sakib modification for reference to obejctmanager
            objectManager = GameObject.FindGameObjectWithTag("objectManager");
            objectManager.GetComponent<ObjectManager>().addSlices(resultObjNeg);
            //objectManager.GetComponent<ObjectManager>().pc.bendingOn = true;
            //slicePieces = objectManager.GetComponent<ObjectManager>().slicePieces;
        }

       

        private void AddModifier(GameObject resultObjNeg)
        {

            //MeshModifyObject bendModify = resultObjNeg.AddComponent<MeshModifyObject>();
            //MeshModifiers bendModify = resultObjNeg.AddComponent<MeshModifiers>();

            //resultObjNeg.AddComponent<MegaModifier>();
            //resultObjNeg.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            //resultObjNeg.GetComponent<MeshFilter>().mesh.RecalculateNormals();
            //resultObjNeg.GetComponent<MeshFilter>().mesh.RecalculateTangents();
           
            //MeshBend bend = resultObjNeg.AddComponent<MeshBend>();


            //MeshBend bend = resultObjNeg.GetComponent<MeshBend>();
            //bend.ModEnabled = true;

            //bend.ModUpdate();
            ////bend.doRegion = true;
            //bend.ResetOffset();
           
            //bend.axis = BendAxis.Z;
           

            // TO-DO: Procedurally bend angles based on knife input 
        }

        private void RepairRigidbody(GameObject resultObjNeg)
        {
            Rigidbody resultNegRigidbody = resultObjNeg.GetComponent<Rigidbody>();
           
            resultNegRigidbody.isKinematic = true;
            //resultNegRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            //resultObjNeg.AddComponent<JellyCube.RubberEffect>();
            //resultNegRigidbody.AddForceAtPosition(Vector3.right * 2.5f, -Vector3.down * 2f, ForceMode.VelocityChange);
        }

        protected void RepairColliders(GameObject resultNeg, GameObject resultPos,
			List<Collider> collidersNeg, List<Collider> collidersPos)
		{
			Profiler.BeginSample("RepairColliders");
			var lazyRunnerNeg = resultNeg.GetComponent<LazyActionRunner>();
			var lazyRunnerPos = resultPos.GetComponent<LazyActionRunner>();

			for (int i = 0; i < _colliderResults.Length; i++)
			{
				var collider = _colliderResults[i];

				Collider colliderNeg = BzSlicerHelper.GetSameComponentForDuplicate(collider.OriginalCollider, _originalObject, resultNeg);
				Collider colliderPos = BzSlicerHelper.GetSameComponentForDuplicate(collider.OriginalCollider, _originalObject, resultPos);
				var goNeg = colliderNeg.gameObject;
				var goPos = colliderPos.gameObject;

				if (collider.SliceResult == SliceResult.Sliced)
				{
					Mesh resultMeshNeg = collider.meshDissector.SliceResultNeg.GenerateMesh();
					Mesh resultMeshPos = collider.meshDissector.SliceResultPos.GenerateMesh();

					var SlicedColliderNeg = new MeshColliderConf(resultMeshNeg, collider.OriginalCollider.material);
					var SlicedColliderPos = new MeshColliderConf(resultMeshPos, collider.OriginalCollider.material);

					AddCollider(SlicedColliderNeg, goNeg, lazyRunnerNeg, colliderNeg);
                    AddCollider(SlicedColliderPos, goPos, lazyRunnerPos, colliderPos);
				}

				if (collider.SliceResult == SliceResult.Sliced)
				{
					collidersNeg.Add(colliderNeg);
					collidersPos.Add(colliderPos);
				}
				else if (collider.SliceResult == SliceResult.Neg)
				{
					collidersNeg.Add(colliderNeg);
					UnityEngine.Object.Destroy(colliderPos);
				}
				else if (collider.SliceResult == SliceResult.Pos)
				{
					collidersPos.Add(colliderPos);
					UnityEngine.Object.Destroy(colliderNeg);
				}
				else
					throw new InvalidOperationException();
			}
			Profiler.EndSample();
		}

		private static void AddCollider(MeshColliderConf colliderConf, GameObject go, LazyActionRunner lazyRunner, Collider originalCollider)
		{
			Profiler.BeginSample("AddCollider");
			UnityEngine.Object.Destroy(originalCollider);


			var action = new Action(() =>
			{
				Profiler.BeginSample("Action: AddCollider");

				var collider = go.AddComponent<MeshCollider>();
				collider.sharedMaterial = colliderConf.Material;
				collider.sharedMesh = colliderConf.Mesh;
				//collider.inflateMesh = true;

				var convexResult = new ConvexSetResult();
				convexResult.SetConvex(collider);

				Profiler.EndSample();
			});

			lazyRunner.AddLazyAction(action);

			Profiler.EndSample();
		}

		private static ColliderSliceResult[] SliceColliders(Plane plane, Collider[] colliders)
		{
			ColliderSliceResult[] results = new ColliderSliceResult[colliders.Length];
			bool ColliderExistsNeg = false;
			bool ColliderExistsPos = false;

			for (int i = 0; i < colliders.Length; i++)
			{
				var collider = colliders[i];

				var colliderB = collider as BoxCollider;
				var colliderS = collider as SphereCollider;
				var colliderC = collider as CapsuleCollider;
				var colliderM = collider as MeshCollider;

				ColliderSliceResult result;
				if (colliderB != null)
				{
					var mesh = Cube.Create(colliderB.size, colliderB.center);
					result = PrepareSliceCollider(colliderB.center, collider, mesh, plane);
				}
				else if (colliderS != null)
				{
					var mesh = IcoSphere.Create(colliderS.radius, colliderS.center);
					result = PrepareSliceCollider(colliderS.center, collider, mesh, plane);
				}
				else if (colliderC != null)
				{
					var mesh = Capsule.Create(colliderC.radius, colliderC.height, colliderC.direction, colliderC.center);
					result = PrepareSliceCollider(colliderC.center, collider, mesh, plane);
				}
				else if (colliderM != null)
				{
					Mesh mesh = UnityEngine.Object.Instantiate(colliderM.sharedMesh);
					result = PrepareSliceCollider(Vector3.zero, collider, mesh, plane);
				}
				else
					throw new NotSupportedException("Not supported collider type '" + collider.GetType().Name + "'");

				ColliderExistsNeg |= result.SliceResult == SliceResult.Sliced | result.SliceResult == SliceResult.Neg;
				ColliderExistsPos |= result.SliceResult == SliceResult.Sliced | result.SliceResult == SliceResult.Pos;
				results[i] = result;
			}

			bool sliced = ColliderExistsNeg & ColliderExistsPos;
			return sliced ? results : null;
		}

		protected static ColliderSliceResult PrepareSliceCollider(Vector3 locPos, Collider collider, Mesh mesh, Plane plane)
		{
			var result = new ColliderSliceResult();
			IBzSliceAddapter adapter = new BzSliceColliderAddapter(mesh.vertices, collider.gameObject);
			BzMeshDataDissector meshDissector = new BzMeshDataDissector(mesh, plane, null, adapter, null);

			result.SliceResult = SliceResult.Sliced;
			result.OriginalCollider = collider;
			result.meshDissector = meshDissector;

			return result;
		}

		protected class ColliderSliceResult
		{
			public Collider OriginalCollider;
			public BzMeshDataDissector meshDissector;
			public SliceResult SliceResult;
		}

		protected class MeshColliderConf
		{
			public MeshColliderConf(Mesh mesh, PhysicMaterial material)
			{
				Mesh = mesh;
				Material = material;
			}
			public readonly Mesh Mesh;
			public readonly PhysicMaterial Material;
		}
	}
}