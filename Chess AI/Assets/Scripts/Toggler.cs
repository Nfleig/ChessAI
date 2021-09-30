using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toggler : MonoBehaviour
{
    public GameObject to;
    private Text text;
    public void Awake()
    {
        text = transform.GetChild(0).GetComponent<Text>();
    }
    public void Toggle()
    {
        to.SetActive(!to.activeSelf);
        if (to.activeSelf)
        {
            text.text = "Stop AI";
        }
        else
        {
            text.text = "Start AI";
        }
    }
}
