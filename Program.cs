using System;
using System.Collections.Generic;
using System.Linq;
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
            public int maxGravity = 50;
            public int screenWidth = 1024;
            public int screenHeight = 768;
            public int screenMoveLimit = 350;
            public int score = 0;
            public int lifes = 3;
            public bool soundEnabled = false;
            public bool debugMode = true;
            public string menuMessage = "Foxy Runner";
            public string subMessage = "Presiona 'R' para comenzar";
            public float pauseCooldown = 0.3f;
            public float lastPauseTime = 0;
            public float takeDamageCooldown = 1f;
            public float lastTakeDamageTime = 0;
        }
        static readonly GameConfig gc = new GameConfig();
        static readonly Image[] fondo = new Image[10];
        static readonly Image[] ui = new Image[7];
        static readonly SoundPlayer[] sonidos = new SoundPlayer[2];
        static readonly Font[] font = new Font[6];
        static Character player;
        static Dictionary<string, Image[]> itemSprites;
        static Item[] items;
        enum GameState
        {
            Menu,
            Presentation,
            Playing,
            Paused,
            GameOver,
            Victory
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
            font[0] = Engine.LoadFont("assets/fonts/1.ttf", 80);
            font[1] = Engine.LoadFont("assets/fonts/1.ttf", 30);
            font[2] = Engine.LoadFont("assets/fonts/2.ttf", 15);
            font[3] = Engine.LoadFont("assets/fonts/2.ttf", 30);
            font[4] = Engine.LoadFont("assets/fonts/2.ttf", 50);
            font[5] = Engine.LoadFont("assets/fonts/3.ttf", 30);
            sonidos[0] = new SoundPlayer("assets/sounds/menu.wav");
            sonidos[1] = new SoundPlayer("assets/sounds/level.wav");
            fondo[0] = Engine.LoadImage("assets/backgrounds/fondo1.png");
            fondo[1] = Engine.LoadImage("assets/backgrounds/fondo2.png");
            fondo[2] = Engine.LoadImage("assets/backgrounds/fondo3.png");
            fondo[3] = Engine.LoadImage("assets/backgrounds/fondo4.png");
            fondo[4] = Engine.LoadImage("assets/backgrounds/parallax1.png");
            fondo[5] = Engine.LoadImage("assets/backgrounds/parallax1.png");
            fondo[6] = Engine.LoadImage("assets/backgrounds/parallax2.png");
            fondo[7] = Engine.LoadImage("assets/backgrounds/parallax2.png");
            fondo[8] = Engine.LoadImage("assets/backgrounds/parallax3.png");
            fondo[9] = Engine.LoadImage("assets/backgrounds/parallax3.png");
            ui[0] = Engine.LoadImage("assets/UI/title.png");
            ui[1] = Engine.LoadImage("assets/UI/hp.png");
            ui[2] = Engine.LoadImage("assets/UI/life.png");
            ui[3] = Engine.LoadImage("assets/UI/placeholder.png");
            ui[4] = Engine.LoadImage("assets/UI/hpbar.png");
            ui[5] = Engine.LoadImage("assets/UI/hpbar_inicial.png");
            ui[6] = Engine.LoadImage("assets/UI/hpbar_final.png");
            itemSprites = new Dictionary<string, Image[]>()
                {
                    {"Gem", new Image[]
                        {
                            Engine.LoadImage("assets/items/gem/1.png"),
                            Engine.LoadImage("assets/items/gem/2.png"),
                            Engine.LoadImage("assets/items/gem/3.png"),
                            Engine.LoadImage("assets/items/gem/4.png"),
                            Engine.LoadImage("assets/items/gem/5.png")
                        }
                    }
                };
            }
        static void Main(string[] args)
        {
            Engine.Initialize(gc.screenWidth, gc.screenHeight);
            LoadAssets();
            float previousTime = Sdl.SDL_GetTicks() / 1000f;
            IniciarMenu();

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
            public int Maxhp = 125;
            public int currentHp;
            public int speed = 4;
            public bool isJumping = false;
            public bool isTakingDamage = false;
            public bool isMoving = false;
            public bool isIdle = false;
            public int jumpVelocity = 0;
            public string animation = "idle_R";
            public string spriteDirection = "R";
            public int jumpStrength = 12;
            public int characterWidth = 32;
            public int characterHeight = 33;
            public int dir = 1;
            public int frame = 0;
            public float frameTimer = 0f;
            public Image image;
            public Dictionary<string, float> animationSpeed;
            public Dictionary<string, Image[]> animaciones;

            public Character(int startX, int startY)
            {
                posX = startX;
                posY = startY;
                currentHp = Maxhp;
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
                    {"hurt", 0.5f},
                    {"hurt2", 0.1f},
                    {"roll_L", 0.04f},
                    {"roll_R", 0.04f}
                };
            }
        }
        class Item
        {
            public int posX;
            public int posY;
            public int value = 5;
            public int speed = 4;
            public int width = 32;
            public int height = 33;
            public int frame = 0;
            public float frameTimer = 0f;
            public float animationSpeed = 0.1f;
            public Image image;
            public Image[] sprites;

            public Item(string id,int startX, int startY)
            {
                posX = startX;
                posY = startY;
                sprites = itemSprites[id];
                image = sprites[0];
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
            if (Engine.KeyPress(Engine.KEY_DOWN) || Engine.KeyPress(Engine.KEY_S)) TakeDamage(5);
            if (Engine.KeyPress(Engine.KEY_ESC)) Environment.Exit(0);
            if (Engine.KeyPress(Engine.KEY_R) && currentState == GameState.Menu)
            {
                currentState = GameState.Presentation;
                gc.subMessage = "Buena Suerte!";
            }
            if (currentState == GameState.Playing && !Engine.KeyPress(Engine.KEY_RIGHT) && !Engine.KeyPress(Engine.KEY_D) && !Engine.KeyPress(Engine.KEY_LEFT) && !Engine.KeyPress(Engine.KEY_A)) player.isMoving = false;
        }
        static void Render()
        {
            Engine.Clear();
            switch (currentState)
            {
                case GameState.Playing:
                    DrawBackground();
                    Engine.Draw(player.image, player.posX, player.posY);
                    DrawUI();
                    DrawItems();
                    break;

                case GameState.Paused:
                    DrawBackground();
                    DrawPause();
                    DrawUI();
                    break;

                case GameState.Menu:
                    DrawMenu();
                    break;

                case GameState.GameOver:
                    DrawGameOver();
                    break;

                case GameState.Presentation:
                    DrawPresentacion();
                    break;
            }
                Engine.Show();
        }
        static void Update(float deltaTime)
        {
            switch (currentState)
            {
                case GameState.Playing:
                    UpdateAnimations(deltaTime);
                    UpdateItems(deltaTime);
                    MoveItems();
                    AplicateGravity();
                    if (!player.isMoving && !player.isJumping && !player.isTakingDamage)
                    {
                        player.isIdle = true;
                        SetAnimation($"idle_{player.spriteDirection}");
                    }
                    else
                    {
                        player.isIdle = false;
                    }
                    break;
                case GameState.Presentation:
                    AnimarPresentacion();
                    break;
                default:
                    break;
            }
        
        }
        static void IniciarMenu()
        {
            currentState = GameState.Menu;
            if (gc.soundEnabled)
            {
            sonidos[0].PlayLooping();
            }
        }
        static void IniciarJuego()
        {
            posYFondo[2] = 0;
            sonidos[0].Stop();
            currentState = GameState.Playing;
            player = new Character(gc.screenWidth / 2 - 16, 0);
            player.isJumping = true;
            items = new Item[1];
            {
                items[0] = new Item("Gem",600,600);
            }
            if (gc.soundEnabled)
            {
                sonidos[1].PlayLooping();
            }
        }
        static void DrawUI()
        {
            Engine.Draw(ui[1], 75, 47); // HP Barra
            for (int i = 0; i < player.currentHp / 4; i++)
            {

                if (i > 0 && i < player.currentHp / 5 - 1)
                {
                    Engine.Draw(ui[4], 81 + i * 11, 53); // Barras intermedias
                }
                if (i == 0)
                {
                    Engine.Draw(ui[5], 81, 53); // Barra inicial
                }
                if (i == player.currentHp / 5 - 1 && player.currentHp / 5 > 1)
                {
                    Engine.Draw(ui[6], 81 + i * 11, 53); // Barra final
                }
            }
            Engine.Draw(ui[2], 10, 10); // Lifes
            Engine.DrawText($"Puntaje: {gc.score}", 820, 15, 232, 120, 55, font[3]);
            Engine.DrawText($"x {gc.lifes}", 15, 85, 232, 120, 55, font[4]);
            if (gc.debugMode)
            {
                Engine.DrawText($"Jumping: {player.isJumping.ToString()}", 25, 150, 232, 120, 55, font[3]);
                Engine.DrawText($"Moving: {player.isMoving.ToString()}", 25, 170, 232, 120, 55, font[3]);
                Engine.DrawText($"Idle: {player.isIdle.ToString()}", 25, 190, 232, 120, 55, font[3]);
                Engine.DrawText($"Damage: {player.isTakingDamage.ToString()}", 25, 210, 232, 120, 55, font[3]);
            }
        }
        static void DrawBackground()
        {
            Engine.Draw(fondo[1], 0, 0);
            Engine.Draw(fondo[4], posXParallax["1A"], 0);
            Engine.Draw(fondo[5], posXParallax["1B"], 0);
            Engine.Draw(fondo[6], posXParallax["2A"], 0);
            Engine.Draw(fondo[7], posXParallax["2B"], 0);
            Engine.Draw(fondo[8], posXParallax["3A"], 0);
            Engine.Draw(fondo[9], posXParallax["3B"], 0);
        }
        static void DrawItems()
        {
            for (int i = 0; i < items.Length; i++)
            {
                Engine.Draw(items[i].image, items[i].posX, items[i].posY);
            }
        }
        static void DrawPresentacion()
        {
            Engine.Draw(fondo[0], 0, posYFondo[0]);
            Engine.Draw(ui[0], gc.screenWidth / 2 - 320, posYFondo[0] + 190);
            Engine.Draw(fondo[2], 0, posYFondo[1]);
            Engine.Draw(fondo[3], 0, posYFondo[2]);
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 239, posYFondo[0] + 248, 232, 120, 55, font[0]);
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 235, posYFondo[0] + 250, 243, 198, 35, font[0]);
            Engine.DrawText($"{gc.subMessage}", gc.screenWidth / 2 - 120, posYFondo[0] + 450, 243, 198, 35, font[1]);
        }
        static void DrawGameOver()
        {
            Engine.Draw(fondo[0], 0, posYFondo[0]);
            gc.menuMessage = "Game Over";
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 239, posYFondo[0] + 248, 232, 120, 55, font[0]);
        }
        static void DrawMenu()
        {
            Engine.Draw(fondo[0], 0, posYFondo[0]);
            Engine.Draw(ui[0], gc.screenWidth / 2 - 320, posYFondo[0] + 195);
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 239, posYFondo[0] + 248, 232, 120, 55, font[0]);
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 235, posYFondo[0] + 250, 243, 198, 35, font[0]);
            Engine.DrawText($"{gc.subMessage}", gc.screenWidth / 2 - 170, posYFondo[0] + 450, 243, 198, 35, font[3]);
        }
        static void DrawPause()
        {
            Engine.Draw(player.image, player.posX, player.posY);
            Engine.Draw(ui[3], gc.screenWidth / 2 - 207, gc.screenHeight / 2 - 180); // Cartel
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 120, gc.screenHeight / 2 - 150, 232, 120, 55, font[0]);
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 116, gc.screenHeight / 2 - 148, 243, 198, 35, font[0]);
        }
        static void AplicateGravity()
        {
            
            player.posY += player.jumpVelocity;
            if (player.jumpVelocity <= gc.maxGravity)
            {
            player.jumpVelocity += gc.gravity;
            }
            if (player.jumpVelocity > 0 && player.isJumping)
            {
                SetAnimation($"roll_{player.spriteDirection}");
            }
            if (player.posY >= gc.groundLevel - player.characterHeight)
            {
                player.posY = gc.groundLevel - player.characterHeight;
                player.isJumping = false;
                player.jumpVelocity = 0;
            }
        }
        static void UpdateAnimations(float deltaTime)
        {
            player.frameTimer += deltaTime;
            player.spriteDirection = (player.dir > 0) ? player.spriteDirection = "R" : player.spriteDirection = "L";
            if (player.frameTimer >= player.animationSpeed[player.animation])
            {
                player.frame++;
                if (player.frame >= player.animaciones[player.animation].Length)
                {
                    player.frame = 0;
                }
                player.frameTimer = 0f;
                if (player.frame < player.animaciones[player.animation].Length)
                {
                    player.image = player.animaciones[player.animation][player.frame];
                }
            }
        }
        static void UpdateItems(float deltaTime)
        {       
            for (int i = 0; i < items.Length; i++)
            {
                items[i].frameTimer += deltaTime;
                if (items[i].frameTimer >= items[i].animationSpeed)
                {
                    items[i].frame++;
                    if (items[i].frame >= items[i].sprites.Length)
                    {
                        items[i].frame = 0;
                    }
                    items[i].frameTimer = 0f;
                    if (items[i].frame < items[i].sprites.Length)
                    {
                        items[i].image = items[i].sprites[items[i].frame];
                    }
                }
            }            
        }
        static void MoveItems()
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (player.posX <= gc.screenMoveLimit || player.posX >= gc.screenWidth - gc.screenMoveLimit)
                {
                    if ((!player.isIdle && !player.isJumping) || player.isMoving)
                    {
                        items[i].posX += items[i].speed * player.dir * -1;
                    }
                }
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
        static void Move(int direction)
        {
            if (currentState == GameState.Playing)
            {
                player.isMoving = true;
                player.dir = direction;
                if (player.posX >= gc.screenMoveLimit && direction < 0 || player.posX <= gc.screenWidth - gc.screenMoveLimit && direction > 0)
                {
                    player.posX += player.speed * direction;                    
                }
                else
                {
                    MoveParallax();
                }
                if (!player.isJumping && !player.isTakingDamage)
                {
                SetAnimation($"run_{player.spriteDirection}");
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
                    SetAnimation($"jump_{player.spriteDirection}");
                }
            }                
        }
        static void TakeDamage(int damage)
        {
            float currentTime = Sdl.SDL_GetTicks() / 1000f;
            if (currentTime - gc.lastTakeDamageTime >= gc.takeDamageCooldown && !player.isTakingDamage)
            {
                player.isTakingDamage = true;
                SetAnimation("hurt");
                player.currentHp -= damage;
                gc.lastTakeDamageTime = currentTime;
            }
            else
            { 
                player.isTakingDamage = false;
            }
            //SetAnimation("hurt");

            if (player.currentHp <= 0)
            {
                if (gc.lifes == 0)
                {
                    currentState = GameState.GameOver;
                }
                else
                {
                    gc.lifes--;
                    player.isTakingDamage = false;
                    player.currentHp = player.Maxhp;
                    player.posY = -32;
                    player.isJumping = true;
                    player.posX = gc.screenWidth / 2 - 16;
                }

            }
        }
        static void SetAnimation(string anim)
        {
            if (currentState == GameState.Playing)
            {
                if (player.animation != anim)
                {
                    player.frameTimer = player.animationSpeed[anim];
                    player.animation = anim;
                }
            }                
        }
        static void AnimarPresentacion()
        {
            for (int i = 0; i < posYFondo.Length; i++)
            {
                posYFondo[i] -= 7;
            }
            if (currentState == GameState.Presentation && posYFondo[2] <= 0)
            {
                IniciarJuego();
            }
        }
    }
}
