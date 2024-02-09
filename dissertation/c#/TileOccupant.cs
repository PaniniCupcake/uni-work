using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileOccupant : MonoBehaviour
{
    public string unit_name;
    public int unit_variant;
    public int number;
    public int alpha;
    public Vector2 target = new Vector2(9, 9);
    public Coordinate targetcoord;
    public Vector2 target2 = new Vector2(9, 9);
    public Coordinate targetcoord2;
    public Vector2 weaponid1 = new Vector2(-1, -1);
    public Vector2 weaponid2 = new Vector2(-1, -1);
    public Coordinate prev_target = new Coordinate(9, 9);
    public Coordinate prev_location = new Coordinate(9, 9);
    public int max_health;
    public int health;
    public int movement;
    public int allignment; //-1 enemy, 0 nuetral, 1 friendly, 2 grid
    public bool fire;
    public bool fireimmune;
    public bool acid;
    public bool shielded;
    public bool frozen;
    public bool armoured;
    public bool stable;
    public int objective_type; //1 for energy, 2 for favor, 3 for core 
    public bool penetrable;
    public bool flying;
    public bool massive;
    public int webber_type;//0 for not, 1 for target, 2 for all adjacent
    public bool webbed;
    public bool psion;
    public bool auto_attack = false;
    public Weapon weapon_1;
    public Weapon weapon_2;
    public bool chasm_dead = false;
    public bool attack_cancelled = false;
    public int max_damage = 12;
    public List<SpriteRenderer> sprites = new List<SpriteRenderer>();
    public Controller controller;
    public Tile myTile;
    public int pilot;//Some pilots are redundant. 0 for none, 1 for camila, 2 for gana, 3 for archimedes, 4 for henry, 5 for silica, 6 for chen, 7 for harold, 8 for kaz, 9 for mafan. Doesn't matter for now
    //1 for smoke/web immune, 2 for deploy anyway, 3 for move again after attacking, 4 for move through enemies, 5 for attack twice without moving, 6 for move 1 after attacking, 7 for push when repairing, 8 for attack instead of repair, 9 for regen shield.


    private void Awake()
    {
        controller = FindObjectOfType<Controller>();
        if(controller.cur_grid == 1)
        {
            return;
        }
        penetrable = true;
        Weapons_list weapons = controller.weapons;
        if (allignment == -1)
        {
            weapon_1 = weapons.enemyweapons[unit_variant, alpha];
            unit_variant = (int) weaponid1.x;
        }
        if(allignment == 1)
        {
            if(weaponid1.x > -1)
            {
                weapon_1 = weapons.allyweapons[(int)weaponid1.x, (int)weaponid1.y];
            }
            if(weaponid2.x > -1)
            {
                weapon_2 = weapons.allyweapons[(int)weaponid2.x, (int)weaponid2.y];
            }
        }
        if (allignment == 0 && stable)
        {
            max_damage = 1;
            penetrable = false;
            fireimmune = true;
            max_health = health;
        }
        if (allignment == 2)
        {
            stable = true;
            penetrable = false;
            max_health = health;
            fireimmune = true;
        }
        targetcoord = new Coordinate((int)target.x, (int)target.y);
        targetcoord2 = new Coordinate((int)target2.x, (int)target2.y);

    }

    //--------------------------------------------------------------------
    //Updating vals
    //--------------------------------------------------------------------


    public void Steal(TileOccupant t)
    {
        unit_name = t.unit_name;
        unit_variant = t.unit_variant;
        number = t.number;
        alpha = t.alpha;
        target = t.target;
        targetcoord.setX(t.targetcoord.getX());
        targetcoord.setY(t.targetcoord.getY());
        target2 = t.target2;
        targetcoord2.setX(t.targetcoord2.getX());
        targetcoord2.setY(t.targetcoord2.getY());
        weaponid1 = t.weaponid1;
        weaponid2 = t.weaponid2;
        prev_target = t.prev_target;
        prev_location = t.prev_location;
        max_health = t.max_health;
        health = t.health;
        movement = t.movement;
        allignment = t.allignment; //-1 enemy, 0 nuetral, 1 friendly, 2 grid
        fire = t.fire;
        fireimmune = t.fireimmune;
        acid = t.acid;
        shielded = t.shielded;
        frozen = t.frozen;
        armoured = t.armoured;
        stable = t.stable;
        objective_type = t.objective_type; //1 for energy, 2 for favor, 3 for core 
        penetrable = t.penetrable;
        flying = t.flying;
        massive = t.massive;
        webber_type = t.webber_type;//0 for not, 1 for target, 2 for all adjacent
        webbed = t.webbed;
        psion = t.psion;
        auto_attack = t.auto_attack;
        weapon_1 = t.weapon_1;
        weapon_2 = t.weapon_2;
        attack_cancelled = t.attack_cancelled;
        max_damage = t.max_damage;
        chasm_dead = t.chasm_dead;
    }

    public void emptySelf()
    {
        target = new Vector2(-1, -1);
        number = 0;
        targetcoord = new Coordinate((int)target.x, (int)target.y); ;
        max_health = 0;
        health = 0;
        movement = 0;
        allignment = 0; //-1 enemy, 0 nuetral, 1 friendly, 2 grid
        fire = false;
        fireimmune = false;
        acid = false;
        shielded = false;
        frozen = false;
        armoured = false;
        stable = false;
        objective_type = 0;
        penetrable = false;
        flying = false;
        webbed = false;
        weapon_1 = null;
        weapon_2 = null;
        max_damage = 12;
    }

    public void Override(int variant, Weapon weapon, int num, int move, int hp, int allign, int pil, int alp, bool auto)
    {
        unit_variant = variant;
        number = num;
        movement = move;
        health = hp;
        max_health = hp;
        stable = ((variant == 5 && allign == -1) || allign == 2 || (allign == 0 && health == 2));
        flying = ((variant == 12 || variant == 1) && allign == -1);
        attack_cancelled = true;
        max_damage = 12;
        if ((allign == 0 && health == 2))
        {
            max_damage = 1;
        }
        massive = false;
        penetrable = true;
        objective_type = 0;
        allignment = allign;
        fire = false;
        massive = allign == 1;
        fireimmune = (allign == 2 || (allign == 0 && health == 2));
        pilot = pil;
        frozen = false;
        shielded = false;
        acid = false;
        armoured = false;
        weapon_1 = weapon;
        webbed = false;
        webber_type = 0;
        alpha = alp;
        if (allign == -1)
        {
            controller.QueueUnit(this);
        }
        targetcoord = new Coordinate(9, 9);
        targetcoord2 = new Coordinate(9, 9);
        if (variant >= 12)
        {
            controller.getCurTileGrid().psion_active = controller.psion_type;
            controller.PsionEffect();
        }
        auto_attack = auto;
        chasm_dead = false;
    }


    public void updateSprites()
    {

        if (allignment == 1)
        {
            sprites[0].sprite = controller.friendlysprites[number];
        }
        else if (allignment == -1)
        {
            sprites[0].sprite = controller.enemysprites[Mathf.Min(8, number)];
        }
        else if (allignment == 2)
        {
            if (objective_type == 0)
            {
                sprites[0].sprite = controller.power_grid;
            }
            else if (objective_type == 1)
            {
                sprites[0].sprite = controller.objective[0];
            }
            else if (objective_type > 1)
            {
                sprites[0].sprite = controller.objective[1];
            }
        }
        else if (allignment == 0)
        {
            if (!stable)
            {
                sprites[0].sprite = controller.rock;
            }
            else
            {
                sprites[0].sprite = controller.mountain[health - 1];
            }
        }
        if (health < 0)
        {
            health = 0;
        }
        if(health > 9)
        {
            print("Bad");
            print(health);
            health = 9;
        }
        sprites[1].sprite = controller.health[health];
        sprites[2].sprite = controller.movement[movement];
        if (!targetcoord.addCoord(myTile.location).outOfBounds() && !attack_cancelled)
        {
            sprites[3].sprite = controller.targetx[targetcoord.addCoord(myTile.location).getX()];
            sprites[4].sprite = controller.targety[targetcoord.addCoord(myTile.location).getY()];
        }
        else
        {
            sprites[3].sprite = controller.empty_sprite;
            sprites[4].sprite = controller.empty_sprite;
        }
        if (fireimmune)
        {
            sprites[5].sprite = controller.status[6];
        }
        else if (fire)
        {
            sprites[5].sprite = controller.status[0];
        }
        else
        {
            sprites[5].sprite = controller.empty_sprite;
        }
        if (frozen)
        {
            sprites[6].sprite = controller.status[2];
        }
        else
        {
            sprites[6].sprite = controller.empty_sprite;
        }
        if (acid)
        {
            sprites[7].sprite = controller.status[1];
        }
        else
        {
            sprites[7].sprite = controller.empty_sprite;
        }
        if (shielded)
        {
            sprites[8].sprite = controller.status[3];
        }
        else
        {
            sprites[8].sprite = controller.empty_sprite;
        }
        if (webbed)
        {
            sprites[9].sprite = controller.status[5];
        }
        else
        {
            sprites[9].sprite = controller.empty_sprite;
        }
        if (stable)
        {
            sprites[10].sprite = controller.status[4];
        }
        else
        {
            sprites[10].sprite = controller.empty_sprite;
        }
    }

    //--------------------------------------------------------------------
    //General functions
    //--------------------------------------------------------------------


    public void cancelAttack()
    {
       // print("Cancelled");
        targetcoord = new Coordinate(9, 9);
        targetcoord2 = new Coordinate(9, 9);
        attack_cancelled = true;
        webber_type = 0;
    }

    public void die()
    {
        //print("Unit " + unit_variant + "Has died");
        if(!myTile.occupied)
        {
            return;
        }
        health = 0;
        if (allignment != 1)
        {

            if (acid && !(stable&&max_damage == 1))
            {
                myTile.addAcid();
            }
            if (allignment == -1)
            {
                controller.getCurTileGrid().attack_order.Remove(this);
                controller.getCurTileGrid().mission_counters[0]++;
                //print("killed " + unit_variant + " on tile " + myTile.location.getX() + "," + myTile.location.getY() + " with allignment " + allignment);
                if (controller.cur_grid == 0)
                {
                    controller.log.enemies_killed++;
                }
                if (unit_variant >= 12)
                {
                    controller.getCurTileGrid().psion_active = 0;
                    controller.UndoPsionEffect();
                }
                else if (controller.getCurTileGrid().psion_active == 3)
                {
                    controller.explode_queue.Enqueue(myTile.location);
                }
            }
            emptySelf();
            myTile.occupied = false;

        }
        else
        {
            controller.getCurTileGrid().actions_left[number] = false;
            controller.getCurTileGrid().actions_left[number + 3] = false;
        }
        webber_type = 0;
        movement = 0;
        attack_cancelled = true;
        weapon_1 = null;
    }

    public int getMovement()
    {
        checkWebbed(false);
        if (webbed || frozen)
        {
            return 0;
        }
        return movement;
    }


    public void updateTarget()
    {
        if (allignment == -1 && weapon_1 != null && !attack_cancelled)
        {
            //print(unit_variant+"Type" + weapon_1.type);
            if (weapon_1.type == 2)
            {
                //print("Target updated");
                Coordinate unit = targetcoord.getUnitVector();
                targetcoord = unit;
                //print(unit_name);
                //print("Unit" + unit.getX() + "," + unit.getY());
                //print("Unit" + targetcoord.getX() + "," + targetcoord.getY());
                if (targetcoord.addCoord(myTile.location).outOfBounds())
                {
                    cancelAttack();
                }
                while (!myTile.location.addCoord(targetcoord).outOfBounds() && !controller.getTile(myTile.location.addCoord(targetcoord)).occupied)
                {
                    targetcoord = targetcoord.addCoord(unit);
                }
                if (myTile.location.addCoord(targetcoord).outOfBounds())
                {
                    targetcoord = targetcoord.subtractCoord(unit);
                }

                if (targetcoord.Equals(myTile.location))
                {
                    cancelAttack();
                }
            }
            if (target2 != new Vector2(9, 9) && weapon_2 != null)
            {
                Coordinate unit = targetcoord.getUnitVector();
                targetcoord2 = unit;
                if (targetcoord2.addCoord(myTile.location).outOfBounds())
                {
                    cancelAttack();
                }
                while (!myTile.location.addCoord(targetcoord2).outOfBounds() && !controller.getTile(myTile.location.addCoord(targetcoord2)).occupied)
                {
                    targetcoord2 = targetcoord2.addCoord(unit);
                }
                if (myTile.location.addCoord(targetcoord2).outOfBounds())
                {
                    targetcoord2 = targetcoord2.subtractCoord(unit);
                }
            }
        }
        if (targetcoord.addCoord(myTile.location).outOfBounds())
        {
           // print(targetcoord.getX() + "," + targetcoord.getY());
            //print("Out");
            cancelAttack();
        }
        if(!targetcoord.outOfBounds() && !controller.getTile(targetcoord).occupied)
        {
            webber_type = 0;
        }
    }

    //--------------------------------------------------------------------
    //Status effects
    //--------------------------------------------------------------------


    public void addFire()
    {
        if (!fireimmune && !shielded && myTile.occupied)
        {
            fire = true;
            if (allignment == 1)
            {
                //controller.getCurTileGrid().action_score[3] -= 4;
            }
            else if (allignment == -1)
            {
                //controller.getCurTileGrid().action_score[3] += 4;
            }
        }
        frozen = false;//fire melts ice even if shielded
    }

    public void burnDamage()
    {
        if (health > 0 && fire && !fireimmune && !(allignment == 1 && controller.flame_shielding))
        {
            health--;
            if (health <= 0)
            {
                die();
            }
            else
            {
                burrowCheck();
            }
        }
    }
    public bool addAcid()
    {
        if (!shielded && myTile.occupied)
        {
            acid = true;
            if (allignment == 1)
            {
                //controller.getCurTileGrid().action_score[3] -= health / 2;
            }
            else if (allignment == -1)
            {
                //controller.getCurTileGrid().action_score[3] += health;
            }
            return true;
        }
        if(myTile.liquid)
        {
            myTile.addAcid();
        }
        return false;
    }

    public void addShield()
    {
        shielded = true;
    }

    public void addFrozen()
    {
        if (!shielded && myTile.occupied)
        {
            frozen = true;
            fire = false;
            attack_cancelled = true;
            webber_type = 0;
            if(allignment > 0)
            {
                //controller.getCurTileGrid().action_score[3] -= 8;
            }
            else if(allignment == -1)
            {
                //controller.getCurTileGrid().action_score[3] += 12;
            }
        }
    }



    public void electricDamage(int damage)
    {
        if (allignment == -1)
        {
            if (shielded)
            {
                shielded = false;
            }
            else if (frozen)
            {
                frozen = false;
            }
            else
            {
                health -= damage;
                if (health <= 0)
                {
                    die();
                    return;
                }
                else
                {
                    burrowCheck();
                    return;
                }
            }
            if (health <= 0)
            {
                die();
            }
        }
    }


    public void checkWebbed(bool first)//Call at turn start
    {
        if(!first && !webbed)
        {
            return;
        }
        webbed = false;
        for (int i = 0; i < 4; i++)
        {
            //print(number);
            //print(allignment);
            //print("?");
            //print(myTile.location);
            Coordinate possible = myTile.location.addCoord(controller.unitCoords[i]);
            //print("2?");
            if (!possible.outOfBounds() && controller.getTile(possible).occupied)
            {
                //print("3?");
                TileOccupant cur = controller.getTile(possible).occupant;
                //print("e?");
                if (cur.webber_type == 2 || cur.webber_type == 1 && !cur.attack_cancelled && cur.targetcoord.Equals(myTile.location))
                {
                    webbed = true;
                }
                //print("4?");
            }
        }
    }

    //--------------------------------------------------------------------
    //Damage
    //--------------------------------------------------------------------


    public void bumpDamage(int damage)
    {
        if (shielded)
        {
            shielded = false;
        }
        else if (frozen)
        {
            frozen = false;
        }
        else
        {
            if (allignment == -1)
            {
                health -= damage;
                if (health <= 0)
                {
                    die();
                    return;
                }
                else
                {
                    burrowCheck();
                    return;
                }
            }
            if (allignment == 2)
            {
                controller.getCurTileGrid().mission_counters[2]++;
                if (controller.getCurTileGrid().grid_health <= 1)
                {
                    //controller.getCurTileGrid().action_score[1] -= 1;
                }
                if (objective_type > 0)
                {
                    if (objective_type == 1)
                    {
                        //controller.getCurTileGrid().action_score[2] -= (7 + (8 - controller.getCurTileGrid().grid_health));
                    }
                    else if (objective_type == 2)
                    {
                        //controller.getCurTileGrid().action_score[2] -= 10;
                    }
                    else if (objective_type == 3)
                    {
                        //controller.getCurTileGrid().action_score[2] -= 30;
                    }
                    if (controller.getCurTileGrid().perfect_bonus)
                    {
                        //controller.getCurTileGrid().action_score[2] -= 10;
                        controller.getCurTileGrid().perfect_bonus = false;
                    }
                }
                //controller.getCurTileGrid().action_score[2] -= (7 + (8 - controller.getCurTileGrid().grid_health));
                int r = Random.Range(0, 100);
                if (r >= controller.resist_chance) 
                {
                    //print("Building damaged");
                    controller.getCurTileGrid().grid_health -= 1;
                    if(objective_type > 0)
                    {
                        controller.getCurTileGrid().perfect_bonus = false;
                    }
                    objective_type = 0;
                }
                if(controller.getCurTileGrid().grid_health - 1 <= 0)
                {
                    controller.getCurTileGrid().critical_damage++;
                }
                health--;
            }
            else if (allignment == 1)
            {
                if (health == 1)
                {
                    //controller.getCurTileGrid().action_score[3] -= 5;
                }
                //controller.getCurTileGrid().action_score[3] -= (4 / health);
                health--;
            }
            else if (allignment == 0)
            {
                health--;
            }
            else if (allignment == -1)
            {
                if (health == 1)
                {
                    //controller.getCurTileGrid().action_score[3] += 10;
                }
                //controller.getCurTileGrid().action_score[3] += 2;
                health--;
            }
        }
        if (health <= 0)
        {
            die();
        }
    }

    public void takeDamage(int damage)
    {
        if (damage > 0 && myTile.occupied)
        {
            if (shielded)
            {
                damage = 0;
                shielded = false;
            }
            else if (frozen)
            {
                damage = 0;
                frozen = false;
            }
            else
            {

                if (acid && allignment != 2)
                {
                    damage *= 2;
                }
                else if (armoured)
                {
                    damage--;
                }
                damage = Mathf.Min(max_damage, damage);
                damage = Mathf.Min(health, damage);
                if (allignment == 2)
                {
                    //print("GRID TOOK DAMAGE " + Mathf.Min(health, damage));
                    controller.getCurTileGrid().mission_counters[2]+= damage;
                    damage = Mathf.Min(health, damage);
                    if (objective_type > 0)
                    {
                        if (objective_type == 1)
                        {
                            //controller.getCurTileGrid().action_score[2] -= (7 + (8 - controller.getCurTileGrid().grid_health));
                        }
                        else if (objective_type == 2)
                        {
                           // controller.getCurTileGrid().action_score[2] -= 10;
                        }
                        else if (objective_type == 3)
                        {
                           // controller.getCurTileGrid().action_score[2] -= 30;
                        }
                        if (controller.getCurTileGrid().perfect_bonus)
                        {
                            //controller.getCurTileGrid().action_score[2] -= 10;
                            controller.getCurTileGrid().perfect_bonus = false;
                        }
                    }
                    int r = Random.Range(0, 100);
                    if (r >= controller.resist_chance)
                    {
                        //print("Building damaged");
                        controller.getCurTileGrid().grid_health -= damage;
                        health -= damage;
                        objective_type = 0;
                        if (objective_type > 0)
                        {
                            controller.getCurTileGrid().perfect_bonus = false;
                        }
                    }
                    if (controller.getCurTileGrid().grid_health - damage <= 0)
                    {
                        controller.getCurTileGrid().critical_damage++;
                    }
                }
                else if (allignment == 1)
                {
                    if (health > 0 && damage >= health)
                    {
                        //controller.getCurTileGrid().action_score[3] -= 5;
                    }
                    for (int i = 0; i < damage; i++)
                    {
                        //controller.getCurTileGrid().action_score[3] -= (4 / health);
                        health--;
                    }
                }
                else if (allignment == 0)
                {
                    health -= damage;
                }
                else if(allignment == -1)
                {
                    if (damage >= health)
                    {
                       // controller.getCurTileGrid().action_score[3] += 10;
                    }
                    for (int i = 0; i < damage; i++)
                    {
                        //controller.getCurTileGrid().action_score[3] += 2;
                        health--;
                    }
                }
               // print("Taken damage" + unit_name);
                if (health <= 0)
                {
                    die();
                    return;
                }
                else
                {
                    burrowCheck();
                    return;
                }
            }
            if (health <= 0)
            {
                die();
            }
            if (damage > 0)
            {
                myTile.addDamage();
            }
        }
    }

    private void burrowCheck()
    {
        //print("Cokc check!");
        //print(unit_variant);
        if (allignment == -1 && unit_variant == 5)
        {
            print("Burrow time");
            fire = false;
            cancelAttack();
            //print("Ca;; 3");
            Tile temp = myTile;
            if (!controller.getCurTileGrid().hiddenBurrowers[0].occupied)
            {
                controller.getCurTileGrid().hiddenBurrowers[0].location = myTile.location;
                controller.getCurTileGrid().hiddenBurrowers[0].SwapOccupantMovement(myTile);
            }
            else
            {
                controller.getCurTileGrid().hiddenBurrowers[1].location = myTile.location;
                controller.getCurTileGrid().hiddenBurrowers[1].SwapOccupantMovement(myTile);
            }
            temp.occupied = false;
        }
    }

    //--------------------------------------------------------------------
    //Other
    //--------------------------------------------------------------------


    public void Moved()
    {
        webber_type = 0;
    }

    public void Heal()
    {
        health++;
        if (health > max_health)
        {
            health = max_health;
        }

    }

    public void Repair()
    {
        fire = false;
        acid = false;
        frozen = false;
        if(health < max_health)
        {
            //controller.getCurTileGrid().action_score[3] += 1;
        }
        Heal();
    }

    public void AssignHealth(int h)
    {
        health = h;
        max_health = h;
    }
    public void AssignMove(int m)
    {
        movement = m;
    }



}
