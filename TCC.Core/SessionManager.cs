﻿using System;
using System.Collections.Generic;
using System.Linq;
using TCC.Data;
using TCC.Data.Databases;
using TCC.Tera.Data;
using TCC.TeraCommon.Game;
using TCC.ViewModels;
using Player = TCC.Data.Pc.Player;

namespace TCC
{
    public static class SessionManager
    {
        public const int MaxWeekly = 16;
        public const int MaxDaily = 16;
        public const int MaxGuardianQuests = 40;
        private static bool _logged;
        private static bool _loadingScreen = true;
        private static bool _encounter;
        private static bool _inGameChatOpen;
        private static bool _inGameUiOn;

        public static Server Server { get; set; }
        public static string Language => BasicTeraData.Instance.Servers.StringLanguage;


        public static bool LoadingScreen
        {
            get => _loadingScreen;
            set
            {
                if (_loadingScreen == value) return;
                _loadingScreen = value;
                //WindowManager.NotifyVisibilityChanged();
                App.BaseDispatcher.Invoke(() => LoadingScreenChanged?.Invoke());

            }
        }
        public static bool Encounter
        {
            get => _encounter;
            set
            {
                if (_encounter == value) return;
                _encounter = value;
                App.BaseDispatcher.Invoke(() => EncounterChanged?.Invoke());
            }
        }

        public static bool Combat
        {
            get => CurrentPlayer?.IsInCombat ?? false;
            set
            {
                if (Combat != value) App.BaseDispatcher.Invoke(() => CombatChanged?.Invoke());
                CurrentPlayer.IsInCombat = value;
            }
        }

        public static bool Logged
        {
            get => _logged;
            set
            {
                if (_logged == value) return;
                _logged = value;
                App.BaseDispatcher.Invoke(() => LoggedChanged?.Invoke());
            }
        }
        public static bool IsElite { get; set; }

        public static bool InGameUiOn
        {
            get => _inGameUiOn;
            set
            {
                if (_inGameUiOn == value) return;
                _inGameUiOn = value;
                GameUiModeChanged?.Invoke();
            }
        }

        public static bool InGameChatOpen
        {
            get => _inGameChatOpen;
            set
            {
                if (_inGameChatOpen == value) return;
                _inGameChatOpen = value;
                ChatModeChanged?.Invoke();
            }
        }

        public static event Action ChatModeChanged;
        public static event Action GameUiModeChanged;
        public static event Action EncounterChanged;
        public static event Action CombatChanged;
        public static event Action LoadingScreenChanged;
        public static event Action LoggedChanged;

        public static readonly Player CurrentPlayer = new Player();

        public static readonly Dictionary<uint, string> GuildMembersNames = new Dictionary<uint, string>();

        public static string GetGuildMemberName(uint id)
        {
            return GuildMembersNames.ContainsKey(id) ? GuildMembersNames[id] : "Unknown player";
        }

        public static AccountBenefitDatabase AccountBenefitDatabase { get; private set; }
        public static MonsterDatabase MonsterDatabase { get; private set; }
        public static ItemsDatabase ItemsDatabase { get; private set; }
        public static SkillsDatabase SkillsDatabase { get; private set; }
        public static SystemMessagesDatabase SystemMessagesDatabase { get; private set; }
        public static GuildQuestDatabase GuildQuestDatabase { get; private set; }
        public static AchievementDatabase AchievementDatabase { get; private set; }
        public static AchievementGradeDatabase AchievementGradeDatabase { get; private set; }
        public static MapDatabase MapDatabase { get; private set; }
        public static QuestDatabase QuestDatabase { get; private set; }
        public static AbnormalityDatabase AbnormalityDatabase { get; private set; }
        public static DungeonDatabase DungeonDatabase { get; private set; }
        public static SocialDatabase SocialDatabase { get; private set; }
        public static bool CivilUnrestZone { get; internal set; }

        //public static void SetCombatStatus(bool combat)
        //{
        //    //var old = Me.IsInCombat;
        //    //Me.IsInCombat = combat;
        //    //if (combat != old) App.BaseDispatcher.Invoke(() => CombatChanged?.Invoke());
        //}
        public static void SetPlayerHp(float hp)
        {
            CurrentPlayer.CurrentHP = hp;
        }
        public static void SetPlayerMp(ulong target, float mp)
        {
            if (target != CurrentPlayer.EntityId) return;
            CurrentPlayer.CurrentMP = mp;
        }
        public static void SetPlayerSt(ulong target, float st)
        {
            if (target != CurrentPlayer.EntityId) return;
            CurrentPlayer.CurrentST = st;
            if (Settings.SettingsHolder.ClassWindowSettings.Enabled) ClassWindowViewModel.Instance.CurrentManager.SetST(Convert.ToInt32(st));
        }
        public static void SetPlayerFe(float en)
        {
            CurrentPlayer.FlightEnergy = en;
            WindowManager.FlightDurationWindow.SetEnergy(en);
        }
        public static void SetPlayerLaurel(Player p)
        {
            try
            {
                p.Laurel = WindowManager.Dashboard.VM.Characters.First(x => x.Name == p.Name).Laurel;
            }
            catch
            {
                p.Laurel = Laurel.None;
            }
        }

        public static void SetPlayerMaxHp(ulong target, long maxHp)
        {
            if (target != CurrentPlayer.EntityId) return;
            CurrentPlayer.MaxHP = maxHp;
            //ClassManager.SetMaxHP(Convert.ToInt32(maxHp));
        }
        public static void SetPlayerMaxMp(ulong target, int maxMp)
        {
            if (target != CurrentPlayer.EntityId) return;
            CurrentPlayer.MaxMP = maxMp;
            //ClassManager.SetMaxMP(Convert.ToInt32(maxMp));
        }
        public static void SetPlayerMaxSt(ulong target, int maxSt)
        {
            if (target != CurrentPlayer.EntityId) return;
            CurrentPlayer.MaxST = maxSt;
            if (Settings.SettingsHolder.ClassWindowSettings.Enabled) ClassWindowViewModel.Instance.CurrentManager.SetMaxST(Convert.ToInt32(maxSt));
        }

        public static void SetPlayerShield(uint damage)
        {
            if (CurrentPlayer.CurrentShield < 0) return;
            CurrentPlayer.CurrentShield -= damage;
        }
        public static void SetPlayerMaxShield(uint shield)
        {
            CurrentPlayer.MaxShield = shield;
            CurrentPlayer.CurrentShield = shield;
        }

        public static void InitDatabases(string lang)
        {
            MonsterDatabase = new MonsterDatabase(lang);
            AccountBenefitDatabase = new AccountBenefitDatabase(lang);
            ItemsDatabase = new ItemsDatabase(lang);
            SkillsDatabase = new SkillsDatabase(lang);
            AbnormalityDatabase = new AbnormalityDatabase(lang);
            DungeonDatabase = new DungeonDatabase(lang);
            SocialDatabase = new SocialDatabase(lang);
            SkillsDatabase = new SkillsDatabase(lang);
            SystemMessagesDatabase = new SystemMessagesDatabase(lang);
            GuildQuestDatabase = new GuildQuestDatabase(lang);
            AchievementDatabase = new AchievementDatabase(lang);
            AchievementGradeDatabase = new AchievementGradeDatabase(lang);
            MapDatabase = new MapDatabase(lang);
            QuestDatabase = new QuestDatabase(lang);
        }

        public static void SetSorcererElements(bool pFire, bool pIce, bool pArcane)
        {
            CurrentPlayer.Fire = pFire;
            CurrentPlayer.Ice = pIce;
            CurrentPlayer.Arcane = pArcane;
            
            if (Settings.SettingsHolder.ClassWindowSettings.Enabled && CurrentPlayer.Class == Class.Sorcerer)
            {
                ((SorcererBarManager)ClassWindowViewModel.Instance.CurrentManager).NotifyElementChanged();
            }

        }

        public static void SetSorcererElementsBoost(bool f, bool i, bool a)
        {
            CurrentPlayer.FireBoost = f;
            CurrentPlayer.IceBoost = i;
            CurrentPlayer.ArcaneBoost = a;

            if (Settings.SettingsHolder.ClassWindowSettings.Enabled && CurrentPlayer.Class == Class.Sorcerer)
            {
                ((SorcererBarManager)ClassWindowViewModel.Instance.CurrentManager).NotifyElementBoostChanged();
            }


        }
    }
}
