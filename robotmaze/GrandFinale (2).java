import uk.ac.warwick.dcs.maze.logic.IRobot;

/*
This robot reuses and adds to the code from Ex3.
In the RobotData class, the arrived list is replaced by the junctionAccesses variable. It stores the arrived values, as well as how many times it has been entered and or left from each direction.
The robot increments the times a tile is accessed from a direction every time it is entered or left from that direction.
If a tile is incremented to 2, that direction is treated like a wall from then onwards.
Every time the previous script would have checked for a wall, this script also checks if the direction has been incremented enough to be treated like a wall.
When the maze resets, the tile storage is not flushed. Instead, tile directions that were visited on this runs but not blocked off are reset so that their routes can be taken again. 
Other directions have their values set to 2 so that they will never be taken in subsequent runs

Choices:
I decided to use Tremaux's algorithm because it was a more logical step from my Ex3 (derived itself from Ex1) than the alternate, which is derived from Ex2
Although option B seemed like it was a better idea for memory conservation, it also has less potential. It can only remove deadends and closed loops from its possible routes, and would take a lot of modification to do anything else.
This algorithm, can be modified into finding a good route from the route it originally take by simply removing the line that blocks off unexplored junctions.
In addition, the code barely needs to behave differently on the second run

Debugging:
Changed CheckAccessible function to check both the current tile and the tile the robot is heading to in case one is not a corridor

Discovered and fixed error where start tile was incrementing the wrong direction. Narrowed down error using a print statement in incrementJunctions function

Located unknown error that caused robot to behave erratically
Narrowed down by discovering that the error occured in parts of loopy mazes where there is a ring around a single tile
Made a 7*7 maze comprised of paths around 9 wall blocks, and drew it on paper as well
Annotated paper map, eventually discovering that the robot was blocking off walls behind it to create a corridor, and then invariably going right.
Fixed by treating recently changed tiles like what they just were.

Modified when junctions are incremented, removing need for check both the current and next tile in CheckAccessible
*/

public class GrandFinale
{
    private int pollRun = 0;     // Incremented after each passprivate Returns all parameters input with the script
    private RobotData4 robotData4; // Data store for junctions
    private boolean checked = false;
    private boolean backtracking = false;
    private int startX;
    private int startY;//Stores the original X and Y positions of the robot so it can check if it is where it started
    private int i;
    private int directions;
    public void controlRobot(IRobot robot)
    {
        int possibleDirections = 0;
        int accessedDirections = 0;
        boolean thisChecked;
        int xPos = robot.getLocation().x;
        int yPos = robot.getLocation().y;//The script would call these functions a lot, so I decided to store their value in variables to call them once instead.
        System.out.println("\n Run" + pollRun + " Coordinates: " + +xPos + " X " + yPos + " Y");
        
        //This half of the controlRobot tells the robot what to do on the first run, updates new junctions and updates variables
        
        if (pollRun == 0)
        {
	    startX = xPos;
	    startY = yPos;
            if ((robot.getRuns() == 0))
            {
                robotData4 = new RobotData4(); //reset the data store on the first move of the first run of a new maze
            }
            robot.setHeading(optionSelect(robot, xPos, yPos)); //Faces the robot so that it can't be facing away from a junction on the first turn
        }//Might have to store start x and y pos
	else
	{
	    robotData4.incrementJunctionAccess(xPos, yPos, (robot.getHeading() - IRobot.NORTH + 2) % 4 + 1);//Mark the tile as having been crossed from the direction the robot came from, when not the first tile
	}
	
        possibleDirections = passageExits(robot, xPos, yPos);
        accessedDirections = beenBeforeExits(robot, xPos, yPos);
        thisChecked = checked;
        checked = updateChecked(possibleDirections, accessedDirections);

        /*if (possibleDirections == 0)
        {            
	    System.out.println("Nowhere to go");
        } Checked to see if the robot is trapping itself somehow */
        
        if (robotData4.returnJunctionData(xPos, yPos, 0) == 0 && possibleDirections >= 3)//Corridors and deadends don't need to be incremented as they are just passageways connecting intersections that were incremented
        {
            robotData4.changeJunctionArrived(xPos, yPos, robot.getHeading());
            robotData4.incrementJcounter();
        }

        if (!checkAccessible(modifyHeading(robot.getHeading(), 2), xPos, yPos) || (robot.getLocation().x == startX && robot.getLocation().y == startY))
        {
	    //System.out.println("Special instance");
            possibleDirections++;
        }
        /*
        The robot will sometimes enter a corridor that has just been a t section or a dead end that was just a corridor. 
        This will cause the robot to behave unusually, making the robot double back into blocked off sections and other unusual behavior
        The previous if statement makes the robot treat the current tile like what it just was for this run, so that it can reorientate itself.
        */
        
        System.out.println(possibleDirections + " Possible");
        System.out.println(accessedDirections + " Accessed");
        
        //This half of the controlRobot actually instructs the robot where to face
        
        if (possibleDirections == 2)
        {//Always go where you weren't just at at a corridor or corner
            corridor(robot, xPos, yPos);
        }
        else if (possibleDirections == 1 || !backtracking && thisChecked)
        {//This part is still the same as the TurnBack function was, but increments the junction. The robot treats a corner start tile like an intersection, so also doubles back at it while not backtracking
            turnBack(robot);
        }
        else
        {
            if (!checked)
            {
                backtracking = false;
                robot.setHeading(optionSelect(robot, xPos, yPos));
            }
            else
            {//Head back towards previous junctions 
                //System.out.println(robotData4.ReturnJunctionData(xPos, yPos, 0));
                robot.setHeading(modifyHeading(robotData4.returnJunctionData(xPos, yPos, 0), 2));
                //System.out.println(robotData4.ReturnJunctionData(xPos, yPos, 0));
            }
	}
	
        pollRun++; // Increment pollRun so that the data is not reset each time the robot moves, as well as for testing
	robotData4.incrementJunctionAccess(xPos, yPos, robot.getHeading() - IRobot.NORTH + 1);//Always increment the junction you are leaving.
	/*if(robot.look(IRobot.AHEAD) == IRobot.WALL)
	{
	    System.out.println("Facing wall at " + xPos + " X " + yPos + " Y");
	}*/
    }
    
    private int modifyHeading(int heading, int modifier)
    {//Takes a heading and rotates it 90 degrees times the modifier
	return ((heading - IRobot.NORTH + modifier) % 4 + IRobot.NORTH);
    }

    public int optionSelect(IRobot robot, int xPos, int yPos)
    {
	int randNo;
	do
	{//Keeps looping until the random direction does not point to a wall
	    randNo = (int)Math.round(Math.random() * 4);
	    switch (randNo)
	    {
		case(1):
		    robot.setHeading(IRobot.NORTH);
		    break;
		case(2):
		    robot.setHeading(IRobot.EAST);
		    break;
		case(3):
		    robot.setHeading(IRobot.SOUTH);
		    break;
		default:
		    robot.setHeading(IRobot.WEST);
	    }
	} while (robot.look(IRobot.AHEAD) == IRobot.WALL || robot.look(IRobot.AHEAD) == IRobot.BEENBEFORE || !checkAccessible(robot.getHeading(), xPos, yPos));
	
	return (robot.getHeading()); //Int instead of void for when this value is needed to be modified
    }

    public int passageExits(IRobot robot, int xPos, int yPos)
    {
	directions = 0;
	for (i = IRobot.AHEAD; i <= IRobot.LEFT; i++)//Reusing this from cw1ex1 to find the nonwallexits, as I will need to be able to know where to go depending on the block
	{
	    if (robot.look(i) != IRobot.WALL && checkAccessible(modifyHeading(robot.getHeading(), i - IRobot.AHEAD), xPos, yPos))
	    {
		directions++;
	    }
	}
	return directions;
    }

    public int beenBeforeExits(IRobot robot, int xPos, int yPos)
    {
	directions = 0;
	for (i = IRobot.AHEAD; i <= IRobot.LEFT; i++)//Reusing this from cw1ex1 to find the beenbeforeexits, as I will need to be able to know where to go depending on the block
	{
	    if (robot.look(i) != IRobot.WALL && robot.look(i) == IRobot.BEENBEFORE && checkAccessible(modifyHeading(robot.getHeading(), i - IRobot.AHEAD), xPos, yPos))
	    {
		directions++;
	    }
	}
	return directions;
    }

    public boolean updateChecked(int possibleDirections, int accessedDirections)
    {//Checks if there are any new exits to take by comparing the available directions with the ones that have been accessed before
	if (possibleDirections == accessedDirections)
	{
	    return true;
	}
	else
	{
	    return false;
	}
    }

    public boolean checkAccessible(int robotHeading, int xPos, int yPos)
    {
	if (robotData4.returnJunctionData(xPos, yPos, robotHeading - IRobot.NORTH + 1) == 2) //Checks whether the current tile has that exit point maxed
	{
	    return false;
	}
	return true;
    }

    public void turnBack(IRobot robot)
    {
	//Makes the robot turn back.
	robot.face(IRobot.BEHIND);
	backtracking = true;
    }

    public void corridor(IRobot robot, int xPos, int yPos)
    {//Always go where you weren't just at at a corridor or corner
	for (i = IRobot.RIGHT; i <= IRobot.LEFT; i+= 2)
	{
	    if (robot.look(i) != IRobot.WALL && checkAccessible(modifyHeading(robot.getHeading(), i - IRobot.AHEAD), xPos, yPos))
	    {
		robot.face(i);
		break;
	    }
	}
    }

    public void reset()
    {
	robotData4.assignAccesses();
	pollRun = 0;
    }

}
class RobotData4
{
    private static int junctionCounter = 0; // No. of junctions stored
    private int[][][] junctionAccesses = new int[399][399][5];
    //Similarly to the Explorer arrived list, this 3d list stores a list containing the first access (formerly the arrived int) and total accesses from each direction (NESW) within the yth index of the xth index 
    
    public void assignAccesses()
    {
        int i;
        int j;
        int k;
        System.out.println("\n\nI encountered this many junctions " + (junctionCounter));
        junctionCounter = 0;
        int allowed = 0;
        for (i = 0; i < 399; i++)
        {
            for (j = 0; j < 399; j++)
            {
                junctionAccesses[i][j][0] = 0;
                for (k = 1; k < 5; k++)
                {
                    if (junctionAccesses[i][j][k] == 1) 
                    {//Sets all tiles on the path that weren't blocked off to be accessible again for the next run
                        junctionAccesses[i][j][k] = 0;
                        allowed ++;
                    }
                    else
                    {//Sets tiles that weren't explored this run to inaccesible so no new dead ends or loops will be explored
			junctionAccesses[i][j][k] = 2;
                    }
                }
            }
        }
        System.out.println("This many tiles are available " + (allowed/2 + 2)); //An accessible tile that is not the start or finish will have two access points    
    }
    
    public int returnJcounter()
    {
        return junctionCounter;
    }
    public void incrementJcounter()
    {
        junctionCounter ++; //Junction counter will only ever be incremented, never set
    }
    public int returnJunctionData(int xPos, int yPos, int direction)
    {
        return junctionAccesses[xPos - 1][yPos - 1][direction];
    }
    public void incrementJunctionAccess(int xPos, int yPos, int direction)
    {
        junctionAccesses[xPos - 1][yPos - 1][direction]++; //The visit direction totals will also only ever need to be decremented
    }
    public void changeJunctionArrived(int xPos, int yPos, int value)
    {
        junctionAccesses[xPos - 1][yPos - 1][0] = value;
    }
}