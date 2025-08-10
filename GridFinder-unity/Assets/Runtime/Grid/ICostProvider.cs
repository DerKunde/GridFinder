namespace Runtime.Grid
{
    public interface ICostProvider
    {
        //Rückgabe: byte.MaxValue = unpassierbar, sonst Kosten >= 1
        byte GetCost(int x, int y);
    }
}