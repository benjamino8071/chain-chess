using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class GameData_SO : ScriptableObject
{
    public int numOfRoomsToBeat;
    public int finalLevel; //This starts at 0

    public int roomsEntered;
    public int piecesCaptured;
    public int bestTurn; //Stores highest number of pieces captured in a single turn
    public int chainCompletes; //When the player removes all pieces in room without it being the enemy's turn

    private List<PieceUsedAmount> _pieceUsedAmounts = new();
    
    public int seedUsed;

    public Piece capturedByPiece;
    
    public void IncrementPieceUsed(Piece piece)
    {
        for (int i = 0; i < _pieceUsedAmounts.Count; i++)
        {
            if (_pieceUsedAmounts[i].Piece == piece)
            {
                _pieceUsedAmounts[i].Amount++;
                break;
            }
        }
    }

    public Piece GetMostUsedPiece()
    {
        Piece mostUsedPiece = Piece.Queen; //We know the queen will be used at least once as it is the starting piece
        int amountUsed = 1;

        foreach (PieceUsedAmount pieceUsedAmount in _pieceUsedAmounts)
        {
            if (pieceUsedAmount.Amount > amountUsed)
            {
                mostUsedPiece = pieceUsedAmount.Piece;
                amountUsed = pieceUsedAmount.Amount;
            }
        }
        
        return mostUsedPiece;
    }
    
    public void ResetData()
    {
        roomsEntered = 0;
        piecesCaptured = 0;
        bestTurn = 0;
        chainCompletes = 0;

        List<Piece> pieces = new()
        {
            Piece.Bishop,
            Piece.King,
            Piece.Knight,
            Piece.Pawn,
            Piece.Queen,
            Piece.Rook
        };

        _pieceUsedAmounts = new();
        foreach (Piece piece in pieces)
        {
            _pieceUsedAmounts.Add(new PieceUsedAmount()
            {
                Piece = piece,
                Amount = 0
            });
        }
        
        seedUsed = 0;
    }
    
    private class PieceUsedAmount
    {
        public Piece Piece;
        public int Amount;
    }
}
