﻿using UnityEngine;
using BzKovSoft.ObjectSlicer;
using System.Diagnostics;
using System;
using System.Collections;

namespace BzKovSoft.ObjectSlicerSamples
{
	/// <summary>
	/// Test class for demonstration purpose
	/// </summary>
	public class SampleKnifeSlicer : MonoBehaviour
	{
#pragma warning disable 0649
		[SerializeField]
		private GameObject _blade;
#pragma warning restore 0649

		void Update()
		{
			if (Input.GetMouseButtonDown(0))
			{
				var knife = _blade.GetComponentInChildren<BzKnife>();
				knife.BeginNewSlice();

				StartCoroutine(SwingSword());
			}
		}

		IEnumerator SwingSword()
		{
            var transformB = _blade.transform;
            transformB.position = Camera.main.transform.position;
            transformB.rotation = Camera.main.transform.rotation;

            const float seconds = .5f;
			for (float f = 0f; f < seconds; f += Time.deltaTime)
			{
				float aY = (f / seconds) * 180 - 90;
				//float aX = (f / seconds) * 60 - 30;
				float aX = (f / seconds) * 90;

				//var r = Quaternion.Euler(aX, -aY, 0);
				var r = Quaternion.Euler(aX, 0, 0);

				transformB.rotation = Camera.main.transform.rotation * r;
				yield return null;
			}
		}
	}
}