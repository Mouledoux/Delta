using UnityEngine;
using System.Collections;

public class AITest : MonoBehaviour {

    /*
        The goal of this script is to create a basis for a modular AI system
    
        This script will be used in conjunction with an overseeing "state manager" for coordination
        between NPCs, however it will not be reliant on it
        
        This script should:
        -   Allow objects to move between two points effectively while
        avoiding any non-walkable surfaces (including walls and other npcs)
        -   Give objects a "cone of sight" for detecting other entities within
        their sight lines
        -   Allow objects to "patrol" an area while doing the above
        -   Allow objects to "engage" another entity while doing the above
        -   (If necessary) allow an object to react to combat situations
        -   Allow objects to effectively "search" for another entity if LOS is lost
        */
}
