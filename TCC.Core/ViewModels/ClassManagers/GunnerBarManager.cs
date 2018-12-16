﻿using TCC.Data;
using TCC.Data.Skills;

namespace TCC.ViewModels
{
    public class GunnerBarManager : ClassManager
    {

        public Cooldown BurstFire { get; set; }
        public Cooldown Balder { get; set; }
        public Cooldown Bombardment { get; set; }
        public DurationCooldownIndicator ModularSystem { get; set; }

        private void FlashBfIfFullWp(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == nameof(StaminaTracker.Val))
            //{
            //    if (StaminaTracker.Factor > .8) BurstFire.ForceEnded();
            //    else BurstFire.ForceStopFlashing();
            //}
        }

        public override void LoadSpecialSkills()
        {
            SessionManager.SkillsDatabase.TryGetSkill(51000, Class.Gunner, out var bfire);
            SessionManager.SkillsDatabase.TryGetSkill(130200, Class.Gunner, out var balder);
            SessionManager.SkillsDatabase.TryGetSkill(20600, Class.Gunner, out var bombard);
            SessionManager.SkillsDatabase.TryGetSkill(410100, Class.Gunner, out var modSys);


            BurstFire = new Cooldown(bfire, true);
            Bombardment = new Cooldown(bombard, false) { CanFlash = true };
            Balder = new Cooldown(balder, false) { CanFlash = true };

            ModularSystem = new DurationCooldownIndicator(Dispatcher)
            {
                Buff = new Cooldown(modSys, false),
                Cooldown = new Cooldown(modSys, true) { CanFlash = true }
            };
            Balder.FlashOnAvailable = true;
            Bombardment.FlashOnAvailable = true;
            ModularSystem.Cooldown.FlashOnAvailable = true;

            //StaminaTracker.PropertyChanged += FlashBfIfFullWp;
        }

        public override void Dispose()
        {
            Bombardment.Dispose();
            Balder.Dispose();
            ModularSystem.Cooldown.Dispose();
        }

        public override bool StartSpecialSkill(Cooldown sk)
        {
            if (Balder.Skill != null && sk.Skill.IconName == Balder.Skill.IconName)
            {
                Balder.Start(sk.Duration);
                return true;
            }
            if (Bombardment.Skill != null && sk.Skill.IconName == Bombardment.Skill.IconName)
            {
                Bombardment.Start(sk.Duration);
                return true;
            }
            if (ModularSystem.Cooldown.Skill != null && sk.Skill.IconName == ModularSystem.Cooldown.Skill.IconName)
            {
                ModularSystem.Cooldown.Start(sk.Duration);
                return true;
            }
            return false;
        }
    }
}
