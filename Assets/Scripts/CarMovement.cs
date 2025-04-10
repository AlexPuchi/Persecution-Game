using System.Collections.Generic;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private List<Vector3> path;
    private int currentIndex = 0;
    private bool isMoving = false;

    [SerializeField] private float moveSpeed = 5f;

    public void SetPath(List<Vector3> newPath)
    {
        path = newPath;
        currentIndex = 0;
    }

    public void StartMoving()
    {
        if (path != null && path.Count > 0)
        {
            isMoving = true;
        }
    }

    void Update()
    {
        if (isMoving && path != null && currentIndex < path.Count)
        {
            Vector3 target = path[currentIndex];
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target) < 0.01f)
            {
                currentIndex++;
                if (currentIndex >= path.Count)
                {
                    isMoving = false; // Stop when path is complete
                }
            }
        }
    }
}