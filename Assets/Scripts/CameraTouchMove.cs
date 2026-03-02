using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CameraTouchMove : MonoBehaviour
{
    public static CameraTouchMove main;
    public GameObject topLeftPoint;
    public GameObject bottomRightPoint;
    public float minZoom = 3.0f;
    public float maxZoom = 20.0f;

    [HideInInspector] public bool cameraMove = true;
    [HideInInspector] public float moveSpeed = 0.03f;    
    [HideInInspector] public float moveSpeedMouse = 0.05f;    
    [HideInInspector] public float zoomSpeed = 0.01f;
    [HideInInspector] public float zoomSpeedMouse = 0.5f;

    Camera cam;
    Vector2 touchStartPos;
    Vector3 cameraStartPos;
    Vector2 minBounds;
    Vector2 maxBounds;
    float baseOrthographicSize;

    void Awake()
    {
        if (main == null)
        {
            main = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        minBounds.x = topLeftPoint.transform.position.x;
        maxBounds.x = bottomRightPoint.transform.position.x;
        minBounds.y = bottomRightPoint.transform.position.y;        
        maxBounds.y = topLeftPoint.transform.position.y;
        cam = Camera.main;
        cam.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, -100);
        baseOrthographicSize = cam.orthographicSize;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseScreenPosition = Input.mousePosition;
            touchStartPos = mouseScreenPosition;
            cameraStartPos = Camera.main.transform.position;          
        }

        if (cameraMove && Input.GetMouseButton(0))
        {
            Vector2 mouseScreenPosition = Input.mousePosition;
            Vector2 delta = mouseScreenPosition - touchStartPos;
            float zoomFactor = cam.orthographicSize / baseOrthographicSize;
            float adjustedMoveSpeed = moveSpeedMouse * zoomFactor;
            Vector3 newPos = cameraStartPos - new Vector3(delta.x * adjustedMoveSpeed, delta.y * adjustedMoveSpeed, 0);

            newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
            newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);
            newPos.z = -100;
            
            if (delta.magnitude > 0 && UIManager.main != null && !UIManager.main.IsGamePause())
            {
                cam.transform.position = newPos;
            }
        }

        float scroll = Input.mouseScrollDelta.y;
        float currentScroll = Mathf.Clamp(cam.orthographicSize - scroll * zoomSpeedMouse, minZoom, maxZoom);

        if (scroll != 0 && GameObject.FindGameObjectWithTag("Popup") == null && GetMaxZoomForCurrentAspect() > currentScroll)
        {
            cam.orthographicSize = currentScroll;
        }
        else if (GetMaxZoomForCurrentAspect() < currentScroll) {
            cam.orthographicSize = GetMaxZoomForCurrentAspect();
        }

        LimitCameraPosition();
    }

    void LimitCameraPosition()
    {
        Vector3 pos = cam.transform.position;
        float horizontalExtent = GetHorizontalLimit();
        float verticalExtent = GetVerticalLimit();
        
        float effectiveMinX = minBounds.x + horizontalExtent;
        float effectiveMaxX = maxBounds.x - horizontalExtent;
        float effectiveMinY = minBounds.y + verticalExtent;
        float effectiveMaxY = maxBounds.y - verticalExtent;
        
        if (effectiveMinX > effectiveMaxX)
        {
            pos.x = (minBounds.x + maxBounds.x) / 2f;
        }
        else
        {
            pos.x = Mathf.Clamp(pos.x, effectiveMinX, effectiveMaxX);
        }
        
        if (effectiveMinY > effectiveMaxY)
        {
            pos.y = (minBounds.y + maxBounds.y) / 2f;
        }
        else
        {
            pos.y = Mathf.Clamp(pos.y, effectiveMinY, effectiveMaxY);
        }
        
        pos.z = -100;
        cam.transform.position = pos;
    }

    float GetHorizontalLimit()
    {
        return cam.orthographicSize * cam.aspect;
    }

    float GetVerticalLimit()
    {
        return cam.orthographicSize;
    }

    float GetMaxZoomForCurrentAspect()
    {
        float maxZoomBasedOnWidth = (maxBounds.x - minBounds.x) / (2f * cam.aspect);
        float maxZoomBasedOnHeight = (maxBounds.y - minBounds.y) / 2f;
        return Mathf.Min(maxZoomBasedOnWidth, maxZoomBasedOnHeight, maxZoom);
    }
}
