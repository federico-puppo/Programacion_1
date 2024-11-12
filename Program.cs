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
            public float gravity = 9.8f;
            public int screenWidth = 1024;
            public int screenHeight = 768;
            public int screenMoveLimit = 350;
            public int score = 0;
            public int lifes = 2;
            public int scoreToWin = 1500;
            public int timer;
            public DateTime startTime;
            public bool soundEnabled = true;
            public bool debugMode = false;
            public string menuMessage = "Foxy Runner";
            public string subMessage = "Presiona 'R' para comenzar";
            public float keyCooldown = 0.3f;
            public float lastKeyPressTime = 0;
            public float takeDamageCooldown = 1f;
            public float lastTakeDamageTime = 0;
            public float spawnEnemyCooldown = 5f;
            public float lastSpawnEnemyTime = 0;
            public float spawnItemCooldown = 1.2f;
            public float lastSpawnItemTime = 0;

        }
        static readonly GameConfig gc = new GameConfig();
        static readonly Image[] fondo = new Image[12];
        static readonly Image[] ui = new Image[7];
        static readonly SoundPlayer[] sonidos = new SoundPlayer[2];
        static readonly Font[] font = new Font[6];
        static Player player;
        static Dictionary<string, Image[]> itemSprites;
        static Dictionary<string, Image[]> enemiesSprites;
        static List<Item> items = new List<Item>();
        static List<Enemy> enemies = new List<Enemy>();
        static Random random = new Random();
        /// <summary>
        /// Estados de juego disponibles
        /// </summary>
        enum GameState
        {
            Menu,
            Presentation,
            Playing,
            Paused,
            GameOver,
            Victory
        }
        /// <summary>
        /// Enemigos disponibles en el juego
        /// </summary>
        enum Enemies
        {
            Bear,
            Bird
        }
        /// <summary>
        /// items disponibles en el juego
        /// </summary>
        enum Items
        {
            Gem,
            Cherry,
            Egg
        }
        enum SpawnPattern
        {
            row,
            line,
            doble,
            square,
            single            
        }
        static GameState gameState = GameState.Menu;
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
        static float previousTime;
        static float currentTime;
        static float deltaTime = currentTime - previousTime;
        /// <summary>
        /// Carga los assets que se utilizan en el juego (fuentes,sonidos,fondos,ui,etc)
        /// </summary>
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
            fondo[10] = Engine.LoadImage("assets/backgrounds/victory.png");
            fondo[11] = Engine.LoadImage("assets/backgrounds/defeat.png");
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
                },
                {"Cherry", new Image[]
                    {
                        Engine.LoadImage("assets/items/cherry/1.png"),
                        Engine.LoadImage("assets/items/cherry/2.png"),
                        Engine.LoadImage("assets/items/cherry/3.png"),
                        Engine.LoadImage("assets/items/cherry/4.png"),
                        Engine.LoadImage("assets/items/cherry/5.png"),
                        Engine.LoadImage("assets/items/cherry/6.png"),
                        Engine.LoadImage("assets/items/cherry/7.png")
                    }
                },
                {"Egg", new Image[]
                    {
                        Engine.LoadImage("assets/items/egg/1.png")
                    }
                },
                 {"Egg_destroy", new Image[]
                    {
                        Engine.LoadImage("assets/items/egg/1.png"),
                        Engine.LoadImage("assets/items/egg/2.png"),
                        Engine.LoadImage("assets/items/egg/3.png"),
                        Engine.LoadImage("assets/items/egg/4.png"),
                        Engine.LoadImage("assets/items/egg/5.png"),
                        Engine.LoadImage("assets/items/egg/6.png"),
                        Engine.LoadImage("assets/items/egg/6.png"),
                        Engine.LoadImage("assets/items/egg/7.png"),
                        Engine.LoadImage("assets/items/egg/8.png"),
                        Engine.LoadImage("assets/items/egg/9.png"),
                        Engine.LoadImage("assets/items/egg/10.png"),
                        Engine.LoadImage("assets/items/egg/11.png"),
                        Engine.LoadImage("assets/items/egg/12.png"),
                    }
                }
            };
            enemiesSprites = new Dictionary<string, Image[]>()
            {
                {"Bear_run_R", new Image[]
                    {
                        Engine.LoadImage("assets/enemies/bear/run_R/1.png"),
                        Engine.LoadImage("assets/enemies/bear/run_R/2.png"),
                        Engine.LoadImage("assets/enemies/bear/run_R/3.png"),
                        Engine.LoadImage("assets/enemies/bear/run_R/4.png")
                    }
                },
                {"Bear_run_L", new Image[]
                    {
                        Engine.LoadImage("assets/enemies/bear/run_L/1.png"),
                        Engine.LoadImage("assets/enemies/bear/run_L/2.png"),
                        Engine.LoadImage("assets/enemies/bear/run_L/3.png"),
                        Engine.LoadImage("assets/enemies/bear/run_L/4.png")
                    }
                },
                {"Bird_fly_R", new Image[]
                    {
                        Engine.LoadImage("assets/enemies/bird/fly_R/1.png"),
                        Engine.LoadImage("assets/enemies/bird/fly_R/2.png"),
                        Engine.LoadImage("assets/enemies/bird/fly_R/3.png"),
                        Engine.LoadImage("assets/enemies/bird/fly_R/4.png")
                    }
                },
                {"Bird_fly_L", new Image[]
                    {
                        Engine.LoadImage("assets/enemies/bird/fly_L/1.png"),
                        Engine.LoadImage("assets/enemies/bird/fly_L/2.png"),
                        Engine.LoadImage("assets/enemies/bird/fly_L/3.png"),
                        Engine.LoadImage("assets/enemies/bird/fly_L/4.png")
                    }
                }
            };
        }
        static void Main(string[] args)
        {
            Engine.Initialize(gc.screenWidth, gc.screenHeight);
            LoadAssets();
            IniciarMenu();
            while (true)
            {
                CheckInputs();
                currentTime = Sdl.SDL_GetTicks() / 1000f;
                
                deltaTime = currentTime - previousTime;
                previousTime = currentTime;
                if (gameState != GameState.Paused)
                {                    
                    Update();
                }
                Render();
            }
        }

        //----------Clases Principales----------//

        class Player
        {
            public int posX;
            public int posY;
            public int Maxhp = 125;
            public int currentHp;
            public int speed = 100;
            public bool isJumping = true;
            public bool isTakingDamage = false;
            public bool isMoving = false;
            public bool isIdle = false;
            public float jumpVelocity = 0;
            public string animation = "idle_R";
            public string spriteDirection = "R";
            public int jumpStrength = 5;
            public int characterWidth = 26;
            public int characterHeight = 32;
            public int dir = 1;
            public int frame = 0;
            public float frameTimer = 0f;
            public Image image;
            public Dictionary<string, float> animationSpeed;
            public Dictionary<string, Image[]> sprites;

            public Player(int startX, int startY)
            {
                posX = startX;
                posY = startY;
                currentHp = Maxhp;
                LoadCharacterAssets();
                image = sprites[animation][0];
            }

            /// <summary>
            /// Carga en un diccionario todas las imagenes correspondientes a las animaciones del jugador
            /// </summary>
            private void LoadCharacterAssets()
            {
                sprites = new Dictionary<string, Image[]>()
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
                    {"hurt", new Image[]
                        {
                            Engine.LoadImage("assets/character/hurt/1.png"),
                            Engine.LoadImage("assets/character/hurt/2.png")
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
                    {"hurt", 0.2f},
                    {"roll_L", 0.04f},
                    {"roll_R", 0.04f}
                };
            }
        }
        class Item
        {
            public int posX;
            public int posY;
            public int value;
            public int width;
            public int height;
            public float speed;
            public int frame = 0;
            public float frameTimer = 0f;
            public float animationSpeed = 0.1f;
            public bool active = true;
            public bool animationLoop = true;            
            public bool isProyectil = false;
            public bool isHostil = false;
            public bool toRemove = false;
            public bool inRemoveQueue = false;
            public Image[] destroySprites;
            public Items id;
            public Image image;
            public Image[] sprites;

            public Item(Items id,int startX, int startY)
            {
                switch (id)
                {
                    case Items.Cherry:
                        value = -15;
                        width = 14;
                        height = 12;
                        isHostil = true;
                        break;
                    case Items.Gem:
                        value = 25;
                        width = 14;
                        height = 12;
                        break;
                    case Items.Egg:
                        value = 15;
                        width = 14;
                        height = 12;
                        isProyectil = true;
                        isHostil = true;
                        destroySprites = itemSprites[$"{id}_destroy"];
                        break;
                }
                this.id = id;
                posX = startX;
                posY = startY;
                sprites = itemSprites[$"{id}"];
                image = sprites[0];
            }
        }
        class Enemy
        {
            public int posX;
            public int posY;
            public int health;
            public int speed;
            public int damage;
            public int width;
            public int height;
            public int frame = 0;
            public int dir = -1;
            public float frameTimer = 0f;
            public float animationSpeed = 0.1f;
            public float attackCD = 1f;
            public float lastTimeAttack = 0f;
            public string animation;
            public string animationSprite;
            public Enemies id;
            public bool active = true;
            public bool toRemove = false;
            public bool inRemoveQueue = false;
            public Image image;
            public Image[] sprites;
            public string spriteDirection = "L";

            public Enemy(Enemies id, int startX, int startY)
            {
                switch (id)
                {
                    case Enemies.Bear:
                        health = 2;
                        speed = 150;
                        damage = 25;
                        width = 40;
                        height = 50;
                        animation = "run";
                        break;
                    case Enemies.Bird:
                        health = 3;
                        speed = 200;
                        damage = 15;
                        width = 38;
                        height = 38;
                        animation = "fly";
                        break;
                }
                posX = startX;
                posY = startY;
                this.id = id;
                sprites = enemiesSprites[$"{id}_{animation}_{spriteDirection}"];
                image = enemiesSprites[$"{id}_{animation}_{spriteDirection}"][0];
            }
        }

        //----------Funciones Principales----------//

        static void CheckInputs()
        {
            if (Engine.KeyPress(Engine.KEY_P) && gameState != GameState.Menu && gameState != GameState.Presentation && (currentTime - gc.lastKeyPressTime > gc.keyCooldown))
            {
                gc.lastKeyPressTime = currentTime;
                gc.menuMessage = "PAUSA";
                gameState = (gameState == GameState.Paused) ? GameState.Playing : GameState.Paused;
            }
            if (Engine.KeyPress(Engine.KEY_RIGHT) || Engine.KeyPress(Engine.KEY_D)) Move(1);
            if (Engine.KeyPress(Engine.KEY_LEFT) || Engine.KeyPress(Engine.KEY_A)) Move(-1);
            if (Engine.KeyPress(Engine.KEY_UP) || Engine.KeyPress(Engine.KEY_W) || Engine.KeyPress(Engine.KEY_ESP)) Jump();
            if (Engine.KeyPress(Engine.KEY_ESC)) Environment.Exit(0);
            if (Engine.KeyPress(Engine.KEY_1) && (currentTime - gc.lastKeyPressTime > gc.keyCooldown))
            {
                gc.lastKeyPressTime = currentTime;
                gc.debugMode = !gc.debugMode;
            }
            if (Engine.KeyPress(Engine.KEY_R) && (currentTime - gc.lastKeyPressTime > gc.keyCooldown))
            {
                gc.lastKeyPressTime = currentTime;
                switch (gameState)
                {
                    case GameState.Menu:
                        gameState = GameState.Presentation;
                        break;
                    case GameState.Victory:
                    case GameState.GameOver:
                        ResetGame();
                        break;
                    default:
                        break;
                }
                
            }
            if (gameState == GameState.Playing && !Engine.KeyPress(Engine.KEY_RIGHT) && !Engine.KeyPress(Engine.KEY_D) && !Engine.KeyPress(Engine.KEY_LEFT) && !Engine.KeyPress(Engine.KEY_A)) player.isMoving = false;
        }
        static void Render()
        {
            Engine.Clear();
            switch (gameState)
            {
                case GameState.Playing:
                    DrawBackground();
                    Engine.Draw(player.image, player.posX, player.posY);
                    DrawUI();
                    DrawItems();
                    DrawEnemies();
                    break;

                case GameState.Paused:
                    DrawBackground();
                    Engine.Draw(player.image, player.posX, player.posY);
                    DrawUI();
                    DrawItems();
                    DrawEnemies();
                    DrawPause();
                    break;

                case GameState.Menu:
                    DrawMenu();
                    break;

                case GameState.GameOver:
                    DrawGameOver();
                    break;
                case GameState.Victory:
                    DrawGameVictory();
                    break;
                case GameState.Presentation:
                    DrawPresentacion();
                    break;
                default:
                    break;
            }
                Engine.Show();
        }
        static void Update()
        {
            switch (gameState)
            {
                case GameState.Playing:
                    UpdatePlayerSprite();
                    UpdateEnemySprite();
                    UpdateItemSprites();
                    UpdateItemPosition();
                    UpdateEnemyPosition();
                    UpdateGravity();
                    GameManager();
                    gc.timer = (int)(DateTime.Now - gc.startTime).TotalSeconds;
                    if (!player.isMoving && !player.isJumping && !player.isTakingDamage)
                    {
                        player.isIdle = true;
                        SetAnimation($"idle_{player.spriteDirection}");
                    }
                    else
                    {
                        player.isIdle = false;
                    }
                    if (player.isTakingDamage && currentTime - gc.lastTakeDamageTime <= gc.takeDamageCooldown)
                    {
                        SetAnimation($"hurt");
                    }
                    else if (currentTime - gc.lastTakeDamageTime >= gc.takeDamageCooldown)
                    {
                        player.isTakingDamage = false;
                    }                    
                    break;
                case GameState.Presentation:
                    AnimarPresentacion();
                    break;
                default:
                    break;
            }
        }

        //----------Seccion de funciones de INPUTS----------//

        /// <summary>
        /// Se encarga de mover al Personaje hacia la izquierda o derecha
        /// </summary>
        /// <param name="direction">define la direccion del movimiento, siendo -1 a la izquierda y 1 a la derecha</param>
        static void Move(int direction)
        {
            if (gameState == GameState.Playing)
            {
                player.isMoving = true;
                player.dir = direction;
                if (player.posX >= gc.screenMoveLimit && direction < 0 || player.posX <= gc.screenWidth - gc.screenMoveLimit && direction > 0)
                {
                    player.posX += (int)(player.speed * direction * deltaTime);
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
        /// <summary>
        /// Chequea si el jugador esta saltando, y si no lo esta inicia el salto
        /// </summary>
        static void Jump()
        {
            if (gameState == GameState.Playing)
            {
                if (!player.isJumping)
                {
                    player.isJumping = true;
                    player.jumpVelocity = -player.jumpStrength;
                    SetAnimation($"jump_{player.spriteDirection}");
                }
            }
        }

        //----------Seccion de funciones del RENDER----------//

        /// <summary>
        /// Dibuja la interfaz y datos de debug
        /// </summary>
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
            Engine.DrawText($"Tiempo: {gc.timer}", 420, 15, 232, 120, 55, font[3]);
            Engine.DrawText($"x {gc.lifes}", 15, 85, 232, 120, 55, font[4]);
            if (gc.debugMode)
            {
                Engine.DrawText($"Jumping: {player.isJumping}", 25, 150, 232, 120, 55, font[3]);
                Engine.DrawText($"Moving: {player.isMoving}", 25, 170, 232, 120, 55, font[3]);
                Engine.DrawText($"Idle: {player.isIdle}", 25, 190, 232, 120, 55, font[3]);
                Engine.DrawText($"Damage: {player.isTakingDamage}", 25, 210, 232, 120, 55, font[3]);
                Engine.DrawText($"Velocity-Y: {player.jumpVelocity}", 25, 230, 232, 120, 55, font[3]);
                Engine.DrawText($"Deltatime: {deltaTime}", 25, 250, 232, 120, 55, font[3]);
                Engine.DrawText($"CurrentTime: {currentTime}", 25, 270, 232, 120, 55, font[3]);
                Engine.DrawText($"PreviousTime: {previousTime}", 25, 290, 232, 120, 55, font[3]);
                Engine.DrawText($"LastDamageTime: {gc.lastTakeDamageTime}", 25, 310, 232, 120, 55, font[3]);
                Engine.DrawText($"TakeDamage CD: {gc.takeDamageCooldown}", 25, 330, 232, 120, 55, font[3]);
                Engine.DrawText($"Game State: {gameState}", 25, 350, 232, 120, 55, font[3]);
                Engine.DrawText($"Enemies count: {enemies.Count}", 25, 370, 232, 120, 55, font[3]);
                Engine.DrawText($"Items count: {items.Count}", 25, 390, 232, 120, 55, font[3]);

            }
        }
        /// <summary>
        /// Dibuja el fondo parallax
        /// </summary>
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
        /// <summary>
        /// Dibuja todos los items activos
        /// </summary>
        static void DrawItems()
        {
            foreach (var item in items)
            {
                if (item.active)
                {
                    Engine.Draw(item.image, item.posX, item.posY);
                }
            }
        }
        /// <summary>
        /// Dibuja todos los enemigos activos
        /// </summary>
        static void DrawEnemies()
        {
            foreach (var enemy in enemies)
            {
                if (enemy.active)
                {
                    Engine.Draw(enemy.image, enemy.posX, enemy.posY);
                }
            }
        }
        /// <summary>
        /// Dibuja el fondo y los textos de la presentacion
        /// </summary>
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
        /// <summary>
        /// Dibuja la pantalla de derrota
        /// </summary>
        static void DrawGameOver()
        {
            Engine.Draw(fondo[11], 0, 0);
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 189, 268, 232, 120, 55, font[0]);
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 185, 270, 243, 198, 35, font[0]);
            Engine.DrawText($"{gc.subMessage}", gc.screenWidth / 2 - 210, 720, 255, 255, 255, font[3]);
            Engine.DrawText($"tu puntuacion fue: {gc.score}", gc.screenWidth / 2 - 110, 380, 255, 255, 255, font[3]);
            Engine.DrawText($"duraste {gc.timer} segundos", gc.screenWidth / 2 - 110, 410, 255, 255, 255, font[3]);
        }
        /// <summary>
        /// Dibuja la pantalla de victoria
        /// </summary>
        static void DrawGameVictory()
        {
            Engine.Draw(fondo[10], 0, 0);
            Engine.Draw(ui[0],10,10);
            Engine.DrawText($"{gc.menuMessage}", 159, 64, 232, 120, 55, font[0]);
            Engine.DrawText($"{gc.menuMessage}", 155, 67, 243, 198, 35, font[0]);
            Engine.DrawText($"{gc.subMessage}", gc.screenWidth / 2 - 210, 720, 0, 0, 0, font[3]);
            Engine.DrawText($"tu puntuacion fue: {gc.score}", 685, 30, 243, 198, 35, font[3]);
            Engine.DrawText($"tu tiempo fue: {gc.timer}", 685, 60, 243, 198, 35, font[3]);
        }
        /// <summary>
        /// Dibuja la pantalla de Menu
        /// </summary>
        static void DrawMenu()
        {
            Engine.Draw(fondo[0], 0, posYFondo[0]);
            Engine.Draw(ui[0], gc.screenWidth / 2 - 320, posYFondo[0] + 195);
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 239, posYFondo[0] + 248, 232, 120, 55, font[0]);
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 235, posYFondo[0] + 250, 243, 198, 35, font[0]);
            Engine.DrawText($"{gc.subMessage}", gc.screenWidth / 2 - 170, posYFondo[0] + 450, 243, 198, 35, font[3]);
        }
        /// <summary>
        /// Dibuja la interfaz de pausa
        /// </summary>
        static void DrawPause()
        {
            Engine.Draw(ui[3], gc.screenWidth / 2 - 207, gc.screenHeight / 2 - 180); // placeholder
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 120, gc.screenHeight / 2 - 150, 232, 120, 55, font[0]);
            Engine.DrawText($"{gc.menuMessage}", gc.screenWidth / 2 - 116, gc.screenHeight / 2 - 148, 243, 198, 35, font[0]);
        }

        //----------Seccion de funciones del UPDATE----------//

        /// <summary>
        /// Aplica la gravedad al jugador
        /// </summary>
        static void UpdateGravity()
        {
            player.jumpVelocity += gc.gravity * deltaTime;
            player.posY += (int)(player.jumpVelocity * deltaTime * 100);
            if (player.jumpVelocity > 0 && player.isJumping)
                {
                    SetAnimation($"roll_{player.spriteDirection}");
                }
            CheckGroundCollision();
        }        
        /// <summary>
        /// Se encarga de recorrer el array de imagenes de las animaciones del jugador y setear el sprite correspondiente
        /// </summary>
        static void UpdatePlayerSprite()
        {
            player.frameTimer += deltaTime;
            player.spriteDirection = (player.dir > 0) ? player.spriteDirection = "R" : player.spriteDirection = "L";
            if (player.frameTimer >= player.animationSpeed[player.animation])
            {
                player.frame++;
                if (player.frame >= player.sprites[player.animation].Length)
                {
                    player.frame = 0;
                }
                player.frameTimer = 0f;
                if (player.frame < player.sprites[player.animation].Length)
                {
                    player.image = player.sprites[player.animation][player.frame];
                }
            }
        }
        /// <summary>
        /// Recorre el array de enemigos y actualiza sus sprites 
        /// </summary>
        static void UpdateEnemySprite()
        {
            foreach (var enemy in enemies)
            {
                if (enemy.active)
                {
                    enemy.frameTimer += deltaTime;
                    enemy.spriteDirection = (enemy.dir > 0) ? enemy.spriteDirection = "R" : enemy.spriteDirection = "L";
                    if (enemy.animationSprite != $"{enemy.animation}_{enemy.spriteDirection}")
                    {
                        enemy.animationSprite = ($"{enemy.animation}_{enemy.spriteDirection}");
                        enemy.sprites = enemiesSprites[$"{enemy.id}_{enemy.animation}_{enemy.spriteDirection}"];
                    }
                    if (enemy.frameTimer >= enemy.animationSpeed)
                    {
                        enemy.frame++;
                        if (enemy.frame >= enemy.sprites.Length)
                        {
                            enemy.frame = 0;
                        }
                        enemy.frameTimer = 0f;
                        if (enemy.frame < enemy.sprites.Length)
                        {
                            enemy.image = enemy.sprites[enemy.frame];
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Recorre el array de items y actualiza sus sprites
        /// </summary>
        static void UpdateItemSprites()
        {
            foreach (var item in items)
            {
                item.frameTimer += deltaTime;
                if (item.frameTimer >= item.animationSpeed)
                {
                    item.frame++;
                    if (item.frame >= item.sprites.Length)
                    {
                        if (item.animationLoop)
                        {
                            item.frame = 0;
                        }
                        else
                        {
                            item.frame = item.sprites.Length;
                            item.toRemove = true;
                        }
                    }
                    item.frameTimer = 0f;
                    if (item.frame < item.sprites.Length)
                    {
                        item.image = item.sprites[item.frame];
                    }
                }
            }
        }
        /// <summary>
        /// Se encarga de actualizar la posicion de los Items
        /// </summary>
        static void UpdateItemPosition()
        {
            ItemGarbageCollector();
            foreach (var item in items)
            {
                if (item.active)
                {
                    // Items comunes
                    if (player.posX <= gc.screenMoveLimit || player.posX >= gc.screenWidth - gc.screenMoveLimit)
                    {
                        if ((!player.isIdle && !player.isJumping && !player.isTakingDamage) || player.isMoving)
                        {
                            item.posX += (int)(player.speed * player.dir * -1 * deltaTime);
                        }
                    }
                    // Items tipo Proyectil
                    if (item.isProyectil)
                    {
                        if (item.posY <= gc.groundLevel - 36)
                        {
                            item.speed += gc.gravity * deltaTime;
                            item.posY += (int)(item.speed * deltaTime * 90);
                        }
                        else
                        {
                            item.sprites = item.destroySprites;
                            item.animationLoop = false;
                        }
                    }
                    // Item Collision
                    if (CheckCollision(player, item))
                    {
                        item.active = false;
                        if (!item.isHostil)
                        {
                            gc.score += item.value;
                            CheckVictory();
                        }
                        else
                        {
                            ModifyPlayerHP(item.value);
                        }
                    }
                    // Remover items que se van de pantalla
                    if (item.posX <= -gc.screenWidth)
                    {
                        item.toRemove = true;
                        item.active = false;
                    }
                }
            }
            
        }
        /// <summary>
        /// Se encarga de actualizar la posicion de los Enemigos
        /// </summary>
        static void UpdateEnemyPosition()
        {
            EnemyGarbageCollector();
            foreach (var enemy in enemies)
            {
                if (enemy.active)
                {
                    if (enemy.posX <= 0 || enemy.posX >= gc.screenWidth - enemy.width)
                    {
                        int inicialDir = enemy.dir;
                        if (enemy.posX <= 0)
                        {
                            if (enemy.dir < 0)
                            {
                                enemy.dir *= -1;
                            }
                        }
                        else
                        {
                            if (enemy.dir > 0)
                            {
                                enemy.dir *= -1;
                            }
                        }
                        if (enemy.health > 0 && enemy.dir != inicialDir)
                        {
                            enemy.health--;
                        }
                        else if (enemy.dir != inicialDir)
                        {
                            enemy.active = false;
                        }
                    }
                    if (player.posX <= gc.screenMoveLimit || player.posX >= gc.screenWidth - gc.screenMoveLimit)
                    {
                        if ((!player.isIdle && !player.isJumping && !player.isTakingDamage) || player.isMoving)
                        {
                            enemy.posX += (int)(player.speed * player.dir * -1 * deltaTime);
                        }
                    }
                    if (CheckCollision(player, enemy))
                    {
                        ModifyPlayerHP(enemy.damage);
                    }
                    CheckEnemyAttack(enemy);
                    enemy.posX += (int)(enemy.speed * enemy.dir * deltaTime);
                }
            }
        }

        //----------Seccion de funciones del GAMEPLAY----------//

        /// <summary>
        /// Se encarga de crear los items y enemigos del juego
        /// </summary>
        static void GameManager()
        {
            if (currentTime - gc.lastSpawnItemTime > gc.spawnItemCooldown && player.posX >= gc.screenWidth - gc.screenMoveLimit && !player.isIdle)
            {
                gc.lastSpawnItemTime = currentTime;
                Items randomItem = GetRandomItem();
                SpawnPattern pattern = GetRandomItemPattern();
                SpawnItems(randomItem, pattern);
            }
            if (currentTime - gc.lastSpawnEnemyTime > gc.spawnEnemyCooldown)
            {
                gc.lastSpawnEnemyTime = currentTime;
                Enemies randomEnemy = GetRandomEnemy();
                SpawnEnemy(randomEnemy);
            }
        }
        /// <summary>
        /// Devuelve aleatoriamente un Item
        /// </summary>
        /// <returns>Item</returns>
        static Items GetRandomItem()
        {
            int chance = random.Next(1, 101);
            Items item = (chance <= 5) ? Items.Cherry : Items.Gem;
            return item;
        }
        /// <summary>
        /// Devuelve aleatoriamente un patron de Items
        /// </summary>
        /// <returns>patron de items</returns>
        static SpawnPattern GetRandomItemPattern()
        {
            int chance = random.Next(1, 501);
            if (chance <= 25) return SpawnPattern.square;
            else if (chance <= 100) return SpawnPattern.row;
            else if (chance <= 250) return SpawnPattern.line;
            else if (chance <= 400) return SpawnPattern.doble;
            else return SpawnPattern.single;
        }
        /// <summary>
        /// Devuelve un enemigo aleatorio
        /// </summary>
        /// <returns></returns>
        static Enemies GetRandomEnemy()
        {
            int chance = random.Next(1, 101);
            Enemies enemy = (chance <= 30) ? Enemies.Bear : Enemies.Bird;
            return enemy;
        }
        /// <summary>
        /// Crea items segun el id y patron
        /// </summary>
        /// <param name="id">id del item</param>
        /// <param name="pattern">patron de items</param>
        static void SpawnItems(Items id, SpawnPattern pattern )
        {
            var posX = gc.screenWidth;
            var posY = gc.groundLevel - 25 * random.Next(2, 4);
            var cant = random.Next(1, 4);
            switch(pattern)
            {
                case SpawnPattern.single:
                    items.Add(new Item(id, posX, posY));
                    break;
                case SpawnPattern.doble:
                    items.Add(new Item(id, posX, posY));
                    items.Add(new Item(id, posX + 50, posY));
                    break;
                case SpawnPattern.row:
                    for (int i = 0;i< cant;i++)
                    {
                        items.Add(new Item(id, posX + i * 50, posY));
                    }                    
                    break;
                case SpawnPattern.line:
                    for (int i = 0; i < cant; i++)
                    {
                        items.Add(new Item(id, posX, posY - i * 25));
                    }
                    break;
                case SpawnPattern.square:
                    for (int i = 0; i < cant; i++)
                    {
                        for (int j = 0; j < cant; j++)
                        {
                            items.Add(new Item(id, posX + i * 50, posY - j * 25));
                        }                           
                    }
                    break;
            }       
        }
        /// <summary>
        /// Crea un enemigo segun el id
        /// </summary>
        /// <param name="id">Enemigo</param>
        static void SpawnEnemy(Enemies id)
        {
            var posX = gc.screenWidth;
            var posY = gc.groundLevel - 25 * random.Next(2, 4);
            switch (id)
            {
                case Enemies.Bear:
                    posX = gc.screenWidth + 50;
                    posY = gc.groundLevel - 65;
                    enemies.Add(new Enemy(id, posX, posY));
                    break;
                case Enemies.Bird:
                    posX = gc.screenWidth + 50;
                    posY = 250 + 50 * random.Next(-1, 5);
                    enemies.Add(new Enemy(id, posX, posY));
                    break;
            }
        }

        //----------Seccion de funciones Auxiliares----------//

        /// <summary>
        /// Se encarga de mover en el eje X mis imagenes de fondo para generar el efecto Parallax (cada par de imagenes se mueve a una velocidad distinta)
        /// </summary>
        static void MoveParallax()
        {
            posXParallax["1A"] -= (int)(player.speed / 4 * player.dir * deltaTime);
            posXParallax["1B"] -= (int)(player.speed / 4 * player.dir * deltaTime);
            posXParallax["2A"] -= (int)(player.speed / 2 * player.dir * deltaTime);
            posXParallax["2B"] -= (int)(player.speed / 2 * player.dir * deltaTime);
            posXParallax["3A"] -= (int)(player.speed * player.dir * deltaTime);
            posXParallax["3B"] -= (int)(player.speed * player.dir * deltaTime);
            CheckParallax();
        }
        /// <summary>
        /// Chequea cuando mis imagenes del Parallax llegan al final de la pantalla y las reubica
        /// </summary>
        static void CheckParallax()
        {
            //Parallax que consiste de 3 pares de imagenes
            for (int i = 1; i < posXParallax.Count / 2 + 1; i++)
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
                if (posXParallax[$"{i}B"] <= -gc.screenWidth)
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
        /// <summary>
        /// Chequea si colisiona el jugador con un item
        /// </summary>
        /// <param name="a">jugador</param>
        /// <param name="b">item</param>
        /// <returns>Verdadero si hay collision</returns>
        static bool CheckCollision(Player a, Item b)
        {
            return a.posX < b.posX + b.width &&
                   a.posX + a.characterWidth > b.posX &&
                   a.posY < b.posY + b.height &&
                   a.posY + a.characterHeight > b.posY;
        }
        /// <summary>
        /// Chequea si colisiona el jugador con un enemigo
        /// </summary>
        /// <param name="a">jugador</param>
        /// <param name="b">enemigo</param>
        /// <returns>Verdadero si hay collision</returns>
        static bool CheckCollision(Player a, Enemy b)
        {
            return a.posX < b.posX + b.width &&
                   a.posX + a.characterWidth > b.posX &&
                   a.posY < b.posY + b.height &&
                   a.posY + a.characterHeight > b.posY;
        }
        /// <summary>
        /// Chequea si el jugador esta en el suelo
        /// </summary>
        static void CheckGroundCollision()
        {
            if (player.posY >= gc.groundLevel - player.characterHeight)
            {
                player.posY = gc.groundLevel - player.characterHeight;
                player.isJumping = false;
                player.jumpVelocity = 0;
            }
        }
        /// <summary>
        /// Chequea si la barra de vida del jugador llega a 0, en tal caso resta 1 a las vidas, en caso de no quedar mas vidas disponibles, dispara el fin del juego
        /// </summary>
        static void CheckDefeat()
        {
            if (gc.lifes <= 0)
            {
                gc.menuMessage = "Game Over";
                gc.subMessage = "Presiona 'R' para volver al menu";
                gameState = GameState.GameOver;
            }
            else
            {
                gc.lifes--;
                ResetPlayer();
            }           
        }
        /// <summary>
        /// Chequea si se llego al puntaje de victoria
        /// </summary>
        static void CheckVictory()
        {
            if (gc.score >= gc.scoreToWin)
            {
                gc.menuMessage = "Ganaste!";
                gc.subMessage = "Presiona 'R' para volver al menu";
                gameState = GameState.Victory;
            }
        }
        /// <summary>
        /// Resetea las variables del juego como puntaje y vidas 
        /// </summary>
        static void CheckEnemyAttack(Enemy e)
        {
            switch (e.id)
            {
                case Enemies.Bird:
                    if (player.posX < e.posX + e.width && player.posX + player.characterWidth > e.posX && (currentTime - e.lastTimeAttack > e.attackCD))
                    {
                        e.lastTimeAttack = currentTime;
                        items.Add(new Item(Items.Egg, e.posX, e.posY));
                    }
                        break;
                default:
                    break;
            }            
        }
        /// <summary>
        /// Se encarga de remover de la lista los items inactivos
        /// </summary>
        static void ItemGarbageCollector()
        {
            List<Item> itemsToRemove = new List<Item>();
            foreach (var item in items)
            {
                if ((!item.active || item.toRemove) && !item.inRemoveQueue)
                {
                    item.inRemoveQueue = true;
                    itemsToRemove.Add(item);
                }
            }
            foreach (var item in itemsToRemove)
            {
                RemoveItem(item);
            }
        }
        /// <summary>
        /// Se encarga de remover de la lista los enemigos inactivos
        /// </summary>
        static void EnemyGarbageCollector()
        {
            List<Enemy> enemyToRemove = new List<Enemy>();
            foreach (var enemy in enemies)
            {
                if ((!enemy.active || enemy.toRemove) && !enemy.inRemoveQueue)
                {
                    enemy.inRemoveQueue = true;
                    enemyToRemove.Add(enemy);
                }
            }
            foreach (var enemy in enemyToRemove)
            {
                RemoveEnemy(enemy);
            }
        }
        /// <summary>
        /// Remueve un item de la lista
        /// </summary>
        /// <param name="i">indice del item a remover</param>
        static void RemoveItem(Item i)
        {
            items.Remove(i);
        }
        /// <summary>
        /// Remueve un enemigo de la lista
        /// </summary>
        /// <param name="i">indice del item a remover</param>
        static void RemoveEnemy(Enemy e)
        {
            enemies.Remove(e);
        }
        /// <summary>
        /// Reinicia el puntaje,vidas,timer,enemigos,items etc y vuelve al menu
        /// </summary>
        static void ResetGame()
        {
            gameState = GameState.Menu;
            gc.menuMessage = "Foxy Runner";
            gc.subMessage = "Presiona 'R' para comenzar";
            gc.score = 0;
            gc.lifes = 2;
            posYFondo = new int[3]
            {
                0,gc.screenHeight,gc.screenHeight * 2
            };
            items.Clear();
            enemies.Clear();
            sonidos[1].Stop();
            sonidos[0].PlayLooping();
        }
        /// <summary>
        /// Se encarga de Dañar o Curar al jugador
        /// </summary>
        /// <param name="value">si este valor es positivo, dañara al personaje, si es negativo lo curara</param>
        static void ModifyPlayerHP(int value)
        {
            if (value < 0)
            {
                if (player.currentHp - value < player.Maxhp)
                {
                    player.currentHp -= value;
                }
                else player.currentHp = player.Maxhp;
                return;
            }
            if (currentTime - gc.lastTakeDamageTime >= gc.takeDamageCooldown && !player.isTakingDamage)
            {
                player.isTakingDamage = true;
                player.currentHp -= value;
                gc.lastTakeDamageTime = currentTime;
            }
            if (player.currentHp <= 0)
            {
                CheckDefeat();
            }
        }
        /// <summary>
        /// Resetea la vida y posicion del jugador
        /// </summary>
        static void ResetPlayer()
        {
            player.currentHp = player.Maxhp;
            player.posY = -player.characterHeight;
            player.isJumping = true;
            player.posX = gc.screenWidth / 2 - player.characterWidth / 2;
        }
        /// <summary>
        /// Asigna la animacion y la velocidad de actualizacion de la misma al jugador
        /// </summary>
        /// <param name="anim">texto correspondiente a la animacion (debe coincidir con las animaciones definidas en el diccionario de animaciones del jugador) </param>
        static void SetAnimation(string anim)
        {
            if (gameState == GameState.Playing)
            {
                if (player.animation != anim)
                {
                    player.frameTimer = player.animationSpeed[anim];
                    player.animation = anim;
                }
            }
        }
        /// <summary>
        /// Se encarga de mover las imagenes de la intro
        /// </summary>
        static void AnimarPresentacion()
        {
            
            gc.subMessage = "Buena Suerte!";
            for (int i = 0; i < posYFondo.Length; i++)
            {
                posYFondo[i] -= 7;
            }
            if (gameState == GameState.Presentation && posYFondo[2] <= 0)
            {
                IniciarJuego();
            }
        }
        /// <summary>
        /// Muestra el menu de inicio del juego
        /// </summary>
        static void IniciarMenu()
        {
            gameState = GameState.Menu;
            if (gc.soundEnabled)
            {
                sonidos[0].PlayLooping();
            }
        }
        /// <summary>
        /// Inicial el juego y crea al jugador
        /// </summary>
        static void IniciarJuego()
        {
            posYFondo[2] = 0;
            sonidos[0].Stop();
            gc.startTime = DateTime.Now;
            gameState = GameState.Playing;
            player = new Player(gc.screenWidth / 2 - 16, 0);
            if (gc.soundEnabled)
            {
                sonidos[1].PlayLooping();
            }
        }
    }
}
