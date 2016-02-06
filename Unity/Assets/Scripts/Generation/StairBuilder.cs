using UnityEngine;
using System.Collections;

public class StairBuilder : MonoBehaviour {

    public Transform Bottom;
    public Transform Top;
    public float minStepHeight, maxStepHeight;
    public float minStepLength, maxStepLength;
    public float stairWidth;

    void BuildStair()
    {
        int xDirection;
        int zDirection;

        float height;
        float xlength;
        float zlength;

        findProperties(out height, out xlength, out zlength, out xDirection, out zDirection);


        if (xlength > zlength)
        {
            //follow zdirection followed by x direction
        }

        else if (xlength < zlength)
        {
            //follow xdirection followed by zdirection
        }

        else
        {
            //follow whichever direction isn't equal to zero
        }

    }

    void findProperties(out float height, out float xlength, out float zlength, out int xDirection, out int zDirection)
    {

        height = Top.position.y - Bottom.position.y;
        xlength = Mathf.Abs(Top.position.x - Bottom.position.x);
        zlength = Mathf.Abs(Top.position.z - Bottom.position.z);

        #region GetStairDirection
        if (Bottom.position.x < Top.position.x)
        {
            xDirection = 1;
        }

        else if (Bottom.position.x > Top.position.x)
        {
            xDirection = 2;
        }

        else
        {
            xDirection = 0;
        }

        if (Bottom.position.z < Top.position.z)
        {
            zDirection = 1;
        }

        else if (Bottom.position.z > Top.position.z)
        {
            zDirection = 2;
        }

        else
        {
            zDirection = 0;
        }
        #endregion

    }
}
