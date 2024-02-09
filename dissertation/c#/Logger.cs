using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger
{
    public int missions;
    public int[] turns_survived = new int[6];
    public int[] power_remaining = new int[8];
    public int mean_end_turn;
    private int end_turn_score;
    private int end_turns;
    public int action_score;
    private int actions;
    public int move_score;
    private int moves;
    public int[] action_counter = new int[255];
    public int objectives_succeeded = 0;
    public int pods_recovered = 0;
    public int enemies_killed = 0;
    public int poss_actions = 0;
    public int enemy_actions = 0;
    public int enemy_poss_per_turn = 0;
    public Logger()
    {
        missions = 0;
        for(int i = 0;i<6;i++)
        {
            turns_survived[i] = 0;
        }
        for (int i = 0; i < 8; i++)
        {
            power_remaining[i] = 0;
        }
    }
    public float getMeanEnd()
    {
        return end_turn_score / end_turns;
    }
    public float getMeanAct()
    {
        return action_score / actions;
    }
    public float getMeanMove()
    {
        return move_score / moves;
    }
    public void mission_over(int turn, int pow)
    {
        turns_survived[turn]++;
        power_remaining[pow]++;
    }
    public void action(int score)
    {
        action_score += score;
        actions++;
    }
    public void move(int score)
    {
        move_score += score;
        moves++;
    }
    public void end_turn(int score)
    {
        end_turn_score += score;
        end_turns++;
    }
}
