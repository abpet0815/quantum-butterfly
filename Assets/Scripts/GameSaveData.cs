using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class GameSaveData
{
    [Header("Game State")]
    public Vector2Int gridSize;
    public int matchedPairs;
    public int totalPairs;
    public bool gameInProgress;
    
    [Header("Cards Data")]
    public List<CardSaveData> cards = new List<CardSaveData>();
    
    [Header("Score Data")]
    public int currentScore;
    public int totalMoves;
    public int currentCombo;
    public bool isPerfectGame;
    public float gameTime;
    
    [Header("Settings")]
    public bool useResponsiveLayout;
    public bool autoScaleCards;
    
    [Header("Meta")]
    public string saveDateTime;
    public string gameVersion;
    
    public GameSaveData()
    {
        cards = new List<CardSaveData>();
        saveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        gameVersion = Application.version;
    }
}

[Serializable]
public class CardSaveData
{
    public int cardValue;
    public float posX, posY, posZ;
    public bool isFlipped;
    public bool isMatched;
    public int gridX, gridY;
    
    public CardSaveData() { }
    
    public CardSaveData(Card card, Vector2Int gridPos)
    {
        cardValue = card.CardValue;
        Vector3 pos = card.transform.position;
        posX = pos.x;
        posY = pos.y;
        posZ = pos.z;
        isFlipped = card.IsFlipped;
        isMatched = card.IsMatched;
        gridX = gridPos.x;
        gridY = gridPos.y;
    }
    
    public Vector3 GetPosition()
    {
        return new Vector3(posX, posY, posZ);
    }
    
    public Vector2Int GetGridPosition()
    {
        return new Vector2Int(gridX, gridY);
    }
}
