public class DeckUnit
{
    private readonly UnitData data;
    public UnitData Data => data;

    public int PointScore => data.PointScore;

    public DeckUnit(UnitData data)
    {
        this.data = data;
    }
}
