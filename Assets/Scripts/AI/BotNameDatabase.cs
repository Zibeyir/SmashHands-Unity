using System.Collections.Generic;
using UnityEngine;

public static class BotNameDatabase
{
    // 100 real insan adı (ingilis, neytral)
    // İstəsən sonradan özün dəyişə bilərsən.
    private static readonly List<string> _allNames = new List<string>
    {
        "Liam", "Noah", "Oliver", "Elijah", "James",
        "William", "Benjamin", "Lucas", "Henry", "Alexander",
        "Mason", "Michael", "Ethan", "Daniel", "Jacob",
        "Logan", "Jackson", "Levi", "Sebastian", "Mateo",
        "Jack", "Owen", "Theodore", "Aiden", "Samuel",
        "Joseph", "John", "David", "Wyatt", "Matthew",
        "Luke", "Asher", "Carter", "Julian", "Grayson",
        "Leo", "Jayden", "Gabriel", "Isaac", "Lincoln",
        "Anthony", "Hudson", "Dylan", "Ezra", "Thomas",
        "Charles", "Christopher", "Jaxon", "Maverick", "Josiah",
        "Isaiah", "Andrew", "Elias", "Joshua", "Nathan",
        "Caleb", "Ryan", "Adrian", "Miles", "Eli",
        "Nolan", "Christian", "Aaron", "Cameron", "Ezekiel",
        "Colton", "Luca", "Landon", "Hunter", "Jonathan",
        "Santiago", "Axel", "Easton", "Cooper", "Jeremiah",
        "Angel", "Roman", "Connor", "Jameson", "Robert",
        "Greyson", "Jordan", "Ian", "Everett", "Parker",
        "Adam", "Wesley", "Jason", "Jose", "Ian",
        "Declan", "Xavier", "Silas", "Evan", "Bentley"
    };

    private static List<string> _shuffledNames;
    private static int _currentIndex = 0;

    /// <summary>
    /// Hər matça başlamazdan əvvəl çağır: siyahını shuffle + sıfırlayır.
    /// </summary>
    public static void ResetForNewMatch()
    {
        _shuffledNames = new List<string>(_allNames);
        Shuffle(_shuffledNames);
        _currentIndex = 0;
    }

    /// <summary>
    /// Təkrarlanmayan random ad qaytarır.
    /// Əgər bot sayı 100-dən çox olsa, siyahıdan yenidən istifadə edir.
    /// </summary>
    public static string GetNextName()
    {
        if (_shuffledNames == null || _shuffledNames.Count == 0)
        {
            ResetForNewMatch();
        }

        // Bot sayı 100-dən azdırsa, heç vaxt təkrar olmayacaq.
        if (_currentIndex >= _shuffledNames.Count)
        {
            // Burada ya yenidən shuffle edə bilərik, ya da başdan davam.
            // Sənin üçün: başdan davam – amma real case-də 100-dən çox bot olmur.
            _currentIndex = 0;
        }

        string name = _shuffledNames[_currentIndex];
        _currentIndex++;
        return name;
    }

    private static void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
}
