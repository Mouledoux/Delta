using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class StructuralGeneration : MonoBehaviour
{
    /*
        This script is used to generate the structure of the dungeon.
        This consists of the floor, walls, rooms, hallways, and ceiling.

        How dungeon generation works:

        First a little lingo:
            Cell = a single rectangular FLOOR gameobject prefab (primitive plane with a texture for the floor)
            Grid = A large rectangular plane made up of all cells. It is divided into even quadrants.
            Quadrant = A section of the grid made up of a group of cells.

        Alright, now onto the steps.

        Step 1: Generates a grid
        Step 2: Divide grid into even quadrants (specified as a variable)
        Step 3: Generate four walls around each cell (primitive cube with a texture for walls)
        Step 4: Define a default dungeon, thereby generating rooms
        Step 4: Use a seed to shift the positions of the default dungeon
        Step 5: Define the main rooms
        Step 6: Connect the main rooms using a main hall
        Step 7: Ensure every other room is connected in some way to the main hall, either directly or indirectly


        How the seed works
    
            Step 1: Generate the quadrants of the grid
            Step 2: Find default room positions in each quadrant of the grid.
                - The default room positions are: 

                //////////////////////////////////////////////////////  
                //// Top Left        Top Middle      Top Right,   ////      
                //// Middle Left     Center          Middle Right ////     
                //// Bottom Left     Bottom Middle   Bottom Right ////
                //////////////////////////////////////////////////////
                                                                        
                - Room's default positions are placed in these positions

            Step 3: Execute transformations and exchanges based on numbers 0-9 to shift, scale, and switch rooms

            The seed is currently an int array with length 12.
            The first nine numbers are used to transform the dungeon
            The tenth and eleventh numbers determine which rooms (every room is numbered from 0 - number of rooms)
                will be considered the main rooms
            The twelfth number determines the boss that spawns
            This array may be expanded to help with object generation if wanted (more details when I get to that)

            As stated previously, numbers 0-9 are used for transformations and whatnot.
            They are currently used in the following manner:
            0 - shifts every room up in their quadrant by room number
            1 - shifts every number right in their quadrant room number (if it reaches side, it goes to other side)
            2 - shifts every room up on the entire grid by room number (same rules as 0)
            3 - shifts every room right on the entire grid by room number (same rules as 1)
            4 - switches room numbers and sizes within quadrant
            5 - switches room numbers and sizes among the grid
            6 - Not currently in use
            7 - Not currently in use
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
     *  - Deform cells so they don't all form perfect rectangles
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
     * Generate hallways
     * Generate Seed
     * Generate Dungeon during runtime
     * 
     * 
     * 
     * 
     * 
     * 
     */
    //After Global Variables, scroll to Start to begin tracking code.

    #region Global Variables

    public bool Generate;           //Determines whether or not to generate past the grid

    public int x_cells, z_cells;    //Number of cells generated along x and z axis
    public int roomsPerQuadrant;    //Number of rooms allowed per quadrant
    public int QuadrantsWanted;     //Number of quadrants to generate

    public Vector3 cellSize;        //Determines the size of individual cells (testing only)
    
    public float cellWallHeight;    //determines height of cell walls
    public float cellWallWidth;     //determines width of cell walls

    public GameObject FLOOR;        //Floor gameobject
    public GameObject WALL;         //Wall gameobject

    private float[] boundaries = new float[] { 0, 0, 0, 0 };            //Tracks the furthest points of the grid
    private List<float[]> QuadrantBoundaries;                           //Tracks furthest points of each quadrant

    private static List<GameObject> cells;                              //Tracks all of the physical cells used in the game

    public int[] seed = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };        //stores seed
    public string seeddisplay;                                          //controls seed

    public bool seedGen;                                                //Generate a seed
    private bool running;                                               //Checks if game has started


    #endregion

    #region Generate Grid

    private List<GameObject> generateGrid() 
    {
        //Function generates the grid


        List<GameObject> cells = new List<GameObject>(); //used for keeping track of individual cells

        int x_halfcells = x_cells / 2; //determines the distance from the center of the grid to the side edge.
        int z_halfcells = z_cells / 2; //determines the distance from the center of the grid to the top/bottom edge

        Vector3 startingGridPosition = Vector3.zero;    //The grid's bottom edge cells will center on the origin
        Vector3 cellSpace = cellSize * 10;              //calculate how much world space a single cell takes up

        int cellName = 0;   //Initialize cell names

        for (int i = 0; i < z_cells; i++)       //Row
        {
            Vector3 currentGridPosition = startingGridPosition;     //keeps track of the current world position
            currentGridPosition.x += (x_halfcells * cellSpace.x);   //starting from the bottom right corner of the grid

            if (i != 0) //If it isn't the first row
            {
                currentGridPosition.z += cellSpace.z * i; //move the grid position up along z-axis
            }

            for (int x = 0; x < x_cells; x++)   //Column
            {
                GameObject cell = Instantiate(FLOOR);           //Create new cell
                cell.name = "cell " + cellName;                 //name the cell for easier reference
                cellName++;                                     //Increment cellName  
                cell.transform.position = currentGridPosition;  //bring cell to current position
                cell.transform.localScale = cellSize;           //set cell to the size determined
                cells.Add(cell);                                //add cell to cells List
                currentGridPosition.x -= cellSpace.x;           //update currentGridPosition to next empty spot
            }

        }

        return cells;   //Return list of cells
    }

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
        GameObject[] wall = new GameObject[] {Instantiate(WALL), Instantiate(WALL), Instantiate(WALL), Instantiate(WALL)};

        wall[0].name = "north_wall";    //identifies north wall
        wall[1].name = "east_wall";     //identifies east wall
        wall[2].name = "south_wall";    //identifies south wall
        wall[3].name = "west_wall";     //identifies west wall

        wall[0].transform.localScale = nsCellWallSize; //adjust scale for north wall
        wall[1].transform.localScale = ewCellWallSize; //adjust scale for east wall
        wall[2].transform.localScale = nsCellWallSize; //adjust scale for south wall
        wall[3].transform.localScale = ewCellWallSize; //adjust scale for west wall

        for (int x = 0; x < wall.Length; x++ )
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

    private float[] furthestDirections(List<GameObject> a_cells)
    {
        //Function returns the furthest world positions in all directions
        //0 = north, 1 = south, 2 = east, 3 = west


        float[] furthestPoints = new float[] { -500.0f, 500.0f, -500.0f, 500.0f };  //Initialize to something ridiculous

        for (int i = 0; i < a_cells.Count; i++)
        {
            if (a_cells[i].transform.position.x > furthestPoints[2])    //If the cell is furthest east
            {
                furthestPoints[2] = a_cells[i].transform.position.x;        //set furthest east to cell position
            }

            if (a_cells[i].transform.position.x < furthestPoints[3])    //If the cell is furthest west
            {
                furthestPoints[3] = a_cells[i].transform.position.x;        //set furthest west to cell position
            }

            if (a_cells[i].transform.position.z > furthestPoints[0])    //If the cell is furthest north
            {
                furthestPoints[0] = a_cells[i].transform.position.z;        //set furthest north to cell position
            }

            if (a_cells[i].transform.position.z < furthestPoints[1])    //If the cell is furthest south
            {
                furthestPoints[1] = a_cells[i].transform.position.z;        //set furthest south to cell position
            }
        }

            return furthestPoints;  //return furthest points
    }
    #endregion

    #region Generate Quadrants

    private void generateQuadrants()
    {
        //Function divides grid into quadrants
        //Can only generate using squares and even numbers.

        float sqrtQuadWanted = Mathf.Sqrt(QuadrantsWanted);             //Take the square root of quadrants wanted
        int x_quads = 0;                                                //determine quads along x (Columns)
        int z_quads = 0;                                                //determine quads along z (Rows)
        float totalWidth = (Mathf.Abs(boundaries[3]) + boundaries[2]);  //get total width of the grid

        if (sqrtQuadWanted % 1 == 0)        //If quads wanted is square
        {
            x_quads = z_quads = (int)sqrtQuadWanted;    //x and z quads = square root
        }

        else if (QuadrantsWanted % 2 == 0)  //If quads wanted is even
        {
            x_quads = QuadrantsWanted / 2;  //x_quads takes the Least Common Multiple
            int iter = 0;                   //Track iterations through while loop

            while (x_quads % 2 == 0)        //while not LCM
            {
                x_quads /= 2;                   //Divide by 2
                iter++;                         //increment
                if (iter > 10)                  //Prevents infinite loop and crash
                    throw new Exception("Quadrant Generation took too long");
            }
            z_quads = QuadrantsWanted / x_quads;        //Get number of z_Quads
        }

        else    //If quads is odd
        {
            QuadrantsWanted = 9;    //Set quadrants wanted to 9
            x_quads = z_quads = 3;  //Default x_quads and z_quads
            Debug.LogError("Quadrants Wanted must be even or a square. Defaulted to 9 Quadrants.");
        }

        DrawQuadrant(totalWidth, boundaries[0], x_quads, z_quads);  //Create quadrants
    }

    private List<List<GameObject>> DrawQuadrant(float gridWidth, float gridHeight, int x_quads, int z_quads)
    {

        List<List<GameObject>> Quadrants = new List<List<GameObject>>();    //List for Quadrant Gameobjects
        List<float[]> Zones = new List<float[]>();                          //List for quadrant boundaries

        int H_movement = Convert.ToInt32(gridWidth / x_quads);  //determines horizontal movement between quadrants
        int V_movement = Convert.ToInt32(gridHeight / z_quads); //determines vertical movement between quadrants
        for (int i = 0; i < QuadrantsWanted; i++)
        {
            Quadrants.Add(new List<GameObject>());  //Add appropriate number of quadrant game objects
            Zones.Add(new float[] {0,0,0,0 });      //Add appropriate number of zones
        }

        int zone = 0;                       //Tracks which quadrant we are building

        //Used to ensure that all cells are attached to a quadrant
        float[] lastBoundaries = new float[] { boundaries[1], boundaries[1], boundaries[3], boundaries[3] };

        for (int i = 0; i < z_quads; i++)
        {
            lastBoundaries[2] = boundaries[3];                      //Set last east boundary to western grid boundary
            float northbase = lastBoundaries[0] + V_movement;       //quadrant north perimeter calculation
            float southbase = lastBoundaries[0];                    //quadrant south perimeter calculation

            for (int x = 0; x < x_quads; x++)
            {
                Zones[zone][0] = northbase;                         //set north perimeter to calculation
                Zones[zone][1] = southbase;                         //set south perimeter to calculation  
                Zones[zone][2] = lastBoundaries[2] + H_movement;    //set east perimeter to calculation
                Zones[zone][3] = lastBoundaries[2];                 //set west perimeter to calculation
                
                lastBoundaries[0] = Zones[zone][0];                 //lastboundary north = zone north
                lastBoundaries[1] = Zones[zone][1];                 //lasboundary south = zone south          
                lastBoundaries[2] = Zones[zone][2];                 //lastboundary east = zone east
                lastBoundaries[3] = Zones[zone][3];                 //lastboundary west =  zone west
                zone += 1;                                          //increment to next quadrant
            }
        }

        QuadrantBoundaries = Zones;                                 //Set the global variable: QuadrantBoundaries
        
        //iterate through all cells and determine their quadrant based on their position
        for (int i = 0; i < cells.Count; i++)
        {
            Vector3 position = cells[i].transform.position; //use local variable for ease

            //iterate through quadrant boundaries to determine if cell belongs in the quadrant
            for (int x = 0; x < Zones.Count; x++)
            {
                if (x + 1 == Zones.Count)   //If we're on the last zone (top right)
                {
                    //If a cells position is greater than the top right's east, add it to the quadrant with the same height
                    //This ensures that cells don't go causing shitstorms for the rest of the script
                    if (position.x >= Zones[Zones.Count - 1][2] && position.z <= Zones[x][0] && position.z >= Zones[x][1])
                    {
                        Quadrants[x].Add(cells[i]);
                    }
                }

                if (position.z > Zones[x][0])               //greater than north perimeter
                {
                    continue;
                }

                else if (position.z < Zones[x][1])          //less than south perimeter
                    continue;

                else if (position.x > Zones[x][2])          //greater than east perimeter
                    continue;

                else if (position.x < Zones[x][3])          //less than west perimeter
                    continue;

                else
                {
                    Quadrants[x].Add(cells[i]);             //add cell to quadrant
                    break;
                }
            }
        }

        for (int i = 0; i < Quadrants.Count; i++)
        {
            parentObject(Quadrants[i], "Cells", "Quadrant " + i);   //Parent quadrants under cells game object
        }

        return Quadrants;
    }
    #endregion

    #region Generate Rooms

    private List<GameObject> generateRoom(Vector3 roomCenter, int roomSizex, int roomSizez, out Vector3 returnCenter)
    {
        //Function marks cells for rooms.
        //Parameters: 
        //a position in world space for the center
        //size of the room (cells along x and z)
        //quadrant of room,
        //Vector3 variable to save the position of the center if it changes

        bool mergeRoom = false;                                     //Determines if rooms need to be merged
        Vector3 roomToMergePosition = Vector3.zero;                 //Gets room that needs to be merged
        int roomSizezCheck = ((roomSizez) / 2) * returnCellSizez;   //Used for checking size of room from center along z
        int roomSizexCheck = ((roomSizex) / 2) * returnCellSizex;   //Used for checking size of room from center along x
        List<GameObject> cellsInRoom = new List<GameObject>();      //Used to store cells of the room
        roomCenter = positionCorrection(roomCenter);                //Make sure position could be on grid

        //If the center is not in a valid position on the grid...
        //  (ex. Is not on the grid, exceeds boundaries of grid or quadrant)
        //...it will be shifted to the nearest valid position

        //check against grid east and west boundaries
        roomCenter.x = confineRoom(roomCenter.x, roomSizexCheck , boundaries[2], boundaries[3], "x");

        //check against grid north and south boundaries
        roomCenter.z = confineRoom(roomCenter.z, roomSizezCheck, boundaries[0], boundaries[1], "z");

        int Quadrant = getQuadrant(roomCenter);         //Get the quadrant of the roomcenter

        //check against quadrant east and west boundaries
        roomCenter.x = confineRoom(roomCenter.x, roomSizexCheck, QuadrantBoundaries[Quadrant][2], QuadrantBoundaries[Quadrant][3], "x");

        //check against quadrant north and south boundaries
        roomCenter.z = confineRoom(roomCenter.z, roomSizezCheck, QuadrantBoundaries[Quadrant][0], QuadrantBoundaries[Quadrant][1], "z");    


        returnCenter = roomCenter;  //store the new room center

        findCell(returnCenter).name += " Center";   //Name center cell
        cellsInRoom.Add(findCell(roomCenter));      //Add roomcenter to list of cells in room

        //calculates starting position for generating the room (bottom left corner of room)
        Vector3 startingPosition = roomCenter + new Vector3(-roomSizex / 2  * returnCellSizex, 0.0f, -roomSizez / 2 * returnCellSizez);

        for (int i = 0; i < roomSizez; i++)     //Rows
        {
            Vector3 curPosition = startingPosition;             //set current position to starting position
            curPosition = moveCellPosition(0, curPosition, false, i);  //moves between cells of the room on z axis

            for (int v = 0; v < roomSizex; v++) //Colums
            {
                //The following isn't currently used to the same extent as previous generation,
                //However, may still be necessary. I haven't tested without it
                //BEGIN
                //Don't affect Main Room and Boss Room

                string nameTest = findCell(curPosition).transform.parent.name;  //get name of cell's parent at the current position

                //If it is not a main room or the center for another room
                if (nameTest != "Main Room" && nameTest != "Boss Room" && !nameTest.Contains("Center")) 
                {
                    cellsInRoom.Add(findCell(curPosition)); //Add cell to List of gameobjects of room

                    if (findCell(curPosition).transform.parent.name.Contains("Room"))
                    {
                        mergeRoom = true;
                        roomToMergePosition = curPosition;
                    }
                }
                //END
                curPosition = moveCellPosition(1, curPosition, false);     //move between cells of room on x axis
            }  
        }

        float[] roomBoundaries = furthestDirections(cellsInRoom);   //gets the boundaries of the rooms

        foreach (GameObject cell in cellsInRoom)
        {
            destroyWalls(cell, roomBoundaries);                     //Destroys walls of cells that aren't on the edges of the room
        }

        if (mergeRoom)
        {
            parentObject(cellsInRoom, "Rooms", findCell(roomToMergePosition).transform.parent.name);
            return null;
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

    private void destroyWalls(GameObject cell, float[] roomBoundaries)
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

        foreach(GameObject child in children)
        {
            //If cell position is not on the edge of the room, destroy the walls and set the parent to null
            if (cell.transform.position.z != getBoundaries[1] && cell.transform.position.z != roomBoundaries[1] && child.name.Contains("south") ||
                cell.transform.position.z != getBoundaries[0] && cell.transform.position.z != roomBoundaries[0] && child.name.Contains("north") ||
                cell.transform.position.x != getBoundaries[2] && cell.transform.position.x != roomBoundaries[2] && child.name.Contains("east") ||
                cell.transform.position.x != getBoundaries[3] && cell.transform.position.x != roomBoundaries[3] && child.name.Contains("west"))
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
        float north = QuadrantBoundaries[quadrant][0];
        float south = QuadrantBoundaries[quadrant][1];
        float east = QuadrantBoundaries[quadrant][2];
        float west = QuadrantBoundaries[quadrant][3];

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

        for (int i = 0; i < cells.Count; i++)   //iterate through cells
        {
            if (cells[i].transform.position == cellPosition)    //if a position matches the one given
            {
                return cells[i];                          //return it
            }
        }

        return null;
    }

    private float confineRoom(float point, int roomSize, float check1, float check2, string var)
    {
        //Function confines a point of a room within the parameters given

        while (point + roomSize > check1)
        {
            if (var == "x")
                point -= returnCellSizex;
            else
                point -= returnCellSizez;
        }

        while (point - roomSize < check2)
        {
            if (var == "x")
                point += returnCellSizex;
            else
                point += returnCellSizez;
        }

        return point;
    }

    #endregion

    #region Generate Hallways

    private bool connectCells(GameObject cellIn, GameObject cellToConnect)
    {
        //Function takes two cell gameObjects and connects them.
        //If function returns false, then they are already connected or not possible to connect

        string cellWallOne, cellWallTwo;    //Get the walls that need to be connected in each cell

        if (!cellIn.transform.parent.name.Contains("Hall") || !cellIn.transform.parent.name.Contains("Room"))
        {
            detectConnectingWalls(cellIn.transform.position, cellToConnect.transform.position,
                out cellWallOne, out cellWallTwo);

            destroyConnectingWalls(cellIn, cellToConnect, cellWallOne, cellWallTwo);
        }

        else
        {
            return false;
        }

        return true;
    }

    //This method is used by connectCells to determine which walls are connecting the cells, if any
    private void detectConnectingWalls(Vector3 cellOne, Vector3 cellTwo, out string wallOne, out string wallTwo)
    {
        float cellCalcx = returnCellSizex;
        float cellCalcz = returnCellSizez;

        if (cellOne.z + cellCalcz == cellTwo.z) //north
        {
            wallOne = "north_wall";
            wallTwo = "south_wall";
        }

        else if (cellOne.z - cellCalcz == cellTwo.z) //south
        {
            wallOne = "south_wall";
            wallTwo = "north_wall";
        }

        else if (cellOne.x + cellCalcx == cellTwo.x) //east
        {
            wallOne = "east_wall";
            wallTwo = "west_wall";
        }

        else if (cellOne.x - cellCalcx == cellTwo.x) //west
        {
            wallOne = "west_wall";
            wallTwo = "east_wall";
        }

        else
            throw new Exception("These cells cannot be connected:" + findCell(cellOne).name + findCell(cellTwo).name);
    }

    //This method is used by connectCells to destroy connected walls
    private void destroyConnectingWalls(GameObject cellOne, GameObject cellTwo, string wallOne, string wallTwo)
    {
        if (cellOne.transform.Find(wallOne)) 
        {
            Destroy(cellOne.transform.Find(wallOne).gameObject);
        }

        if (cellTwo.transform.Find(wallTwo))
        {
            Destroy(cellTwo.transform.Find(wallTwo).gameObject);
        }
    }

    //This checks to see if a cell is currently parented (a room or a hall)
    private bool checkCellStatus(Vector3 a_cell)
    {
        if (findCell(a_cell).transform.parent.transform.parent.name == "Rooms")
        {
            return false;
        }
        return true;
    }

    #endregion

    #region Start and Update


    void Start()
    {
        //Generating Cells
        cells = generateGrid();                 //generate grid
        parentObject(cells, "Cells");           //Parent every cell generated under a game object named "Cells"

        boundaries = furthestDirections(cells); //Get the furthest world positions of the grid in each direction
        generateQuadrants();                    //Divide the grid into quadrants

        //Generating Rooms and Halls
       // if (Generate) //If we want to generate walls, rooms, and hallways
       // {
            generateCellWalls(cells); //Generate walls around each cell

       //     GenerateDungeon();                  //Generate Dungeon
       //     Destroy(GameObject.Find("Cells")); //Destroy whatever cells aren't used

       //     GameObject Rooms = GameObject.Find("Rooms");

       //     for (int i = 0; i < Rooms.transform.childCount; i++)
        //    {
        //        Rooms.transform.GetChild(i).name = "Room " + i;
        //    }
      //  }
       // Generate = false;
        running = true;
    }

    void Update()
    {
        if (Generate)
        {
            StartCoroutine(Generation());
            Generate = false;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            Destroy(GameObject.Find("Rooms"));
            cells = generateGrid();
            boundaries = furthestDirections(cells);
            parentObject(cells, "Cells");
            generateQuadrants();
            generateCellWalls(cells);
            Generate = true;
            seeddisplay = "";
        }

    }

    IEnumerator Generation()
    {
        yield return new WaitForSeconds(1.0f);
        GenerateDungeon();

    }

    IEnumerator GenerationDampener(List<Vector3> roomPositions, int[] allRooms, int[] roomSizes)
    {
        for (int i = 0; i < roomPositions.Count; i++)
        {
            string temp = allRooms[roomSizes[i]].ToString();

            int[] roomsize = new int[] { Convert.ToInt32(temp[0].ToString()), Convert.ToInt32(temp[1].ToString()) };
            Vector3 returnPos = roomPositions[i];
            List<GameObject> parent = generateRoom(roomPositions[i], roomsize[0], roomsize[1], out returnPos);
            parentObject(parent, "Rooms", "Rooms " + i);
            yield return new WaitForSeconds(0.3f * Time.deltaTime);
        }

        GameObject Rooms = GameObject.Find("Rooms");

        for (int i = 0; i < Rooms.transform.childCount; i++)
        {
            Rooms.transform.GetChild(i).name = "Room " + i;
        }

        Destroy(GameObject.Find("Cells"));
    }
    #endregion

    #region Generate Dungeon
    private void GenerateDungeon()
    {
        if (roomsPerQuadrant > 9)
            roomsPerQuadrant = 9;                           //Ensure roomsPerQuadrant <= 9

        List<Vector3> roomPositions = new List<Vector3>();  //Store positions for rooms

        int[] defaultlayout = new int[roomsPerQuadrant];                    //Call layouts from quadrant positions
        int[] roomNum = new int[roomsPerQuadrant * QuadrantsWanted];        //Tracks room numbers

        int[] roomSizes = new int[roomsPerQuadrant * QuadrantsWanted];      //Track room sizes
        int[] allRooms = getRoomSizes();                                    //Get the list of room sizes

        #region Position Default Dungeon
        for (int i = 0; i < defaultlayout.Length; i++)
        {
            defaultlayout[i] = i + 4;

            if (defaultlayout[i] > 8)
                defaultlayout[i] -= 9;
        }
        int s = 0;
        for (int i = 0; i < QuadrantsWanted; i++)
        {
            List<Vector3> positions = findRoomPositions(QuadrantBoundaries[i]);
            for (int x = 0; x < roomsPerQuadrant; x++)
            {
                roomPositions.Add(positions[defaultlayout[x]]);
                roomSizes[s] = 0;
                roomNum[s] = s;
                s++;
                
            }
        }

        roomNum = Shuffle(roomNum);     //Shuffle room numbers
        #endregion

        if (seedGen)    //Generate seed
        {
            if (!string.IsNullOrEmpty(seeddisplay)) //If manual seed is input
            {
                seed = seedReturn();                    //Use it
            }

            else
                seed = generateSeed();                  //Generate new
        }

        roomPositions = breakdownSeed(roomPositions, roomNum, roomSizes, allRooms.Length);  //Room Transformations and whatnot

        if (running)    //If the game is running
            StartCoroutine(GenerationDampener(roomPositions, allRooms, roomSizes)); //Generate in coroutine

        else            //Otherwise generate everything instantly
        {
            for (int i = 0; i < roomPositions.Count; i++)
            {
                string temp = allRooms[roomSizes[i]].ToString();

                int[] roomsize = new int[] { Convert.ToInt32(temp[0].ToString()), Convert.ToInt32(temp[1].ToString()) };
                Vector3 returnPos = roomPositions[i];

                List<GameObject> parent = generateRoom(roomPositions[i], roomsize[0], roomsize[1], out returnPos);
                parentObject(parent, "Rooms", "Rooms " + i);
            }
        }

    }

    private List<Vector3> breakdownSeed(List<Vector3> positions, int[] roomNum, int[] roomSizes, int totalRoomSizes)
    {
        for (int i = 0; i < seed.Length; i++)
        {

            if (seed[i] == 0)
            {
                positions = ShiftRooms(positions, roomNum.ToList(), 0, true);   //Shift rooms up in quadrant
                roomSizes = ScaleRooms(roomSizes, roomNum, totalRoomSizes);     //Scale rooms
            }

            else if (seed[i] == 1)
            {
                positions = ShiftRooms(positions, roomNum.ToList(), 1, true);   //Shift rooms right in quadrant
                roomSizes = ScaleRooms(roomSizes, roomNum, totalRoomSizes);     //Scale rooms
            }

            else if (seed[i] == 2)
            {
                positions = ShiftRooms(positions, roomNum.ToList(), 0, false);  //Shift rooms up on grid
                roomSizes = ScaleRooms(roomSizes, roomNum, totalRoomSizes);     //Scale rooms
            }

            else if (seed[i] == 3)
            {
                positions = ShiftRooms(positions, roomNum.ToList(), 1, false);  //Shift rooms right on grid
                roomSizes = ScaleRooms(roomSizes, roomNum, totalRoomSizes);     //Scale rooms
            }

            else if (seed[i] == 4)
            {
                if (roomsPerQuadrant > 3)
                    roomNum = ShuffleNumbers(roomNum, true);
                else
                    roomNum = ShuffleNumbers(roomNum, false);
            }

            else if (seed[i] == 5)
            {
                roomNum = ShuffleNumbers(roomNum, false);
            }

            else if (seed[i] == 6)
            {

            }

            else if (seed[i] == 7)
            {

            }

            else if (seed[i] == 8)
            {

            }

            else if (seed[i] == 9)
            {

            }
        }

        return positions;
    }

    private List<Vector3> findRoomPositions(float[] quadrant)
    {
        //Function is used to find the: Top Left, Top Middle, Top Right, Middle Left, Center, Middle Right
        //                              Bottom Left, Bottom Middle, and Bottom Right
        
        //                              cells of a quadrant

        List<Vector3> roomPositions = new List<Vector3>();  //Stores room positions for quadrant

        float north = quadrant[0];                  //Store north quadrant boundary
        float south = quadrant[1];                  //Store south quadrant boundary
        float east  = quadrant[2];                  //Store east  quadrant boundary
        float west  = quadrant[3];                  //Store west  quadrant boundary

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

    private int[] generateSeed()
    {
        for (int i = 0; i < seed.Length; i++)
        {
            seed[i] = randomGenerator(0, 5);
            seeddisplay += seed[i].ToString();
        }

        return seed;      
    }

    #region Room Transformations

    //All transformations are handled here

    private List<Vector3> ShiftRooms(List<Vector3> positions, List<int> roomNumbers, int direction, bool quadrant)
    {
        float[] boundaries;
        for (int i = 0; i < positions.Count; i++)
        {
            if (quadrant)
                boundaries = QuadrantBoundaries[getQuadrant(positions[i])];
            else
                boundaries = getBoundaries;

            positions[i] = moveCellPosition(direction, positions[i], true, (int)roomNumbers[i]);
        }

        return positions;
    }

    private int[] ScaleRooms(int[] roomSizes, int[] roomNum, int totalRooms, int factor = 1)
    {
        for (int i = 0; i < roomSizes.Length; i++)
        {
            roomSizes[i] += (factor * roomNum[i]);
            roomSizes[i] = limitSize(totalRooms, roomSizes[i]);
        }

        return roomSizes;
    }

    private int limitSize(int roomSizeLength, int position)
    {
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

    private int[] ShuffleNumbers(int[] RoomNumbers, bool quadrantOnly)
    {

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

                roomNums = Shuffle(roomNums);

                iteration = 0;
                for (int x = 0; x < roomsPerQuadrant; x++)
                {
                    RoomNumbers[iteration] = roomNums[x];
                    iteration++;
                }
            }
        }

        else
            Shuffle(RoomNumbers);

        return RoomNumbers;
    }

    private int[] getRoomSizes()
    {
        int[] RoomSizes = new int[] { 33, 34, 35, 36, 37, 43, 44, 45, 46, 47, 53, 54, 55, 56,
                                        57, 63, 64, 65, 66, 67, 73, 74, 75, 76, 77 };
        return RoomSizes;
    }
    #endregion


    #endregion

    #region Properties

    //Used to return the furthest positions of the grid (read-only)
    public float[] getBoundaries 
    {
        get
        {
            return boundaries;
        }
    }

    //Used to add/remove from cell grid
    public static List<GameObject> getCells 
    {
        get 
        {
            return cells;
        }
    }

    //Calculation for movement between cells along x axis
    public int returnCellSizex
    {
        get { return Convert.ToInt32(cellSize.x * 10); }
    }

    //Calculation for movement between cells along z axis
    public int returnCellSizez
    {
        get { return Convert.ToInt32(cellSize.x * 10); }
    }

    #endregion

    #region Universal Functions
    //This section contains functions that are used throughout the entire process of dungeon generation

    //This function creates an empty gameobject parent for objects, or add objects to parent if they already exist
    public static void parentObject(List<GameObject> child, string parentName, string subParentName = "")
    {
        if (child != null)
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

                addChild(childObjects, parent);
            }
        }
    }

    //This function is used by parentObject to add objects to parent
    private static void addChild(List<GameObject> child, GameObject parent)
    {
        for (int i = 0; i < child.Count; i++)
        {
            child[i].transform.parent = parent.transform;
        }
    }

    //This function is used to get a room generated on the level if it exist. Call this before getRoomCells
    private static GameObject getRoomOrHall(string roomHallName)
    {
        if (GameObject.Find(roomHallName))
        {
            return GameObject.Find(roomHallName);
        }

        else
        {
            throw new Exception("Room " + roomHallName + " does not exist");
        }
    }

    //This function is used to edit individual cells in a room if it exists. Call after getRoom
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

    //This function is used to get the quadrant of a position
    private int getQuadrant(Vector3 position)
    {

        for (int i = 0; i < QuadrantsWanted; i++)
        {
            if (position.x > QuadrantBoundaries[i][2])
                continue;

            else if (position.x < QuadrantBoundaries[i][3])
                continue;

            else if (position.z > QuadrantBoundaries[i][0])
                continue;

            else if (position.z < QuadrantBoundaries[i][1])
                continue;

            else
                return i;
        }

        throw new Exception("Couldn't find quadrant for :" + position);
    }

    //This function takes two cells and determines their location in relation to each other
    private void detectCellLocation(Vector3 cellOne, Vector3 cellTwo, out int relation)
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
            int decide = randomGenerator(0, 10);

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

    private int getCellIndex(GameObject cellToGet)
    {
        int index = -1;
        string breakdown = "";
        for (int i = 0; i < cellToGet.name.Length; i++)
        {
            int x = 0;
            if (int.TryParse(cellToGet.name[i].ToString(), out x))
                breakdown += x;
        }
        index = int.Parse(breakdown);
        return index;
    }

    //generates random number between max and min
    public int randomGenerator(int minValue, int maxValue)
    {
        System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());

        int result = rand.Next(minValue, (maxValue + 1));

        return result;
    }

    private int[] Shuffle(int[] numbers)
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

    private int[] seedReturn()
    {
        seed = new int[seeddisplay.Length];

        for (int i = 0; i < seeddisplay.Length; i++)
        {
            seed[i] = int.Parse(seeddisplay[i].ToString());
        }
        return seed;
    }
    #endregion

}
