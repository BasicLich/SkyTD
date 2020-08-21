using Godot;
using System;
using System.Collections.Generic;

public static class PersistentDataStorage
{
    public static System.Collections.Generic.Dictionary<Guid, EnemyDataBase> PersistentEnemyDatas { get; } = new System.Collections.Generic.Dictionary<Guid, EnemyDataBase>()
    {
        [DataSkeleton.GID] = new DataSkeleton(),
        [DataElSkello.GID] = new DataElSkello(),
        [DataSlashetor.GID] = new DataSlashetor(),
        [DataNecromancer.GID] = new DataNecromancer(),
        [DataOgreSkeleton.GID] = new DataOgreSkeleton(),
        [DataGhost.GID] = new DataGhost(),
        [DataWraith.GID] = new DataWraith(),
        [DataUndeadLord.GID] = new DataUndeadLord(),
        [DataSpiderman.GID] = new DataSpiderman(),
        [DataSpiderling.GID] = new DataSpiderling(),
        [DataBigSpider.GID] = new DataBigSpider(),
        [DataSkelepider.GID] = new DataSkelepider(),
        [DataAlpha.GID] = new DataAlpha(),
        [DataKobold.GID] = new DataKobold(),
        [DataGremlin.GID] = new DataGremlin(),
        [DataKoboldPeasant.GID] = new DataKoboldPeasant(),
        [DataKoboldTank.GID] = new DataKoboldTank(),
        [DataBugbear.GID] = new DataBugbear(),
        [DataKnobold.GID] = new DataKnobold(),
        [DataTrueKoboldTank.GID] = new DataTrueKoboldTank(),
        [DataCrab.GID] = new DataCrab(),
        [DataEel.GID] = new DataEel(),
        [DataCrocco.GID] = new DataCrocco(),
        [DataSquid.GID] = new DataSquid(),
        [DataSiren.GID] = new DataSiren(),
        [DataTheEnd.GID] = new DataTheEnd(),
        [DataTheEnd2.GID] = new DataTheEnd2(),
        [DataTheEnd3.GID] = new DataTheEnd3(),
        [DataTheEnd4.GID] = new DataTheEnd4(),
        [DataTheEnd5.GID] = new DataTheEnd5(),
    };

    public static void Save()
    {
        File savegame = new File();
        savegame.Open("user://skyjamtd.bin", File.ModeFlags.Write);
        savegame.Store16((ushort)PersistentEnemyDatas.Count);
        foreach (KeyValuePair<Guid, EnemyDataBase> kv in PersistentEnemyDatas)
        {
            savegame.StoreBuffer(kv.Key.ToByteArray());
            savegame.Store8(kv.Value.IsUnlocked ? (byte)1 : (byte)0);
        }

        savegame.Close();
    }

    public static void Load()
    {
        File savegame = new File();
        if (savegame.FileExists("user://skyjamtd.bin"))
        {
            savegame.Open("user://skyjamtd.bin", File.ModeFlags.Read);
            ushort count = savegame.Get16();
            for (int i = 0; i < count; ++i)
            {
                byte[] buf = savegame.GetBuffer(16);
                Guid g = new Guid(buf);
                PersistentEnemyDatas[g].IsUnlocked = savegame.Get8() == 1;
            }
        }
    }
}
