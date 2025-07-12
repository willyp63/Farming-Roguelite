public class DeckTile
{
    private readonly SeasonType season;
    public SeasonType Season => season;

    private DeckUnit unit = null;
    public DeckUnit Unit => unit;

    public DeckTile(SeasonType seasonType)
    {
        season = seasonType;
    }

    public void ClearUnit()
    {
        unit = null;
    }

    public void SetUnit(UnitData unitData)
    {
        unit = new DeckUnit(unitData);
    }
}
