using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData
{
    public static string curScene;
    public static bool isLoadingIn = false;
    public static bool isPaused;
    public static bool longhausDead = false;
    public static bool gameEnd = false;

    public static List<string> loadingHint = new List<string>
    {
        "Soldiers heal up quick when resting at the Barracks.",
        "The Frost Tower & Shockwave Tower slow enemies down, use them with other defences!",
        "Battering Rams move slow, but wreak havoc when they arrive. Try the Barracks against them!",
        "Red Invaders are stronger than brown Invaders. Black Invaders are stronger still",
        "Production Structures work best when surrounded by their corresponding environment.",
        "Heavy Invaders are harder to kill and deal lots of damage. Try the Ballista Tower against them!",
        "The smaller Invaders are a force to be reckoned with in large groups. Try the Catapult against them!",
        "Flying Invaders can only be targeted by the Ballista and the Lightning Tower.",
        "Petards carry an explosive barrel which can be set off by some towers... like the Lightning Tower!",
        "Press the R key to repair all buildings.",
        "Press the N key to train a new villager.",
        "Press the H key to hide enemy & soldier healthbars.",
        "Press the 1 - 6 keys to select a building to place.",
        "Press the tab key to change Structure categories.",
        "The best way of dealing with enemies is to reduce their health to zero.",
        "You can destroy buildings with the red button next to the repair button.",
        "How do Vikings talk to each other? Using Norse Code!",
        "Production Structures and Defensive Structures need Villagers to be of any use.",
        "Use the Villager Priority panel in the bottom-left to prioritise resource production.",
        "Villagers will die if a building they're in gets destroyed by invaders.",
        "Your Villagers will starve to death if you run out of Food. (That's bad.)",
        "Unallocated Villagers still need to eat.. it's best to find them something to do!",
        "The cost of Structures increase as you place them. The more Farms you have, the more Farms cost."
    };

    public static string currentLoadingHint = "This is a test loading hint. You shouldn't be reading it!";

    public static string NumberToWords(int number)
    {
        if (number == 0)
            return "zero";

        if (number < 0)
            return "minus " + NumberToWords(Math.Abs(number));

        string words = "";

        if ((number / 1000000) > 0)
        {
            words += NumberToWords(number / 1000000) + " million";
            number %= 1000000;
        }

        if ((number / 1000) > 0)
        {
            words += NumberToWords(number / 1000) + " thousand";
            number %= 1000;
        }

        if ((number / 100) > 0)
        {
            words += NumberToWords(number / 100) + " hundred";
            number %= 100;
        }

        if (number > 0)
        {
            if (words != "")
                words += " and ";

            var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

            if (number < 20)
                words += unitsMap[number];
            else
            {
                words += tensMap[number / 10];
                if ((number % 10) > 0)
                    words += "-" + unitsMap[number % 10];
            }
        }

        return words;
    }

}
