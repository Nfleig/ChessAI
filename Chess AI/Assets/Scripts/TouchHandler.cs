using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchHandler : MonoBehaviour
{
    bool hitObject;
    GameObject go;
    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 1)
        {
            // touch on screen
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit hit = new RaycastHit();
                hitObject = Physics.Raycast(ray, out hit);
                if (hitObject)
                {
                    go = hit.transform.gameObject;
                    if(go.tag != "IgnoreTouch")
                    {
                        go.SendMessage("OnMouseDown");
                    }
                }

            }


            // release touch/dragging
            if ((Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled) && go != null)
            {
                hitObject = false;
            }
        }
    }
}
