using System.Collections;
using System;
using System.Collections.Generic;

public static class GenerationSeed
{
    //Checks for generated seed
    private static bool checkForSeed(string seed)
    {
        if (string.IsNullOrEmpty(seed))
            return false;

        return true;
    }

    //Generates a room from a random cell on the grid
    public static string generateRoomSeed(int cellNumber)
    {
        int roomSize = (int)randomGenerator(0, Enum.GetNames(typeof(RoomSizes)).Length);

        string room = cellNumber.ToString() + Enum.GetName(typeof(RoomSizes), roomSize);

        return room;
    }

    //Used to check if a cell position is already the center of a room
    public static bool checkGeneratedRoomPosition(int roomPosition, List<int> generatedRooms)
    {
        if (roomPosition == -1)
            return false;

        foreach (int room in generatedRooms)
        {
            if (roomPosition == room)
            {
                return false;
            }
        }

        return true;
    }

    //generates random number between max and min
    public static int randomGenerator(int minValue, int maxValue)
    {
        System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());
        
        int result = rand.Next(minValue, maxValue);

        return result;
    }

    public static int breakDownSeed(string seed, out List<int> cell, out List<char> cellSizes, int roomsToGen, int toSkip)
    {
        cell = new List<int>();
        cellSizes = new List<char>();
        string cellNumber = "";
        int stopped = 0;
        int roomsgen = 0;

        for (int i = toSkip; i < seed.Length; i++)
        {
            int g = 0;

            if (int.TryParse(seed[i].ToString(), out g))
            {
                cellNumber += seed[i].ToString();
            }

            else
            {
                cell.Add(int.Parse(cellNumber));
                cellNumber = "";
                roomsgen += 1;
                cellSizes.Add(seed[i]);
            }

            if (roomsgen == roomsToGen)
            {
                i++;
                stopped = i;
                break;
            }

        }

        return stopped;
    }

    public static void breakDownHallSeed(int begin, string seed, out List<int> hallCells, int hallsToGen)
    {
        hallCells = new List<int>();
        string hallNumber = "";
        int hallsgen = 0;

        for (int i = begin; i < seed.Length; i++)
        {
            int h = 0;

            if (int.TryParse(seed[i].ToString(), out h))
            {
                hallNumber += seed[i].ToString();
            }

            else
            {
                hallCells.Add(int.Parse(hallNumber));
                hallNumber = "";
                hallsgen += 1;
            }

            if (hallsgen == hallsToGen * 2)
                break;
        }
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
