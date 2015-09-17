using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour {

    //Tracks state of menus
    public bool MainGame;
    public bool InventoryMenu;
    public bool StatMenu;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.I))
        {
            InventoryMenu = true;
        }
	
	}
}
