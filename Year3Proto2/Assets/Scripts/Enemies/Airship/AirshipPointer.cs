using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirshipPointer : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private RectTransform indicator;
    [SerializeField] private float offset = 1.0f;

    public void SetTargetPosition(Transform target)
    {
        this.target = target;
    }

    private void Update()
    {
        //Converting world co-ordinates to screen.
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.localPosition);

        bool xOffScreen = screenPosition.x <= offset || screenPosition.x >= Screen.width - offset;
        bool yOffScreen = screenPosition.y <= offset || screenPosition.y >= Screen.height - offset;
        bool zOffScreen = screenPosition.z <= offset;

        // Object is behind camera
        if (zOffScreen)
        {
            screenPosition.x = -screenPosition.x;
            screenPosition.y = -screenPosition.y;
        }
        

        //Checking if the indicator is off screen.
        if (xOffScreen || yOffScreen || zOffScreen)
        {
            //Indicator Angle
            Vector3 direction = (target.position - Camera.main.transform.position).normalized;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            indicator.localEulerAngles = new Vector3(0.0f, 0.0f, angle);

            //Indicator Screen Position
        }
        else
        {
            //Indicator World Position
            if(indicator.localEulerAngles.z != 270.0f) indicator.localRotation = Quaternion.Euler(0.0f, 0.0f, 270.0f);
            indicator.position = screenPosition;
        }
    }
}

