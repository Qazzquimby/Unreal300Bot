using Deltin.CustomGameAutomation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unreal300Bot {
    class GameLoop {
        static CustomGame cg = Program.cg;
        static int server_time = 0;

        public void Start() {
            Setup();
            Loop();
        }

        static void Setup() {
            //Put personal server code here.
            cg.GameSettings.SetGameName("Unreal 300%, 1 year+ balancing");
            cg.GameSettings.SetTeamName(PlayerTeam.Blue, @"\ Blue Team");
            cg.GameSettings.SetTeamName(PlayerTeam.Red, "* Red Team");

            if(cg.GetGameState() == GameState.InLobby) {
                cg.StartGame();
            }

            cg.Chat.SwapChannel(Channel.Match);
            cg.Chat.Chat(">>Beep boop. Starting up.");
        }

        static void Loop() {
            System.Diagnostics.Debug.WriteLine("Starting loop");
            while(true) {
                foreach(int delay in Program.phase.LoopFuncs.Keys) {
                    if(server_time % delay == 0) {
                        try {
                            foreach(Action func in Program.phase.LoopFuncs[delay]) {
                                try {
                                    func();
                                }
                                catch(KeyNotFoundException) {}
                            }
                        } catch(KeyNotFoundException) { }
                        
                    }
                }

                server_time += 1;
                Program.time_at_current_map += 1;
                System.Threading.Thread.Sleep(500);
            }
        }


    }



    class MapChooser {
        public static Map current = Map.C_Lijiang;
        public static int mode_i = 2;
        static CustomGame cg;
        static Random rnd = new Random();
        static List<Map> recent_maps = new List<Map>();

        static Map[] AE_maps = {
            Map.AE_Eichenwalde,
            Map.AE_Hollywood,
            Map.AE_KingsRow,
            Map.AE_Numbani
        };

        static Map[] A_maps = {
            Map.A_Hanamura,
            Map.A_HorizonLunarColony,
            Map.A_TempleOfAnubis,
            Map.A_VolskayaIndustries
        };

        static Map[] C_maps = {
            Map.C_Ilios,
            Map.C_Lijiang,
            Map.C_Nepal,
            Map.C_Oasis
        };

        static Map[] TDM_maps = {
            Map.TDM_Antarctica,
            Map.TDM_BlackForest,
            Map.TDM_Castillo,
            Map.TDM_ChateauGuillard,
            //Map.TDM_Dorado,
            //Map.TDM_Eichenwalde,
            //Map.TDM_Hanamura,
            //Map.TDM_Hollywood,
            //Map.TDM_HorizonLunarColony,
            //Map.TDM_Ilios_Lighthouse,
            //Map.TDM_Ilios_Ruins,
            //Map.TDM_Ilios_Well,
            //Map.TDM_KingsRow,
            //Map.TDM_Lijiang_ControlCenter,
            //Map.TDM_Lijiang_Garden,
            //Map.TDM_Lijiang_NightMarket,
            Map.TDM_Necropolis,
            //Map.TDM_Nepal_Sanctum,
            //Map.TDM_Nepal_Shrine,
            //Map.TDM_Nepal_Village,
            //Map.TDM_Oasis_CityCenter,
            //Map.TDM_Oasis_Gardens,
            //Map.TDM_Oasis_University,
            Map.TDM_Petra,
            //Map.TDM_TempleOfAnubis,
            //Map.TDM_VolskayaIndustries
        };

        static Map[] E_maps = {
            Map.E_Dorado,
            Map.E_Junkertown,
            Map.E_Route66,
            Map.E_Gibraltar
        };


        static Map[][] modes = { AE_maps, A_maps, C_maps, TDM_maps, E_maps };

        public MapChooser(CustomGame cg_input) {
            cg = cg_input;
            cg.ModesEnabled = new ModesEnabled();
            cg.ModesEnabled.Assault = true;
            cg.ModesEnabled.AssaultEscort = true;
            cg.ModesEnabled.Control = true;
            cg.ModesEnabled.Escort = true;
            cg.ModesEnabled.TeamDeathmatch = true;

            cg.CurrentOverwatchEvent = cg.GetCurrentOverwatchEvent();
        }

        public void SetRandomMap() {
            System.Diagnostics.Debug.WriteLine("Setting map");
            Map map = GetRandomMap();
            foreach(Map recent_map in recent_maps) {
                System.Diagnostics.Debug.WriteLine(String.Format("Recent maps contains {0}.", recent_map.MapName));
            }
            
            while(recent_maps.Contains(map)) {
                System.Diagnostics.Debug.WriteLine(String.Format("Recently went to {0}. Rerolling.", CustomGame.CG_Maps.MapNameFromID(map)));
                map = GetRandomMap();
            }

            System.Diagnostics.Debug.WriteLine(String.Format("New Map is {0}.", CustomGame.CG_Maps.MapNameFromID(map)));

            SetMap(map);
        }

        static Map GetRandomMap() {
            Map[] mode = RandomMode();

            Map map;
            map = mode[rnd.Next(mode.Length)];
            return map;
        }

        static Map[] RandomMode() {
            int index;
            index = rnd.Next(modes.Length);
            if(Program.num_bots >= 4) {
                while(index == 3) {
                    index = rnd.Next(modes.Length);
                }
            }
            Map[] mode = modes[index];
            mode_i = index;
            return mode;
        }

        public void SetMap(Map map) {
            //Map[] maps_to_set = { map };
            //Map[] maps_to_unset = { current };
            //cg.Maps.ToggleMap(ToggleAction.EnableAll, map);
            cg.Maps.ToggleMap(ToggleAction.DisableAll, map);
            if(recent_maps.Count >= 5) {
                recent_maps.RemoveAt(0);
            }
            recent_maps.Add(map);
            current = map;
        }

    }



    class GameLogger {
        public DateTime start_time;
        public DateTime end_time;
        public int num_ticks = 0;

        public int joins;
        public int leaves;

        public Map map;
        public int mode_i;

        public Dictionary<Team, Dictionary<Hero, int>> hero_play_time;
        public Dictionary<Team, int> player_count;
        public PlayerTeam winning_team;

        public GameLogger() {
            start_time = DateTime.Now;

            num_ticks = 0;

            joins = 0;
            leaves = 0;

            map = MapChooser.current;
            mode_i = MapChooser.mode_i;
            
            hero_play_time = new Dictionary<Team, Dictionary<Hero, int>>()
            {
                {
                    Team.Blue, new Dictionary<Hero, int>()
                    {
                        { Hero.Doomfist, 0 },
                        { Hero.Genji, 0 },
                        { Hero.McCree, 0 },
                        { Hero.Pharah, 0 },
                        { Hero.Reaper, 0 },
                        { Hero.Soldier76, 0 },
                        { Hero.Sombra, 0 },
                        { Hero.Tracer, 0 },
                        { Hero.Bastion, 0 },
                        { Hero.Hanzo, 0 },
                        { Hero.Junkrat, 0 },
                        { Hero.Mei, 0 },
                        { Hero.Torbjorn, 0 },
                        { Hero.Widowmaker, 0 },
                        { Hero.DVA, 0 },
                        { Hero.Orisa, 0 },
                        { Hero.Reinhardt, 0 },
                        { Hero.Roadhog, 0 },
                        { Hero.Winston, 0 },
                        { Hero.Zarya, 0 },
                        { Hero.Ana, 0 },
                        { Hero.Brigitte, 0 },
                        { Hero.Lucio, 0 },
                        { Hero.Mercy, 0 },
                        { Hero.Moira, 0 },
                        { Hero.Symmetra, 0 },
                        { Hero.Zenyatta, 0 }
                    }
                },
                {
                    Team.Red, new Dictionary<Hero, int>()
                    {
                        { Hero.Doomfist, 0 },
                        { Hero.Genji, 0 },
                        { Hero.McCree, 0 },
                        { Hero.Pharah, 0 },
                        { Hero.Reaper, 0 },
                        { Hero.Soldier76, 0 },
                        { Hero.Sombra, 0 },
                        { Hero.Tracer, 0 },
                        { Hero.Bastion, 0 },
                        { Hero.Hanzo, 0 },
                        { Hero.Junkrat, 0 },
                        { Hero.Mei, 0 },
                        { Hero.Torbjorn, 0 },
                        { Hero.Widowmaker, 0 },
                        { Hero.DVA, 0 },
                        { Hero.Orisa, 0 },
                        { Hero.Reinhardt, 0 },
                        { Hero.Roadhog, 0 },
                        { Hero.Winston, 0 },
                        { Hero.Zarya, 0 },
                        { Hero.Ana, 0 },
                        { Hero.Brigitte, 0 },
                        { Hero.Lucio, 0 },
                        { Hero.Mercy, 0 },
                        { Hero.Moira, 0 },
                        { Hero.Symmetra, 0 },
                        { Hero.Zenyatta, 0 }
                    }
                }
            };
            player_count = new Dictionary<Team, int>() {
                {Team.Blue, 0 },
                {Team.Red, 0 }
            };
        }
    }


    abstract class Phase {
        virtual public Dictionary<int, List<Action>> LoopFuncs {
            get;
            set;
        }

        virtual public void Enter() {
        }
        virtual public void Exit() {
        }
    }


    class First30SecondsPhase : Phase {
        static int timer = 0;

        public override Dictionary<int, List<Action>> LoopFuncs {
            get; set;
        } = new Dictionary<int, List<Action>>(){
            { 1, new List<Action>()
                {
                    IncrementTimer
                }
            },
            {
                5, new List<Action>() {
                    ScrambleIfImbalance
                }
           },
        };

        private static void IncrementTimer() {
            timer++;
            if(timer >= 30) {
                Program.EnterPhase(typeof(GamePhase));
            }
        }

        private static void ScrambleIfImbalance() {
            int blue_team_size_advantage = Program.GetBlueTeamSizeAdvantage();
            if(Math.Abs(blue_team_size_advantage) >= 2) {
                System.Diagnostics.Debug.WriteLine("Evening teams");
                Program.SwapToBalance();
            }
        }

        public override void Enter() {
            System.Diagnostics.Debug.WriteLine("First30Seconds phase");
            timer = 0;
            Program.EndGameLog();
            Program.RemoveBots();
            Program.ScrambleTeams();
        }
        public override void Exit() {
            Program.BeginGameLog();
        }

    }

    class GamePhase : Phase {
        public override Dictionary<int, List<Action>> LoopFuncs {get; set;} = new Dictionary<int, List<Action>>(){
            { 30, new List<Action>()
                {
                    Program.print_running_trace,
                    Program.HandleBots
                }
            },
            { 15, new List<Action>()
                {
                    Program.HandleAutoBalance,
                    Program.OnePlayerDM,
                    Program.UpdateGameLog
                }
            },
            { 1, new List<Action>()
                {
                    HandleGameOver,
                    HandleMissedGameOver,
                    Program.prevent_map_timeout,
                    Program.PerformAutoBalance
                }
            }

        };

        static void HandleGameOver() {
            if(Program.game_ended) {
                Program.EnterPhase(typeof(RunningGameEndingPhase));
            }
        }

        static void HandleMissedGameOver() {
            if(Program.cg.GetGameState() == GameState.Ending_Commend) {
                Program.GameOver();
                Program.NextMap();
                
            }
        }

        public override void Enter() {
            System.Diagnostics.Debug.WriteLine("game phase");
            Program.time_at_current_map = 60;
            Program.cg.Chat.JoinChannel(Channel.Match);
        }
        public override void Exit() {
            System.Diagnostics.Debug.WriteLine("leaving game phase");
        }

    };

    class RunningGameEndingPhase : Phase {
        public override Dictionary<int, List<Action>> LoopFuncs {
            get; set;
        } = new Dictionary<int, List<Action>>() {
           { 1, new List<Action>()
                {
                    SkipPostGame
                }
           }
        };


        static void SkipPostGame() {
            int wait_timer = 0;
            while(Program.cg.GetGameState() != GameState.Ending_Commend && wait_timer < 20) {
                System.Threading.Thread.Sleep(1000);
                wait_timer++;
                System.Diagnostics.Debug.WriteLine(string.Format("{0} seconds wait", wait_timer));
            }
            Program.NextMap();
        }

        public override void Enter() {
            System.Diagnostics.Debug.WriteLine("game ending phase");
            Program.GameOver();
        }
        public override void Exit() {
        }
    }


    class Program {
        public static CustomGame cg = new CustomGame(default(IntPtr), ScreenshotMethods.ScreenCopy);
        public static int num_bots = 0;
        public static GameLogger current_game_log;
        //public static Phase phase = new First30SecondsPhase();
        public static Phase phase;
        public static bool game_ended = false;
        public static double time_at_current_map = 0;

        public static int current_red_count = 0;
        public static int current_blue_count = 0;
        public static int prev_red_count = 0;
        public static int prev_blue_count = 0;

        static DateTime now = DateTime.Now;
        static DateTime birth = DateTime.Parse("2017-02-16T00:00:00-0:00");
        static DateTime autobalance_start_time = DateTime.Parse("2017-02-16T00:00:00-0:00");
        static Random rnd = new Random();
        static int ticks_per_second = 2;
        static int blue_team_size_advantage = 0;
        static int prev_blue_team_size_advantage = 0;
        static bool autobalance = false;
        static MapChooser map_chooser;
        static DateTime last_game_over = DateTime.Parse("2017-02-16T00:00:00-0:00");


        static void Main(string[] args) {
            map_chooser = new MapChooser(cg);

            cg.OnGameOver += SetGameEnded;

            EnterPhase(typeof(GamePhase));

            GameLoop loop = new GameLoop();

            loop.Start();
        }

        public static void EnterPhase(Type new_phase) {
            if(phase != null) {
                phase.Exit();
            }
            phase = (Phase) Activator.CreateInstance(new_phase);
            phase.Enter();
        }

        static void SetGameEnded(object sender, GameOverArgs e) {
            double passed_seconds = DateTime.Now.Subtract(last_game_over).TotalSeconds;

            if(passed_seconds > 60) { //Cant gameover twice in quick succession.
                System.Diagnostics.Debug.WriteLine("game ended");
                last_game_over = DateTime.Now;

                if(current_game_log != null) {
                    System.Diagnostics.Debug.WriteLine(String.Format("Winning team is {0}.", e.GetWinningTeam()));
                    current_game_log.winning_team = e.GetWinningTeam();
                }
                game_ended = true;
            }
        }

        public static void GameOver() {
            game_ended = false;
            System.Diagnostics.Debug.WriteLine("running game over");
            
            cg.Chat.Chat(String.Format(">>Unreal 300%. Faster and more lethal, with {0} months of iterative testing.", Math.Round(now.Subtract(birth).Days / (365.25 / 12))));
            //cg.Chat.Chat(">>If you'd like to find the server again, you can friend me so it will appear at the top of your server browser.");
            cg.Chat.Chat(">>This server should be up 24/7. Search for Unreal to find it again.");

            map_chooser.SetRandomMap(); 
        }

        public static void print_running_trace() {
            System.Diagnostics.Debug.WriteLine("...Running...");
        }

        public static void prevent_map_timeout() {
            time_at_current_map++;
            if(time_at_current_map > (28 * 60) * ticks_per_second && num_bots > 0) {
                cg.Chat.Chat(">>Sever is timing out soon. Moving on.");
                RandomAndNextMap();
                return;
            }
        }

        public static void OnePlayerDM() {
            if(MapChooser.mode_i == 3) {
                if(cg.PlayerCount < 2) {
                    cg.Chat.Chat(">>Can't have one person in DM. Moving on.");
                    RandomAndNextMap();
                }
            }
        }

        public static void RandomAndNextMap() {
            System.Diagnostics.Debug.WriteLine("Random next map");
            map_chooser.SetRandomMap();

            NextMap();
        }

        public static void NextMap() {
            System.Diagnostics.Debug.WriteLine("Next map");
            time_at_current_map = 5;
            cg.RestartGame();
            Program.EnterPhase(typeof(First30SecondsPhase));
        }

        public static void ScrambleTeams() {
            if(cg.PlayerCount > 0) {
                cg.Chat.Chat(">>Scrambling teams.");
                SettleBlueTeam();
                SettleRedTeam();

                ShuffleBlueTeam();

                int total_rows = Math.Max(cg.RedCount, cg.BlueCount);
                int num_swaps = (total_rows + 1) / 2;
                System.Diagnostics.Debug.WriteLine(String.Format("num_swaps {0}.", num_swaps));

                List<int> swappable_rows = new List<int>();
                for(int i = 0; i < total_rows; i++) {
                    swappable_rows.Add(i);
                }

                List<int> rows_to_swap = new List<int>();
                for(int i = 0; i < num_swaps; i++) {
                    int swappable_row_index = rnd.Next(swappable_rows.Count);
                    int row = swappable_rows[swappable_row_index];
                    rows_to_swap.Add(row);
                    swappable_rows.RemoveAt(swappable_row_index);
                }

                for(int i = 0; i < rows_to_swap.Count; i++) {
                    int row = rows_to_swap[i];
                    cg.Interact.Move(row, row + 6);
                }
            }
        }


        static List<int> EmptyRedSlots() {
            List<int> empty_slots = new List<int>();
            for(int i = 6; i < 12; i++) {
                if(!cg.RedSlots.Contains(i)) {
                    empty_slots.Add(i);
                }
            }
            return empty_slots;
        }

        static List<int> EmptyBlueSlots() {
            List<int> empty_slots = new List<int>();
            for(int i = 0; i < 6; i++) {
                if(!cg.BlueSlots.Contains(i)) {
                    empty_slots.Add(i);
                }
            }
            return empty_slots;
        }

        static void SettleBlueTeam() {
            System.Diagnostics.Debug.WriteLine("Settling blue team");
            List<int> filled_slots = cg.BlueSlots;
            System.Diagnostics.Debug.WriteLine(String.Format("blue filled slots {0}.", filled_slots));
            int swaps = 0;
            for(int i = 0; i < filled_slots.Count; i++) {
                if(!filled_slots.Contains(i)) {
                    cg.Interact.Move(filled_slots[filled_slots.Count-swaps-1], i);
                    swaps++;
                }
            }
        }

        static void SettleRedTeam() {
            System.Diagnostics.Debug.WriteLine("Settling red team");
            List<int> filled_slots = cg.RedSlots;
            System.Diagnostics.Debug.WriteLine(String.Format("red filled slots {0}.", filled_slots));
            for(int i = 0; i < filled_slots.Count; i++) {
                if(filled_slots[i] != i + 6) {
                    cg.Interact.Move(filled_slots[i], i + 6);
                }
            }
        }

        static void ShuffleBlueTeam() {
            List<int> slots = cg.BlueSlots;
            if(slots.Count > 1) {
                System.Diagnostics.Debug.WriteLine(String.Format("Shuffling team starting at {0}", slots[0]));
                for(int i = 0; i < slots.Count - 1; i++) {
                    int random_index = rnd.Next(i + 1, slots.Count);
                    cg.Interact.Move(i, random_index);
                }
            }
        }

        public static int GetBlueTeamSizeAdvantage() {
            return cg.BlueCount - cg.RedCount;
        }

        public static void HandleAutoBalance() {
            prev_blue_team_size_advantage = blue_team_size_advantage;
            blue_team_size_advantage = GetBlueTeamSizeAdvantage();
            if(Math.Abs(blue_team_size_advantage) < 2) {
                EndAutoBalance();
            } else if(Math.Abs(blue_team_size_advantage) == 2) {
                System.Diagnostics.Debug.WriteLine(String.Format("blue team size advantage {0} vs {1}", cg.BlueCount, cg.RedCount));
                if(Math.Abs(prev_blue_team_size_advantage) >= 2) {
                    BeginAutoBalance();
                }
            } else if(Math.Abs(blue_team_size_advantage) >= 3) {
                System.Diagnostics.Debug.WriteLine(String.Format("blue team size advantage {0} vs {1}", cg.BlueCount, cg.RedCount));
                BeginAutoBalance();
            }
        }

        public static void PerformAutoBalance() {
            blue_team_size_advantage = GetBlueTeamSizeAdvantage();
            if(autobalance) {
                if(Math.Abs(blue_team_size_advantage) < 2) {
                    EndAutoBalance();
                } else {
                    //Wait for death
                    if(DateTime.Now.Subtract(autobalance_start_time).TotalSeconds < 15) {
                        List<int> slots;
                        List<int> empties;
                        if(blue_team_size_advantage > 0) {
                            slots = cg.BlueSlots;
                            empties = EmptyRedSlots();
                        } else {
                            slots = cg.RedSlots;
                            empties = EmptyBlueSlots();
                        }

                        List<int> dead = cg.PlayerInfo.PlayersDead();
                        foreach(int dead_slot in dead) {
                            if(slots.Contains(dead_slot)) {
                                if(empties.Count > 0) {
                                    int last_empty = empties[empties.Count - 1];
                                    cg.Interact.Move(dead_slot, last_empty);
                                }
                            }
                        }
                    } else {//Swap anyone
                        SwapToBalance();
                    }
                }
            }
        }

        public static void SwapToBalance() {
            blue_team_size_advantage = GetBlueTeamSizeAdvantage();
            if(Math.Abs(blue_team_size_advantage) >= 2) {
                List<int> slots;
                List<int> empties;
                if(blue_team_size_advantage > 0) {
                    slots = cg.BlueSlots;
                    empties = EmptyRedSlots();
                } else {
                    slots = cg.RedSlots;
                    empties = EmptyBlueSlots();
                }

                int random_slot = slots[rnd.Next(slots.Count)];
                if(empties.Count > 0) {
                    int last_empty = empties[empties.Count - 1];
                    cg.Interact.Move(random_slot, last_empty);
                }
            }
        }


        static void BeginAutoBalance() {
            if(!autobalance) {
                autobalance_start_time = DateTime.Now;
                cg.Chat.Chat(">>Team size imbalance detected. Swapping someone from the larger team.");
                autobalance = true;
                RemoveBotsIfAny();
            }
        }

        static void EndAutoBalance() {
            if(autobalance) {
                cg.Chat.Chat(">>Team sizes balanced.");
                autobalance = false;
            }
        }

        public static void HandleBots() {
            if(MapChooser.mode_i == 3) {
                RemoveBotsIfAny();
            } else {
                //Remove bots if there are more than there should be.
                if(num_bots > 4) {
                    System.Diagnostics.Debug.WriteLine(String.Format("Bot count {0} > 4. Removing bots.", num_bots));
                    RemoveBotsIfAny();
                }

                //Remove bots if many players
                int human_count = cg.PlayerCount - num_bots;
                if(human_count >= 7 && num_bots > 1) {
                    System.Diagnostics.Debug.WriteLine(String.Format("Human count {0} >= 7. Removing bots.", human_count));
                    RemoveBotsIfAny();
                }

                //Add bots if few players
                if(human_count < 7 && num_bots < 4 && Math.Abs(GetBlueTeamSizeAdvantage())<2) {
                    System.Diagnostics.Debug.WriteLine(String.Format("Human count {0} < 7 and BotCount {1} < 4. Adding bots.", human_count, num_bots));
                    cg.AI.AddAI(AIHero.Roadhog, Difficulty.Medium, BotTeam.Red, 1);
                    cg.AI.AddAI(AIHero.Roadhog, Difficulty.Medium, BotTeam.Blue, 1);
                    cg.AI.AddAI(AIHero.McCree, Difficulty.Medium, BotTeam.Red, 1);
                    cg.AI.AddAI(AIHero.McCree, Difficulty.Medium, BotTeam.Blue, 1);
                    num_bots += 4;
                }
            }
        }

        public static void RemoveBotsIfAny() {
            if(num_bots > 0) {
                RemoveBots();
            }
        }

        public static void RemoveBots() {
            cg.AI.RemoveAllBotsAuto();
            num_bots = 0;
            System.Diagnostics.Debug.WriteLine("All bots removed.");
        }


        public static void BeginGameLog() {
            System.Diagnostics.Debug.WriteLine("Beginning log");

            current_game_log = new GameLogger();
            
        }

        public static void UpdateGameLog() {
            if(current_game_log != null) {
                if(cg.RedCount + cg.BlueCount > 0) {
                    current_game_log.num_ticks++;

                    current_game_log.player_count[Team.Blue] += cg.BlueCount;
                    current_game_log.player_count[Team.Red] += cg.RedCount;
                    if(num_bots == 4) {
                        current_game_log.player_count[Team.Blue]--;
                        current_game_log.player_count[Team.Red]--;
                    }

                    prev_blue_count = current_blue_count;
                    prev_red_count = current_red_count;

                    current_blue_count = cg.BlueCount;
                    current_red_count = cg.RedCount;

                    int blue_joins = Math.Max(0, current_blue_count - prev_blue_count);
                    int blue_leaves = Math.Min(0, current_blue_count - prev_blue_count);

                    int red_joins = Math.Max(0, current_red_count - prev_red_count);
                    int red_leaves = Math.Min(0, current_red_count - prev_red_count);

                    current_game_log.joins += blue_joins + red_joins;
                    current_game_log.leaves += blue_leaves + red_leaves;

                    foreach(int slot in cg.BlueSlots) {
                        Hero? hero_or_null = cg.PlayerInfo.GetHero(slot);
                        if(hero_or_null != null) {
                            Hero hero = (Hero)hero_or_null;
                            current_game_log.hero_play_time[Team.Blue][hero]++;
                        }
                    }
                    foreach(int slot in cg.RedSlots) {
                        Hero? hero_or_null = cg.PlayerInfo.GetHero(slot);
                        if(hero_or_null != null) {
                            Hero hero = (Hero)hero_or_null;
                            current_game_log.hero_play_time[Team.Red][hero]++;
                        }
                    }
                    if(num_bots == 4) {
                        current_game_log.hero_play_time[Team.Blue][Hero.McCree]--;
                        current_game_log.hero_play_time[Team.Blue][Hero.Roadhog]--;
                        current_game_log.hero_play_time[Team.Red][Hero.McCree]--;
                        current_game_log.hero_play_time[Team.Red][Hero.Roadhog]--;
                    }
                }
            }
        }

        public static void EndGameLog() {
            System.Diagnostics.Debug.WriteLine("Saving log");
            if(current_game_log != null) {
                current_game_log.end_time = DateTime.Now;

                using(System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\life itself\Unreal300%\Logs\GameLogs.txt", true)) {
                    file.WriteLine("=============================\n");
                    file.WriteLine(String.Format("Start: {0}\n", current_game_log.start_time));
                    file.WriteLine(String.Format("End: {0}\n", current_game_log.end_time));
                    file.WriteLine(String.Format("Winning team: {0}\n", current_game_log.winning_team));
                    file.WriteLine(String.Format("Map: {0}\n", CustomGame.CG_Maps.MapNameFromID(current_game_log.map)));
                    file.WriteLine(String.Format("Mode index: {0}\n", current_game_log.mode_i));

                    file.WriteLine(String.Format("Joins: {0}\n", current_game_log.joins));
                    file.WriteLine(String.Format("Leaves: {0}\n", current_game_log.leaves));

                    file.WriteLine(String.Format("Blue player count: {0}\n", (double)current_game_log.player_count[Team.Blue] / (double)current_game_log.num_ticks));
                    file.WriteLine(String.Format("Red player count: {0}\n", (double)current_game_log.player_count[Team.Red] / (double)current_game_log.num_ticks));

                    Team[] teams = { Team.Blue, Team.Red };
                    foreach(Team team in teams) {
                        string[] names = Enum.GetNames(typeof(Hero));
                        Hero[] heroes = Enum.GetValues(typeof(Hero)).Cast<Hero>().ToArray();

                        for(int i = 0; i < heroes.Length; i++) {
                            file.WriteLine(String.Format(
                                "{0} {1} Average count: {2}\n", 
                                team, 
                                names[i], 
                                (double)current_game_log.hero_play_time[team][heroes[i]] / (double)current_game_log.num_ticks));
                        }
                        
                    }

                }
            }

        }


    }
}
