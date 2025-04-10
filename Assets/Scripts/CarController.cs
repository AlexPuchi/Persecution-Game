using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*****************************************************
* Code by: Alex Puigdengolas
*
* Class Purpose:
* CarController is the script that gives logic controls to 
* the Car objects. This script controls mainly their movement.
* ******************************************************/
public class CarController : MonoBehaviour
{
    public float speed = 2f;
    private List<Vector3> path;
    private int targetIndex;
    public bool isMoving = false;
    private bool isPaused = false;

    private LineRenderer lineRenderer;

    /*********
    * The method Awake loads all the constants for the lineRender
    * to work as designed
    **********/
    void Awake()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 0;
        lineRenderer.widthMultiplier = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.sortingOrder = 5;
    }

    /****
    * The method SetPath grabs the points marked by the user and 
    * contained in the Line renderer, to show this class the path
    * the car need's to follow.
    ****/
    public void SetPath(List<Vector3> pathPoints)
    {
        path = new List<Vector3>(pathPoints);
        targetIndex = 0;

        lineRenderer.positionCount = path.Count;
        lineRenderer.SetPositions(path.ToArray());
    }

    /****
    * This method erases all the points of the line that the car 
    * is following
    ****/
    public void ClearPathLine()
    {
        lineRenderer.positionCount = 0;
    }

    /*****
    * The method StartMoving changes the boolean values and 
    * allows the cars on the scene to move
    ******/
    public void StartMoving()
    {
        isMoving = true;
        isPaused = false;
    }

    /****
    * The method StopMoving changes the boolean values and 
    * stops all the cars on the scene to make them not move
    ****/
    public void StopMoving()
    {
        isPaused = true;
    }

    void Update()
    {
        if (!isMoving || isPaused || path == null || targetIndex >= path.Count)
            return;

        transform.position = Vector3.MoveTowards(transform.position, path[targetIndex], speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, path[targetIndex]) < 0.1f)
        {
            targetIndex++;
        }

        if (targetIndex >= path.Count)
        {
            isMoving = false;
        }
    }
}