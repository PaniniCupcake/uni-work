import uk.ac.warwick.dcs.maze.logic.IRobot;
/*
This script is mostly the same as the Ex1, except there are some changes made,  causing the robot to treat the start pos like a junction, so that it doubles back at the start corner rather than going through it
This script works on loopy mazes, since if a junction is reached that has been accessed before when not backtracking, it just treats is as a wall and backtracks. 
This means the robot essentially treats loopy mazes as primm mazes, just adding virtual walls if sections of the maze links back into an explored section

Choices:
I chose to treat this exercise the same as Ex1,  and just made minor changes since Ex1 works for the most part on loopy mazes

Error checking:
Robot behaving strangely at corner starts. Fixed by treating start corners like junctions when not backtracking. Robot still functioned, but didn't follow the desired behaviour of the robot
*/
public class Ex3
{
	private int pollRun = 0; 
	private RobotData3 robotData3;
	private boolean checked = false;
	private boolean backtracking = false;
	private int startX;
	private int startY;
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
	      startX = robot.getLocation().x;
	      startY = robot.getLocation().y;
	      if ((robot.getRuns() == 0))
	      {
		robotData3 = new RobotData3(); //reset the data store on the first move of the first run of a new maze
	      }
	      robot.setHeading(optionSelect(robot)); //Faces the robot so that it can't be facing away from a junction on the first turn
	    }
	    
	    possibleDirections = passageExits(robot);
	    accessedDirections = beenBeforeExits(robot);
	    thisChecked = checked; //Stores whether the robot moved onto an explored space from last space. If there was nowhere new to go last turn, it will have gone onto a checked tile. 
	    checked = updateChecked(possibleDirections,accessedDirections);//Stores whether every possible direction from this tile has been accessed before
	    
	    if((possibleDirections >= 3 || (robot.getLocation().x == startX && robot.getLocation().y == startY)) && !thisChecked)//Will add the location of this junction only if this is an unchecked junction
	    {
	      /*if(robotData.CheckArrived(robot.getLocation().x,robot.getLocation().y) != 0)
	      {
		 System.out.println("I've been here before");
	      }Error check to see if the robot is adding a new exit for a previously accessed tile*/
	      robotData3.addLocation(robot);
	      robotData3.incrementJcounter();
	    }
	    if ((robot.getLocation().x == startX && robot.getLocation().y == startY))
	    {
		//System.out.println("Special instance");
		possibleDirections++;
	    }
	    
	    //This half of the controlRobot actually instructs the robot where to face
	    
	    if (possibleDirections == 2)
	    {//Always go where you weren't just at at a corridor or corner
		corridor(robot);
	    }
	    else if (possibleDirections == 1 || !backtracking && thisChecked)
	    {//This part is still the same as the TurnBack function was, but increments the junction. The robot treats a corner start tile like an intersection, so also doubles back at it while not backtracking
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
		  robot.setHeading(modifyHeading(robotData3.checkArrived(robot.getLocation().x,robot.getLocation().y)));
	      }
	    }
	    pollRun++; // Increment pollRun so that the data is not reset each time the robot moves
	    //System.out.println("\n On run " + pollRun + " Checked is " + checked);
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
	{//Checks if there are any new exits to take by comparing the available directions with the ones that have been accessed before
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
	  //Makes the robot turn back, and makes backtracking true. It doesn't matter if backtracking is set to true on the start tile, as it will become false when it reaches any junction
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
	  robotData3.resetJunctions();
	}
}
class RobotData3
{
  private int[][] arrived = new int[399][399]; //Each array is stored within the junction's y coordinates' index of the junction's x coodinates' index. Bundles juncX, juncY and arrived, saving space
  private static String[] heading_names = {"North","East","South","West"}; //A list to store the directions the robot can take
  private int junctionCounter = 0;
  public void resetJunctions() 
  {
    int i;
    int j;
    for(i = 0;i<399;i++)
    {
      for(j=0;j<399;j++)
      {
	arrived[i][j] = 0;
      }
    }
    System.out.println("This many unique junctions were visited this run: " + junctionCounter);
    
  }
  public void incrementJcounter()
  {
    junctionCounter ++; //Junction counter will only ever be incremented, never set
  }
  public void addLocation(IRobot robot)
  {
    arrived[robot.getLocation().x-1][robot.getLocation().y-1] = robot.getHeading();
    System.out.println("Junction  (x = " + robot.getLocation().x + ", y = " + robot.getLocation().y + ") Heading " + heading_names[robot.getHeading() - IRobot.NORTH]);
  }
  public int checkArrived(int x_pos,int y_pos)
  {
    return arrived[x_pos-1][y_pos-1];
  }
}