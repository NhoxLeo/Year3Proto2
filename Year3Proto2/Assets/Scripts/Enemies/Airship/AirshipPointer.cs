using UnityEngine;

// Bachelor of Software Engineering
// Media Design School
// Auckland
// New Zealand
//
// (c) 2020 Media Design School.
//
// File Name    : AirshipSpawner.cs
// Description  : Spawns enemy airships
// Author       : Tjeu Vreeburg
// Mail         : tjeu.vreeburg@gmail.com

public class AirshipPointer : MonoBehaviour
{
    [SerializeField] private float xOffset = 200.0f;
    [SerializeField] private float yOffset = 80.0f;
    [SerializeField] private float scale = 1.0f;

    private RectTransform indicator;
    private Transform target;
    private Vector3 center;

    /**************************************
    * Name of the Function: Start
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    private void Start()
    {
        // Data Assignment
        center = new Vector3(Screen.width, Screen.height, 0.0f) / 2.0f;
        indicator = GetComponent<RectTransform>();
        indicator.localScale = Vector3.one * scale;
        transform.SetParent(FindObjectOfType<Canvas>().transform);
    }

    /**************************************
    * Name of the Function: Update
    * @Author: Tjeu Vreeburg
    * @Parameter: n/a
    * @Return: void
    ***************************************/
    private void Update()
    {
        //Converting world co-ordinates to screen.
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.localPosition);

        bool xOffScreen = screenPosition.x <= 0.0f || screenPosition.x >= Screen.width;
        bool yOffScreen = screenPosition.y <= 0.0f || screenPosition.y >= Screen.height;
        bool zOffScreen = screenPosition.z <= 0.0f;

        // Object is behind camera
        if (zOffScreen) screenPosition *= -1.0f;

        // Checking if the indicator is off screen.
        if (xOffScreen || yOffScreen || zOffScreen)
        {
            // Indicator Angle
            Vector3 direction = (screenPosition - center).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x);

            // Indicator Screen Position
            float width = Mathf.Abs(center.x / Mathf.Cos(angle));
            float height = Mathf.Abs(center.y / Mathf.Cos(angle + (Mathf.PI * 0.5f)));
            float minimum = Mathf.Min(width + (xOffset * 2.0f), height + yOffset);

            // Updating Indicator
            indicator.localPosition = minimum * direction;
            indicator.localEulerAngles = new Vector3(0.0f, 0.0f, angle * Mathf.Rad2Deg);
        }
        else
        {
            // Indicator World Position
            if(indicator.localEulerAngles.z != 270.0f) indicator.localEulerAngles = new Vector3(0.0f, 0.0f, 270.0f);
            indicator.position = screenPosition;
        }
    }

    /**************************************
    * Name of the Function: SetTarget
    * @Author: Tjeu Vreeburg
    * @Parameter: Transform
    * @Return: void
    ***************************************/
    public void SetTarget(Transform target)
    {
        this.target = target;
    }
}

