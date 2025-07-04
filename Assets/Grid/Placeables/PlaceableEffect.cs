using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlaceableEffect : MonoBehaviour
{
    public abstract void OnPlace(Placeable placeable);

    public abstract void OnRemove(Placeable placeable);

    public abstract void OnEndOfTurn(Placeable placeable);

    public abstract void OnStartOfTurn(Placeable placeable);

    public abstract void OnEndOfRound(Placeable placeable);

    public abstract void OnStartOfRound(Placeable placeable);
}
