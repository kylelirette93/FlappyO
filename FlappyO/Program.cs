using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO.Pipes;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlappyO
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainMenu();
        }

        static void MainMenu()
        {
            // Set up menu screen.
            int screenWidth = 30;
            int screenHeight = 25;
            Console.SetWindowSize(screenWidth, screenHeight);
            Console.SetBufferSize(screenWidth, screenHeight);
            
            Console.WriteLine("Welcome to Flappy O!\n" +
                "Press Enter to play...\n" +
                "Press Escape to quit...");

            // Handle menu input.
            ConsoleKeyInfo key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                NewGame();
            }
            else if (key.Key == ConsoleKey.Escape)
            { 
                Environment.Exit(0);
            }
            else
            {
                // Prevent other keys from being pressed.
                Console.Clear();
                MainMenu();
            }
        }

        static void NewGame()
        {
            // Clear the console and create a new game class.
            Console.Clear();
            Game game = new Game();
            game.Start();
        }

        public class Game
        {
            // Bird variables
            private bool birdIsDead;
            int birdPositionY = 9;
            int birdPositionX = 9;
            

            // Screen size variables
            int screenHeight = 25;
            int screenWidth = 30;
            
            // Frame rate variables
            public int frameCount = 0;
            int spaceKeyPresses = 0;

            // Pipe variables
            int pipeX;
            int pipeY;
            int randomY;
            Pipe pipe;

            // Score variables
            int score = 0;
            const int point = 1;

            

            public void Start() 
            {
                // Setup display window.
                DisplayScreen();

                // Create an instance of pipe class.
                pipe = new Pipe();
               
                // Begin main game loop.
                while (!birdIsDead)
                {     
                    // Clear the console.
                    Console.Clear();

                    // Handle game states.
                    DisplayScore();
                    HandleInput();
                    pipe.DrawPipe();
                    pipe.MovePipe();
                    CollisionDetection();
                    
                    // If the bird dies, break out of main game loop.
                    if (birdIsDead)
                    {
                        break;
                    }

                    // Delay before redrawing screen.
                    Thread.Sleep(100);
                }
                
                GameOver();

            }

            void DisplayScreen()
            {
                // Set the window size, buffer size and disable the cursor.
                Console.WindowHeight = screenHeight;
                Console.WindowWidth = screenWidth;
                Console.BufferHeight = Console.WindowHeight;
                Console.BufferWidth = Console.WindowWidth;
                Console.CursorVisible = false;
            }

            void DisplayScore()
            {
                // Display the score in top left corner.
                Console.Write(" Score: " + score);
                Console.SetCursorPosition(0, 0);
            }

            void HandleInput()
            {        
                // Check if key has been pressed.
                if (Console.KeyAvailable)
                {
                    // Get information of key press and compare with spacebar.
                    ConsoleKeyInfo key = Console.ReadKey();
                    if (key.Key == ConsoleKey.Spacebar)
                    {
                        // Make the bird jump.
                        birdPositionY -= 2;
                    }
                }

                else if (birdPositionY <= 0)
                {
                    // If the bird reaches top of the screen, game over.
                    birdPositionY = 0;
                    birdIsDead = true;
                }
                else if (birdPositionY >= screenHeight - 1)
                {
                    // If bird falls below the screen, game over.
                    birdIsDead = true;
                }
                
                
                else
                {
                    // If no key has been pressed, just apply gravity.
                    ApplyGravity();
                }
                // Keep the bird's position within screen bounds to avoid overshooting.
                birdPositionY = Math.Max(0, Math.Min(birdPositionY, screenHeight - 1));

                // Update the bird's position.
                Console.SetCursorPosition(birdPositionX, birdPositionY);
                Console.Write("O");
            }      

            void ApplyGravity()
            {
                birdPositionY += 1;
            }

            void CollisionDetection()
            {
                if (birdPositionX == pipe.pipePositionX)
                {
                    // Check if the bird's y position is above or below the gap, if so the bird is dead.
                    if (birdPositionY < pipe.randomYPosition || birdPositionY > pipe.randomYPosition + pipe.gapHeight)
                    {
                        birdIsDead = true;
                    }
                    else
                    {
                        // Otherwise, if the bird goes through the gap, increment score.
                        Console.SetCursorPosition(birdPositionX, birdPositionY);
                        Console.Write("O");
                        IncrementScore(point);
                    }
                }
            }
            
            

            void IncrementScore(int points)
            {
                score += points;
            }

            void GameOver()
            {
                // Clear the game.
                Console.Clear();

                // Display game over text.
                Console.WriteLine("GAME OVER!");
                Console.WriteLine("PRESS ENTER TO PLAY AGAIN...");
                Console.WriteLine("PRESS ESC TO QUIT...");

                // Get key info for game over display and compare it with enter or escape button.
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    NewGame();
                    
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    // Quit the application.
                    Environment.Exit(0);
                }
                else
                {
                    Console.Clear();
                    GameOver();
                }
            }
        }       
    }

    public class Pipe
    {
        // Pipe variables.
        public int pipePositionX = 28;
        public int pipePositionY = 25;
        public int pipeMovement = -1;

        // Gap variables.
        public int randomYPosition;
        public int gapHeight = 2;
        

        public Pipe()
        {
            // Call a constructor of a class and create a random gap.
            // This avoids randomizing the gap within the main game loop.
            Random randomGap = new Random();
            randomYPosition = randomGap.Next(0, pipePositionY - gapHeight);
        }

        
        public void DrawPipe()
        {
            // Loop through screen height with small offsets.
            for (int i = 1; i <= pipePositionY - 2; i++)
            {
                // Draw a pipe everywhere except for the gap space.
                if (i < randomYPosition || i > randomYPosition + gapHeight)
                {
                    Console.SetCursorPosition(pipePositionX, i);
                    Console.Write("\u2590");
                }
                else
                {
                    // Draw the gap.
                    Console.SetCursorPosition(pipePositionX, i);
                    Console.Write("  ");
                }            
            }
            
        }

        void DrawGap()
        {
            Console.SetCursorPosition(pipePositionX, randomYPosition);
            Console.Write(" ");
        }
        

        
        public void MovePipe()
        {
            // Move the pipe to the left.
            pipePositionX += pipeMovement;     

            // When the pipes hits the left side of the screen, create a random gap.
            if (pipePositionX <= 0)
            {
                Random randomGap = new Random();
                randomYPosition = randomGap.Next(0, pipePositionY - gapHeight);

                // Reset pipe position. 
                pipePositionX = (pipePositionX + Console.WindowWidth) % Console.WindowWidth;          
            }

            // Draw another random gap.
            DrawGap();
        }     
    }
}
