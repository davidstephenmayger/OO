using UnityEngine;
using System.Collections;

public class Player {
    private static System.Random random = new System.Random();
    private int health;
    private int AC;
    private int EV;
    private int speed;
    private Player(int health, int AC, int EV, int speed)
    {
        this.health = health;
        this.AC = AC;
        this.EV = EV;
        this.speed = speed;
    }
    public void DealDamage(int damage)
    {
        //handle dodge.
        int num = random.Next(0, 100);
        if (EV > num)
            return;

        //handle armor
        int armor = random.Next(0, AC);
        int realDamage = damage - armor;

        //apply true damage.
        health = health - realDamage;
    }
    public int GetHealth()
    {
        return health;
    }
    public int GetAC()
    {
        return AC;
    }
    public int GetEV()
    {
        return EV;
    }
    public void AddAC(int modify)
    {
        AC += modify;
    }
    public void AddEV(int modify)
    {
        EV += modify;
    }
    //
}
