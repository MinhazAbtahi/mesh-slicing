using UnityEngine;
using BzKovSoftSlice.ObjectSlicer;
using System;
using System.Collections;
using BzKovSoftSlice.ObjectSlicerSamples;
using DG.Tweening;
using System.Collections.Generic;

public class PlayerControllerSlice : MonoBehaviour
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

    public bool sowrdUp=false;
    public bool sowrdDown=false;

    private GameObject slice;
    public float bendAngle;
    public float prevbendAngle;
    public bool bendingOn;
    public float bendAngleForSqure;
    public float prevdeviation;
    public float swordposY;


    //adio
    public AudioSource astart;
    public AudioSource aend;
    public bool sliceOn=false;

    private void Start()
    {
        sowrdUp = false;
        bendAngle = 0;
        bendAngleForSqure = 0;
        prevbendAngle = 0;
        bendingOn = false;
       currentSpeed = speed;
#if UNITY_EDITOR
        currentSpeed = 10;  
        
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

        if (Input.GetMouseButton(0)&&!sowrdUp)
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
                //playSliceMusic();
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
            currentSpeed = 10;

#endif

            //KnifeUp();
            knifeAutoMoveUp();
        }

        if (sliceOn && !astart.isPlaying)
        {
            playSliceMusic();
            sliceOn = false;
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
               

                GameObject CurrentSlice = objectManager.slicePieces[count - 1];
                float massCurrent = CurrentSlice.GetComponent<Rigidbody>().mass;
                if (massCurrent < .075f)
                {
                    massCurrent = .075f;
                }
                else if (massCurrent > .15f)
                {
                    massCurrent = .15f;
                }
                float bendValue = Mathf.Pow(bendAngleForSqure * 2 / (massCurrent * 15), 2);
                //float bendValueNopower = bendAngleForSqure * 2 / (massCurrent * 15);
                //objectManager.slicePieces[count - 1].GetComponent<MeshBend>().angle = Mathf.Pow(bendAngleForSqure, 5);
                CurrentSlice.GetComponent<CurveShapeDeformer>().Multiplier = /*-bendAngleForSqure*6f;*/ bendValue;
                //CurrentSlice.transform.rotation=(Quaternion.Euler(0,0, -bendValue * 4.5f));
                //CurrentSlice.transform.position += new Vector3(-bendValue / 2000, bendValue / 900, 0);

                sliceOn = true;

                //ager piece
                if (objectManager.oldSlicePieces.Count!=0)
                {
                    foreach (GameObject slice in objectManager.oldSlicePieces)
                    {
                        //objectManager.slicePieces[count - 1].transform.GetChild(0).GetComponent<MeshBend>().angle += bendAngleForSqure/5;
                        //slice.GetComponent<MeshBend>().angle += bendAngle / 3;
                        if (slice.GetComponent<CurveShapeDeformer>().Multiplier< bendValue)
                        {
                            slice.GetComponent<CurveShapeDeformer>().Multiplier = bendValue;
                        }
                        else
                        {
                            slice.GetComponent<CurveShapeDeformer>().Multiplier -= -.01f/*bendAngle / 20*/;
                        }
                       
                       
                    }



                }

                //sliceOn = true;
                

                //#if !UNITY_EDITOR && UNITY_ANDROID

                //            Vibration.Vibrate(20);
                //#endif
                prevbendAngle = bendAngleForSqure;
            }

            else if (bendAngleForSqure > prevbendAngle){
                //Invoke("stopSliceMusic", 1);
                stopSliceMusic();
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
        bladeTransform.DOMoveY(maxY - .4f, duration / 2f).SetEase(Ease.OutBack).OnComplete(() =>
          { sowrdUp = false; });
    }

    public void stopObjectmovement()
    {
        objectManager.StopMoving();
        //bladeTransform.DOKill();
    }

    public void playEndMusic()
    {
        aend.Play();
        stopSliceMusic();
        sliceOn = false;
    }

    public void playSliceMusic()
    {
        astart.Play(0);
    }
    public void stopSliceMusic()
    {
        astart.Stop();
    }
}