using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tile : MonoBehaviour
{
    public Controller control;
    public GameObject smokeobject;
    public GameObject hazard_outline;
    public GameObject spawn_point;
    public GameObject target_outline;
    public bool liquid;
    public int frozen; //0 for not, 1 for cracked, 2 for fully
    public bool acid;
    public bool fire;
    public bool smoke;
    public bool chasm;
    public bool forest;
    public bool desert;
    public bool hazardous;
    public bool spawn;
    public bool occupied = false;
    public bool pod;
    public bool burrower_tile;
    public bool targetted = false;
    public TileOccupant occupant;
    private List<Sprite> icons = new List<Sprite>(16);
    public Coordinate location = new Coordinate(9,9);

    public void updateSprites()
    {
        occupant.transform.position = transform.position;
        occupant.updateSprites();
        if (!occupied)
        {
            occupant.gameObject.SetActive(false);
        }
        else
        {
            occupant.gameObject.SetActive(true);
        }
        if (smoke)
        {
            smokeobject.transform.position = this.transform.position;
        }
        if (control.smokedamage > 0)
        {
            smokeobject.GetComponent<SpriteRenderer>().sprite = icons[14];
        }
        if(pod)
        {
            GetComponent<SpriteRenderer>().sprite = icons[16];
        }
        else if (forest)
        {
            GetComponent<SpriteRenderer>().sprite = icons[1];
        }
        else if (desert)
        {
            GetComponent<SpriteRenderer>().sprite = icons[2];
        }
        else if (chasm)
        {
            GetComponent<SpriteRenderer>().sprite = icons[12];
        }
        else if (liquid)
        {
            if (fire)
            {
                GetComponent<SpriteRenderer>().sprite = icons[7];
            }
            else if (acid)
            {
                GetComponent<SpriteRenderer>().sprite = icons[6];
            }
            else
            {
                GetComponent<SpriteRenderer>().sprite = icons[5];
            }
        }
        else if (fire)
        {
            GetComponent<SpriteRenderer>().sprite = icons[3];
        }
        else if (frozen > 0)
        {
            if (acid)
            {
                if (frozen == 1)
                {
                    GetComponent<SpriteRenderer>().sprite = icons[11];
                }
                else
                {
                    GetComponent<SpriteRenderer>().sprite = icons[10];
                }
            }
            else
            {
                if (frozen == 1)
                {
                    GetComponent<SpriteRenderer>().sprite = icons[9];
                }
                else
                {
                    GetComponent<SpriteRenderer>().sprite = icons[8];
                }
            }
        }
        else if (acid)
        {
            GetComponent<SpriteRenderer>().sprite = icons[4];
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = icons[0];
        }
        if (hazardous)
        {
            hazard_outline.transform.position = this.transform.position;
        }
        else
        {
            hazard_outline.transform.position = this.transform.position + new Vector3(6, 0, 0);
        }
        if (targetted)
        {
            target_outline.transform.position = this.transform.position;
        }
        else
        {
            target_outline.transform.position = this.transform.position + new Vector3(6, 0, 0);
        }
        if (spawn)
        {
            spawn_point.transform.position = this.transform.position;
        }
        else
        {
            spawn_point.transform.position = this.transform.position + new Vector3(6, 0, 0);
        }
    }

    public void Steal(Tile t)
    {
        liquid = t.liquid;
        frozen = t.frozen;
        acid = t.acid;
        fire = t.fire;
        smoke = t.fire;
        chasm = t.chasm;
        forest = t.forest;
        desert = t.desert;
        hazardous = t.hazardous;
        spawn = t.spawn;
        occupied = t.occupied;
        pod = t.pod;
        burrower_tile = t.burrower_tile;
        targetted = t.targetted;
        occupant.Steal(t.occupant);
    }

    void Awake()
    {
        icons = control.tiles;
        if (control.initial_tiles)
        {
            occupant.myTile = this;
            smokeobject = Instantiate(smokeobject);
            smokeobject.transform.SetParent(control.transform);
            smokeobject.transform.position = this.transform.position + new Vector3(6, 0, 0);
            hazard_outline = Instantiate(hazard_outline);
            hazard_outline.transform.SetParent(control.transform);
            hazard_outline.transform.position = this.transform.position + new Vector3(6, 0, 0);
            target_outline = Instantiate(target_outline);
            target_outline.transform.SetParent(control.transform);
            target_outline.transform.position = this.transform.position + new Vector3(6, 0, 0);
            spawn_point = Instantiate(spawn_point);
            spawn_point.transform.SetParent(control.transform);
            spawn_point.transform.position = this.transform.position + new Vector3(6, 0, 0);
        }
        else
        {
            smokeobject = control.dummy;
            hazard_outline = control.dummy;
            spawn_point = control.dummy;
            target_outline = control.dummy;
        }
    }

    public void Clear()
    {
        liquid = false;
        acid = false;
        fire = false;
        smoke = false;
        chasm = false;
        forest = false;
        desert = false;
        desert = false;
        hazardous = false;
        pod = false;
        spawn = false;
        frozen = 0;
        targetted = false;
        smokeobject.transform.position = this.transform.position + new Vector3(6, 0, 0);
        hazard_outline.transform.position = this.transform.position + new Vector3(6, 0, 0);
        spawn_point.transform.position = this.transform.position + new Vector3(6, 0, 0);
        occupant.Override(0, null, 0, 0, 0, 0, 0,0,false);
        occupied = false;
    }

    public void SwapOccupant(Tile other)
    {
        if (other != this && !occupant.stable && !other.occupant.stable)
        {
            occupant.myTile = other;
            other.occupant.myTile = this;
            TileOccupant temp = occupant;
            occupant = other.occupant;
            other.occupant = temp;
            bool temp2 = other.checkOccupied();
            other.setOccupied(checkOccupied());
            setOccupied(temp2);
            other.occupant.webber_type = 0;
            occupant.webber_type = 0;
            if (!occupied && other.occupant.webbed || !other.occupied && occupant.webbed)
            {
                occupant.webbed = false;
                other.occupant.webbed = false;
            }
            else if (occupant.webbed && occupied)
            {
                occupant.webbed = false;
                if (other.occupant.pilot != 1)
                {
                    other.occupant.webbed = true;
                }
            }
            else if (other.occupant.webbed && other.occupied)
            {
                other.occupant.webbed = false;
                if (occupant.pilot != 1)
                {
                    occupant.webbed = true;
                }
            }
            applyTileEffects();
            other.applyTileEffects();
            control.updateSprites();
            if (occupied && pod)
            {
                pod = false;
                if (other.occupant.allignment < 0)
                {
                    //control.getCurTileGrid().action_score[1] -= 35;
                    control.getCurTileGrid().pod_destroyed = -1;
                    control.getCurTileGrid().perfect_bonus = false;
                }
            }
            if (other.occupied && other.pod)
            {
                other.pod = false;
                if (other.occupant.allignment < 0)
                {
                    //control.getCurTileGrid().action_score[1] -= 35;
                    control.getCurTileGrid().pod_destroyed = -1;
                    control.getCurTileGrid().perfect_bonus = false;
                }
            }
            occupant.Moved();
            other.occupant.Moved();
        }
    }


    //Treated differently for stable units willingly moving
    public void SwapOccupantMovement(Tile other)
    {
        if (other != this)
        {
            //print("Swappin");
            occupant.myTile = other;
            other.occupant.myTile = this;
            TileOccupant temp = occupant;
            occupant = other.occupant;
            other.occupant = temp;
            bool temp2 = other.checkOccupied();
            other.setOccupied(checkOccupied());
            setOccupied(temp2);
            other.occupant.webber_type = 0;
            occupant.webber_type = 0;
            applyTileEffects();
            other.applyTileEffects();
            control.updateSprites();
            if (occupied && pod)
            {
                pod = false;
                if (other.occupant.allignment < 0)
                {
                    //control.getCurTileGrid().action_score[2] -= 35;
                    control.getCurTileGrid().pod_destroyed = -1;
                    control.getCurTileGrid().perfect_bonus = false;
                }
            }
            if (other.occupied && other.pod)
            {
                other.pod = false;
                if (other.occupant.allignment < 0)
                {
                    //control.getCurTileGrid().action_score[2] -= 35;
                    control.getCurTileGrid().pod_destroyed = -1;
                    control.getCurTileGrid().perfect_bonus = false;
                }
            }
            occupant.Moved();
            other.occupant.Moved();
        }
    }

    //Unused
    void setOccupied(bool occ)
    {
        occupied = occ;
    }
    bool checkOccupied()
    {
        return occupied;
    }

    public void applyTileEffects()
    {
        if (!occupied)
        {
            return;
        }
        if (chasm)
        {
            if (!occupant.flying || occupant.frozen)
            {
                occupant.die();
                occupant.chasm_dead = true;
                occupied = false;
            }
        }
        else if (frozen > 0)
        { }
        else if (liquid)
        {
            if (!occupant.flying || occupant.frozen)
            {
                if (occupant.massive && !occupant.flying)
                {
                    if (fire)
                    {
                        occupant.addFire();
                    }
                    else if (acid)
                    {
                        occupant.addAcid();
                    }
                    if(occupant.acid)
                    {
                        acid = true;
                    }
                    occupant.frozen = false;
                    occupant.cancelAttack();
                }
                else if(occupant.massive && occupant.flying)
                {
                    occupant.frozen = false;
                }
                else
                {
                    occupant.die();
                }
            }
        }
        else if (fire)
        {
            occupant.addFire();
        }
        else if (acid)
        {
           // print("ACID CHECK");
           // print(location.getX() + "," + location.getY());
            if (occupant.addAcid())
            {
                //print("YOU FAILED THE ACID CHECK");
                acid = false;
            }
            //print("NO ACIDS?");
            //print(acid);
        }
        if(smoke)
        {
            occupant.fire = false;
            if(!(occupant.pilot == 1))
            {
                occupant.cancelAttack();
            }
        }
    }
    

    public void addLava()
    {
        pod = false;
        liquid = true;
        fire = true;
        acid = false;
        desert = false;
        forest = false;
        chasm = false;
        spawn = false;
    }

    public void addWater()
    {
        pod = false;
        liquid = true;
        fire = false;
        desert = false;
        forest = false;
        chasm = false;
        spawn = false;
    }

    public void addChasm()
    {
        pod = false;
        liquid = false;
        fire = false;
        desert = false;
        forest = false;
        chasm = true;
        spawn = false;
    }

    public void rockFall()
    {
        pod = false;
        liquid = false;
        fire = false;
    }


    public void addIce()
    {
        if(!liquid)
        {
            fire = false;
        }
        else if (liquid && !fire)
        {
            liquid = false;
            frozen = 2;
        }
        if (occupied)
        {
            occupant.addFrozen();
        }
    }

    public void addShield()
    {
        if(occupied)
        {
            occupant.addShield();
        }
    }

    public void addPod()
    {
        pod = true;
        forest = false;
        desert = false;
        acid = false;
    }

    public void addFire()
    {
        if (pod)
        {
            control.getCurTileGrid().pod_destroyed = -1;
            control.getCurTileGrid().perfect_bonus = false;
        }
        pod = false;
        if(frozen > 0)
        {
            liquid = true;
        }
        else if(!liquid && !chasm)
        {
            fire = true;
            acid = false;
            smoke = false;
            smokeobject.transform.position = this.transform.position + new Vector3(6, 0, 0);
            desert = false;
        }
        if(occupied)
        {
            occupant.addFire();
        }
    }
    public void addSmoke()
    {
        smokeobject.transform.position = this.transform.position;
        smoke = true;
        if (!liquid && fire)
        {
            fire = false;
        }
        occupant.cancelAttack();
    }
    public void addAcid()
    {
        // print("HOW ACID");
        if (pod)
        {
            control.getCurTileGrid().pod_destroyed = -1;
            control.getCurTileGrid().perfect_bonus = false;
        }
        pod = false;
        if(chasm)
        { }
        else if(frozen > 0)
        {
            acid = true;
        }
        else if (!liquid)
        {
            fire = false;
            acid = true;
        }
        else if (!fire)
        {
            acid = true;
        }
        if (occupied)
        {
            occupant.addAcid();
        }
    }
    public void addDamage()
    {
        if (pod)
        {
            control.getCurTileGrid().pod_destroyed = -1;
            control.getCurTileGrid().perfect_bonus = false;
        }
        pod = false;
        if(forest)
        {
            addFire();
        }
        else if(desert)
        {
            smoke = true;
            desert = false;
        }
        else if(frozen == 1)
        {
            frozen = 0;
            liquid = true;
        }
        else if(frozen == 2)
        {
            frozen = 1;
        }
    }
}
