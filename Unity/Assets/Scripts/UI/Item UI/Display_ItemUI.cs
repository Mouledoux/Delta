using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Display_ItemUI : MonoBehaviour 
{
    private bool state = true;
    void Start()
    {
        gameObject.transform.GetChild(0).GetComponent<Text>().text = gameObject.transform.parent.name;

    }

    void OnGUI()
    {
        if (!state)
            turnOffDisplay();

        transform.forward = GameObject.Find("Player").transform.forward;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (state)
                state = false;
            else
            {
                turnOnDisplay();
                state = true;
            }
            
        }
    }

    void turnOffDisplay()
    {
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    void turnOnDisplay()
    {
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.SetActive(true);
        }
    }
}
