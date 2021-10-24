using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toggler : MonoBehaviour
{
    public DeepGold to;
    private Text text;
    public void Awake()
    {
        text = transform.GetChild(0).GetComponent<Text>();
        if (to.getActive())
        {
            text.text = "Stop AI";
        }
        else
        {
            text.text = "Start AI";
        }
    }
    public void Toggle()
    {
        to.Activate(!to.getActive());
        if (to.getActive())
        {
            text.text = "Stop AI";
        }
        else
        {
            text.text = "Start AI";
        }
    }
}
