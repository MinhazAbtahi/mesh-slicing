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
    private float maxY = 10.5f;
    [SerializeField]
    private float minY = 7.5f;
    [SerializeField]
    private float duration = .5f;
    private bool gone;

    private void Start()
    {
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
                bladeTransform.DOMoveY(maxY * 2, duration * 2);
            }
            return;
        }

        if (!objectManager.isGameStart)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            //objectManager.MoveForward();
        }

        if (Input.GetMouseButton(0))
        {
            //knife.BeginNewSlice();

            KnifeDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
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

    private void KnifeUp()
    {
        bladeTransform.DOKill();
        bladeTransform.DOMoveY(maxY, duration/2f).OnComplete(()=>
        {
            objectManager.MoveForward();
        });
    }
}