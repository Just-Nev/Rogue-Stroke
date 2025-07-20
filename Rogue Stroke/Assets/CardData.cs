using UnityEngine;

public enum CardRarity { Common, Rare, Epic }

public class CardData : MonoBehaviour
{
    public int cardID;
    public CardRarity rarity;
}
