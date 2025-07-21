using System.Runtime.InteropServices;

namespace EscapeRoom
{
    internal class Program
    {
        private static string infoMessage = "";
        private static ConsoleColor infoMessageColor = ConsoleColor.White;

        private static int roomHeight = 0;
        private static int roomWidth = 0;

        private static Tile[,] roomTiles = { };

        private static char playerChar = '>';

        private static int playerX = 0;
        private static int playerY = 0;

        private static int directionX = 0;
        private static int directionY = 0;

        private static bool wasLastInputVertical = false;

        private static List<int[]> wallPositions = new List<int[]> { };

        private static bool hasPlayerWon = false;
        private static EKey key = EKey.none;

        private static ConsoleColor defaultBackgroundColor;

        private static bool hasFoundNewKey = false;

        private static string contentInit = "CAVE_";

        // Indexes for charGiver
        private static int charIndexInit = 0;
        private static int charIndex0 = 0;
        private static int charIndex1 = 0;
        private static int charIndex2 = 0;
        private static int charIndex3 = 0;
        private static int charIndex4 = 0;

        private static Dictionary<int, (string,string)> verses = new Dictionary<int, (string,string)>
        {
            { 0, ("YOU CANT RUN FROM THE PRISON THAT IS YOU IS THE PRISON ", "DAY AFTER ") },
            { 1, ("IN THE END YOU ARE YOUR OWN PRISON YOU CAN RUN BUT ", "IT WAS LIKE ")},
            { 2, ("EVEN IF YOU LEAVE THIS ROOM THERE IS NOTHING YOU CAN DO ", "GO ON LIKE THIS TO ")},
            { 3, ("THERE IS NO ESCAPE FROM WHAT CAN NOT BE ESCAPED ", "AN EYE FOR ")},
            { 4, ("GOAL YOU REACH THERE IS ANOTHER GOAL YOU MISS AND IN EVERY ", "FROM ONE TO ZERO AND FROM ZERO TO ONE AND ")},
        };


        static void Main(string[] args)
        {
            defaultBackgroundColor = Console.BackgroundColor;

            Game();
        }

        private static void Game()
        {
            StartGame();
            Explanation();
            InitRoom();
            SetPlayerStartPosition();
            CreateDoor();
            DrawRoom();
            CreateKey(EKey.first);
            EscapeRoom();
        }

        #region gamePhases

        private static void StartGame()
        {
            bool hasFinishedStart = false;
            while (!hasFinishedStart)
            {
                Console.Clear();
                Console.WriteLine("You awake in a dark room. \n With your bare hands you feel along the Walls of the room. \n What is the width of a wall in feet?");
                ColoredText("(You can also type ESC or ESCAPE to leave the program here)", ConsoleColor.Yellow, true);
                ColoredText(infoMessage,ConsoleColor.Red);
                Console.Write("\n");

                string playerInput = Console.ReadLine();

                int playerChosenNumber;
                bool isValidNumber = int.TryParse(playerInput, out playerChosenNumber);

                if (isValidNumber)
                {
                    if (playerChosenNumber < 10)
                    {
                        infoMessage = "The room can't be that small! Try again.";

                    }
                    else if (playerChosenNumber > Console.WindowHeight - 2)// "-2" hold two extra Lines at the End of the room free
                    {
                        infoMessage = $"The room can't be that big! Try again. (Maximum: {Console.WindowHeight - 2})";
                    }
                    else
                    {
                        infoMessage = "";
                        roomHeight = playerChosenNumber;
                        roomWidth = roomHeight * 2;
                        Console.Clear();
                        hasFinishedStart = true;
                    }
                }
                else
                {
                    float floatError;
                    bool isNumberFloat = float.TryParse(playerInput, out floatError);

                    if (isNumberFloat)
                    {
                        infoMessage = $"You typed a floating Number, please use an integer number! (You typed {floatError}, so use for example {(int)floatError} instead.)";
                    }
                    else
                    {
                        playerInput = playerInput.ToLower();

                        if (playerInput == "esc" || playerInput == "escape")
                        {
                            CheckExitGame(ConsoleKey.Escape);
                        }
                        infoMessage = "You made a mistake, this must be a number... try again to feel along the walls!";
                    }
                }
            }
        }

        private static void Explanation()
        {
            Console.Write("The shadows of a room slowly appear in front of you...\n");
            ColoredText("\n\t> You can move through the room using the arrow keys or W-A-S-D.\n\t> Find the door and a key to exit the room.\n\t> Don't get lost in your own thoughts.", ConsoleColor.Yellow, ConsoleColor.Black);
            ColoredText("\n\nPress ENTER to continue", ConsoleColor.Yellow);
            AwaitInputEnterOrExit();
            Console.Clear();
        }

        private static void InitRoom()
        {
            roomTiles = new Tile[roomWidth, roomHeight];

            char[,] verticalRoomOrder0 = CreateVerticalContent(verses[0].Item2);
            char[,] verticalRoomOrder1 = CreateVerticalContent(verses[1].Item2);
            char[,] verticalRoomOrder2 = CreateVerticalContent(verses[2].Item2);
            char[,] verticalRoomOrder3 = CreateVerticalContent(verses[3].Item2);
            char[,] verticalRoomOrder4 = CreateVerticalContent(verses[4].Item2);

            for (int y = 0; y < roomHeight; y++)
            {
                for (int x = 0; x < roomWidth; x++)
                {
                    if (x == 0 && y == 0 || x == roomWidth - 1 && y == roomHeight - 1 ||
                        x == 0 && y == roomHeight - 1 || x == roomWidth - 1 && y == 0)
                    {
                        roomTiles[x, y] = new Tile(ETile.edge, '+');
                    }
                    else if (x == 0 || y == 0 || x == roomWidth - 1 || y == roomHeight - 1)
                    {
                        wallPositions.Add(new int[] { x, y });

                        if (y == 0 || y == roomHeight - 1)
                        {
                            roomTiles[x, y] = new Tile(ETile.wall, '-');
                        }
                        else
                        {
                            roomTiles[x, y] = new Tile(ETile.wall, '|');
                        }
                    }
                    else
                    {
                        char nextCharInit = CharGiver(ref charIndexInit, contentInit);

                        Dictionary<int, (char, char)> tileVerses = new Dictionary<int, (char, char)>
                        {
                            { 0, (CharGiver(ref charIndex0, verses[0].Item1), verticalRoomOrder0[x, y]) },
                            { 1, (CharGiver(ref charIndex1, verses[1].Item1), verticalRoomOrder1[x, y]) },
                            { 2, (CharGiver(ref charIndex2, verses[2].Item1), verticalRoomOrder2[x, y]) },
                            { 3, (CharGiver(ref charIndex3, verses[3].Item1), verticalRoomOrder3[x, y]) },
                            { 4, (CharGiver(ref charIndex4, verses[4].Item1), verticalRoomOrder4[x, y]) }
                        };

                        roomTiles[x, y] = new Tile(ETile.free, nextCharInit, tileVerses);
                    }
                }
            }
        }

        private static void DrawRoom()
        {
            for (int y = 0; y < roomHeight; y++)
            {
                for (int x = 0; x < roomWidth; x++)
                {
                    if (roomTiles[x, y].Type == ETile.door)
                    {
                        ColoredText(roomTiles[x, y].InitialIndividual, ConsoleColor.Black, ConsoleColor.Yellow);
                    }
                    else if (roomTiles[x, y].Type == ETile.free)
                    {
                        ColoredText(roomTiles[x, y].InitialIndividual, ConsoleColor.DarkGray);
                    }
                    else
                    {
                        Console.Write(roomTiles[x, y].InitialIndividual);
                    }
                }
                Console.Write("\n");
            }
        }

        private static void EscapeRoom()
        {
            while (!hasPlayerWon)
            {
                UpdatePlayer();

                ColoredText($"\n {infoMessage}", infoMessageColor);

                if (hasFoundNewKey)
                {
                    AwaitInputEnterOrExit();

                    ClearLine(roomHeight + 1);

                    hasFoundNewKey = false;
                }

                directionX = 0;
                directionY = 0;

                ConsoleKeyInfo userInput = Console.ReadKey(true);

                CheckExitGame(userInput.Key);

                switch (userInput.Key)
                {
                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        playerChar = '^';
                        directionY = -1;
                        wasLastInputVertical = true;
                        break;
                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        playerChar = 'v';
                        directionY = 1;
                        wasLastInputVertical = true;
                        break;
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                        playerChar = '<';
                        directionX = -1;
                        wasLastInputVertical = false;
                        break;
                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        playerChar = '>';
                        directionX = 1;
                        wasLastInputVertical = false;
                        break;
                    default:
                        break;
                }
            } 
        }

        private static void UpdatePlayer()
        {
            int newPlayerX = playerX + directionX;
            int newPlayerY = playerY + directionY;

            if (roomTiles[newPlayerX, newPlayerY].Type == ETile.free)
            {
                if (roomTiles[newPlayerX, newPlayerY].TileKey != EKey.none)
                {
                    infoMessageColor = ConsoleColor.Yellow;
                    infoMessage = "You found this key on the ground! (Press ENTER)";

                    Console.SetCursorPosition(playerX, playerY);

                    ColoredText(roomTiles[newPlayerX, newPlayerY].WalkingOnNewIndividual(wasLastInputVertical,key), PlayerWalkColor(key));

                    playerX = newPlayerX;
                    playerY = newPlayerY;

                    Console.SetCursorPosition(playerX, playerY);
                    ColoredText('!', ConsoleColor.Yellow);

                    key = roomTiles[playerX, playerY].TileKey;
                    roomTiles[playerX, playerY].TileKey = EKey.none;

                    EKey nextKey = (EKey)(int)key+1;

                    if ((int)nextKey <= (int)EKey.last)
                    {
                        CreateKey(nextKey);
                    }
                    else
                    {
                        CreateKey(EKey.first);
                    }

                    Console.SetCursorPosition(roomWidth, roomHeight);

                    hasFoundNewKey = true;
                }
                else
                {
                    if (infoMessage.Length != 0)
                    {
                        ClearLine(roomHeight+1);
                        infoMessage = "";
                    }

                    Console.SetCursorPosition(playerX, playerY);

                    ColoredText(roomTiles[newPlayerX, newPlayerY].WalkingOnNewIndividual(wasLastInputVertical, key), PlayerWalkColor(key), wasLastInputVertical ? ConsoleColor.DarkGray : ConsoleColor.Black);

                    playerX = newPlayerX;
                    playerY = newPlayerY;

                    Console.SetCursorPosition(playerX, playerY);
                    ColoredText(playerChar, ConsoleColor.Yellow);
                    Console.SetCursorPosition(roomWidth, roomHeight);
                }
            }
            else if (roomTiles[newPlayerX, newPlayerY].Type == ETile.door)
            {
                if (key != EKey.none)
                {
                    GameEnd();
                }
                else
                {
                    infoMessage = "This door is closed!";
                    infoMessageColor = ConsoleColor.Red;

                    Console.SetCursorPosition(playerX, playerY);
                    ColoredText('?', ConsoleColor.Yellow);
                    Console.SetCursorPosition(roomWidth, roomHeight);
                }
            }
            else if (roomTiles[newPlayerX, newPlayerY].Type == ETile.wall)
            {
                infoMessage = "You hit a wall, be careful!";
                infoMessageColor = ConsoleColor.Red;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Console.Beep(500,300);
                }

                Console.SetCursorPosition(playerX, playerY);
                ColoredText('%', ConsoleColor.DarkRed);
                Console.SetCursorPosition(roomWidth, roomHeight);
            }
        }

        private static void GameEnd()
        {
            Console.Clear();

            hasPlayerWon = true;

            ColoredText("You escaped your prison!", ConsoleColor.Yellow, true);

            AwaitInputEnterOrExit();

            ColoredText("Press ENTER to restart or ESCAPE to exit.", ConsoleColor.White, true);

            while(hasPlayerWon)
            {
                AwaitInputEnterOrExit();

                hasPlayerWon = false;
                key = EKey.none;
                wallPositions.Clear();

                Game();
            }
        }

        #endregion

        #region randomPositioning

        private static void SetPlayerStartPosition()
        {
            Random random = new Random();

            playerX = random.Next(1, roomWidth - 1);
            playerY = random.Next(1, roomHeight - 1);
        }

        private static void CreateDoor()
        {
            Random random = new Random();
            int[] doorPosition = wallPositions[random.Next(0, wallPositions.Count)];

            roomTiles[doorPosition[0], doorPosition[1]] = new Tile(ETile.door,'§');
        }

        private static void CreateKey(EKey nextKey)
        {
            EKey newKey = nextKey;

            Random random = new Random();
            int keyX = random.Next(1, roomWidth - 1);
            int keyY = random.Next(1, roomHeight - 1);

            if (keyX != playerX && keyY != playerY)
            {
                roomTiles[keyX, keyY].TileKey = newKey;

                Console.SetCursorPosition(keyX, keyY);
                ColoredText('±', ConsoleColor.Black, PlayerWalkColor(newKey));
                Console.SetCursorPosition(roomWidth, roomHeight);
            }
            else
            {
                CreateKey(newKey);
            }
        }

        #endregion

        #region helper

        private static ConsoleColor PlayerWalkColor(EKey currentKey)
        {
            if(currentKey==EKey.none)
            {
                return ConsoleColor.White;
            }
            else if (currentKey== EKey.first)
            {
                return ConsoleColor.Cyan;
            }
            else if (currentKey == EKey.second)
            {
                return ConsoleColor.Magenta;
            }
            else if (currentKey == EKey.third)
            {
                return ConsoleColor.Blue;
            }
            else if (currentKey == EKey.last)
            {
                return ConsoleColor.DarkGreen;
            }
            return ConsoleColor.DarkGray;
        }

        private static char CharGiver(ref int charIndex, string content)
        {
            char[] letters = content.ToCharArray();
            char letter = letters[charIndex];

            if (charIndex < letters.Length-1)
            {
                charIndex++;
            }
            else
            {
                charIndex = 0;
            }

            return letter;
        }

        private static char[,] CreateVerticalContent(string content)
        {
            char[,] characters = new char[roomWidth, roomHeight];
            int verticalCharIndex = 0;

            for (int x = 0; x < roomWidth; x++)
            {
                for (int y = 0; y < roomHeight; y++)
                {
                    characters[x, y] = x % 2 == 1 ? CharGiver(ref verticalCharIndex, content) : ' ';
                }
            }

            return characters;
        }

        private static void ColoredText(char content, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(content);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void ColoredText(string content, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(content);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static void ColoredText(char content, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.Write(content);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = defaultBackgroundColor;
        }

        private static void ColoredText(string content, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
        {
            Console.BackgroundColor = backgroundColor;
            Console.ForegroundColor = foregroundColor;
            Console.Write(content);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = defaultBackgroundColor;
        }

        private static void ColoredText(string content, ConsoleColor color, bool hasLineBreak)
        {
            ColoredText(content, color);
            Console.Write("\n");
        }

        private static void ClearLine(int yPosition)
        {
            int cursorPosition = 0;

            while (cursorPosition<Console.BufferWidth)
            {
                Console.SetCursorPosition(cursorPosition, yPosition);
                Console.Write(' ');
                cursorPosition++;
            }
            Console.SetCursorPosition(0, yPosition);
        }

        private static void AwaitInputEnterOrExit()
        {
            ConsoleKeyInfo playerInput = Console.ReadKey(true);

            while (playerInput.Key != ConsoleKey.Enter)
            {
                CheckExitGame(playerInput.Key);

                playerInput = Console.ReadKey(true);
            }
        }

        private static void CheckExitGame(ConsoleKey userKey)
        {
            if (userKey == ConsoleKey.Escape)
            {
                Environment.Exit(0);
            }
        }

        #endregion

    }
}