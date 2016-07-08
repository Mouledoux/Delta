using UnityEngine;
using System.Collections;

public class _Stats : MonoBehaviour
{
    public int m_Level;         // Floor the item was found on
    public float m_RareMod;     // Number all stats are moded by

    public float m_Speed;       // Speed the entity moves
    public float m_Attack;      // Ammount of damage the entity deals
    public float m_Defence;     // Ammount of damage the entity ignores
    public float m_Stealth;     // Chance of not being detected by enemies
    public float m_Dextarity;   // Atticking Speed

    public int m_RuneSlots;     // Number of possible augmentations

    public bool twoHanded;      // If this item requires 2 hands to use

    public enum e_Rarity
    {
        e_Lame,         // Stats mod of 0.7
        e_Common,       // Stats mod of 1.0
        e_Uncommon,     // Stats mod of 2.0
        e_Rare,         // Stats mod of 3.0
        e_Epic,         // Stats mod of 4.0
        e_Legendary,    // Stats mod of 5.0
        e_Holy,         // Stats mod of 3.5  + Life absorbe
        e_Demonic,      // Stats mod of 10.0 + Total defence mod of 0.01
    }
    public e_Rarity m_Rarity;

    private void Rarity()
    {
        int t_Lame          = 10;       // Threshold for an item to be e_Lame
        int t_Common        = 50;       // Threshold for an item to be e_Common
        int t_Uncommon      = 60;       // Threshold for an item to be e_Uncommon
        int t_Rare          = 70;       // Threshold for an item to be e_Rare
        int t_Epic          = 80;       // Threshold for an item to be e_Epic
        int t_Legendary     = 90;       // Threshold for an item to be e_Legendary
        int t_Holy          = 99;       // Threshold for an item to be e_Holy
        int t_Demonic       = 100;      // Threshold for an item to be e_Demonic

        int t_RareLevel = Random.Range(0, t_Demonic + 1);           // Randomly decides an item's m_Rarity
                                                                    //
        if (t_RareLevel <= t_Lame) /////////////////////////////////// Lame
        {                                                           //
            m_Rarity = e_Rarity.e_Lame;                             //
            m_RareMod = 0.7f;                                       //
            m_RuneSlots = 0;                                        //
        }                                                           //
                                                                    //
        else if (t_RareLevel <= t_Common) //////////////////////////// Common
        {                                                           //
            m_Rarity = e_Rarity.e_Common;                           //
            m_RareMod = 1.0f;                                       //
            m_RuneSlots = 1;                                        //
        }                                                           //
                                                                    //
        else if (t_RareLevel <= t_Uncommon) ////////////////////////// Uncommon
        {                                                           //
            m_Rarity = e_Rarity.e_Uncommon;                         //
            m_RareMod = 2.0f;                                       //
            m_RuneSlots = 2;                                        //
        }                                                           //
                                                                    //
        else if (t_RareLevel <= t_Rare) ////////////////////////////// Rare
        {                                                           //
            m_Rarity = e_Rarity.e_Rare;                             //
            m_RareMod = 3.0f;                                       //
            m_RuneSlots = 3;                                        //
        }                                                           //
                                                                    //
        else if (t_RareLevel <= t_Epic) ////////////////////////////// Epic
        {                                                           //
            m_Rarity = e_Rarity.e_Epic;                             //
            m_RareMod = 4.0f;                                       //
            m_RuneSlots = 4;                                        //
        }                                                           //
                                                                    //
        else if (t_RareLevel <= t_Legendary) ///////////////////////// Legendary
        {                                                           //
            m_Rarity = e_Rarity.e_Legendary;                        //
            m_RareMod = 5.0f;                                       //
            m_RuneSlots = 5;                                        //
        }                                                           //
                                                                    //
        else if (t_RareLevel == t_Holy) ////////////////////////////// Holy
        {                                                           //
            m_Rarity = e_Rarity.e_Holy;                             //
            m_RareMod = 3.5f;                                       //
            m_RuneSlots = 5;                                        //
        }                                                           //
                                                                    //
        else if (t_RareLevel == t_Demonic) /////////////////////////// Demonic
        {                                                           //
            m_Rarity = e_Rarity.e_Demonic;                          //
            m_RareMod = 10.0f;                                      //
            m_RuneSlots = 7;                                        //
        }
    }

    public void Build(int a_Level = 1)
    {
        m_Level = a_Level;                  // Sets item's m_Level (1 by default)

        Rarity();                           // Sets item's m_Rarity

        m_Speed     += Random.Range(-1, 6);  // Random (de)buff to m_Speed
        m_Attack    += Random.Range(-1, 6);  // Random (de)buff to m_Strength
        m_Defence   += Random.Range(-1, 6);  // Random (de)buff to m_Defence
        m_Stealth   += Random.Range(-1, 6);  // Random (de)buff to m_Stealth
        m_Dextarity += Random.Range(-1, 6);  // Random (de)buff to m_Dextarity

        m_Speed     *= m_Level * m_RareMod; // Sets m_Speed to      (level n stat * m_Level * m_RareMod)
        m_Attack    *= m_Level * m_RareMod; // Sets m_Strength to   (level n stat * m_Level * m_RareMod)
        m_Defence   *= m_Level * m_RareMod; // Sets m_Defence to    (level n stat * m_Level * m_RareMod)
        m_Stealth   *= m_Level * m_RareMod; // Sets m_Stealth to    (level n stat * m_Level * m_RareMod)
        m_Dextarity *= m_Level * m_RareMod; // Sets m_Dextarity to  (level n stat * m_Level * m_RareMod)
    }
}
