using System;
using System.Linq;

public class RandomNameGenerator
{
    private static readonly Random random = new Random();

    // Списки слогов для генерации имен
    private static readonly string[] syllables = new[]
    {
        "al", "an", "ar", "as", "at", "ea", "ed", "en", "er", "es", "ha", "he",
        "hi", "in", "is", "it", "le", "me", "nd", "ne", "ng", "nt", "on", "or",
        "ou", "re", "se", "st", "te", "th", "ti", "to", "ve", "wa", "all", "and",
        "are", "but", "ent", "era", "ere", "eve", "for", "had", "hat", "hen", "her",
        "hin", "his", "ing", "ion", "ith", "not", "ome", "oul", "our", "sho", "ted",
        "ter", "tha", "the", "thi", "tio", "uld", "ver", "was", "wit", "you"
    };

    private static readonly string[] vowels = new[] { "a", "e", "i", "o", "u", "y" };

    public static string GenerateRandomName()
    {
        // Определяем длину имени (3-8 слогов)
        int syllableCount = random.Next(3, 9);
        string name = "";

        // Генерируем имя из случайных слогов
        for (int i = 0; i < syllableCount; i++)
        {
            // Иногда добавляем гласную между слогами для благозвучия
            if (i > 0 && random.NextDouble() > 0.7)
            {
                name += vowels[random.Next(vowels.Length)];
            }

            name += syllables[random.Next(syllables.Length)];
        }

        // Делаем первую букву заглавной
        name = char.ToUpper(name[0]) + name.Substring(1);

        return name;
    }

    // Альтернативный вариант - генерация имен из списка
    public static string GenerateFromList()
    {
        string[] names = new[]
        {
            "Alex", "Blake", "Casey", "Drew", "Ellis", "Finley", "Harley", "Jamie",
            "Kai", "Morgan", "Peyton", "Quinn", "Riley", "Skyler", "Taylor", "Zion",
            "Arin", "Brook", "Cameron", "Dakota", "Emerson", "Frankie", "Hayden", "Jordan"
        };

        return names[random.Next(names.Length)];
    }
}