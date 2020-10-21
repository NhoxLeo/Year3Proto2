using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerAnimation : MonoBehaviour
{
    private bool working = false;
    private float timer = 0f;
    private Animator animController = null;

    // Start is called before the first frame update
    void Start()
    {
        animController = GetComponent<Animator>();
        timer = Random.Range(1f, 3f);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            if (working)
            {
                timer = Random.Range(1f, 3f);
                working = false;
            }
            else
            {
                timer = Random.Range(8f, 16f);
                working = true;
            }
            animController.SetBool("Working", working);
        }
    }
}
