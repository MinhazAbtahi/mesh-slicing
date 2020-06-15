using UnityEngine;
using BzKovSoft.ObjectSlicer;
using System;
using System.Collections;
using BzKovSoft.ObjectSlicerSamples;
using DG.Tweening;

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
    private float moveY;
    [SerializeField]
    private float speed = 10f;
    [SerializeField]
    public float currentSpeed;
    [SerializeField]
    private float smoothing = 0.05f;
    private bool firsMove;

    public bool sowrdUp=true;
    public bool sowrdDown=false;

    private void Start()
    {
        currentSpeed = speed;
#if UNITY_EDITOR
        currentSpeed = 30;  
        
#endif

      
        bladeTransform = _blade.transform;
        knife = _blade.GetComponentInChildren<BzKnife>();
    }

    void Update()
    {
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

        if (Input.GetMouseButtonDown(0))
        {
            if(!firsMove)
            {
                firsMove = true;
                objectManager.MoveForward();
            }
        }

        if (Input.GetMouseButton(0))
        {
            //knife.BeginNewSlice();

            float y = Input.GetAxis("Mouse Y");
            moveY = Mathf.Lerp(moveY, y, smoothing);
            Vector3 deviation = new Vector3(0f, moveY * Time.deltaTime * currentSpeed, 0f);
            bladeTransform.position += deviation;
            if (bladeTransform.position.y > 10.5f)
            {
                bladeTransform.position = new Vector3(bladeTransform.position.x, maxY, bladeTransform.position.z);
            }
            else if (bladeTransform.position.y < 7.6f)
            {
                bladeTransform.position = new Vector3(bladeTransform.position.x, minY, bladeTransform.position.z);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            currentSpeed = speed;
#if UNITY_EDITOR
            currentSpeed = 30;

#endif

            KnifeUp();
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

    private void KnifeDown()
    {
        objectManager.StopMoving();
        bladeTransform.DOKill();
        bladeTransform.DOMoveY(minY, duration);
    }

    public void KnifeUp()
    {
        bladeTransform.DOKill();
        bladeTransform.DOMoveY(maxY, duration/2f).SetEase(Ease.OutBack).OnComplete(()=>
        {
            objectManager.MoveForward();
        });
    }
}