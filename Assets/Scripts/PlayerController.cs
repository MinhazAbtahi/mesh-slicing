using UnityEngine;
using BzKovSoft.ObjectSlicer;
using System;
using System.Collections;
using BzKovSoft.ObjectSlicerSamples;
using DG.Tweening;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public ObjectManager objectManager;

    [Header("Blade Prefab:")]
    [SerializeField]
    private GameObject _blade;
    private Transform bladeTransform;
    private BzKnife knife;

    [Space(5)]
    [Header("Cotroller Values:")]
    [SerializeField]
    private float maxY = 10.50f;
    [SerializeField]
    private float minY = 7.55f;
    [SerializeField]
    private float duration = .5f;
    private bool gone;
    public float moveY;
    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    public float currentSpeed;
    [SerializeField]
    private float smoothing = 0.05f;
    private bool firsMove;

    public bool sowrdUp=true;
    public bool sowrdDown=false;

    private GameObject slice;
    public float bendAngle;
    public float prevbendAngle;
    public bool bendingOn;
    public float bendAngleForSqure;
    public float prevdeviation;
    public float swordposY;

    private void Start()
    {
       
        bendAngle = 0;
        bendAngleForSqure = 0;
        prevbendAngle = 0;
        bendingOn = false;
       currentSpeed = speed;
#if UNITY_EDITOR
        currentSpeed = 20;  
        
#endif

      
        bladeTransform = _blade.transform;
        knife = _blade.GetComponentInChildren<BzKnife>();
        swordposY = bladeTransform.position.y;
        prevdeviation = swordposY;
    }

    void Update()
    {
        //swordpos update
        swordposY= bladeTransform.position.y;

        if (objectManager.isGameOver)
        {
            if (!gone)
            {
                gone = true;
                bladeTransform.DOMoveY(maxY * 2, duration * 2).SetEase(Ease.InBack);
            }
            return;
        }

        if (!objectManager.isGameStart)
        {
            return;
        }

        //if (Input.GetMouseButtonDown(0))
        //{
        //    if(!firsMove)
        //    {
        //        firsMove = true;
        //        objectManager.MoveForward();
        //    }
        //}

        if (Input.GetMouseButton(0))
        {
            //knife.BeginNewSlice();

            float y = Input.GetAxis("Mouse Y");
            moveY = Mathf.Lerp(moveY, y, smoothing);
            Vector3 deviation = new Vector3(0f, moveY * Time.deltaTime * currentSpeed, 0f);
            bladeTransform.position += deviation;
            if (bladeTransform.position.y > 9.25f)
            {
                bladeTransform.position = new Vector3(bladeTransform.position.x, maxY, bladeTransform.position.z);
            }
            else if (bladeTransform.position.y < 7.6f)
            {
                bladeTransform.position = new Vector3(bladeTransform.position.x, minY, bladeTransform.position.z);
            }


            
            //bend value check
            if (/*deviation.y <= 0 &&*/ bendingOn)
            {
                bendAngle += deviation.y;
                bendAngleForSqure = bendAngle*2f;
                //bend sakib
                bendcheckAndBend();

            }



            //knife moving up cause object move
            if (swordposY > prevdeviation && !bendingOn)
            {
                KnifeUp();
                prevdeviation = swordposY;
            }
            else if (swordposY < prevdeviation && !bendingOn)
            {
                stopObjectmovement();
                prevdeviation = swordposY;
            }
           





        }

        if (Input.GetMouseButtonUp(0))
        {
            currentSpeed = speed;
#if UNITY_EDITOR
            currentSpeed = 30;

#endif

            //KnifeUp();
            knifeAutoMoveUp();
        }
    }

    IEnumerator SwingSword()
    {
        var transformB = _blade.transform;
        //transformB.position = Camera.main.transform.position;
        //transformB.rotation = Camera.main.transform.rotation;

        const float seconds = .5f;
        for (float f = 0f; f < seconds; f += Time.deltaTime)
        {
            float aY = (f / seconds) * 180 - 90;
            //float aX = (f / seconds) * 60 - 30;
            float aX = (f / seconds) * 90;

            //var r = Quaternion.Euler(aX, -aY, 0);
            var r = Quaternion.Euler(-aX, 0, 0);

            transformB.rotation = /*Camera.main.transform.rotation **/ r;
            yield return null;
        }
    }

    private void bendcheckAndBend()
    {
        int count= objectManager.slicePieces.Count;
        
        //sakib modify bending
        if (count != 0)
        {
            //foreach (GameObject slice in objectManager.slicePieces)
            //{
            //    if (bendAngle < prevbendAngle)
            //    {
            //        slice.GetComponent<MeshBend>().angle = bendAngle;
            //        //Debug.Log("bend ammount"+ (bendAngle));
            //        prevbendAngle = bendAngle;
            //    }
            //}
            //}
            if (bendAngleForSqure < prevbendAngle)
            {
                //ager piece
                if (objectManager.oldSlicePieces.Count!=0)
                {
                    foreach (GameObject slice in objectManager.oldSlicePieces)
                    {
                        //objectManager.slicePieces[count - 1].transform.GetChild(0).GetComponent<MeshBend>().angle += bendAngleForSqure/5;
                        //slice.GetComponent<MeshBend>().angle += bendAngle / 3;
                        slice.GetComponent<CurveShapeDeformer>().Multiplier -= -.05f/*bendAngle / 20*/;
                    }



                }
                //objectManager.slicePieces[count - 1].GetComponent<MeshBend>().angle = Mathf.Pow(bendAngleForSqure, 5);
                objectManager.slicePieces[count - 1].GetComponent<CurveShapeDeformer>().Multiplier = /*-bendAngleForSqure*6f;*/Mathf.Pow(bendAngleForSqure*2, 2);
#if !UNITY_EDITOR && UNITY_ANDROID

            Vibration.Vibrate(20);
#endif
                prevbendAngle = bendAngleForSqure;
            }

            //foreach (GameObject slice in objectManager.slicePieces)
            //{
            //    if (bendAngleForSqure < prevbendAngle && slice!= objectManager.slicePieces[count - 1])
            //    {
            //        slice.GetComponent<MeshBend>().angle -= bendAngleForSqure;
            //        //Debug.Log("bend ammount"+ (bendAngle));
                    
            //    }
            //}
            //prevbendAngle = bendAngleForSqure;


        }
    }

    private void KnifeDown()
    {
        objectManager.StopMoving();
        bladeTransform.DOKill();
        bladeTransform.DOMoveY(minY, duration);
    }

    public void KnifeUp()
    {
        //setting prev piece ref
        if (objectManager.slicePieces.Count != 0)
        {
            objectManager.oldSlicePieces.Add(objectManager.slicePieces[0]);
        }
        objectManager.slicePieces = new List<GameObject>();

        //bladeTransform.DOKill();
        //bladeTransform.DOMoveY(maxY, duration/2f).SetEase(Ease.OutBack).OnComplete(()=>
        //{
        //    objectManager.MoveForward();
        //});


        bendAngle = 0;
        bendAngleForSqure = 0;
        prevbendAngle = 0;

        //new edit sakib for object movement
        objectManager.isMoving = true;
        //objectManager.MoveForward();


     

        
    }


    public void knifeAutoMoveUp()
    {
        bladeTransform.DOKill();
        bladeTransform.DOMoveY(maxY-.4f, duration / 2f).SetEase(Ease.OutBack);
    }

    public void stopObjectmovement()
    {
        objectManager.StopMoving();
        //bladeTransform.DOKill();
    }
}