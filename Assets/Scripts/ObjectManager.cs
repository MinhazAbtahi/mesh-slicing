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
    private float StartPositionX;
    [SerializeField]
    private float step;
    public bool isMoving;
    public int totalMove = 0;
    public bool isGameOver;
    public bool isGameStart;
    public int levelsCount = 0;

    public GameObject ui;
    //sakib modify list for rigidbody objects
    public List<GameObject> slicePieces;
    public List<GameObject> oldSlicePieces;
    public PlayerController pc;

    // Start is called before the first frame update
    void Start()
    {
        levelsCount = PlayerPrefs.GetInt("LevelsCount", 0)/*0*/;
        sliceObject = sliceObjects[levelsCount];
        sliceObject.transform.position = new Vector3(0f, sliceObject.transform.position.y, 0);
        sliceObject.SetActive(true);

        sliceObject.transform.DOMoveX(StartPositionX, 1f).OnComplete(()=>
        {
            //sliceObject.GetComponent<RubberEffect>().enabled = true;
            //sliceObject.GetComponent<RubberEffect>()
            isGameStart = true;
        });
        step = sliceObject.transform.localScale.x / 5f;
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            StartCoroutine(MoveRoutine());
        }
    }

    public void addSlices(GameObject slice)
    {
        slicePieces.Add(slice);
    }
   
    public void MoveForward()
    {
        //if (totalMove <= 5000)
        //{
        //targetPositionX += step;
        //targetPositionX += sliceObject.transform.localScale.x;
        //StartCoroutine(MoveRoutine());

        //}
        //sakib changes movement
        if (isMoving)
        {
            StartCoroutine(MoveRoutine());
        }
    }

    //sakib call for kinematic trigger on slice objects
    public void repairRigidTrigger()
    {

        foreach (GameObject slice in slicePieces)
        {
            Rigidbody rb = slice.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            //rb.AddRelativeTorque(0, 0, -25);
            rb.AddForce(rb.mass * 20, 0, 0);

        }
        //old cuts rigidbody
        //moving old cuts
        foreach (GameObject slice in oldSlicePieces)
        {
            Rigidbody rb = slice.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            //rb.AddRelativeTorque(0, 0, -25);
            rb.AddForce(rb.mass * 20, 0, 0);

        }
        oldSlicePieces= new List<GameObject>();
        //Rigidbody rblastPiece = slicePieces[slicePieces.Count - 1].GetComponent<Rigidbody>();
        ////rblastPiece.isKinematic = false;

        //rblastPiece.AddRelativeTorque(0, 0, -25);
        //rblastPiece.AddForce(rblastPiece.mass * 20, 0, 0);


    }

    public void StopMoving()
    {
        isMoving = false;
        sliceObject.transform.DOKill();
        //stop all cutouts
        foreach (GameObject slice in oldSlicePieces)
        {
            slice.transform.DOKill();

        }
        StopAllCoroutines();
    }

    private IEnumerator MoveRoutine()
    {
        //++totalMove;
        //if (totalMove >= 60)
        //{
        //    isGameStart = false;
        //    isGameOver = true;
        //    ++levelsCount;
        //    if (levelsCount >= sliceObjects.Count)
        //    {
        //        levelsCount = levelsCount % (sliceObjects.Count);
        //    }
        //    PlayerPrefs.SetInt("LevelsCount", levelsCount);
        //}

        if (sliceObject.transform.position.x >= targetPositionX-.5f)
        {
            isGameStart = false;
            isGameOver = true;
           
            ui.SetActive(isGameOver);
        }

        //yield return new WaitForSeconds(.25f);
        //yield return null;
        isMoving = false;

        //moving old cuts
        foreach (GameObject slice in oldSlicePieces)
        {
            slice.transform.DOMoveX(targetPositionX, 20f);

        }
        sliceObject.transform.DOMoveX(targetPositionX, 20f);

        yield return new WaitForSeconds(.1f);
        isMoving = true;

        //sliceObject.transform.DOMoveX(targetPositionX, 5f).OnComplete(()=>
        //{
        //    ui.SetActive(isGameOver);
        //});
    }

    public void Reload()
    {
        ++levelsCount;
        if (levelsCount >= sliceObjects.Count)
        {
            levelsCount = levelsCount % (sliceObjects.Count);
        }
        PlayerPrefs.SetInt("LevelsCount", levelsCount);
        Debug.Log("LevelsCount=" + levelsCount);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
