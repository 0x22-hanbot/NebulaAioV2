﻿using EnsoulSharp;
using EnsoulSharp.SDK;
using EnsoulSharp.SDK.MenuUI;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace NebulaAio.Champions
{

    public class Ezreal
    {
        private static Spell Q, W, R;
        private static Menu Config;

        public static void OnGameLoad()
        {
            if (ObjectManager.Player.CharacterName != "Ezreal")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 1200f);
            W = new Spell(SpellSlot.W, 1200f);
            R = new Spell(SpellSlot.R, 25000f);

            Q.SetSkillshot(0.25f, 120, 2000f, true, SpellType.Line);
            W.SetSkillshot(0.25f, 160, 1700f, false, SpellType.Line);
            R.SetSkillshot(1.0f, 320f, 2000f, false, SpellType.Line);


            Config = new Menu("Ezreal", "[Nebula]: Ezreal", true);

            var menuC = new Menu("Csettings", "Combo");
            menuC.Add(new MenuBool("UseQ", "Use Q in Combo"));
            menuC.Add(new MenuBool("UseW", "Use W in Combo"));
            menuC.Add(new MenuBool("UseR", "Use R in Combo"));

            var menuL = new Menu("Clear", "Clear");
            menuL.Add(new MenuBool("LcQ", "Use Q in Lanclear"));
            menuL.Add(new MenuBool("JcQ", "Use Q in Jungleclear"));

            var menuK = new Menu("Killsteal", "Killsteal");
            menuK.Add(new MenuBool("KsQ", "Use Q to Killsteal"));
            menuK.Add(new MenuBool("KsR", "Use R to Killsteal"));
            
            var menuM = new Menu("Misc", "Misc");
            menuM.Add(new MenuKeyBind("semiR", "Semi R", Keys.T, KeyBindType.Press));
            menuM.Add(new MenuSlider("Rrange", "R Range Slider", 2500, 0, 25000));

            var menuH = new Menu("skillpred", "SkillShot HitChance ");
            menuH.Add(new MenuList("qchance", "q HitChance:", new[] { "Low", "Medium", "High", }, 2));
            menuH.Add(new MenuList("wchance", "W HitChance:", new[] { "Low", "Medium", "High", }, 2));
            menuH.Add(new MenuList("rchance", "E HitChance:", new[] { "Low", "Medium", "High", }, 2));

            var menuD = new Menu("dsettings", "Drawings ");
            menuD.Add(new MenuBool("drawQ", "Q Range  (White)", true));
            menuD.Add(new MenuBool("drawW", "W Range  (White)", true));
            menuD.Add(new MenuBool("drawE", "E Range (White)", true));
            menuD.Add(new MenuBool("drawR", "R Range  (Red)", true));



            Config.Add(menuC);
            Config.Add(menuL);
            Config.Add(menuK);
            Config.Add(menuM);
            Config.Add(menuH);
            Config.Add(menuD);

            Config.Attach();

            GameEvent.OnGameTick += OnGameUpdate;
            Drawing.OnDraw += OnDraw;
        }

        public static void OnGameUpdate(EventArgs args)
        {
            
            if (Config["Misc"].GetValue<MenuKeyBind>("semiR").Active)
            {
                SemiR();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Combo)
            {
                LogicW();
                LogicQ();
                LogicR();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.LaneClear)
            {
                Laneclear();
                Jungle();
            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.LastHit)
            {

            }

            if (Orbwalker.ActiveMode == OrbwalkerMode.Harass)
            {

            }
            Killsteal();
        }

        private static void OnDraw(EventArgs args)
        {
            if (Config["dsettings"].GetValue<MenuBool>("drawQ").Enabled)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White);
            }
            
            if (Config["dsettings"].GetValue<MenuBool>("drawW").Enabled)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, W.Range, System.Drawing.Color.White);
            }

            if (Config["dsettings"].GetValue<MenuBool>("drawR").Enabled)
            {
                Render.Circle.DrawCircle(ObjectManager.Player.Position, Config["Misc"].GetValue<MenuSlider>("Rrange").Value, System.Drawing.Color.Red);
            }
        }
        
        private static void SemiR()
        {
            var target = TargetSelector.GetTarget((1500));
            if (target == null || !target.IsValidTarget(1500)) return;
            var rPred = R.GetPrediction(target);
            if (rPred.Hitchance >= HitChance.VeryHigh) R.Cast((rPred.CastPosition));
        }

        private static void LogicR()
        {
            var target = TargetSelector.GetTarget(1000);
            var useR = Config["Csettings"].GetValue<MenuBool>("UseR");
            var input = R.GetPrediction(target);
            var rFarmSet = Config["skillpred"].GetValue<MenuList>("rchance").SelectedValue;
            if (target == null) return;
            
            string final = rFarmSet;
            var skill = HitChance.High;
            
            if (final == "0")
            {
                skill = HitChance.Low;
            }

            if (final == "1")
            {
                skill = HitChance.Medium;
            }

            if (final == "2")
            {
                skill = HitChance.High;
            }
            
            if (R.IsReady() && useR.Enabled && input.Hitchance >= skill && ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) > target.Health && target.IsValidTarget(Config["Misc"].GetValue<MenuSlider>("Rrange").Value))
            {
                R.Cast(input.UnitPosition);
            }
        }

        private static void LogicW()
        {
            var target = TargetSelector.GetTarget(W.Range);
            var useW = Config["Csettings"].GetValue<MenuBool>("UseW");
            var input = W.GetPrediction(target);
            var wFarmSet = Config["skillpred"].GetValue<MenuList>("wchance").SelectedValue;
            string final = wFarmSet;
            var skill = HitChance.High;
            if (target == null) return;

            if (final == "0")
            {
                skill = HitChance.Low;
            }

            if (final == "1")
            {
                skill = HitChance.Medium;
            }

            if (final == "2")
            {
                skill = HitChance.High;
            }

            if (W.IsReady() && useW.Enabled && input.Hitchance >= skill && target.IsValidTarget(W.Range))
            {
                W.Cast(input.UnitPosition);
            }
        }

        private static void LogicQ()
        {
            var target = TargetSelector.GetTarget(Q.Range);
            if (target == null) return;
            var useQ = Config["Csettings"].GetValue<MenuBool>("UseQ").Enabled;

            if (Q.IsReady() && useQ && target.IsValidTarget(Q.Range))
            {
                var qpred = Q.GetPrediction(target, false, 0);
                    if (qpred.Hitchance >= HitChance.High)
                    {
                        Q.Cast(qpred.UnitPosition);
                    }
            }
        }

        private static void Jungle()
        {
            var JcQq = Config["Clear"].GetValue<MenuBool>("JcQ");
            var mobs = GameObjects.Jungle.Where(x => x.IsValidTarget(Q.Range)).OrderBy(x => x.MaxHealth)
                .ToList<AIBaseClient>();
            if (mobs.Count > 0)
            {
                var mob = mobs[0];
                if (JcQq.Enabled && Q.IsReady() &&  ObjectManager.Player.Distance(mob.Position) < Q.Range) Q.Cast(mob);
            }
        }


        private static void Laneclear()
        {
            var lcq = Config["Clear"].GetValue<MenuBool>("LcQ");
            if (lcq.Enabled && Q.IsReady())
            {
                var minions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range) && x.IsMinion())
                    .Cast<AIBaseClient>().ToList();
                if (minions.Any())
                {
                    var qFarmLocation = Q.GetLineFarmLocation(minions);
                    if (qFarmLocation.Position.IsValid())
                    {
                        Q.Cast(qFarmLocation.Position);
                        return;
                    }
                }
            }
        }

        private static void Killsteal()
        {
            var ksQ = Config["Killsteal"].GetValue<MenuBool>("KsQ").Enabled;
            var ksR = Config["Killsteal"].GetValue<MenuBool>("KsR").Enabled;
            var target = TargetSelector.GetTarget(1000);

            if (target == null) return;
            if (target.IsInvulnerable) return;

            if (!(ObjectManager.Player.Distance(target.Position) <= Q.Range) ||
                !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q) >= target.Health + 20)) return;
            if (Q.IsReady() && ksQ) Q.Cast(target);
            
            if (!(ObjectManager.Player.Distance(target.Position) <= R.Range) ||
                !(ObjectManager.Player.GetSpellDamage(target, SpellSlot.R) >= target.Health + 20)) return;
            if (R.IsReady() && ksR) R.Cast(target);

        }
    }
}