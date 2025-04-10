using System.Collections.Generic;
using UnityEngine;

/************************************************************
* Code by: Alex Puigdengolas
* 
* Class Purpose:
* PathDrawer, is in charge to allow the user to draw the path 
* that the car will follow.
************************************************************/
public class PathDrawer : MonoBehaviour
{
    private List<Vector3> currentPathPoints = new List<Vector3>();
    private CarController selectedCar;
    private LineRenderer tempLineRenderer;

    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.1f;
    private bool anyCarMoving = false;
    private Dictionary<CarController, List<Vector3>> carToPath = new Dictionary<CarController, List<Vector3>>();

    /*************
    * The method Update has all the functionality this class
    * needs: 
    *   - It controls the mouse buttons to know if it needs to start drawing the lines
    *   - It controls if the mouse is on top of a car to be able to draw a line
    *   - It controls if the user presses the space bar to make the cars move or stop
    *************/
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                selectedCar = hit.collider.GetComponent<CarController>();
                if (selectedCar != null)
                {
                    currentPathPoints.Clear();
                    currentPathPoints.Add(selectedCar.transform.position);

                    GameObject tempLineObj = new GameObject("TempLine");
                    tempLineRenderer = tempLineObj.AddComponent<LineRenderer>();
                    tempLineRenderer.material = lineMaterial;
                    tempLineRenderer.widthMultiplier = lineWidth;
                    tempLineRenderer.positionCount = 1;
                    tempLineRenderer.SetPosition(0, selectedCar.transform.position);
                    tempLineRenderer.sortingOrder = 10;
                }
            }
        }

        if (Input.GetMouseButton(0) && selectedCar != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            if (currentPathPoints.Count == 0 || Vector3.Distance(currentPathPoints[currentPathPoints.Count - 1], mousePos) > 0.1f)
            {
                currentPathPoints.Add(mousePos);
                tempLineRenderer.positionCount = currentPathPoints.Count;
                tempLineRenderer.SetPositions(currentPathPoints.ToArray());
            }
        }

        if (Input.GetMouseButtonUp(0) && selectedCar != null)
        {
            selectedCar.SetPath(currentPathPoints);
            carToPath[selectedCar] = new List<Vector3>(currentPathPoints);

            if (tempLineRenderer != null)
            {
                Destroy(tempLineRenderer.gameObject);
                tempLineRenderer = null;
            }

            selectedCar = null;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (anyCarMoving)
            {
                foreach (var car in carToPath.Keys)
                {
                    car.StopMoving();
                }
                anyCarMoving = false;
            }
            else
            {
                foreach (var car in carToPath.Keys)
                {
                    car.StartMoving();
                }
                anyCarMoving = true;
            }
        }
    }
}
