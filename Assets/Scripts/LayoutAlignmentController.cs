using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayoutAlignmentController : MonoBehaviour
{
    [Header("UI Reference")]
    public GameObject lockLayoutButton; // Assign in Inspector a button that calls LockLayout()

    private void Start()
    {
        // When spawned, ensure the button is visible
        if (lockLayoutButton != null)
        {
            lockLayoutButton.SetActive(true);
        }
    }

    // Called by a "Lock Alignment" UI Button
    public void LockLayout()
    {
        Debug.Log("Locking layout in place and unlinking furniture items...");

        // Get all children (the spawned furniture pieces)
        List<Transform> children = new List<Transform>();
        foreach (Transform child in transform)
        {
            children.Add(child);
        }

        // Unparent them so they become independent objects again
        // This allows ARFurniturePlacer to select them individually!
        foreach (Transform child in children)
        {
            child.parent = null;
        }

        // Hide the lock button
        if (lockLayoutButton != null)
        {
            lockLayoutButton.SetActive(false);
        }

        // Destroy the empty grouping container
        Destroy(gameObject);
    }
}
