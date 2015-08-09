﻿/*File for this file Basic Copyright 2012 no0dl */
using System;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
using System.Collections.Generic;
using Zepheus.FiestaLib.Data;


namespace Fiesta.Zone.Game.Buffs
{
    /*public sealed class MapObjectBuffCollection : IEnumerable<Buff>
    {
        public int Count
        {
            get
            {
                lock (MapObject.ThreadLocker)
                {
                    return List.Count;
                }
            }
        }



        private LivingObject MapObject;
        private List<Buff> List;

        public MapObjectBuffCollection(LivingObject MapObject)
        {
            this.MapObject = MapObject;
            List = new List<Buff>();

            MapObject.OnUpdate += On_MapObject_Update;
        }
        public void Dispose()
        {
            MapObject = null;

            List.ForEach(b => b.Dispose());
            List.Clear();
            List = null;
        }

        private void On_MapObject_Update(MapObject Object, DateTime ServerTime)
        {
            lock (MapObject.ThreadLocker)
            {
                var toRemove = new List<Buff>();

                foreach (var buff in List)
                {
                    if (buff.ExpireTime < ServerTime)
                    {
                        toRemove.Add(buff);
                    }
                }

                foreach (var buff in toRemove)
                {
                    Remove(buff);
                }

                toRemove.Clear();
                toRemove = null;
            }
        }


        IEnumerator IEnumerable.GetEnumerator()
        {
            List<Buff> listCopy;

            lock (MapObject.ThreadLocker)
            {
                listCopy = List.Copy();
            }

            return new MapObjectBuffCollectionEnumerator(listCopy);
        }
        public IEnumerator<Buff> GetEnumerator()
        {
            List<Buff> listCopy;

            lock (MapObject.ThreadLocker)
            {
                listCopy = List.Copy();
            }

            return new MapObjectBuffCollectionTypeEnumerator(listCopy);
        }


        public void LoadFromDatabase(SqlConnection con)
        {
            var ch = (MapObject as Character);

            if (ch != null)
            {
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM Buffs WHERE CharacterID = @pCharacterID";

                    cmd.Parameters.Add(new SqlParameter("@pCharacterID", ch.ID));


                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var abStateID = (ushort)reader.GetInt16(2);
                            AbStateInfo abStateInfo;
                            if (!BuffDataProvider.GetAbStateInfo(a => a.ID.Equals(abStateID), out abStateInfo))
                                throw new InvalidOperationException("Unable to load buff from database. Can't find AbState with ID: " + abStateID);


                            var buff = new Buff(MapObject, abStateInfo, (uint)reader.GetInt32(3))
                                {
                                    ID = reader.GetInt64(0),

                                    StartTime = reader.GetDateTime(4),
                                    ExpireTime = reader.GetDateTime(5),
                                };

                            buff.Activate();

                            List.Add(buff);
                        }
                    }
                }
            }
        }
        public void Save(SqlConnection con)
        {
            var ch = (MapObject as Character);

            if (ch != null)
            {
                lock (MapObject.ThreadLocker)
                {
                    foreach (var buff in List)
                    {
                        if (buff.AbStateInfo.IsSave)
                        {
                            using (var cmd = con.CreateCommand())
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = "dbo.Buff_Save";

                                cmd.Parameters.Add(new SqlParameter("@pID", buff.ID));

                                cmd.Parameters.Add(new SqlParameter("@pStrength", (int)buff.Strength));
                                cmd.Parameters.Add(new SqlParameter("@pStartTime", buff.StartTime));
                                cmd.Parameters.Add(new SqlParameter("@pEndTime", buff.ExpireTime));




                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }



        public bool GetBuff(Predicate<Buff> Match, out Buff Buff)
        {
            lock (MapObject.ThreadLocker)
            {
                Buff = List.Find(Match);
            }

            return (Buff != null);
        }



        public void Add(AbStateInfo AbState, uint Strength, uint? KeepTime = null)
        {
            lock (MapObject.ThreadLocker)
            {
                //get time
                var time = ZoneService.Instance.Time;

                KeepTime = (KeepTime ?? (uint)AbState.SubAbStates[Strength].KeepTime.TotalMilliseconds);


                //check if we already own that buff
                var buff = List.Find(b => b.AbStateInfo.ID.Equals(AbState.ID));
                if (buff != null)
                {
                    //check if new state is stronger
                    if (Strength > buff.Strength)
                    {
                        //deactivate buff
                        buff.Deactivate();

                        //update sub abstate of buff
                        buff.UpdateSubAbState(Strength);

                        //reactivate buff
                        buff.Activate();
                    }
                    else if (Strength < buff.Strength)
                    {
                        //we dont update our buff if it is the same with a higher strength
                        return;
                    }


                    //update start + expire time if the new buff is the same or stronger like our buff
                    buff.StartTime = time;
                    buff.ExpireTime = time.AddMilliseconds(KeepTime.Value);
                }
                else
                {
                    //create new buff
                    buff = new Buff(MapObject, AbState, Strength)
                        {
                            StartTime = time,
                        };
                    buff.ExpireTime = time.AddMilliseconds(KeepTime.Value);


                    //add to list
                    List.Add(buff);


                    //activate buff
                    buff.Activate();


                    if (AbState.IsSave
                        && MapObject is Character)
                    {
                        //add to db and assign id to buff
                        using (var con = DatabaseManager.DB_Game.GetConnection())
                        {
                            using (var cmd = con.CreateCommand())
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = "dbo.Buff_Create";

                                cmd.Parameters.Add(new SqlParameter("@pCharacterID", (MapObject as Character).ID));
                                cmd.Parameters.Add(new SqlParameter("@pAbStateID", (short)AbState.ID));
                                cmd.Parameters.Add(new SqlParameter("@pStrength", (int)Strength));
                                cmd.Parameters.Add(new SqlParameter("@pStartTime", buff.StartTime));
                                cmd.Parameters.Add(new SqlParameter("@pEndTime", buff.ExpireTime));
                                var outID = cmd.Parameters.Add(new SqlParameter("@pID", SqlDbType.BigInt)
                                    {
                                        Direction = ParameterDirection.Output,
                                    });


                                cmd.ExecuteNonQuery();



                                buff.ID = (long)outID.Value;
                            }
                        }
                    }
                }

                
                //update to client if its a character
                if (MapObject is Character)
                {
                    using (var packet = new GamePacket(GameOpCode.Server.H9.SetBuffSelf))
                    {
                        packet.WriteUInt32(AbState.AbStateIndex);                                   // abstata index
                        //packet.WriteUInt32((uint)buff.SubAbStateInfo.KeepTime.TotalMilliseconds);   // keeptime in ms
                        packet.WriteUInt32((uint)(buff.ExpireTime - time).TotalMilliseconds);   // keeptime in ms



                        (MapObject as Character).Client.Send(packet);
                    }
                }

                using (var packet = new GamePacket(GameOpCode.Server.H9.SetBuffObject))
                {
                    packet.WriteUInt16(MapObject.ObjectID);
                    packet.WriteUInt32(AbState.AbStateIndex);                                   // abstata index
                    packet.WriteUInt32((uint)buff.SubAbStateInfo.KeepTime.TotalMilliseconds);   // keeptime in ms


                    MapObject.Broadcast(packet, false);
                }
            }
        }
        public void Remove(Buff Buff)
        {
            lock (MapObject.ThreadLocker)
            {
                //remove from list
                List.Remove(Buff);

                //deactivate buff
                Buff.Deactivate();

                if (Buff.AbStateInfo.IsSave
                    && MapObject is Character)
                {
                    //remove from db
                    using (var con = DatabaseManager.DB_Game.GetConnection())
                    {
                        using (var cmd = con.CreateCommand())
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "dbo.Buff_Remove";


                            cmd.Parameters.Add(new SqlParameter("@pID", Buff.ID));


                            
                            cmd.ExecuteNonQuery();
                        }
                    }
                }



                using (var packet = new GamePacket(GameOpCode.Server.H9.RemoveBuff))
                {
                    packet.WriteUInt16(MapObject.ObjectID);
                    packet.WriteUInt32(Buff.AbStateInfo.AbStateIndex);



                    MapObject.Broadcast(packet, true);
                }


                //clean up
                Buff.Dispose();
                Buff = null;
            }
        }
    }*/
}