﻿/*File for this file Basic Copyright 2012 no0dl */
using System;
using System.Collections.Generic;
using Zepheus.FiestaLib.Data;
using System.Collections;

namespace Zepheus.Zone.Game.Buffs
{
    public class Buff
    {
       /* /// <summary>
        /// The ID of this buff. Only used if its saved in database.
        /// </summary>
        public long ID { get; set; }


        /// <summary>
        /// The object to which this buff belongs.
        /// </summary>
        public LivingObject MapObject { get; private set; }

        /// <summary>
        /// The abstate info of this buff.
        /// </summary>
        public AbStateInfo AbStateInfo { get; private set; }
        /// <summary>
        /// The strength of this buff. Used for getting sub abstate infos.
        /// </summary>
        public uint Strength { get; private set; }
        /// <summary>
        /// The sub abstate info of this buff.
        /// </summary>
        public SubAbstateInfo SubAbStateInfo { get; private set; }

        /// <summary>
        /// A list with all actions of the sub abstate.
        /// </summary>
        public ReadOnlyCollection<BuffAction> Actions { get { return ActionList.AsReadOnly(); } }
        private List<BuffAction> ActionList;


        public DateTime StartTime { get; set; }
        public DateTime ExpireTime { get; set; }


        public bool IsDisposed { get { return IsDisposedInt > 0; } }
        private int IsDisposedInt;




        private bool HaveStatsChangerActions;



        public Buff(LivingObject MapObject, AbStateInfo AbStateInfo, uint Strength)
        {
            this.MapObject = MapObject;
            this.AbStateInfo = AbStateInfo;
            this.Strength = Strength;


            ActionList = new List<BuffAction>();

            UpdateSubAbState(Strength);
        }
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref IsDisposedInt, 1, 0) == 0)
            {
                MapObject = null;
                AbStateInfo = null;
                SubAbStateInfo = null;


                ActionList.ForEach(a => a.Dispose());
                ActionList.Clear();
                ActionList = null;
            }
        }
        ~Buff()
        {
            Dispose();
        }





        public void Activate()
        {
            lock (ActionList)
            {
                foreach (var action in ActionList)
                {
                    action.Activate();
                }

                if (HaveStatsChangerActions)
                    MapObject.Stats.Update();
            }
        }
        public void Deactivate()
        {
            lock (ActionList)
            {
                foreach (var action in ActionList)
                {
                    action.Deactivate();
                }

                if (HaveStatsChangerActions)
                    MapObject.Stats.Update();
            }
        }





        public void UpdateSubAbState(uint Strength)
        {
            SubAbstateInfo subState;
            if (!AbStateInfo.SubAbStates.TryGetValue(Strength, out subState))
                throw new InvalidOperationException(String.Format("Can't find sub abstate for abstate '{0}' with strength '{1}'", AbStateInfo.InxName, Strength));

            SubAbStateInfo = subState;
            this.Strength = Strength;


            //refresh actions
            lock (ActionList)
            {
                ActionList.ForEach(a => a.Dispose());
                ActionList.Clear();

                HaveStatsChangerActions = false;

                foreach (var action in SubAbStateInfo.Actions)
                {
                    if (action.Type == SubAbStateActionType.None)
                        continue;


                    BuffAction buffAction;
                    if (!CreateBuffAction(this, action, out buffAction))
                    {
                        Log.Write(LogType.Debug, "Unsupported buff action type: " + action.Type);
                        continue;
                    }

                    if (buffAction is StatsAction)
                    {
                        HaveStatsChangerActions = true;
                    }


                    ActionList.Add(buffAction);
                }
            }
        }
        private static bool CreateBuffAction(Buff Buff, SubAbStateAction Action, out BuffAction BuffAction)
        {
            switch (Action.Type)
            {
                case SubAbStateActionType.DamageIncrease:
                case SubAbStateActionType.DamageIncrease2:
                case SubAbStateActionType.DamageDefenseIncrease:
                case SubAbStateActionType.DefenseIncrease:
                case SubAbStateActionType.DefenseIncrease2:
                case SubAbStateActionType.ChangeDex:
                case SubAbStateActionType.ChangeAim:
                case SubAbStateActionType.ChangeEvasion:
                case SubAbStateActionType.MagicDamageDefenseIncrease:
                case SubAbStateActionType.MagicDefenseDecrease:
                case SubAbStateActionType.MagicDefenseIncrease:
                case SubAbStateActionType.IncreaseShieldBlockRate:
                case SubAbStateActionType.SpeedIncrease:
                case SubAbStateActionType.DecreaseAtkSpeed:
                case SubAbStateActionType.IncreaseHP:
                case SubAbStateActionType.IncreaseHP2:
                case SubAbStateActionType.IncreaseSP:
                case SubAbStateActionType.IncreaseSP2:
                case SubAbStateActionType.IncreasePoisionResistance:
                case SubAbStateActionType.IncreaseDebuffResistance:
                case SubAbStateActionType.IncreaseCurseResistance:
                case SubAbStateActionType.IncreaseCrit:
                case SubAbStateActionType.IncreaseInt:
                case SubAbStateActionType.IncreaseAllStats:
                    BuffAction = new StatsAction(Buff, Action);
                    break;


                case SubAbStateActionType.None:
                default:
                    BuffAction = null;
                    break;
            }


            return (BuffAction != null);
        }*/
    }
}