using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARFurniturePlacer : MonoBehaviour
{
    public ARRaycastManager raycastManager;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private float lastTapTime = 0f;
    private float doubleTapThreshold = 0.3f;

    private GameObject selectedObject = null;

    void Update()
    {
        Vector2 touchPosition;

        // =========================
        // INPUT (EDITOR + MOBILE)
        // =========================
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
            touchPosition = Input.mousePosition;
        else
            return;
#else
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        touchPosition = touch.position;
#endif

        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        RaycastHit hit;

        // =========================
        // ROTATE OBJECT (MOBILE - 2 FINGER)
        // =========================
#if !UNITY_EDITOR
        if (Input.touchCount == 2 && selectedObject != null)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prevDir = (t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition);
            Vector2 currDir = t0.position - t1.position;

            float angle = Vector2.SignedAngle(prevDir, currDir);

            selectedObject.transform.Rotate(Vector3.up, angle);

            return;
        }
#endif

        // =========================
        // ROTATE OBJECT (PC)
        // =========================
#if UNITY_EDITOR
        if (selectedObject != null && Input.GetKey(KeyCode.R))
        {
            selectedObject.transform.Rotate(Vector3.up, 100 * Time.deltaTime);
        }
#endif

        // =========================
        // DRAG OBJECT
        // =========================
#if UNITY_EDITOR
        if (Input.GetMouseButton(0) && selectedObject != null)
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved && selectedObject != null)
#endif
        {
            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose pose = hits[0].pose;
                selectedObject.transform.position = pose.position;
            }
            return;
        }

#if !UNITY_EDITOR
        if (Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            selectedObject = null;
            return;
        }

        if (Input.GetTouch(0).phase != TouchPhase.Began)
            return;
#endif

        float timeSinceLastTap = Time.time - lastTapTime;

        // =========================
        // DOUBLE TAP DELETE
        // =========================
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && timeSinceLastTap <= doubleTapThreshold)
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && timeSinceLastTap <= doubleTapThreshold)
#endif
        {
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.CompareTag("Furniture"))
                {
                    Debug.Log("Deleting object 🗑️");

                    GameObject rootObj = hit.transform.root.gameObject;

                    // 🔥 REMOVE FROM COST SYSTEM FIRST
                    if (CostManager.Instance != null)
                    {
                        CostManager.Instance.RemoveItem(rootObj);
                    }

                    // Existing delete system
                    FurnitureManager.Instance.DeletePlacedObject(rootObj);
                }
            }

            lastTapTime = 0f;
            return;
        }

        // =========================
        // SELECT OBJECT
        // =========================
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.CompareTag("Furniture"))
            {
                selectedObject = hit.transform.root.gameObject;
                Debug.Log("Selected object ✋");
                lastTapTime = Time.time;
                return;
            }
        }

        // =========================
        // PLACE OBJECT
        // =========================
        GameObject prefab = FurnitureManager.Instance.GetSelectedFurniture();

        if (prefab == null)
        {
            Debug.Log("No furniture selected ❌");
            return;
        }

        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose pose = hits[0].pose;

            Debug.Log("Spawning: " + prefab.name);

            GameObject obj = Instantiate(prefab, pose.position, pose.rotation);

            // Slight lift
            obj.transform.position += Vector3.up * 0.02f;

            GameObject rootObj = obj.transform.root.gameObject;

            Debug.Log("Registering object: " + rootObj.name);

            // 1. Register the placed object
            FurnitureManager.Instance.RegisterPlacedObject(rootObj);

            // 2. Add cost for the placed object
            if (CostManager.Instance != null)
            {
                CostManager.Instance.AddItem(rootObj);
            }

            // 3. Clear selection AFTER everything is done
            FurnitureManager.Instance.ClearSelection();

            Debug.Log("Furniture placed + cost updated ✅");
        }
        else
        {
            Debug.Log("No plane detected ❌");
        }

        lastTapTime = Time.time;
    }
}