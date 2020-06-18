using System;
using System.Collections.Generic;
using UnityEngine;

namespace BzKovSoft.ObjectSlicerSamples
{
	/// <summary>
	/// The script must be attached to a GameObject that have collider marked as a "IsTrigger".
	/// </summary>
	public class BzKnife : MonoBehaviour
	{
        public PlayerController playerController;
        public GameObject sliceobject;

		public int SliceID { get; private set; }
		Vector3 _prevPos;
		Vector3 _pos;

		[SerializeField]
		private Vector3 _origin = Vector3.down;

		[SerializeField]
		private Vector3 _direction = Vector3.up;

		private void Update()
		{
			_prevPos = _pos;
			_pos = transform.position;
		}

		public Vector3 Origin
		{
			get
			{
				Vector3 localShifted = transform.InverseTransformPoint(transform.position) + _origin;
				return transform.TransformPoint(localShifted);
			}
		}

		public Vector3 BladeDirection { get { return transform.rotation * _direction.normalized; } }
		public Vector3 MoveDirection { get { return (_pos - _prevPos).normalized; } }

		public void BeginNewSlice()
		{
			SliceID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
		}

        private void OnTriggerEnter(Collider other)
        {
            //Debug.Log("kinfe lagse "+ other.tag + " er sathe");




            if (other.tag == "Object")
            {


                playerController.objectManager.StopMoving();
                //playerController.currentSpeed = playerController.currentSpeed / 2;
                //Debug.Log("object thamay rakhse");
                sliceobject = other.gameObject;

                playerController.bendingOn = true;






            }

            if (other.tag == "table")
            {

                
                playerController.sowrdDown = true;
                //sliceobject.GetComponent<KnifeSliceableAsync>().knifedowTriggered();
                //playerController.KnifeUp();
               
                //playerController.bendingOn = false;
                playerController.bendAngle = 0;
                playerController.prevbendAngle = 0;
                playerController.bendAngleForSqure = 0;
                playerController.objectManager.repairRigidTrigger();

                playerController.objectManager.slicePieces = new List<GameObject>();
                //playerController.objectManager.oldSlicePieces = new List<GameObject>();

                playerController.knifeAutoMoveUp();
#if !UNITY_EDITOR && UNITY_ANDROID

            Vibration.Vibrate(80);
#endif

            }

        }
        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Object")
            {
              
                playerController.bendingOn = false;
                playerController.prevdeviation = playerController.swordposY;
            }
        }



    }
}
