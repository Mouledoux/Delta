using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GenerationRevamped : MonoBehaviour
{
    /*
        This script is used to generate the structure of the dungeon.
        This consists of the floor, walls, rooms, hallways, and ceiling.

        How dungeon generation works:

        First a little lingo:
            Cell = a single rectangular FLOOR gameobject prefab (primitive plane with a texture for the floor)
            Grid = A large rectangular plane made up of all cells. It is divided into 9 even quadrants.
            Quadrant = A section of the grid made up of a group of cells.

        Alright, now onto the steps.

        Step 1: The script generates a grid
        Step 2: Divide grid into even quadrants (specified as a variable)
        Step 3: Generate four walls around each cell (primitive cube with a texture for walls)
        Step 4: Use GenerationSeed to determine where rooms and hallways belong
        Step 5: Generate the rooms and hallways
     * 
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
     * This script can currently:
     * Generate the grid.
     * Generate basic cells
     * Generate walls around cells
     * Generate the main room
     * Place player in main room
     * Generate boss room
     * Generate hallways
     * Generate based on seed
     * 
     * This script needs to add:
     * Deformity to halls and rooms
     * Secret halls and corridors
     * 
     */
    //After Global Variables, scroll to Start to begin tracking code.

    #region Global Variables

    public bool Generate;           //Determines whether or not to generate past the grid

    public int x_cells, z_cells;    //Number of cells generated along x and z axis
    public int roomsPerQuadrant;    //Number of rooms allowed per quadrant
    public int QuadrantsWanted;     //Number of quadrants to generate

    public int roomsToGen;          //How many rooms the dungeon will gen
    public int hallsToGen;          //How many halls the dungeon will gen

    public Vector3 cellSize;        //Determines the size of individual cells (testing only)
    
    public float cellWallHeight;    //determines height of cell walls
    public float cellWallWidth;     //determines width of cell walls

    public int[] seed;              //stores seed
    public int seeddisplay;         //displays seed (testing only)

    public GameObject FLOOR;        //Floor gameobject
    public GameObject WALL;         //Wall gameobject

    private float[] boundaries = new float[] { 0, 0, 0, 0 };    //Tracks the furthest points of the grid
    private List<float[]> QuadrantBoundaries;                   //Tracks furthest points of each quadrant

    private static List<GameObject> cells;                      //Tracks all of the physical cells used in the game


    #endregion

    #region Generating Cells

    private List<GameObject> generateGrid() 
    {
        //Function generates the grid


        List<GameObject> cells = new List<GameObject>(); //used for keeping track of individual cells

        int x_halfcells = x_cells / 2; //determines the distance from the center of the grid to the side edge.
        int z_halfcells = z_cells / 2; //determines the distance from the center of the grid to the top/bottom edge

        Vector3 startingGridPosition = new Vector3(0.0f, 0.0f, 0.0f); //The grid's bottom edge cells will center on the origin
        Vector3 cellSpace = cellSize * 10; //calculate how much world space a single cell takes up

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


        //calculate the final size of the north/south walls of a cell
        Vector3 nsCellWallSize = new Vector3(returnCellSizex, cellWallHeight, cellWallWidth);

        //calculate the final size of the east/west walls of a cell
        Vector3 ewCellWallSize = new Vector3(cellWallWidth, cellWallHeight, returnCellSizez);

        float nsMovePosition = 10 * (cellSize.z / 2); //tracks the space required to move to the n/s edge of a cell
        float ewMovePosition = 10 * (cellSize.x / 2); //tracks the space required to move to the e/w edge of a cell

        for (int i = 0; i <= a_cells.Count - 1; i++)
        {
            //Set current position to center of cell in world space
            Vector3 currentCellPosition = a_cells[i].transform.position; 

            GameObject[] wall = cellWallGenerator(currentCellPosition, nsMovePosition, ewMovePosition,
                                                    nsCellWallSize, ewCellWallSize);
            for (int x = 0; x < wall.Length; x++)
            {
                wall[x].transform.parent = a_cells[i].transform;    //Set parent of walls to appropriate cell
            }


            //GameObject cellWallParent = new GameObject(); cellWallParent.name = "Cell Walls";
            //parentingGameObjects(l_cellWall, a_cells[i].name + "_walls").transform.parent = cellWallParent.transform;
            //UNCOMMENT THIS ^ IF YOU WANT TO PUT CELL WALLS IN A SEPARATE GAME OBJECT FOR SOME REASON
        }
    }


    private GameObject[] cellWallGenerator(Vector3 cellPosition, float nsCellWallMovePosition,
                                          float ewCellWallMovePosition, Vector3 nsCellWallSize, Vector3 ewCellWallSize)
    {
        //Function generates cell walls

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

        wall[0].transform.position += new Vector3(0.0f, 0.0f, nsCellWallMovePosition); //Move north wall to top of cell
        wall[1].transform.position += new Vector3(ewCellWallMovePosition, 0.0f, 0.0f); //Move east wall to right of cell
        wall[2].transform.position -= new Vector3(0.0f, 0.0f, nsCellWallMovePosition); //Move south wall to bottom of cell
        wall[3].transform.position -= new Vector3(ewCellWallMovePosition, 0.0f, 0.0f); //Move west wall to left of cell

        return wall;    //return walls
    }

    private float[] furthestDirections(List<GameObject> a_cells)
    {
        //Function returns the furthest points in all directions
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

    private void generateQuadrants()
    {
        //Function divides grid into quadrants

        float sqrtQuadWanted = Mathf.Sqrt(QuadrantsWanted);
        int x_quads = 0;
        int z_quads = 0;
        float totalWidth = (Mathf.Abs(boundaries[3]) + boundaries[2]);

        if (sqrtQuadWanted % 1 == 0)
        {
            x_quads = z_quads = (int)sqrtQuadWanted;
        }

        else
        {
            throw new Exception("Generation only handles a square number of quadrants");
        }

        float quadrantHeight = boundaries[0] / sqrtQuadWanted;

        DrawQuadrant(totalWidth, boundaries[0], x_quads, z_quads);
    }

    private List<List<GameObject>> DrawQuadrant(float gridWidth, float gridHeight, int x_quads, int z_quads)
    {

        List<List<GameObject>> Quadrants = new List<List<GameObject>>();    //List for Quadrant Gameobjects
        List<float[]> Zones = new List<float[]>();

        int H_movement = Convert.ToInt32(gridWidth / x_quads);
        int V_movement = Convert.ToInt32(gridHeight / z_quads);
        Debug.Log(H_movement);

        for (int i = 0; i < QuadrantsWanted; i++)
        {
            Quadrants.Add(new List<GameObject>());
            Zones.Add(new float[] {0,0,0,0 });
        }

        int zone = 0;
        for (int i = 0; i < z_quads; i++)
        {
            float northbase = V_movement + (V_movement * i);
            float southbase = V_movement * i;

            for (int x = 0; x < x_quads; x++)
            {
                Zones[zone][0] = northbase;
                Zones[zone][1] = boundaries[3] + (H_movement + (H_movement * x));
                Zones[zone][2] = southbase;
                Zones[zone][3] = boundaries[3] + (H_movement * x);
                zone += 1;
            }
        }

        getQuadrantBoundaries = Zones;
        
        for (int i = 0; i < cells.Count; i++)
        {
            Vector3 position = cells[i].transform.position;

            for (int x = 0; x < Zones.Count; x++)
            {
                if (position.z > Zones[x][0])
                {
                    continue;
                }

                else if (position.z < Zones[x][2])
                    continue;

                else if (position.x > Zones[x][1])
                    continue;

                else if (position.x < Zones[x][3])
                    continue;

                else
                {
                    Quadrants[x].Add(cells[i]);
                    break;
                }
            }
        }

        for (int i = 0; i < Quadrants.Count; i++)
        {
            parentObject(Quadrants[i], "Cells", "Quadrant " + i);
        }

        return Quadrants;
    }

    void determineQuadPosition()
    {

    }
    #endregion

    #region Generate Rooms

    private List<GameObject> generateRoom(Vector3 roomCenter, int roomSizex, int roomSizez, int Quadrant, out Vector3 returnCenter)
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
       
        //If the center is not in a valid position on the grid...
        //  (ex. Is not on the grid, exceeds boundaries of grid or quadrant)
        //...it will be shifted to the nearest valid position
        roomCenter.x = confineRoom(roomCenter.x, roomSizexCheck , boundaries[2], boundaries[3]);    //check against grid east and west boundaries
        roomCenter.x = confineRoom(roomCenter.x, roomSizexCheck, QuadrantBoundaries[Quadrant][2], QuadrantBoundaries[Quadrant][3]); //check against quadrant east and west boundaries
        roomCenter.z = confineRoom(roomCenter.z, roomSizezCheck, boundaries[0], boundaries[1]);     //check against grid north and south boundaries
        roomCenter.z = confineRoom(roomCenter.z, roomSizezCheck, QuadrantBoundaries[Quadrant][0], QuadrantBoundaries[Quadrant][1]); //check against quadrant north and south boundaries
        returnCenter = roomCenter;  //store the new room center

        //The following generates a null exception
        //BEGIN
        if (findCell(roomCenter).transform.parent.name == "Main Room" 
        || findCell(roomCenter).transform.parent.name == "Boss Room"
        || findCell(roomCenter).name.Contains("Center"))
        {
            return null;
        }
        //END

        findCell(returnCenter).name += " Center";   //Name room
        cellsInRoom.Add(findCell(roomCenter));      //Add room to list of cells in room

        //calculates starting position (bottom left corner of room)
        Vector3 startingPosition = roomCenter + new Vector3(-roomSizex / 2  * returnCellSizex, 0.0f, -roomSizez / 2 * returnCellSizez);

        for (int i = 0; i < roomSizez; i++)
        {
            Vector3 curPosition = startingPosition;             //set current position to starting position
            curPosition = moveCellPosition(0, curPosition, i);  //moves between cells of the room on z axis

            for (int v = 0; v < roomSizex; v++)
            {
                //Don't affect Main Room and Boss Room
                string nameTest = findCell(curPosition).transform.parent.name;  //get name of cell's parent at the current position
                if (nameTest != "Main Room" && nameTest != "Boss Room" && !nameTest.Contains("Center")) //If it is not a main room or the center for another room
                {
                    cellsInRoom.Add(findCell(curPosition)); //Add cell to List of gameobjects of room

                    if (findCell(curPosition).transform.parent.name.Contains("Room"))
                    {
                        mergeRoom = true;
                        roomToMergePosition = curPosition;
                    }
                }
                curPosition = moveCellPosition(1, curPosition); //move between cells of room on x axis
            }  
        }
        float[] roomBoundaries = furthestDirections(cellsInRoom); //gets the boundaries of the rooms

        foreach (GameObject cell in cellsInRoom)
        {
            destroyWalls(cell, roomBoundaries); //Destroys walls of cells that aren't on the edges of the room
        }

        if (mergeRoom)
        {
            parentObject(cellsInRoom, "Rooms", findCell(roomToMergePosition).transform.parent.name);
            return null;
        }
        return cellsInRoom;
    }

    //Destroys the walls of a cell
    private void destroyWalls(GameObject cell, float[] roomBoundaries)
    {
        var children = new List<GameObject>();
        float[] bound = getBoundaries;
        foreach (Transform child in cell.transform)
        {
            children.Add(child.gameObject);
            
        }

        foreach(GameObject child in children)
        {
            if (cell.transform.position.z != getBoundaries[1] && cell.transform.position.z != roomBoundaries[1] && child.name.Contains("south") ||
                cell.transform.position.z != getBoundaries[0] && cell.transform.position.z != roomBoundaries[0] && child.name.Contains("north") ||
                cell.transform.position.x != getBoundaries[2] && cell.transform.position.x != roomBoundaries[2] && child.name.Contains("east") ||
                cell.transform.position.x != getBoundaries[3] && cell.transform.position.x != roomBoundaries[3] && child.name.Contains("west"))
            {
                Destroy(child.gameObject);
                child.transform.parent = null;
            }
        }
;
    }
     
    //This function properly moves vector positions between cells.
    private Vector3 moveCellPosition(int direction, Vector3 position, int movement = 1)
    {
        //Debug.Log("Moving Cell Position");
        //Debug.Log("Current Pos = " + position);
        int timesMoved = 0;

        while (timesMoved < movement)
        {
            switch (direction)
            {
                case 0: //north
                    //Debug.Log("Moving North");
                    position += new Vector3(0.0f, 0.0f, cellSize.z * 10);
                    break;

                case 1: //east
                   // Debug.Log("Moving east");
                    position += new Vector3(cellSize.x * 10, 0.0f, 0.0f);
                    break;

                case 2: //south
                    //Debug.Log("Moving South");
                    position -= new Vector3(0.0f, 0.0f, cellSize.z * 10);
                    break;

                case 3: //west
                   // Debug.Log("Moving West");
                    position -= new Vector3(cellSize.x * 10, 0.0f, 0.0f);
                    break;
            }

            timesMoved += 1;
        }
        //Debug.Log("Final Position = " + position);

        return position;
    }

    //This function returns a cell that matches the position of the parameter, if it exists
    private GameObject findCell(Vector3 cellPosition)
    {
        GameObject targetCell = null;

        for (int i = 0; i < cells.Count; i++)
        {
            if (cells[i].transform.position == cellPosition)
            {
                targetCell = cells[i];
                break;
            }
        }

        return targetCell;
    }

    //This function ensures any room created is pushed onto the grid
    private static float confineRoom(float point, int roomSize, float check1, float check2)
    {
        while (point + roomSize > check1)
        {
            point -= 1.0f;
        }

        while (point - roomSize < check2)
        {
            point += 1.0f;
        }

        return point;
    }

    //This function ensures that a room is created from the center of a cell
    private static int roomFinalizer(float value, float cellSize)
    {
        Debug.Log("Value =" + value);
        cellSize = cellSize * 10;
        int valueint = Convert.ToInt32(value);

        int finalValue = Convert.ToInt32(valueint - (valueint % cellSize));

        Debug.Log(" = " + (valueint  - (valueint % cellSize)));

        return finalValue;
    }

    #endregion

    #region Generate Hallways

    //Use this method to connect two cells. IF this returns false, there's no need to connect the cells
    private bool connectCells(GameObject cellIn, GameObject cellToConnect)
    {
        string cellWallOne, cellWallTwo;
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
        float cellCalcx = cellSize.x *10;
        float cellCalcz = cellSize.z *10;

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

    //This method takes any two cell positions on the grid and creates a hall between them
    //By default, it will use the shortest possible path, however the path can be offset
    //by using an integer from 0-9
    private List<GameObject> createHall(Vector3 startingCell, Vector3 endCell, int offset)
    {
        int start = -1;

        endCell = MoveEntrance(endCell, out start);
        startingCell = MoveEntrance(startingCell, out start);
        List<GameObject> hall = caclulateTravelCells(startingCell, endCell, start);

        for (int i = 0; i < hall.Count - 1; i++)
        {
            if (!connectCells(hall[i], hall[i + 1]))
            {
                break;
            }
        }

        return hall;
    }

    private Vector3 MoveEntrance(Vector3 cell, out int startDirection)
    {
        startDirection = -1;
        for (int i = 0; i < findCell(cell).transform.childCount; i++)
        {
            if (findCell(cell).transform.GetChild(i).name.Contains("north"))
            {
                cell = moveCellPosition(0, cell);
                startDirection = 0;
                break;
            }

            if (findCell(cell).transform.GetChild(i).name.Contains("south"))
            {
                cell = moveCellPosition(2, cell);
                startDirection = 1;       
                break;
            }

            if (findCell(cell).transform.GetChild(i).name.Contains("east"))
            {
                cell = moveCellPosition(1, cell);
                startDirection = 2;
                break;
            }

            if (findCell(cell).transform.GetChild(i).name.Contains("west"))
            {
                cell = moveCellPosition(3, cell);
                startDirection = 3;
                break;
            }
        }
        return cell;
    }

    private List<GameObject> caclulateTravelCells(Vector3 start, Vector3 end, int startDirect)
    {
        List<GameObject> hall = new List<GameObject>();
        float xMove = 0.0f, zMove = 0.0f;
        int nxDirection = determineTravelDirectionX( end.x, start.x, out xMove);
        int nzDirection = determineTravelDirectionX( end.z, start.z, out zMove);

        int xDirection = determineTravelDirectionX(start.x, end.x, out xMove);
        int zDirection = determineTravelDirectionZ(start.z, end.z, out zMove);
        xMove = Convert.ToInt32(Math.Abs(xMove)) / returnCellSizex;
        zMove = Convert.ToInt32(Math.Abs(zMove)) / returnCellSizez;
        //hall.Add(findCell(start));

        if (startDirect == 0 || startDirect == 1)
        {
            hall.Add(findCell(start));
            hall = zTravel(start, zMove, hall, zDirection);
            start = hall[hall.Count - 1].transform.position;
            hall = xTravel(start, xMove, hall, xDirection);

        }

        else
        {
            hall.Add(findCell(start));
            hall = xTravel(start, xMove, hall, xDirection);
            start = hall[hall.Count - 1].transform.position;
            hall = zTravel(start, zMove, hall, zDirection);
        }


        return hall;
    }

    private List<GameObject> xTravel(Vector3 start, float xmovement, List<GameObject> hall, int xDirect)
    {
        for (int i = 0; i < xmovement; i++)
        {
            start = moveCellPosition(xDirect, start);
            if (!findCell(start).transform.parent.name.Contains("Room") && !findCell(start).transform.parent.name.Contains("Hall"))
                hall.Add(findCell(start));

            else
            {
                connectCells(findCell(start), hall[hall.Count - 1]);
                break;
            }
        }
        return hall;
    }

    private List<GameObject> zTravel(Vector3 start, float zmovement, List<GameObject> hall, int zDirect)
    {
        for (int i = 0; i < zmovement; i++)
        {
            start = moveCellPosition(zDirect, start);
            if (!findCell(start).transform.parent.name.Contains("Room") && !findCell(start).transform.parent.name.Contains("Hall"))
                hall.Add(findCell(start));

            else
            { 
                connectCells(findCell(start), hall[hall.Count - 1]);
                break;
            }
        }
        return hall;
    }
    //used by calculateTravelCells to determine direction needed for x movement
    private int determineTravelDirectionX(float start, float end, out float distance)
    {
        distance = end - start;

        if (start < end)
            return 1; //east

        return 3; //west
    }

    //used by calculateTravelCells to determine direction needed for z movement
    private int determineTravelDirectionZ(float start, float end, out float distance)
    {
        distance = end - start;

        if (start < end)
            return 0; //north

        return 2; //south
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

    #region Start


    void Start()
    {
        //Generating Cells
        cells = generateGrid();                 //generates grid
        parentObject(cells, "Cells");           //Parents every cell generated under a game object named "Cells"

        boundaries = furthestDirections(cells); //Gets the furthest positions of the grid (n,s,e,w)
        generateQuadrants();                    //Breaks the grid into 9 quadrants

        if (Generate) //If we want to generate walls, rooms, and hallways
        {
            generateCellWalls(cells); //Generates walls around each cell

            //GenerateDungeon();                  //Generate Dungeon
            //Destroy(GameObject.Find("Cells")); //Destroy whatever isn't used
        }
    }
    #endregion

    #region GenerateDungeon
    private void GenerateDungeon()
    {

    }

    private void defaultDungeon()
    {
        List<List<Vector3>> rooms = new List<List<Vector3>>();
        List<Vector3> roomPositions = new List<Vector3>();
        roomPositions.Add(new Vector3());   //Top Middle    0
        roomPositions.Add(new Vector3());   //Bottom Middle 1
        roomPositions.Add(new Vector3());   //Right Middle  2
        roomPositions.Add(new Vector3());   //Left Middle   3
        roomPositions.Add(new Vector3());   //Top Left      4
        roomPositions.Add(new Vector3());   //Top Right     5
        roomPositions.Add(new Vector3());   //Bottom Left   6
        roomPositions.Add(new Vector3());   //Bottom Right  7
        roomPositions.Add(new Vector3());   //Center        8

        List<float> QuadrantDimensions = new List<float>();

        for (int i = 0; i < QuadrantsWanted; i++)
        {
            float dimension = getQuadrantDimensions(QuadrantBoundaries[i]);
            int d = -1;
            if (!checkForDimension(QuadrantDimensions, dimension, out d))
            {
                QuadrantDimensions.Add(dimension);
                roomPositions = findRoomPositions(QuadrantBoundaries[i]);
                rooms.Add(roomPositions);
            }

            int room = 0;
            while (room < roomsPerQuadrant)
            {

            }
        }
    }

    private List<Vector3> findRoomPositions(float[] quadrant)
    {
        List<Vector3> roomPositions = new List<Vector3>();

        float north = quadrant[0];
        float south = quadrant[2];
        float east  = quadrant[1];
        float west  = quadrant[3];
        float totalHeight = north - south;
        float totalWidth = Mathf.Abs(east - west);
        int xcells = Convert.ToInt32(totalWidth / returnCellSizex);
        int zcells = Convert.ToInt32(totalHeight / returnCellSizez);
        float xmid = (returnCellSizex * (xcells / 2));
        float zmid = (returnCellSizez * (zcells / 2));

        roomPositions.Add(new Vector3(xmid, 0.0f, north));  //Top Middle
        roomPositions.Add(new Vector3(xmid, 0.0f, south));  //Bottom Middle
        roomPositions.Add(new Vector3(east, 0.0f, zmid));   //Right Middle
        roomPositions.Add(new Vector3(west, 0.0f, zmid));   //Left Middle
        roomPositions.Add(new Vector3(west, 0.0f, north));  //Top Left
        roomPositions.Add(new Vector3(east, 0.0f, north));  //Top Right
        roomPositions.Add(new Vector3(west, 0.0f, south));  //Bottom Left
        roomPositions.Add(new Vector3(east, 0.0f, south));  //Bottom Right
        roomPositions.Add(new Vector3(xmid, 0.0f, zmid));   //Center

        return roomPositions;
    }

    private float getQuadrantDimensions(float[] quadrant)
    {
        float north = quadrant[0];
        float south = quadrant[2];
        float east = quadrant[1];
        float west = quadrant[3];
        float totalHeight = north - south;
        float totalWidth = Mathf.Abs(east - west);
        float totalDimension = totalHeight * totalWidth;

        return totalDimension;
    }

    private bool checkForDimension(List<float> QuadrantDimensions, float dimension, out int d)
    {
        d = -1;
        if (QuadrantDimensions.Count > 0)
        {
            for (int x = 0; x < QuadrantDimensions.Count; x++)
            {
                if (dimension == QuadrantDimensions[x])
                {
                    d = x;
                    return true;
                }
            }
        }
        return false;
    }
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

    public List<float[]> getQuadrantBoundaries
    {
        get
        {
            return QuadrantBoundaries;
        }

        set
        {
            QuadrantBoundaries = value;
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
                    throw new Exception(@"Part of Room provided does not exist in any of the objects children
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

    //This function is used to get the quadrant of a cell
    private int getQuadrant(GameObject cell)
    {
        float totalWidth = (Mathf.Abs(boundaries[3]) + boundaries[2]) / 3;
        float totalHeight = boundaries[0] / 3;

        //Q0
        if (cell.transform.position.x <= totalWidth + boundaries[3] && cell.transform.position.z <= totalHeight)
        {
            return 0;
        }
        //Q1
        else if (cell.transform.position.x >= totalWidth + boundaries[3] && cell.transform.position.x <= totalWidth * 2 + boundaries[3] && cell.transform.position.z <= totalHeight)
        {
            return 1;
        }
        //Q2
        else if (cell.transform.position.x >= totalWidth * 2 + boundaries[3] && cell.transform.position.z <= totalHeight)
        {
            return 2;
        }

        //Q3
        else if (cell.transform.position.x <= totalWidth + boundaries[3] && cell.transform.position.z >= totalHeight && cell.transform.position.z <= totalHeight * 2)
        {
            return 3;
        }

        //Q4
        else if (cell.transform.position.x >= totalWidth + boundaries[3] && cell.transform.position.x <= totalWidth * 2 + boundaries[3] && cell.transform.position.z >= totalHeight && cell.transform.position.z <= totalHeight * 2)
        {
            return 4;
        }

        //Q5
        else if (cell.transform.position.x >= totalWidth * 2 + boundaries[3] && cell.transform.position.z >= totalHeight && cell.transform.position.z <= totalHeight * 2)
        {
            return 5;
        }

        //Q6
        else if (cell.transform.position.x <= totalWidth + boundaries[3] && cell.transform.position.z >= totalHeight * 2)
        {
            return 6;
        }
        //Q7
        else if (cell.transform.position.x >= totalWidth + boundaries[3] && cell.transform.position.x <= totalWidth * 2 + boundaries[3] && cell.transform.position.z >= totalHeight * 2)
        {
            return 7;
        }

        //Q8
        else if (cell.transform.position.x >= totalWidth * 2 + boundaries[3] && cell.transform.position.z >= totalHeight * 2)
        {
            return 8;
        }

        throw new Exception("Could not return Quadrant");
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
            int decide = GenerationSeed.randomGenerator(0, 10);

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
    #endregion
}
