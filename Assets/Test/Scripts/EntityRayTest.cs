using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityRayTest : MonoBehaviour
{
    public Camera mainCam;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Ray2Entity();
    }

    private void Ray2Entity()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit,10000000))
            {
                if (hit.collider)
                {
                    Debug.Log(hit);
                    Destroy(hit.collider.gameObject);
                }
            }
        }
    }
}
