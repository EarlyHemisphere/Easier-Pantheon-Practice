using System;
using Modding;
using UnityEngine;
using USceneManager = UnityEngine.SceneManagement.SceneManager;
using Satchel.BetterMenus;

namespace Easier_Pantheon_Practice
{
    public class EasierPantheonPractice : Mod, ITogglableMod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
    {
        public EasierPantheonPractice() : base("Easier Pantheon Practice") {}
        public override string GetVersion() => "v1.0.7";
        public bool ToggleButtonInsideMenu => true;

        private bool isEnabled;
        private Menu ExtraSettings;
        public static Menu menuRef = null;

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {

            string[] maskvalues = new string[11];
            for (int i = 0; i < 10; i++)
            {
                maskvalues[i] = i.ToString();
            }

            string[] soulvalues = new string[7];
            for (int i = 0; i * 33 <= 198; i++)
            {
                soulvalues[i] = (i * 33).ToString();
            }
            
            string[] boolvalues = {"False", "True"};

            ExtraSettings ??= new Menu(
                "Additional Settings",
                new Element[]{
                    new HorizontalOption(
                        name: "Funny Descriptions",
                        description: "",
                        values: new[]{"False", "True"},
                        applySetting: val => settings.funny_descriptions = val == 1,
                        loadSetting: () => settings.funny_descriptions ? 1 : 0
                    ),
                    new HorizontalOption(
                        name: "Only Apply Settings",
                        description: "",
                        values: new[]{"False", "True"},
                        applySetting: val => settings.only_apply_settings = val == 1,
                        loadSetting: () => settings.funny_descriptions ? 1 : 0
                    ),
                    new HorizontalOption(
                        name: "Can Reloads Boss in Loads",
                        description: "",
                        values: new[]{"False", "True"},
                        applySetting: val => settings.allow_reloads_in_loads = val == 1,
                        loadSetting: () => settings.allow_reloads_in_loads ? 1 : 0
                    ),
                    new KeyBind(
                        name: "Move Around HoG",
                        playerAction: settings.keybinds.Key_teleport_around_HoG
                    )
                }
            );

            menuRef ??= new Menu(
                "Easier Pantheon Practice Settings",
                new Element[] {
                    new HorizontalOption(
                        name: "Toggle Mod",
                        description: "",
                        values: new []{"On","Off"},
                        applySetting: val => isEnabled = val == 0,
                        loadSetting: () => settings.remove_health
                    ),
                    new HorizontalOption(
                        name: "Remove Health",
                        description: "",
                        values: maskvalues,
                        applySetting: val => settings.remove_health = val,
                        loadSetting: () => settings.remove_health
                    ),
                    new HorizontalOption(
                        name: "Lifeblood",
                        description: "",
                        values: maskvalues,
                        applySetting: val => settings.lifeblood = val,
                        loadSetting: () => settings.lifeblood
                    ),
                    new HorizontalOption(
                        name: "Soul",
                        description: "",
                        values: soulvalues,
                        applySetting: val => settings.soul = Int32.Parse(soulvalues[val]),
                        loadSetting: () => settings.soul
                    ),
                    new HorizontalOption(
                        name: "Hitless Practice",
                        description: "",
                        values: new[]{"False", "True"},
                        applySetting: val => settings.hitless_practice = val == 1,
                        loadSetting: () => settings.hitless_practice ? 1 : 0
                    ),
                    new HorizontalOption(
                        name: "Reload Boss On Death",
                        description: "",
                        values: new[]{"False", "True"},
                        applySetting: val => {
                            Modding.Logger.Log("HERE");
                            Modding.Logger.Log(val);
                            Modding.Logger.Log(val != 0);
                            settings.reload_boss_on_death = val == 1;
                        },
                        loadSetting: () => settings.reload_boss_on_death ? 1 : 0
                    ),
                    new HorizontalOption(
                        name: "Infinite AnyRad Plats Practice",
                        description: "",
                        values: new[]{"False", "True"},
                        applySetting: val => settings.infinite_anyrad_plats_practice = val == 1,
                        loadSetting: () => settings.infinite_anyrad_plats_practice ? 1 : 0
                    ),
                    new KeyBind(
                        name: "Reload Boss",
                        playerAction: settings.keybinds.Key_Reload_Boss
                    ),
                    new KeyBind(
                        name: "Return To HoG",
                        playerAction: settings.keybinds.Key_return_to_hog
                    ),
                    Blueprints.NavigateToMenu(
                        name: "Additional Settings",
                        description: "",
                        getScreen: () => ExtraSettings.GetMenuScreen(menuRef.menuScreen)
                    ),
                }
            );

            return menuRef.GetMenuScreen(modListMenu);
        }

        public static GlobalSettings settings { get; set; } = new GlobalSettings();
        public void OnLoadGlobal(GlobalSettings s) => EasierPantheonPractice.settings = s;
        public GlobalSettings OnSaveGlobal() => EasierPantheonPractice.settings;


        internal static EasierPantheonPractice Instance;

        public override void Initialize()
        {
            Instance = this;
            
            Log("Trying to load mod");

            Load_Mod();

            ModHooks.SavegameLoadHook += Load_Save;
            ModHooks.NewGameHook += Load_Mod;
            ModHooks.LanguageGetHook += BossDesc;
        }

        public override int LoadPriority() => 2;

        private string BossDesc(string key, string sheettitle, string orig)
        {
            #region for the trolls

            if (settings.funny_descriptions)
            {
                switch (key)
                {
                    case "NAME_MEGA_MOSS_CHARGER": return "UNKILLABLE MOSS CHARGER";
                    case "GG_S_MEGAMOSS":          return "The True Champion of the Gods. Try as hard as you want, you can not kill it";
                    case "MEGA_MOSS_SUPER":        return "UNKILLABLE";
                    case "MEGA_MOSS_SUB":          return "THE TRUE CHAMPION OF THE GODS";

                    case "GG_S_GRUZ":              return "My head hurts. Please dont make me slam my head again";
                    case "GG_S_BIGBUZZ":           return "Vicious God of running away";
                    case "GG_S_FLUKEMUM":          return "Alluring God of standing still";
                    case "GG_S_BIGBEES":           return "Gods of RNG";
                    case "GG_S_NOSK_HORNET":       return "Vicious God of running away, but worse";
                    case "GG_S_COLLECTOR":         return "The boss that gives nightmares to All Binding players";
                    case "GG_S_MIGHTYZOTE":        return "I like giving ear aches";
                    case "KNIGHT_STATUE_1":        
                    case "KNIGHT_STATUE_2":        
                    case "KNIGHT_STATUE_3":        return "Did you really just spend all these hours grinding just to get this??";
                    case "GG_S_SLY":               return "Bug Yoda";
                    case "GG_S_GHOST_HU":          return "I love PANCAKES";
                    case "GG_S_GHOST_GORB":        return "Ascend with Gorb";
                    case "GG_S_SOULMASTER":        return "Teleporting freak";
                    case "GG_S_SOUL_TYRANT":       return "Teleporting freak v2";
                    case "GG_S_MAGEKNIGHT":        return "Am i really a boss?";
                    case "CHARM_NAME_2":           return "OP Compass";
                    case "CHARM_DESC_2":           return "Its the most OP charm in the game.<br><br>Wear this charm to get good";
                }
            }

            #endregion

            return orig;
        }
        

        private void Load_Mod()
        {
            var MainComponent = GameManager.instance.gameObject.GetComponent<FindBoss>();
            if (MainComponent == null) GameManager.instance.gameObject.AddComponent<FindBoss>();
        }

        private void Load_Save(int obj)
        {
            Load_Mod();
        }

        public void Unload()
        {
            ModHooks.LanguageGetHook -= BossDesc;
            ModHooks.SavegameLoadHook -= Load_Save;
            ModHooks.NewGameHook -= Load_Mod;
            var MainComponent = GameManager.instance?.gameObject.GetComponent<FindBoss>();
            if (MainComponent != null) UnityEngine.Object.Destroy(MainComponent);

        }
    }
}