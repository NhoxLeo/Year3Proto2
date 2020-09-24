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

    public static List<string> loadingHint = new List<string>
    {
        "Ballista Towers are great when dealing with single targets",
        "Catapults are efficient at dealing with large groups of enemies",
        "Form your own army of soldiers by constructing Barracks",
        "Freeze Towers excel at hampering ground-based invaders",
        "Soldiers will target the nearest enemy on the island",
        "Ballistas and Catapults consume Wood and Metal respectively when attacking",
        "Invading Battering Rams are devastating to buildings but weak to soldiers",
        "Red invaders are stronger than brown invaders. Black invaders are stronger still",
        "Production buildings gain bonuses if placed adjacent to their bonus tiles",
        "Heavy invaders are slower than standard invaders but have more health",
        "Small invaders are a force to be reckoned with in large groups",
        "Flying invaders cannot be targeted by Catapults, Freeze Towers or Soldiers",
        "Press the R key to repair all buildings",
        "The best way of dealing with enemies is to reduce their health to zero",
        "You can destroy buildings with the red button next to the repair button",
        "How do Vikings talk to each other? Using Norse Code!",
        "Production buildings and defensive structures need Villagers to run them",
        "Use the Villager Priority panel in the bottom-left to prioritise resource production",
        "Villagers will die if a building they're occupying gets destroyed by invaders",
        "Your Villagers will starve to death if you run out of food!",
        "Unallocated Villagers still consume food"//,
        //"Buildings of a type will cost more to build the more there are"
    };

    public static string currentLoadingHint = "This is a test loading hint";

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
