using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public static class GenerationSeed
{
    /*
    How the seed works
    
        The GenerationRevamped class builds the quadrants of the grid
        It then sends the room center position for a 3x3 room for the top center, bottom right, and bottom left
        of each quadrant to this class
        This setup is considered the "Default" dungeon (000000000000). Boring yes, but that's the point.
        From here, this script executes a series of transformations and exchanges based on numbers 0-9.
        The seed is currently an int array with length 12.
        The first nine numbers are used to transform the dungeon
        The tenth and eleventh numbers determine which rooms (0-26 since there are 27 rooms in the dungeon)
        will be considered the main rooms
        The twelfth number determines the boss that spawns
        This array may be expanded to help with object generation if wanted (more details when I get to that)

        Every room is numbered from 0-26. To see what room number is where by default, see GenerationRevamped

        As stated previously, numbers 0-9 are used for transformations and whatnot.
        They are used in the following manner:
        0 - shifts every room up in their quadrant by room number (if it reaches the top, it goes to bottom)
        1 - shifts every number right in their quadrant room number (if it reaches side, it goes to other side)
        2 - shifts every room up on the entire grid by room number (same rules as 0)
        3 - shifts every room right on the entire grid by room number (same rules as 1)
        4 - switches room numbers within quadrant
        5 - switches room numbers among the grid
        6 - increases the scale of every room by its number / 3
        7 - increases the scale of every room by its number * 3
        8 - swaps room sizes among quadrant
        9 - swaps room sizes among grid 

        Adding a negative to these numbers processes the command in the opposite manner

        One seed is used for the entire dungeon.

        1st level uses original seed.
        2nd level uses the inverse
        3-10 adds 1 (going past 9 resets to 0)
        11 uses the inverse
        12-19 subtracts 1 (going below 0 resets to 9)

        20 changes the default top middle to bottom middle, and bottom left and right to upper left and right
            and then uses the original seed
        21 uses inverse
        22-29 adds 1
        30 uses inverse
        31-38 subtracts 1

        39 changes bottom middle to right middle, upper left and right to upper and bottom middle
            and then uses original seed
        40 uses the inverse
        41-48 adds 1
        49 uses inverse
        50-57 subtracts 1

        58 changes right middle to middle and then uses original seed
        59 uses inverse
        60-67 adds 1
        68 uses inverse
        69-76 subtracts 1
        77 changes bottom and upper middle to left and right middle and uses original
        78 uses inverse
        79-86 adds 1
        87 uses inverse
        88-95 subtracts 1

        95-100 uses custom dungeon based on difficulty ( one idea)
        95-100 uses a slightly different algorithm (another idea)
        */
    static int[] seed = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    public static void defaultSeed(Vector3 top, Vector3 b_Right, Vector3 b_Left)
    {

    }

    //generates random number between max and min
    public static int randomGenerator(int minValue, int maxValue)
    {
        System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());
        
        int result = rand.Next(minValue, maxValue);

        return result;
    }

    //returns room size from RoomSizes
    public static int findRoomSize(char letter)
    {
        string[] roomSizes = Enum.GetNames(typeof(RoomSizes));

        for (int i = 0; i < roomSizes.Length; i++)
        {
            if (letter == roomSizes[i][0])
            {
                return indexRoomSizes()[i];
            }
        }

        throw new Exception("Seed Could Not Be Found");
    }

    //Stores actual values for room sizes
    private static int[] indexRoomSizes()
    {
        int[] roomSizes = new int[]{23, 24, 25, 33, 34, 35, 43, 44, 45, 53, 54, 55,
        63, 64, 65, 66};

        return roomSizes;
    }

    //int[] roomSizes = new int[]{11, 12, 13, 14, 15, 21, 22, 23, 24, 25, 31, 32, 33, 34, 35, 41, 42, 43, 44, 45,
       // 51, 52, 53, 54, 55};

    //List of letters a - i for use in the seed
    public enum RoomSizes
    {
        a,
        b,
        c,
        d,
        e,
        f,
        g,
        h,
        i,
        j,
        k,
        l,
        m,
        n,
        o,
        p,
        /*q,
        r,
        s,
        t,
        u,
        /*v,
        /*w,
        x,
        y,
        z*/
    };
}
