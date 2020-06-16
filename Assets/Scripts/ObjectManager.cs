using System.Collections;
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
    //sakib modify list for rigidbody objects
    public List<GameObject> slicePieces;

    // Start is called before the first frame update
    void Start()
    {
        levelsCount =2 /*PlayerPrefs.GetInt("LevelsCount", 0)*/;
        sliceObject = sliceObjects[levelsCount];
        sliceObject.transform.position = new Vector3(0f, sliceObject.transform.position.y, 0f);
        sliceObject.SetActive(true);

        sliceObject.transform.DOMoveX(targetPositionX, 0).OnComplete(()=>
        {
            //sliceObject.GetComponent<RubberEffect>().enabled = true;
            //sliceObject.GetComponent<RubberEffect>()
            isGameStart = true;
        });
        step = sliceObject.transform.localScale.x / 5f;
    }

    public void addSlices(GameObject slice)
    {
        slicePieces.Add(slice);
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

    //sakib call for kinematic trigger on slice objects
    public void repairRigidTrigger()
    {

        //foreach (GameObject slice in slicePieces)
        //{
        //    Rigidbody rb = slice.GetComponent<Rigidbody>();
        //    rb.isKinematic = false;
           
        //}
        Rigidbody rblastPiece = slicePieces[slicePieces.Count-1].GetComponent<Rigidbody>();
        rblastPiece.isKinematic = false;
        
        rblastPiece.AddRelativeTorque(0, 0, -25);
        rblastPiece.AddForce(rblastPiece.mass * 20, 0, 0);


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

        yield return new WaitForSeconds(.25f);
        isMoving = true;
        sliceObject.transform.DOMoveX(targetPositionX, .5f).OnComplete(()=>
        {
            ui.SetActive(isGameOver);
        });
    }

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
