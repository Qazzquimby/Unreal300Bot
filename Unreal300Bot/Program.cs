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
        static Dictionary<int, Dictionary<GameState, List<Action>>> loop_funcs = new Dictionary<int, Dictionary<GameState, List<Action>>>();
        static Dictionary<int, List<Action>> map_time_triggers = new Dictionary<int, List<Action>>();

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
                GameState current_state = cg.GetGameState();
                if(current_state == GameState.Unknown) {
                    current_state = GameState.Ingame; //Just assume
                }
                foreach(int delay in loop_funcs.Keys) {
                    if(server_time % delay == 0) {
                        if(loop_funcs[delay].Keys.Contains(current_state)) {
                            foreach(Action func in loop_funcs[delay][current_state]) {
                                func();
                            }
                        }
                    }
                }
                foreach(int delay in map_time_triggers.Keys) {
                    if(Program.map_time == delay) {
                        if(current_state == GameState.Ingame || current_state == GameState.Unknown) {
                            foreach(Action func in map_time_triggers[delay]) {
                                func();
                            }
                        }
                    }
                }

                server_time += 1;
                Program.map_time += 1;
                System.Threading.Thread.Sleep(500);
            }
        }

        public void AddToLoop(Action func, int delay, GameState game_state) {
            try {
                loop_funcs[delay][game_state].Add(func);
            } catch(KeyNotFoundException) {
                try {
                    loop_funcs[delay].Add(game_state, new List<Action>());
                    loop_funcs[delay][game_state].Add(func);
                } catch(KeyNotFoundException) {
                    loop_funcs.Add(delay, new Dictionary<GameState, List<Action>>());
                    loop_funcs[delay].Add(game_state, new List<Action>());
                    loop_funcs[delay][game_state].Add(func);
                }
            }

        }

        public void AtMapTime(Action func, int delay) {
            try {
                map_time_triggers[delay].Add(func);
            } catch(KeyNotFoundException) {
                map_time_triggers.Add(delay, new List<Action>());
                map_time_triggers[delay].Add(func);
            }
        }
    }



    class MapChooser {
        public static Map current = Map.C_Lijiang;
        public static int mode_i = 2;
        static CustomGame cg;
        static Random rnd = new Random();

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
            Map.TDM_Dorado,
            Map.TDM_Eichenwalde,
            Map.TDM_Hanamura,
            Map.TDM_Hollywood,
            Map.TDM_HorizonLunarColony,
            Map.TDM_Ilios_Lighthouse,
            Map.TDM_Ilios_Ruins,
            Map.TDM_Ilios_Well,
            Map.TDM_KingsRow,
            Map.TDM_Lijiang_ControlCenter,
            Map.TDM_Lijiang_Garden,
            Map.TDM_Lijiang_NightMarket,
            Map.TDM_Necropolis,
            Map.TDM_Nepal_Sanctum,
            Map.TDM_Nepal_Shrine,
            Map.TDM_Nepal_Village,
            Map.TDM_Oasis_CityCenter,
            Map.TDM_Oasis_Gardens,
            Map.TDM_Oasis_University,
            Map.TDM_Petra,
            Map.TDM_TempleOfAnubis,
            Map.TDM_VolskayaIndustries
        };

        static Map[][] modes = { AE_maps, A_maps, C_maps, TDM_maps };

        public MapChooser(CustomGame cg_input) {
            cg = cg_input;
            cg.ModesEnabled.Assault = true;
            cg.ModesEnabled.AssaultEscort = true;
            cg.ModesEnabled.Control = true;
            cg.ModesEnabled.Escort = true;
            cg.ModesEnabled.TeamDeathmatch = true;

            cg.CurrentOverwatchEvent = cg.GetCurrentOverwatchEvent();
        }

        public void SetRandomMap() {
            System.Diagnostics.Debug.WriteLine("Setting map");
            Map[] mode = RandomMode();

            Map map;
            map = mode[rnd.Next(mode.Length)];

            System.Diagnostics.Debug.WriteLine(String.Format("New Map is {0}.", CustomGame.CG_Maps.MapNameFromID(map)));
            SetMap(map);
        }

        static Map[] RandomMode() {
            int index;
            if(cg.PlayerCount - Program.num_bots >= 5) {
                index = rnd.Next(modes.Length);
            } else {
                index = rnd.Next(rnd.Next(3));
            }
            Map[] mode = modes[index];
            mode_i = index;
            return mode;
        }

        public void SetMap(Map map) {
            Map[] maps_to_set = { map };
            Map[] maps_to_unset = { current };
            cg.Maps.ToggleMap(ToggleAction.DisableAll, map);
            current = map;
        }

    }



    class GameLogger {
        public DateTime start_time;
        public DateTime end_time;

        public int joins;
        public int leaves;

        public Map map;
        public int mode_i;
        public Dictionary<Team, Dictionary<Hero, int>> hero_play_time;
        public PlayerTeam winning_team;
    }


    class StateMachine {

    }



    class Program {
        public static CustomGame cg = new CustomGame(default(IntPtr), ScreenshotMethods.ScreenCopy);
        public static int num_bots = 0;
        public static GameLogger current_game_log;
        public static int map_time = 0;
        public enum Server { LoggableGame, Balancing };

        static DateTime now = DateTime.Now;
        static DateTime birth = DateTime.Parse("2017-02-16T00:00:00-0:00");
        static Random rnd = new Random();
        static double time_at_current_map = 0;
        static int ticks_per_second = 2;
        static int blue_team_size_advantage = 0;
        static int prev_blue_team_size_advantage = 0;
        static bool autobalance = false;
        static MapChooser map_chooser;
        static DateTime last_game_over = DateTime.Parse("2017-02-16T00:00:00-0:00");

        static void Main(string[] args) {
            map_chooser = new MapChooser(cg);

            cg.OnGameOver += GameOver;

            GameLoop loop = new GameLoop();
            loop.AddToLoop(print_running_trace, 30, GameState.Ingame);
            loop.AddToLoop(prevent_map_timeout, 30, GameState.Ingame);
            loop.AddToLoop(HandleBots, 30, GameState.Ingame);
            loop.AddToLoop(HandleAutoBalance, 15, GameState.Ingame);

            loop.AddToLoop(CheckAutobalance, 10, GameState.Ingame);
            loop.AddToLoop(OnePlayerDM, 15, GameState.Ingame);

            loop.AtMapTime(EndGameLog, 30);
            loop.AtMapTime(BeginGameLog, 30);


            loop.Start();
        }

        static void GameOver(object sender, GameOverArgs e) {
            double passed_seconds = DateTime.Now.Subtract(last_game_over).TotalSeconds;
            System.Diagnostics.Debug.WriteLine(String.Format("Seconds past since previous game over {0}", passed_seconds));
            if(passed_seconds > 30) { //Cant gameover twice in quick succession.
                System.Diagnostics.Debug.WriteLine("Game over");
                last_game_over = DateTime.Now;
                cg.Chat.Chat(String.Format(">>Unreal 300%. Faster and more lethal, with {0} months of iterative testing.", Math.Round(now.Subtract(birth).Days / (365.25 / 12))));
                cg.Chat.Chat(">>If you'd like to find the server again, you can friend me so it will appear at the top of your server browser.");
                System.Diagnostics.Debug.WriteLine(String.Format("Winning team is {0}.", e.GetWinningTeam()));
                if(current_game_log != null) {
                    current_game_log.winning_team = e.GetWinningTeam();
                }
                RemoveBotsIfAny();
                map_chooser.SetRandomMap();
                if(cg.PlayerCount - num_bots < 4) {
                    NextMap();
                } else {
                    int wait_timer = 0;
                    while(cg.GetGameState() != GameState.Ending_Commend && wait_timer < 30) {
                        System.Threading.Thread.Sleep(50);
                        wait_timer++;
                    }
                    NextMap();
                }
            }
        }

        static void print_running_trace() {
            System.Diagnostics.Debug.WriteLine("...Running...");
        }

        static void prevent_map_timeout() {
            if(time_at_current_map > (25 * 60) * ticks_per_second) {
                cg.Chat.Chat(">>Sever is timing out soon. Moving on.");
                RandomAndNextMap();
                return;
            }
        }

        static void OnePlayerDM() {
            if(MapChooser.mode_i > 2) {
                if(cg.PlayerCount < 2) {
                    RandomAndNextMap();
                    cg.Chat.Chat(">>Can't have one person in DM. Moving on.");
                }
            }
        }

        static void CheckAutobalance() {
            if(autobalance) {
                if(Math.Abs(blue_team_size_advantage) < 2) {
                    EndAutoBalance();
                } else {
                    if(map_time < 30) {
                        ScrambleTeams();
                    } else {
                        List<int> slots;
                        List<int> empties;
                        if(blue_team_size_advantage > 0) {
                            slots = cg.BlueSlots;
                            empties = EmptyRedSlots();
                        } else {
                            slots = cg.RedSlots;
                            empties = EmptyBlueSlots();
                        }

                        List<int> dead = cg.PlayerInfo.PlayersDead(slots);
                        if(dead.Count > 0 && empties.Count > 0) {
                            int first_dead = dead[0];
                            int last_empty = empties[empties.Count - 1];
                            cg.Interact.Move(first_dead, last_empty);
                        }
                    }
                }
            }
        }

        static void NextMap() {
            System.Diagnostics.Debug.WriteLine("Next map");
            map_time = 0;
            RemoveBotsIfAny();

            time_at_current_map = 5;
            cg.RestartGame();

            System.Threading.Thread.Sleep(5000);
            ScrambleTeams();
        }

        static void RandomAndNextMap() {
            System.Diagnostics.Debug.WriteLine("Random next map");
            map_time = 0;
            RemoveBotsIfAny();
            map_chooser.SetRandomMap();

            time_at_current_map = 5;
            cg.RestartGame();

            System.Threading.Thread.Sleep(5000);
            ScrambleTeams();
        }

        static void ScrambleTeams() {
            System.Diagnostics.Debug.WriteLine("Scrambling teams");
            if(cg.PlayerCount > 0) {
                cg.Chat.Chat(">>Scrambling teams.");
                RemoveBotsIfAny();
                SettleBlueTeam();
                SettleRedTeam();

                System.Diagnostics.Debug.WriteLine("Shuffling teams.");
                ShuffleBlueTeam();

                int total_rows = Math.Max(cg.RedCount, cg.BlueCount);
                System.Diagnostics.Debug.WriteLine(String.Format("Total rows {0}.", total_rows));
                int num_swaps = (total_rows + 1) / 2;
                System.Diagnostics.Debug.WriteLine(String.Format("num_swaps {0}.", num_swaps));

                System.Diagnostics.Debug.WriteLine("Getting swappable rows.");
                List<int> swappable_rows = new List<int>();
                for(int i = 0; i < total_rows; i++) {
                    swappable_rows.Add(i);
                }

                System.Diagnostics.Debug.WriteLine("Getting rows to swap.");
                List<int> rows_to_swap = new List<int>();
                for(int i = 0; i < num_swaps; i++) {
                    int swappable_row_index = rnd.Next(swappable_rows.Count);
                    int row = swappable_rows[swappable_row_index];
                    rows_to_swap.Add(row);
                    swappable_rows.RemoveAt(swappable_row_index);
                }

                System.Diagnostics.Debug.WriteLine("Swapping rows.");
                for(int i = 0; i < rows_to_swap.Count; i++) {
                    int row = rows_to_swap[i];
                    cg.Interact.Move(row, row + 6);
                }
                System.Diagnostics.Debug.WriteLine("Done shuffling.");
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
            for(int i = 0; i < filled_slots.Count; i++) {
                if(filled_slots[i] != i) {
                    cg.Interact.Move(filled_slots[i], i);
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

        static int GetBlueTeamSizeAdvantage() {
            return cg.BlueCount - cg.RedCount;
        }

        static void HandleAutoBalance() {
            if(MapChooser.mode_i != 3) {
                prev_blue_team_size_advantage = blue_team_size_advantage;
                blue_team_size_advantage = GetBlueTeamSizeAdvantage();
                if(Math.Abs(blue_team_size_advantage) == 2) {
                    System.Diagnostics.Debug.WriteLine(String.Format("blue team size advantage {0} vs {1}", cg.BlueCount, cg.RedCount));
                    if(Math.Abs(prev_blue_team_size_advantage) >= 2) {
                        BeginAutoBalance();
                    }
                } else if(Math.Abs(blue_team_size_advantage) >= 3) {
                    System.Diagnostics.Debug.WriteLine(String.Format("blue team size advantage {0} vs {1}", cg.BlueCount, cg.RedCount));
                    BeginAutoBalance();
                }
            }
        }

        static void BeginAutoBalance() {
            if(!autobalance) {
                cg.Chat.Chat(">>Team size imbalance detected. Swapping next killed player on larger team.");
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

        static void HandleBots() {
            if(MapChooser.mode_i > 2) {
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
                if(human_count < 7 && num_bots < 4 && !autobalance) {
                    System.Diagnostics.Debug.WriteLine(String.Format("Human count {0} < 7 and BotCount {1} < 4. Adding bots.", human_count, num_bots));
                    cg.AI.AddAI(AIHero.Roadhog, Difficulty.Medium, BotTeam.Red, 1);
                    cg.AI.AddAI(AIHero.Roadhog, Difficulty.Medium, BotTeam.Blue, 1);
                    cg.AI.AddAI(AIHero.McCree, Difficulty.Medium, BotTeam.Red, 1);
                    cg.AI.AddAI(AIHero.McCree, Difficulty.Medium, BotTeam.Blue, 1);
                    num_bots += 4;
                }
            }
        }

        static void RemoveBotsIfAny() {
            if(num_bots > 0) {
                cg.AI.RemoveAllBotsAuto();
                num_bots = 0;
                System.Diagnostics.Debug.WriteLine("All bots removed.");
            }
        }

        static void BeginGameLog() {
            System.Diagnostics.Debug.WriteLine("Beginning log");
            current_game_log = new GameLogger();
            current_game_log.start_time = DateTime.Now;
            current_game_log.map = MapChooser.current;
            current_game_log.mode_i = MapChooser.mode_i;
        }

        static void UpdateGameLog() {

        }

        static void EndGameLog() {
            System.Diagnostics.Debug.WriteLine("Saving log");
            if(current_game_log != null) {
                current_game_log.end_time = DateTime.Now;

                using(System.IO.StreamWriter file = new System.IO.StreamWriter(@"D:\life itself\Unreal300%\Logs\GameLogs.txt", true)) {
                    file.WriteLine("=============================\n");
                    file.WriteLine(String.Format("{0}\n", current_game_log.start_time));
                    file.WriteLine(String.Format("{0}\n", current_game_log.end_time));
                    file.WriteLine(String.Format("{0}\n", current_game_log.winning_team));
                    file.WriteLine(String.Format("{0}\n", CustomGame.CG_Maps.MapNameFromID(current_game_log.map)));
                    file.WriteLine(String.Format("{0}\n", current_game_log.mode_i));
                }
            }

        }


    }
}
