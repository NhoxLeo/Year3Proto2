using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]

    [SerializeField] [Tooltip("Keybinding for moving the camera up")]
    private KeyCode moveNorth;

    [SerializeField] [Tooltip("Keybinding for moving the camera right")]
    private KeyCode moveEast;

    [SerializeField] [Tooltip("Keybinding for moving the camera down")]
    private KeyCode moveSouth;

    [SerializeField] [Tooltip("Keybinding for moving the camera left")]
    private KeyCode moveWest;

    [SerializeField] [Tooltip("Whether to move camera if mouse near edge of screen")]
    private bool mouseEdgeMove = true;

    [SerializeField] [Tooltip("Buffer zone for mouse camera control at edge of screen")]
    private float mouseXBuffer = 50.0f;

    [SerializeField] [Tooltip("Buffer zone for mouse camera control at edge of screen")]
    private float mouseYBuffer = 50.0f;

    [SerializeField] [Tooltip("Rate of movement for the camera")]
    private float sensitivity;

    [SerializeField] [Tooltip("Rate at which camera lerps movement")]
    private float lerpSpeed = 10.0f;

    private float xAxisMax;
    private float xAxisMin;
    private float zAxisMax;
    private float zAxisMin;
    private float scrollMax;
    private float scrollMin;

    private Vector3 north;
    private Vector3 east;
    private Vector3 south;
    private Vector3 west;

    private float scrollOffset = 0f;

    private Vector3 cameraZoomMidPoint;

    private Vector2 lastFrameMousePos;

    void Start()
    {
        float quarterPi = Mathf.Deg2Rad * 45.0f;
        north = Vector3.RotateTowards(Vector3.forward, Vector3.right, quarterPi, 5f).normalized;
        east = Vector3.RotateTowards(Vector3.back, Vector3.right, quarterPi, 5f).normalized;
        south = Vector3.RotateTowards(Vector3.back, Vector3.left, quarterPi, 5f).normalized;
        west = Vector3.RotateTowards(Vector3.forward, Vector3.left, quarterPi, 5f).normalized;
        cameraZoomMidPoint = transform.position;
        lastFrameMousePos = Vector2.zero;
        (Vector4, Vector2) settings = SuperManager.GetInstance().GetCurrentCamSettings();
        zAxisMax = settings.Item1.x;
        zAxisMin = settings.Item1.y;
        xAxisMax = settings.Item1.z;
        xAxisMin = settings.Item1.w;
        scrollMin = settings.Item2.x;
        scrollMax = settings.Item2.y;
    }

    void FixedUpdate()
    {
        float mouseScroll = Mathf.Clamp(Input.mouseScrollDelta.y, -50f, 50f);
        scrollOffset += mouseScroll;

        if (scrollOffset > scrollMax) { scrollOffset = scrollMax; }
        if (scrollOffset < scrollMin) { scrollOffset = scrollMin; }

        float scrollMoveBonus = 1f + (-scrollOffset + 10f) * 0.15f;

        Vector2 mp = Input.mousePosition;
        float mouseMult = (StructureManager.GetInstance().isOverUI || GlobalData.isPaused || !mouseEdgeMove) ? 0.0f : 1.0f;

        float movementCoeff = Time.fixedDeltaTime * sensitivity * scrollMoveBonus;

        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            cameraZoomMidPoint += north * movementCoeff * (lastFrameMousePos.y - mp.y) * .07f;
            cameraZoomMidPoint += east * movementCoeff * (lastFrameMousePos.x - mp.x) * .07f;
        }
        else
        {
            float northKey = Input.GetKey(moveNorth) ? 1.0f : 0.0f;
            float northMouse = Mathf.Clamp((mp.y - (Screen.height - mouseYBuffer)) / mouseYBuffer, 0.0f, 1.0f) * mouseMult;
            float northMove = Mathf.Max(northKey, northMouse);

            float eastKey = Input.GetKey(moveEast) ? 1.0f : 0.0f;
            float eastMouse = Mathf.Clamp((mp.x - (Screen.width - mouseXBuffer)) / mouseXBuffer, 0.0f, 1.0f) * mouseMult;
            float eastMove = Mathf.Max(eastKey, eastMouse);

            float southKey = Input.GetKey(moveSouth) ? 1.0f : 0.0f;
            float southMouse = Mathf.Clamp(1.0f - (mp.y / mouseYBuffer), 0.0f, 1.0f) * mouseMult;
            float southMove = Mathf.Max(southKey, southMouse);

            float westKey = Input.GetKey(moveWest) ? 1.0f : 0.0f;
            float westMouse = Mathf.Clamp(1.0f - (mp.x / mouseXBuffer), 0.0f, 1.0f) * mouseMult;
            float westMove = Mathf.Max(westKey, westMouse);

            cameraZoomMidPoint += northMove * north * movementCoeff;
            cameraZoomMidPoint += eastMove * east * movementCoeff;
            cameraZoomMidPoint += southMove * south * movementCoeff;
            cameraZoomMidPoint += westMove * west * movementCoeff;
        }


        if (cameraZoomMidPoint.x > xAxisMax) { cameraZoomMidPoint.x = xAxisMax; }
        if (cameraZoomMidPoint.x < xAxisMin) { cameraZoomMidPoint.x = xAxisMin; }
        if (cameraZoomMidPoint.z > zAxisMax) { cameraZoomMidPoint.z = zAxisMax; }
        if (cameraZoomMidPoint.z < zAxisMin) { cameraZoomMidPoint.z = zAxisMin; }

        transform.position = Vector3.Lerp(transform.position, cameraZoomMidPoint + transform.forward * scrollOffset * .5f, Time.smoothDeltaTime * lerpSpeed);

        lastFrameMousePos = mp;
    }
}
