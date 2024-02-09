using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapons_list
{
    public Weapon[,] allyweapons = new Weapon[27,4]; //Broken into chunks of *3*
    public int[,] core_costs = new int[36, 2]; //Broken into chunks of *4*
    public int[] mech_hp = new int[27];
    public int[] mech_moves = new int[27];
    public Weapon[,] enemyweapons = new Weapon[16, 3];
    public int[] max_spawns = {3,3,3,2,2,2,2,2,2,1,1,1,1,1,1,1};
    /*max_pawns = { 
		Scorpion = 3, Firefly = 3, Hornet = 3, Beetle = 2, Scarab = 2, Digger = 1, Blobber = 1, Spider = 1, Leaper = 2, Crab = 2, Centipede = 2, Burrower = 2,
		Snowmine = 1, Snowlaser = 3, Snowtank = 3, Snowart = 2, 
		Jelly_Health = 1, Jelly_Regen = 1, Jelly_Armor = 1, Jelly_Explode = 1, Jelly_Lava = 1,
	},*/
    //Scorpion, Hornet, Firefly, Scarab, Leaper, Burrower, Beetle, Crab, Centipede, Digger, Blobber, Spider
    public int[] enemyScore = { 18, 16, 16, 16, 20, 19, 18, 20, 20, 25, 25, 30, 16, 16, 20, 20 };//Score penalty for leaving enemies alive
    public int[] alphaHpScore = { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 2, 2, 2, 2 };
    public int[] enemyHpScore = { 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 2, 2 };
    public int[] alphaScore = { 22, 22, 20, 20, 23, 22, 22, 24, 24, 29, 29, 30, 16, 16, 20, 20 };
    public Weapon[] deployables = new Weapon[6];
    public int[,] stats = new int[17,4];  //Movement, Health1, Health2, Health3 (if implemented)
    public int[,] spawnstats = new int[5, 3]; //Movement, Health, Allignment
    private Coordinate[] unitCoords = { new Coordinate(1, 0), new Coordinate(0, -1), new Coordinate(-1, 0), new Coordinate(0, 1) };
    private int i = 0;
    private List<TileEffects> target = new List<TileEffects>();
    private List<TileEffects> self = new List<TileEffects>();
    private List<TileEffects> variable = new List<TileEffects>();
    private static Coordinate centre = new Coordinate(0, 0);

    public Weapons_list()
    {
        setupMechs();
        setupVek();
        setUpDeployables();
    }

    private void setupMechs()
    {
        target.Add(new TileEffects(centre, 2, 0));//Titan Fist
        allyweapons[0, 0] = new Weapon("Titan Fist", 1, target, self);
        allyweapons[0, 1] = new Weapon("Titan Fist", 1, target, self,true);
        target.Clear();
        target.Add(new TileEffects(centre, 4, 0));//Titan Fist
        allyweapons[0, 2] = new Weapon("Titan Fist", 1, target, self);
        allyweapons[0, 3] = new Weapon("Titan Fist", 1, target, self,true);
        core_costs[0, 0] = 2;
        core_costs[0, 1] = 3;
        mech_hp[0] = 3;
        mech_moves[0] = 3;

        target.Clear();
        target.Add(new TileEffects(centre, 1, 0));//Taurus Cannon
        allyweapons[1, 0] = new Weapon("Taurus Cannon", 1, target, self);
        target.Clear();
        target.Add(new TileEffects(centre, 2, 0));//Taurus Cannon
        allyweapons[1, 1] = new Weapon("Taurus Cannon", 1, target, self);
        allyweapons[1, 2] = new Weapon("Taurus Cannon", 1, target, self);
        target.Clear();
        target.Add(new TileEffects(centre, 3, 0));//Taurus Cannon
        allyweapons[1, 3] = new Weapon("Taurus Cannon", 1, target, self);
        core_costs[1, 0] = 2;
        core_costs[1, 1] = 3;
        mech_hp[1] = 3;
        mech_moves[1] = 3;

        target.Clear();
        target.Add(new TileEffects(centre, 1));
        for (i = 0; i < 4; i++)
        {
            target.Add(new TileEffects(unitCoords[i], 0, i));
        }
        allyweapons[2, 0] = new Weapon("Artemis Artillery", 1, target, self, 2, 8);
        allyweapons[2, 1] = new Weapon("Artemis Artillery", 1, target, self, 2, 8,2);
        target.Clear();
        target.Add(new TileEffects(centre, 3));
        for (i = 0; i < 4; i++)
        {
            target.Add(new TileEffects(unitCoords[i], 0, i));
        }
        allyweapons[2, 2] = new Weapon("Artemis Artillery", 1, target, self, 2, 8);
        allyweapons[2, 3] = new Weapon("Artemis Artillery", 1, target, self, 2, 8,2);
        core_costs[2, 0] = 1;
        core_costs[2, 1] = 3;
        mech_hp[2] = 2;
        mech_moves[2] = 3;

        core_costs[3, 0] = -1;
        core_costs[3, 1] = -1;
    }
    private void setupVek()
    {
        //Scorpion
        stats[0,0] = 3;
        stats[0, 1] = 3;
        stats[0, 2] = 5;
        target.Clear();
        target.Add(new TileEffects(centre, 1));
        enemyweapons[0, 0] = new Weapon("", 1, target, self);
        target.Clear();
        target.Add(new TileEffects(centre, 3));
        enemyweapons[0, 1] = new Weapon("", 1, target, self);
        target.Clear();
        for (i = 0; i < 4; i++)
        {
            self.Add(new TileEffects(unitCoords[i], 2, i));
        }
        enemyweapons[0, 2] = new Weapon("", 1, target, self,0,0);
        //Hornet
        stats[1, 0] = 5;
        stats[1, 1] = 2;
        stats[1, 2] = 4;
        self.Clear();
        target.Add(new TileEffects(centre, 1));
        enemyweapons[1, 0] = new Weapon("", 1, target, self);
        target.Clear();
        target.Add(new TileEffects(centre, 2));
        target.Add(new TileEffects(unitCoords[0], 2));
        enemyweapons[1, 1] = new Weapon("", 1, target, self);
        target.Clear();
        target.Add(new TileEffects(centre, 2));
        target.Add(new TileEffects(unitCoords[0], 2));
        target.Add(new TileEffects(new Coordinate(2, 0), 2));
        enemyweapons[1, 2] = new Weapon("", 1, target, self);
        //Firefly
        stats[2, 0] = 2;
        stats[2, 1] = 3;
        stats[2, 2] = 5;
        target.Clear();
        target.Add(new TileEffects(centre, 1));
        enemyweapons[2, 0] = new Weapon("", 2, target, self);
        target.Clear();
        target.Add(new TileEffects(centre, 3));
        enemyweapons[2, 1] = new Weapon("", 2, target, self);
        target.Clear();
        target.Add(new TileEffects(centre, 4));
        enemyweapons[2, 2] = new Weapon("", 2, target, self);
        //Scarab
        stats[3, 0] = 3;
        stats[3, 1] = 2;
        stats[3, 2] = 4;
        target.Clear();
        target.Add(new TileEffects(centre, 1));
        enemyweapons[3, 0] = new Weapon("", 1, target, self, 2, 8);
        target.Clear();
        target.Add(new TileEffects(centre, 3));
        enemyweapons[3, 1] = new Weapon("", 1, target, self, 2, 8);
        //Leaper
        stats[4, 0] = 4;
        stats[4, 1] = 1;
        stats[4, 2] = 3;
        enemyweapons[4, 0] = new Weapon("", 1, target, self);
        target.Clear();
        target.Add(new TileEffects(centre, 5));
        enemyweapons[4, 1] = new Weapon("", 1, target, self);
        //Beetle
        stats[6, 0] = 2;
        stats[6, 1] = 4;
        stats[6, 2] = 5;
        target.Clear();
        target.Add(new TileEffects(centre, 1,0));
        enemyweapons[6, 0] = new Weapon("", 2, target, self,true);
        target.Clear();
        target.Add(new TileEffects(centre, 3,0));
        enemyweapons[6, 1] = new Weapon("", 2, target, self, true);
        target.Clear();
        variable.Add(new TileEffects(centre, 0, -1,1));
        variable.Add(new TileEffects(centre, 3, 0));
        enemyweapons[6, 2] = new Weapon("", 2, target, self, 1,1,true,variable);
        //Crab
        stats[7, 0] = 3;
        stats[7, 1] = 3;
        stats[7, 2] = 5;
        target.Clear();
        target.Add(new TileEffects(centre, 1));
        target.Add(new TileEffects(unitCoords[0], 1));
        enemyweapons[7, 0] = new Weapon("", 1, target, self, 2, 8);
        target.Clear();
        target.Add(new TileEffects(centre, 3));
        target.Add(new TileEffects(unitCoords[0], 3));
        enemyweapons[7, 1] = new Weapon("", 1, target, self, 2, 8);
        //Centipede
        stats[8, 0] = 2;
        stats[8, 1] = 3;
        stats[8, 2] = 5;
        target.Clear();
        target.Add(new TileEffects(centre, 1, -1, 2));
        target.Add(new TileEffects(unitCoords[1], 1, -1, 2));
        target.Add(new TileEffects(unitCoords[3], 1, -1, 2));
        enemyweapons[8, 0] = new Weapon("", 2, target, self);
        target.Clear();
        target.Add(new TileEffects(centre, 2, -1, 2));
        target.Add(new TileEffects(unitCoords[1], 2, -1, 2));
        target.Add(new TileEffects(unitCoords[3], 2, -1, 2));
        enemyweapons[8, 1] = new Weapon("", 2, target, self);
        //Digger
        stats[9, 0] = 3;
        stats[9, 1] = 2;
        stats[9, 2] = 4;
        target.Clear();
        for (i = 0; i < 4; i++)
        {
            self.Add(new TileEffects(unitCoords[i], 1));
        }
        enemyweapons[9, 0] = new Weapon("", 1, target, self,0,0);
        self.Clear();
        for (i = 0; i < 4; i++)
        {
            self.Add(new TileEffects(unitCoords[i], 2));
        }
        enemyweapons[9, 1] = new Weapon("", 1, target, self,0,0);
        //Burrower
        stats[5, 0] = 4;
        stats[5, 1] = 3;
        stats[5, 2] = 5;
        self.Clear();
        target.Add(new TileEffects(centre, 1));
        target.Add(new TileEffects(unitCoords[1], 1));
        target.Add(new TileEffects(unitCoords[3], 1));
        enemyweapons[5, 0] = new Weapon("", 1, target, self);
        target.Clear();
        target.Add(new TileEffects(centre, 2));
        target.Add(new TileEffects(unitCoords[1], 2));
        target.Add(new TileEffects(unitCoords[3], 2));
        enemyweapons[5, 1] = new Weapon("", 1, target, self);
        //Blobber
        target.Clear();
        stats[10, 0] = 3;
        stats[10, 1] = 2;
        stats[10, 2] = 4;
        for (i = 0; i < 4; i++)
        {
            target.Add(new TileEffects(unitCoords[i], 0));
        }
        enemyweapons[10, 0] = new Weapon("", 5, target, self, 2, 7);
        target.Clear();
        for (i = 0; i < 4; i++)
        {
            target.Add(new TileEffects(unitCoords[i], 0));
        }
        enemyweapons[10, 1] = new Weapon("", 5, target, self, 2, 7);

        //Spider
        target.Clear();
        stats[11, 0] = 2;
        stats[11, 1] = 2;
        stats[11, 2] = 4;
        for (i = 0; i < 4; i++)
        {
            target.Add(new TileEffects(unitCoords[i], 0));
        }
        enemyweapons[11, 0] = new Weapon("", 5, target, self,2,7);
        enemyweapons[11, 1] = new Weapon("", 5, target, self,2,7);
        //Psion 
        enemyweapons[12, 0] = null;
        stats[12, 0] = 2;
        stats[12, 1] = 2;
        enemyweapons[13, 0] = null;
        stats[13, 0] = 2;
        stats[13, 1] = 2;
        enemyweapons[14, 0] = null;
        stats[14, 0] = 2;
        stats[14, 1] = 2;
        enemyweapons[15, 0] = null;
        stats[15, 0] = 2;
        stats[15, 1] = 2;

        //Ignore bots for time being
        /*
                //Cannon bot
                self.Clear();
                target.Add(new TileEffects(centre, 1,-1,1));
                enemyweapons[11, 0] = new Weapon("", 2, target, self);
                target.Clear();
                target.Add(new TileEffects(centre, 3,-1,1));
                enemyweapons[11, 1] = new Weapon("", 2, target, self);
                //Artillery bot
                target.Clear();
                target.Add(new TileEffects(centre, 1));
                target.Add(new TileEffects(unitCoords[1], 1));
                target.Add(new TileEffects(unitCoords[3], 1));
                enemyweapons[12, 0] = new Weapon("", 1, target, self, 2, 8);
                target.Clear();
                target.Add(new TileEffects(centre, 3));
                target.Add(new TileEffects(unitCoords[1], 3));
                target.Add(new TileEffects(unitCoords[3], 3));
                enemyweapons[12, 1] = new Weapon("", 1, target, self, 2, 8);
                //Laser bot
                target.Clear();
                for (i = 0;i<8;i++)
                {
                    target.Add(new TileEffects(new Coordinate(i, 0), 1));
                }
                enemyweapons[13, 0] = new Weapon("Beam", 1, target, self);
                target.Clear();
                for (i = 0; i < 8; i++)
                {
                    target.Add(new TileEffects(new Coordinate(i, 0), Mathf.Max(1,3-i)));
                }
                enemyweapons[13, 1] = new Weapon("Beam", 1, target, self);
                //Boss bot
                enemyweapons[14, 0] = new Weapon("", 1, target, self, 2, 8);
                enemyweapons[14, 1] = new Weapon("", 1, target, self, 2, 8);
                enemyweapons[14, 2] = new Weapon("", 1, target, self, 2, 8);
        */



    }

    public void setUpDeployables() //Unused
    {
        self.Clear();
        target.Clear();
        //Rock
        spawnstats[2, 0] = 0;
        spawnstats[2, 1] = 1;
        spawnstats[2, 2] = 0;
        deployables[0] = new Weapon("", 1, target, self);
        //Blob
        spawnstats[1, 0] = 0;
        spawnstats[1, 1] = 1;
        spawnstats[1, 2] = -1;
        self.Add(new TileEffects(centre, 0, -1, 6));
        for (i = 0; i < 4; i++)
        {
            self.Add(new TileEffects(unitCoords[i], 1));
        }
        deployables[1] = new Weapon("", 1, target, self,0,0);

        //Alpha blob
        spawnstats[2, 0] = 0;
        spawnstats[2, 1] = 1;
        spawnstats[2, 2] = -1;
        self.Clear();
        self.Add(new TileEffects(centre, 0, -1, 6));
        for (i = 0; i < 4; i++)
        {
            self.Add(new TileEffects(unitCoords[i], 2));
        }
        deployables[2] = new Weapon("Alpha blob", 1, target, self,0,0);

        //Sack
        spawnstats[3, 0] = 0;
        spawnstats[3, 1] = 1;
        spawnstats[3, 2] = -1;
        self.Clear();
        self.Add(new TileEffects(centre, 0, -1, 6,4));
        deployables[3] = new Weapon("Hatch", 1, target, self,0,0);
        //Spiderling
        spawnstats[4, 0] = 0;
        spawnstats[4, 1] = 1;
        spawnstats[4, 2] = -1;
        self.Clear();
        target.Add(new TileEffects(centre, 1));
        deployables[4] = new Weapon("Spider bite", 1, target, self);
    }
}

