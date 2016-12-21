using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Contains static functions that are used throughout the generation process
/// </summary>
public static class UniversalHelper
{
    /// <summary>
    /// Creates an empty gameobject parent for objects, or adds objects to parent if it already exist
    /// </summary>
    /// <param name="child"></param>
    /// <param name="parentName"></param>
    /// <param name="subParentName"></param>
    public static bool parentObject(List<GameObject> child, string parentName, string subParentName = "")
    {
        Vector3 position = child[0].transform.position;
        GameObject parent = GetObject(parentName, position);                //Get the parent if it exists, else create it

        if (child.Count > 1) //If there is an object to be parented
        {
            if (!string.IsNullOrEmpty(subParentName))                   //If subParentName has a value
            {
                GameObject subParent = GetObject(subParentName, position);  //Get the subParent if it exists else create it

                addChild(child, subParent);                                 //Add the children to the sub parent
                subParent.transform.parent = parent.transform;              //Add children to parent
            }

            else                                                        //Otherwise
                addChild(child, parent);                                        //Add the children to the parent
        }

        else    //Otherwise, output to the log that there are no children
        {
            Debug.LogError("No child objects to parent");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns the specified game object if it exists or creates it if it doesn't.
    /// </summary>
    /// <param name="objName"></param>
    /// <returns></returns>
    public static GameObject GetObject(string objName, Vector3 position)
    {
        GameObject parent = GameObject.Find(objName);
        if (!parent)
        {
            parent = new GameObject(objName);
            parent.transform.position = position;
        }

        return parent;
    }

    /// <summary>
    /// Parents a list of game objects
    /// </summary>
    /// <param name="child"></param>
    /// <param name="parent"></param>
    private static void addChild(List<GameObject> child, GameObject parent)
    {
        for (int i = 0; i < child.Count; i++)
        {
            child[i].transform.parent = parent.transform;
        }
    }

    /// <summary>
    /// Get the cells in a specified room
    /// </summary>
    /// <param name="room"></param>
    /// <param name="partOfRoom"></param>
    /// <returns></returns>
    private static List<GameObject> getRoomCells(GameObject room, string partOfRoom = "")
    {
        if (room.transform.childCount > 0)
        {
            List<GameObject> returnObj = new List<GameObject>();

            if (!string.IsNullOrEmpty(partOfRoom)) //Returns part of room ie walls
            {
                foreach (Transform child in room.transform) //Accesses child
                {
                    for (int i = 0; i < child.childCount; i++) //Needed to check if child's children contain partofroom
                    {
                        if (child.GetChild(i).name.Contains(partOfRoom))
                        {
                            returnObj.Add(child.GetChild(i).gameObject); //returns the part of room
                        }
                    }
                }

                if (returnObj == null)
                {
                    throw new Exception(@"The part of room provided does not exist in any of the objects children
                    If you wish to access a direct child of the object, use getRoom instead");
                }
            }

            else
            {
                foreach (Transform child in room.transform)
                {
                    returnObj.Add(child.gameObject);
                }
            }

            return returnObj;
        }

        else
        {
            throw new Exception("This object has no children");
        }
    }

    /// <summary>
    ///Shuffles numbers using a predictable algorithm
    /// </summary>
    /// <param name="numbers"></param>
    /// <returns></returns>
    public static int[] Shuffle(int[] numbers, int shuffle, out int shuffleCall)
    {
        shuffleCall = shuffle;
        if (numbers.Length == 24)
            shuffleCall = shuffle + 1;
        List<List<int>> groups = new List<List<int>>();
        List<int> group = new List<int>();

        for (int i = 0; i < numbers.Length; i++)
        {
            group.Add(numbers[i]);

            if ((i + 1) % 4 == 0)
            {
                groups.Add(group);
                group = new List<int>();
            }
        }

        foreach (List<int> g in groups)
        {
            if (g.Count < 4)
            {
                int count = 0;
                g.Reverse();
                int i = 0;
                for (int x = 0; x < g.Count; x++)
                {
                    if (groups.Count < i)
                    {
                        int temp = groups[i][0];
                        groups[i][0] = x;
                        x = temp;
                        i++;
                    }

                    else
                    {
                        i = 0;
                        x--;
                        count++;

                        if (count > 20)
                        {
                            Debug.LogError("Shuffle error");
                            break;
                        }
                    }
                }
                continue;
            }

            if (shuffleCall % 3 == 0)
            {
                int temp = g[0];
                g[0] = g[3];
                g[3] = temp;
            }

            else if (shuffleCall % 4 == 0)
            {
                int temp = g[1];
                g[1] = g[2];
                g[2] = temp;
            }

            else
            {
                int temp = g[0];
                g[0] = g[1];
                g[1] = temp;

                temp = g[2];
                g[2] = g[3];
                g[3] = temp;
            }

            if (shuffleCall % 2 != 0)
            {
                int temp = g[0];
                g[0] = g[3];
                g[3] = g[2];
                g[2] = g[1];
                g[1] = temp;
            }

            else
            {
                int temp = g[0];
                g[0] = g[1];
                g[1] = g[2];
                g[2] = g[3];
                g[3] = temp;
            }

        }

        if (shuffleCall >= 4)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                List<int> temp = groups[i];

                try
                {
                    groups[i] = groups[i + 1];
                    groups[i + 1] = temp;
                }

                catch
                {
                    groups[i] = groups[0];
                    groups[0] = temp;
                }

            }
            shuffleCall = 0;
        }

        int n = 0;

        foreach (List<int> g in groups)
        {
            foreach (int number in g)
            {
                numbers[n] = number;
                n++;
            }
        }

        return numbers;

    }

    /// <summary>
    /// Generates random int between min and max
    /// </summary>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <returns></returns>
    public static int randomGenerator(int minValue, int maxValue, System.Random rand)
    {
        int result = rand.Next(minValue, (maxValue + 1));

        return result;
    }

    /// <summary>
    /// Determines the directions needed to get from object one to object two. 
    /// Returns array of length two. First value is x. Second value is z.
    /// u = undefined. n s e w = north south east west
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public static char[] detectDirection(GameObject one, GameObject two)
    {
        //n,e,s,w = north, east, south, and west respectively
        // u = undefined

        char[] direction = new char[3];

        Vector3 oneVec = one.transform.position;
        Vector3 twoVec = two.transform.position;

        if (oneVec.x > twoVec.x)
        {
            direction[0] = 'w';
        }

        else if (oneVec.x < twoVec.x)
        {
            direction[0] = 'e';
        }

        else
        {
            direction[0] = 'u';
        }

        if (oneVec.z > twoVec.z)
        {
            direction[1] = 's';
        }

        else if (oneVec.z < twoVec.z)
        {
            direction[1] = 'n';
        }

        else
        {
            direction[1] = 'u';
        }
        return direction;
    }

    public static int compareTo(float a, float b)
    {
        if (a > b)
            return 1;
        else if (b > a)
            return -1;

        else
            return 0;
    }


}
