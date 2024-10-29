using System;
using System.Collections.Generic;
using System.Media;
using Tao.Sdl;

namespace MyGame
{
    class Program
    {
        static Image[] fondo = new Image[10];
        static SoundPlayer[] sonidos = new SoundPlayer[2];
        static Font[] font = new Font[2];
        static Character player;
        enum GameState
        {
            Menu,
            Presentation,
            Playing,
            Paused
        }
        static GameState currentState = GameState.Menu;
        static int groundLevel = 718;
        static int gravity = 1;
        static string menuMessage = "Foxy Runner";
        static string subMessage = "Presiona 'R' para comenzar";
        static int ScreenWidth = 1024;
        static int ScreenHeight = 768;

        static int posYFondo1 = 0;
        static int posYFondo2 = ScreenHeight;
        static int posYFondo3 = ScreenHeight * 2;
        static int posXParallax1A = 0;
        static int posXParallax1B = ScreenWidth;
        static int posXParallax2A = 0;
        static int posXParallax2B = ScreenWidth;
        static int posXParallax3A = 0;
        static int posXParallax3B = ScreenWidth;
        static int frame = 0;
        static float frameTimer = 0f;
        static float frameDuration = 0.06f;
        static float pauseCooldown = 0.3f;
        static float lastPauseTime = 0;

        static void LoadAssets()
        {
            font[0] = Engine.LoadFont("assets/font.ttf", 80);
            font[1] = Engine.LoadFont("assets/font.ttf", 30);
            sonidos[0] = new SoundPlayer("assets/sounds/menu.wav");
            sonidos[1] = new SoundPlayer("assets/sounds/level2.wav");
            fondo[0] = Engine.LoadImage("assets/fondo.png");
            fondo[1] = Engine.LoadImage("assets/fondo4.png");
            fondo[2] = Engine.LoadImage("assets/fondo2.png");
            fondo[3] = Engine.LoadImage("assets/fondo3.png");
            fondo[4] = Engine.LoadImage("assets/parallax1.png");
            fondo[5] = Engine.LoadImage("assets/parallax1.png");
            fondo[6] = Engine.LoadImage("assets/parallax2.png");
            fondo[7] = Engine.LoadImage("assets/parallax2.png");
            fondo[8] = Engine.LoadImage("assets/parallax3.png");
            fondo[9] = Engine.LoadImage("assets/parallax3.png");
        }

        static void Main(string[] args)
        {
            Engine.Initialize(ScreenWidth, ScreenHeight);
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
            public bool isAnimating = false;
            public int jumpVelocity = 0;
            public string animation = "idle";
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
                image = animaciones["idle_R"][0];
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
            if (Engine.KeyPress(Engine.KEY_P) && currentState != GameState.Menu && currentState != GameState.Presentation && (currentTime - lastPauseTime > pauseCooldown))
            {
                lastPauseTime = currentTime;
                menuMessage = "PAUSA";
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
                subMessage = "buena suerte...";
            }
        }
        static void Render()
        {
            Engine.Clear();
            switch (currentState)
            {
                case GameState.Playing:
                    Engine.Draw(fondo[1], 0, 0);
                    Engine.Draw(fondo[4], posXParallax1A, 0);
                    Engine.Draw(fondo[5], posXParallax1B, 0);
                    Engine.Draw(fondo[6], posXParallax2A, 0);
                    Engine.Draw(fondo[7], posXParallax2B, 0);
                    Engine.Draw(fondo[8], posXParallax3A, 0);
                    Engine.Draw(fondo[9], posXParallax3B, 0);
                    Engine.Draw(player.image, player.posX, player.posY);
                    break;
                case GameState.Paused:
                    Engine.Draw(fondo[1], 0, 0);
                    Engine.Draw(fondo[4], posXParallax1A, 0);
                    Engine.Draw(fondo[5], posXParallax1B, 0);
                    Engine.Draw(fondo[6], posXParallax2A, 0);
                    Engine.Draw(fondo[7], posXParallax2B, 0);
                    Engine.Draw(fondo[8], posXParallax3A, 0);
                    Engine.Draw(fondo[9], posXParallax3B, 0);
                    Engine.Draw(player.image, player.posX, player.posY);
                    Engine.DrawText($"{menuMessage}", ScreenWidth / 2 - 120, ScreenHeight / 2 - 150, 243, 198, 35, font[0]);
                    break;
                case GameState.Menu:
                    Engine.Draw(fondo[0], 0, posYFondo1);
                    Engine.DrawText($"{menuMessage}", ScreenWidth / 2 - 250, posYFondo1 + 250, 243, 198, 35, font[0]);
                    Engine.DrawText($"{subMessage}", ScreenWidth / 2 - 210, posYFondo1 + 450, 243, 198, 35, font[1]);
                    break;
                case GameState.Presentation:
                    Engine.Draw(fondo[0], 0, posYFondo1);
                    Engine.Draw(fondo[2], 0, posYFondo2);
                    Engine.Draw(fondo[3], 0, posYFondo3);
                    Engine.DrawText($"{menuMessage}", ScreenWidth / 2 - 250, posYFondo1 + 250, 243, 198, 35, font[0]);
                    Engine.DrawText($"{subMessage}", ScreenWidth / 2 - 120, posYFondo1 + 450, 243, 198, 35, font[1]);
                    break;
                }
                Engine.Show();
        }

        static void Update(float deltaTime)
        {

            CheckAnimations(deltaTime);
            CheckParallax();
            AplicateGravity();
            if (currentState == GameState.Presentation)
            {
                AnimarPresentacion(deltaTime);
            }
        }

        static void IniciarJuego()
        {
            currentState = GameState.Menu;
            //sonidos[0].PlayLooping();
        }

        static void Animate(int frame)
        {
            if (frame < player.animaciones[player.animation].Length)
            {
                player.image = player.animaciones[player.animation][frame];
            }
        }

        static void AplicateGravity()
        {
            if (currentState == GameState.Playing)
            {
                player.posY += player.jumpVelocity;
                player.jumpVelocity += gravity;
                if (player.jumpVelocity > 0)
                {
                    string dir = (player.dir > 0) ? dir = "R" : dir = "L";
                    SetAnimation($"roll_{dir}");
                }
                if (player.posY >= groundLevel - player.characterHeight)
                {
                    player.posY = groundLevel - player.characterHeight;
                    player.isJumping = false;
                    string dir = (player.dir > 0) ? dir = "R" : dir = "L";
                    SetAnimation($"idle_{dir}");
                }
            }
           
        }
        static void MoveParallax()
        {
                posXParallax1A -= player.speed / 4 * player.dir;
                posXParallax1B -= player.speed / 4 * player.dir;
                posXParallax2A -= player.speed / 2 * player.dir;
                posXParallax2B -= player.speed / 2 * player.dir;
                posXParallax3A -= player.speed * player.dir;
                posXParallax3B -= player.speed * player.dir;
        }
        static void CheckParallax()
        {
            if (currentState != GameState.Paused)
            {
                //PRIMERA CAPA PARALLAX
                //FONDO 1A Sale por la izquierda -> fondo 1A entra por la derecha
                if (posXParallax1A <= -ScreenWidth)
                {
                    posXParallax1A = posXParallax1B + ScreenWidth;
                }
                //FONDO 1A Sale por la derecha -> fondo 1B entra por la izquierda
                if (posXParallax1A > 0)
                {
                    posXParallax1B = posXParallax1A - ScreenWidth;
                }
                //FONDO 1B Sale por la izquierda -> fondo 1B entra por la derecha
                if (posXParallax1B <= -ScreenWidth)
                {
                    posXParallax1B = posXParallax1A + ScreenWidth;
                }
                //FONDO 1B Sale por la derecha -> fondo 1A entra por la izquierda
                if (posXParallax1B > 0)
                {
                    posXParallax1A = posXParallax1B - ScreenWidth;
                }
                //SEGUNDA CAPA PARALLAX
                //FONDO 2A Sale por la izquierda -> fondo 2A entra por la derecha
                if (posXParallax2A <= -ScreenWidth)
                {
                    posXParallax2A = posXParallax2B + ScreenWidth;
                }
                //FONDO 2A Sale por la derecha -> fondo 2B entra por la izquierda
                if (posXParallax2A > 0)
                {
                    posXParallax2B = posXParallax2A - ScreenWidth;
                }
                //FONDO 2B Sale por la izquierda -> fondo 2B entra por la derecha
                if (posXParallax2B <= -ScreenWidth)
                {
                    posXParallax2B = posXParallax2A + ScreenWidth;
                }
                //FONDO 2B Sale por la derecha -> fondo 2A entra por la izquierda
                if (posXParallax2B > 0)
                {
                    posXParallax2A = posXParallax2B - ScreenWidth;
                }
                //TERCER CAPA PARALLAX
                //FONDO 3A Sale por la izquierda -> fondo 3A entra por la derecha
                if (posXParallax3A <= -ScreenWidth)
                {
                    posXParallax3A = posXParallax3B + ScreenWidth;
                }
                //FONDO 3A Sale por la derecha -> fondo 3B entra por la izquierda
                if (posXParallax3A > 0)
                {
                    posXParallax3B = posXParallax3A - ScreenWidth;
                }
                //FONDO 3B Sale por la izquierda -> fondo 3B entra por la derecha
                if (posXParallax3B <= -ScreenWidth)
                {
                    posXParallax3B = posXParallax3A + ScreenWidth;
                }
                //FONDO 3B Sale por la derecha -> fondo 3A entra por la izquierda
                if (posXParallax3B > 0)
                {
                    posXParallax3A = posXParallax3B - ScreenWidth;
                }
            }           
        }

        static void CheckAnimations(float deltaTime)
        {
            if (currentState == GameState.Playing)
            {
                frameTimer += deltaTime;
                if (frameTimer >= frameDuration)
                {
                    if (player.isAnimating)
                    {
                        frame++;
                        if (frame >= player.animaciones[player.animation].Length)
                        {
                            frame = 0;
                        }
                        frameTimer = 0f;
                    }
                    else
                    {
                        string dir = (player.dir > 0) ? dir = "R" : dir = "L";
                        SetAnimation($"idle_{dir}");
                        player.isAnimating = true;
                    }
                    if (frame < player.animaciones[player.animation].Length)
                    {
                        player.image = player.animaciones[player.animation][frame];
                    }
                }
            }                
        }

        static void Move(int direction)
        {
            if (currentState == GameState.Playing)
            {
                player.dir = direction;
                string dir = (player.dir > 0) ? dir = "R" : dir = "L";
                if (player.posX >= 250 && direction < 0 || player.posX <= ScreenWidth - 250 && direction > 0)
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
                    player.isAnimating = true;
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
                    frameDuration = player.animationSpeed[anim];
                    player.animation = anim;
                }
            }                
        }

        static void AnimarPresentacion(float deltaTime)
        {
            float frameDuration = 0.03f;
            frameTimer += deltaTime;
            if (frameTimer >= frameDuration)
            {
                posYFondo1 -= 7;
                posYFondo2 -= 7;
                posYFondo3 -= 7;
                if (currentState == GameState.Presentation && posYFondo3 <= 0)
                {
                    posYFondo3 = 0;
                    sonidos[0].Stop();
                    currentState = GameState.Playing;
                    player = new Character(ScreenWidth / 2 - 16, 0);
                    //sonidos[1].PlayLooping();
                }
            }
        }
    }
}
