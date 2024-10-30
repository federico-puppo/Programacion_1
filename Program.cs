using System;
using System.Collections.Generic;
using System.Media;
using Tao.Sdl;

namespace MyGame
{

    class Program
    {
        class GameConfig
        {
            public int groundLevel = 718;
            public int gravity = 1;
            public int screenWidth = 1024;
            public int screenHeight = 768;
            public int screenMoveLimit = 350;
            public int score = 0;
            public bool soundEnabled = false;
            public string menuMessage = "Foxy Runner";
            public string subMessage = "Presiona 'R' para comenzar";
            public int frame = 0;
            public float frameTimer = 0f;
            public float frameDuration = 0.06f;
            public float pauseCooldown = 0.3f;
            public float lastPauseTime = 0;
        }

        static readonly GameConfig gc = new GameConfig();
        static readonly Image[] fondo = new Image[10];
        static readonly SoundPlayer[] sonidos = new SoundPlayer[2];
        static readonly Font[] font = new Font[2];
        static Character player;
        enum GameState
        {
            Menu,
            Presentation,
            Playing,
            Paused
        }
        static GameState currentState = GameState.Menu;
 
        static int[] posYFondo = new int[3]
        {
            0,gc.screenHeight,gc.screenHeight * 2
        };

        static Dictionary<string, int> posXParallax = new Dictionary<string, int>()
        {
            {"1A", 0},
            {"1B", gc.screenWidth},
            {"2A", 0},
            {"2B", gc.screenWidth},
            {"3A", 0},
            {"3B", gc.screenWidth}
        };
        static void LoadAssets()
        {
            font[0] = Engine.LoadFont("assets/font.ttf", 80);
            font[1] = Engine.LoadFont("assets/font.ttf", 30);
            sonidos[0] = new SoundPlayer("assets/sounds/menu.wav");
            sonidos[1] = new SoundPlayer("assets/sounds/level.wav");
            fondo[0] = Engine.LoadImage("assets/fondo1.png");
            fondo[1] = Engine.LoadImage("assets/fondo2.png");
            fondo[2] = Engine.LoadImage("assets/fondo3.png");
            fondo[3] = Engine.LoadImage("assets/fondo4.png");
            fondo[4] = Engine.LoadImage("assets/parallax1.png");
            fondo[5] = Engine.LoadImage("assets/parallax1.png");
            fondo[6] = Engine.LoadImage("assets/parallax2.png");
            fondo[7] = Engine.LoadImage("assets/parallax2.png");
            fondo[8] = Engine.LoadImage("assets/parallax3.png");
            fondo[9] = Engine.LoadImage("assets/parallax3.png");
        }

        static void Main(string[] args)
        {
            Engine.Initialize(gc.screenWidth, gc.screenHeight);
            LoadAssets();
            float previousTime = Sdl.SDL_GetTicks() / 1000f;
            IniciarJuego();

            while (true)
            {
                CheckInputs();
                if (currentState != GameState.Paused)
                {
                    float currentTime = Sdl.SDL_GetTicks() / 1000f;
                    float deltaTime = currentTime - previousTime;
                    previousTime = currentTime;
                    Update(deltaTime);
                }
                Render();
            }
        }

        class Character
        {
            public int posX;
            public int posY;
            public int speed = 4;
            public bool isJumping = false;
            public int jumpVelocity = 0;
            public string animation = "idle_R";
            public int jumpStrength = 12;
            public int characterWidth = 32;
            public int characterHeight = 33;
            public int dir = 1;
            public Image image;
            public Dictionary<string, float> animationSpeed;
            public Dictionary<string, Image[]> animaciones;

            public Character(int startX, int startY)
            {
                posX = startX;
                posY = startY;
                LoadCharacterAssets();
                image = animaciones[animation][0];
            }

            private void LoadCharacterAssets()
            {
                animaciones = new Dictionary<string, Image[]>()
                {
                    {"idle_R", new Image[]
                        {
                            Engine.LoadImage("assets/character/idle_R/1.png"),
                            Engine.LoadImage("assets/character/idle_R/2.png"),
                            Engine.LoadImage("assets/character/idle_R/3.png"),
                            Engine.LoadImage("assets/character/idle_R/4.png")
                        }
                    },
                    {"idle_L", new Image[]
                        {
                            Engine.LoadImage("assets/character/idle_L/1.png"),
                            Engine.LoadImage("assets/character/idle_L/2.png"),
                            Engine.LoadImage("assets/character/idle_L/3.png"),
                            Engine.LoadImage("assets/character/idle_L/4.png")
                        }
                    },
                    {"run_L", new Image[]
                        {
                            Engine.LoadImage("assets/character/run_L/1.png"),
                            Engine.LoadImage("assets/character/run_L/2.png"),
                            Engine.LoadImage("assets/character/run_L/3.png"),
                            Engine.LoadImage("assets/character/run_L/4.png"),
                            Engine.LoadImage("assets/character/run_L/5.png"),
                            Engine.LoadImage("assets/character/run_L/6.png")
                        }
                    },
                    {"run_R", new Image[]
                        {
                            Engine.LoadImage("assets/character/run_R/1.png"),
                            Engine.LoadImage("assets/character/run_R/2.png"),
                            Engine.LoadImage("assets/character/run_R/3.png"),
                            Engine.LoadImage("assets/character/run_R/4.png"),
                            Engine.LoadImage("assets/character/run_R/5.png"),
                            Engine.LoadImage("assets/character/run_R/6.png")
                        }
                    },
                    {"jump_R", new Image[]
                        {
                            Engine.LoadImage("assets/character/jump_R/1.png"),
                            Engine.LoadImage("assets/character/jump_R/2.png")
                        }
                    },
                    {"jump_L", new Image[]
                        {
                            Engine.LoadImage("assets/character/jump_L/1.png"),
                            Engine.LoadImage("assets/character/jump_L/2.png")
                        }
                    },
                    {"crouch", new Image[]
                        {
                            Engine.LoadImage("assets/character/crouch/1.png"),
                            Engine.LoadImage("assets/character/crouch/2.png")
                        }
                    },
                    {"hurt", new Image[]
                        {
                            Engine.LoadImage("assets/character/hurt/1.png"),
                            Engine.LoadImage("assets/character/hurt/2.png")
                        }
                    },
                     {"hurt2", new Image[]
                        {
                            Engine.LoadImage("assets/character/hurt2/1.png"),
                            Engine.LoadImage("assets/character/hurt2/2.png"),
                            Engine.LoadImage("assets/character/hurt2/3.png"),
                            Engine.LoadImage("assets/character/hurt2/4.png"),
                            Engine.LoadImage("assets/character/hurt2/5.png"),
                            Engine.LoadImage("assets/character/hurt2/6.png"),
                            Engine.LoadImage("assets/character/hurt2/7.png")
                        }
                    },
                    {"roll_R", new Image[]
                        {
                            Engine.LoadImage("assets/character/roll_R/1.png"),
                            Engine.LoadImage("assets/character/roll_R/2.png"),
                            Engine.LoadImage("assets/character/roll_R/3.png"),
                            Engine.LoadImage("assets/character/roll_R/4.png")
                        }
                    },
                    {"roll_L", new Image[]
                        {
                            Engine.LoadImage("assets/character/roll_L/1.png"),
                            Engine.LoadImage("assets/character/roll_L/2.png"),
                            Engine.LoadImage("assets/character/roll_L/3.png"),
                            Engine.LoadImage("assets/character/roll_L/4.png")
                        }
                    }
                };

                animationSpeed = new Dictionary<string, float>()
                {
                    {"idle_R", 0.15f},
                    {"idle_L", 0.15f},
                    {"run_L", 0.06f},
                    {"run_R", 0.06f},
                    {"jump_R", 0.18f},
                    {"jump_L", 0.18f},
                    {"crouch", 0.1f},
                    {"hurt", 0.1f},
                    {"hurt2", 0.1f},
                    {"roll_L", 0.04f},
                    {"roll_R", 0.04f}
                };
            }
        }

        static void CheckInputs()
        {
            float currentTime = Sdl.SDL_GetTicks() / 1000f;
            if (Engine.KeyPress(Engine.KEY_P) && currentState != GameState.Menu && currentState != GameState.Presentation && (currentTime - gc.lastPauseTime > gc.pauseCooldown))
            {
                gc.lastPauseTime = currentTime;
                gc.menuMessage = "PAUSA";
                currentState = (currentState == GameState.Paused) ? GameState.Playing : GameState.Paused;
            }
            if (Engine.KeyPress(Engine.KEY_RIGHT) || Engine.KeyPress(Engine.KEY_D)) Move(1);
            if (Engine.KeyPress(Engine.KEY_LEFT) || Engine.KeyPress(Engine.KEY_A)) Move(-1);
            if (Engine.KeyPress(Engine.KEY_UP) || Engine.KeyPress(Engine.KEY_W) || Engine.KeyPress(Engine.KEY_ESP)) Jump();
            if (Engine.KeyPress(Engine.KEY_DOWN) || Engine.KeyPress(Engine.KEY_S)) SetAnimation("crouch");
            if (Engine.KeyPress(Engine.KEY_ESC)) Environment.Exit(0);
            if (Engine.KeyPress(Engine.KEY_R) && currentState == GameState.Menu)
            {
                currentState = GameState.Presentation;
                gc.subMessage = "buena suerte...";
            }
        }
        static void Render()
        {
            Engine.Clear();
            switch (currentState)
            {
                case GameState.Playing:
                    Engine.Draw(fondo[1], 0, 0);
                    Engine.Draw(fondo[4], posXParallax["1A"], 0);
                    Engine.Draw(fondo[5], posXParallax["1B"], 0);
                    Engine.Draw(fondo[6], posXParallax["2A"], 0);
                    Engine.Draw(fondo[7], posXParallax["2B"], 0);
                    Engine.Draw(fondo[8], posXParallax["3A"], 0);
                    Engine.Draw(fondo[9], posXParallax["3B"], 0);
                    Engine.Draw(player.image, player.posX, player.posY);
                    Engine.DrawText($"Puntaje: {gc.score}",20, 15, 232, 120, 55, font[1]);
                    break;
                case GameState.Paused:
                    Engine.Draw(fondo[1], 0, 0);
                    Engine.Draw(fondo[4], posXParallax["1A"], 0);
                    Engine.Draw(fondo[5], posXParallax["1B"], 0);
                    Engine.Draw(fondo[6], posXParallax["2A"], 0);
                    Engine.Draw(fondo[7], posXParallax["2B"], 0);
                    Engine.Draw(fondo[8], posXParallax["3A"], 0);
                    Engine.Draw(fondo[9], posXParallax["3B"], 0);
                    Engine.Draw(player.image, player.posX, player.posY);
                    Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 120, gc.screenHeight / 2 - 150, 243, 198, 35, font[0]);
                    Engine.DrawText($"Puntaje: {gc.score}", 20, 15, 232, 120, 55, font[1]);

                    break;
                case GameState.Menu:
                    Engine.Draw(fondo[0], 0, posYFondo[0]);
                    Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 250, posYFondo[0] + 250, 243, 198, 35, font[0]);
                    Engine.DrawText($"{gc.subMessage}", gc.screenWidth / 2 - 210, posYFondo[0] + 450, 243, 198, 35, font[1]);
                    break;
                case GameState.Presentation:
                    Engine.Draw(fondo[0], 0, posYFondo[0]);
                    Engine.Draw(fondo[2], 0, posYFondo[1]);
                    Engine.Draw(fondo[3], 0, posYFondo[2]);
                    Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 250, posYFondo[0] + 250, 243, 198, 35, font[0]);
                    Engine.DrawText($"{gc.subMessage}", gc.screenWidth / 2 - 120, posYFondo[0] + 450, 243, 198, 35, font[1]);
                    break;
                }
                Engine.Show();
        }

        static void Update(float deltaTime)
        {
            switch (currentState)
            {
                case GameState.Playing:
                    CheckAnimations(deltaTime);
                    AplicateGravity();
                    break;
                case GameState.Presentation:
                    AnimarPresentacion(deltaTime);
                    break;
                default:
                    break;
            }
        }

        static void IniciarJuego()
        {
            currentState = GameState.Menu;
            if (gc.soundEnabled)
            {
            sonidos[0].PlayLooping();
            }
        }

        static void AplicateGravity()
        {
                player.posY += player.jumpVelocity;
                player.jumpVelocity += gc.gravity;
                if (player.jumpVelocity > 0)
                {
                    string dir = (player.dir > 0) ? dir = "R" : dir = "L";
                    SetAnimation($"roll_{dir}");
                }
                if (player.posY >= gc.groundLevel - player.characterHeight)
                {
                    player.posY = gc.groundLevel - player.characterHeight;
                    player.isJumping = false;
                    string dir = (player.dir > 0) ? dir = "R" : dir = "L";
                    SetAnimation($"idle_{dir}");
                }
           
        }
        static void MoveParallax()
        {
            posXParallax["1A"] -= player.speed / 4 * player.dir;
            posXParallax["1B"] -= player.speed / 4 * player.dir;
            posXParallax["2A"] -= player.speed / 2 * player.dir;
            posXParallax["2B"] -= player.speed / 2 * player.dir;
            posXParallax["3A"] -= player.speed * player.dir;
            posXParallax["3B"] -= player.speed * player.dir;
            CheckParallax();
        }

        static void CheckParallax()
        {
                for (int i = 1; i < posXParallax.Count/2 + 1; i++)
                {
                    //FONDO A llega al limite por la izquierda -> fondo A entra por la derecha
                    if (posXParallax[$"{i}A"] <= -gc.screenWidth)
                    {
                        posXParallax[$"{i}A"] = posXParallax[$"{i}B"] + gc.screenWidth;
                    }
                    //FONDO A Sale por la derecha -> fondo B entra por la izquierda
                    if (posXParallax[$"{i}A"] > 0)
                    {
                        posXParallax[$"{i}B"] = posXParallax[$"{i}A"] - gc.screenWidth;
                    }
                    //FONDO B llega al limite por la izquierda -> fondo B entra por la derecha
                    if (posXParallax[$"{i}B"] <= - gc.screenWidth)
                    {
                        posXParallax[$"{i}B"] = posXParallax[$"{i}A"] + gc.screenWidth;
                    }
                    //FONDO B Sale por la derecha -> fondo A entra por la izquierda
                    if (posXParallax[$"{i}B"] > 0)
                    {
                        posXParallax[$"{i}A"] = posXParallax[$"{i}B"] - gc.screenWidth;
                    }
                }
        }
        static void CheckAnimations(float deltaTime)
        {
            gc.frameTimer += deltaTime;
            if (gc.frameTimer >= gc.frameDuration)
            {
                gc.frame++;
                if (gc.frame >= player.animaciones[player.animation].Length)
                {
                    gc.frame = 0;
                }
                gc.frameTimer = 0f;
                if (gc.frame < player.animaciones[player.animation].Length)
                {
                    player.image = player.animaciones[player.animation][gc.frame];
                }
            }              
        }

        static void Move(int direction)
        {
            if (currentState == GameState.Playing)
            {
                player.dir = direction;
                string dir = (player.dir > 0) ? dir = "R" : dir = "L";
                if (player.posX >= gc.screenMoveLimit && direction < 0 || player.posX <= gc.screenWidth - gc.screenMoveLimit && direction > 0)
                {
                    player.posX += player.speed * direction;
                }
                else
                {
                    MoveParallax();
                }
                if (!player.isJumping)
                {
                    SetAnimation($"run_{dir}");
                }
            }
        }

        static void Jump()
        {
            if (currentState == GameState.Playing)
            {
                if (!player.isJumping)
                {
                    player.isJumping = true;
                    player.jumpVelocity = -player.jumpStrength;
                    string dir = (player.dir > 0) ? dir = "R" : dir = "L";
                    SetAnimation($"jump_{dir}");
                }
            }                
        }

        static void SetAnimation(string anim)
        {
            if (currentState == GameState.Playing)
            {
                if (player.animation != anim)
                {
                    gc.frameDuration = player.animationSpeed[anim];
                    player.animation = anim;
                }
            }                
        }

        static void AnimarPresentacion(float deltaTime)
        {
            float frameDuration = 0.03f;
            gc.frameTimer += deltaTime;
            if (gc.frameTimer >= frameDuration)
            {
                for (int i = 0; i < posYFondo.Length; i++)
                {
                    posYFondo[i] -= 7;
                }
                if (currentState == GameState.Presentation && posYFondo[2] <= 0)
                {
                    posYFondo[2] = 0;
                    sonidos[0].Stop();
                    currentState = GameState.Playing;
                    player = new Character(gc.screenWidth / 2 - 16, 0);
                    if (gc.soundEnabled)
                    {
                        sonidos[1].PlayLooping();
                    }
                }
            }
        }
    }
}
