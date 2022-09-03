using Verse;

namespace ResourceGenerator;

public class CompProperties_ResourceSpawner : CompProperties
{
    public bool inheritFaction;

    public bool requiresPower;

    public string saveKeysPrefix;

    public bool showMessageIfOwned;

    public bool spawnForbidden;

    public IntRange spawnIntervalRange = new IntRange(100, 100);

    public int spawnMaxAdjacent = -1;

    public bool writeTimeLeftToSpawn;

    public CompProperties_ResourceSpawner()
    {
        compClass = typeof(CompResourceSpawner);
    }
}