﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using BzKovSoft;
using UnityEngine.SceneManagement;
using JellyCube;

public class ObjectManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> sliceObjects;
    [SerializeField]
    private GameObject sliceObject;
    [SerializeField]
    private float targetPositionX;
    [SerializeField]
    private float step;
    private bool isMoving;
    public int totalMove = 0;
    public bool isGameOver;
    public bool isGameStart;
    public int levelsCount = 0;

    public GameObject ui;

    // Start is called before the first frame update
    void Start()
    {
        levelsCount = PlayerPrefs.GetInt("LevelsCount", 0);
        sliceObject = sliceObjects[levelsCount];
        sliceObject.transform.position = new Vector3(0f, sliceObject.transform.position.y, 0f);
        sliceObject.SetActive(true);

        sliceObject.transform.DOMoveX(targetPositionX, 1f).OnComplete(()=>
        {
            sliceObject.GetComponent<RubberEffect>().enabled = true;
            //sliceObject.GetComponent<RubberEffect>()
            isGameStart = true;
        });
        step = sliceObject.transform.localScale.x / 5f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveForward()
    {
        if (totalMove <= 5)
        {
            targetPositionX += step;
            //targetPositionX += sliceObject.transform.localScale.x;
            StartCoroutine(MoveRoutine());
        }
    }

    public void StopMoving()
    {
        sliceObject.transform.DOKill();
        //StopAllCoroutines();
    }

    private IEnumerator MoveRoutine()
    {
        ++totalMove;
        if (totalMove >= 6)
        {
            isGameStart = false;
            isGameOver = true;
            ++levelsCount;
            if (levelsCount >= sliceObjects.Count)
            {
                levelsCount = levelsCount % (sliceObjects.Count);
            }
            PlayerPrefs.SetInt("LevelsCount", levelsCount);
        }

        yield return new WaitForSeconds(.1f);
        isMoving = true;
        sliceObject.transform.DOMoveX(targetPositionX, 2f).OnComplete(()=>
        {
            ui.SetActive(isGameOver);
        });
    }

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
