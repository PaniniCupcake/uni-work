using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    public string name;
    public int type;//Fixed range, projectile, anywhere, judo
    public int min_range;
    public int max_range;
    public bool reposition;
    public int immune;
    public List<TileEffects> targetEffects = new List<TileEffects>();
    public List<TileEffects> selfEffects = new List<TileEffects>();
    public List<TileEffects> variableEffects = new List<TileEffects>();//Aerial bombs and such

    private void Setup(string n, int t, List<TileEffects> targeteffects, List<TileEffects> selfeffects, int min, int max, int imm, bool repos, List<TileEffects> variable)
    {
        int i = 0;
        name = n;
        type = t; 
        for (i = 0; i < targeteffects.Count; i++)
        {
            targetEffects.Add(targeteffects[i]);
        }
        if (selfeffects != null)
        {
            for (i = 0; i < selfeffects.Count; i++)
            {
                selfEffects.Add(selfeffects[i]);
            }
        }
        min_range = min;
        max_range = max;
        immune = imm;
        reposition = repos;
        if (variable != null)
        {
            for (i = 0; i < variable.Count; i++)
            {
                variableEffects.Add(variable[i]);
            }
        }
    }

    public Weapon(string n, int t, List<TileEffects> targeteffects, List<TileEffects> selfeffects)
    {
        Setup(n,t,targeteffects,selfeffects,1,1,-2,false,null);
    }
    public Weapon(string n, int t, List<TileEffects> targeteffects, List<TileEffects> selfeffects, int min, int max)
    {
        Setup(n, t, targeteffects, selfeffects, min, max, -2, false,null);
    }
    public Weapon(string n, int t, List<TileEffects> targeteffects, List<TileEffects> selfeffects, bool repos)
    {
        Setup(n, t, targeteffects, selfeffects, 1, 1, -2, repos, null);
    }
    public Weapon(string n, int t, List<TileEffects> targeteffects, List<TileEffects> selfeffects,int min, int max,int imm)
    {
        Setup(n, t, targeteffects, selfeffects, min, max, imm, false, null);
    }
    public Weapon(string n, int t, List<TileEffects> targeteffects, List<TileEffects> selfeffects, int min, int max,bool repos)
    {
        Setup(n, t, targeteffects, selfeffects, min, max, -2, repos, null);
    }
    public Weapon(string n, int t, List<TileEffects> targeteffects, List<TileEffects> selfeffects, int min, int max, bool repos, List<TileEffects> variable)
    {
        Setup(n, t, targeteffects, selfeffects, min, max, -2, repos, variable);
    }
    public Weapon(string n, int t, List<TileEffects> targeteffects, List<TileEffects> selfeffects, int min, int max, int imm, List<TileEffects> variable)
    {
        Setup(n, t, targeteffects, selfeffects, min, max, imm, false, variable);
    }
}
public class TileEffects
{
    public Coordinate relative_pos;
    public int push_dir; //-1 for no push, 0 for up right, 1 for down right, 2 for down left, 3 for up left
    public int status;//Fire, acid,freeze, shield, smoke, kill, flip, heal
    public int damage;
    public Coordinate relative_dest;
    public int spawntype;
    public TileEffects(Coordinate pos, int dmg,int push, int stat)
    {
        relative_pos = pos;
        push_dir = push;
        status = stat; //Fire, acid,freeze, shield, smoke, kill, flip, heal
        damage = dmg;
        relative_dest = null;
        spawntype = -1;
    }

    public TileEffects(Coordinate pos, int dmg)
    {
        relative_pos = pos;
        push_dir = -1;
        status = 0; //Fire, acid,freeze, shield, smoke, kill, flip, heal
        damage = dmg;
        relative_dest = null;
        spawntype = -1;
    }
    public TileEffects(Coordinate pos, int dmg, int push)
    {
        relative_pos = pos;
        push_dir = push;
        status = 0; //Fire, acid,freeze, shield, smoke, kill, flip, heal
        damage = dmg;
        relative_dest = null;
        spawntype = -1;
    }
    public TileEffects(Coordinate pos, int dmg, Coordinate dest)
    {
        relative_pos = pos;
        push_dir = -1;
        status = 0; 
        damage = dmg;
        relative_dest = dest;
        spawntype = -1;
    }
    public TileEffects(Coordinate pos, int dmg, int push, int stat, int spawn)
    {
        relative_pos = pos;
        push_dir = -1;
        status = stat;
        damage = dmg;
        relative_dest = null;
        spawntype = spawn;
    }
    public TileEffects Clone(Coordinate pos)
    {
        return new TileEffects(pos,damage,push_dir,status);
    }

}
