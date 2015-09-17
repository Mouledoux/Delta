using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class GenerationRevamped : MonoBehaviour
{
    /*Ignore this script for now.
     * This is just so I can brainstorm with a few concepts as far as generation is concerned.
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
    //Scroll to Start to begin tracking code.

    #region Global Variables
    public bool Generate;
    private static List<GameObject> cells; //Tracks all of the physical cells used in the game
    public int x_cells, z_cells; //determines the size of the grid (testing only)
    public Vector3 cellSize; //determines the size of individual cells (testing only)
    public int roomsPerQuadrant;
    private float[] boundaries = new float[] { 0, 0, 0, 0 }; //Tracks the farthest points of the grid
    private float[][] QuadrantBoundaries; //Tracks furthest points of each quadrant
    public string seed; //Stores seed
    public GameObject FLOOR;
    public GameObject WALL;
    #endregion

    #region Generating Cells

    public float cellWallHeight; //determines height of cell walls
    public float cellWallWidth; //determines width of cell walls

    //This function generates a grid and returns all of its cells in a list of game objects
    private List<GameObject> generateGrid() 
    {
        List<GameObject> cells = new List<GameObject>(); //used for keeping track of individual cells

        int x_halfcells = x_cells / 2; //determines the distance from the center of the grid to a side edge.
        int z_halfcells = z_cells / 2; //determines the distance from the center of the grid to a top/bottom edge
        Vector3 startingGridPosition = new Vector3(0.0f, 0.0f, 0.0f); //The grid's bottom edge cells will center on the origin
        Vector3 cellSpace = cellSize * 10; //calculation for how many spaces a single cell takes up
        int cellName = 0;
        for (int i = 0; i < z_cells; i++)
        {
            Vector3 currentGridPosition = startingGridPosition; //keeps track of the current position being occupied
            currentGridPosition.x += (x_halfcells * cellSpace.x); //starting from the bottom right corner of the grid

            if (i != 0)
            {
                currentGridPosition.z += cellSpace.z * i; //move the grid position up by z-axis
            }

            for (int x = 0; x < x_cells; x++)
            {
                GameObject cell = Instantiate(FLOOR); //Create new cell
                cell.name = "cell " + cellName; //name the cell for easier reference
                cellName++;
                cell.transform.position = currentGridPosition; //bring cell to correct place on grid
                cell.transform.localScale = cellSize; //set cell to the size determined
                cells.Add(cell); //add cell to cells List
                currentGridPosition.x -= cellSpace.x; //update currentGridPosition to next empty spot
            }

        }

        return cells;
    }

    //This function Generates Cell Walls
    List<List<GameObject>> generateCellWalls(List<GameObject> a_cells)
    {
        //calculate the final size of the north/south walls of a cell
        Vector3 nsCellWallSize = new Vector3(0.0f, cellWallHeight, cellWallWidth);
 
        nsCellWallSize.x = cellSize.x * 10; //sets the north/south wall to the length of the edge of the cell

        //calculate the final size of the east/west walls of a cell
        Vector3 ewCellWallSize = new Vector3(cellWallWidth, cellWallHeight, 0.0f);
        ewCellWallSize.z = cellSize.z * 10; //sets the east/west wall to the length of the edge of the cell

        float nsCellWallMovePosition = 10 * (cellSize.z / 2); //tracks the space required to move to the n/s edge of a cell
        float ewCellWallMovePosition = 10 * (cellSize.x / 2); //tracks the space required to move to the e/w edge of a cell

        List<List<GameObject>> allCellWalls = new List<List<GameObject>>(); //create list of the list of cell walls for cells
        for (int i = 0; i <= a_cells.Count - 1; i++)
        {
            Vector3 currentCellPosition = a_cells[i].transform.position; //Sets current position to center of cell
            List<GameObject> l_cellWall = new List<GameObject>();
            /*
             * Four cell walls will be generated for each cell
             * Each list will contain 4 walls in order from north, east, south, west
             * translating to positions 0, 1, 2, 3 inside the list.
             * This list is added to the cellsWalls list of list.
             * This makes it easier to track the location of walls within individual cells
             * rather than trying to decipher exactly where all walls are using only a single list
             */

            GameObject[] wall = cellWallGenerator(i, currentCellPosition, nsCellWallMovePosition, ewCellWallMovePosition,
                                                    nsCellWallSize, ewCellWallSize);
            for (int x = 0; x < wall.Length; x++)
            {
                wall[x].transform.parent = a_cells[i].transform;
                l_cellWall.Add(wall[x]);
            }

            allCellWalls.Add(l_cellWall);
            //GameObject cellWallParent = new GameObject(); cellWallParent.name = "Cell Walls";
            //parentingGameObjects(l_cellWall, a_cells[i].name + "_walls").transform.parent = cellWallParent.transform;
            //uncomment this ^ if you want to put cell walls in a seperate game object
        }

            return allCellWalls;
    }

    //This function helps generateCellWalls generate walls. Argument i is from the for loop
    private GameObject[] cellWallGenerator(int i, Vector3 cellPosition, float nsCellWallMovePosition,
                                          float ewCellWallMovePosition, Vector3 nsCellWallSize, Vector3 ewCellWallSize)
    {
        GameObject[] wall = new GameObject[] {Instantiate(WALL), Instantiate(WALL), Instantiate(WALL), Instantiate(WALL)};

        wall[0].name = "north_wall"; //identifies north wall
        wall[1].name = "east_wall"; //identifies east wall
        wall[2].name = "south_wall"; //identifies south wall
        wall[3].name = "west_wall"; //identifies west wall

        wall[0].transform.localScale = nsCellWallSize; //adjust scale for north wall
        wall[1].transform.localScale = ewCellWallSize; //adjust scale for east wall
        wall[2].transform.localScale = nsCellWallSize; //adjust scale for south wall
        wall[3].transform.localScale = ewCellWallSize; //adjust scale for west wall

        for (int x = 0; x < wall.Length; x++ )
        {
            //moves wall to the top of the plane (cell)
            wall[x].transform.position = cellPosition + new Vector3(0.0f, wall[x].transform.localScale.y / 2, 0.0f);
        }

        wall[0].transform.position += new Vector3(0.0f, 0.0f, nsCellWallMovePosition); //Move to the north wall position
        wall[1].transform.position += new Vector3(ewCellWallMovePosition, 0.0f, 0.0f); //Move to the east wall position
        wall[2].transform.position -= new Vector3(0.0f, 0.0f, nsCellWallMovePosition); //Move to the south wall position
        wall[3].transform.position -= new Vector3(ewCellWallMovePosition, 0.0f, 0.0f); //Move to the west wall position

        return wall;
    }

    //This function returns the furthest points in all directions
    private float[] farthestDirections(List<GameObject> a_cells)
    {

        float[] farthestPoints = new float[] { -500.0f, 500.0f, -500.0f, 500.0f }; //0 = north, 1 = south, 2 = east, 3 = west

        for (int i = 0; i < a_cells.Count; i++)
        {
            if (a_cells[i].transform.position.x > farthestPoints[2])
            {
                farthestPoints[2] = a_cells[i].transform.position.x;
            }

            if (a_cells[i].transform.position.x < farthestPoints[3])
            {
                farthestPoints[3] = a_cells[i].transform.position.x;
            }

            if (a_cells[i].transform.position.z > farthestPoints[0])
            {
                farthestPoints[0] = a_cells[i].transform.position.z;
            }

            if (a_cells[i].transform.position.z < farthestPoints[1])
            {
                farthestPoints[1] = a_cells[i].transform.position.z;
            }
        }

            return farthestPoints;
    }

    private void generateQuadrants()
    {
        float totalWidth = (Mathf.Abs(boundaries[3]) + boundaries[2]) / 3;
        float totalHeight = boundaries[0] / 3;

        List<List<GameObject>> quadrants = DrawQuadrant(totalWidth, totalHeight);
        QuadrantBoundaries = new float[quadrants.Count][];
        for (int i = 0; i < quadrants.Count; i++ )
        {
            QuadrantBoundaries[i] = farthestDirections(quadrants[i]);
        }
    }

    private List<List<GameObject>> DrawQuadrant(float totalWidth, float totalHeight)
    {
        List<List<GameObject>> Quadrants = new List<List<GameObject>>();
        Quadrants.Add(new List<GameObject>());
        Quadrants.Add(new List<GameObject>());
        Quadrants.Add(new List<GameObject>());
        Quadrants.Add(new List<GameObject>());
        Quadrants.Add(new List<GameObject>());
        Quadrants.Add(new List<GameObject>());
        Quadrants.Add(new List<GameObject>());
        Quadrants.Add(new List<GameObject>());
        Quadrants.Add(new List<GameObject>());
        
        for (int i = 0; i < cells.Count; i++)
        {
            //Q0
            if (cells[i].transform.position.x <= totalWidth + boundaries[3] && cells[i].transform.position.z <= totalHeight)
            {
                Quadrants[0].Add(cells[i]);
            }
            //Q1
            else if (cells[i].transform.position.x >= totalWidth + boundaries[3] && cells[i].transform.position.x <= totalWidth * 2 + boundaries[3] && cells[i].transform.position.z <= totalHeight)
            {
                Quadrants[1].Add(cells[i]);
            }
            //Q2
            else if (cells[i].transform.position.x >= totalWidth * 2 + boundaries[3] && cells[i].transform.position.z <= totalHeight)
            {
                Quadrants[2].Add(cells[i]);
            }

            //Q3
            else if (cells[i].transform.position.x <= totalWidth + boundaries[3] && cells[i].transform.position.z >= totalHeight && cells[i].transform.position.z <= totalHeight * 2)
            {
                Quadrants[3].Add(cells[i]);
            }

            //Q4
            else if (cells[i].transform.position.x >= totalWidth + boundaries[3] && cells[i].transform.position.x <= totalWidth * 2 + boundaries[3] && cells[i].transform.position.z >= totalHeight && cells[i].transform.position.z <= totalHeight * 2)
            {
                Quadrants[4].Add(cells[i]);
            }

            //Q5
            else if (cells[i].transform.position.x >= totalWidth * 2 + boundaries[3] && cells[i].transform.position.z >= totalHeight && cells[i].transform.position.z <= totalHeight * 2)
            {
                Quadrants[5].Add(cells[i]);
            }

            //Q6
            else if (cells[i].transform.position.x <= totalWidth + boundaries[3] && cells[i].transform.position.z >= totalHeight * 2)
            {
                Quadrants[6].Add(cells[i]);
            }
            //Q7
            else if (cells[i].transform.position.x >= totalWidth + boundaries[3] && cells[i].transform.position.x <= totalWidth * 2 + boundaries[3] && cells[i].transform.position.z >= totalHeight * 2)
            {
                Quadrants[7].Add(cells[i]);
            }

            //Q8
            else if (cells[i].transform.position.x >= totalWidth * 2 + boundaries[3] && cells[i].transform.position.z >= totalHeight * 2)
            {
                Quadrants[8].Add(cells[i]);
            }
        }

        for (int i = 0; i < Quadrants.Count; i++)
        {
            parentObject(Quadrants[i], "Cells", "Quadrant " + i);
        }

        return Quadrants;
    }
    #endregion

    #region Generate Rooms

    //This function generates any room. Room center is center of room, roomSize is dimension of room
    private List<GameObject> generateRoom(Vector3 roomCenter, int roomSizex, int roomSizez, int Quadrant, out Vector3 returnCenter)
    {
        bool mergeRoom = false;
        Vector3 roomToMergePosition = Vector3.zero;
        int roomSizezCheck = ((roomSizez) / 2) * returnCellSizez; //Used for checking size of room from center along z
        int roomSizexCheck = ((roomSizex) / 2) * returnCellSizex; //Used for checking size of room from center along x
        List<GameObject> cellsInRoom = new List<GameObject>(); //Used to store positions of the room
       
        //Confines room to grid and to boundaries of the quadrant it's in
        roomCenter.x = confineRoom(roomCenter.x, roomSizexCheck , boundaries[2], boundaries[3]);
        roomCenter.x = confineRoom(roomCenter.x, roomSizexCheck, QuadrantBoundaries[Quadrant][2], QuadrantBoundaries[Quadrant][3]);
        roomCenter.z = confineRoom(roomCenter.z, roomSizezCheck, boundaries[0], boundaries[1]);
        roomCenter.z = confineRoom(roomCenter.z, roomSizezCheck, QuadrantBoundaries[Quadrant][0], QuadrantBoundaries[Quadrant][1]);
        returnCenter = roomCenter;

        if (findCell(roomCenter).transform.parent.name == "Main Room" 
            || findCell(roomCenter).transform.parent.name == "Boss Room"
            || findCell(roomCenter).name.Contains("Center"))
        {
            return null;
        }
        //Returns new position for center of room
        findCell(returnCenter).name += " Center";
        cellsInRoom.Add(findCell(roomCenter));

        //calculates starting position (bottom left corner of room)
        Vector3 startingPosition = roomCenter + new Vector3(-roomSizex / 2  * returnCellSizex, 0.0f, -roomSizez / 2 * returnCellSizez);

        for (int i = 0; i < roomSizez; i++)
        {
            Vector3 curPosition = startingPosition; //set current position to starting position
            curPosition = moveCellPosition(0, curPosition, i); //moves between cells of the room on z axis

            for (int v = 0; v < roomSizex; v++)
            {
                //Don't affect Main Room and Boss Room
                string nameTest = findCell(curPosition).transform.parent.name;
                if (nameTest != "Main Room" && nameTest != "Boss Room" && !nameTest.Contains("Center"))
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
        float[] roomBoundaries = farthestDirections(cellsInRoom); //gets the boundaries of the rooms

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
    public int roomsToGen; //How many rooms the dungeon will gen
    public int hallsToGen; //How many halls the dungeon will gen

    void Start()
    {
        //Generating Cells
        cells = generateGrid(); //generates grid (A very large plane made up of smaller planes(cells) )

        parentObject(cells, "Cells"); //Parents every cell generated under cell

        if (Generate) //generate more than just grids
        {
            List<List<GameObject>> cellWalls = generateCellWalls(cells); //Generates walls around each cell
            boundaries = farthestDirections(cells); //Gets the furthest positions of the grid (n,s,e,w)
            generateQuadrants(); //Breaks the grid into 9 quadrants
        
            if (string.IsNullOrEmpty(seed)) //Generate seed if seed one isn't input
            {
                roomsToGen = GenerationSeed.randomGenerator(minMaxRooms[0], minMaxRooms[1]); //Get how many rooms to generate (At least 2)
                hallsToGen = roomsToGen - 2;//GenerationSeed.randomGenerator(roomToGen, roomToGen + (roomToGen / 2)); //Get how many halls to generate (Based on roomsToGen)
                seed += roomsToGen; seed += hallsToGen; //add rooms to gen and halls to gen to seed
                GenerateSeed(roomsToGen, hallsToGen); //Generate rest of seed
            }
            //Generate dungeon
            GenerateDungeon();
            Destroy(GameObject.Find("Cells")); //Destroy whatever isn't used
        }
    }
    #endregion

    #region Generate Seed
    public int[] minMaxRooms = new int[] { 0, 0 };
    private List<int> generatedRooms = new List<int>(); //tracks cells that are centers of rooms

    private void GenerateSeed(int roomsToGen, int hallstoGen)
    {
        int maxCells = x_cells * z_cells;
        int[] roomsInQuadrants = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        GenerateRoomSeed(roomsToGen, roomsInQuadrants);
      
    }

    private void GenerateRoomSeed(int roomsToGen, int[] roomsInQuadrants)
    {
        //Track quadrants currently available
        List<int> availableQuadrants = new List<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8 };

        for (int i = 0; i < roomsToGen; i++)
        {
            //Gets a random quadrant
            int quadrantToGet = availableQuadrants[GenerationSeed.randomGenerator(0, availableQuadrants.Count)];

            ///////////////////////////////////////////////////////////////////////////////
            if (i == 1) //Prevents main room and boss rooms from being in the same quadrant
            {                                                                           ///
                while (roomsInQuadrants[quadrantToGet] > 0)                             ///
                    quadrantToGet = GenerationSeed.randomGenerator(0, 9);               ///
            }                                                                           ///
            ///////////////////////////////////////////////////////////////////////////////

            roomsInQuadrants[quadrantToGet] += 1;   //Increments number of rooms in quadrant

            int roomToGet = -1; //Initialize variable

            
            GameObject quads = GameObject.Find("Quadrant " + quadrantToGet); //Used for room positioning test |
             //                                                                                               V
            while (!GenerationSeed.checkGeneratedRoomPosition(roomToGet, generatedRooms))
            {
                int chances = 2; //If after two tries the selected cell is in main room or boss room, select another quad

                roomToGet = GenerationSeed.randomGenerator(0, quads.transform.childCount); //Get a random room in quad

                //If the cell to get is apart of the main room or boss room, then select another cell
                if (quads.transform.GetChild(roomToGet).transform.parent.name == "Main Room" 
                    || quads.transform.GetChild(roomToGet).transform.parent.name == "Boss Room")
                {
                    roomToGet = -1; //Reset variable so that it fails the check
                    chances -= 1;   //Takes away chances cause it's fed up with this shit
                }

                if (chances == 0) //It's tired of fucking around
                {//                                                                                               |
                    roomsInQuadrants[quadrantToGet] -= 1; //Taking the fucking quadrant out and getting a new one V
                    quadrantToGet = availableQuadrants[GenerationSeed.randomGenerator(0, availableQuadrants.Count)];
                    roomsInQuadrants[quadrantToGet] += 1; //New quadrant gets incremented
                    chances = 0; //Now don't fuck up
                    Debug.Log("Taking too long");
                }
            }

            if (roomsInQuadrants[quadrantToGet] == roomsPerQuadrant) //No more than the specified number of rooms per quadrant
            {
                availableQuadrants.Remove(quadrantToGet); //If the quadrant is filled, remove it from possibility
            }

            //Get the index of the cell chosen and add it to seed and generated rooms
            int cellindex = getCellIndex(quads.transform.GetChild(roomToGet).gameObject);
            seed += GenerationSeed.generateRoomSeed(cellindex);
            generatedRooms.Add(cellindex);
        }
    }

    private bool checkRoomsinQuadrant(int[] Quadrants, int quadNum)
    {
        if (Quadrants[quadNum] >= 3)
            return false;

            return true;
    }

    private void GenerateHallSeed()
    {
        
        GameObject Rooms = GameObject.Find("Rooms");
        hallsToGen = Rooms.transform.childCount;
        //Seed needs to generate at least one hall for each room, skipping the main rooms since its covered already
        for (int i = 2; i < Rooms.transform.childCount; i++ )
        {
            int relation; //gets relational location of two cells
            //get a random hall and a random cell on the hall
            int hall = GenerationSeed.randomGenerator(0, GameObject.Find("Halls").transform.childCount);
            int hallSpot = GenerationSeed.randomGenerator(0, GameObject.Find("Halls").transform.GetChild(hall).transform.childCount);

            //detect the location of the room and hall and their relation to each other
            detectCellLocation(Rooms.transform.GetChild(i).transform.GetChild(0).transform.position,
                getRoomOrHall("Halls").transform.GetChild(hall).transform.GetChild(hallSpot).transform.position,
                out relation);

            //Get cell index of room entrance and hall and add it to the seed
            int entranceIndex = getCellIndex(getMedianWall(getRoomCells(Rooms.transform.GetChild(i).gameObject, "wall"), relation).transform.parent.gameObject);
            int hallIndex = getCellIndex(getRoomOrHall("Hall " + hall).transform.GetChild(hallSpot).gameObject);
            seed += entranceIndex;
            seed += "w";
            seed += hallIndex;
            seed += "w";
        }
    }

    //Returns correct wall to connect
    private GameObject getMedianWall(List<GameObject> roomWalls, int relation)
    {
        GameObject medianWall;

        if (relation == 0 || relation == 1)
            medianWall = getZWalls(roomWalls, relation);

        else
            medianWall = getXWalls(roomWalls, relation);


        return medianWall;
    }

    private GameObject getXWalls(List<GameObject> roomWalls, int xRelation)
    {
        List<GameObject> walls = new List<GameObject>();
        foreach (GameObject wall in roomWalls)
        {
            if (xRelation == 2)
            {
                if (wall.name.Contains("east"))
                {
                    walls.Add(wall);
                }
            }

            else
            {
                if (wall.name.Contains("west"))
                {
                    walls.Add(wall);
                }
            }
        }

        return (walls[walls.Count / 2]);
    }

    private GameObject getZWalls(List<GameObject> roomWalls, int zRelation)
    {
        List<GameObject> walls = new List<GameObject>();

        foreach (GameObject wall in roomWalls)
        {
            if (zRelation == 0)
            {
                if (wall.name.Contains("north"))
                {
                    walls.Add(wall);
                }
            }

            else
            {
                if (wall.name.Contains("south"))
                {
                    walls.Add(wall);
                }
            }
        }
        return walls[walls.Count / 2];
    }

    
#endregion

    #region GenerateDungeon
    private void GenerateDungeon()
    {
        List<int> cellNames = new List<int>(); //Used to store the index name of cells used for rooms in seed
        List<char> cellTypesTemp = new List<char>(); //Used to pull the letter of the room size from the seed
        List<char> cellTypes = new List<char>(); //Used to pull the dimensions from the letter
        List<int> hallCellNames = new List<int>(); //Stores index name of cells used for halls in seed
        int toSkip = roomsToGen.ToString().Length + hallsToGen.ToString().Length;
        int stopped = GenerationSeed.breakDownSeed(seed, out cellNames, out cellTypesTemp, roomsToGen, toSkip); //Breaks down seed for generation

        //Pulls dimensions of rooms from each room size stored in cellTypesTemp
        for (int i = 0; i < cellTypesTemp.Count; i++)
        {
            int e = GenerationSeed.findRoomSize(cellTypesTemp[i]);
            cellTypes.Add(e.ToString().ToCharArray()[0]);
            cellTypes.Add(e.ToString().ToCharArray()[1]);
        }

        createMainRooms(cellNames, cellTypes); //Creates the main room and the boss room
        createMainHall(); //Creates the hall to connect the main rooms

        createRooms(cellNames, cellTypes, roomsToGen); //Generates other rooms
        GenerateHallSeed();
        GenerationSeed.breakDownHallSeed(stopped, seed, out hallCellNames, hallsToGen);
        createHalls(hallCellNames, hallsToGen);

    }

    private void createMainRooms(List<int> cellNames, List<char> cellTypes)
    {
        Vector3 finalRoomPosition; //Needed to place player at the center of main room

        //Generate main room and parent it inside Rooms under Main Room
        parentObject(generateRoom(cells[cellNames[0]].transform.position, 5, 
            5, getQuadrant(cells[cellNames[0]]), out finalRoomPosition), "Rooms", "Main Room");

        //Set player position to main room center
        GameObject.Find("Player").transform.position = finalRoomPosition + new Vector3(0.0f, 0.93f, 0.0f); 

        //Generate boss room and parent it inside Rooms under Boss Room
        parentObject(generateRoom(cells[cellNames[1]].transform.position, 5,
            5, getQuadrant(cells[cellNames[1]]), out finalRoomPosition), "Rooms", "Boss Room");
    }

    //Creates the main hall. Every hall is either connected to this hall or one that connects to this hall
    private void createMainHall()
    {
        int MainRelation; //Needed for detecting cell location
        int BossRelation; //Also needed

        //Scroll to universal functions and find detect cell location for info
        detectCellLocation(getRoomOrHall("Main Room").transform.GetChild(0).transform.position,
                getRoomOrHall("Boss Room").transform.GetChild(0).transform.position,
                out MainRelation);

        //Scroll to universal functions and find detect cell location for info
        detectCellLocation(getRoomOrHall("Boss Room").transform.GetChild(0).transform.position,
                getRoomOrHall("Main Room").transform.GetChild(0).transform.position,
                out BossRelation);

        //Gets the cell index of the center of the rooms
        int MainIndex = getCellIndex(getMedianWall(getRoomCells(getRoomOrHall("Main Room"), "wall"), MainRelation).transform.parent.gameObject);
        int BossIndex = getCellIndex(getMedianWall(getRoomCells(getRoomOrHall("Boss Room"), "wall"), BossRelation).transform.parent.gameObject);

        //Creates the main hall and parents the containing cells to Halls under Hall 0
        parentObject(createHall(cells[MainIndex].transform.position, cells[BossIndex].transform.position, 0), "Halls", "Hall 0");
    }

    private void createRooms(List<int> cellNames, List<char> cellTypes, int roomsToGen)
    {
        int x = 4; //Used to get the correct position in cellTypes (0-3) was used in createMainHall()

        Vector3 needed; //Not actually necessary for anything in this function

        for (int i = 2; i < roomsToGen; i++)
        {
            //Creates rooms generated in the seed and parents them to Rooms and their respective room #
            parentObject(generateRoom(cells[cellNames[i]].transform.position, int.Parse(cellTypes[x].ToString()), 
                int.Parse(cellTypes[x + 1].ToString()), getQuadrant(cells[cellNames[i]]), out needed), "Rooms", "Room " + i);

            x += 2;
        }
    }

    private void createHalls(List<int> hallNames, int hallsToGen)
    {
        for (int i = 0; i < hallNames.Count; i++)
        {
            parentObject(createHall(cells[hallNames[i]].transform.position, cells[hallNames[i + 1]].transform.position, 0), 
                "Halls", "Hall " + (i + 1));
            i++;
        }
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
