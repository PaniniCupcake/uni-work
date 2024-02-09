import uk.ac.warwick.dcs.maze.logic.IRobot;
/*
This script is mostly a recreation of Ex1. The difference occurs in the parts that use RobotData. 
Whenever the robot encounters a new junction, its arrived value is put into the array at the location of the junctionCounter pointer, and the pointer is incremented.
Whenever the robot backtracks at a junction, the robot faces the arrived value of the last value added to the array, and the pointer is decremented so that the robot will not use that value again.
This allows the robot to navigate without x and y storage, since the due to the robot's behaviour it will never backtrack through an explored junction unless it was the most recent one it visited

Choices:
I used an array to store the junctions rather than a stack, since the induvidual values can be more easily accessed for testing, and so I don't have to import it
I chose to retain the backtrack at explored junctions code, as it was specified as necessary in the first code

Bugs:
Error occured when running a maze with different Explorer classes, fixed by changing the names of the RobotData class between scripts
Robot repeatedly facing wall and exhausting the arrived array due to incorrect incrementing of junctionCounter
*/
public class Ex2
{
	private int pollRun = 0; 
	private RobotData2 robotData2;
	private boolean checked = false;
	private boolean backtracking = false;
	private int i;
	private int directions;
	public void controlRobot(IRobot robot) 
	{
	    int possibleDirections = 0;
	    int accessedDirections = 0;
	    boolean thisChecked;
	    
	    //This half of the controlRobot tells the robot what to do on the first run, updates new junctions and updates variables
	    
	    if(pollRun == 0)
	    {
		if ((robot.getRuns() == 0))
		{
		    robotData2 = new RobotData2(); //reset the data store on the first move of the first run of a new maze
		}
		robot.setHeading(modifyHeading(optionSelect(robot))); //On the first run, sets the robot to a random direction so that if the first tile is a corner, it won't always go the same direction
	    }
	      
	    System.out.println(robot.getLocation().x + " X " + robot.getLocation().y + " Y" + (robotData2.returnJcounter()));
	    possibleDirections = passageExits(robot);
	    accessedDirections = beenBeforeExits(robot);
	    thisChecked = checked; //Stores whether the robot moved onto an explored space from last space
	    checked = updateChecked(possibleDirections,accessedDirections);//Stores whether every direction has been accessed before
	      
	    if(possibleDirections >= 3 && !thisChecked)//Will add the location of this junction only if this is an unchecked junction
	    {
		System.out.println("Junction added at coordinates " + robot.getLocation().x + " X " + robot.getLocation().y + " Y with possible directions " + possibleDirections + " backtracking is " + backtracking +" run is " + pollRun + " trying to go in direction " + (robot.getHeading() - IRobot.NORTH));
		robotData2.editArrived(robotData2.returnJcounter(),robot.getHeading());
		robotData2.incrementJcounter(1);
	    }
	    
	    //This half of the controlRobot actually instructs the robot where to face
	    
	    if(possibleDirections == 2)
	    {//Runs the corridor function at a corridor or corner
		corridor(robot);
	    }
	    else if(possibleDirections == 1 || (thisChecked && !backtracking))
	    {//Always double back at a dead end. 
		turnBack(robot);
	    }
	    else
	    {
		if(!checked)
		{
		    robot.setHeading(optionSelect(robot));
		    backtracking = false;
		}
		else
		{
		    robotData2.incrementJcounter(-1);
		    robot.setHeading(modifyHeading(robotData2.checkArrived(robotData2.returnJcounter())));
		}
	    }
	    // On the first move of the first run of a new maze
	    /*if(robot.look(IRobot.AHEAD) == IRobot.WALL)
	    {
		System.out.println("Error at coordinates " + robot.getLocation().x + " X " + robot.getLocation().y + " Y with possible directions " + possibleDirections + " backtracking " + backtracking + " on turn " + pollRun + "trying to go in direction" + (robot.getHeading() - IRobot.NORTH));
	    }*/
	    pollRun++; // Increment pollRun so that the data is not reset each time the robot moves
	}
	
	private int modifyHeading(int heading)
	{//Takes a heading and flips it to its oposite
	  return ((heading - IRobot.NORTH + 2) % 4 + IRobot.NORTH);
	}
	
	public int optionSelect(IRobot robot)
	{
	  int randNo;
	  do
	  {//Keeps looping until the random direction does not point to a wall
	    //Reuses the random direction code from Cw1
	    randNo = (int) Math.round(Math.random() * 4);
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
	  } while(robot.look(IRobot.AHEAD) == IRobot.WALL || robot.look(IRobot.AHEAD) == IRobot.BEENBEFORE);
	  return(robot.getHeading());//Int instead of void for when this value is needed to be modified
	}
	
	public int passageExits(IRobot robot)
	{
	  directions = 0;
	  for(i=IRobot.AHEAD;i<=IRobot.LEFT;i++)//Loops to find the nonwallexits, as I will need to be able to know where to go depending on the block
	    {
	      if(robot.look(i) != IRobot.WALL)
	      {
		directions ++;
	      }
	    }
	    //System.out.println("There are " + directions +" unchecked directions");
	  return directions;
	}
	
	public int beenBeforeExits(IRobot robot)
	{
	  directions = 0;
	  for(i=IRobot.AHEAD;i<=IRobot.LEFT;i++)//Similiar function as nonwallexits to find the beenbeforeexits
	  {
	    if(robot.look(i) == IRobot.BEENBEFORE)
	    {
	      directions ++;
	    }
	  }
	  //System.out.println("There are " + directions +" explored directions");
	  return directions;
	}
	
	public boolean updateChecked(int possibleDirections, int accessedDirections)
	{
	  if(possibleDirections == accessedDirections)
	  {
	    return true;
	  }
	  else
	  {
	    return false;
	  }
	}
	
	public void turnBack(IRobot robot)
	{
	  //Makes the robot turn back.
	    robot.face(IRobot.BEHIND);
	    backtracking = true;
	}
	
	public void corridor(IRobot robot)
	{//The direction to go will either be left, right or forwards by default
	  for(i=IRobot.RIGHT;i<=IRobot.LEFT;i+=2)
	  {
	    if(robot.look(i) != IRobot.WALL)
	    {
		robot.face(i);
	    }
	  }
	}
	
	public void reset() 
	{
	  pollRun = 0;
	  robotData2.resetJunctionCounter();
	}
}
class RobotData2
{
  private static int maxJunctions = 159197; // Max number possible (399*399 - 4 for the corners) This accounts for the worst case scenario, where every space in a 399*399 empty maze is visited
  private static int junctionCounter; // No. of junctions stored
  private int[] arrived= new int[maxJunctions];         // Heading the robot first arrived from
  public void resetJunctionCounter() 
  {
    System.out.println("This many junctions were en route to the finish: " + junctionCounter);
    junctionCounter = 0;
    int i;
    for(i = 0;i < maxJunctions;i++)
    {
      arrived[i] = 0;
    }
  }
  public void incrementJcounter(int amount)
  {
    junctionCounter += amount;
  }
  public int returnJcounter()
  {
    return junctionCounter;
  }
  public void editArrived(int i, int value)
  {
    arrived[i] = value;
  }
  public int checkArrived(int i)
  {
    return arrived[i];
  }
}