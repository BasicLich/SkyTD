using System;

public abstract class EnemyWave
{
    public static EnemyWave[] AllWaves { get; } = new EnemyWave[] {
        new Wave1(), new Wave2(), new Wave3(), new Wave4(), new Wave5(), new Wave6(), new Wave7(), new Wave8(), new Wave9(), new Wave10(),
        new Wave11(), new Wave12(), new Wave13(), new Wave14(), new Wave15(), new Wave16(), new Wave17(), new Wave18(), new Wave19(), new Wave20(),
        new Wave21(), new Wave22(), new Wave23(), new Wave24(), new Wave25(), new Wave26(), new Wave27(), new Wave28(), new Wave29(), new Wave30(),
        new Wave31(), new Wave32(), new Wave33(), new Wave34(), new Wave35(), new Wave36()
    };

    public abstract Tuple<EnemyDataBase, int>[] Spawns { get; }
    public abstract int[] SpawnIndices { get; }
    public abstract WaveType WaveType { get; }
    public virtual float SpawnDelay { get; } = 1f;
}

public enum WaveType
{
    Land,
    Sea,
    Minecart
}

public class Wave1 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataSkeleton(), 5),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave2 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataSkeleton(), 8),
         new Tuple<EnemyDataBase, int>(new DataElSkello(), 1),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 1, 0, 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave3 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataSkeleton(), 20),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave4 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataSkeleton(), 8),
         new Tuple<EnemyDataBase, int>(new DataElSkello(), 8),
         new Tuple<EnemyDataBase, int>(new DataGhost(), 1),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 2, 1, 1, 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave5 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataCrab(), 3),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0 };
    public override WaveType WaveType => WaveType.Sea;
}

public class Wave6 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataSkeleton(), 10),
         new Tuple<EnemyDataBase, int>(new DataElSkello(), 10),
         new Tuple<EnemyDataBase, int>(new DataSlashetor(), 10)
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave7 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataSlashetor(), 5),
         new Tuple<EnemyDataBase, int>(new DataGhost(), 5),
         new Tuple<EnemyDataBase, int>(new DataWraith(), 1),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave8 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataCrab(), 5),
         new Tuple<EnemyDataBase, int>(new DataEel(), 2),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 1, 1 };
    public override WaveType WaveType => WaveType.Sea;
}

public class Wave9 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataGhost(), 10),
         new Tuple<EnemyDataBase, int>(new DataWraith(), 5),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave10 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataOgreSkeleton(), 4),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave11 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataOgreSkeleton(), 8),
         new Tuple<EnemyDataBase, int>(new DataNecromancer(), 2),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 1, 0, 0, 0, 0, 1, 0, 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave12 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataEel(), 15),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public override WaveType WaveType => WaveType.Sea;
}

public class Wave13 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataNecromancer(), 5),
         new Tuple<EnemyDataBase, int>(new DataGhost(), 5),
         new Tuple<EnemyDataBase, int>(new DataWraith(), 5),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave14 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataOgreSkeleton(), 15),
         new Tuple<EnemyDataBase, int>(new DataWraith(), 5),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave15 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataUndeadLord(), 1),
    };
    public override int[] SpawnIndices => new int[] { 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave16 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataSpiderman(), 10),
         new Tuple<EnemyDataBase, int>(new DataSpiderling(), 10),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave17 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataSpiderling(), 20),
         new Tuple<EnemyDataBase, int>(new DataBigSpider(), 5),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave18 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataEel(), 10),
         new Tuple<EnemyDataBase, int>(new DataCrocco(), 6),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1 };
    public override WaveType WaveType => WaveType.Sea;
}

public class Wave19 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataBigSpider(), 8),
         new Tuple<EnemyDataBase, int>(new DataSkelepider(), 4),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave20 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataSkelepider(), 15),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave21 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataCrocco(), 20),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public override WaveType WaveType => WaveType.Sea;
}

public class Wave22 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataAlpha(), 1),
    };
    public override int[] SpawnIndices => new int[] { 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave23 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataKobold(), 50),
         new Tuple<EnemyDataBase, int>(new DataGremlin(), 1),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave24 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataKobold(), 20),
         new Tuple<EnemyDataBase, int>(new DataKoboldPeasant(), 10),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave25 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataKoboldTank(), 10),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave26 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataKoboldTank(), 10),
         new Tuple<EnemyDataBase, int>(new DataBugbear(), 10),
    };
    public override int[] SpawnIndices => new int[] { 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1, 0, 1 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave27 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataBugbear(), 10),
         new Tuple<EnemyDataBase, int>(new DataKnobold(), 4),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0, 1, 0, 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave28 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataCrocco(), 10),
         new Tuple<EnemyDataBase, int>(new DataSquid(), 4),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1, 0, 1 };
    public override WaveType WaveType => WaveType.Sea;
}

public class Wave29 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataSquid(), 10),
    };
    public override int[] SpawnIndices => new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public override float SpawnDelay => 2;
    public override WaveType WaveType => WaveType.Sea;
}

public class Wave30 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataKnobold(), 20),
         new Tuple<EnemyDataBase, int>(new DataBugbear(), 10),
         new Tuple<EnemyDataBase, int>(new DataTrueKoboldTank(), 1),
    };
    public override int[] SpawnIndices => new int[] { 2 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave31 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataSiren(), 1),
    };
    public override int[] SpawnIndices => new int[] { 0 };
    public override WaveType WaveType => WaveType.Sea;
}

public class Wave32 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataTheEnd(), 1),
    };
    public override int[] SpawnIndices => new int[] { 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave33 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataTheEnd2(), 1),
    };
    public override int[] SpawnIndices => new int[] { 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave34 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataTheEnd3(), 1),
    };
    public override int[] SpawnIndices => new int[] { 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave35 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataTheEnd4(), 1),
    };
    public override int[] SpawnIndices => new int[] { 0 };
    public override WaveType WaveType => WaveType.Land;
}

public class Wave36 : EnemyWave
{
    public override Tuple<EnemyDataBase, int>[] Spawns => new Tuple<EnemyDataBase, int>[]
    {
         new Tuple<EnemyDataBase, int>(new DataTheEnd5(), 1),
    };
    public override int[] SpawnIndices => new int[] { 0 };
    public override WaveType WaveType => WaveType.Land;
}