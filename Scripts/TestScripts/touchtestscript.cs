using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class touchtestscript : MonoBehaviour
{
    public GameObject particle;

    void Update()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log("1: "+ touch.position);
                // Construct a ray from the current touch coordinates
                Ray ray = Camera.main.ViewportPointToRay(touch.position);
                RaycastHit hit;
                if (Physics.Raycast(ray,out hit))
                {
                    Debug.Log("2");
                    // Create a particle if hit
                    Instantiate(particle, hit.point, transform.rotation);
                }
            }
        }
    }
}
