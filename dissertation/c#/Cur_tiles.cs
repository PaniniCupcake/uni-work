using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cur_tiles : MonoBehaviour
{
    public int[] action_score = { 0, 0, 0, 0, 0, 0 };
    public int prev_score = 0;
    public int score = 0;
    public int critical_damage = 0; //keep track of number of losing grid hits
    public bool perfect_bonus;
    public int[] mission_counters = new int[4]; //Vek kills (7), blocked spawns(3), grid damage(3), mech damage(4)
    public Tile objective1;
    public Tile objective2;
    public int pod_destroyed = 0; // 0 for no pod, 1 for pod, -1 for destroyed pod.
    public List<TileOccupant> attack_order = new List<TileOccupant>();
    public List<TileOccupant> mechs = new List<TileOccupant>();
    public Tile[] hiddenBurrowers = new Tile[2];//There can only be two burrowers
    public Tile[,] tile_grid = new Tile[8, 8];
    public int grid_health;
    public bool[] actions_left = new bool[6];//Move move move attack attack attack
    public int psion_active = 0;
    public List<int> valid_actions = new List<int>();

    private void Start()
    {
        hiddenBurrowers[0].burrower_tile = true;
        hiddenBurrowers[1].burrower_tile = true;
    }

    //Copy other tile grid
    public void Steal(Cur_tiles t)
    {
        mission_counters = t.mission_counters;
        score = t.score;
        prev_score = t.prev_score;
        perfect_bonus = t.perfect_bonus;
        pod_destroyed = t.pod_destroyed;
        critical_damage = t.critical_damage;
        if (t.objective1 != null)
        {
            objective1 = tile_grid[t.objective1.location.getX(), t.objective1.location.getY()];
        }
        else
        {
            objective1 = null;
        }
        if(t.objective2 != null)
        {
            objective2 = tile_grid[t.objective2.location.getX(), t.objective2.location.getY()];
        }
        else
        {
            objective2 = null;
        }

        psion_active = t.psion_active;
        grid_health = t.grid_health;
        hiddenBurrowers[0].Steal(t.hiddenBurrowers[0]);
        hiddenBurrowers[1].Steal(t.hiddenBurrowers[1]);
        for (int i = 0;i<8;i++)
        {
            for(int j =0;j<8;j++)
            {
                tile_grid[i, j].Steal(t.tile_grid[i, j]);
            }
        }
        mechs.Clear();
        for(int i = 0;i<t.mechs.Count;i++)
        {
            Coordinate mtile = t.mechs[i].myTile.location;
            mechs.Add(tile_grid[mtile.getX(),mtile.getY()].occupant);
        }
        attack_order.Clear();
        for (int i = 0; i < t.attack_order.Count; i++)
        {
            Coordinate vtile = t.attack_order[i].myTile.location;
            attack_order.Add(tile_grid[vtile.getX(), vtile.getY()].occupant);
        }
        for(int i = 0;i<6;i++)
        {
            actions_left[i] = t.actions_left[i];
        }

    }

    //Reset certain values
    public void Refresh()
    {
        int i;
        for (i = 0; i < 6; i++)
        {
            action_score[i] = 0;
        }
        for (i = 0; i < 4; i++)
        {
            mission_counters[i] = 0;
        }
        critical_damage = 0;
        pod_destroyed = 0;
        objective1 = null;
        objective2 = null;
        attack_order.Clear();
        mechs.Clear();
        for (i=0; i < 3; i++)
        {
            actions_left[i] = true;
        }
        for (; i < 6; i++)
        {
            actions_left[i] = false;
        }
    }
}
