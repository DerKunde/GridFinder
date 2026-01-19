namespace GridFinder.Spawner
{
    public interface ISpawnCommandFactory
    {
        bool TryCreate(in SpawnIntent intent, out SpawnCommandData cmd);
    }
}