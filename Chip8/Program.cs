using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SFML.Graphics;
using SFML.Window;

namespace Chip8
{
    class Program
    {
        static void Main(string[] args)
        {
            //args = new string[1] { "c8games/TICTAC" };
            if (args.Length <= 0)
            {
                Console.WriteLine("No game dragged into exe");
                return;
            }
            else
            {
                Emulator em = new Emulator();
                //Console.WriteLine(args[0]);
                string FILELOAD = args[0];
                em.LoadROM(FILELOAD);

                RenderWindow window = new RenderWindow(new VideoMode(900, 600), "Chip8.net");
                window.Closed += new EventHandler(window_Closed);

                float TILESIZEX = (float)window.Size.X / em.Screen.GetLength(0);
                float TILESIZEY = (float)window.Size.Y / em.Screen.GetLength(1);
                RectangleShape shape = new RectangleShape(new Vector2f(TILESIZEX, TILESIZEY));
                window.SetFramerateLimit(60);
                window.SetTitle("Chip8.net PLAYING: " + FILELOAD);
                while (window.IsOpen())
                {
                    bool changescreen = em.Step();
                    //Console.WriteLine(changescreen);
                    if (changescreen)
                    {
                        window.DispatchEvents();

                        //window.Clear();


                        for (int x = 0; x < em.Screen.GetLength(0); x++)
                        {
                            for (int y = 0; y < em.Screen.GetLength(1); y++)
                            {
                                shape.Position = new Vector2f(x * TILESIZEX, y * TILESIZEY);
                                if (em.Screen[x, y] != 0)
                                {
                                    shape.FillColor = new Color(255, 255, 255);
                                }
                                else
                                {
                                    shape.FillColor = new Color(100, 100, 100);
                                }
                                window.Draw(shape);
                            }
                        }

                        em.SendInput(0, Keyboard.IsKeyPressed(Keyboard.Key.Num0));
                        em.SendInput(1, Keyboard.IsKeyPressed(Keyboard.Key.Num2));
                        em.SendInput(2, Keyboard.IsKeyPressed(Keyboard.Key.Num3));
                        em.SendInput(3, Keyboard.IsKeyPressed(Keyboard.Key.Num4));
                        em.SendInput(4, Keyboard.IsKeyPressed(Keyboard.Key.Q));
                        em.SendInput(5, Keyboard.IsKeyPressed(Keyboard.Key.W));
                        em.SendInput(6, Keyboard.IsKeyPressed(Keyboard.Key.E));
                        em.SendInput(7, Keyboard.IsKeyPressed(Keyboard.Key.R));
                        em.SendInput(8, Keyboard.IsKeyPressed(Keyboard.Key.A));
                        em.SendInput(9, Keyboard.IsKeyPressed(Keyboard.Key.S));
                        em.SendInput(10, Keyboard.IsKeyPressed(Keyboard.Key.D));
                        em.SendInput(11, Keyboard.IsKeyPressed(Keyboard.Key.F));
                        em.SendInput(12, Keyboard.IsKeyPressed(Keyboard.Key.Z));
                        em.SendInput(13, Keyboard.IsKeyPressed(Keyboard.Key.X));
                        em.SendInput(14, Keyboard.IsKeyPressed(Keyboard.Key.C));
                        em.SendInput(15, Keyboard.IsKeyPressed(Keyboard.Key.V));

                        window.Display();
                    }
                }
            }
        }

        static void window_Closed(object sender, EventArgs e)
        {
            ((RenderWindow)sender).Close();
        }
    }
}
