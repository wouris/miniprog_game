using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace miniprog
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            for (int currentMap = 1; currentMap <= 5; currentMap++)
            {
                // load map from the file
                string map = File.ReadAllText($"mapy/mapa{currentMap}.txt");
                string realMap = map.Replace("\r\n", string.Empty);

                int rowLength = 1;
                foreach (var symbol in map)
                {
                    if (symbol.Equals('\r'))
                    {
                        rowLength--;
                        break;
                    }

                    rowLength++;
                }

                int rowCount = 0;
                for (int i = 0; i < map.Length; i++)
                {
                    if (map[i].Equals('\n'))
                        rowCount++;
                }

                // find positions (player, monsters, arrows, end points)
                int startPos = 0;
                List<int> endPos = new(), monsterPos = new(), arrowPos = new ();
                for (int charIndex = 0; charIndex < realMap.Length; charIndex++)
                {
                    if (realMap[charIndex] == 'S') // player
                        startPos = charIndex;
                    
                    if (realMap[charIndex] == 'X') // end points
                        endPos.Add(charIndex);
                    
                    if (realMap[charIndex] == 'P') // monsters
                        monsterPos.Add(charIndex);
                    
                    if (realMap[charIndex] == 'A') // arrows
                        arrowPos.Add(charIndex);
                }

                //declare necessary variables
                int currentPos = startPos; // set currentPos to startPos at the start of the game
                int currentWindowHeight = Console.WindowHeight;
                int currentWindowWidth = Console.WindowWidth;
                int arrowCount = 0;

                // controls
                HashSet<ConsoleKey> right = new HashSet<ConsoleKey>() {ConsoleKey.D, ConsoleKey.RightArrow};
                HashSet<ConsoleKey> left = new HashSet<ConsoleKey>() {ConsoleKey.A, ConsoleKey.LeftArrow};
                HashSet<ConsoleKey> up = new HashSet<ConsoleKey>() {ConsoleKey.W, ConsoleKey.UpArrow};
                HashSet<ConsoleKey> down = new HashSet<ConsoleKey>() {ConsoleKey.S, ConsoleKey.DownArrow};

                Console.CursorVisible = false;
                // game loop
                Console.Clear();
                while (true)
                {
                    bool currentPosIsEndPos = false;
                    foreach (int pos in endPos)
                    {
                        if (currentPos == pos)
                            currentPosIsEndPos = true; // break while loop and continue for loop
                    }
                    if (currentPosIsEndPos) break;
                    
                    foreach (int pos in monsterPos)
                    {
                        if (arrowCount <= 0)
                        {
                            if (currentPos == pos)
                            {
                                Console.SetCursorPosition(Console.WindowWidth/2-8,Console.WindowHeight/2-3);
                                Console.WriteLine("+--------------+");
                                Console.SetCursorPosition(Console.WindowWidth/2-8,Console.CursorTop);
                                Console.WriteLine("|  Game Over!  |");
                                Console.SetCursorPosition(Console.WindowWidth/2-8,Console.CursorTop);
                                Console.WriteLine("+--------------+");
                                Environment.Exit(1);
                            }
                        }
                    }
                    
                    bool currentPosIsArrowPos = false;
                    foreach (int pos in arrowPos)
                    {
                        if (currentPos == pos)
                        {
                            arrowCount++;
                            currentPosIsArrowPos = true;
                        }
                    }
                    if (currentPosIsArrowPos) arrowPos.Remove(currentPos);
                    
                    
                    if (Console.WindowHeight != currentWindowHeight || Console.WindowWidth != currentWindowWidth)
                    {
                        Console.Clear(); // if you have resized the window, it will refresh it
                        
                        // this is here if you've somehow resized the window back to exactly as is was declared before
                        currentWindowHeight = Console.WindowHeight;
                        currentWindowWidth = Console.WindowWidth;
                    }

                    try
                    {
                        Console.SetCursorPosition(Console.WindowWidth / 2 - rowLength / 2,
                            Console.WindowHeight / 2 - rowCount + 1);
                    }
                    catch
                    {
                        Console.SetCursorPosition(Console.WindowWidth / 2 - rowLength / 2,
                            0);
                    }
                    int rowIndex = 1, index = 1;
                    foreach (var symbol in realMap)
                    {
                        if (index == realMap.Length)
                        {
                            Console.Write(symbol);
                            break;
                        }

                        Console.Write(symbol);
                        if (rowIndex == rowLength)
                        {
                            if (index != realMap.Length)
                                Console.Write("\n");
                            Console.SetCursorPosition(Console.WindowWidth / 2 - rowLength/2, Console.CursorTop);
                            Console.Write(symbol);
                            Console.SetCursorPosition(Console.WindowWidth / 2 - rowLength/2, Console.CursorTop);
                            rowIndex = 0;
                        }

                        rowIndex++;
                        index++;
                    }

                    while (Console.KeyAvailable)
                    {
                        // flush all spammed keys
                        Console.ReadKey(true);
                    }
                    
                    ConsoleKeyInfo input = Console.ReadKey(true);

                    while (input.Key == ConsoleKey.Spacebar)
                    {
                        if (arrowCount <= 0) break;
                        
                        ConsoleKeyInfo inputArrow = Console.ReadKey(true);

                        if (right.Contains(inputArrow.Key))
                        {
                            int shotArrowPos = 1;
                            while (true)
                            {
                                shotArrowPos++;
                                // ReplaceAtIndex(currentPos + shotArrowPos,'>',realMap);
                                // Thread.Sleep(100);
                                if (realMap[shotArrowPos] == 'P')
                                {
                                    ReplaceAtIndex(currentPos + shotArrowPos,' ',realMap);
                                    break;
                                }
                                
                                if (realMap[shotArrowPos] == '#')
                                {
                                    break;
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    if (right.Contains(input.Key))
                    {
                        if (realMap[currentPos + 1] != '#')
                        {
                            realMap = ReplaceAtIndex(currentPos, ' ', realMap);
                            realMap = ReplaceAtIndex(currentPos + 1, 'S', realMap);
                            currentPos += 1;
                        }
                    }

                    if (left.Contains(input.Key))
                    {
                        if (realMap[currentPos - 1] != '#')
                        {
                            realMap = ReplaceAtIndex(currentPos, ' ', realMap);
                            realMap = ReplaceAtIndex(currentPos - 1, 'S', realMap);
                            currentPos -= 1;
                        }
                    }

                    if (up.Contains(input.Key))
                    {
                        for (int i = currentPos; i > 0; i--)
                        {
                            if (i == currentPos - rowLength && realMap[currentPos - rowLength] != '#')
                            {
                                realMap = ReplaceAtIndex(currentPos, ' ', realMap);
                                realMap = ReplaceAtIndex(currentPos - rowLength, 'S', realMap);
                                currentPos -= rowLength;
                                break;
                            }

                            if (i == rowLength)
                            {
                                break;
                            }
                        }
                    }

                    if (down.Contains(input.Key))
                    {
                        for (int i = currentPos; i <= realMap.Length; i++)
                        {
                            if (i == currentPos + rowLength && realMap[currentPos + rowLength] != '#')
                            {
                                realMap = ReplaceAtIndex(currentPos, ' ', realMap);
                                realMap = ReplaceAtIndex(currentPos + rowLength, 'S', realMap);
                                currentPos += rowLength;
                                break;
                            }

                            if (i == rowLength)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        static string ReplaceAtIndex(int i, char value, string word)
        {
            char[] letters = word.ToCharArray();
            letters[i] = value;
            return string.Join("", letters);
        }
    }
}