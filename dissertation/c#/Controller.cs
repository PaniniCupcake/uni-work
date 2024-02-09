using System.Collections;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
public class Controller : MonoBehaviour
{//This class stores attributes and methods universal to any iteration of the current board
    public bool slow;
    public List<Cur_tiles> tile_grids;
    public int cur_grid = 0;
    public Weapons_list weapons = new Weapons_list();
    private int count = 0;

    private int cur_squad = 0; //Rift walkers, rusting hulks, zenith guard, blitzkreig, steel judoka, flame behemoths, frozen titans, hazardous mechs, ss
    private List<int> weapon_upgrades = new List<int>(); //Mech 1,2,3 and passive
    private List<int> hp_upgrades = new List<int>();
    private List<int> move_upgrades = new List<int>();
    private int cores;
    private float favour;

    //Comms
    public int latest_action; //Debugging
    private int receive_ver = 1; //Only read if value matches
    private int send_ver = 0; //Write this value
    public ReadMem read_mem;
    public bool random_actions; //If don't want to comm with AI
    public bool optimal_actions;
    //func
    private int[,] legal_tiles = new int[8, 8]; //-1 for impassable, 0 for passable, 1 for movable
    private int[,] tile_dist = new int[8, 8];
    private int[,] legal_move_tiles1 = new int[8, 8];
    private int[,] legal_move_tiles2 = new int[8, 8];
    private int[,] legal_move_tiles3 = new int[8, 8];
    private bool[,] legal_attack_tiles1 = new bool[8, 8];
    private bool[,] legal_attack_tiles2 = new bool[8, 8];
    private bool[,] legal_attack_tiles3 = new bool[8, 8];

    //Objectives
    private int cur_mission;
    private int mission_score;

    //Oher
    public int resist_chance = 15;
    public int cur_turn = 1;
    private int island_number;
    private List<int> island_spawns = new List<int>();
    private List<int> strong_spawns = new List<int>();
    public int turn_num;
    private bool turn_ended;
    private int score;
    private int score_store;//Outdated
    private int score_store2;//Outdated
    private int score_store_true;//Outdated
    private string layout_string;
    private int[,] tile_threat = new int[8, 8];

    private List<int> weak_remaining = new List<int>();
    private List<int> strong_remaining = new List<int>();
    private List<int> weak_prob = new List<int>();
    private List<int> alpha_prob = new List<int>();
    private int max_alpha = 0;
    private List<Coordinate> spawn_tiles = new List<Coordinate>();
    private List<Vector2Int> spawn_vek = new List<Vector2Int>();//Not actually a coordinate, just using it as a tuple (Vek variant, alpha)

    public int vek_bonus;
    public int stage_hazard = -1; //-6 for none, -1 for instakill, 1 for freeze, 2 for water, 3 for chasm, 4 for lava, -2 for boulders. - 4 for tentacles, -3 for fireballs


    public bool enemy_turn;
    public int psion_type;//0 for none, 1 for hp, 2 for regen, 3 for explode, 4 for armour, 5 for tyrant
    private List<int> psions_remaining = new List<int>();

    public Queue<Coordinate> explode_queue = new Queue<Coordinate>(); // For explosive psion
    public bool initial_tiles = true;
    public GameObject dummy;
    private int best_evaluation;

    //Constants
    public Coordinate[] unitCoords = { new Coordinate(1, 0), new Coordinate(0, -1), new Coordinate(-1, 0), new Coordinate(0, 1) };
    private bool[,] moveturnOrder = new bool[5, 6];
    private int[,] turnOrder = new int[6, 3];
    private int[,] moveOrder = new int[6, 3];


    //Update vars
    List<bool> pod_chance = new List<bool>();
    List<bool> diff_chance = new List<bool>();

    //Passives
    public int smokedamage;
    public int bumpdamage;
    public bool flame_shielding;
    public bool nanomachines;//son

    //Sprite lists
    public Sprite empty_sprite;
    public Sprite power_grid;
    public Sprite rock;
    public List<Sprite> mountain = new List<Sprite>();
    public List<Sprite> tiles = new List<Sprite>();
    public List<Sprite> health = new List<Sprite>();
    public List<Sprite> movement = new List<Sprite>();
    public List<Sprite> friendlysprites = new List<Sprite>();
    public List<Sprite> enemysprites = new List<Sprite>();
    public List<Sprite> status = new List<Sprite>();
    public List<Sprite> targetx = new List<Sprite>();
    public List<Sprite> targety = new List<Sprite>();
    public List<Sprite> objective = new List<Sprite>();
    public List<Sprite> numbers = new List<Sprite>();

    //--------------------------------------------------------------------
    //Start func
    //--------------------------------------------------------------------

    public Logger log = new Logger();

    void Start()
    {
        //print(weapons.deployables[3].selfEffects[0].status);
        int i = 0;
        int j = 0;
        for (i = 0; i < 3; i++)
        {
            weapon_upgrades.Add(0);
            hp_upgrades.Add(0);
            move_upgrades.Add(0);
            getCurTileGrid().actions_left[i] = true;
        }
        weapon_upgrades.Add(0);

        string s;


        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                tile_threat[i, j] = 0; 
            }
        }
        for (i = 0; i < 8; i++)
        {
            for (j = 1; j < 9; j++)
            {
                char c = (char)(97 + i);
                s = j + "" + c;
                tile_grids[cur_grid].tile_grid[i, j - 1] = GameObject.Find(s).GetComponent<Tile>();
                tile_grids[cur_grid].tile_grid[i, j - 1].location.setX(i);
                tile_grids[cur_grid].tile_grid[i, j - 1].location.setY(j - 1);
            }
        }
        for (i = 0; i < 8; i++)
        {
            for (j = 1; j < 9; j++)
            {
                for (int k = 2; k < 5; k++)
                {
                    char c = (char)(97 + i);
                    s = j + "" + c + "" + k;
                    tile_grids[k - 1].tile_grid[i, j - 1] = GameObject.Find(s).GetComponent<Tile>();
                    tile_grids[k - 1].tile_grid[i, j - 1].location.setX(i);
                    tile_grids[k - 1].tile_grid[i, j - 1].location.setY(j - 1);
                }
            }
        }
        //For adding existing units in testing
        //int units = 0;
        /*int enemies = 0;
        bool incremented = true;
        while (incremented)
        {
            incremented = false;
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    if (getTile(i, j).occupant.allignment == -1 && getTile(i, j).occupant.number == enemies)
                    {
                        incremented = true;
                        getAttackOrder().Add(getTile(i, j).occupant);
                        enemies++;
                    }
                    /*else if (getTile(i, j).occupant.allignment == 1 && getTile(i, j).occupant.number == units)
                    {
                        incremented = true;
                        tile_grids[cur_grid].mechs.Add(getTile(i, j).occupant);
                        units++;
                    }
                }
            }
        }*/
        //
        initial_tiles = false;
        updateSprites();
        //print("Ken Tucky");
        psions_remaining.Clear();
        for (i = 12; i < 16; i++)
        {
            psions_remaining.Add(i);
        }
        strong_spawns.Clear();
        for (i = 5; i < 12; i++)
        {
            strong_spawns.Add(i);
        }
        DetermineIslandSpawn();
        layout_string = writeToFileAlt();
        //Reduced training loop
        island_number = 1;
        pod_chance = new List<bool>();

        if (slow ) 
        {
            return;
        }
        for (i = 0; i < 10000; i++)
        {
            if (pod_chance.Count == 0)
            {
                if (psions_remaining.Count == 0)
                {
                    for (j = 12; j < 16; j++)
                    {
                        psions_remaining.Add(j);
                    }
                }
                DetermineIslandSpawn();
                psion_type = island_spawns[0] - 11;
                pod_chance.Add(false);
                pod_chance.Add(false);
                pod_chance.Add(false);
                pod_chance.Add(false);
                pod_chance.Add(true);
            }

            bool temp = pod_chance[UnityEngine.Random.Range(0, pod_chance.Count)];
            pod_chance.Remove(temp);
            cores = 0;
            Mission(false, temp);
        }
        string str = "";
        str += "turns_survived" + "\n";
        for (i = 0; i < 6; i++)
        {
            str += i + ":" + log.turns_survived[i] + "\n";
        }
        str += "power rem" + "\n";
        for (i = 0; i < 8; i++)
        {
            str += i + ":" +log.power_remaining[i] + "\n";
        }
        str += "Actions" + "\n";
        for (i = 0; i < 255; i++)
        {
            //print(i + ":" + log.action_counter[i]);
        }
        str += "pod" + "\n";
        str += log.pods_recovered + "\n";
        str += "Missi" + "\n";
        str += log.objectives_succeeded + "\n";
        str += "Kill" + "\n";
        str += log.enemies_killed + "\n";
        StreamWriter sw = new StreamWriter("./assets/smol.txt");
        sw.Write(str);
        sw.Close();
        
        updateSpritesFinal();
    }




    //--------------------------------------------------------------------
    //Functions referencing the current tiles
    //--------------------------------------------------------------------

    private List<TileOccupant> getAttackOrder()
    {
        return tile_grids[cur_grid].attack_order;
    }
    public Tile getTile(Coordinate coord)
    {
        return tile_grids[cur_grid].tile_grid[coord.getX(), coord.getY()];
    }
    public Tile getTile(int x, int y)
    {
        return tile_grids[cur_grid].tile_grid[x, y];
    }
    public Cur_tiles getCurTileGrid()
    {
        return tile_grids[cur_grid];
    }


    //--------------------------------------------------------------------
    //Functions for gameplay
    //--------------------------------------------------------------------

    public void Game()
    {
        int i;
        cores = 0;
        favour = 0;
        resist_chance = 15;
        getCurTileGrid().grid_health = 5;
        psions_remaining.Clear();
        for (i = 12; i < 16; i++)
        {
            psions_remaining.Add(i);
        }
        strong_spawns.Clear();
        for (i = 5; i < 12; i++)
        {
            strong_spawns.Add(i);
        }

        island_number = 1;
        DetermineIslandSpawn();
        //print("Island 1");
        getCurTileGrid().perfect_bonus = true;
        for (i = 0; i < island_spawns.Count; i++)
        {
            //print(island_spawns[i]);
        }
        psion_type = island_spawns[0] - 11;
        pod_chance = new List<bool>();
        diff_chance = new List<bool>();
        pod_chance.Add(false);
        pod_chance.Add(false);
        pod_chance.Add(false);
        pod_chance.Add(true);
        bool temp = pod_chance[UnityEngine.Random.Range(0, 4)];
        Mission(false, temp);
        pod_chance.Remove(temp);
        temp = pod_chance[UnityEngine.Random.Range(0, 3)];
        Mission(false, temp);
        pod_chance.Remove(temp);
        temp = pod_chance[UnityEngine.Random.Range(0, 2)];
        Mission(false, temp);
        pod_chance.Remove(temp);
        temp = pod_chance[0];
        Mission(false, temp);
        pod_chance.Remove(temp);
        Mission(false, false);

        if (getCurTileGrid().perfect_bonus)
        {
            favour++;
        }
        cores += (int)favour / 3;
        favour %= 3;
        favour = favour / 2;


        island_number = 2;
        DetermineIslandSpawn();
        //print("Island 2");
        getCurTileGrid().perfect_bonus = true;
        for (i = 0; i < island_spawns.Count; i++)
        {
            //print(island_spawns[i]);
        }
        psion_type = island_spawns[0] - 11;
        pod_chance.Add(false);
        pod_chance.Add(false);
        pod_chance.Add(false);
        pod_chance.Add(true);
        diff_chance.Add(false);
        diff_chance.Add(false);
        diff_chance.Add(false);
        diff_chance.Add(true);
        temp = pod_chance[UnityEngine.Random.Range(0, 4)];
        bool temp2 = diff_chance[UnityEngine.Random.Range(0, 4)];
        Mission(temp2, temp);
        pod_chance.Remove(temp);
        diff_chance.Remove(temp2);
        temp = pod_chance[UnityEngine.Random.Range(0, 3)];
        temp2 = diff_chance[UnityEngine.Random.Range(0, 3)];
        Mission(temp2, temp);
        diff_chance.Remove(temp2);
        pod_chance.Remove(temp);
        temp = pod_chance[UnityEngine.Random.Range(0, 2)];
        temp2 = diff_chance[UnityEngine.Random.Range(0, 2)];
        Mission(temp2, temp);
        pod_chance.Remove(temp);
        diff_chance.Remove(temp2);
        temp = pod_chance[0];
        temp2 = diff_chance[0];
        Mission(temp2, temp);
        pod_chance.Remove(temp);
        diff_chance.Remove(temp2);
        Mission(false, false);

        if (getCurTileGrid().perfect_bonus)
        {
            favour++;
        }
        cores += (int)favour / 3;
        favour %= 3;
        favour = favour / 2;

        island_number = 3;
        DetermineIslandSpawn();
        //rint("Island 3");
        getCurTileGrid().perfect_bonus = true;
        for (i = 0; i < island_spawns.Count; i++)
        {
            //print(island_spawns[i]);
        }
        psion_type = island_spawns[0] - 11;
        pod_chance.Add(false);
        pod_chance.Add(false);
        pod_chance.Add(true);
        pod_chance.Add(true);
        diff_chance.Add(false);
        diff_chance.Add(false);
        diff_chance.Add(false);
        diff_chance.Add(true);
        temp = pod_chance[UnityEngine.Random.Range(0, 4)];
        temp2 = diff_chance[UnityEngine.Random.Range(0, 4)];
        Mission(temp2, temp);
        pod_chance.Remove(temp);
        diff_chance.Remove(temp2);
        temp = pod_chance[UnityEngine.Random.Range(0, 3)];
        temp2 = diff_chance[UnityEngine.Random.Range(0, 3)];
        Mission(temp2, temp);
        pod_chance.Remove(temp);
        diff_chance.Remove(temp2);
        temp = pod_chance[UnityEngine.Random.Range(0, 2)];
        temp2 = diff_chance[UnityEngine.Random.Range(0, 2)];
        Mission(temp2, temp);
        pod_chance.Remove(temp);
        diff_chance.Remove(temp2);
        temp = pod_chance[0];
        temp2 = diff_chance[0];
        Mission(temp2, temp);
        pod_chance.Remove(temp);
        diff_chance.Remove(temp2);
        Mission(false, false);

        if (getCurTileGrid().perfect_bonus)
        {
            favour++;
        }
        cores += (int)favour / 3;
        favour %= 3;
        favour = favour / 2;

        island_number = 4;
        DetermineIslandSpawn();
        //print("Island 4");
        getCurTileGrid().perfect_bonus = true;
        for (i = 0; i < island_spawns.Count; i++)
        {
            //print(island_spawns[i]);
        }
        psion_type = island_spawns[0] - 11;
        pod_chance.Add(false);
        pod_chance.Add(false);
        pod_chance.Add(true);
        pod_chance.Add(true);
        diff_chance.Add(false);
        diff_chance.Add(false);
        diff_chance.Add(false);
        diff_chance.Add(true);
        temp = pod_chance[UnityEngine.Random.Range(0, 4)];
        temp2 = diff_chance[UnityEngine.Random.Range(0, 4)];
        Mission(temp2, temp);
        pod_chance.Remove(temp);
        diff_chance.Remove(temp2);
        temp = pod_chance[UnityEngine.Random.Range(0, 3)];
        temp2 = diff_chance[UnityEngine.Random.Range(0, 3)];
        Mission(temp2, temp);
        pod_chance.Remove(temp);
        diff_chance.Remove(temp2);
        temp = pod_chance[UnityEngine.Random.Range(0, 2)];
        temp2 = diff_chance[UnityEngine.Random.Range(0, 2)];
        Mission(temp2, temp);
        pod_chance.Remove(temp);
        diff_chance.Remove(temp2);
        temp = pod_chance[0];
        temp2 = diff_chance[0];
        Mission(temp2, temp);
        diff_chance.Remove(temp2);
        pod_chance.Remove(temp);
        Mission(false, false);

        if (getCurTileGrid().perfect_bonus)
        {
            favour++;
        }
        cores += (int)favour / 3;
        favour %= 3;
        favour = favour / 2;

        print("Game complete!" + count);
    }

    public void Mission(bool difficult, bool pod)
    {
        getCurTileGrid().Refresh();
        getCurTileGrid().grid_health = 5;
        AssignCores();
        FlushLevel();
        //print("Flushed level");
        GenerateMap();
        //print("Map generated");
        updateSprites();
        PopulateVekLimits();
        //Turn 5
        cur_turn = 1;
        Tile t = getRandomGridTile();
        t.occupant.health = 1;
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            t.occupant.objective_type = 1;
        }
        else
        {
            t.occupant.objective_type = 2;
        }
        getCurTileGrid().objective1 = t;

        if (difficult)
        {
            t = getRandomGridTile();
            if (t != null && t.occupant.objective_type != 0)//Just act like a hard objective was skipped if it selects the same objective
            {
                getCurTileGrid().objective2 = t;
                t.occupant.health = 1;
                SpawnAlpha();
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    t.occupant.objective_type = 1;
                }
                else
                {
                    t.occupant.objective_type = 3;
                }
                getCurTileGrid().objective1 = t;
            }
        }
        else
        {
            getCurTileGrid().objective2 = null;
        }
        cur_mission = UnityEngine.Random.Range(0, 4);
        if (island_number > 2)
        {
            QueueSpawns(3);
        }
        else
        {
            QueueSpawns(2);
        }
        if (pod)
        {
            //print("Spawning pod");
            SpawnPod();
        }
        else
        {
            //print("No pods");
        }
        SpawnsEmerge();
        updateSprites();
        //
        //Get initial mission score and layout
        getCurTileGrid().prev_score = scorePosition();
        //score_store = getCurTileGrid().prev_score;
        //score_store2 = score_store;
        //score_store_true = 0;
        //writeLegalActions();
        //Player spawn
        playerTurn();
        EnemyTurn();
        if (true)//Reduced turn count
        {
            QueueSpawns(1);
            //Player turn 1
            cur_turn = 2;
            RefreshActions();
            playerTurn();
            EnemyTurn();

            if (getCurTileGrid().grid_health <= 0)
            {
                MissionFailed();
                return;
            }
        }
        QueueSpawns(2);
        //Player turn 2
        cur_turn = 3;
        RefreshActions();
        playerTurn();
        //updateSprites();

        EnemyTurn();
        if (getCurTileGrid().grid_health <= 0)
        {
            MissionFailed();
            return;
        }
        if (true)
        {
            QueueSpawns(1);
            //Player turn 3
            cur_turn = 4;
            RefreshActions();
            playerTurn();
            EnemyTurn();
            if (getCurTileGrid().grid_health <= 0)
            {
                MissionFailed();
                return;
            }
        }
        //Player turn 4
        cur_turn = 5;
        RefreshActions();
        for (int i = 0; i < spawn_tiles.Count; i++)
        {
            getTile(spawn_tiles[i]).spawn = false;
        }
        spawn_tiles.Clear();
        spawn_vek.Clear();
        playerTurn();
        EnemyTurn();
        if (getCurTileGrid().grid_health <= 0)
        {
            MissionFailed();
            return;
        }
        updateSprites();
        //score = getCurTileGrid().prev_score - score_store2;
        //score = score_store_true;
        score = getActionScore();
        sendHandshake(1);
        ///
        log.mission_over(cur_turn, getCurTileGrid().grid_health);
        if (getCurTileGrid().pod_destroyed == 1)
        {
            log.pods_recovered++;
            cores++;
            favour += 0.5f;
        }

        if (getCurTileGrid().objective1.occupied && getCurTileGrid().objective1.occupant.allignment == 2)
        {
            log.objectives_succeeded++;
            if (getCurTileGrid().objective1.occupant.objective_type == 1 && getCurTileGrid().grid_health < 7)
            {
                getCurTileGrid().grid_health++;
            }
            else if (getCurTileGrid().grid_health == 7)
            {
                if (resist_chance < 25)
                {
                    resist_chance++;
                }
                if (resist_chance < 40)
                {
                    resist_chance++;
                }
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 2)
            {
                favour++;
            }
        }
        if (!(getCurTileGrid().objective2 == null || (getCurTileGrid().objective1.occupied && getCurTileGrid().objective1.occupant.allignment == 2)))
        {
            log.objectives_succeeded++;
            if (getCurTileGrid().objective1.occupant.objective_type == 1 && getCurTileGrid().grid_health < 7)
            {
                getCurTileGrid().grid_health++;
            }
            else if (getCurTileGrid().grid_health == 7)
            {
                if (resist_chance < 25)
                {
                    resist_chance++;
                }
                if (resist_chance < 40)
                {
                    resist_chance++;
                }
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 2)
            {
                favour++;
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 3)
            {
                cores++;
            }
        }
        int hp_loss = 0;
        for (int i = 0; i < 3; i++)
        {
            TileOccupant mech = getCurTileGrid().mechs[i];
            hp_loss += mech.max_health - mech.health;
        }
        int[] mission_counters = getCurTileGrid().mission_counters;
        if (cur_mission == 0)//Vek kills (7), blocked spawns(3), grid damage(3), mech damage(4)
        {
            if (mission_counters[0] >= 7)
            {
                favour++;
            }
        }
        else if (cur_mission == 1)
        {
            if (mission_counters[1] >= 3)
            {
                favour++;
            }
        }
        else if (cur_mission == 2)
        {
            if (mission_counters[2] < 3)
            {
                favour++;
            }
        }
        else if (cur_mission == 3)
        {
            if (hp_loss >= 4)
            {
                favour++;
            }
        }
    }

    public void MissionFailed()
    {
        log.mission_over(cur_turn, 0);
        print("Failed");
        getCurTileGrid().prev_score = scorePosition();
        score = -10000;
        //layout_string = writeToFileAlt();
        sendHandshake(1);
    }

    //First part of mission, only for update version
    public void MissionSetup(bool difficult, bool pod)
    {
        getCurTileGrid().Refresh();
        if (getCurTileGrid().grid_health <= 0)
        {
            getCurTileGrid().grid_health = 5;
        }
        AssignCores();
        FlushLevel();
        //print("Flushed level");
        GenerateMap();
        //print("Map generated");
        updateSprites();
        PopulateVekLimits();
        //Turn 5
        cur_turn = 1;
        Tile t = getRandomGridTile();
        t.occupant.health = 1;
        if (UnityEngine.Random.Range(0, 2) == 0)
        {
            t.occupant.objective_type = 1;
        }
        else
        {
            t.occupant.objective_type = 2;
        }
        getCurTileGrid().objective1 = t;

        if (difficult)
        {
            t = getRandomGridTile();
            if (t != null && t.occupant.objective_type != 0)//Just act like a hard objective was skipped if it selects the same objective
            {
                getCurTileGrid().objective2 = t;
                t.occupant.health = 1;
                SpawnAlpha();
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    t.occupant.objective_type = 1;
                }
                else
                {
                    t.occupant.objective_type = 3;
                }
                getCurTileGrid().objective1 = t;
            }
        }
        else
        {
            getCurTileGrid().objective2 = null;
        }
        cur_mission = UnityEngine.Random.Range(0, 4);
        if (island_number > 2)
        {
            QueueSpawns(3);
        }
        else
        {
            QueueSpawns(2);
        }
        if (pod)
        {
            //print("Spawning pod");
            SpawnPod();
        }
        else
        {
            //print("No pods");
        }
        SpawnsEmerge();
        updateSprites();
        //
        //Get initial mission score and layout
        //getCurTileGrid().prev_score = scorePosition();
        //layout_string = writeToFileAlt();
    }

    void Update() //Game but not all at once
    {
        /*int i;
        if (count == 2000)
        {
            string str = "";
            str += "turns_survived" + "\n";
            for (i = 0; i < 6; i++)
            {
                str += i + ":" + log.turns_survived[i] + "\n";
            }
            str += "power rem" + "\n";
            for (i = 0; i < 8; i++)
            {
                str += i + ":" + log.power_remaining[i] + "\n";
            }
            str += "Actions" + "\n";
            for (i = 0; i < 255; i++)
            {
                //print(i + ":" + log.action_counter[i]);
            }
            str += "pod" + "\n";
            str += log.pods_recovered + "\n";
            str += "Missi" + "\n";
            str += log.objectives_succeeded + "\n";
            str += "Kill" + "\n";
            str += log.enemies_killed + "\n";
            str += "Poss" + "\n";
            str += log.poss_actions + "\n";
            StreamWriter sw = new StreamWriter("./assets/smol.txt");
            sw.Write(str);
            sw.Close();
            count = 2001;
        }
        print(count);
        if(count == 2001)
        {
            return;
        }
        if (psions_remaining.Count == 0)
        {
            for (i = 12; i < 16; i++)
            {
                psions_remaining.Add(i);
            }
        }
        DetermineIslandSpawn();
        psion_type = island_spawns[0] - 11;
        pod_chance.Add(false);
        pod_chance.Add(false);
        pod_chance.Add(false);
        pod_chance.Add(false);
        pod_chance.Add(true);
        bool temp = pod_chance[UnityEngine.Random.Range(0, pod_chance.Count)];
        pod_chance.Remove(temp);
        cores = 0;
        Mission(false, temp);
        temp = pod_chance[UnityEngine.Random.Range(0, pod_chance.Count)];
        pod_chance.Remove(temp);
        Mission(false, temp);
        temp = pod_chance[UnityEngine.Random.Range(0, pod_chance.Count)];
        pod_chance.Remove(temp);
        Mission(false, temp);
        temp = pod_chance[UnityEngine.Random.Range(0, pod_chance.Count)];
        pod_chance.Remove(temp);
        Mission(false, temp);
        temp = pod_chance[UnityEngine.Random.Range(0, pod_chance.Count)];
        pod_chance.Remove(temp);
        Mission(false, temp);

        print(count);
        count++;
        */
        if (!slow || count > 1900)
        {
            return;
        }

        bool temp = false;
        bool temp2 = false;
        int i = 0;
        if (count == 0)
        {
            cores = 0;
            favour = 0;
            getCurTileGrid().grid_health = 5;
            psions_remaining.Clear();
            for (i = 12; i < 16; i++)
            {
                psions_remaining.Add(i);
            }
            strong_spawns.Clear();
            for (i = 5; i < 12; i++)
            {
                strong_spawns.Add(i);
            }

            island_number = 1;
            DetermineIslandSpawn();
            //print("Island 1");
            getCurTileGrid().perfect_bonus = true;
            for (i = 0; i < island_spawns.Count; i++)
            {
                //print(island_spawns[i]);
            }
            psion_type = island_spawns[0] - 11;
            pod_chance = new List<bool>();
            diff_chance = new List<bool>();
            pod_chance.Add(false);
            pod_chance.Add(false);
            pod_chance.Add(false);
            pod_chance.Add(true);
            temp = pod_chance[UnityEngine.Random.Range(0, 4)];
            pod_chance.Remove(temp);
            MissionSetup(false, temp);
        }
        if (count % 100 == 10)
        {
            //Player spawn
            cur_turn = 1;
            RefreshActions();
            playerTurn();
            //turn_ended = false;
            /*if (random_actions)
            {

            }
            else
            {
                while (true)
                {
                    string s = read_mem.readMem();
                    //print(s);
                    char[] chars = s.ToCharArray();
                    if (chars.Length > 0 && (chars[0] - '0') == 2)
                    {
                        Application.Quit();
                        print(getCurTileGrid().mechs[100]);//Error to stop code
                        break;
                    }
                    if (chars.Length > 0 && (chars[0] - '0') == receive_ver)
                    {
                        sendHandshake(0);
                        break;
                    }
                }
            }*/
        }/*
        if(count % 100 > 10 && count % 100 < 15)
        { 
            while (true)
            {
                string s = read_mem.readMem();
                //print(s);
                char[] chars = s.ToCharArray();
                if (chars.Length > 0 && (chars[0] - '0') == 2)
                {
                    Application.Quit();
                    print(getCurTileGrid().mechs[100]);//Error to stop code
                    break;
                }
                if (chars.Length > 1 && (chars[0] - '0') == receive_ver)
                {
                    int ac = 0;
                    for (i = 1; i < s.Length; i++)
                    {
                            ac = ac * 10 + (s[i] - '0');
                    }
                    if (Action(ac))
                    {
                        //print("Succ acc!");
                        //print(ac);
                        score = getActionScore();
                        layout_string = writeToFileAlt();
                        if(ac == 244)
                        {
                            count = 20;
                        }
                        break;
                    }
                    else
                    {
                        //print("Fail acc");
                        score = -200;
                    }
                    //print("The goodness!");
                }
            }
            sendHandshake(0);
            updateSprites();
        }*/
        if (count % 100 == 20)
        {
            EnemyActions();
            QueueSpawns(1);
            //RefreshActions();//Remove this after test
            /*cur_turn = 2;
            turn_ended = false;
            while (true)
            {
                string s = read_mem.readMem();
                //print(s);
                char[] chars = s.ToCharArray();
                if (chars.Length > 0 && (chars[0] - '0') == 2)
                {
                    Application.Quit();
                    print(getCurTileGrid().mechs[100]);//Error to stop code
                    break;
                }
                if (chars.Length > 0 && (chars[0] - '0') == receive_ver)
                {
                    sendHandshake(0);
                    break;
                }
            }*/
        }
        /*if(count > 20 && count < 27)
        {
                while (true)
                {
                    string s = read_mem.readMem();
                    //print(s);
                    char[] chars = s.ToCharArray();
                    if (chars.Length > 0 && (chars[0] - '0') == 2)
                    {
                        Application.Quit();
                        print(getCurTileGrid().mechs[100]);//Error to stop code
                        break;
                    }
                    if (chars.Length > 1 && (chars[0] - '0') == receive_ver)
                    {
                        int ac = 0;
                        for (i = 1; i < s.Length; i++)
                        {
                            ac = ac * 10 + (s[i] - '0');
                        }
                        if (Action(ac))
                        {
                            //print("Succ acc!");
                            //print(ac);
                            score = getActionScore();
                            layout_string = writeToFileAlt();
                            if (ac == 244)
                            {
                                count = 40;
                            }
                            break;
                        }
                        else
                        {
                            //print("Fail acc");
                            score = -200;
                        }
                        //print("The goodness!");
                    }
                }
                sendHandshake(0);
                updateSprites();
        }*/
        if (count % 100 == 30)
        {
            //Player turn 1
            cur_turn = 2;
            RefreshActions();
            playerTurn();
        }
        if (count % 100 == 40)
        {
            EnemyTurn();
            QueueSpawns(2);
        }
        if (count % 100 == 50)
        {
            //Player turn 2
            cur_turn = 3;
            RefreshActions();
            playerTurn();
            updateSprites();
        }
        if (count % 100 == 60)
        {
            EnemyTurn();
            QueueSpawns(1);
        }
        if (count % 100 == 70)
        {
            //Player turn 3
            cur_turn = 4;
            RefreshActions();
            playerTurn();
        }
        if (count % 100 == 80)
        {
            EnemyTurn();
            //Player turn 4
            cur_turn = 5;
            RefreshActions();
            for (i = 0; i < spawn_tiles.Count; i++)
            {
                getTile(spawn_tiles[i]).spawn = false;
            }
            spawn_tiles.Clear();
            spawn_vek.Clear();
            playerTurn();
        }
        if (count % 100 == 90)
        {
            EnemyTurn();
            updateSprites();
            ///
            if (getCurTileGrid().pod_destroyed == 1)
            {
                cores++;
                favour += 0.5f;
            }

            if (getCurTileGrid().objective1.occupied && getCurTileGrid().objective1.occupant.allignment == 2)
            {
                if (getCurTileGrid().objective1.occupant.objective_type == 1 && getCurTileGrid().grid_health < 7)
                {
                    getCurTileGrid().grid_health++;
                }
                else if (getCurTileGrid().grid_health == 7)
                {
                    if (resist_chance < 25)
                    {
                        resist_chance++;
                    }
                    if (resist_chance < 40)
                    {
                        resist_chance++;
                    }
                }
                else if (getCurTileGrid().objective1.occupant.objective_type == 2)
                {
                    favour++;
                }
            }
            if (!(getCurTileGrid().objective2 == null || (getCurTileGrid().objective1.occupied && getCurTileGrid().objective1.occupant.allignment == 2)))
            {
                if (getCurTileGrid().objective1.occupant.objective_type == 1 && getCurTileGrid().grid_health < 7)
                {
                    getCurTileGrid().grid_health++;
                }
                else if (getCurTileGrid().grid_health == 7)
                {
                    if (resist_chance < 25)
                    {
                        resist_chance++;
                    }
                    if (resist_chance < 40)
                    {
                        resist_chance++;
                    }
                }
                else if (getCurTileGrid().objective1.occupant.objective_type == 2)
                {
                    favour++;
                }
                else if (getCurTileGrid().objective1.occupant.objective_type == 3)
                {
                    cores++;
                }
            }
            int hp_loss = 0;
            for (i = 0; i < 3; i++)
            {
                TileOccupant mech = getCurTileGrid().mechs[i];
                hp_loss += mech.max_health - mech.health;
            }
            int[] mission_counters = getCurTileGrid().mission_counters;
            if (cur_mission == 0)//Vek kills (7), blocked spawns(3), grid damage(3), mech damage(4)
            {
                if (mission_counters[0] >= 7)
                {
                    favour++;
                }
            }
            else if (cur_mission == 1)
            {
                if (mission_counters[1] >= 3)
                {
                    favour++;
                }
            }
            else if (cur_mission == 2)
            {
                if (mission_counters[2] < 3)
                {
                    favour++;
                }
            }
            else if (cur_mission == 3)
            {
                if (hp_loss >= 4)
                {
                    favour++;
                }
            }
        }
        if (count == 100)
        {
            temp = pod_chance[UnityEngine.Random.Range(0, 3)];
            MissionSetup(false, temp);
            pod_chance.Remove(temp);
        }
        if (count == 200)
        {
            temp = pod_chance[UnityEngine.Random.Range(0, 2)];
            pod_chance.Remove(temp);
            MissionSetup(false, temp);
        }
        if (count == 300)
        {
            temp = pod_chance[0];
            pod_chance.Remove(temp);
            MissionSetup(false, temp);

        }
        if (count == 400)
        {
            Mission(false, false);

            if (getCurTileGrid().perfect_bonus)
            {
                favour++;
            }
            cores += (int)favour / 3;
            favour %= 3;
            favour = favour / 2;


            island_number = 2;
            DetermineIslandSpawn();
            //print("Island 2");
            getCurTileGrid().perfect_bonus = true;
            for (i = 0; i < island_spawns.Count; i++)
            {
                //print(island_spawns[i]);
            }
            psion_type = island_spawns[0] - 11;
            pod_chance.Add(false);
            pod_chance.Add(false);
            pod_chance.Add(false);
            pod_chance.Add(true);
            diff_chance.Add(false);
            diff_chance.Add(false);
            diff_chance.Add(false);
            diff_chance.Add(true);
            temp = pod_chance[UnityEngine.Random.Range(0, 4)];
            temp2 = diff_chance[UnityEngine.Random.Range(0, 4)];
            pod_chance.Remove(temp);
            MissionSetup(temp2, temp);
            diff_chance.Remove(temp2);
        }
        if (count == 500)
        {
            temp = pod_chance[UnityEngine.Random.Range(0, 3)];
            temp2 = diff_chance[UnityEngine.Random.Range(0, 3)];
            MissionSetup(temp2, temp);
            pod_chance.Remove(temp);
            diff_chance.Remove(temp2);
        }
        if (count == 600)
        {
            temp = pod_chance[UnityEngine.Random.Range(0, 2)];
            temp2 = diff_chance[UnityEngine.Random.Range(0, 2)];
            MissionSetup(temp2, temp);
            pod_chance.Remove(temp);
            diff_chance.Remove(temp2);
        }
        if (count == 700)
        {
            temp = pod_chance[0];
            temp2 = diff_chance[0];
            MissionSetup(temp2, temp);
            pod_chance.Remove(temp);
            diff_chance.Remove(temp2);
        }
        if (count == 800)
        {
            MissionSetup(false, false);
        }
        if (count == 900)
        {
            if (getCurTileGrid().perfect_bonus)
            {
                favour++;
            }
            cores += (int)favour / 3;
            favour %= 3;
            favour = favour / 2;

            island_number = 3;
            DetermineIslandSpawn();
            //rint("Island 3");
            getCurTileGrid().perfect_bonus = true;
            for (i = 0; i < island_spawns.Count; i++)
            {
                //print(island_spawns[i]);
            }
            psion_type = island_spawns[0] - 11;
            pod_chance.Add(false);
            pod_chance.Add(false);
            pod_chance.Add(true);
            pod_chance.Add(true);
            diff_chance.Add(false);
            diff_chance.Add(false);
            diff_chance.Add(false);
            diff_chance.Add(true);
            temp = pod_chance[UnityEngine.Random.Range(0, 4)];
            temp2 = diff_chance[UnityEngine.Random.Range(0, 4)];
            MissionSetup(temp2, temp);
            pod_chance.Remove(temp);
            diff_chance.Remove(temp2);
        }
        if (count == 1000)
        {
            temp = pod_chance[UnityEngine.Random.Range(0, 3)];
            temp2 = diff_chance[UnityEngine.Random.Range(0, 3)];
            MissionSetup(temp2, temp);
            pod_chance.Remove(temp);
            diff_chance.Remove(temp2);
        }
        if (count == 1100)
        {
            temp = pod_chance[UnityEngine.Random.Range(0, 2)];
            temp2 = diff_chance[UnityEngine.Random.Range(0, 2)];
            MissionSetup(temp2, temp);
            pod_chance.Remove(temp);
            diff_chance.Remove(temp2);
        }
        if (count == 1200)
        {
            temp = pod_chance[0];
            temp2 = diff_chance[0];
            MissionSetup(temp2, temp);
            pod_chance.Remove(temp);
            diff_chance.Remove(temp2);
        }
        if (count == 1300)
        {
            MissionSetup(false, false);
        }
        if (count == 1400)
        {

            if (getCurTileGrid().perfect_bonus)
            {
                favour++;
            }
            cores += (int)favour / 3;
            favour %= 3;
            favour = favour / 2;

            island_number = 4;
            DetermineIslandSpawn();
            //print("Island 4");
            getCurTileGrid().perfect_bonus = true;
            for (i = 0; i < island_spawns.Count; i++)
            {
                //print(island_spawns[i]);
            }
            psion_type = island_spawns[0] - 11;
            pod_chance.Add(false);
            pod_chance.Add(false);
            pod_chance.Add(true);
            pod_chance.Add(true);
            diff_chance.Add(false);
            diff_chance.Add(false);
            diff_chance.Add(false);
            diff_chance.Add(true);
            temp = pod_chance[UnityEngine.Random.Range(0, 4)];
            temp2 = diff_chance[UnityEngine.Random.Range(0, 4)];
            MissionSetup(temp2, temp);
            pod_chance.Remove(temp);
            diff_chance.Remove(temp2);
        }
        if (count == 1500)
        {
            temp = pod_chance[UnityEngine.Random.Range(0, 3)];
            temp2 = diff_chance[UnityEngine.Random.Range(0, 3)];
            MissionSetup(temp2, temp);
            pod_chance.Remove(temp);
            diff_chance.Remove(temp2);
        }
        if (count == 1600)
        {
            temp = pod_chance[UnityEngine.Random.Range(0, 2)];
            temp2 = diff_chance[UnityEngine.Random.Range(0, 2)];
            MissionSetup(temp2, temp);
            pod_chance.Remove(temp);
            diff_chance.Remove(temp2);
        }
        if (count == 1700)
        {
            temp = pod_chance[0];
            temp2 = diff_chance[0];
            MissionSetup(temp2, temp);
            pod_chance.Remove(temp);
            diff_chance.Remove(temp2);
        }
        if (count == 1800)
        {
            MissionSetup(false, false);

            if (getCurTileGrid().perfect_bonus)
            {
                favour++;
            }
            cores += (int)favour / 3;
            favour %= 3;
            favour = favour / 2;

            print("Game complete!");
        }
        count += 10;
    }

    //--------------------------------------------------------------------
    //Functions for spawning extra start of mission things
    //--------------------------------------------------------------------

    private void SpawnAlpha()
    {
        List<Tile> spawnableTiles = GetSpawnableTiles();
        Coordinate temp = spawnableTiles[UnityEngine.Random.Range(0, spawnableTiles.Count)].location;

        if (weak_prob.Count == 0)
        {
            GenerateSpawnSelection();
        }
        int temp2 = weak_prob[UnityEngine.Random.Range(0, weak_prob.Count)];
        int temp4 = 0;
        if (temp2 == 1)
        {
            temp4 = strong_remaining[UnityEngine.Random.Range(0, strong_remaining.Count)];
            strong_remaining.Remove(temp4);
        }
        if (temp2 == 0)
        {
            temp4 = weak_remaining[UnityEngine.Random.Range(0, weak_remaining.Count)];
            weak_remaining.Remove(temp4);
        }
        SpawnUnit(temp, temp4, 1);
    }

    private void SpawnPod()
    {
        int i, j;
        getCurTileGrid().pod_destroyed = 1;
        List<Tile> spawnableTiles = new List<Tile>();
        for (i = 1; i < 7; i++)
        {
            for (j = 2; j < 4; j++)
            {
                if (CheckSpawnable(getTile(i, j)))
                {
                    spawnableTiles.Add(getTile(i, j));
                }
            }
        }
        //print(spawnableTiles.Count);
        if (spawnableTiles.Count == 0) //Give extra 2 tiles leeway
        {
            //print("Pod Leeway required");
            for (i = 1; i <= 7; i++)
            {
                if (CheckSpawnable(getTile(i, 1)))
                {
                    spawnableTiles.Add(getTile(i, 1));
                }
            }
        }
        if (spawnableTiles.Count == 0) //Fine you can try everywhere that isn't friendly spawn
        {
            for (i = 1; i < 7; i++)
            {
                for (j = 2; j < 4; j++)
                {
                    if (CheckSpawnable(getTile(i, j)) && !(j >= 4 && j < 7 && i >= 1 && i < 7))
                    {
                        spawnableTiles.Add(getTile(i, j));
                    }
                }
            }
        }
        spawnableTiles[UnityEngine.Random.Range(0, spawnableTiles.Count)].addPod();
    }

    //--------------------------------------------------------------------
    //Functions for generating new random level
    //--------------------------------------------------------------------

    private void FlushLevel()
    {
        getAttackOrder().Clear();
        getCurTileGrid().mechs.Clear();

        int i = 0;
        int j = 0;
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                tile_grids[cur_grid].tile_grid[i, j].Clear();
            }
        }
    }
    private List<string> maps_left = new List<string>();
    private void GenerateMap()
    {
        string s = "";
        /*int rand = UnityEngine.Random.Range(0, 5);//Ignore sand snow acid for reduced set
        switch (rand)
        {
            case 0:
                s = "any";
                rand = UnityEngine.Random.Range(0, 50);
                break;
            case 1:
                s = "grass";
                rand = UnityEngine.Random.Range(0, 10);
                break;
            case 2:
                s = "sand";
                rand = UnityEngine.Random.Range(0, 15);
                break;
            case 3:
                s = "snow";
                rand = UnityEngine.Random.Range(0, 25);
                break;
            default:
                s = "acid";
                rand = UnityEngine.Random.Range(0, 15);
                break;
        }*/
        //Reduced
        int rand;
        s = "any";
        rand = UnityEngine.Random.Range(0, 50);

        s += "" + rand;

        print("Map no: " + s);
        FileStream fs = new FileStream("./assets/maps/" + s + ".map", FileMode.Open, FileAccess.Read);
        StreamReader sr = new StreamReader(fs);
        sr.BaseStream.Seek(0, SeekOrigin.Begin);
        s = sr.ReadLine();
        s = sr.ReadLine();
        readMapString(s, 11);
        s = sr.ReadLine();
        while (s != null && s.Length > 5)
        {
            readMapString(s, 0);
            s = sr.ReadLine();
        }
        sr.Close();
    }
    private void readMapString(string s, int offset)
    {
        //print(s);
        if (s.Length < 41 + offset)
        {
            return;
        }
        char[] charArr = s.ToCharArray();
        int y = charArr[18 + offset] - '0';
        y = 7 - y;
        int x = charArr[21 + offset] - '0';
        x = 7 - x;
        int terrain = charArr[40 + offset] - '0';
        switch (terrain)//0 and 1 for nothing, 3 for water, 4 for mountain, 5 ice,6 for forest 7 for desert, 9 for hole
        {
            case 3:
                getTile(x, y).liquid = true;
                break;
            case 4:
                getTile(x, y).occupied = true;
                getTile(x, y).occupant.Override(0, null, 0, 0, 2, 0, 0, 0, false);
                break;
            case 5:
                getTile(x, y).frozen = 2;
                break;
            case 6:
                getTile(x, y).forest = true;
                break;
            case 7:
                getTile(x, y).desert = true;
                break;
            case 9:
                getTile(x, y).chasm = true;
                break;
            default:
                break;
        }
        if (s.Length > 50 + offset)
        {
            if (charArr[47 + offset] == 'p')
            {
                int hp = UnityEngine.Random.Range(0, 9);
                if (hp < 5)
                {
                    hp = 1;
                }
                else
                {
                    if (island_number >= 3 && hp == 9)
                    {
                        hp = 4;
                    }
                    else
                    {
                        hp = 2;
                    }
                }
                getTile(x, y).occupant.Override(0, null, 0, 0, hp, 2, 0, 0, false);
                getTile(x, y).occupied = true;
                //print("Tile " + x + "," + y + "Has a grid");
            }
            else
            {
                getTile(x, y).acid = true;
            }
        }
    }


    //--------------------------------------------------------------------
    //Functions for visualisation
    //--------------------------------------------------------------------


    public void updateSprites()
    {
        if (slow)
        {
            if (tile_grids[cur_grid].grid_health >= 0)
            {
                GetComponent<SpriteRenderer>().sprite = numbers[tile_grids[cur_grid].grid_health];
            }

            int i = 0;
            int j = 0;
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    tile_grids[cur_grid].tile_grid[i, j].updateSprites();
                }
            }
            tile_grids[cur_grid].hiddenBurrowers[0].updateSprites();
            tile_grids[cur_grid].hiddenBurrowers[1].updateSprites();
        }
    }
    public void updateSpritesFinal()
    {
        if (tile_grids[cur_grid].grid_health >= 0)
        {
            GetComponent<SpriteRenderer>().sprite = numbers[tile_grids[cur_grid].grid_health];
        }

        int i = 0;
        int j = 0;
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                tile_grids[cur_grid].tile_grid[i, j].updateSprites();
            }
        }
        tile_grids[cur_grid].hiddenBurrowers[0].updateSprites();
        tile_grids[cur_grid].hiddenBurrowers[1].updateSprites();
    }

    //--------------------------------------------------------------------
    //Functions involving spawns
    //--------------------------------------------------------------------


    public void SpawnUnit(Coordinate location, int type, int alpha) //alpha = -1 for deployable, 2 for boss (alpha values of 1 and 0 only used)
    {
        //print("W");
        int i = getAttackOrder().Count;

        if (location.outOfBounds())
        {
            return;
        }
        if (alpha == -1 && type != 0)
        {
            //print("Wuh");
            if (getTile(location).occupied)
            {
                return;
            }
            //print("Spawning1");
            //print(type);
            //print(weapons.spawnstats[type, 1]);
            //print(type);
            getTile(location).occupant.Override(type + 28, weapons.deployables[type], i, weapons.spawnstats[type, 0], weapons.spawnstats[type, 1], -1, 0, -1, type != 4);
            if (type == 3)
            {
                getTile(location).occupant.webber_type = 2;
            }
            //  print("--------------------------------");
            // print("Spawned enemy on " + location.getX() + "," + location.getY());
            // print("--------------------------------");
        }
        else if (alpha == -1)
        {
            if (getTile(location).occupied)
            {
                return;
            }
            getTile(location).occupant.Override(type, null, 0, 0, 1, 0, 0, 0, false);
            // print("--------------------------------");
            // print("Spawned rock on " + location.getX() + "," + location.getY());
            // print("--------------------------------");
        }
        else
        {
            getTile(location).occupant.Override(type, weapons.enemyweapons[type, alpha], i, weapons.stats[type, 0], weapons.stats[type, 1 + alpha], -1, 0, alpha, false);
            if (type == 0 || type == 4)
            {
                getTile(location).occupant.webber_type = 1;
                if (alpha == 2)
                {
                    getTile(location).occupant.webber_type = 2;
                }
            }
            if (getCurTileGrid().psion_active == 4 && type < 12)
            {
                getTile(location).occupant.armoured = true;
            }
            else if (getCurTileGrid().psion_active == 1 && type < 12)
            {
                getTile(location).occupant.max_health++;
                getTile(location).occupant.health++;
            }
        }
        //print("Truer words have never been spoken");
        getTile(location).occupied = true;
        getTile(location).applyTileEffects();
    }

    public void QueueUnit(TileOccupant unit) //Adda new enemy spawn to the enemy list
    {
        getAttackOrder().Add(unit);
        for (int i = 0; i < getAttackOrder().Count; i++)
        {
            getAttackOrder()[i].number = i;
        }
    }

    public void DetermineIslandSpawn()
    {
        int i;
        int c;
        int chosen;
        List<int> helper_list = new List<int>();
        island_spawns.Clear();
        chosen = psions_remaining[UnityEngine.Random.Range(0, psions_remaining.Count)];
        psions_remaining.Remove(chosen);
        island_spawns.Add(chosen);
        c = 0;

        helper_list.Add(0);
        helper_list.Add(1);
        helper_list.Add(2);
        if (island_number != 4 || !(island_spawns[0] == 14 && strong_spawns.Contains(10) && strong_spawns.Contains(11) && strong_spawns.Contains(5) && strong_spawns.Contains(7)))
        {
            helper_list.Add(3);
            //print("Scarab!");
        }
        helper_list.Add(4);

        while (c < 3)
        {
            chosen = helper_list[UnityEngine.Random.Range(0, helper_list.Count)];
            helper_list.Remove(chosen);
            if (chosen == 0)
            {
                helper_list.Remove(4);
            }
            else if (chosen == 4)
            {
                helper_list.Remove(0);
            }
            island_spawns.Add(chosen);
            c++;
        }
        if (island_number > 1)
        {
            List<int> denied_list = new List<int>();
            if (island_spawns.Contains(3))
            {
                denied_list.Add(7);
                strong_spawns.Remove(7);
            }
            if (island_spawns.Contains(14))
            {
                denied_list.Add(5);
                strong_spawns.Remove(5);
            }
            try
            {
                chosen = strong_spawns[UnityEngine.Random.Range(0, strong_spawns.Count)];
                strong_spawns.Remove(chosen);
            }
            catch
            {
                chosen = 6;
            }
            island_spawns.Add(chosen);
            if (island_number > 2)
            {
                if (island_spawns.Contains(10))
                {
                    denied_list.Add(11);
                    strong_spawns.Remove(11);
                }
                else if (island_spawns.Contains(11))
                {
                    denied_list.Add(10);
                    strong_spawns.Remove(10);
                }
                try
                {
                    chosen = strong_spawns[UnityEngine.Random.Range(0, strong_spawns.Count)];
                    strong_spawns.Remove(chosen);
                }
                catch
                {
                    chosen = 8;
                }
                island_spawns.Add(chosen);
            }
            for (i = 0; i < denied_list.Count; i++)
            {
                strong_spawns.Add(denied_list[i]);
            }
        }
        /*/print("Stuff");
        for(i=0;i<island_spawns.Count;i++)
        {
            print(island_spawns[i]);
        }
        print("O");
        for (i = 0; i < helper_list.Count; i++)
        {
            print(helper_list[i]);
        }
        print("Da");
        for (i = 0; i < strong_spawns.Count; i++)
        {
            print(strong_spawns[i]);
        }*/
        c = 0;

    }

    private bool CheckSpawnable(Tile tile)
    { //Accounts for edge case where a mech dies in a chasm. vv
        return !(tile.occupied || tile.occupant.allignment == 1 || tile.fire || tile.chasm || tile.liquid || tile.frozen > 0 || tile.spawn || tile.pod);
    }

    private List<Tile> GetSpawnableTiles()
    {
        int i, j;
        List<Tile> spawnableTiles = new List<Tile>();
        for (i = 2; i < 6; i++)
        {
            for (j = 0; j < 3; j++)
            {
                if (CheckSpawnable(getTile(i, j)))
                {
                    spawnableTiles.Add(getTile(i, j));
                }
            }
        }

        //print(spawnableTiles.Count);
        if (spawnableTiles.Count == 0) //Give extra 2 tiles leeway
        {
            //print("Leeway required");
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 3; j++)
                {
                    if (CheckSpawnable(getTile(i, j)))
                    {
                        spawnableTiles.Add(getTile(i, j));
                    }
                }
            }
        }
        if (spawnableTiles.Count == 0) //Fine you can try the next row too
        {
            //print("Double leeway required");
            for (i = 0; i < 8; i++)
            {
                if (CheckSpawnable(getTile(i, 3)))
                {
                    spawnableTiles.Add(getTile(i, 3));
                }
            }
        }
        if (spawnableTiles.Count == 0) //Give up they win
        {
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    if (CheckSpawnable(getTile(i, j)))
                    {
                        spawnableTiles.Add(getTile(i, j));
                    }
                }
            }
        }
        return spawnableTiles;
    }

    public void QueueSpawns(int spawns)
    {
        print(spawns);
        int i, j;
        List<Tile> spawnableTiles = GetSpawnableTiles();
        int major_vek = 0;
        int total_vek = 0;
        for (i = 0; i < getAttackOrder().Count; i++)
        {
            if (getAttackOrder()[i].frozen)
            {
                total_vek += 2;
                if (!(getAttackOrder()[i].auto_attack || (getAttackOrder()[i].max_health == 1 && getAttackOrder()[i].movement == 3)))
                {
                    major_vek += 2;
                }
            }
            else
            {
                total_vek += 3;
                if (!(getAttackOrder()[i].auto_attack || (getAttackOrder()[i].max_health == 1 && getAttackOrder()[i].movement == 3)))
                {
                    major_vek += 3;
                }
            }
        }
        major_vek += spawn_tiles.Count;
        total_vek += spawn_tiles.Count;
        while (total_vek % 3 != 0)
        {
            total_vek--;
        }
        if (major_vek <= 6 && total_vek <= 9 && spawns < 4)
        {//Disabled for easy mode
            //spawns = Mathf.Min(spawns + 1, 3);
        }
        
        spawns = Mathf.Min(spawns, (18 - total_vek) / 3);
        if (spawns < 0)
        {
            spawns = 0;
        }
        //print(spawns);
        for (i = 0; i < spawns; i++)
        {
            Coordinate temp = spawnableTiles[UnityEngine.Random.Range(0, spawnableTiles.Count)].location;
            spawnableTiles.Remove(getTile(temp));
            if (weak_prob.Count == 0)
            {
                GenerateSpawnSelection();
            }
            int temp2 = weak_prob[UnityEngine.Random.Range(0, weak_prob.Count)];
            //print(temp2);
            int temp3 = alpha_prob[UnityEngine.Random.Range(0, alpha_prob.Count)];
            //print(temp3);
            weak_prob.Remove(temp2);
            alpha_prob.Remove(temp3);
            if (temp3 == 1)
            {
                int counter = 0;
                for (j = 0; j < getAttackOrder().Count; j++)
                {
                    if (getAttackOrder()[j].alpha == 1)
                    {
                        counter++;
                    }
                }
                if (counter >= max_alpha)
                {
                    temp3 = 0;
                }
            }
            int temp4 = 0;
            if (temp2 == 1)
            {
                if (strong_remaining.Count == 0)
                {
                    temp2 = 0;
                }
                else
                {
                    temp4 = strong_remaining[UnityEngine.Random.Range(0, strong_remaining.Count)];
                    strong_remaining.Remove(temp4);
                }
            }
            if (temp2 == 0)
            {
                if (weak_remaining.Count == 0)
                {
                    temp4 = island_spawns[UnityEngine.Random.Range(1, 4)];
                }
                else
                {
                    temp4 = weak_remaining[UnityEngine.Random.Range(0, weak_remaining.Count)];
                    weak_remaining.Remove(temp4);
                }
            }
            if (temp4 >= 12)
            {
                temp3 = 0;
            }
            //print("Spawning vek no " + temp4 + " which has alpha status " + temp3 + " on tile " + temp.getX() + "," + temp.getY());
            getTile(temp).spawn = true;
            spawn_tiles.Add(temp);
            spawn_vek.Add(new Vector2Int(temp4, temp3));
            if (spawnableTiles.Count == 0)
            {
                spawnableTiles = GetSpawnableTiles();
            }

        }
    }

    private void PopulateVekLimits()
    {
        weak_remaining.Clear();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < weapons.max_spawns[island_spawns[i]]; j++)
            {
                weak_remaining.Add(island_spawns[i]);
            }
        }
        strong_remaining.Clear();
        for (int i = 4; i < island_spawns.Count; i++)
        {
            for (int j = 0; j < weapons.max_spawns[island_spawns[i]]; j++)
            {
                strong_remaining.Add(island_spawns[i]);
            }
        }
    }

    private void SpawnsEmerge()
    {
        int i = 0;
        explode_queue.Clear();
        //print(spawn_tiles.Count);
        while (i < spawn_tiles.Count)
        {
            if (!getTile(spawn_tiles[i]).occupied)
            {
                SpawnUnit(spawn_tiles[i], spawn_vek[i].getX(), spawn_vek[i].getY());
                getTile(spawn_tiles[i]).spawn = false;
                spawn_tiles.Remove(spawn_tiles[i]);
                spawn_vek.Remove(spawn_vek[i]);
            }
            else
            {
                getTile(spawn_tiles[i]).occupant.bumpDamage(1);
                getCurTileGrid().mission_counters[1]++;
                i++;
            }
        }
        CheckExplode();
    }

    private void GenerateSpawnSelection()
    {
        /*[1] = Spawner:new{	
			num_weak = 4,
			num_upgrades = 1,  
			upgrade_max = 1,  
		},
		[2] = Spawner:new{
			num_weak = 4,
			num_upgrades = 2,  
			upgrade_max = 3, 
		},
		[3] = Spawner:new{
			num_weak = 3,
			num_upgrades = 3,
			upgrade_max = 5,  
		},
		[4] = Spawner:new{
			num_weak = 3,
			num_upgrades = 5,
			upgrade_max = 6,
		},*/
        weak_prob.Clear();
        alpha_prob.Clear();
        if (island_number == 1)
        {
            //EASY MODE
            weak_prob.Add(0);
            weak_prob.Add(0);
            weak_prob.Add(0);
            weak_prob.Add(0);
            weak_prob.Add(1);

            alpha_prob.Add(0);
            alpha_prob.Add(0);
            alpha_prob.Add(0);
            alpha_prob.Add(0);
            alpha_prob.Add(0);
            max_alpha = 0;
            //HARD MODE
            /*
            weak_prob.Add(0);
            weak_prob.Add(0);
            weak_prob.Add(0);
            weak_prob.Add(0);
            weak_prob.Add(1);

            alpha_prob.Add(0);
            alpha_prob.Add(0);
            alpha_prob.Add(0);
            alpha_prob.Add(0);
            alpha_prob.Add(1);

            max_alpha = 1;
            */
        }
        else if (island_number == 2)
        {
            weak_prob.Add(0);
            weak_prob.Add(0);
            weak_prob.Add(0);
            weak_prob.Add(0);
            weak_prob.Add(1);



            alpha_prob.Add(0);
            alpha_prob.Add(0);
            alpha_prob.Add(0);
            alpha_prob.Add(1);
            alpha_prob.Add(1);

            max_alpha = 3;
        }
        else if (island_number == 3)
        {
            weak_prob.Add(0);
            weak_prob.Add(0);
            weak_prob.Add(1);
            weak_prob.Add(1);
            weak_prob.Add(1);

            alpha_prob.Add(0);
            alpha_prob.Add(0);
            alpha_prob.Add(0);
            alpha_prob.Add(1);
            alpha_prob.Add(1);

            max_alpha = 5;

        }
        else
        {
            weak_prob.Add(0);
            weak_prob.Add(0);
            weak_prob.Add(1);
            weak_prob.Add(1);
            weak_prob.Add(1);

            alpha_prob.Add(1);
            alpha_prob.Add(1);
            alpha_prob.Add(1);
            alpha_prob.Add(1);
            alpha_prob.Add(1);

            max_alpha = 6;
        }
    }

    private Tile getRandomGridTile()
    {
        List<Tile> grid_tiles = new List<Tile>();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (getTile(i, j).occupant.allignment == 2 && getTile(i, j).occupant.objective_type == 0)
                {
                    grid_tiles.Add(getTile(i, j));
                }
            }
        }
        //print(grid_tiles.Count);
        return grid_tiles[UnityEngine.Random.Range(0, grid_tiles.Count)];
    }

    //--------------------------------------------------------------------
    //Function for levelling random skills (unused in reduced ver)
    //--------------------------------------------------------------------

    private void AssignCores()
    {
        int i;
        int cores_unspent = cores;
        weapon_upgrades.Clear();
        hp_upgrades.Clear();
        move_upgrades.Clear();
        for (i = 0; i < 3; i++)//Reset upgrades
        {
            weapon_upgrades.Add(0);
            hp_upgrades.Add(0);
            move_upgrades.Add(0);
        }
        weapon_upgrades.Add(0);

        List<int> temp = new List<int>();
        List<int> shuffle = new List<int>();
        for (i = 0; i < 8; i++)
        {
            temp.Add(i);
        }

        while (temp.Count > 0)
        {
            int cur = temp[UnityEngine.Random.Range(0, temp.Count)];
            temp.Remove(cur);
            shuffle.Add(cur);
        }
        for (i = 8; i < 11; i++)
        {
            temp.Add(i);
        }
        while (temp.Count > 0)
        {
            int cur = temp[UnityEngine.Random.Range(0, temp.Count)];
            temp.Remove(cur);
            shuffle.Add(cur);
        }
        for (i = 11; i < 14; i++)
        {
            temp.Add(i);
        }
        while (temp.Count > 0)
        {
            int cur = temp[UnityEngine.Random.Range(0, temp.Count)];
            temp.Remove(cur);
            shuffle.Add(cur);
        }
        for (i = 0; i < temp.Count; i++)
        {
            switch (shuffle[i])
            {
                case 0:
                    if (cores_unspent >= weapons.core_costs[cur_squad * 4, 0])
                    {
                        weapon_upgrades[0] += 1;
                        cores_unspent -= weapons.core_costs[cur_squad * 4, 0];
                    }
                    break;
                case 1:
                    if (cores_unspent >= weapons.core_costs[cur_squad * 4, 1] && weapons.core_costs[cur_squad * 4, 1] > -1)
                    {
                        weapon_upgrades[0] += 2;
                        cores_unspent -= weapons.core_costs[cur_squad * 4, 1];
                    }
                    break;
                case 2:
                    if (cores_unspent >= weapons.core_costs[cur_squad * 4 + 1, 0])
                    {
                        weapon_upgrades[1] += 1;
                        cores_unspent -= weapons.core_costs[cur_squad * 4 + 1, 0];
                    }
                    break;
                case 3:
                    if (cores_unspent >= weapons.core_costs[cur_squad * 4 + 1, 1] && weapons.core_costs[cur_squad * 4 + 1, 1] > -1)
                    {
                        weapon_upgrades[1] += 2;
                        cores_unspent -= weapons.core_costs[cur_squad * 4 + 1, 1];
                    }
                    break;
                case 4:
                    if (cores_unspent >= weapons.core_costs[cur_squad * 4 + 2, 0])
                    {
                        weapon_upgrades[2] += 1;
                        cores_unspent -= weapons.core_costs[cur_squad * 4 + 2, 0];
                    }
                    break;
                case 5:
                    if (cores_unspent >= weapons.core_costs[cur_squad * 4 + 2, 1] && weapons.core_costs[cur_squad * 4 + 2, 1] > -1)
                    {
                        weapon_upgrades[2] += 2;
                        cores_unspent -= weapons.core_costs[cur_squad * 4 + 2, 1];
                    }
                    break;
                case 6:
                    if (cores_unspent >= weapons.core_costs[cur_squad * 4 + 3, 0] && weapons.core_costs[cur_squad * 4 + 3, 0] > -1)
                    {
                        weapon_upgrades[3] += 1;
                        cores_unspent -= weapons.core_costs[cur_squad * 4 + 3, 0];
                    }
                    break;
                case 7:
                    if (cores_unspent >= weapons.core_costs[cur_squad * 4 + 3, 1] && weapons.core_costs[cur_squad * 4 + 3, 1] > -1)
                    {
                        weapon_upgrades[3] += 2;
                        cores_unspent -= weapons.core_costs[cur_squad * 4 + 3, 1];
                    }
                    break;
                case 8:
                    if (cores_unspent > 0)
                    {
                        move_upgrades[0] = 1;
                        cores_unspent--;
                    }
                    break;
                case 9:
                    if (cores_unspent > 0)
                    {
                        move_upgrades[1] = 1;
                        cores_unspent--;
                    }
                    break;
                case 10:
                    if (cores_unspent > 0)
                    {
                        move_upgrades[2] = 1;
                        cores_unspent--;
                    }
                    break;
                case 11:
                    if (cores_unspent > 0)
                    {
                        hp_upgrades[0] = 2;
                        cores_unspent--;
                    }
                    break;
                case 12:
                    if (cores_unspent > 0)
                    {
                        hp_upgrades[1] = 2;
                        cores_unspent--;
                    }
                    break;
                default:
                    if (cores_unspent > 0)
                    {
                        hp_upgrades[2] = 2;
                        cores_unspent--;
                    }
                    break;
            }
            if (cores_unspent <= 0)
            {
                break;
            }
        }
    }


    //--------------------------------------------------------------------
    //Functions for acting
    //--------------------------------------------------------------------


    private void InvalidAction(string s, int a)
    {
        //print("Invalid action " + a + " because " + s);
        for (int i = 0; i < 6; i++)
        {
            if (getCurTileGrid().actions_left[i])
            {
                getCurTileGrid().action_score[5] = -1;
                return;
            }
        }
        getCurTileGrid().action_score[0] = -1;
    }


    private void RefreshActions()
    {
        for (int i = 0; i < getCurTileGrid().mechs.Count; i++)
        {
            getCurTileGrid().mechs[i].checkWebbed(false);
        }
        for (int i = 0; i < 6; i++)
        {
            getCurTileGrid().actions_left[i] = true;
        }
        for (int i = 0; i < getCurTileGrid().mechs.Count; i++)
        {
            if (getCurTileGrid().mechs[i].health <= 0)
            {
                getCurTileGrid().actions_left[i] = false;
                getCurTileGrid().actions_left[i + 3] = false;
            }
        }
    }

    public void playerTurn()
    {
        getCurTileGrid().critical_damage = 0;
        if (random_actions)
        {
            getValidActions();
            while (getCurTileGrid().valid_actions.Count > 0)
            {
                if(getCurTileGrid().valid_actions.Count > 1)
                {
                    log.poss_actions += getCurTileGrid().valid_actions.Count;
                }
                Action(getCurTileGrid().valid_actions[UnityEngine.Random.Range(0, getCurTileGrid().valid_actions.Count)]);
                getValidActions();
            }
            Action(245);
        }
        else if (optimal_actions)
        {
            //print("New turn");
            for (int i = 0; i < 6; i++)
            {
                //print("New action");
                List<Vector2Int> actions = getTopActions();
                //print("All");
                for (int j = 0; j < actions.Count; j++)
                {
                    //print(actions[j].getX() + " " + actions[j].getY());
                }
                //print("Best");
                //print(actions[0].getX());
                Action(actions[0].getX());
            }
            Action(245);
        }
        else
        {
            //score = getActionScore();
            //Detriment compared to last turn. Faulty logic
            /*
            if (cur_turn <= 2)
            {
                score = 0;
            }
            else
            {
                score = score_store_true;
                //score = getCurTileGrid().prev_score - score_store2;
                //print("Guh");
                //print(cur_turn);
                //print(getCurTileGrid().prev_score);
                //print(score_store2);
            }*/
            //getCurTileGrid().prev_score = scorePosition();
            score = getActionScore();
            layout_string = writeToFileAlt();

            turn_ended = false;
            while (true)
            {
                string s = read_mem.readMem();
                //print(s);
                char[] chars = s.ToCharArray();
                if (chars.Length > 0 && (chars[0] - '0') == 2)
                {
                    Application.Quit();
                    print(getCurTileGrid().mechs[100]);//Error to stop code
                    break;
                }
                if (chars.Length > 0 && (chars[0] - '0') == receive_ver)
                {
                    sendHandshake(0);
                    break;
                }
            }

            while (true)
            {
                string s = read_mem.readMem();
                //print(s);
                char[] chars = s.ToCharArray();
                if (chars.Length > 0 && (chars[0] - '0') == 2)
                {
                    Application.Quit();
                    print(getCurTileGrid().mechs[100]);//Error to stop code
                    break;
                }
                if (chars.Length > 1 && (chars[0] - '0') == receive_ver)
                {
                    int ac = 0;
                    if (s[1] == 'r')
                    {
                        //print("Hi 2");
                        List<Vector2Int> actions = getTopActions();
                        int a =  actions[UnityEngine.Random.Range(0, actions.Count)].getX();
                        sendBestHandshake(a);
                        continue;
                    }
                    else if (s[1] == 't')
                    {
                        //print("Hi 2");
                        List<Vector2Int> actions = getTopActions();
                        int a = actions[UnityEngine.Random.Range(0, actions.Count)].getX();
                        sendBestHandshake(a);
                        continue;
                    }
                    else
                    {
                        for (int i = 1; i < s.Length; i++)
                        {
                            ac = ac * 10 + (s[i] - '0');
                        }
                    }
                    if (Action(ac))
                    {
                        //print("Succ acc!");
                        //print(ac);
                        score = getActionScore();
                        layout_string = writeToFileAlt();
                    }
                    else
                    {
                        //print("Fail acc");
                        score = -200;
                    }
                    //print("The goodness!");
                    if(turn_ended || getCurTileGrid().grid_health <= 0)
                    {
                        break;
                    }
                    sendHandshake(0);
                    updateSprites();
                }
            }

        }
    }

    public bool Action(int action)
    {
        bool[] actions = getCurTileGrid().actions_left;
        int x;
        int y;
        latest_action = action;
        TileOccupant mech;
        //print("Action: " + action);
        //print("Turn:"+ cur_turn);
        if (action < 64)
        {
            x = action / 8;
            y = action % 8;
            if (x > 7)
            {
                x = 7;
            }
            if (x < 0)
            {
                x = 0;
            }
            if (y > 7)
            {
                y = 7;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (!actions[0])
            {
                InvalidAction("Used move for mech 0 already on turn " + cur_turn, action);
                return false;
            }

            if (cur_turn == 1)
            {
                if (x == 7 || x == 0 || y == 7 || y < 4 || getTile(x, y).occupied)
                {
                    InvalidAction("Invalid move for mech 0 on turn 1", action);
                    return false;
                }
                Tile tile = getTile(x, y);
                tile.occupant.Override(0, weapons.allyweapons[cur_squad * 3, weapon_upgrades[0]], 0, weapons.mech_moves[cur_squad * 3] + move_upgrades[0], weapons.mech_hp[cur_squad * 3] + hp_upgrades[0], 1, 0, 0, false);
                tile.occupied = true;
                getCurTileGrid().mechs.Add(tile.occupant);
            }
            else
            {
                mech = tile_grids[cur_grid].mechs[0];
                legal_move_tiles1 = getLegalMoveTiles(mech.myTile.location, mech);
                if (legal_move_tiles1[x, y] != 1)
                {
                    InvalidAction("Invalid move for mech 0", action);
                    return false;
                }
                mech.myTile.SwapOccupantMovement(getTile(x, y));
            }
            if (slow)
            {
                print("Moved mech 1 to tile " + x + "," + y);
            }
            actions[0] = false;
        }
        else if (action < 128)
        {
            x = (action - 64) / 8;
            y = (action - 64) % 8;
            if (x > 7)
            {
                x = 7;
            }
            if (x < 0)
            {
                x = 0;
            }
            if (y > 7)
            {
                y = 7;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (!actions[1])
            {
                InvalidAction("Used move for mech 1 already on turn " + cur_turn, action);
                return false;
            }
            if (cur_turn == 1)
            {
                if (x == 7 || x == 0 || y == 7 || y < 4 || getTile(x, y).occupied)
                {
                    InvalidAction("Invalid move for mech 1 on turn 1", action);
                    return false;
                }
                Tile tile = getTile(x, y);
                tile.occupant.Override(1, weapons.allyweapons[cur_squad * 3 + 1, weapon_upgrades[1]], 1, weapons.mech_moves[cur_squad * 3 + 1] + move_upgrades[1], weapons.mech_hp[cur_squad * 3 + 1] + hp_upgrades[1], 1, 0, 0, false);
                tile.occupied = true;
                getCurTileGrid().mechs.Add(tile.occupant);
            }
            else
            {
                mech = tile_grids[cur_grid].mechs[1];
                legal_move_tiles2 = getLegalMoveTiles(mech.myTile.location, mech);
                if (legal_move_tiles2[x, y] != 1)
                {
                    InvalidAction("Invalid move for mech 1", action);
                    return false;
                }
                mech.myTile.SwapOccupantMovement(getTile(x, y));
            }
            if (slow)
            {
                print("Moved mech 2 to tile " + x + "," + y);
            }
            actions[1] = false;
        }
        else if (action < 192)
        {
            x = (action - 128) / 8;
            y = (action - 128) % 8;
            if (x > 7)
            {
                x = 7;
            }
            if (x < 0)
            {
                x = 0;
            }
            if (y > 7)
            {
                y = 7;
            }
            if (y < 0)
            {
                y = 0;
            }
            if (!actions[2])
            {
                InvalidAction("Used move for mech 2 already on turn " + cur_turn, action);
                return false;
            }
            if (cur_turn == 1)
            {
                if (x == 7 || x == 0 || y == 7 || y < 4 || getTile(x, y).occupied)
                {
                    InvalidAction("Invalid move for mech 2 on turn 1", action);
                    return false;
                }
                Tile tile = getTile(x, y);
                tile.occupant.Override(2, weapons.allyweapons[cur_squad * 3 + 2, weapon_upgrades[2]], 2, weapons.mech_moves[cur_squad * 3 + 2] + move_upgrades[2], weapons.mech_hp[cur_squad * 3 + 2] + hp_upgrades[2], 1, 0, 0, false);
                tile.occupied = true;
                getCurTileGrid().mechs.Add(tile.occupant);
            }
            else
            {
                mech = tile_grids[cur_grid].mechs[2];
                legal_move_tiles3 = getLegalMoveTiles(mech.myTile.location, mech);
                if (legal_move_tiles3[x, y] != 1)
                {
                    InvalidAction("Invalid move for mech 2", action);
                    return false;
                }
                mech.myTile.SwapOccupantMovement(getTile(x, y));
            }
            if (slow)
            {
                print("Moved mech 3 to tile " + x + "," + y);
            }
            actions[2] = false;
        }
        else if (action < 208)
        {
            if (tile_grids[cur_grid].mechs.Count < 1)
            {
                InvalidAction("Trying to move mech 0 when it doesn't exist", action);
                return false;
            }
            mech = tile_grids[cur_grid].mechs[0];
            if (!actions[3] || cur_turn == 1 || mech.myTile.smoke || mech.frozen || mech.health <= 0 || (mech.myTile.liquid && !mech.flying))
            {
                InvalidAction("Mech 0 already used action or can't act", action);
                return false;
            }
            action -= 192;
            Tile tile;
            if (action < 8)
            {
                tile = getTile(action, mech.myTile.location.getY());
            }
            else
            {
                action -= 8;
                tile = getTile(mech.myTile.location.getX(), action);
            }
            legal_attack_tiles1 = GetValidAttackPositionTiles(mech.myTile.location, mech.weapon_1);
            if (!legal_attack_tiles1[tile.location.getX(), tile.location.getY()])
            {
                InvalidAction("Mech 0 can't attack tile " + tile.location.getX() + "," + tile.location.getY() + " from location " + mech.myTile.location.getX() + "," + mech.myTile.location.getY(), action);
                return false;
            }
            mech.attack_cancelled = false;
            MakeAttack(mech.myTile.location, tile.location, mech.weapon_1);
            actions[0] = false;
            actions[3] = false;
            if (slow)
            {
                print("Mech 1 attacked " + tile.location.getX() + "," + tile.location.getY());
            }
        }
        else if (action < 224)
        {
            if (tile_grids[cur_grid].mechs.Count < 2)
            {
                InvalidAction("Trying to move mech 1 when it doesn't exist", action);
                return false;
            }
            mech = tile_grids[cur_grid].mechs[1];
            if (!actions[4] || cur_turn == 1 || mech.myTile.smoke || mech.frozen || mech.health <= 0 || (mech.myTile.liquid && !mech.flying))
            {
                InvalidAction("Mech 1 already used action or can't act", action);
                return false;
            }
            action -= 208;
            Tile tile;
            if (action < 8)
            {
                tile = getTile(action, mech.myTile.location.getY());
            }
            else
            {
                action -= 8;
                tile = getTile(mech.myTile.location.getX(), action);
            }
            legal_attack_tiles2 = GetValidAttackPositionTiles(mech.myTile.location, mech.weapon_1);
            if (!legal_attack_tiles2[tile.location.getX(), tile.location.getY()])
            {
                InvalidAction("Mech 1 can't attack tile " + tile.location.getX() + "," + tile.location.getY() + " from location " + mech.myTile.location.getX() + "," + mech.myTile.location.getY(), action);
                return false;
            }
            mech.attack_cancelled = false;
            MakeAttack(mech.myTile.location, tile.location, mech.weapon_1);
            if (slow)
            {
                print("Mech 2 attacked " + tile.location.getX() + "," + tile.location.getY());
            }
            actions[1] = false;
            actions[4] = false;
        }
        else if (action < 240)
        {
            //print("!???");
            if (tile_grids[cur_grid].mechs.Count < 3)
            {
                InvalidAction("Trying to move mech 2 when it doesn't exist", action);
                return false;
            }
            mech = tile_grids[cur_grid].mechs[2];
            if (!actions[5] || cur_turn == 1 || mech.myTile.smoke || mech.frozen || mech.health <= 0 || (mech.myTile.liquid && !mech.flying))
            {
                InvalidAction("Mech 2 already used action or can't act", action);
                return false;
            }
            action -= 224;
            Tile tile;
            if (action < 8)
            {
                tile = getTile(action, mech.myTile.location.getY());
            }
            else
            {
                action -= 8;
                tile = getTile(mech.myTile.location.getX(), action);
            }
            legal_attack_tiles3 = GetValidAttackPositionTiles(mech.myTile.location, mech.weapon_1);
            if (!legal_attack_tiles3[tile.location.getX(), tile.location.getY()])
            {
                InvalidAction("Mech 2 can't attack tile " + tile.location.getX() + "," + tile.location.getY() + " from location " + mech.myTile.location.getX() + "," + mech.myTile.location.getY(), action);
                return false;
            }
            //print("Attacking woop!" + tile.location.getX() + "," + tile.location.getY());
            //print(mech.weapon_1.name);
            mech.attack_cancelled = false;
            MakeAttack(mech.myTile.location, tile.location, mech.weapon_1);
            if (slow)
            {
                print("Mech 3 attacked " + tile.location.getX() + "," + tile.location.getY());
            }
            actions[2] = false;
            actions[5] = false;
        }
        else if (action == 240)
        {
            if (tile_grids[cur_grid].mechs.Count < 1)
            {
                InvalidAction("Trying to rep mech 0 when it doesn't exist", action);
                return false;
            }
            mech = tile_grids[cur_grid].mechs[0];
            if (!actions[3] || cur_turn == 1 || mech.myTile.smoke || mech.health <= 0 || (mech.myTile.liquid && !mech.flying))
            {
                InvalidAction("Mech 0 already used action or can't repair", action);
                return false;
            }
            tile_grids[cur_grid].mechs[0].Repair();
            if (slow)
            { 
            print("Mech 1 repaired");
            }
            actions[0] = false;
            actions[3] = false;
        }
        else if (action == 241)
        {
            if (tile_grids[cur_grid].mechs.Count < 2)
            {
                InvalidAction("Trying to rep mech 1 when it doesn't exist", action);
                return false;
            }
            mech = tile_grids[cur_grid].mechs[1];
            if (!actions[4] || cur_turn == 1 || mech.myTile.smoke || mech.health <= 0 || (mech.myTile.liquid && !mech.flying))
            {
                InvalidAction("Mech 1 already used action or can't repair", action);
                return false;
            }
            tile_grids[cur_grid].mechs[1].Repair();
            if (slow)
            {
                print("Mech 2 repaired");
            }
            actions[1] = false;
            actions[4] = false;
        }
        else if (action == 242)
        {
            if (tile_grids[cur_grid].mechs.Count < 3)
            {
                InvalidAction("Trying to rep mech 2 when it doesn't exist", action);
                return false;
            }
            mech = tile_grids[cur_grid].mechs[2];
            if (!actions[5] || cur_turn == 1 || mech.myTile.smoke || mech.health <= 0 || (mech.myTile.liquid && !mech.flying))
            {
                InvalidAction("Mech 2 already used action or can't repair", action);
                return false;
            }
            tile_grids[cur_grid].mechs[2].Repair();
            actions[2] = false;
            actions[5] = false;
            if (slow)
            {
                print("Mech 3 repaired");
            }
        }
        else if (action == 243)
        {
            InvalidAction("Can't swap yet :PP", action);
            return false;
        }
        else
        {
            if (tile_grids[cur_grid].mechs.Count < 3)
            {
                InvalidAction("Haven't deployed all mechs", action);
                return false;
            }
            turn_ended = true;
            if (slow)
            {
                print("Ending turn");
            }
        }
        updateSprites();
        getCurTileGrid().actions_left = actions;
        return true;
    }

    //--------------------------------------------------------------------
    //Functions for communication and writing data
    //--------------------------------------------------------------------


    private void sendHandshake(int done)
    {//Send starts 0, rec starts 1
        if(random_actions || optimal_actions)
        {
            return;
        }
        string legal = writeLegalActions();
        while (true)
        {
            try
            {
                StreamWriter sw = new StreamWriter("./assets/ActData.txt");
                sw.Write(legal);
                sw.Close();
                StreamWriter sw2 = new StreamWriter("./assets/EnvData.txt");
                //print("Sending " + send_ver);
                sw2.Write(send_ver + "" + done + "" + score + "," + layout_string);
                sw2.Close();
                break;
            }
            catch
            {
                print("Sussy sus error");
            }
        }
        send_ver = (send_ver + 1) % 2;
        receive_ver = (receive_ver + 1) % 2;
    }

    private void sendBestHandshake(int action)
    {//Send starts 0, rec starts 1
        string legal = writeLegalActions();
        while (true)
        {
            try
            {
                StreamWriter sw2 = new StreamWriter("./assets/EnvData.txt");
                //print("Sending " + send_ver);
                sw2.Write(send_ver + "" + action);
                sw2.Close();
                break;
            }
            catch
            {
                print("Sussy sus error 2");
            }
        }
        send_ver = (send_ver + 1) % 2;
        receive_ver = (receive_ver + 1) % 2;
    }

    private int scorePositionSimulate()
    {
        //print(cur_grid);
        int i = 0;
        for (i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                tile_threat[i, j] = 0;
            }
        }
        cur_grid = cur_grid + 1;
        tile_grids[cur_grid].Steal(tile_grids[cur_grid - 1]);
        EnemyTurn();

        for (i = 1; i < 5; i++)
        {
            getCurTileGrid().action_score[i] = 0;
        }

        for (i = 0; i < getCurTileGrid().grid_health; i++)
        {
            getCurTileGrid().action_score[2] += (7 + (8 - i));
        }
        getCurTileGrid().action_score[2] -= getCurTileGrid().critical_damage * 10;
        if (getCurTileGrid().pod_destroyed == -1)
        {
            getCurTileGrid().action_score[2] -= 35;
        } 
        /*if (!getCurTileGrid().perfect_bonus)
        {
            getCurTileGrid().action_score[2] -= 10;
        }*/
        if (!(getCurTileGrid().objective1.occupied && getCurTileGrid().objective1.occupant.allignment == 2))
        {
            if (getCurTileGrid().objective1.occupant.objective_type == 1 && getCurTileGrid().grid_health < 7)
            {
                getCurTileGrid().action_score[2] -= (5 + (8 - getCurTileGrid().grid_health));
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 1 && getCurTileGrid().grid_health == 7)
            {
                getCurTileGrid().action_score[2] -= 1;
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 2)
            {
                getCurTileGrid().action_score[2] -= 10;
            }
        }
        if (!(getCurTileGrid().objective2 == null || (getCurTileGrid().objective1.occupied && getCurTileGrid().objective1.occupant.allignment == 2)))
        {
            if (getCurTileGrid().objective1.occupant.objective_type == 1 && getCurTileGrid().grid_health < 7)
            {
                getCurTileGrid().action_score[2] -= (5 + (8 - getCurTileGrid().grid_health));
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 1 && getCurTileGrid().grid_health == 7)
            {
                getCurTileGrid().action_score[2] -= 1;
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 2)
            {
                getCurTileGrid().action_score[2] -= 10;
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 3)
            {
                getCurTileGrid().action_score[2] -= 30;
            }
        }
        //print("Current action 2 value" + getCurTileGrid().action_score[2]);
        /*for(int i=0;i<8;i++)
        {
            for(int j = 0;j<8;j++)
            {
                print(getTile(i, j).location.getX());
            }
        }*/
        for (i = 0; i < getCurTileGrid().mechs.Count; i++)
        {
            //print(getCurTileGrid().mechs.Count);
            TileOccupant mech = getCurTileGrid().mechs[i];

            //print("Eh?" + mech.myTile.location.getX());

            getCurTileGrid().action_score[4] += getLegalMoves(mech.myTile.location, mech).Count;
            if (mech.health <= 0)
            {
                float predicted_loss = (5 - cur_turn) * 1.5f;
                for (int j = 0; j < predicted_loss; j++)
                {
                    getCurTileGrid().action_score[2] -= (7 + (8 - getCurTileGrid().grid_health));
                }
                //getCurTileGrid().action_score[2] -= 10;
                continue;
            }
            for (int j = 1; j <= mech.health; j++)
            {
                getCurTileGrid().action_score[3] += 5 / j;
            }
            if (mech.fire)
            {
                float predicted_loss = (5 - cur_turn);
                /*for (int j = mech.health; j > Mathf.Min(1, mech.health - predicted_loss); j++)
                {
                    getCurTileGrid().action_score[3] -= 4 / j;
                }*/
                getCurTileGrid().action_score[3] -= 1;
            }
            if (mech.frozen)
            {
                getCurTileGrid().action_score[3] -= 1;
                //getCurTileGrid().action_score[3] -= 10;
            }
            if (mech.acid)
            {
                getCurTileGrid().action_score[3] -= 1;
                //getCurTileGrid().action_score[3] -= 4;
            }
            if (mech.webbed)//For bigger
            {
                //getCurTileGrid().action_score[3] -= 5;
            }
            /*
            int has_target = 0;
            //print(i);

            if (tile_grids[cur_grid].actions_left[i + 3])
            {
                List<Coordinate> v = GetValidAttackPositions(mech.myTile.location, mech.weapon_1);
                for (int j = 0; j < v.Count; j++)
                {
                    for (int l = 0; l < mech.weapon_1.targetEffects.Count; l++)
                    {
                        Coordinate affected = v[j].addCoord(mech.weapon_1.targetEffects[l].relative_pos.rotate(mech.myTile.location.getDirection(v[j])));
                        if (!affected.outOfBounds() && getTile(affected).occupied)
                        {
                            if (getTile(affected).occupant.allignment == -1)
                            {
                                has_target = 2;
                                break;
                            }
                        }
                    }
                    if (has_target == 2)
                    {
                        break;
                    }
                }
            }
            //getCurTileGrid().action_score[4] += 25 * has_target; Bad, provides no overall reward since Q values updated afterwards anyway
            if((mech.myTile.liquid && !mech.flying) || mech.myTile.smoke)
            {
                //getCurTileGrid().action_score[4] -= 50;
            }
            */
        }
        for (i = 0; i < getCurTileGrid().attack_order.Count; i++)
        {
            TileOccupant enemy = getCurTileGrid().attack_order[i];
            if (!enemy.frozen)
            {
                if (enemy.alpha == 1)
                {
                    getCurTileGrid().action_score[3] -= weapons.alphaScore[enemy.unit_variant];
                    getCurTileGrid().action_score[3] -= enemy.health * weapons.alphaHpScore[enemy.unit_variant];
                }
                else
                {
                    getCurTileGrid().action_score[3] -= weapons.enemyScore[enemy.unit_variant];
                    getCurTileGrid().action_score[3] -= enemy.health * weapons.enemyHpScore[enemy.unit_variant];
                }
                
                if (enemy.fire)
                {
                    //getCurTileGrid().action_score[3] += Mathf.Min(5 - cur_turn, enemy.health) * 2;
                    getCurTileGrid().action_score[3] += 1;
                }
            }
            if (enemy.acid)
            {
                //getCurTileGrid().action_score[3] += enemy.health - 1;
                getCurTileGrid().action_score[3] += 1;
            }
        }
        /*
        int unblocked = 0;
        for (i = 0; i < spawn_tiles.Count; i++)
        {
            if (getTile(spawn_tiles[i]).occupied)
            {
                getCurTileGrid().action_score[3] += 5;
            }
            else
            {
                unblocked++;
            }
        }*/
        /*int[] mission_counters = getCurTileGrid().mission_counters;
        int next_enemies = unblocked + getCurTileGrid().attack_order.Count;
        if (cur_turn < 4 && (next_enemies < 3 || next_enemies > 3))
        {
            getCurTileGrid().action_score[3] -= 8;
        }
        if (cur_mission == 0)//Vek kills (7), blocked spawns(3), grid damage(3), mech damage(4)
        {
            if (mission_counters[0] >= 7)
            {
                getCurTileGrid().action_score[2] += 10;
            }
            else
            {
                getCurTileGrid().action_score[3] += 10 * Mathf.Min(7, mission_counters[0]);
            }
        }
        else if (cur_mission == 1)
        {
            if (mission_counters[1] >= 3)
            {
                getCurTileGrid().action_score[2] += 10;
            }
            else
            {
                getCurTileGrid().action_score[3] += 10 * Mathf.Min(7, mission_counters[1]);
            }
        }
        else if (cur_mission == 2)
        {
            getCurTileGrid().action_score[3] -= (10 / 3) * Mathf.Min(3, mission_counters[2]);
        }
        else if (cur_mission == 3)
        {
            if (hp_loss >= 4)
            {
                getCurTileGrid().action_score[2] -= 10;
            }
            else
            {
                getCurTileGrid().action_score[3] -= 7 * Mathf.Min(4, hp_loss);
            }
        }*/
        //getCurTileGrid().action_score[1] -= getCurTileGrid().critical_damage;
        //print("Build damage neg " + getCurTileGrid().action_score[2]);
        //print("OVerkill damage" + getCurTileGrid().action_score[1] * 1000000);
        /*print("Cur score:");
        print(getCurTileGrid().action_score[1] * 200000 + getCurTileGrid().action_score[2] * 1000 + getCurTileGrid().action_score[3] * 100 + getCurTileGrid().action_score[4]);
        print(getCurTileGrid().action_score[1] * 200000);
        print(getCurTileGrid().action_score[2] * 1000);
        print(getCurTileGrid().action_score[3] * 100);
        print(getCurTileGrid().action_score[4]);
        print("There we go");
        //Get score sum
       */
        updateSprites();
        
        cur_grid--;
        getCurTileGrid().score =tile_grids[cur_grid + 1].action_score[2] * 1000 + tile_grids[cur_grid + 1].action_score[3] * 100 + tile_grids[cur_grid + 1].action_score[4];
        //getCurTileGrid().score = tile_grids[cur_grid + 1].action_score[2] * 50 + tile_grids[cur_grid + 1].action_score[3];
        //print(getCurTileGrid().score);
        return getCurTileGrid().score;
    }

    private int scorePosition()//Old
    {
        return scorePositionSimulate();


        for (int i = 1; i < 5; i++)
        {
            getCurTileGrid().action_score[i] = 0;
        }

        for (int i = 0; i < getCurTileGrid().grid_health; i++)
        {
            getCurTileGrid().action_score[2] += (7 + (8 - i));
        }
        if (getCurTileGrid().pod_destroyed == -1)
        {
            getCurTileGrid().action_score[2] -= 35;
        }
        if (!getCurTileGrid().perfect_bonus)
        {
            getCurTileGrid().action_score[2] -= 10;
        }
        if (!(getCurTileGrid().objective1.occupied && getCurTileGrid().objective1.occupant.allignment == 2))
        {
            if (getCurTileGrid().objective1.occupant.objective_type == 1 && getCurTileGrid().grid_health < 7)
            {
                getCurTileGrid().action_score[2] -= (7 + (8 - getCurTileGrid().grid_health));
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 1 && getCurTileGrid().grid_health == 7)
            {
                getCurTileGrid().action_score[2] -= 1;
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 2)
            {
                getCurTileGrid().action_score[2] -= 10;
            }
        }
        if (!(getCurTileGrid().objective2 == null || (getCurTileGrid().objective1.occupied && getCurTileGrid().objective1.occupant.allignment == 2)))
        {
            if (getCurTileGrid().objective1.occupant.objective_type == 1 && getCurTileGrid().grid_health < 7)
            {
                getCurTileGrid().action_score[2] -= (7 + (8 - getCurTileGrid().grid_health));
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 1 && getCurTileGrid().grid_health == 7)
            {
                getCurTileGrid().action_score[2] -= 1;
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 2)
            {
                getCurTileGrid().action_score[2] -= 10;
            }
            else if (getCurTileGrid().objective1.occupant.objective_type == 3)
            {
                getCurTileGrid().action_score[2] -= 30;
            }
        }
        //print("Current action 2 value" + getCurTileGrid().action_score[2]);
        int hp_loss = 0;
        /*for(int i=0;i<8;i++)
        {
            for(int j = 0;j<8;j++)
            {
                print(getTile(i, j).location.getX());
            }
        }*/
        for (int i = 0; i < getCurTileGrid().mechs.Count; i++)
        {
            //print(getCurTileGrid().mechs.Count);
            TileOccupant mech = getCurTileGrid().mechs[i];
            hp_loss += mech.max_health - mech.health;
            //print("Eh?" + mech.myTile.location.getX());

            getCurTileGrid().action_score[4] += getLegalMoves(mech.myTile.location, mech).Count;
            if (mech.health <= 0)
            {
                float predicted_loss = (5 - cur_turn) * 1.5f;
                for (int j = 0; j < predicted_loss; j++)
                {
                    getCurTileGrid().action_score[2] -= (7 + (8 - getCurTileGrid().grid_health));
                }
                continue;
            }
            for (int j = 1; j <= mech.health; j++)
            {
                getCurTileGrid().action_score[3] += 4 / j;
            }
            if (mech.fire)
            {
                float predicted_loss = (5 - cur_turn);
                for (int j = mech.health; j > Mathf.Min(1, mech.health - predicted_loss); j++)
                {
                    getCurTileGrid().action_score[3] -= 4 / j;
                }
            }
            if (mech.frozen)
            {
                getCurTileGrid().action_score[3] -= 12;
            }
            if (mech.acid)
            {
                getCurTileGrid().action_score[3] -= 3;
            }
            if (mech.webbed)
            {
                getCurTileGrid().action_score[3] -= 5;
            }
            int has_target = 0;
            if(getCurTileGrid().actions_left[i+3])
            {
                List<Coordinate> v = GetValidAttackPositions(mech.myTile.location, mech.weapon_1);
                for (int j = 0;j<v.Count;j++)
                {
                    for(int l = 0;l<mech.weapon_1.targetEffects.Count;l++)
                    {
                        Coordinate affected = v[j].addCoord(mech.weapon_1.targetEffects[l].relative_pos.rotate(mech.myTile.location.getDirection(v[j])));
                        if(!affected.outOfBounds() && getTile(affected).occupied)
                        {
                            if(getTile(affected).occupant.allignment == -1)
                            {
                                has_target = 2;
                                break;
                            }
                            if(getTile(affected).occupant.allignment == 1)
                            {
                                has_target = 1;
                            }

                        }
                    }
                    if(has_target == 2)
                    {
                        break;
                    }
                }
            }
            getCurTileGrid().action_score[4] += 50 * has_target;
        }
        for (int i = 0; i < getCurTileGrid().attack_order.Count; i++)
        {
            TileOccupant enemy = getCurTileGrid().attack_order[i];
            if (!enemy.frozen)
            {
                if (enemy.alpha == 1)
                {
                    getCurTileGrid().action_score[3] -= 3;
                }
                if(enemy.alpha == -1 && !enemy.fire)
                {
                    getCurTileGrid().action_score[3] -= 5;
                    break;
                }
                getCurTileGrid().action_score[3] -= weapons.enemyScore[enemy.unit_variant];
                getCurTileGrid().action_score[3] -= enemy.health * 2;
                if (enemy.fire)
                {
                    getCurTileGrid().action_score[3] += Mathf.Min(5 - cur_turn, enemy.health) * 2;
                }
                if (enemy.acid)
                {
                    getCurTileGrid().action_score[3] += enemy.health - 1;
                }
            }
        }
        int unblocked = 0;
        for (int i = 0; i < spawn_tiles.Count; i++)
        {
            if (getTile(spawn_tiles[i]).occupied)
            {
                getCurTileGrid().action_score[3] += 7;
            }
            else
            {
                unblocked++;
            }
        }
        int[] mission_counters = getCurTileGrid().mission_counters;
        int next_enemies = unblocked + getCurTileGrid().attack_order.Count;
        if (cur_turn < 4 && (next_enemies < 3 || next_enemies > 3))
        {
            getCurTileGrid().action_score[3] -= 8;
        }
        if (cur_mission == 0)//Vek kills (7), blocked spawns(3), grid damage(3), mech damage(4)
        {
            if (mission_counters[0] >= 7)
            {
                getCurTileGrid().action_score[2] += 10;
            }
            else
            {
                getCurTileGrid().action_score[3] += 10 * Mathf.Min(7, mission_counters[0]);
            }
        }
        else if (cur_mission == 1)
        {
            if (mission_counters[1] >= 3)
            {
                getCurTileGrid().action_score[2] += 10;
            }
            else
            {
                getCurTileGrid().action_score[3] += 10 * Mathf.Min(7, mission_counters[1]);
            }
        }
        else if (cur_mission == 2)
        {
            getCurTileGrid().action_score[3] -= (10 / 3) * Mathf.Min(3, mission_counters[2]);
        }
        else if (cur_mission == 3)
        {
            if (hp_loss >= 4)
            {
                getCurTileGrid().action_score[2] -= 10;
            }
            else
            {
                getCurTileGrid().action_score[3] -= 7 * Mathf.Min(4, hp_loss);
            }
        }
        getCurTileGrid().action_score[1] -= getCurTileGrid().critical_damage;
        //Get value of targetted tiles
        int future_damage = 0;
        for (int i = 0; i < getCurTileGrid().attack_order.Count; i++)
        {
            TileOccupant enemy = getCurTileGrid().attack_order[i];
            enemy.updateTarget();
            if(enemy.attack_cancelled || enemy.targetcoord.Equals(new Coordinate(9,9)))
            {
                continue;
            }
            Coordinate target = enemy.targetcoord.addCoord(enemy.myTile.location);
            for (int l = 0; l < enemy.weapon_1.targetEffects.Count; l++)
            {
                int damage = enemy.weapon_1.targetEffects[l].damage;
                //
                Coordinate affected = target.addCoord(enemy.weapon_1.targetEffects[l].relative_pos.rotate(enemy.myTile.location.getDirection(target)));
                //print("Target is " + getTile(affected).occupant.allignment);
                if (affected.outOfBounds() || !getTile(affected).occupied || damage == 0)
                {
                    continue;
                }
                if (getTile(affected).occupant.allignment == 1)
                {
                    getCurTileGrid().action_score[3] -= damage * 2;
                    if (getTile(affected).occupant.health - damage <= 0 || getTile(affected).occupant.acid && getTile(affected).occupant.health - 2 * damage <= 0)
                    {
                        float predicted_loss = (4 - cur_turn) * 1.5f;
                        for (int j = 0; j < predicted_loss; j++)
                        {
                            getCurTileGrid().action_score[2] -= (7 + (8 - getCurTileGrid().grid_health));
                        }
                    }
                }
                else if (getTile(affected).occupant.allignment == 2)
                {
                    if (getTile(affected).occupant.objective_type == 1 && getCurTileGrid().grid_health < 7)
                    {
                        getCurTileGrid().action_score[2] -= (7 + (8 - getCurTileGrid().grid_health));
                    }
                    else if (getTile(affected).occupant.objective_type == 1 && getCurTileGrid().grid_health == 7)
                    {
                        getCurTileGrid().action_score[2] -= 1;
                    }
                    else if (getTile(affected).occupant.objective_type == 2)
                    {
                        getCurTileGrid().action_score[2] -= 10;
                    }
                    else if (getTile(affected).occupant.objective_type == 3)
                    {
                        getCurTileGrid().action_score[2] -= 30;
                    }
                    //print("Lowering by " + damage);
                    getCurTileGrid().action_score[2] -= 10 * Mathf.Min(damage, getTile(affected).occupant.health);
                    future_damage += Mathf.Min(damage, getTile(affected).occupant.health);
                }
                else if (getTile(affected).occupant.allignment == -1)
                {
                    if(getTile(affected).occupant.frozen)
                    {
                        if (getTile(affected).occupant.alpha == -1)
                        {
                            getCurTileGrid().action_score[3] -= 5;
                        }
                        else
                        {
                            getCurTileGrid().action_score[3] -= weapons.enemyScore[getTile(affected).occupant.unit_variant];
                        }
                        getCurTileGrid().action_score[3] -= getTile(affected).occupant.health * 2;
                    }
                    else
                    {
                        if (damage > getTile(affected).occupant.health)
                        {
                            if (getTile(affected).occupant.alpha == -1)
                            {
                                getCurTileGrid().action_score[3] += 5;
                            }
                            else
                            {
                                getCurTileGrid().action_score[3] += weapons.enemyScore[getTile(affected).occupant.unit_variant];
                            }
                        }
                        getCurTileGrid().action_score[3] += Mathf.Min(damage, getTile(affected).occupant.health) * 2;
                    }
                }
                if (getTile(affected).pod)
                {
                    getCurTileGrid().action_score[2] -= 35;
                    break;
                }
            }
            for (int l = 0; l < enemy.weapon_1.selfEffects.Count; l++)
            {
                int damage = enemy.weapon_1.selfEffects[l].damage;
                Coordinate affected = target.addCoord(enemy.weapon_1.selfEffects[l].relative_pos.rotate(enemy.myTile.location.getDirection(target)));
                if (affected.outOfBounds() || !getTile(affected).occupied || damage == 0)
                {
                    continue;
                }
                if (getTile(affected).occupant.allignment == 1)
                {
                    getCurTileGrid().action_score[3] -= damage * 2;
                    if (getTile(affected).occupant.health - damage <= 0 || getTile(affected).occupant.acid && getTile(affected).occupant.health - 2 * damage <= 0)
                    {
                        float predicted_loss = (4 - cur_turn) * 1.5f;
                        for (int j = 0; j < predicted_loss; j++)
                        {
                            getCurTileGrid().action_score[2] -= (7 + (8 - getCurTileGrid().grid_health));
                        }
                    }
                }
                else if (getTile(affected).occupant.allignment == 2 && damage > 0)
                {
                    if (getTile(affected).occupant.objective_type == 1 && getCurTileGrid().grid_health < 7)
                    {
                        getCurTileGrid().action_score[2] -= (7 + (8 - getCurTileGrid().grid_health));
                    }
                    else if (getTile(affected).occupant.objective_type == 1 && getCurTileGrid().grid_health == 7)
                    {
                        getCurTileGrid().action_score[2] -= 1;
                    }
                    else if (getTile(affected).occupant.objective_type == 2)
                    {
                        getCurTileGrid().action_score[2] -= 10;
                    }
                    else if (getTile(affected).occupant.objective_type == 3)
                    {
                        getCurTileGrid().action_score[2] -= 30;
                    }
                    getCurTileGrid().action_score[2] -= 10 * Mathf.Min(damage,getTile(affected).occupant.health);
                    future_damage += Mathf.Min(damage, getTile(affected).occupant.health);
                }
                else if (getTile(affected).occupant.allignment == -1)
                {
                    if (getTile(affected).occupant.frozen)
                    {
                        if (getTile(affected).occupant.alpha == -1)
                        {
                            getCurTileGrid().action_score[3] -= 5;
                        }
                        else
                        {
                            getCurTileGrid().action_score[3] -= weapons.enemyScore[getTile(affected).occupant.unit_variant];
                        }
                        getCurTileGrid().action_score[3] -= getTile(affected).occupant.health * 2;
                    }
                    else
                    {
                        if (damage > getTile(affected).occupant.health)
                        {
                            if (getTile(affected).occupant.alpha == -1)
                            {
                                getCurTileGrid().action_score[3] += 5;
                            }
                            else
                            {
                                getCurTileGrid().action_score[3] += weapons.enemyScore[getTile(affected).occupant.unit_variant];
                            }
                        }
                        getCurTileGrid().action_score[3] += Mathf.Min(damage, getTile(affected).occupant.health) * 2;
                    }
                }
                if (getTile(affected).pod)
                {
                    getCurTileGrid().action_score[2] -= 35;
                }
            }
        }
        //print("Build damage neg " + getCurTileGrid().action_score[2]);
        //print("OVerkill damage" + getCurTileGrid().action_score[1] * 1000000);
        getCurTileGrid().action_score[1] += Mathf.Min((getCurTileGrid().grid_health - future_damage),0);
        print("Cur score orig:");
        print(getCurTileGrid().action_score[1] * 1000000 + getCurTileGrid().action_score[2] * 1000 + getCurTileGrid().action_score[3] * 100 + getCurTileGrid().action_score[4]);
        print(getCurTileGrid().action_score[1] * 1000000);
        print(getCurTileGrid().action_score[2] * 1000);
        print(getCurTileGrid().action_score[3] * 100);
        print(getCurTileGrid().action_score[4]);
        print("There we go Da");
        //Get score sum
        cur_grid = 0;
        return getCurTileGrid().score = getCurTileGrid().action_score[1] * 200000 + getCurTileGrid().action_score[2] * 1000 + getCurTileGrid().action_score[3] * 100 + getCurTileGrid().action_score[4];
    }

    private int getActionScore()
    {
        int temp = scorePosition();
        score = temp - getCurTileGrid().prev_score;
        getCurTileGrid().prev_score = temp;
        return score;
    }

    private void updateVekNumbers() //Makes sure vek are in order
    {
        for(int i = 0;i<getCurTileGrid().attack_order.Count;i++)
        {
            getCurTileGrid().attack_order[i].number = i;
        }
    }

    public string writeLegalActions()
    {
        string s = "";
        bool can_end = true;
        if (cur_turn == 1)
        {
            string ss = "";
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (i == 7 || i == 0 || j == 7 || j < 4 || getTile(i, j).occupied)
                    {
                        ss += "0";
                    }
                    else
                    {
                        ss += "1";
                    }
                }
            }
            for(int i = 0;i<3;i++)
            {
                if(getCurTileGrid().actions_left[i])
                {
                    s += ss;
                    can_end = false;
                }
                else
                {
                    s += "0000000000000000000000000000000000000000000000000000000000000000";
                }
            }
            s += "0000000000000000";
            s += "0000000000000000";
            s += "0000000000000000";
            s += "0000";
        }
        else
        {
            for (int k = 0; k < 3; k++)
            {
                if (getCurTileGrid().actions_left[k])
                {
                    int[,] temp = getLegalMoveTiles(getCurTileGrid().mechs[k].myTile.location, getCurTileGrid().mechs[k]);
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (temp[i,j] == 1)
                            {
                                s += "1";
                                can_end = false;
                            }
                            else
                            {
                                s += "0";
                            }
                        }
                    }
                }
                else
                {
                    s += "0000000000000000000000000000000000000000000000000000000000000000";
                }
            }
            for (int k = 0; k < 3; k++)
            {
                if (getCurTileGrid().actions_left[k + 3] && !getCurTileGrid().actions_left[k])//Force movement before action
                {
                    //print("Action " + k);
                    int pos = getCurTileGrid().mechs[k].myTile.location.getY();
                    bool[,] temp = GetValidAttackPositionTiles(getCurTileGrid().mechs[k].myTile.location, getCurTileGrid().mechs[k].weapon_1);
                    for (int i = 0; i < 8; i++)
                    {
                        if (temp[i, pos])
                        {
                            s += "1";
                            can_end = false;
                        }
                        else
                        {
                            s += "0";
                        }
                    }
                    pos = getCurTileGrid().mechs[k].myTile.location.getX();
                    for (int i = 0; i < 8; i++)
                    {
                        if (temp[i, pos])
                        {
                            s += "1";
                            can_end = false;
                        }
                        else
                        {
                            s += "0";
                        }
                    }
                }
                else
                {
                    s += "0000000000000000";
                }
            }
            for (int i = 0; i < 3; i++)
            {
                TileOccupant mech = getCurTileGrid().mechs[i];
                if (getCurTileGrid().actions_left[i+3] && !(mech.myTile.smoke || mech.health <= 0 || (mech.myTile.liquid && !mech.flying)) && !getCurTileGrid().actions_left[i])
                {
                    s += "1";
                    if (!mech.frozen)
                    {
                        can_end = false;
                    }
                }
                else
                {
                    s += "0";
                }
            }
            if (cur_squad != 2)
            {
                s += "0";
            }
            else
            {
                s += "1";
            }
        }

        if(can_end && (cur_turn != 1 || getCurTileGrid().mechs.Count == 3))
        {
            s += "1";
        }
        else
        {
            s += "0";
        }
        return s;
    }

    private bool checkReasonableAttack(Weapon weapon,int x, int y) //Help weed out bad actions. Only programmed to consider rift walkers. (This is the only thing programmed without generality in mind)
    {
        if(!optimal_actions)
        {
            return true;
        }
        if (getTile(x, y).occupied)
        {
            if (getTile(x, y).occupant.allignment == 2 && weapon.targetEffects.Count == 1)//You never want to attack only a building. I have 200 hours in this game and have never had to directly attack a building.
            {
                return false;
            }
            if (getTile(x, y).occupant.allignment != 2)
            {
                //print("Returning true because occupant on tile " + x + "," + y);
                return true;
            } 
        }
        if (getTile(x, y).forest || getTile(x, y).desert ||  getTile(x, y).frozen > 0)
        {
            //print("Returning true because changeable on tile " + x + "," + y);
            return true;
        }
        if (weapon.targetEffects.Count > 1)
        {
            Coordinate center = new Coordinate(x, y);
            for(int i = 0;i<4;i++)
            {
                Coordinate rel = center.addCoord(unitCoords[i]);
                if(!rel.outOfBounds())
                {
                    if(getTile(rel).occupied && !getTile(rel).occupant.stable)
                    {
                        //print("Returning true because unstable occupant on tile " + rel.getX() + "," + rel.getY() + "next to tile" + x + "," + y);
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public string writeToFileReduced()
    {//Send ver starts at 0
        string s = "";//e for environment, a for action
        int i, j = 0;
        //scorePosition();
        /*for (i = 0; i < 6; i++)//Score
        {
            s += getCurTileGrid().action_score[i] + ",";
        }*/
        //s += getCurTileGrid().score - getCurTileGrid().prev_score; 
        for (i = 0; i < 3; i++)//Actions available
        {
            if (getCurTileGrid().actions_left[i + 3])
            {
                s += "1,";
                if (getCurTileGrid().actions_left[i])
                {
                    s += "1,";
                }
                else
                {
                    s += "0,";
                }
            }
            else
            {
                s += "0,0,";
            }
        }
        s += getCurTileGrid().grid_health + ",";
        //s += Mathf.Floor(resist_chance / 10) + ",";
        s += cur_turn + ",";
        //6
        /*if (getCurTileGrid().perfect_bonus)
        {
            s += 1 + ",";
        }
        else
        {
            s += 0 + ",";
        }*/
        /*if (cur_mission == 0)//Vek kills (7), blocked spawns(3), grid damage(3), mech damage(4)
        {
            s += getCurTileGrid().mission_counters[0] + ",";
            s += -1 + ",";
            s += -1 + ",";
            s += -1 + ",";
        }
        else if (cur_mission == 1)
        {
            s += -1 + ",";
            s += getCurTileGrid().mission_counters[1] + ",";
            s += -1 + ",";
            s += -1 + ",";
        }
        else if (cur_mission == 2)
        {
            s += -1 + ",";
            s += -1 + ",";
            s += getCurTileGrid().mission_counters[2] + ",";
            s += -1 + ",";
        }
        else if (cur_mission == 3)
        {
            s += -1 + ",";
            s += -1 + ",";
            s += -1 + ",";
            int hp_loss = 0;
            for (i = 0; i < getCurTileGrid().mechs.Count; i++)
            {
                TileOccupant mech = getCurTileGrid().mechs[i];
                hp_loss += mech.max_health - mech.health;
            }
            s += hp_loss + ",";
        }*/
        for (i = 0; i < getCurTileGrid().mechs.Count; i++)
        {
            TileOccupant mech = getCurTileGrid().mechs[i];
            if (mech.health > 0)
            {
                s += "1,";
            }
            else
            {
                s += "0,";
            }
            if (!mech.chasm_dead)
            {
                s += mech.myTile.location.getX() + ",";
                s += mech.myTile.location.getY() + ",";
            }
            else
            {
                s += "-1,-1,";
            }
            s += mech.getMovement() + ",";
        }
        for (; i < 3; i++)
        {
            s += "0,-1,-1,0,";
        }
        
        
        //Psions
        updateVekNumbers();
        int vek_checked = 0;
        for (i = 0; i < getAttackOrder().Count; i++)//Add psion first
        {
            TileOccupant vek = getAttackOrder()[i];
            if (vek.unit_variant >= 12)
            {
                if(vek.unit_variant == 12) 
                {
                    s += "1,0,0,0,";
                }
                else if(vek.unit_variant == 13)
                {
                    s += "0,1,0,0,";
                }
                else if(vek.unit_variant == 14)
                {
                    s += "0,0,1,0,";
                }
                else
                {
                    s += "0,0,0,1,";
                }
                s += vek.myTile.location.getX() + "," + vek.myTile.location.getY() + ",";
                vek_checked = 1;
                break;
            }
        }
        if(vek_checked != 1)
        {
            s += "0,0,0,0,-1,-1,";
        }
        //Other vek

        //Old
        /*for (i = 0;i<5;i++)
        {
            for(j = 0;j<getAttackOrder().Count;j++)
            {
                if(getAttackOrder()[j].unit_variant == i)
                {
                    TileOccupant vek = getAttackOrder()[j];
                    s += "1,";
                    s += vek.myTile.location.getX() + "," + vek.myTile.location.getY() + ",";
                    if (!vek.attack_cancelled)
                    {
                        s += vek.targetcoord.getX() + ",";
                        s += vek.targetcoord.getY() + ",";
                    }
                    else
                    {
                        s += "-1,-1,";
                    }
                    if (vek.alpha == 1)
                    {
                        s += "1,";
                    }
                    else
                    {
                        s += "0,";
                    }
                }
                else
                {
                    s += "0,-1,-1,-1,-1,0,";
                }
            }
            for(;j<6;j++)
            {
                s += "0,-1,-1,-1,-1,0,";
            }
        }*/
        vek_checked = 0;
        for (i = 0; i < getAttackOrder().Count; i++)
        {
            TileOccupant vek = getAttackOrder()[i];
            if (vek.unit_variant >= 12)
            {
                continue;
            }
            for(j = 0;j<5;j++)
            {
                if(vek.unit_variant == j)
                {
                    s += "1,";
                }
                else
                {
                    s += "0,";
                }
            }
            
            if(vek.weapon_1 != null)
            {
                s += vek.weapon_1.targetEffects[0].damage + ",";
            }
            else
            {
                s += "0,";
            }
            s += vek.myTile.location.getX() + "," + vek.myTile.location.getY() + ",";
            if (!vek.attack_cancelled)
            {
                s += vek.targetcoord.addCoord(vek.myTile.location).getX() + ",";
                s += vek.targetcoord.addCoord(vek.myTile.location).getY()  + ",";
                //s += vek.myTile.location.getX() + ",";
                //s += vek.myTile.location.getY()  + ",";
            }
            else
            {
                s += "-1,-1,";
            }
            vek_checked++;
        }
        for (; vek_checked < 6; vek_checked++)
        {
            s += "0,0,0,0,0,0,-1,-1,-1,-1,";
        }

        //Mission
        if (getCurTileGrid().objective1 == null)
        {
            s += "-1,-1,";
        }
        else
        {
            s += getCurTileGrid().objective1.location.getX() + "," + getCurTileGrid().objective1.location.getY() + ",";
        }
        //Grid
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                if (getTile(i, j).occupied && getTile(i, j).occupant.allignment == 2)
                {
                    s += "1,";
                }
                else
                {
                    s += "0,";
                }
            }
        }
        //hp
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                Tile t = getTile(i, j);
                if (t.occupied)
                {
                    s += t.occupant.health + ",";
                }
                else
                {
                    s += "0,";
                }
            }
        }

        //Possible spawns ?
        bool[] can_spawn = new bool[16];
        for(i = 0;i<16;i++)
        {
            can_spawn[i] = false;
        }
        for(i=1;i<island_spawns.Count;i++)
        {
            if(island_spawns[i] > 4)
            {
                break;
            }
            can_spawn[island_spawns[i]] = true; 

        }
        for(i=0;i<weak_remaining.Count;i++)
        {
            if(weak_remaining[i] >= 12)
            {
                can_spawn[weak_remaining[i]] = true;
            }
        }
        for (i = 0; i < strong_remaining.Count; i++)
        {
            can_spawn[strong_remaining[i]] = true;
        }
        for (i = 0; i < 16; i++)
        {
            if(can_spawn[i])
            {
                s += "1,";
            }
            else
            {
                s += "0,";
            }
        }
        //Spawns
        for (i = 0; i < spawn_tiles.Count && i < 3; i++)
        {
            s += "1,";
            s += spawn_tiles[i].getX() + ",";
            s += spawn_tiles[i].getY() + ",";
        }
        for (; i < 6; i++)
        {
            s += "0,-1,-1,";
        }
        //Pod
        if (getCurTileGrid().pod_destroyed < 1)
        {
            s += "-1,-1,";
        }
        else
        {
            bool pod_collected = true;
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    if (getTile(i, j).pod)
                    {
                        s += i + "," + j + ",";
                        pod_collected = false;
                        break;
                    }
                }
                if (!pod_collected)
                {
                    break;
                }
            }
            if (pod_collected)
            {
                s += "-1,-1,";
            }
        }
        //Tile stats
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                Tile t = getTile(i, j);
                if (t.liquid || t.chasm)
                {
                    s += "1,";
                }
                else
                {
                    s += "0,";
                }
            }
        }
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                s += Mathf.Min(7,tile_threat[i, j]) + ",";
            }
        }
        return s;
    }

    public string writeToFileAlt()
    {//Send ver starts at 0
        return writeToFileReduced();
    }

    //--------------------------------------------------------------------
    //Functions for tree search. Tree search is too deep to handle quickly, so only depth 1-2
    //--------------------------------------------------------------------

    private List<Vector2Int> best_chain = new List<Vector2Int>();//These vars are irrelevant sadly
    private List<Vector2Int> cur_chain = new List<Vector2Int>(); 
    private int cur_top;
    private List<Vector2Int> getTopActions()
    {
        getValidActions();
        int max = -20000000;
        int min = 20000000;
        List<Vector2Int> top = new List<Vector2Int>();
        if (getCurTileGrid().valid_actions.Count == 0)
        {
            top.Add(new Vector2Int(244, scorePosition()));
            return top;
        }
        cur_top = -20000000;
        for (int i = 0; i < getCurTileGrid().valid_actions.Count; i++)
        {
            cur_chain.Clear();
            for(int j = 0;j<6;j++)
            {
                cur_chain.Add(new Vector2Int(-1,-1));
            }

            int score = getTopAction(getCurTileGrid().valid_actions[i],0);
            if(score > max)
            {
                max = score;
                best_chain.Clear();
                for(int j = 0;j<cur_chain.Count;j++)
                {
                    best_chain.Add(cur_chain[j]);
                }
            }
            if(top.Count < 10)
            {
                top.Add(new Vector2Int(getCurTileGrid().valid_actions[i],score));
                if(score < min)
                {
                    min = score;
                }
            }
            else
            {
                if (score > min)
                {
                    for (int j = 0; j < top.Count; j++)//Remove worst
                    {
                        Vector2Int worst = top[j];
                        if (worst.getY() == min)
                        {
                            top.Remove(worst);
                            break;
                        }
                    }
                    top.Add(new Vector2Int(getCurTileGrid().valid_actions[i], score));
                    min = 20000000;
                    for (int j = 0; j < top.Count; j++)
                    {
                        if (top[j].getY() < min)
                        {
                            min = top[j].getY();
                        }
                    }
                }

            }
        }
        List<Vector2Int> final = new List<Vector2Int>();
        for (int i = 0;i<top.Count;i++)//Put best at front
        {
            Vector2Int best = top[i];
            if(best.getY() == max)
            {
                top.Remove(best);
                //print("Added top");
                //print(best.getX() + " " + best.getY());
                final.Add(best);
                for(int j = 0;j<top.Count;j++)
                {
                    //print("Adding rest");
                    //print(top[j].getX() + " " + top[j].getY());
                    final.Add(top[j]);
                }
                break;
            }
        }
        return final;
    }

    private int getTopAction(int action, int depth)
    {
        cur_grid++;
        tile_grids[cur_grid].Steal(tile_grids[cur_grid - 1]);
        int top = -20000000;
        if (action < 192) //Truncated tree search finds best immediate attack after a move
        {
            Action(action);
            getValidAttacks();
            int m = action / 64;
            if (m == 0)
            {
                getCurTileGrid().actions_left[4] = false;
                getCurTileGrid().actions_left[5] = false;
            }
            else if (m == 1)
            {
                getCurTileGrid().actions_left[3] = false;
                getCurTileGrid().actions_left[5] = false;
            }
            else if (m == 2)
            {
                getCurTileGrid().actions_left[3] = false;
                getCurTileGrid().actions_left[4] = false;
            }
            if (getCurTileGrid().valid_actions.Count == 0)
            {
                int score = scorePosition();
                cur_grid--;
                return score;
            }
            for (int i = 0; i < getCurTileGrid().valid_actions.Count; i++)
            {
                int temp = getCurTileGrid().valid_actions[i];
                cur_grid++;
                tile_grids[cur_grid].Steal(tile_grids[cur_grid - 1]);
                Action(temp);
                score = scorePosition();
                //print("Move: " + action);
               //print("Action " + temp);
               // print("Score:" + score);
                cur_grid--;
                if (score > top)
                {
                    top = score;
                }
            }
            cur_grid--;
            return top;

        }
        Action(action);
        //getValidActions()
        if (getCurTileGrid().valid_actions.Count == 0 || depth >= 0)
        {
            int score = scorePosition();
            //print("Action " + action);
            //print("Score:" + score);
            cur_grid--;
            return score;
        }
        //print("Oh no");
        //Unreachable
        for (int i = 0; i < getCurTileGrid().valid_actions.Count; i++)
        {
            int temp = getTopAction(getCurTileGrid().valid_actions[i],depth + 1);
            if(temp > top)
            {
                if(temp > cur_top)
                {
                    cur_chain[depth] = new Vector2Int(getCurTileGrid().valid_actions[i],temp);
                    cur_top = temp;
                };
                top = temp;
            }
        }
        cur_grid--;
        return top;
    }

    private int addLegalAction(bool b, int i)
    {
        if(b)
        {
            getCurTileGrid().valid_actions.Add(i);
        }
        return i + 1;
    }

    private void getValidActions()
    {
        getCurTileGrid().valid_actions.Clear();
        int cur = 0;
        if (cur_turn == 1)
        {
            for (int k = 0; k < 3; k++)
            {
                if (getCurTileGrid().actions_left[k])
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (i == 7 || i == 0 || j == 7 || j < 4 || getTile(i, j).occupied)
                            {
                                cur = addLegalAction(false, cur);
                            }
                            else
                            {
                                cur = addLegalAction(true, cur);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0;i<64;i++)
                    {
                        cur = addLegalAction(false, cur);
                    }
                }
            }
            for (int i = 0; i < 52; i++)
            {
                cur = addLegalAction(false, cur);
            }
        }
        else
        {
            for (int k = 0; k < 3; k++)
            {
                if (getCurTileGrid().actions_left[k])
                {
                    int[,] temp = getLegalMoveTiles(getCurTileGrid().mechs[k].myTile.location, getCurTileGrid().mechs[k]);
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (temp[i, j] == 1)
                            {
                                cur = addLegalAction(true, cur);
                            }
                            else
                            {
                                cur = addLegalAction(false, cur);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 64; i++)
                    {
                        cur = addLegalAction(false, cur);
                    }
                }
            }
            for (int k = 0; k < 3; k++)
            {
                if (getCurTileGrid().actions_left[k + 3] && !getCurTileGrid().actions_left[k])//Force movement before action
                {
                    //print("Action " + k);
                    int pos = getCurTileGrid().mechs[k].myTile.location.getY();
                    bool[,] temp = GetValidAttackPositionTiles(getCurTileGrid().mechs[k].myTile.location, getCurTileGrid().mechs[k].weapon_1);
                    for (int i = 0; i < 8; i++)
                    {
                        if (temp[i, pos] && checkReasonableAttack(getCurTileGrid().mechs[k].weapon_1, i,pos))
                        {
                            cur = addLegalAction(true, cur);
                        }
                        else
                        {
                            cur = addLegalAction(false, cur);
                        }
                    }
                    pos = getCurTileGrid().mechs[k].myTile.location.getX();
                    for (int i = 0; i < 8; i++)
                    {
                        if (temp[pos, i] && checkReasonableAttack(getCurTileGrid().mechs[k].weapon_1, pos, i))
                        {
                            cur = addLegalAction(true, cur);
                        }
                        else
                        {
                            cur = addLegalAction(false, cur);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 16; i++)
                    {
                        cur = addLegalAction(false, cur);
                    }
                }
            }
            for (int i = 0; i < 3; i++)
            {
                TileOccupant mech = getCurTileGrid().mechs[i];
                if (getCurTileGrid().actions_left[i + 3] && !(mech.myTile.smoke || mech.health <= 0 || (mech.myTile.liquid && !mech.flying)) && !getCurTileGrid().actions_left[i])
                {
                    cur = addLegalAction(true, cur);
                }
                else
                {
                    cur = addLegalAction(false, cur);
                }
            }
            if (cur_squad != 2)
            {
                cur = addLegalAction(false, cur);
            }
            else
            {
                cur = addLegalAction(true, cur);
            }
        }
    }

    private void getValidAttacks()
    {
        getCurTileGrid().valid_actions.Clear();
        int cur = 192;
        if (cur_turn == 1)
        {
            return;
        }
        else
        {
            for (int k = 0; k < 3; k++)
            {
                if (getCurTileGrid().actions_left[k + 3])
                {
                    //print("Action " + k);
                    int pos = getCurTileGrid().mechs[k].myTile.location.getY();
                    bool[,] temp = GetValidAttackPositionTiles(getCurTileGrid().mechs[k].myTile.location, getCurTileGrid().mechs[k].weapon_1);
                    for (int i = 0; i < 8; i++)
                    {
                        if (temp[i, pos] && checkReasonableAttack(getCurTileGrid().mechs[k].weapon_1, i, pos))
                        {
                            cur = addLegalAction(true, cur);
                        }
                        else
                        {
                            cur = addLegalAction(false, cur);
                        }
                    }
                    pos = getCurTileGrid().mechs[k].myTile.location.getX();
                    for (int i = 0; i < 8; i++)
                    {
                        if (temp[pos, i] && checkReasonableAttack(getCurTileGrid().mechs[k].weapon_1, pos, i))
                        {
                            cur = addLegalAction(true, cur);
                        }
                        else
                        {
                            cur = addLegalAction(false, cur);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < 16; i++)
                    {
                        cur = addLegalAction(false, cur);
                    }
                }
            }
            for (int i = 0; i < 3; i++)
            {
                TileOccupant mech = getCurTileGrid().mechs[i];
                if (getCurTileGrid().actions_left[i + 3] && !(mech.myTile.smoke || mech.health <= 0 || (mech.myTile.liquid && !mech.flying)))
                {
                    cur = addLegalAction(true, cur);
                }
                else
                {
                    cur = addLegalAction(false, cur);
                }
            }
            if (cur_squad != 2)
            {
                cur = addLegalAction(false, cur);
            }
            else
            {
                cur = addLegalAction(true, cur);
            }
        }
    }

    //--------------------------------------------------------------------
    //Functions for getting legal actions
    //--------------------------------------------------------------------


    public int[,] getLegalMoveTiles(Coordinate start, TileOccupant unit)
    {
        int i;
        int j;
        int moves = unit.getMovement();
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                legal_tiles[i, j] = 0;
                tile_dist[i, j] = 0;
            }
        }
        unit.checkWebbed(false);
        //print(start.getX());
        //print(start.getY());
        legal_tiles[start.getX(), start.getY()] = 1;
        tile_dist[start.getX(), start.getY()] = moves;
        if (unit.myTile.burrower_tile && getTile(unit.myTile.location).occupied)
        {
            legal_tiles[start.getX(), start.getY()] = 0;
        }
        if (unit.allignment == -1 && (unit.unit_variant == 4 || unit.unit_variant == 5))
        {
            //print("LeapBurrow");
            unit.flying = true;
        }
        if (moves > 0)
        {
            for (i = 0; i < 4; i++)
            {
                getLegalMovesR(start.addCoord(unitCoords[i]), moves, unit);
            }
        }
        if (unit.allignment == -1 && (unit.unit_variant == 4 || unit.unit_variant == 5))
        {
            unit.flying = false;
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    if (getTile(i, j).chasm || getTile(i, j).liquid)
                    {
                        legal_tiles[i, j] = 9;
                    }
                }
            }
        }
        return legal_tiles;
    }

    public List<Coordinate> getLegalMoves(Coordinate start, TileOccupant unit)//Find available moves a unit can make
    {
        int i;
        int j;
        getLegalMoveTiles(start,unit);
        List<Coordinate> legal = new List<Coordinate>();
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                if (legal_tiles[i, j] == 1)
                {
                    legal.Add(new Coordinate(i, j));
                }
            }
        }
        return legal;
    }

    private void getLegalMovesR(Coordinate cur, int moves_left, TileOccupant unit)
    {
        if (!cur.outOfBounds() && moves_left > tile_dist[cur.getX(), cur.getY()] && legal_tiles[cur.getX(), cur.getY()] != -1)
        {
            if (moves_left > 0)
            {
                if (getTile(cur).occupied)
                {
                    if (!unit.flying && !(getTile(cur).occupant.allignment == unit.allignment || (unit.pilot == 4 && getTile(cur).occupant.allignment == -1)))
                    {
                        legal_tiles[cur.getX(), cur.getY()] = 9;
                    }
                }
                else if (getTile(cur).chasm)
                {
                    if (!unit.flying)
                    {
                        legal_tiles[cur.getX(), cur.getY()] = 9;
                    }
                    else
                    {
                        legal_tiles[cur.getX(), cur.getY()] = 1;
                    }
                }
                else if (getTile(cur).liquid)
                {
                    if (unit.flying || unit.massive)
                    {
                        legal_tiles[cur.getX(), cur.getY()] = 1;
                    }
                    else
                    {
                        legal_tiles[cur.getX(), cur.getY()] = 9;
                    }
                }
                else
                {
                    legal_tiles[cur.getX(), cur.getY()] = 1;
                }
                if (legal_tiles[cur.getX(), cur.getY()] != 9)
                {
                    tile_dist[cur.getX(), cur.getY()] = moves_left;
                    for (int j = 0; j < 4; j++)
                    {
                        getLegalMovesR(cur.addCoord(unitCoords[j]), moves_left - 1, unit);
                    }
                }
            }
        }
    }

    public void Move(TileOccupant unit, Tile location)
    {
        unit.myTile.SwapOccupant(location);
    }

    public bool[,] GetValidAttackPositionTiles(Coordinate source, Weapon weapon)
    {
        int i = 0;
        int j;

        bool[,] valid_tiles = new bool[8, 8];
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                valid_tiles[i, j] = false;
            }
        }
        if (weapon == null)
        {
            return valid_tiles;
        }
        if (getTile(source).smoke || getTile(source).occupant.frozen || (getTile(source).occupant.allignment == 1 && getTile(source).occupant.health <= 0) || (getTile(source).liquid && getTile(source).occupied && !getTile(source).occupant.flying))
        {
            return valid_tiles;
        }
        if (weapon.type == 1)
        {
            for (i = 0; i < 4; i++)
            {
                j = 0;
                Coordinate cur = source;
                if (weapon.max_range == 0)
                {
                    valid_tiles[source.getX(), source.getY()] = true;
                    break;
                }
                while(j < weapon.min_range)
                {
                    cur = cur.addCoord(unitCoords[i]);
                    j++;
                }
                while (!cur.outOfBounds() && j <= weapon.max_range)
                {
                    if (!((weapon.reposition) && getTile(cur).occupied))
                    {
                        valid_tiles[cur.getX(), cur.getY()] = true;
                    }
                    cur = cur.addCoord(unitCoords[i]);
                    j++;
                }
            }
        }
        else if (weapon.type == 2)
        {
            for (i = 0; i < 4; i++)
            {
                Coordinate cur = source.addCoord(unitCoords[i]);
                if (cur.outOfBounds())
                {
                    continue;
                }
                Coordinate prev = cur;
                bool beetle_die = false;
                while (!cur.outOfBounds() && !getTile(cur).occupied)
                {
                    prev = cur;
                    if ((getTile(prev).liquid || getTile(prev).chasm))
                    {
                        beetle_die = true;
                    }
                    cur = cur.addCoord(unitCoords[i]);
                }
                if (cur.outOfBounds())
                {
                    cur = prev;
                }
                if (beetle_die && weapon.name.Equals("Ram"))
                {
                    continue;
                }
                valid_tiles[cur.getX(), cur.getY()] = true;
            }
        }
        if (weapon.type == 5)
        {
            for (i = 0; i < 4; i++)
            {
                j = weapon.min_range;
                Coordinate cur = source;
                if (weapon.max_range == 0)
                {
                    valid_tiles[source.getX(), source.getY()] = true;
                    break;
                }
                cur = cur.addCoord(unitCoords[i]);
                while (!cur.outOfBounds() && j <= weapon.max_range)
                {
                    if (!(getTile(cur).occupied || getTile(cur).fire || getTile(cur).liquid || getTile(cur).chasm || getTile(cur).smoke))
                    {
                        valid_tiles[cur.getX(), cur.getY()] = true;
                    }
                    cur = cur.addCoord(unitCoords[i]);
                    j++;
                }
            }
        }
        else if (weapon.type == 3)
        {
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    valid_tiles[i, j] = true;
                }
            }
        }
        else if (weapon.type == 4)
        {
            for (i = 0; i < 4; i++)
            {
                if (getTile(source.addCoord(unitCoords[i])).occupied && !getTile(source.addCoord(unitCoords[(i + 2) % 4])).occupied)
                {
                    valid_tiles[source.addCoord(unitCoords[i]).getX(), source.addCoord(unitCoords[i]).getY()] = true;
                }
            }
        }
        return valid_tiles;
    }

    public List<Coordinate> GetValidAttackPositions(Coordinate source, Weapon weapon)
    {
        bool[,] valid_tiles = GetValidAttackPositionTiles(source, weapon);
        List<Coordinate> legal = new List<Coordinate>();
        int i, j;
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                if (valid_tiles[i,j])
                {
                    legal.Add(new Coordinate(i, j));
                }
            }
        }
        return legal;
    }

    //--------------------------------------------------------------------
    //Functions for completing attack actions
    //--------------------------------------------------------------------


    bool[,] electro = new bool[8, 8]; //(unused)
    private void ElectricWhip(Coordinate source, Coordinate target, bool chain)
    {
        for (int i = 0; i < 4; i++)
        {
            Coordinate cur = target.addCoord(unitCoords[i]);
            if (cur != source && electro[cur.getX(), cur.getY()] == false && getTile(cur).occupied && ((chain && getTile(cur).occupant.allignment == 2) || getTile(cur).occupant.penetrable))
            {
                electro[cur.getX(), cur.getY()] = true;
                ElectricWhip(source, cur, chain);
            }
        }
    }
    public void MakeAttack(Coordinate source, Coordinate target, Weapon weapon)
    {
        int i;
        int rotation = source.getDirection(target);
        TileOccupant occ = getTile(source).occupant;
        /*if (getTile(source).occupant.allignment == -1)
        {
            //print("Attacking unit " + getTile(source).occupant.number);
        }*/
        explode_queue.Clear();
        /*if (occ.allignment == -1)
        {
            rotation = 0;
        }*/
        //print("Attack cancelled?");
        //print(target.getX() + "," + target.getY());
        if (!occ.attack_cancelled)
        {
            //print("Attacking!");
            if (weapon.variableEffects != null && weapon.variableEffects.Count > 0)//Generate custom effects
            {
                List<TileEffects> effects = new List<TileEffects>();

                if (weapon.name == "Electric Whip")//OoooOoO you just have to be special
                {
                    for (i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            electro[i, j] = false;
                        }
                    }
                    ElectricWhip(source, target, weapon.immune > 0);
                    for (i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (electro[i, j])
                            {
                                effects.Add(weapon.variableEffects[1].Clone(new Coordinate(i, j)));
                            }
                        }
                    }
                }
                else
                {
                    if (weapon.type == 1)
                    {
                        Coordinate unit = target.subtractCoord(source).getUnitVector();
                        Coordinate cur = unit.addCoord(source);
                        while (!cur.Equals(target))
                        {
                            effects.Add(weapon.variableEffects[0].Clone(cur));
                            cur = cur.addCoord(unit);
                        }
                        if (!weapon.reposition)
                        {
                            effects.Add(weapon.variableEffects[1].Clone(cur));
                        }
                    }
                    else if (weapon.type == 2 && weapon.reposition)//Beetle boss
                    {
                        Coordinate unit = target.subtractCoord(source).getUnitVector();
                        Coordinate cur = source;
                        Coordinate next = cur.addCoord(unit);
                        while (!next.Equals(target))
                        {
                            effects.Add(weapon.variableEffects[0].Clone(cur));
                            cur = cur.addCoord(unit);
                            next = cur.addCoord(unit);
                        }
                        effects.Add(weapon.variableEffects[1].Clone(next));
                    }
                }
                weapon.targetEffects = effects;

            }
            if (weapon.reposition)
            {
                if (weapon.type == 1)
                {
                    getTile(source).SwapOccupant(getTile(target));
                }
                Coordinate unit = target.subtractCoord(source).getUnitVector();
                Coordinate cur = unit.addCoord(source);
                if (weapon.type == 2 && weapon.name != "Ramming Speed" && !getTile(cur).occupied)
                {
                    Coordinate prev = cur;
                    while (!cur.outOfBounds() && !getTile(cur).occupied)
                    {
                        if (getTile(cur).liquid || getTile(cur).chasm)
                        {
                            if (occ.allignment == -1 && occ.unit_variant == 5)
                            {
                                //print(cur.getX());
                                //print(cur.getY());
                                getTile(source).SwapOccupant(getTile(cur));
                                //print("Dead");
                                occ.die();
                                CheckExplode();
                                return;
                            }
                        }
                        prev = cur;
                        cur = cur.addCoord(unit);
                    }
                    if(cur.outOfBounds())
                    {
                        cur = prev;
                    }

                    //print("Swippy");
                    getTile(source).SwapOccupant(getTile(cur));
                }
            }
            //print(weapon.name);
            for (i = 0; i < weapon.targetEffects.Count; i++)
            {
                if (target.addCoord(weapon.targetEffects[i].relative_pos.rotate(rotation)).outOfBounds())
                {
                    continue;
                }
                Tile current = getTile(target.addCoord(weapon.targetEffects[i].relative_pos.rotate(rotation)));
                if (!(current.occupant.allignment == weapon.immune))
                {
                    ApplyAttackEffects(weapon.targetEffects[i], current, source, rotation, weapon);
                }
                if (!current.occupant.penetrable && weapon.name.Contains("Beam"))
                {
                    break;
                }
            }
            for (i = 0; i < weapon.selfEffects.Count; i++)
            {
                //print("Out of bounds?");
                if (target.addCoord(weapon.selfEffects[i].relative_pos.rotate(rotation)).outOfBounds())
                {
                    continue;
                }
                //print("Doin it!");
                Tile current = getTile(source.addCoord(weapon.selfEffects[i].relative_pos.rotate(rotation)));
                ApplyAttackEffects(weapon.selfEffects[i], current, null, rotation, weapon);
            }
            if (weapon.name == "Ramming Speed") //Yeah I wanna apply my self effect before I reposition I'm so cool
            {
                Coordinate unit = target.subtractCoord(source).getUnitVector();
                Coordinate dest = unit.addCoord(source);
                Coordinate cur = unit.addCoord(source);
                if (!getTile(cur).occupied)
                {
                    while (!cur.outOfBounds() && !getTile(cur).occupied)
                    {
                        dest = cur;
                        cur = cur.addCoord(unit);
                    }
                    getTile(source).SwapOccupant(getTile(dest));
                }
            }
            /*for (i = 0; i < getAttackOrder().Count; i++)
            {
                //print(i);
                //getAttackOrder()[i].updateTarget(); 
            }*/
            occ.cancelAttack();
        }
        //else
        //{
        //    print("Attack cancelled?");
        //}
        CheckExplode();
    }
    private void CheckExplode()//Compute blast psion effects after each attack
    {
        if (explode_queue.Count > 0)
        {
            while (explode_queue.Count > 0 && getCurTileGrid().psion_active == 3)
            {
                Coordinate explode = explode_queue.Dequeue();
                getTile(explode).addDamage();
                //print("Explosion!");
                for (int i = 0; i < 4; i++)
                {
                    if (explode.addCoord(unitCoords[i]).outOfBounds())
                    {
                        continue;
                    }
                    Tile affected = getTile(explode.addCoord(unitCoords[i]));
                    tile_threat[affected.location.getX(), affected.location.getY()] += 1;
                    if (affected.occupied)
                    {
                        affected.occupant.bumpDamage(1);
                    }
                    else
                    {
                        affected.addDamage();
                    }
                }
            }
        }
    }

    private void ApplyAttackEffects(TileEffects effects, Tile tile, Coordinate source, int rotation, Weapon wep)
    {
        //print("Made an attack on tile " + tile.location.getX() + "," + tile.location.getY());
        bool was_occupied = tile.occupied;
        TileOccupant occupant = tile.occupant;
        bool stab = occupant.stable;
        //print(occupant.unit_variant);
        //print(effects.spawntype);
        //print("Uwu");
        //print(effects.status);
        //print(effects.spawntype);
        if (effects.status == 6)
        {
            //print("Am die");
            tile.occupant.die();
        }

        if (tile.occupied)
        {
            if (enemy_turn && tile.occupant.allignment == -1)
            {
                tile.occupant.takeDamage(effects.damage + vek_bonus);
            }
            else if (wep.name == "Flame Thrower" && tile.occupant.fire)//OO I special
            {
                tile.occupant.takeDamage(2);
            }
            else
            {
                //print("Damaging!");
                tile.occupant.takeDamage(effects.damage);
            }
        }
        else
        {
            if (effects.damage > 0 && effects.spawntype == -1)
            {
                tile.addDamage();
            }
        }
        if (!tile.occupied && effects.spawntype > -1)
        {
            //print(tile.location.getX());
            //print(tile.location.getY());
            //print("Tryna spawn");
            //print("Am spawn!");
            SpawnUnit(tile.location, effects.spawntype, -1);
        }
        if (effects.status == 1)
        {
            tile.addFire();
        }
        else if (effects.status == 3)
        {
            tile.addIce();
        }
        else if (effects.status == 4)
        {
            tile.addShield();
        }
        else if (effects.status == 5)
        {
            tile.addSmoke();
        }
        else if (effects.status == 7)
        {
            tile.occupant.targetcoord = tile.occupant.targetcoord.rotate(2);
            tile.occupant.targetcoord2 = tile.occupant.targetcoord2.rotate(2);
        }
        tile_threat[tile.location.getX(), tile.location.getY()] += effects.damage;
        if (effects.status == 2)
        {
            if (was_occupied && occupant.health > 0)
            {
                occupant.addAcid();
            }
            else
            {
                tile.addAcid();
            }
        }
        if (effects.push_dir >= 0 && !stab)
        {
           // print(rotation);
            //print((effects.push_dir + rotation) % 4);
            ApplyPush(tile, (effects.push_dir + rotation) % 4,was_occupied);
        }
        else if (effects.relative_dest != null)
        {
            tile.SwapOccupant(getTile(source.addCoord(effects.relative_dest)));
        }
        tile.applyTileEffects();
    }

    private void ApplyPush(Tile source_tile, int direction,bool occ)
    {
        //print("tryna push");
       // print(source_tile.location.getX());
       // print(source_tile.location.getY());
       // print(direction);
        if (occ)
        {
            Coordinate baseVector = unitCoords[direction];
            Coordinate destination = new Coordinate(source_tile.location.getX(), source_tile.location.getY()).addCoord(baseVector);
            if (!destination.outOfBounds())
            {
                Tile dest_tile = getTile(destination);
                if (dest_tile.occupied)
                {
                    //print("Bumping");
                    source_tile.occupant.bumpDamage(bumpdamage);
                    dest_tile.occupant.bumpDamage(bumpdamage);
                    tile_threat[source_tile.location.getX(), source_tile.location.getY()] += 1;
                    tile_threat[dest_tile.location.getX(), dest_tile.location.getY()] += 1;
                }
                else
                {
                    source_tile.SwapOccupant(dest_tile);
                }
            }
        }
    }



    //--------------------------------------------------------------------
    //Functions for enemy behavior
    //--------------------------------------------------------------------


    public void EnemyTurn()
    {

        enemy_turn = true;
        int i = 0;
        int j = 0;
        explode_queue.Clear();
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                if (tile_grids[cur_grid].tile_grid[i, j].occupied)
                {
                    tile_grids[cur_grid].tile_grid[i, j].occupant.burnDamage();
                }
            }
        }
        CheckExplode();
        explode_queue.Clear();
        if (smokedamage > 0)
        {
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    if (tile_grids[cur_grid].tile_grid[i, j].smoke && tile_grids[cur_grid].tile_grid[i, j].occupied && tile_grids[cur_grid].tile_grid[i, j].occupant.allignment == -1)
                    {
                        tile_grids[cur_grid].tile_grid[i, j].occupant.electricDamage(smokedamage);
                    }
                }
            }
        }
        CheckExplode();
        if (getCurTileGrid().psion_active == 2)
        {
            for (i = 0; i < getAttackOrder().Count; i++)
            {
                if (getAttackOrder()[i].unit_variant < 12)
                {
                    getAttackOrder()[i].Heal();
                }
            }
        }
        {
            //Ignore stage hazards for time being
            /*if (stage_hazard > -5)
            {
                if (stage_hazard < 0)
                {
                    for (i = 0; i < 8; i++)
                    {
                        for (j = 0; j < 8; j++)
                        {
                            if (tile_grids[cur_grid].tile_grid[i, j].hazardous)
                            {
                                if (tile_grids[cur_grid].tile_grid[i, j].occupied)
                                {
                                    tile_grids[cur_grid].tile_grid[i, j].occupant.die();
                                }
                                tile_grids[cur_grid].tile_grid[i, j].addDamage();
                            }
                        }
                    }
                }
                if (stage_hazard == 1)
                {
                    for (i = 0; i < 8; i++)
                    {
                        for (j = 0; j < 8; j++)
                        {
                            if (tile_grids[cur_grid].tile_grid[i, j].hazardous)
                            {
                                if (tile_grids[cur_grid].tile_grid[i, j].occupied)
                                {
                                    tile_grids[cur_grid].tile_grid[i, j].occupant.addFrozen();
                                }
                                tile_grids[cur_grid].tile_grid[i, j].addIce();
                            }
                        }
                    }
                }
                else if (stage_hazard == 2)
                {
                    for (i = 0; i < 8; i++)
                    {

                        for (j = 0; j < 8; j++)
                        {
                            if (tile_grids[cur_grid].tile_grid[i, j].hazardous)
                            {
                                tile_grids[cur_grid].tile_grid[i, j].addWater();
                            }
                        }
                    }
                }
                else if (stage_hazard == 3)
                {
                    for (i = 0; i < 8; i++)
                    {
                        for (j = 0; j < 8; j++)
                        {
                            if (tile_grids[cur_grid].tile_grid[i, j].hazardous)
                            {
                                tile_grids[cur_grid].tile_grid[i, j].addChasm();
                            }
                        }
                    }
                }
                else if (Mathf.Abs(stage_hazard) == 4)
                {
                    for (i = 0; i < 8; i++)
                    {
                        for (j = 0; j < 8; j++)
                        {
                            if (tile_grids[cur_grid].tile_grid[i, j].hazardous)
                            {
                                tile_grids[cur_grid].tile_grid[i, j].addLava();
                            }
                        }
                    }
                }
                else if (stage_hazard == -2)
                {
                    for (i = 0; i < 8; i++)
                    {
                        for (j = 0; j < 8; j++)
                        {
                            if (tile_grids[cur_grid].tile_grid[i, j].hazardous)
                            {
                                tile_grids[cur_grid].tile_grid[i, j].rockFall();
                            }
                        }
                    }
                }
                else if (stage_hazard == -3)
                {
                    for (i = 0; i < 8; i++)
                    {
                        for (j = 0; j < 8; j++)
                        {
                            if (tile_grids[cur_grid].tile_grid[i, j].hazardous)
                            {
                                tile_grids[cur_grid].tile_grid[i, j].addFire();
                            }
                        }
                    }
                }
            }
            */
        }
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                tile_grids[cur_grid].tile_grid[i, j].applyTileEffects();
            }
        }
        //print("Grid");
        //print(cur_grid);
        for (i = 0; i < getAttackOrder().Count; i++)
        {
            // print(i);
            // print(!getAttackOrder()[i].attack_cancelled);
            // print(getAttackOrder()[i].weapon_1 != null);
            getAttackOrder()[i].updateTarget();
            if (!getAttackOrder()[i].attack_cancelled && getAttackOrder()[i].weapon_1 != null && !getAttackOrder()[i].targetcoord.Equals(new Coordinate(9, 9)))
            {
                //  print("Attacking");
                //    print(getAttackOrder()[i].unit_name);
                MakeAttack(getAttackOrder()[i].myTile.location, getAttackOrder()[i].targetcoord.addCoord(getAttackOrder()[i].myTile.location), getAttackOrder()[i].weapon_1);
                //if (!getAttackOrder()[i].targetcoord2.Equals(new Coordinate(9, 9)))
                //{
                //    MakeAttack(getAttackOrder()[i].myTile.location, getAttackOrder()[i].targetcoord2.addCoord(getAttackOrder()[i].myTile.location), getAttackOrder()[i].weapon_1);
                //}
            }
        }
        if (cur_grid == 0 && cur_turn != 5)
        {
            //score_store2 = score_store;
            //score_store = getCurTileGrid().prev_score;
            //print("Scores updates");
            //print(cur_turn);
            //print(score_store);
            //print(score_store2);
            SpawnsEmerge();
            EnemyActions();
            for (i = 0; i < getCurTileGrid().mechs.Count; i++)
            {
                getCurTileGrid().mechs[i].checkWebbed(true);
            }
        }
        enemy_turn = false;
        updateSprites();
    }

    private int[,] proximitygrid = new int[8, 8];
    private int[,] proximitygrid2 = new int[8, 8]; //Flying or blocker ignoring units
    private void GenerateProxGrid() //Movement availability for melee enemies
    {
        List<Coordinate> working = new List<Coordinate>();
        List<Coordinate> working2 = new List<Coordinate>();
        int i = 0;
        int j = 0;
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                if (getTile(i, j).occupant.allignment > 0)
                {
                    proximitygrid2[i, j] = 6;
                    proximitygrid[i, j] = 6;
                    working.Add(new Coordinate(i, j));
                }
                else
                {
                    proximitygrid2[i, j] = 0;
                    proximitygrid[i, j] = 0;
                }
            }
        }
        int cur_increment = 5;
        bool incremented = true;
        while (incremented)
        {
            incremented = false;
            for (i = 0; i < working.Count; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    Coordinate rel = working[i].addCoord(unitCoords[j]);
                    if (!rel.outOfBounds())
                    {
                        if((getTile(rel).occupied && getTile(rel).occupant.allignment >= 0) || getTile(rel).liquid || getTile(rel).chasm)
                        {

                        }
                        else if (cur_increment > proximitygrid[rel.getX(), rel.getY()])
                        {
                            working2.Add(rel);
                            incremented = true;
                            proximitygrid[rel.getX(), rel.getY()] = cur_increment;
                        }
                    }
                }
            }
            cur_increment = Mathf.Max(cur_increment-1,0);
            working.Clear();
            for (i = 0; i < working2.Count; i++)
            {
                working.Add(working2[i]);
            }
            working2.Clear();
        }

        //Flying enemies ignore imppassable terrain.
        working.Clear();
        working2.Clear();
        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                if (getTile(i, j).occupant.allignment > 0)
                {
                    working.Add(new Coordinate(i, j));
                }
            }
        }
        cur_increment = 5;
        incremented = true;
        while (incremented)
        {
            incremented = false;
            for (i = 0; i < working.Count; i++)
            {
                for (j = 0; j < 4; j++)
                {
                    Coordinate rel = working[i].addCoord(unitCoords[j]);
                    if (!rel.outOfBounds() && cur_increment > proximitygrid[rel.getX(), rel.getY()])
                    {
                        working2.Add(rel);
                        incremented = true;
                        proximitygrid2[rel.getX(), rel.getY()] = cur_increment;
                    }
                }
            }
            cur_increment = Mathf.Max(cur_increment - 1, 0);
            working.Clear();
            for (i = 0; i < working2.Count; i++)
            {
                working.Add(working2[i]);
            }
            working2.Clear();
        }


        for (i = 0; i < 8; i++)
        {
            //print(proximitygrid[i, 0] + "," + proximitygrid[i, 1] + "," + proximitygrid[i, 2] + "," + proximitygrid[i, 3] + "," + proximitygrid[i, 4] + "," + proximitygrid[i, 5] + "," + proximitygrid[i, 6] + "," + proximitygrid[i, 7]);
        }
    }

    private int ScorePositioning(Tile tile, TileOccupant enemy)
    {
        if (tile.pod)
        {
            return -100;
        }
        if (tile.targetted)
        {
            return -10;
        }
        if (tile.smoke)
        {
            return -2;
        }
        if (tile.fire && !enemy.fire)
        {
            return -10;
        }
        if (tile.spawn)
        {
            return -10;
        }
        if (tile.hazardous)
        {
            return -10;
        }
        if ((tile.location.getX() == 0 || tile.location.getX() == 7) && (tile.location.getY() == 0 || tile.location.getY() == 7))
        {
            return -2;
        }
        else if (tile.location.getX() == 0 || tile.location.getX() == 7 || tile.location.getY() == 0 || tile.location.getY() == 7)
        {
            return 0;
        }
        if (tile == enemy.myTile || tile.location.Equals(enemy.prev_location))
        {
            return 0;
        }
        if (enemy.weapon_1 != null && enemy.weapon_1.max_range < 2)
        {
            for(int i = 0;i<4;i++)
            {
                if(getTile(tile.location.addCoord(unitCoords[i])).occupant.allignment > 0)
                {
                    return 5;
                }
            }
            if (enemy.unit_variant == 1 || enemy.unit_variant == 4 || enemy.unit_variant == 5 || enemy.unit_variant >= 12)
            {
                return proximitygrid2[tile.location.getX(), tile.location.getY()];
            }
            else 
            {
                return proximitygrid[tile.location.getX(), tile.location.getY()];
            }
        }
        return 5;
    }


    private void EnemyActions()
    {
        log.enemy_actions = 1;
        GenerateProxGrid();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                getTile(i, j).targetted = false;
            }
        }
        for (int i = 0; i < getAttackOrder().Count; i++)
        {
            if (!getAttackOrder()[i].frozen)
            {
                //print("Number " + i);
                EnemyAction(getAttackOrder()[i]);
            }
        }
        for (int i = 7; i >= 0; i--)
        {
            //print(targetted[i, 7] + "," + targetted[i, 6] + "," + targetted[i, 5] + "," + targetted[i, 4] + "," + targetted[i, 3] + "," + targetted[i, 2] + "," + targetted[i, 1] + "," + targetted[i, 0]);
        }
        log.enemy_poss_per_turn += log.enemy_actions;
        updateSprites();
    }


    private void EnemyAction(TileOccupant enemy)
    {
        enemy.myTile.occupied = false;
        //print(enemy.number);
        //print("Variant is " + enemy.unit_variant + "Auto attack? " + enemy.auto_attack);
        Coordinate loc = enemy.myTile.location;
        List<Coordinate> legal_moves = getLegalMoves(loc, enemy);
        //print(legal_moves.Count);
        int best_score = -200;
        int next_score = -201;
        List<Coordinate> best_moves = new List<Coordinate>();
        List<Coordinate> best_targets = new List<Coordinate>();
        List<Coordinate> next_moves = new List<Coordinate>();
        List<Coordinate> next_targets = new List<Coordinate>();

        int best_pos_score = 0;
        int next_pos_score = -200;
        List<Coordinate> best_null_moves = new List<Coordinate>();
        List<Coordinate> next_null_moves = new List<Coordinate>();

        int pos_score = 0;
        int final_score = 0;
        int i, k, l;
        int building_pri = 5;

        if (enemy.auto_attack)
        {

            //print("Enemy" + enemy.number);
            if (!enemy.myTile.smoke)
            {
                enemy.attack_cancelled = false;
                enemy.targetcoord = new Coordinate(0, 0);
            }
            else
            {
                enemy.cancelAttack();
            }
            enemy.myTile.occupied = true;
            return;
        }
        else
        {
            if (enemy.unit_variant == 11)
            {
                building_pri = 3;
            }
            //print("This many moves");
            //print(legal_moves.Count);
            for (i = 0; i < legal_moves.Count; i++)
            {

                pos_score = ScorePositioning(getTile(legal_moves[i]), enemy);
                if (pos_score > -5)
                {
                    if (pos_score > best_pos_score)
                    {
                        //print("Pos " + legal_moves[i].getX() + "," + legal_moves[i].getY() + " beats " + best_score + " with score " + pos_score);
                        next_null_moves.Clear();
                        for (l = 0; l < best_null_moves.Count; l++)
                        {
                            next_null_moves.Add(best_null_moves[l]);
                        }
                        best_null_moves.Clear();
                        best_null_moves.Add(legal_moves[i]);
                        next_pos_score = best_pos_score;
                        best_pos_score = pos_score;
                    }
                    else if (pos_score == best_pos_score)
                    {
                        //print("Pos " + legal_moves[i].getX() + "," + legal_moves[i].getY() + " equals " + best_score + " with score " + pos_score);
                        best_null_moves.Add(legal_moves[i]);
                    }
                    else if (pos_score > next_pos_score)
                    {
                        //print("Pos " + legal_moves[i].getX() + "," + legal_moves[i].getY() + " beats " + next_score + " with score " + pos_score);
                        next_null_moves.Clear();
                        next_null_moves.Add(legal_moves[i]);
                        next_pos_score = pos_score;
                    }
                    else if (pos_score == next_pos_score)
                    {
                        //print("Pos " + legal_moves[i].getX() + "," + legal_moves[i].getY() + " equals " + next_score + " with score " + pos_score);
                        next_null_moves.Add(legal_moves[i]);
                    }
                    else
                    {
                        // print("Pos " + legal_moves[i].getX() + "," + legal_moves[i].getY() + " doesn't beat " + best_score + " with score " + pos_score);
                    }
                }
                if (getTile(legal_moves[i]).smoke)
                {
                    continue;
                }
                List<Coordinate> targets = GetValidAttackPositions(legal_moves[i], enemy.weapon_1);
                //print("This many targets: "+targets.Count);
                for (k = 0; k < targets.Count; k++)
                {
                    int target_score = 0;
                    if (k.Equals(enemy.prev_target))
                    {
                        target_score -= 5;
                    }
                    final_score = 0;
                    Coordinate target = targets[k];
                    if (getTile(target).occupied && enemy.unit_variant == 7 && getTile(target).occupant.allignment == -1)
                    {
                        target_score -= 10;
                    }
                    if (getTile(target).occupied &&(enemy.unit_variant == 10||enemy.unit_variant == 11))
                    {
                        continue;
                    }
                    for (l = 0; l < enemy.weapon_1.targetEffects.Count; l++)
                    {
                        Coordinate affected = target.addCoord(enemy.weapon_1.targetEffects[l].relative_pos.rotate(legal_moves[i].getDirection(target)));
                        if (affected.outOfBounds() || !getTile(affected).occupied)
                        {
                            continue;
                        }
                        if (getTile(affected).occupant.allignment == 1 || getTile(affected).occupant.allignment == -1 && getTile(affected).occupant.frozen && !getTile(affected).targetted)
                        {
                            target_score += 5;
                        }
                        else if (getTile(affected).occupant.allignment == 2)
                        {
                            target_score += building_pri;
                        }
                        else if (getTile(affected).occupant.allignment == -1)
                        {
                            target_score -= 2;
                        }
                        if (getTile(affected).pod || getTile(target).pod)
                        {
                            target_score = -100;
                            break;
                        }
                    }
                    for (l = 0; l < enemy.weapon_1.selfEffects.Count; l++)
                    {
                        Coordinate affected = target.addCoord(enemy.weapon_1.selfEffects[l].relative_pos.rotate(legal_moves[i].getDirection(target)));
                        if (affected.outOfBounds() || !getTile(affected).occupied)
                        {
                            continue;
                        }
                        if (getTile(affected).occupant.allignment == 1 || getTile(affected).occupant.allignment == -1 && getTile(affected).occupant.frozen && !getTile(affected).targetted)
                        {
                            target_score += 5;
                        }
                        else if (getTile(affected).occupant.allignment == 2)
                        {
                            target_score += building_pri;
                        }
                        else if (getTile(affected).occupant.allignment == -1)
                        {
                            target_score -= 2;
                        }
                        if (getTile(affected).pod)
                        {
                            target_score = -100;
                            break;
                        }
                    }
                    if(enemy.unit_variant == 10 && (target.getX() == 0 || target.getY() == 0 || target.getX() == 7 || target.getY() == 7))
                    {
                        target_score = -10;
                    }
                    final_score = target_score;
                    if (final_score <= 0)
                    {
                        continue;
                    }
                    if (pos_score <= -5)
                    {
                        final_score = pos_score;
                    }
                    else
                    {
                        final_score += pos_score;
                    }
                    if (final_score > best_score)
                    {
                        //print("Action attack " + target.getX() + "," + target.getY() + "on tile " + legal_moves[i].getX() + "," + legal_moves[i].getY() + " beats " + best_score + " with score " + final_score);
                        next_moves.Clear();
                        next_targets.Clear();
                        for (l = 0; l < best_moves.Count; l++)
                        {
                            next_moves.Add(best_moves[l]);
                            next_targets.Add(best_targets[l]);
                        }
                        best_moves.Clear();
                        best_targets.Clear();
                        best_moves.Add(legal_moves[i]);
                        best_targets.Add(target);
                        next_score = best_score;
                        best_score = final_score;
                    }
                    else if (final_score == best_score)
                    {
                        //print("Action attack " + target.getX() + "," + target.getY() + "on tile " + legal_moves[i].getX() + "," + legal_moves[i].getY() + " equals best " + best_score + " with score " + final_score);
                        best_moves.Add(legal_moves[i]);
                        best_targets.Add(target);
                    }
                    else if (final_score > next_score)
                    {
                        //print("Action attack " + target.getX() + "," + target.getY() + "on tile " + legal_moves[i].getX() + "," + legal_moves[i].getY() + " beats second best " + next_score + " with score " + final_score);
                        next_moves.Clear();
                        next_targets.Clear();
                        next_moves.Add(legal_moves[i]);
                        next_targets.Add(target);
                        next_score = final_score;
                    }
                    else if (final_score == next_score)
                    {
                        //print("Action attack " + target.getX() + "," + target.getY() + "on tile " + legal_moves[i].getX() + "," + legal_moves[i].getY() + " equals second best " + next_score + " with score " + final_score);
                        next_moves.Add(legal_moves[i]);
                        next_targets.Add(target);
                    }
                    else
                    {
                        //print("Action attack " + target.getX() + "," + target.getY() + " doesn't beat " + best_score + " with score " + final_score);
                    }
                    ////print("This many good targets " + (best_targets.Count) + " with score " + best_score);
                    //print("This many kinda good targets " + (next_targets.Count) + " with score " + next_score);
                }
            }
        }

        Coordinate move_choice;
        Coordinate target_choice;

        int dif = 10 - 10 / (best_score - next_score);
        int rand = UnityEngine.Random.Range(0, 10);
        log.enemy_actions *= (best_targets.Count + next_targets.Count);
        if ((rand > dif || best_targets.Count == 0) && next_targets.Count > 0)
        {
            rand = UnityEngine.Random.Range(0, next_moves.Count);
            move_choice = next_moves[rand];
            target_choice = next_targets[rand];
        }
        else if ((rand <= dif || next_targets.Count == 0) && best_targets.Count > 0)
        {
            rand = UnityEngine.Random.Range(0, best_moves.Count);
            move_choice = best_moves[rand];
            target_choice = best_targets[rand];
        }
        else
        {
            if ((rand <= dif || next_null_moves.Count == 0) && !(best_null_moves.Count == 0))
            {
                rand = UnityEngine.Random.Range(0, best_null_moves.Count);
                move_choice = best_null_moves[rand];
                target_choice = null;
            }
            else if ((rand > dif || best_null_moves.Count == 0) && !(next_null_moves.Count == 0))
            {
                rand = UnityEngine.Random.Range(0, next_null_moves.Count);
                move_choice = next_null_moves[rand];
                target_choice = null;
            }
            else
            {
                target_choice = null;
                move_choice = enemy.myTile.location;
            }
        }
        //print("Count");
        //print(best_targets.Count);
        enemy.myTile.occupied = true;
        /*print("Swapping");
        print(move_choice.getX());
        print(move_choice.getY());
        print(enemy.myTile.location.getX());
        print(enemy.myTile.location.getY());
        if (target_choice == null)
        {
            print("No attacks? ");
        }//
        else
        {
            //print("Targer");
            print(target_choice.getX());
            print(target_choice.getY());
        }*/


        enemy.myTile.SwapOccupantMovement(getTile(move_choice));
        if (target_choice != null && !getTile(move_choice).smoke)
        {
            //print("Wha");
            enemy.attack_cancelled = false;
            enemy.targetcoord = target_choice.subtractCoord(enemy.myTile.location);
            if (enemy.unit_variant == 11)
            {
                //print("Qia");
                SpawnUnit(target_choice, 3, -1);
                getTile(target_choice).occupant.webber_type = 2;
                enemy.cancelAttack();
            }
            else
            {
                if (enemy.unit_variant == 0 || enemy.unit_variant == 4)
                {
                    enemy.webber_type = 1;
                }
                else if (enemy.unit_variant == 10)
                {
                    if (enemy.max_health >= 4)
                    {
                        SpawnUnit(target_choice, 2, -1);
                        enemy.cancelAttack();
                    }
                    else
                    {
                        SpawnUnit(target_choice, 1, -1);
                        enemy.cancelAttack();
                    }
                }
                else if (enemy.unit_variant == 9)
                {
                    //print(""); print(""); print(""); print(""); print(""); print(""); print(""); print(""); print(""); print(""); print(""); print(""); print("");
                    for (i = 0; i < 4; i++)
                    {
                        //     print("Spawning rock on" + enemy.myTile.location.addCoord(unitCoords[i]).getX() + "," + enemy.myTile.location.addCoord(unitCoords[i]).getY());
                        if(!getTile(enemy.myTile.location.addCoord(unitCoords[i])).occupied && !getTile(enemy.myTile.location.addCoord(unitCoords[i])).pod)
                        SpawnUnit(enemy.myTile.location.addCoord(unitCoords[i]), 0, -1);
                    }
                }
                for (i = 0; i < enemy.weapon_1.targetEffects.Count; i++)
                {
                    Coordinate affected = target_choice.addCoord(enemy.weapon_1.targetEffects[i].relative_pos.rotate(enemy.myTile.location.getDirection(target_choice)));
                    if (!affected.outOfBounds())
                    {
                        getTile(affected).targetted = true;
                    }
                }
                for (i = 0; i < enemy.weapon_1.selfEffects.Count; i++)
                {
                    Coordinate affected = target_choice.addCoord(enemy.weapon_1.selfEffects[i].relative_pos.rotate(enemy.myTile.location.getDirection(target_choice)));
                    if (!affected.outOfBounds())
                    {
                        getTile(affected).targetted = true;
                    }
                }
                if (enemy.weapon_1.type == 2)
                {
                    Coordinate inc = unitCoords[enemy.myTile.location.getDirection(target_choice)];
                    Coordinate cur = enemy.myTile.location.addCoord(inc);

                    //  print(enemy.myTile.location.getDirection(target_choice));
                    while (!cur.Equals(target_choice) && !cur.outOfBounds())
                    {
                        //   print(cur.getX());
                        //  print(cur.getY());
                        getTile(cur).targetted = true;
                        cur = cur.addCoord(inc);
                    }
                }
            }
        }
        else
        {
            //print("Cancelled");
            enemy.cancelAttack();
        }

        if (enemy.attack_cancelled)
        {
            enemy.prev_target = new Coordinate(9, 9);
        }
        else
        {
            enemy.prev_target = enemy.targetcoord;
        }
        enemy.prev_location = enemy.myTile.location;
    }

    //--------------------------------------------------------------------
    //Functions for psions 
    //--------------------------------------------------------------------


    public void PsionEffect()
    {
        if (getCurTileGrid().psion_active == 4)
        {
            for (int i = 0; i < getAttackOrder().Count; i++)
            {
                if (getAttackOrder()[i].unit_variant < 12)
                {
                    getAttackOrder()[i].armoured = true;
                }
            }
        }
        else if(getCurTileGrid().psion_active == 1)
        {
            for (int i = 0; i < getAttackOrder().Count; i++)
            {
                if (getAttackOrder()[i].unit_variant < 12)
                {
                    getAttackOrder()[i].max_health++;
                    getAttackOrder()[i].health++;
                }
            }
        }
    }
    public void UndoPsionEffect()
    {
        if (getCurTileGrid().psion_active == 4)
        {
            for (int i = 0; i < getAttackOrder().Count; i++)
            {
                getAttackOrder()[i].armoured = false;
            }
        }
        else if (getCurTileGrid().psion_active == 1)
        {
            for (int i = 0; i < getAttackOrder().Count; i++)
            {
                if (getAttackOrder()[i].unit_variant < 12)
                {
                    getAttackOrder()[i].max_health--;
                    getAttackOrder()[i].health--;
                    if(getAttackOrder()[i].health <=0)
                    {
                        getAttackOrder()[i].die();
                    }
                }
            }
        }
    }
}
