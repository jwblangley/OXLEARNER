using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace OXLearner
{
    class Program
    {
        public const int SIZE = 3;
        public static bool isPlayersTurn;
        public static readonly Random r = new Random();
        public static string currentPlay;
        public static readonly int[] allMoves = new int[] { 11, 12, 13, 21, 22, 23, 31, 32, 33 };

        public static bool fillWithRandom = false;

        static void Main(string[] args)
        {
            string fillWithRandomStr = JL.JLUtilities.confirmInputStringChoice("Run a random filling algorithm to increase AI learnt algorithms? (y/n)", new string[] { "y", "n" });
            if (fillWithRandomStr.ToLower() == "y")
            {
                fillWithRandom = true;
            }

            char playAgain;
            do
            {
                Console.WriteLine("\n");
                string[,] board = new string[SIZE, SIZE];
                if (!File.Exists("wins.jwbl"))
                {
                    var winFile = File.Create("wins.jwbl");
                    winFile.Close();
                }
                currentPlay = "";
                string winner = "";
                initiateBoard(ref board);
                Console.WriteLine("Welcome to Noughts and Crosses!");
                Console.WriteLine("\nYou are Crosses\n");

                isPlayersTurn = (r.Next(2) == 0);
                while (!gameIsOver(board, ref winner))
                {
                    displayBoard(board);
                    if (isPlayersTurn)
                    {
                        playerTurn(ref board);
                    }
                    else
                    {
                        AITurn(ref board);
                    }
                    isPlayersTurn = !isPlayersTurn;
                }
                displayBoard(board);
                Console.WriteLine("\nGame Over");
                StreamReader sr = new StreamReader("wins.jwbl");
                string fileContents = sr.ReadToEnd();
                sr.Close();
                StreamWriter sw = new StreamWriter("wins.jwbl");
                sw.Write(fileContents);

                switch (winner)
                {
                    case "O":
                        Console.WriteLine("The Computer Won!");
                        sw.WriteLine(currentPlay);
                        break;
                    case "X":
                        Console.WriteLine("You Won!");
                        currentPlay = currentPlay.Replace('X', 'o');
                        currentPlay = currentPlay.Replace('O', 'X');
                        currentPlay = currentPlay.Replace('o', 'O');
                        sw.WriteLine(currentPlay);
                        break;
                    case "none":
                        /*
                        StreamWriter swDraw = new StreamWriter("draws.jwbl");
                        swDraw.WriteLine(currentPlay);
                        currentPlay = currentPlay.Replace('X', 'o');
                        currentPlay = currentPlay.Replace('O', 'X');
                        currentPlay = currentPlay.Replace('o', 'O');
                        swDraw.WriteLine(currentPlay);
                        swDraw.Close();
                        */
                        Console.WriteLine("It is a draw");
                        break;
                }
                sw.Close();

                //Tidy the file
                sr = new StreamReader("wins.jwbl");
                string[] wholeFile = sr.ReadToEnd().Split('\n');
                sr.Close();
                wholeFile = wholeFile.Distinct().ToArray();
                string tempString = String.Join("\n", wholeFile).Replace("\r\n\r\n", "\r\n");
                sw = new StreamWriter("wins.jwbl");
                sw.WriteLine(tempString);
                sw.Close();

                playAgain = 'n';
                if (!fillWithRandom)
                {
                    string againChoice = "lol";
                    while (againChoice.ToLower() != "y" && againChoice.ToLower() != "n")
                    {
                        Console.Write("\n\nPlay Again?  (y/n)  ");
                        againChoice = Console.ReadLine();
                    }
                    playAgain = Char.ToLower(againChoice[0]);
                    if (playAgain != 'y')
                    {
                        Console.WriteLine("Press Escape to Quit");
                    }
                }
            } while (playAgain == 'y' || Console.ReadKey().Key != ConsoleKey.Escape);
        }

        static void AITurn(ref string[,] board)
        {
            Console.WriteLine("AI's Turn");

            //Win if you can
            Console.WriteLine("Winning");

            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    string[,] tempBoard = (string[,])board.Clone();
                    if (tempBoard[j, i] == " ")
                    {
                        tempBoard[j, i] = "O";
                        string winner = "";
                        if (gameIsOver(tempBoard, ref winner))
                        {
                            if (winner == "O")
                            {
                                Console.WriteLine("AI's Move is " + (j + 1) + (i + 1));
                                board[j, i] = "O";
                                currentPlay += "O" + (j + 1) + (i + 1) + ",";
                                return;
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            //Block player winning
            Console.WriteLine("Blocking");

            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    string[,] tempBoard = (string[,])board.Clone();
                    if (tempBoard[j, i] == " ")
                    {
                        tempBoard[j, i] = "X";
                        string winner = "";
                        if (gameIsOver(tempBoard, ref winner))
                        {
                            if (winner == "X")
                            {
                                Console.WriteLine("AI's Move is " + (j + 1) + (i + 1));
                                board[j, i] = "O";
                                currentPlay += "O" + (j + 1) + (i + 1) + ",";
                                return;
                            }
                        }
                    }
                }
            }

            //follow instructions from file for win
            Console.WriteLine("Following Win");

            StreamReader sr = new StreamReader("wins.jwbl");
            string[] possibleWins = sr.ReadToEnd().Split('\n');
            possibleWins = possibleWins.Where(x => x != "" && x!="\r").ToArray(); ; //avoid empty file issue
            sr.Close();
            possibleWins = possibleWins.Where(x => x.StartsWith(currentPlay)).ToArray();
            possibleWins = possibleWins.Where(x => x.Substring(currentPlay.Length, 2)[0] == 'O').ToArray(); //Only follow wins with AI turn next (important for start)
            if (possibleWins.Length != 0)
            {
                string chosenPlay = possibleWins[r.Next(possibleWins.Length)];
                int move = int.Parse(chosenPlay.Substring(currentPlay.Length + 1 /*+1 to remove O or X*/, 2));
                Console.WriteLine("AI's Move is " + move);
                board[move / 10 - 1, move % 10 - 1] = "O";
                currentPlay += "O" + move + ",";
                return;
            }

            //follow instructions from file to avoid a loss
            Console.WriteLine("Avoidance");

            string currentPlaySwitch = currentPlay.Replace('X', 'o');
            currentPlaySwitch = currentPlaySwitch.Replace('O', 'X');
            currentPlaySwitch = currentPlaySwitch.Replace('o', 'O');
            StreamReader avoidSR = new StreamReader("wins.jwbl");
            string[] possibleLosses = avoidSR.ReadToEnd().Split('\n');
            possibleLosses = possibleLosses.Where(x => x != "" && x != "\r").ToArray(); ; //avoid empty file issue
            avoidSR.Close();
            possibleLosses = possibleLosses.Where(x => x.StartsWith(currentPlaySwitch)).ToArray();
            if (possibleLosses.Length > 0)
            {
                string[] possibleMoves = (string[])allMoves.Select(x => x.ToString()).ToArray().Clone();
                for (int i=0; i<possibleLosses.Length; i++)
                {
                    string badMove = possibleLosses[i].Substring(currentPlaySwitch.Length + 1 /*+1 to remove O or X*/, 2);
                    possibleMoves = possibleMoves.Where(x => x != badMove).ToArray();
                }
                if (possibleMoves.Length > 0)
                {
                    int move;
                    do {
                        move = int.Parse(possibleMoves[r.Next(possibleMoves.Length)]);
                    } while (board[move / 10 - 1, move % 10 - 1] != " ");
                    Console.WriteLine("AI's Move is " + move);
                    board[move / 10 - 1, move % 10 - 1] = "O";
                    currentPlay += "O" + move + ",";
                    return;
                }

            }

            //else do a random move
            Console.WriteLine("Random");

            bool okMove = false;
            while (!okMove)
            {
                int move = allMoves[r.Next(9)];
                if (board[move / 10 -1, move % 10-1] == " ")
                {
                    board[move / 10 -1, move % 10 -1] = "O";
                    currentPlay += "O" + move + ",";
                    Console.WriteLine("AI's Move is " + move);
                    okMove = true;
                    continue;
                }
            }

        }

        static void playerTurn(ref string[,] board)
        {
            Console.Out.WriteLine("Player's Turn");
            if (!fillWithRandom)
            {
                bool validMove = false;
                while (!validMove)
                {
                    Console.WriteLine("Enter your move");
                    int move = 0;
                    int.TryParse(Console.ReadLine(), out move);
                    if (move / 10 <= 3 && move / 10 > 0 && move % 10 <= 3 && move % 10 > 0)
                    {
                        if (board[move / 10 - 1, move % 10 - 1] == " ")
                        {
                            board[move / 10 - 1, move % 10 - 1] = "X";
                            currentPlay += "X" + move + ",";
                            validMove = true;
                            continue;
                        }
                    }
                    Console.WriteLine("Invalid move");
                }
            }
            else
            {
                bool okMove = false;
                while (!okMove)
                {
                    int move = allMoves[r.Next(9)];
                    if (board[move / 10 - 1, move % 10 - 1] == " ")
                    {
                        board[move / 10 - 1, move % 10 - 1] = "X";
                        currentPlay += "X" + move + ",";
                        Console.WriteLine("Fill Player's Move is " + move);
                        okMove =true;
                        continue;
                    }
                }
            }
        }

        static void initiateBoard(ref string[,] board)
        {
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = 0; j < SIZE; j++)
                {
                    board[j, i] = " ";
                }
            }
        }

        public static void displayBoard(string[,] board)
        {
            Console.WriteLine("\n");
            for (int i = 0; i < SIZE; i++)
            {
                Console.Write("\n\t" + (i + 1) + "\t");
                for (int j = 0; j < SIZE; j++)
                {
                    if (j < SIZE - 1)
                    {
                        Console.Out.Write(board[j, i] + "|");
                    }
                    else
                    {
                        Console.Out.Write(board[j, i] + "\n");
                    }
                }
                if (i < SIZE - 1)
                {
                    Console.WriteLine("\t\t_ _ _");
                }
            }
            Console.WriteLine("\n\t\t1 2 3");
            Console.WriteLine("\n");
        }

        public static bool validMove(int move)
        {
            return (move / 10 > 0 && move / 10 < 4 && move % 10 > 0 && move % 10 < 4);

        }

        public static bool gameIsOver(string[,] board, ref string winner)
        {
            for (int i = 0; i < SIZE; i++)
            {
                if (new string[] { board[0, i], board[1, i], board[2, i] }.All(x => x == "X") || new string[] { board[i, 0], board[i, 1], board[i, 2] }.All(x => x == "X"))
                {
                    winner = "X";
                    return true;
                }
                if (new string[] { board[0, i], board[1, i], board[2, i] }.All(x => x == "O") || new string[] { board[i, 0], board[i, 1], board[i, 2] }.All(x => x == "O"))
                {
                    winner = "O";
                    return true;
                }
            }
            if (new string[] { board[0, 0], board[1, 1], board[2, 2] }.All(x => x == "X") || new string[] { board[0, 2], board[1, 1], board[2, 0] }.All(x => x == "X"))
            {
                winner = "X";
                return true;
            }
            else if (new string[] { board[0, 0], board[1, 1], board[2, 2] }.All(x => x == "O") || new string[] { board[0, 2], board[1, 1], board[2, 0] }.All(x => x == "O"))
            {
                winner = "O";
                return true;
            }
            else if(toOneArray(board).All(x=> x == "O" || x == "X"))
            {
                winner = "none";
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string[] toOneArray(string[,] board)
        {
            string[] result = new string[9];
            for (int i=0; i<SIZE; i++)
            {
                for (int j=0; j<SIZE; j++)
                {
                    result[i * 3 + j] = board[j, i];
                }
            }
            return result;
        }
    }
}
