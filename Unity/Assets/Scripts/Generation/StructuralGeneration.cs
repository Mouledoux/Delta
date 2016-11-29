using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class StructuralGeneration : MonoBehaviour
{
    #region Description
    /*  This script is used to generate the structure of the dungeon.
        This consists of the floor, walls, rooms, hallways, and ceiling.
        How dungeon generation works:
        First a little lingo:
            Cell = a single rectangular FLOOR gameobject prefab (primitive cube with a texture for the floor)
            Grid = The large rectangular plane created from placing cells along adjacent X by Z positions. It is divided into even quadrants.
            Quadrant = A section of the grid containing a group of cells.
            Default Dungeon = A dungeon made up of the default room positions. It's further explained in the How the Seed Works section

        Alright, now onto the steps.
        Step 1: Generate a grid
        Step 2: Divide grid into even quadrants (specified as a variable)
        Step 3: Generate four walls around each cell (primitive cube with a texture for walls)
        Step 4: Define a default dungeon, thereby generating rooms
        Step 4: Use a seed to shift the positions of the default dungeon
        Step 5: Define the main rooms
        Step 6: Connect the main rooms using a main hall
        Step 7: Ensure every other room is connected in some way to the main hall, either directly or indirectly

        HOW THE SEED WORKS
    
            Step 1: Generate the quadrants of the grid
            Step 2: Find default room positions in each quadrant of the grid.
                - The 9 default room positions in a quadrant are: 
                //////////////////////////////////////////////////////  
                //// Top Left        Top Middle      Top Right,   ////      
                //// Middle Left     Center          Middle Right ////     
                //// Bottom Left     Bottom Middle   Bottom Right ////
                //////////////////////////////////////////////////////
                                                                        
                - Rooms are placed in these positions first (using a shuffle to random things up), and then transformed.

            Step 3: Execute transformations and exchanges based on numbers 0-9 to shift, scale, and switch rooms
            The seed is currently an int array with length 10.
            The first number is the current floor.
            Floors have an effect on the seed in a variety of ways:
            1. Grid size
            2. Rooms per quadrant
            3. Quadrants wanted
            4. Default Orientation
            The first nine places are used to transform the dungeon
            The tenth place determines the boss that spawns
            This array may be expanded to help with object generation if wanted (more details when I get to that)
            As stated previously, numbers 0-9 will be used for transformations and whatnot.
            They are currently used in the following manner:
            0 - shifts every room up in their QUADRANT by room number (if it reaches the top, it goes to the bottom)
            1 - shifts every number right in their QUADRANT room number (if it reaches the side, it goes to other the side)
            2 - shifts every room up on the entire GRID by room number (same rules as 0)
            3 - shifts every room right on the entire GRID by room number (same rules as 1)
            4 - switches room numbers and sizes within QUADRANT
            5 - switches room numbers and sizes among the GRID
            6 - Elevates rooms
            7 - Rotates the grid 90 degrees
            8 - Not currently in use
            9 - Not currently in use
            
            *Every shift also scales rooms by room number (0-x; x = however many rooms we want to generate)
            Adding a negative to these numbers processes the command in the opposite manner (Though it doesn't do this now)
        *Note that all of the above is subject to change...and probably will*
            
        One seed is used for the entire dungeon.
        Tier I
        Level 1 uses the original default setup
        Levels 2-8 changes the default dungeon orientation
        Level 9 shuffles the room numbers
        Level 10 uses default setup
        Level 11-17 changes default orientation
        Level 18 shuffles room numbers
        Level 19-20 changes default orientation
        Tier II
        Level 21 changes the grid size and roomsPerQuadrant. Uses default room orientation
        Level 22-28 changes the default dungeon orientation
        Level 29 shuffles the room numbers
        Level 30 uses default setup
        Level 31-37 changes default orientation
        Level 38 shuffles the room numbers
        Level 39-40 changes default orientation
        Tier III
        Level 41 changes number of quadrants. Uses default orientation
        Level 42-60 same as Tier II
        Tier IV
        Level 61 changes number of quadrants and roomsPerQuadrant. Uses default room orientation
        Level 62-80 are the same as Tier III
        Tier V
        Level 81 changes grid size. Use default room orientation
        Level 82-97 are the same as Tier IV
        Level 98 changes the grid size and shuffles the room numbers
        Level 99 changes the number of quadrants
        Level 100 changes the roomsPerQuadrant
        
         
        Note that the above is an idea, but the final generation will involve similar structure
     * This script needs to:
     * Generate a grid
     *  - public ints for changing the grid size to meet whatever specifications we want
     *  - Grid is simply going to be x and y (z for unity) number of dungeon cells put together to make a large rectangle
     * Generate dungeon cells
     *  - Determine what objects go in each cell if any
     *  - Subdivide the cells after creation
     * Generate path
     *  - Remove walls to create hallways and rooms
     *  
     * Generate seed
     *
     * This script can currently:
     * Generate the grid.
     * Generate basic cells
     * Generate walls around cells
     * Generate rooms
     * Generate Seed
     * Generate Dungeon during runtime
     * Generae hallway (un-automated)
     * 
     * 
     * 
     * 
     * 
     * 
     */
    //After Global Variables, scroll to Update to begin tracking code.
    #endregion

    #region Global Variables

    //Cells and Grid
    public bool Generate;           //Determines whether or not to generate
    public int xCells, zCells;    //Number of cells generated along x and z axis
    public int xQuadrants, zQuadrants;
    public int roomsPerQuadrant;    //Number of rooms allowed per quadrant
    public Vector3 cellSize;        //Determines the size of individual cells (testing only)
    public float cellWallHeight;    //determines height of cell walls
    public float cellWallWidth;     //determines width of cell walls

    //Game Objects
    public GameObject FLOOR;                //Floor gameobject
    public GameObject WALL;                 //Wall gameobject
    private static List<GameObject> cells;  //Tracks all of the cell gameobjects used in the game

    //Boundary Tracking
    public struct boundaries { public float north, south, east, west; }
    boundaries gridBoundaries = new boundaries();
    private List<boundaries> QuadrantBoundaries = new List<boundaries>();     //Tracks furthest points of each quadrant

    //Seed and Generation
    private int[] seed = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };    //stores seed
    public string seedString;                                           //Displays seed as a string
    public bool realTimeGen;                                            //Coroutine vs Instant generation
    public float numberofRooms;                                         //Outputs number of rooms (debugging)
    public double floorFactor;
    public int floor = 0;

    //Rooms | Halls | Stairs
    public int minRoomSize;                                             //minimum room size
    public int maxRoomSize;                                             //maximum room size

    public int largestRoomCount;                                        //displays the number of cells in largest room

    public static bool structureDone;
    public static bool regenerate;

    #endregion

    #region Update
    //Contains functions that check for inputs pertaining to generating

    public int s1, s2;  //stair 1 and 2. Testing purposes
    public float h;     //elevate. Testing purposes

    //Checks for inputs for generating new dungeons or clearing old ones
    void Update()
    {
        if (Generate)   //If we're generating
        {
            structureDone = false;
            GenerateGrid();             //Generate  the grid and get the boundaries
            generateCellWalls(cells);   //Generate walls around each cell on the grid

            StartCoroutine(WaitSecondsVoid(1.0f, GenerateDungeon));                 //Begin dungeon generation process
            Generate = false;                                                       //Set generate to false
        }

        if (Input.GetKeyDown(KeyCode.G))    //Used to generate a new dungeon during runtime
        {
            ClearDungeon();                 //Clear everything
            Generate = true;                //Set generate to true
            seedString = getDefaultSeed();
            seedStringToSeed();
        }

        if (Input.GetKeyDown(KeyCode.C))    //Clear the grid
        {
            ClearDungeon();
            GenerateGrid();
            generateQuadrants();                    //Generate new quadrants
            generateCellWalls(cells);               //Generate cell walls
            structureDone = false;
        }

    }

    #endregion

    #region Generate Dungeon

    //Runs necessary functions to generate a grid
    private void GenerateGrid()
    {
        generateQuadrants();
        gridBoundaries = calculateBoundary(cells); //Get the furthest world positions of the grid in each direction
    }

    //Runs necessary checks to clear everything
    public static void ClearDungeon()
    {
        if (GameObject.Find("Cells"))           //If cells exist, destroy them
            Destroy(GameObject.Find("Cells"));

        if (GameObject.Find("Rooms"))           //If rooms exist, destroy them
            Destroy(GameObject.Find("Rooms"));

        if (GameObject.Find("Stairs"))          //If stairs exist, destroy them
            Destroy(GameObject.Find("Stairs"));
        if (GameObject.Find("Halls"))
            Destroy(GameObject.Find("Halls"));
    }

    //Runs necessary functions to build the dungeon
    private void GenerateDungeon()
    {
        List<Vector3> roomPositions;    //Track room positions
        int rotationNum;                //Determines how many times the grid will rotate after generation
        int[] defaultlayout;            //Used for getting the default layout positions
        int[] roomNum;                  //Track room numbers
        int[] curRoomSizes;             //Track current room sizes
        List<RoomSizes> roomSizes;      //Get list of allowed room sizes

        //Initialize the above variables
        InitializeGeneration(out roomPositions, out defaultlayout, out roomNum, out curRoomSizes, out roomSizes);

        //Get the default quadrant positions for rooms
        getDefaultPositions(roomPositions, defaultlayout, roomNum, curRoomSizes);

        //Shuffle room numbers
        roomNum = UniversalHelper.Shuffle(roomNum);

        //Generate the seed
        GenerateSeed();

        //Determine final room positions
        roomPositions = breakdownSeed(roomPositions, roomNum, curRoomSizes, roomSizes.Count, out rotationNum);

        if (realTimeGen)    //If we're generating in real time, generate in coroutine
            StartCoroutine(RealTimeGenerator(roomPositions, roomSizes, curRoomSizes, rotationNum));

        else            //Otherwise generate everything instantly
            GenerateRooms(roomPositions, curRoomSizes, roomSizes);

    }

    //Used by Generate Dungeon to initialize variables
    private void InitializeGeneration(out List<Vector3> roomPositions, out int[] defaultlayout, out int[] roomNum, out int[] curRoomSizes, out List<RoomSizes> roomSizes)
    {
        int QuadrantsWanted = xQuadrants * zQuadrants;
        if (roomsPerQuadrant > 9)                                           //Ensure roomsPerQuadrant <= 9
            roomsPerQuadrant = 9;                                           //since there are only 9 default positions

        roomPositions = new List<Vector3>();                                //Create a list of Vector3s to store positions in world
        defaultlayout = new int[roomsPerQuadrant];                          //Determines how many of the default layouts are used
        roomNum = new int[roomsPerQuadrant * QuadrantsWanted];              //Total number of rooms to generate
        curRoomSizes = new int[roomsPerQuadrant * QuadrantsWanted];         //For each room there is a room size
        roomSizes = getRoomSizes();                                         //Get room sizes
        //Get the list of room sizes
    }

    //Used by Generate Dungeon to get default room positions
    private void getDefaultPositions(List<Vector3> roomPositions, int[] defaultlayout, int[] roomNum, int[] curRoomSizes)
    {
        int QuadrantsWanted = xQuadrants * zQuadrants;
        for (int i = 0; i < defaultlayout.Length && i < 9; i++)  //While i < the number of rooms per quadrant or 9
        {
            defaultlayout[i] = i + 4;                   //Set the default position of each room in the quadrant

            if (defaultlayout[i] > 8)                   //If i is greater than 8
                defaultlayout[i] -= 9;                  //Subtract 9
        }

        int s = 0;                                  //Tracks the room numbers to be assigned

        for (int i = 0; i < QuadrantsWanted; i++)   //while i < the number of quadrants wanted
        {
            List<Vector3> positions = getDefaultRooms(QuadrantBoundaries[i]); //Get the default room positions based on
                                                                              //quadrant size and number of quadrants

            for (int x = 0; x < roomsPerQuadrant; x++)  //for every room in each quadrant
            {
                roomPositions.Add(positions[defaultlayout[x]]); //Add the default positions
                curRoomSizes[s] = 0;                            //Set the room size equal to the smallest size
                roomNum[s] = s;                                 //Room number is equal to s
                s++;                                            //increment s

            }
        }
    }


    //Used by Generate Dungeon to generate seed
    private void GenerateSeed()
    {
        if (seedString != getDefaultSeed())     //If manual seed is input
        {
            //Do nothing. Seed is already set
        }

        else                                        //Otherwise
        {
            for (int i = 1; i < seed.Length; i++)   //While i is < than seed length
            {
                seed[i] = UniversalHelper.randomGenerator(0, 7);    //Generate a number between 0 and 7. Will be 9 with all transformations
            }
        }

    }

    //Used by Generate Dungeon to break down the seed
    private List<Vector3> breakdownSeed(List<Vector3> positions, int[] roomNum, int[] curRoomSizes, int roomSizeList,
        out int rotationNum)
    {
        rotationNum = 0;                        //Number of times to rotate
        for (int i = 1; i < seed.Length; i++)   //While i < than seed length
        {
            switch (seed[i])
            {
                case 0:
                    positions = ShiftRooms(positions, roomNum.ToList(), 0, true);   //Shift rooms up in quadrant
                    curRoomSizes = ScaleRooms(curRoomSizes, roomNum, roomSizeList);     //Scale rooms
                    break;

                case 1:
                    positions = ShiftRooms(positions, roomNum.ToList(), 1, true);   //Shift rooms right in quadrant
                    curRoomSizes = ScaleRooms(curRoomSizes, roomNum, roomSizeList);     //Scale rooms
                    break;

                case 2:
                    positions = ShiftRooms(positions, roomNum.ToList(), 0, false);  //Shift rooms up on grid
                    curRoomSizes = ScaleRooms(curRoomSizes, roomNum, roomSizeList);     //Scale rooms
                    break;

                case 3:
                    positions = ShiftRooms(positions, roomNum.ToList(), 1, false);  //Shift rooms right on grid
                    curRoomSizes = ScaleRooms(curRoomSizes, roomNum, roomSizeList);     //Scale rooms
                    break;

                case 4:
                    if (roomsPerQuadrant > 3)   //If there are more than 3 rooms in a quadrant
                        roomNum = ShuffleRooms(roomNum, true);  //Shuffle the room numbers in the quadrant

                    else                        //Otherwise
                        roomNum = ShuffleRooms(roomNum, false); //Shuffle the room numbers across the grid
                    break;

                case 5:
                    roomNum = ShuffleRooms(roomNum, false);     //Shuffle the room numbers across the grid
                    break;

                case 6:
                    break;

                case 7:
                    rotationNum += 1;                           //Add 1 to rotation
                    break;

                case 8:
                    break;

                case 9:
                    break;

                default:
                    positions = ShiftRooms(positions, roomNum.ToList(), 0, true);   //Shift rooms up in quadrant
                    curRoomSizes = ScaleRooms(curRoomSizes, roomNum, roomSizeList);     //Scale rooms
                    break;
            }

        }

        return positions;   //return new positions
    }

    //Used by Generate Dungeon to generate rooms
    private void GenerateRooms(List<Vector3> roomPositions, int[] curRoomSizes, List<RoomSizes> roomSizes)
    {
        List<List<GameObject>> rooms = new List<List<GameObject>>();

        for (int i = 0; i < roomPositions.Count; i++)   //For each room position
        {
            int[] roomSize = new int[] { roomSizes[curRoomSizes[i]].sizeX, roomSizes[curRoomSizes[i]].sizeZ };

            rooms.Add(CreateRoom(roomPositions[i], roomSize));                  //Create the room
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            UniversalHelper.parentObject(rooms[i], "Rooms", "Room " + i);
        }

        numberofRooms = rooms.Count;                 //Display the number of rooms generated
    }

    //Used by GenerateRooms to create a room
    private List<GameObject> CreateRoom(Vector3 roomPosition, int[] roomSize)
    {

        Vector3 returnPos = roomPosition;   //Get the rooms center position
        List<GameObject> MergedRooms;           //Any rooms that share the cells of this room will be merged
                                                //into this one

        //Generate the room and store the cells in a list
        List<GameObject> room = generateRoom(roomPosition, roomSize[0], roomSize[1], out returnPos, out MergedRooms);

        if (MergedRooms.Count != 0) //If there are rooms to merge with this room
        {
            List<GameObject> newRoom = room;          //Create a reference to the created room
            for (int x = 0; x < MergedRooms.Count; x++) //For each room in the list of rooms to be merged
            {
                foreach (Transform cell in MergedRooms[x].transform)    //For each cell in those rooms
                {
                    newRoom.Add(cell.gameObject);                           //Add them to the new room
                }
                //GameObject.Destroy(MergedRooms[x]);                     //Destroy the old room objects
            }
        }

        return room;
    }

    //Used by GenerateRooms to organize game objects in the hierarchy
    private static void FinalizeList(GameObject Rooms, List<GameObject> AllRooms)
    {
        for (int i = 0; i < Rooms.transform.childCount; i++)    //For each room in Rooms  
        {
            Rooms.transform.GetChild(i).name = "Room " + i;         //Get the room and rename it so that it's orderly
            AllRooms.Add(Rooms.transform.GetChild(i).gameObject);   //Add the room to the new list
        }
    }

    //Determines which rooms get priority during hall generation
    private int[] determineHallGen()
    {
        int[] numOfRooms = Enumerable.Range(0, GameObject.Find("Rooms").transform.childCount).ToArray(); //Get the number of rooms generated

        if (numOfRooms.Length >= 4)
            return UniversalHelper.Shuffle(numOfRooms);

        return numOfRooms;
    }

    void GenerateHalls(int[] numOfRooms)
    {
        for (int i = 0; i < numOfRooms.Length - 1; i++)
        {
            GameObject start = GameObject.Find("Room " + numOfRooms[i]);
            GameObject end = GameObject.Find("Room " + numOfRooms[i+1]);

            CreateHall(start, end);
        }
    }

    //Takes two game cells and creates a hall using the grid
    void CreateHall(GameObject start, GameObject end)
    {

        Vector3 startVec = start.transform.position;
        Vector3 endVec = end.transform.position;



        float xDistance, zDistance;

        xDistance = Mathf.Abs(startVec.x - endVec.x) / returnCellSizex; //Number of x cells to travel
        zDistance = Mathf.Abs(startVec.z - endVec.z) / returnCellSizez; //Number of z cells to travel

        char[] direction = UniversalHelper.detectDirection(start, end);

        List<GameObject> travelCells = new List<GameObject>();

        DetermineTravelCells(startVec, xDistance, zDistance, direction, travelCells);

        if (travelCells.Count > 1)
        {

            for (int i = 0; i < travelCells.Count - 1; i++)
            {
                ConnectCell(travelCells[i], travelCells[i + 1]);
            }
            UniversalHelper.parentObject(travelCells, "Halls", start.name + " | " + end.name + " Hall");
        }
    }

    //Determines route through grid to create a hallway
    private void DetermineTravelCells(Vector3 startVec, float xDistance, float zDistance, char[] direction, List<GameObject> travelCells)
    {
        Vector3 n = new Vector3(0, 0, returnCellSizez);
        Vector3 s = new Vector3(0, 0, -returnCellSizez);
        Vector3 e = new Vector3(returnCellSizex, 0, 0);
        Vector3 w = new Vector3(-returnCellSizex, 0, 0);

        Vector3 pos = startVec;

        if (xDistance > zDistance)
        {
            pos = AddNSCells(zDistance, direction, travelCells, n, s, pos);
            pos = AddEWCells(xDistance, direction, travelCells, e, w, pos);
        }

        else if (zDistance > xDistance)    //Reverse the process
        {
            pos = AddNSCells(zDistance, direction, travelCells, n, s, pos);
            pos = AddEWCells(xDistance, direction, travelCells, e, w, pos);
        }
    }

    private Vector3 AddEWCells(float xDistance, char[] direction, List<GameObject> travelCells, Vector3 e, Vector3 w, Vector3 pos)
    {
        if (direction[0] == 'e')
        {
            for (int i = 0; i < xDistance; i++)
            {
                travelCells.Add(findCell(pos + e));
                pos = travelCells[travelCells.Count - 1].transform.position;
            }
        }

        else if (direction[0] == 'w')
        {
            for (int i = 0; i < xDistance; i++)
            {
                travelCells.Add(findCell(pos + w));
                pos = travelCells[travelCells.Count - 1].transform.position;
            }
        }

        return pos;
    }

    private Vector3 AddNSCells(float zDistance, char[] direction, List<GameObject> travelCells, Vector3 n, Vector3 s, Vector3 pos)
    {
        if (direction[1] == 'n')
        {
            for (int i = 0; i < zDistance; i++)
            {
                travelCells.Add(findCell(pos + n));
                pos = travelCells[travelCells.Count - 1].transform.position;
            }
        }

        else if (direction[1] == 's')
        {
            for (int i = 0; i < zDistance; i++)
            {
                travelCells.Add(findCell(pos + s));
                pos = travelCells[travelCells.Count - 1].transform.position;
            }
        }

        return pos;
    }

    //Takes two cells and removes the walls between them. Use this iteratively to connect hallways
    void ConnectCell(GameObject cellOne, GameObject cellTwo)
    {
        char[] directions = UniversalHelper.detectDirection(cellOne, cellTwo);  //Directional relation between cells


        if (directions[0] == 'u' && directions[1] != 'u')
        {

        }

        else if (directions[1] == 'u' && directions[0] != 'u')
        {

        }

        else
        {

            throw new Exception("can't connect walls");
        }

        switch (directions[0])
        {
            case 'e':                                               //East
                if (cellOne.transform.Find("east_wall"))            //Destroy cell one's east wall if it exists
                {
                    Destroy(cellOne.transform.Find("east_wall").gameObject);
                }

                if (cellTwo.transform.Find("west_wall"))            //Destroy cell two's west wall if it exists
                {
                    Destroy(cellTwo.transform.Find("west_wall").gameObject);
                }
                break;

            case 'w':                                               //West
                if (cellOne.transform.Find("west_wall"))            //Destroy cell one's west wall if it exists
                {
                    Destroy(cellOne.transform.Find("west_wall").gameObject);
                }

                if (cellTwo.transform.Find("east_wall"))            //Destroy cell two's east wall if it exists
                {
                    Destroy(cellTwo.transform.Find("east_wall").gameObject);
                }
                break;

            default:
             //   Debug.Log("Nothing destroyed");
                break;
        }

        switch (directions[1])
        {
            case 'n':
                if (cellOne.transform.Find("north_wall"))            //Destroy cell one's north wall if it exists
                {
                    Destroy(cellOne.transform.Find("north_wall").gameObject);
                  //  Debug.Log("destroyed");
                }

                if (cellTwo.transform.Find("south_wall"))            //Destroy cell two's south wall if it exists
                {
                    Destroy(cellTwo.transform.Find("south_wall").gameObject);
               //     Debug.Log("destroyed");
                }
                break;

            case 's':
                if (cellOne.transform.Find("south_wall"))            //Destroy cell one's south wall if it exists
                {
                    Destroy(cellOne.transform.Find("south_wall").gameObject);
                 //   Debug.Log("destroyed");
                }

                if (cellTwo.transform.Find("north_wall"))            //Destroy cell two's north wall if it exists
                {
                    Destroy(cellTwo.transform.Find("north_wall").gameObject);
                 //   Debug.Log("destroyed");
                }
               // Debug.Log("ran s");
                break;

            default:

                break;
        }
    }
    #endregion

    #region Post Generation
    void PushFromCenter()
    {
        //Function pushes all rooms away from center of grid
        float NgridMid = (gridBoundaries.north + gridBoundaries.south) / 2;
        float EgridMid = (gridBoundaries.east + gridBoundaries.west) / 2;

        GameObject Rooms = GameObject.Find("Rooms");

        for (int i = 0; i < Rooms.transform.childCount; i++)
        {
            if (Rooms.transform.GetChild(i).transform.position.z < NgridMid)
            {
                Rooms.transform.GetChild(i).transform.position += new Vector3(0, 0, -returnCellSizez);
            }

            else
            {
                Rooms.transform.GetChild(i).transform.position += new Vector3(0, 0, returnCellSizez);
            }

            if (Rooms.transform.GetChild(i).transform.position.x < EgridMid)
            {
                Rooms.transform.GetChild(i).transform.position += new Vector3(-returnCellSizex, 0, 0);
            }

            else
            {
                Rooms.transform.GetChild(i).transform.position += new Vector3(returnCellSizex, 0, 0);
            }
        }
    }

    void BuildStair(GameObject topCell, GameObject bottomCell)
    {
        //Function builds stairs between two cells.
        //Takes two parameters for the cells, and either connects them directly or creates intermediate
        //positions and then connects them

        float maxStairX = returnCellSizex * 2;              //A single stair cannot extend on the x-axis beyond this
        float maxStairZ = returnCellSizez * 2;              //A single stair cannot extend on the z-axis beyond this
        float maxStairHeight = cellWallHeight;              //A single stair cannot rise above this height

        float stepLength;                                   //Length of steps
        float stepHeight;                                   //Height of steps
        float height;                                       //Height of stair

        Vector3 topPos = topCell.transform.position;
        Vector3 lowPos = bottomCell.transform.position;

        List<float> length;                                 //Length of each stair to connect positions
        List<float> width;                                  //Width of each stair to connect positions

        List<char> dir;                                     //Direction of each stair

        int numOfStairs;

        //Returns the direction of each stair as well as the length and width
        Vector3 distance = StairDirection(topPos, lowPos, out length, out width, out dir, out numOfStairs, out height)[0];

        if (numOfStairs > 1)
        {
            switch (dir[0])
            {
                case 'n':
                    lowPos = moveCellPosition(2, lowPos, false);
                    break;
                case 's':
                    lowPos = moveCellPosition(0, lowPos, false);
                    break;
                case 'e':
                    lowPos = moveCellPosition(3, lowPos, false);
                    break;
                case 'w':
                    lowPos = moveCellPosition(1, lowPos, false);
                    break;
            }
        }
        int numberOfSteps = 13;                 //number of steps in stair
        stepHeight = height / numberOfSteps;    //height of step
        stepLength = length[0] / numberOfSteps; //length of step

        for (int x = 0; x < numOfStairs; x++)
        {

            List<GameObject> steps = new List<GameObject>();    //Game objects for each step

            for (int i = 1; i <= numberOfSteps; i++)
            {
                GameObject step = GameObject.CreatePrimitive(PrimitiveType.Cube);   //Creates a cube

                float set;      //Used to offset the steps so that they form correctly from cell to cell
                Vector3 offSet; //Transforms set into a vector so that it can be added to step.transform.position

                float temp = (stepLength * i) / 2;                      //how far steps need to move to be uniform
                Vector3 tempVector = temp * distance;                   //Transform temp into a vector

                if (dir[0] == 'w' || dir[0] == 'e') //If direction is west or east
                {
                    //The length will be the x, height will be y, width will be z
                    step.transform.localScale = new Vector3(length[0] - (stepLength * i), stepHeight, width[0]);

                    set = (length[0] - returnCellSizez * numOfStairs) * 0.5f; //line stair up correctly

                    if (dir[0] == 'e')  //If direction is east
                        offSet = new Vector3(set, stepHeight * i, 0);    //offset is positive
                    else
                        offSet = new Vector3(-set, stepHeight * i, 0);   //else it is negative

                    if (i == 1 && x >= 1)
                    {
                        step.transform.localScale = new Vector3(returnCellSizex, height + stepHeight, width[0]);

                        if (dir[0] == 'w')
                            tempVector -= new Vector3(-0.2f, height / 2, 0);

                        else
                            tempVector += new Vector3(-0.2f, -(height / 2), 0);
                    }

                    else if (i == numberOfSteps)
                    {
                        step.transform.localScale = new Vector3(returnCellSizex, height, width[0]);

                        if (dir[0] == 'w')
                            tempVector -= new Vector3(step.transform.localScale.x / 2, height / 2 - (stepHeight / 2), 0);

                        else
                            tempVector -= new Vector3(-(step.transform.localScale.x / 2), height / 2 - (stepHeight / 2), 0);
                    }
                }

                else    //If direction is north or south
                {
                    //The width will be x, height is y, length is z
                    step.transform.localScale = new Vector3(width[0], stepHeight, length[0] - (stepLength * i));

                    set = (length[0] - returnCellSizex) * 0.5f; //line up stair

                    if (dir[0] == 'n')  //If direction is north
                        offSet = new Vector3(0, stepHeight * i, set);    //positive offset
                    else
                        offSet = new Vector3(0, stepHeight * i, -set);   //negative offset

                    if (i == 1 && x >= 1)
                    {
                        step.transform.localScale = new Vector3(width[0], height + stepHeight, returnCellSizez);

                        if (dir[0] == 's')
                            tempVector -= new Vector3(0, height / 2, -0.2f);

                        else
                            tempVector += new Vector3(0, -(height / 2), -0.2f);
                    }

                    else if (i == numberOfSteps)
                    {
                        step.transform.localScale = new Vector3(width[0], height, returnCellSizez);
                        if (dir[0] == 's')
                            tempVector -= new Vector3(0, height / 2 - (stepHeight / 2), step.transform.localScale.z / 2);
                        else
                            tempVector -= new Vector3(0, height / 2 - (stepHeight / 2), -(step.transform.localScale.z / 2));
                    }
                }

                step.transform.position = lowPos + offSet + tempVector;   //Move step to correct position
                step.name = "Step " + i;                                                //Name step
                steps.Add(step);                                                        //Add step to List
            }

            UniversalHelper.parentObject(steps, "Stairs", "Steps " + x);   //Parent steps

            if (numOfStairs - 1 != x)
                switch (dir[0])
                {
                    case 'n':
                        lowPos = moveCellPosition(0, lowPos, false, 2);
                        break;
                    case 's':
                        lowPos = moveCellPosition(2, lowPos, false, 2);
                        break;
                    case 'e':
                        lowPos = moveCellPosition(1, lowPos, false, 2);
                        break;
                    case 'w':
                        lowPos = moveCellPosition(3, lowPos, false, 2);
                        break;

                    default:
                        break;
                }

            lowPos += new Vector3(0, height, 0);
        }

    }

    List<Vector3> StairDirection(Vector3 topPos, Vector3 lowPos, out List<float> length, out List<float> width,
        out List<char> dir, out int numberOfStairs, out float height)
    {
        //Function determines which direction the Vector3 distance variable will shift steps in
        //as well as the scale of the steps
        float xTop = topPos.x;
        float xLow = lowPos.x;
        float zTop = topPos.z;
        float zLow = lowPos.z;

        numberOfStairs = 1;

        height = topPos.y - lowPos.y;

        numberOfStairs = Convert.ToInt32(Mathf.Ceil(height / cellWallHeight));
        numberOfStairs = Mathf.Clamp(numberOfStairs, 1, 100);
        height /= numberOfStairs;

        length = new List<float>();
        width = new List<float>();
        dir = new List<char>();

        List<Vector3> distance = new List<Vector3>();

        if (xTop != xLow)
        {
            if (xTop > xLow)
            {
                distance.Add(new Vector3(1, 0, 0));    //Shift steps right
                dir.Add('e');
            }

            else
            {
                distance.Add(new Vector3(-1, 0, 0));   //Shift steps left
                dir.Add('w');
            }

            length.Add(Mathf.Abs(topPos.x - lowPos.x) / numberOfStairs);
            width.Add(returnCellSizez);
        }

        if (zTop != zLow)
        {
            if (zTop > zLow)
            {
                distance.Add(new Vector3(0, 0, 1));    //Shift steps up
                dir.Add('n');
            }

            else
            {
                distance.Add(new Vector3(0, 0, -1));   //Shift steps down 
                dir.Add('s');
            }

            length.Add(Mathf.Abs(topPos.z - lowPos.z) / numberOfStairs);
            width.Add(returnCellSizex);
        }

        return distance;
    }

    void ElevateDungeon(List<GameObject> cells, float height)
    {
        foreach (GameObject cell in cells)
        {
            cell.transform.position += new Vector3(0, height, 0);
        }
    }
    #endregion

    #region Generate Grid

    /*private List<GameObject> generateGrid()
    {
        //Function generates the grid


        List<GameObject> cells = new List<GameObject>(); //used for keeping track of individual cells

        int x_halfcells = xCells / 2; //determines the distance from the center of the grid to the side edge.
        int z_halfcells = zCells / 2; //determines the distance from the center of the grid to the top/bottom edge

        Vector3 startingGridPosition = Vector3.zero;    //The grid's bottom edge cells will center on the origin
        Vector3 cellSpace = cellSize * 10;              //calculate how much world space a single cell takes up

        int cellName = 0;   //Initialize cell names

        for (int i = 0; i < zCells; i++)       //Row
        {
            Vector3 currentGridPosition = startingGridPosition;     //keeps track of the current world position
            currentGridPosition.x += (x_halfcells * cellSpace.x);   //starting from the bottom right corner of the grid

            if (i != 0) //If it isn't the first row
            {
                currentGridPosition.z += cellSpace.z * i; //move the grid position up along z-axis
            }

            for (int x = 0; x < xCells; x++)   //Column
            {
                GameObject cell = Instantiate(FLOOR);           //Create new cell
                cell.name = "cell " + cellName;                 //name the cell for easier reference
                cellName++;                                     //Increment cellName  
                cell.transform.position = currentGridPosition;  //bring cell to current position
                cell.transform.localScale = cellSpace;          //set cell to the size determined
                cells.Add(cell);                                //add cell to cells List
                currentGridPosition.x -= cellSpace.x;           //update currentGridPosition to next empty spot
            }

        }

        return cells;   //Return list of cells
    }*/

    void generateCellWalls(List<GameObject> a_cells)
    {
        //Function correctly positions cell walls


        //calculate the size of the north/south walls of a cell
        Vector3 nsCellWallSize = new Vector3(returnCellSizex, cellWallHeight, cellWallWidth);

        //calculate the size of the east/west walls of a cell
        Vector3 ewCellWallSize = new Vector3(cellWallWidth, cellWallHeight, returnCellSizez);

        float nsMovePosition = 10 * (cellSize.z / 2); //tracks the space required to move to the n/s edge of a cell
        float ewMovePosition = 10 * (cellSize.x / 2); //tracks the space required to move to the e/w edge of a cell

        for (int i = 0; i <= a_cells.Count - 1; i++)
        {
            //Set current position to center of cell in world space
            Vector3 currentCellPosition = a_cells[i].transform.position;

            //Generates walls
            GameObject[] wall = cellWallGenerator(currentCellPosition, nsMovePosition, ewMovePosition,
                                                    nsCellWallSize, ewCellWallSize);
            for (int x = 0; x < wall.Length; x++)
            {
                wall[x].transform.parent = a_cells[i].transform;    //Set parent of walls to appropriate cell
            }

            #region UNCOMMENT THIS IF YOU WANT TO PUT CELL WALLS IN A SEPARATE GAME OBJECT FOR SOME REASON

            //GameObject cellWallParent = new GameObject(); cellWallParent.name = "Cell Walls";
            //parentingGameObjects(l_cellWall, a_cells[i].name + "_walls").transform.parent = cellWallParent.transform;

            #endregion
        }
    }


    private GameObject[] cellWallGenerator(Vector3 cellPosition, float nsCellWallMovePosition,
                                          float ewCellWallMovePosition, Vector3 nsCellWallSize, Vector3 ewCellWallSize)
    {
        //Function generates cell walls

        //Instantiate 4 wall prefabs
        GameObject[] wall = new GameObject[] { Instantiate(WALL), Instantiate(WALL), Instantiate(WALL), Instantiate(WALL) };

        wall[0].name = "north_wall";    //identifies north wall
        wall[1].name = "east_wall";     //identifies east wall
        wall[2].name = "south_wall";    //identifies south wall
        wall[3].name = "west_wall";     //identifies west wall

        wall[0].transform.localScale = nsCellWallSize; //adjust scale for north wall
        wall[1].transform.localScale = ewCellWallSize; //adjust scale for east wall
        wall[2].transform.localScale = nsCellWallSize; //adjust scale for south wall
        wall[3].transform.localScale = ewCellWallSize; //adjust scale for west wall

        for (int x = 0; x < wall.Length; x++)
        {
            //moves wall so that the base rests on top of the plane
            wall[x].transform.position = cellPosition + new Vector3(0.0f, wall[x].transform.localScale.y / 2, 0.0f);
        }

        wall[0].transform.position += new Vector3(0.0f, 0.0f, nsCellWallMovePosition); //Move north wall to front of cell
        wall[1].transform.position += new Vector3(ewCellWallMovePosition, 0.0f, 0.0f); //Move east wall to right of cell
        wall[2].transform.position -= new Vector3(0.0f, 0.0f, nsCellWallMovePosition); //Move south wall to back of cell
        wall[3].transform.position -= new Vector3(ewCellWallMovePosition, 0.0f, 0.0f); //Move west wall to left of cell

        return wall;    //return walls
    }

    private boundaries calculateBoundary(List<GameObject> a_cells)
    {
        //Function returns the furthest world positions in all directions
        //0 = north, 1 = south, 2 = east, 3 = west

        boundaries boundary = new boundaries();
        boundary.north = boundary.east = -2000;
        boundary.west = boundary.south = 2000;

        for (int i = 0; i < a_cells.Count; i++)
        {
            if (a_cells[i].transform.position.x > boundary.east)    //If the cell is furthest east
            {
                boundary.east = a_cells[i].transform.position.x;        //set furthest east to cell position
            }

            if (a_cells[i].transform.position.x < boundary.west)    //If the cell is furthest west
            {
                boundary.west = a_cells[i].transform.position.x;        //set furthest west to cell position
            }

            if (a_cells[i].transform.position.z > boundary.north)    //If the cell is furthest north
            {
                boundary.north = a_cells[i].transform.position.z;        //set furthest north to cell position
            }

            if (a_cells[i].transform.position.z < boundary.south)    //If the cell is furthest south
            {
                boundary.south = a_cells[i].transform.position.z;        //set furthest south to cell position
            }
        }

        return boundary;  //return furthest points
    }
    #endregion
    #region Generate Quadrants
    void generateQuadrants()
    {
        cells = new List<GameObject>();
        int xWidth = xQuadrants * (xCells * returnCellSizex);
        int Height = zQuadrants * (zCells * returnCellSizez);
        Vector3 startPosition = Vector3.zero;
        startPosition.x -= xWidth;
        Vector3 currentPos = startPosition;
        gridBoundaries.south = 0;
        for (int zQ = 0; zQ < zQuadrants; zQ++)
        {
            for (int xQ = 0; xQ < xQuadrants; xQ++)
            {
                QuadrantBoundaries.Add(new boundaries());
                List<GameObject> Quadrant = new List<GameObject>();

                for (int z = 0; z < zCells; z++)
                {
                    
                    currentPos = startPosition + new Vector3((xCells * returnCellSizex * xQ) + returnCellSizex, 0, ((zCells * returnCellSizez * zQ)) + (returnCellSizez * z));

                    for (int x = 0; x < xCells; x++)
                    {
                        currentPos += new Vector3(returnCellSizex,0,0);                
                        GameObject cell = Instantiate(FLOOR);
                        cell.transform.position = currentPos;  //bring cell to current position
                        cell.transform.localScale = cellSize * 10;
                        cell.name = "cell " + cells.Count + " Z" + zQ + "X" + xQ;
                        cells.Add(cell);                                //add cell to cells List
                        Quadrant.Add(cell);
                    }
                }

                QuadrantBoundaries[QuadrantBoundaries.Count - 1] = calculateBoundary(Quadrant);

                UniversalHelper.parentObject(Quadrant, "Cells", "Quadrant " + zQ + xQ);
            }
        }
    }

    public int evenQuadrant()
    {
        int QuadrantsWanted = xQuadrants * zQuadrants;
        double temp = Math.Sqrt(QuadrantsWanted);

        if (QuadrantsWanted % 2 != 0 && temp % 1 != 0)
        {
            QuadrantsWanted++;
        }

        return QuadrantsWanted;
    }
    #endregion

    #region Generate Rooms

    private List<GameObject> generateRoom(Vector3 roomCenter, int roomSizex, int roomSizez, out Vector3 returnCenter, out List<GameObject> MergedRooms)
    {
        //Function marks cells for rooms.
        //Parameters: 
        //a position in world space for the center
        //size of the room (cells along x and z)
        //quadrant of room,
        //Vector3 variable to save the position of the center if it changes

        MergedRooms = new List<GameObject>();                       //Determines if rooms need to be merged
        Vector3 roomToMergePosition = Vector3.zero;                 //Gets room that needs to be merged
        int zCheck = ((roomSizez) / 2) * returnCellSizez;   //Used for checking size of room from center along z
        int xCheck = ((roomSizex) / 2) * returnCellSizex;   //Used for checking size of room from center along x
        List<GameObject> cellsInRoom = new List<GameObject>();      //Used to store cells of the room
        roomCenter = positionCorrection(roomCenter);                //Make sure position could be on grid

        //If the center is not in a valid position on the grid...
        //  (ex. Is not on the grid, exceeds boundaries of grid or quadrant)
        //...it will be shifted to the nearest valid position

        //Ensure the room is completely on the grid (as opposed to just the room center)
        roomCenter = confineRoom(roomCenter, xCheck, zCheck, gridBoundaries);

        int Quadrant = getQuadrant(roomCenter);         //Get the quadrant of the roomcenter

        //Ensure the room is completely within the boundary of a quadrant
        roomCenter = confineRoom(roomCenter, xCheck, zCheck, QuadrantBoundaries[Quadrant]);

        returnCenter = roomCenter;  //store the new room center

        findCell(returnCenter).name = "Center";   //Name center cell
        cellsInRoom.Add(findCell(roomCenter));      //Add roomcenter to list of cells in room

        //calculates starting position for generating the room (bottom left corner of room)
        Vector3 startingPosition = roomCenter + new Vector3(-roomSizex / 2 * returnCellSizex, 0.0f, -roomSizez / 2 * returnCellSizez);

        for (int i = 0; i < roomSizez; i++)     //Rows
        {
            Vector3 curPosition = startingPosition;             //set current position to starting position
            curPosition = moveCellPosition(0, curPosition, false, i);  //moves between cells of the room on z axis

            for (int v = 0; v < roomSizex; v++) //Colums
            {
                //get name of cell's parent at the current position
                GameObject nameTest = findCell(curPosition).transform.parent.gameObject;  

                if (nameTest.name.Contains("Room"))
                {
                    if (MergedRooms.Count > 0)
                        for (int x = 0; x < MergedRooms.Count; x++)
                        {
                            if (MergedRooms[x].name != nameTest.name)
                            {
                                if (x == MergedRooms.Count - 1)
                                    MergedRooms.Add(nameTest);
                            }
                            else
                                break;
                        }

                    else
                        MergedRooms.Add(nameTest);
                    
                }

                cellsInRoom.Add(findCell(curPosition));                     //Add cell to List of gameobjects of room
                curPosition = moveCellPosition(1, curPosition, false);     //move between cells of room on x axis
            }
        }

        boundaries roomBoundaries = calculateBoundary(cellsInRoom);   //gets the boundaries of the rooms

        foreach (GameObject cell in cellsInRoom)
        {
            destroyWalls(cell, roomBoundaries);                     //Destroys walls of cells that aren't on the edges of the room
        }

        return cellsInRoom;
    }

    private Vector3 positionCorrection(Vector3 roomcenter)
    {
        //Function ensures that the position given could be on the grid provided it was big enough to accomodate it
        //ex. The point (0,0,2) is a valid position on grid of 31x31 with a 0.4 cell size, 
        //however no cell could exist there because a cellsize of 0.4 would mean a distance of 4 between each cell. 
        //Since the grid starts at (0,0,0), the next possible position in this case would be (0,0,4)
        //So, we increase the room center's Vector3 until its x and z are divisble by the cell sizes

        int iteration = 0;
        while (roomcenter.x % returnCellSizex != 0) //roomcenter x is not divisible by cellsize x
        {
            roomcenter.x = Mathf.Ceil(roomcenter.x);    //Set roomcenterx to the nearest whole number
            roomcenter.x = roomcenter.x + 1.0f;         //Add 1

            if (iteration > 10)                         //Prevent infinite loop
            {
                Debug.LogError(roomcenter.x);
                throw new Exception("Infinite loop detected in positionCorrection on roomcenter.x");
            }
            iteration++;                                //iterate
        }

        iteration = 0;

        while (roomcenter.z % returnCellSizez != 0) //roomcenter z is not divisible by cellsize z
        {
            roomcenter.z = Mathf.Ceil(roomcenter.z);    //Set roomcenterz to nearest whole number
            roomcenter.z = roomcenter.z + 1.0f;         //Add 1

            if (iteration > 10)                         //Prevent infinite loop
            {
                Debug.LogError(roomcenter.z);
                throw new Exception("Infinite loop detected in positionCorrection on roomcenter.z");
            }
            iteration++;                                //iterate
        }

        return roomcenter;                              //return
    }

    private void destroyWalls(GameObject cell, boundaries roomBoundaries)
    {
        //Function destroys the walls of a cell if they exist
        //Parameters:
        //GameObject of the cell
        //boundaries of the room to prevent from destroying edges

        var children = new List<GameObject>();      //Stores the wall game objects

        foreach (Transform child in cell.transform)
        {
            children.Add(child.gameObject);         //Add wall to List of gameobjects

        }

        foreach (GameObject child in children)
        {
            //If cell position is not on the edge of the room, destroy the walls and set the parent to null
            if (cell.transform.position.z != gridBoundaries.south && cell.transform.position.z != roomBoundaries.south && child.name.Contains("south") ||
                cell.transform.position.z != gridBoundaries.north && cell.transform.position.z != roomBoundaries.north && child.name.Contains("north") ||
                cell.transform.position.x != gridBoundaries.east && cell.transform.position.x != roomBoundaries.east && child.name.Contains("east") ||
                cell.transform.position.x != gridBoundaries.west && cell.transform.position.x != roomBoundaries.west && child.name.Contains("west"))
            {
                GameObject.Destroy(child.gameObject);
                child.transform.parent = null;
            }
        }
;
    }

    private Vector3 moveCellPosition(int direction, Vector3 position, bool restrict, int movement = 1)
    {
        //Function moves correctly between cells
        //Parameters:
        //Direction: direction of movement wanted
        //position: Current position
        //Restrict: whether or not to restrict movements to quadrants
        //Number of times you want to move


        int timesMoved = 0;
        int quadrant = getQuadrant(position);
        float north = QuadrantBoundaries[quadrant].north;
        float south = QuadrantBoundaries[quadrant].south;
        float east = QuadrantBoundaries[quadrant].east;
        float west = QuadrantBoundaries[quadrant].west;

        while (timesMoved < movement)
        {
            switch (direction)
            {
                case 0: //north
                    position.z += returnCellSizez;
                    if (restrict)
                        if (position.z > north)
                        {
                            position.z = south;
                        }
                    break;

                case 1: //east
                    position.x += returnCellSizex;
                    if (restrict)
                        if (position.x > east)
                        {
                            position.x = west;
                        }
                    break;

                case 2: //south
                    position.z -= returnCellSizez;
                    if (restrict)
                        if (position.z < south)
                        {
                            position.z = north;
                        }
                    break;

                case 3: //west
                    position.x -= returnCellSizex;
                    if (restrict)
                        if (position.x < west)
                        {
                            position.x = east;
                        }
                    break;
            }

            timesMoved += 1;
        }
        //Debug.Log("Final Position = " + position);

        return position;
    }

    private GameObject findCell(Vector3 cellPosition)
    {
        //Function returns the cell gameobject of the position given
        cellPosition = confineCell(cellPosition);
        for (int i = 0; i < cells.Count; i++)   //iterate through cells
        {
            if (cells[i].transform.position == cellPosition)    //if a position matches the one given
            {
                return cells[i];                          //return it
            }
        }

        throw new Exception("Could not find cell at position: " + cellPosition);
    }

    private Vector3 confineRoom(Vector3 roomPosition, int xCheck, int zCheck, boundaries boundary)
    {
        //Function confines a point of a room within the parameters given

        while (roomPosition.x + xCheck > boundary.east)
                roomPosition.x -= returnCellSizex;

        while (roomPosition.x - xCheck < boundary.west)
            roomPosition.x += returnCellSizex;

        while (roomPosition.z + zCheck > boundary.north)
            roomPosition.z -= returnCellSizez;

        while (roomPosition.z - zCheck < boundary.south)
            roomPosition.z += returnCellSizez;

        return roomPosition;
    }

    #endregion

    #region Room Transformations

    //All transformations are handled here

    private List<Vector3> ShiftRooms(List<Vector3> positions, List<int> roomNumbers, int direction, bool quadrant)
    {
        boundaries roomBoundary;
        for (int i = 0; i < positions.Count; i++)
        {
            if (quadrant)
                roomBoundary = QuadrantBoundaries[getQuadrant(positions[i])];
            else
                roomBoundary = getBoundaries;

            positions[i] = moveCellPosition(direction, positions[i], true, (int)roomNumbers[i]);
        }

        return positions;
    }

    private int[] ScaleRooms(int[] roomSizes, int[] roomNum, int totalRooms, int factor = 1)
    {
        for (int i = 0; i < roomSizes.Length; i++)
        {
            roomSizes[i] += (factor * roomNum[i]);
            roomSizes[i] = CycleList(totalRooms, roomSizes[i]);
        }

        return roomSizes;
    }

    private int CycleList(int roomSizeLength, int position)
    {
        //Function ensures that room sizes cycle through the list of sizes. Prevents OutofBounds error
        while (position >= roomSizeLength)
        {
            position -= roomSizeLength;
        }

        while (position < 0)
        {
            position += roomSizeLength;
        }

        return position;
    }

    private int[] ShuffleRooms(int[] RoomNumbers, bool quadrantOnly)
    {

        int QuadrantsWanted = xQuadrants * zQuadrants;

        if (quadrantOnly)
        {
            int iteration = 0;
            for (int i = 0; i < QuadrantsWanted; i++)
            {
                int[] roomNums = new int[roomsPerQuadrant];
                for (int x = 0; x < roomsPerQuadrant; x++)
                {
                    roomNums[x] = RoomNumbers[iteration];
                    iteration++;
                }

                roomNums = UniversalHelper.Shuffle(roomNums);

                iteration = 0;
                for (int x = 0; x < roomsPerQuadrant; x++)
                {
                    RoomNumbers[iteration] = roomNums[x];
                    iteration++;
                }
            }
        }

        else
            UniversalHelper.Shuffle(RoomNumbers);

        return RoomNumbers;
    }

    private void ElevateRooms(int elevationNum, List<GameObject> Rooms)
    {
        for (int i = 0; i < elevationNum; i++)
        {
            for (int x = i; x < Rooms.Count; x += 2)
            {
                Rooms[x].transform.position += new Vector3(0.0f, cellWallHeight, 0.0f);
            }
        }
    }

    private void RotateGrid(int rotationNum)
    {
        float ZgridMid = (gridBoundaries.north + gridBoundaries.south) / 2;
        float XgridMid = (gridBoundaries.east + gridBoundaries.west) / 2;

        Vector3 mid = new Vector3(XgridMid, 0.0f, ZgridMid);

        for (int i = 0; i < rotationNum; i++)
        {
            GameObject.Find("Rooms").transform.RotateAround(mid, transform.forward, 90);
            GameObject.Find("Halls").transform.RotateAround(mid, transform.forward, 90);
        }
    }
    #endregion

    #region Coroutines

    IEnumerator RealTimeGenerator(List<Vector3> roomPositions, List<RoomSizes> sizes, int[] roomSizes, int rotationNum)
    {
        List<List<GameObject>> rooms = new List<List<GameObject>>();

        for (int i = 0; i < roomPositions.Count; i++)
        {
            
            int[] roomsize = new int[] { sizes[roomSizes[i]].sizeX, sizes[roomSizes[i]].sizeZ };
            rooms.Add(CreateRoom(roomPositions[i], roomsize));
            /*
            Vector3 returnPos = roomPositions[i];
            List<GameObject> MergedRooms;
            List<GameObject> parent = generateRoom(roomPositions[i], roomsize[0], roomsize[1], out returnPos, out MergedRooms);
            if (MergedRooms.Count != 0)
            {
                List<GameObject> newRoom = parent;
                for (int x = 0; x < MergedRooms.Count; x++)
                {
                    foreach (Transform cell in MergedRooms[x].transform)
                    {
                        newRoom.Add(cell.gameObject);
                    }
                    GameObject.Destroy(MergedRooms[x]);
                }
            }
            */
            //UniversalHelper.parentObject(parent, "Rooms", "Rooms " + i);

            yield return new WaitForSeconds(0.3f * Time.deltaTime);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            UniversalHelper.parentObject(rooms[i], "Rooms", "Room " + i);
        }

        //Rooms.transform.GetChild(largestRoom).name = "Boss Room";
        numberofRooms = rooms.Count;

        GenerateHalls(determineHallGen());
        Destroy(GameObject.Find("Cells"));
        yield return new WaitForSeconds(5.0f * Time.deltaTime);
        RotateGrid(rotationNum);
        structureDone = true;                                                   //Set structure done to true
        StopAllCoroutines();
    }

    public static IEnumerator WaitSecondsVoid(float seconds, Action FunctionName)
    {
        //Function waits for a predetermined number of seconds and then calls another function
        yield return new WaitForSeconds(seconds);
        FunctionName();
    }
    #endregion

    #region Special Functions

    private Vector3 confineCell(Vector3 position)
    {
        if (position.x % returnCellSizex != 0)
        {
            float eastCheck = Mathf.Abs(position.x - gridBoundaries.east);
            float westCheck = Mathf.Abs(position.x - gridBoundaries.west);
            if (eastCheck >= westCheck)
            {
                while (position.x % returnCellSizex != 0)
                    position.x++;
            }

            else
            {
                while (position.x % returnCellSizex != 0)
                    position.x--;
            }
        }

        if (position.z % returnCellSizez != 0)
        {
            float northCheck = Mathf.Abs(position.z - gridBoundaries.north);
            float southCheck = Mathf.Abs(position.z - gridBoundaries.south);
            if (northCheck >= southCheck)
            {
                while (position.z % returnCellSizez != 0)
                    position.z++;
            }

            else
            {
                while (position.z % returnCellSizez != 0)
                    position.z--;
            }
        }

        return position;
    }

    /// <summary>
    /// Return the furthest positions of the grid (read-only)
    /// </summary>
    public boundaries getBoundaries
    {
        get
        {
            return gridBoundaries;
        }
    }

    /// <summary>
    /// Returns distance necessary to move from one cell to another along x-axis
    /// </summary>
    public int returnCellSizex
    {
        get { return Convert.ToInt32(cellSize.x * 10); }
    }

    /// <summary>
    /// Returns distance necessary to move from one cell to another along z-axis
    /// </summary>
    public int returnCellSizez
    {
        get { return Convert.ToInt32(cellSize.x * 10); }
    }

    /// <summary>
    /// Return the current seed
    /// </summary>
    public int[] getCurSeed
    {
        get { return seed; }
    }

    /// <summary>
    /// Return the value stored in seed at index
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    public void setSeedAt(int index, int value)
    {
        seed[index] = value;
    }

    #endregion

    #region Get Functions

    //Returns a list of Room Sizes containing integers for room sizes based on specified values
    private List<RoomSizes> getRoomSizes()
    {

        List<RoomSizes> roomSizes = new List<RoomSizes>();      //Create a new list of the room sizes class
        for (int i = minRoomSize; i <= maxRoomSize; i++)        
        {
            for (int x = minRoomSize; x <= maxRoomSize; x++)
            {
                RoomSizes rs = new RoomSizes();
                rs.sizeX = i;
                rs.sizeZ = x;
                roomSizes.Add(rs);
            }
        }

        return roomSizes;
    }

    //This function is used to get the quadrant of a position
    private int getQuadrant(Vector3 position)
    {
        GameObject Cells = GameObject.Find("Cells");
        for (int i = 0; i < Cells.transform.childCount; i++)
        {
            if (findCell(position).transform.parent == Cells.transform.GetChild(i))
                return i;
        }

        throw new Exception("Could not find quadrant for cell at position " + position);
            
    }

    //This function takes two cells and determines their location in relation to each other
    private void getCellLocation(Vector3 cellOne, Vector3 cellTwo, out int relation)
    {
        relation = -1;

        int x = Math.Abs((int)cellTwo.x - (int)cellOne.x);
        int z = Math.Abs((int)cellTwo.z - (int)cellOne.z);

        if (z > x)
        {
            if (cellOne.x > cellTwo.x)
                relation = 3;

            else if (cellOne.x < cellTwo.x)
                relation = 2;
        }

        else if (z < x)
        {
            if (cellOne.z > cellTwo.z)
                relation = 1;

            else if (cellOne.z < cellTwo.z)
                relation = 0;
        }

        else
        {
            int decide = UniversalHelper.randomGenerator(0, 10);

            if (decide < 5)
            {
                if (cellOne.x > cellTwo.x)
                    relation = 3;

                else if (cellOne.x < cellTwo.x)
                    relation = 2;
            }

            else
            {
                if (cellOne.z > cellTwo.z)
                    relation = 1;

                else if (cellOne.z < cellTwo.z)
                    relation = 0;
            }
        }
    }

    //Takes the string in the inspector, converts it to an int[], and sets seed equal to it.
    public void seedStringToSeed()
    {

        int x = 1;
        for (int i = floor.ToString().Length; i < seedString.Length; i++)
        {
            int test;
            try
            {
                if (int.TryParse(seedString[i].ToString(), out test))
                {
                    try
                    {
                        seed[x] = test;
                        x++;
                    }

                    catch
                    {
                        Debug.LogException(new Exception("Invalid seed"));
                    }
                }
            }

            catch
            {
                Debug.LogException(new Exception("String cannot be parsed"));
            }
        }

    }

    //Return the default seed. (Default seed is 10000...where num of zeroes is determined by seed length - 1.
    public string getDefaultSeed()
    {
        string defaultSeed = floor.ToString();
        for (int i = 1; i < seed.Length; i++)
        {
            defaultSeed += "0";
        }

        return defaultSeed;
    }

    //Gets the default positions for rooms in a quadrant
    private List<Vector3> getDefaultRooms(boundaries quadrant)
    {
        //Function is used to find the: Top Left, Top Middle, Top Right, Middle Left, Center, Middle Right
        //                              Bottom Left, Bottom Middle, and Bottom Right

        //                              cells of a quadrant

        List<Vector3> roomPositions = new List<Vector3>();  //Stores room positions for quadrant

        float north = quadrant.north;               //Store north quadrant boundary
        float south = quadrant.south;               //Store south quadrant boundary
        float east = quadrant.east;                 //Store east  quadrant boundary
        float west = quadrant.west;                 //Store west  quadrant boundary

        float totalHeight = north - south;          //total height of quadrant
        float totalWidth = Mathf.Abs(east - west);  //total width of quadrant

        float xmid = ((totalWidth / 2) + west);     //mid width of quadrant
        float zmid = ((totalHeight / 2) + south);   //mid height of quadrant
        xmid = Mathf.Round(xmid);
        zmid = Mathf.Round(zmid);

        roomPositions.Add(new Vector3(west, 0.0f, north));  //Top Left      0
        roomPositions.Add(new Vector3(xmid, 0.0f, north));  //Top Middle    1
        roomPositions.Add(new Vector3(east, 0.0f, north));  //Top Right     2

        roomPositions.Add(new Vector3(west, 0.0f, zmid));   //Middle Left   3
        roomPositions.Add(new Vector3(xmid, 0.0f, zmid));   //Center        4
        roomPositions.Add(new Vector3(east, 0.0f, zmid));   //Middle Right  5

        roomPositions.Add(new Vector3(west, 0.0f, south));  //Bottom Left   6
        roomPositions.Add(new Vector3(xmid, 0.0f, south));  //Bottom Middle 7
        roomPositions.Add(new Vector3(east, 0.0f, south));  //Bottom Right  8


        return roomPositions;
    }

    public int getCellCount()
    {
        return xCells * zCells * xQuadrants * zQuadrants;
    }
    #endregion

    public static void seedTransformation()
    {
        
        
    }
}

#region Other Classes

/// <summary>
/// Contains integer values for X by Z room sizes
/// </summary>
public class RoomSizes
{
    public int sizeX;
    public int sizeZ;
}

/// <summary>
/// Contains static functions that are used throughout the generation process
/// </summary>
public static class UniversalHelper
{
    /// <summary>
    /// Creates an empty gameobject parent for objects, or add objects to parent if they already exist
    /// </summary>
    /// <param name="child"></param>
    /// <param name="parentName"></param>
    /// <param name="subParentName"></param>
    public static void parentObject(List<GameObject> child, string parentName, string subParentName = "")
    {
        if (child.Count > 1)
        {
            List<GameObject> childObjects = child;
            //Used to create subparents
            if (!string.IsNullOrEmpty(subParentName))
            {
                List<GameObject> parentChild = new List<GameObject>();
                if (GameObject.Find(subParentName))
                {
                    addChild(childObjects, GameObject.Find(subParentName));
                    parentChild.Add(GameObject.Find(subParentName));
                }

                else
                {
                    GameObject subparent = new GameObject();
                    subparent.name = subParentName;
                    subparent.transform.position = childObjects[0].transform.position;
                    addChild(childObjects, subparent);
                    parentChild.Add(subparent);
                }

                childObjects = parentChild;
            }

            if (GameObject.Find(parentName))
            {
                addChild(childObjects, GameObject.Find(parentName));
            }

            else
            {
                GameObject parent = new GameObject();
                parent.name = parentName;
                parent.transform.position = childObjects[0].transform.position;
                addChild(childObjects, parent);
            }
        }

        else
        {
            throw new Exception("Could not parent child. Child object cannot be empty");
        }
    }

    //Used by parentObject
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
    public static int[] Shuffle(int[] numbers)
    {
        //Function takes a set of numbers and Shuffles them using a predictable algorithm
        //Has to have at least an array length of 4
        //Numbers are split into 4 groups
        //Group 1 is exchanged with Group 3
        //Group 2 is exchanged with Group 4
        //Arrays are then reversed
        //If there is more than a single number between groups, the middle numbers are reversed as well

        float divisor = numbers.Length / 4;             //Split number into 4 groups
        List<List<int>> nums = new List<List<int>>();   //Tracks groups of numbers

        for (int i = 0; i < numbers.Length; i++)
        {
            if (i % divisor == 0)
            {
                nums.Add(new List<int>());              //Adds appropriate number of groups
            }

            nums[nums.Count - 1].Add(numbers[i]);       //Adds numbers to group
        }

        List<int> temp;         //Stores numbers temporarily
        temp = nums[2];         //Set temp to Group 3             
        nums[2] = nums[0];      //Set Group 3 to Group 1
        nums[2].Reverse();      //Reverse Group 3

        nums[0] = temp;         //Set Group 1 = temp/Group 3
        nums[0].Reverse();      //Reverse Group 1

        if (divisor > 1)    //If there is more than 1 number in the groups
        {
            nums[2].Reverse((nums[2].Count / 2) - 1, (nums[2].Count / 2) + 1);  //Swap the middle numbers for group 3
            nums[0].Reverse((nums[0].Count / 2) - 1, (nums[0].Count / 2) + 1);  //Swap the middle numbers for group 1
        }

        temp = nums[3];         //Set temp to Group 4
        nums[3] = nums[1];      //Set Group 4 = Group 2
        nums[3].Reverse();      //Reverse Group 4

        nums[1] = temp;         //Set Group 2 = temp/Group 4
        nums[1].Reverse();      //Reverse Group 2

        if (divisor > 1)    //Same as above
        {
            nums[3].Reverse((nums[2].Count / 2) - 1, (nums[2].Count / 2) + 1);
            nums[1].Reverse((nums[0].Count / 2) - 1, (nums[0].Count / 2) + 1);
        }

        int increment = 0;                          //Track var numbers position

        for (int i = 0; i < nums.Count; i++)        //Iterate through the list of nums
        {
            for (int x = 0; x < nums[i].Count; x++)     //Access the shuffled numbers
            {
                numbers[increment] = nums[i][x];            //Set old var numbers = to newly shuffled numbers
                increment++;                                //Increment to the next number
            }
        }

        return numbers;     //return shuffle
    }

    /// <summary>
    /// Generates random int between min and max
    /// </summary>
    /// <param name="minValue"></param>
    /// <param name="maxValue"></param>
    /// <returns></returns>
    public static int randomGenerator(int minValue, int maxValue)
    {
        System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());

        int result = rand.Next(minValue, (maxValue + 1));

        return result;
    }

    /// <summary>
    /// Determines the directions needed to get from object one to object one. 
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


}
#endregion

