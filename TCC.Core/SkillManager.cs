﻿using System;
using TCC.Data;
using TCC.Data.Databases;
using TCC.Data.Skills;
using TCC.ViewModels;

namespace TCC
{
    public static class SkillManager
    {
        public static event Action SkillStarted;

        public const int LongSkillTreshold = 40000;
        public const int Ending = 1; //TODO: this stuff should be removed btw



        public static void AddSkill(uint id, ulong cd)
        {
            if (SessionManager.SkillsDatabase.TryGetSkill(id, SessionManager.CurrentPlayer.Class, out var skill))
            {
                if (!Pass(skill)) return;
                RouteSkill(new Cooldown(skill, cd));
                //WindowManager.SkillsEnded = false;
            }
        }
        public static void AddItemSkill(uint id, uint cd)
        {
            if (SessionManager.ItemsDatabase.TryGetItemSkill(id, out var brooch))
            {
                try
                {
                    RouteSkill(new Cooldown(brooch, cd, CooldownType.Item));

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

        }
        public static void AddPassivitySkill(uint abId, uint cd)
        {
            if (PassivityDatabase.TryGetPassivitySkill(abId, out var skill))
            {
                RouteSkill(new Cooldown(skill, cd * 1000, CooldownType.Passive));
            }

        }
        public static void AddSkillDirectly(Skill sk, uint cd)
        {
            RouteSkill(new Cooldown(sk, cd));
        }

        public static void ChangeSkillCooldown(uint id, uint cd)
        {
            if (SessionManager.SkillsDatabase.TryGetSkill(id, SessionManager.CurrentPlayer.Class, out var skill))
            {
                if (!Pass(skill)) return;
                CooldownWindowViewModel.Instance.Change(skill, cd);
            }

        }
        public static void ResetSkill(uint id)
        {
            if (SessionManager.SkillsDatabase.TryGetSkill(id, SessionManager.CurrentPlayer.Class, out var skill))
            {
                if (!Pass(skill)) return;
                CooldownWindowViewModel.Instance.ResetSkill(skill);
            }
        }
        public static void Clear()
        {
            CooldownWindowViewModel.Instance.ClearSkills();
        }

        private static void RouteSkill(Cooldown skillCooldown)
        {
            if (skillCooldown.Duration== 0)
            {
                skillCooldown.Dispose();
                CooldownWindowViewModel.Instance.Remove(skillCooldown.Skill);
            }
            else
            {
                CooldownWindowViewModel.Instance.AddOrRefresh(skillCooldown);
            }
            App.BaseDispatcher.Invoke(() => SkillStarted?.Invoke());
        }
        private static bool Pass(Skill sk)
        {
            if (sk.Detail == "off") return false;
            return sk.Class != Class.Common && sk.Class != Class.None;
        }
    }
}
