using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pokemon
{
   public  PokemonBase Base;
   public  int Level;
   public int HP { get; set; }
    public List<Move> Moves { get; set; }

    public Pokemon(PokemonBase pBase, int pLevel)
    {
        Base = pBase;
        Level = pLevel;
        HP = MaxHp;

        Moves = new List<Move>();
        foreach (var move in Base.LearnableMoves)
        {
            if (move.Level <= Level)
                Moves.Add(new Move(move.MoveBase));

            if (Moves.Count >= 4)
            {
                break;
            }
        }
    }

    public int Attack
    {
        get { return Mathf.FloorToInt((Base.Attack * Level) / 100f) + 5; }
    }
    public int Defense
    {
        get { return Mathf.FloorToInt((Base.Defense * Level) / 100f) + 5; }
    }
    public int SpAttack
    {
        get { return Mathf.FloorToInt((Base.SpAttack * Level) / 100f) + 5; }
    }
    public int SpDefense
    {
        get { return Mathf.FloorToInt((Base.SpDefense * Level) / 100f) + 5; }
    }
    public int Speed
    {
        get { return Mathf.FloorToInt((Base.Speed * Level) / 100f) + 5; }
    }
    public int MaxHp
    {
        get { return Mathf.FloorToInt((Base.MaxHp * Level) / 100f) + 10 + Level; }
    }
    public bool TakeDamage(Move move, Pokemon attacker)
    {
        // Calculate type effectiveness
        float type = TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type1) *
                     TypeChart.GetEffectiveness(move.Base.Type, this.Base.Type2);

        // Random modifier
        float modifiers = Random.Range(0.85f, 1f) * type;

        // Damage formula
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.Base.Power * ((float)attacker.Attack / this.Defense) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        HP -= damage;
        if (HP <= 0)
        {
            HP = 0;
            return true;
        }

        return false;
    }
    
    public Move GetRanDomMove ()
    {
        if (Moves == null || Moves.Count == 0)
            return null;

        int r = Random.Range(0, Moves.Count);
        return Moves[r];
    }

}
// Note: DamageDetails removed; TakeDamage now returns bool (fainted)
