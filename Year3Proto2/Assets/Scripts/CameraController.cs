using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField] [Tooltip("Rate of movement for the camera")]
    private float sensitivity;

    [Header("Motion Limits")]

    [SerializeField] [Tooltip("xAxis maximum")]
    private float xAxisMax;

    [SerializeField] [Tooltip("xAxis minimum")]
    private float xAxisMin;

    [SerializeField] [Tooltip("zAxis maximum")]
    private float zAxisMax;

    [SerializeField] [Tooltip("zAxis minimum")]
    private float zAxisMin;

    private Vector3 north;
    private Vector3 east;
    private Vector3 south;
    private Vector3 west;

    private float scrollOffset = 0f;

    [Header("Zoom Limits")]

    [SerializeField] [Tooltip("Zoom Max")]
    private float scrollMax = 3f;

    [SerializeField] [Tooltip("Zoom Min")]
    private float scrollMin = -10f;

    private Vector3 cameraZoomMidPoint;

    //private int cu


    // Start is called before the first frame update
    void Start()
    {
        float quarterPi = Mathf.Deg2Rad * 45.0f;
        north = Vector3.RotateTowards(Vector3.forward, Vector3.right, quarterPi, 5f).normalized;
        east = Vector3.RotateTowards(Vector3.back, Vector3.right, quarterPi, 5f).normalized;
        south = Vector3.RotateTowards(Vector3.back, Vector3.left, quarterPi, 5f).normalized;
        west = Vector3.RotateTowards(Vector3.forward, Vector3.left, quarterPi, 5f).normalized;
        cameraZoomMidPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseScroll = 0;
        mouseScroll = Mathf.Clamp(Input.mouseScrollDelta.y, -50f, 50f);
        scrollOffset += mouseScroll;

        if (scrollOffset > scrollMax) { scrollOffset = scrollMax; }
        if (scrollOffset < scrollMin) { scrollOffset = scrollMin; }

        float scrollMoveBonus = 1f + (-scrollOffset + 10f) * 0.15f;


        if (Input.GetKey(moveNorth))
        {
            cameraZoomMidPoint += north * Time.deltaTime * sensitivity * scrollMoveBonus;
        }
        if (Input.GetKey(moveEast))
        {
            cameraZoomMidPoint += east * Time.deltaTime * sensitivity * scrollMoveBonus;
        }
        if (Input.GetKey(moveSouth))
        {
            cameraZoomMidPoint += south * Time.deltaTime * sensitivity * scrollMoveBonus;
        }
        if (Input.GetKey(moveWest))
        {
            cameraZoomMidPoint += west * Time.deltaTime * sensitivity * scrollMoveBonus;
        }

        if (cameraZoomMidPoint.x > xAxisMax) { cameraZoomMidPoint.x = xAxisMax; }
        if (cameraZoomMidPoint.x < xAxisMin) { cameraZoomMidPoint.x = xAxisMin; }
        if (cameraZoomMidPoint.z > zAxisMax) { cameraZoomMidPoint.z = zAxisMax; }
        if (cameraZoomMidPoint.z < zAxisMin) { cameraZoomMidPoint.z = zAxisMin; }

        transform.position = cameraZoomMidPoint + transform.forward * scrollOffset * .5f;

        //Screen.width
    }
}
