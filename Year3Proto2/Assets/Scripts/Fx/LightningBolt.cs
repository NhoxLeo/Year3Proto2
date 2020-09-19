using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningBolt : MonoBehaviour
{
    public Transform target;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


        if (target != null)
        {
            transform.LookAt(target);

            Vector3 scale = transform.localScale;
            scale.z = Vector3.Distance(transform.position, target.transform.position);
            transform.localScale = scale;
        }

    }
}
