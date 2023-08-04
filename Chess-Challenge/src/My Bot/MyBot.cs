using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using ChessChallenge.Application;
public class MyBot : IChessBot
{
    Piece strongPieceInDanger;

    public Move Think(Board board, Timer timer)
    {
        
        return ReturnBestMove(board);
    }

    public Move ReturnBestMove(Board board)
    {
        Move[] moves = board.GetLegalMoves();

        int currentScore = 0;
        Move currentMove = moves[new System.Random().Next(0, moves.Length)];

        strongPieceInDanger = IsPieceInDanger(board);

        for (int i = 0; i < moves.Length; i++)
        {
            int moveScore = ReturnScoreFromMove(board, moves[i]);

            if (moveScore == int.MaxValue)
            {
                return moves[i];
            }

            if (moveScore > currentScore)
            {
                currentMove = moves[i];
                currentScore = moveScore;
            }
        }
        return currentMove;
    }

    public Move[] ReturnOrderedMoves(Board board)
    {
        Move[] moves = board.GetLegalMoves();
        List<(Move,int)> orderedMoves = new List<(Move, int)>();
        for (int i = 0; i < moves.Length; i++)
        {
            orderedMoves.Add((moves[i], ReturnScoreFromMove(board, moves[i])));
        }

        orderedMoves.Sort((x, y) => (y.Item2.CompareTo(x.Item2)));
        for (int i = 0; i < moves.Length; i++)
        {
            moves[i] = orderedMoves[i].Item1;
        }

        return moves;
    }
    
    public Move Test(Board board)
    {
        Move[] moves = ReturnOrderedMoves(board);
        Move bestMove = moves[0];
        int bestTotalMoveScore = 0;
        for (int i = 0; i < moves.Length; i++)
        {
            Move currentMove = moves[i];
            int scoreAttack = ReturnScoreFromMove(board, currentMove);
            board.MakeMove(currentMove);
            Move bestDefendMove = ReturnBestMove(board);
            int scoreDefend = ReturnScoreFromMove(board, bestDefendMove);
            board.UndoMove(currentMove);
            int totalScore = scoreAttack - scoreDefend;

            if(totalScore > bestTotalMoveScore)
            {
                bestMove = currentMove;
                bestTotalMoveScore = totalScore;
            }
        }

        return bestMove;
    }

    private int ReturnScoreFromMove(Board board, Move move)
    {
        int score = 0;

        if(DoesMoveCheckmate(board, move))
        {
            return int.MaxValue;
        }

        if(move.StartSquare == strongPieceInDanger.Square)
        {
            score += PieceScore(move.MovePieceType);
        }

        if(move.IsEnPassant)
        {
            return int.MaxValue;
        }

        if(move.IsCapture)
        {
            score += PieceScore(move.CapturePieceType);
        }

        if(move.IsPromotion)
        {
            score += PieceScore(move.PromotionPieceType);
        }

        if(move.IsCastles)
        {
            score += 100;
        }
        
        if(board.SquareIsAttackedByOpponent(move.TargetSquare))
        {
            score -= PieceScore(move.MovePieceType);    
        }

        if(DoesMoveCheck(board, move))
        {
            score += 20;
        }

        score += ReturnSeenSquare(board, move);

        return score;
    }

    private int[] pieceScores = new int[7] { 0, 10, 30, 30, 50, 90, 100 };

    private int PieceScore(PieceType pieceType)
    {
        return pieceScores[(int)pieceType];
    }

    private bool DoesMoveCheck(Board board, Move move)
    {
        PieceType piece = move.MovePieceType;
        Square square = move.TargetSquare;
        Square kingSquare = board.GetKingSquare(!board.IsWhiteToMove);
        return BitboardHelper.SquareIsSet(ReturnAttackBitBoard(board, square, piece), kingSquare);
    }

    private ulong ReturnAttackBitBoard(Board board, Square square, PieceType piece)
    {
        switch(piece)
        {
            case PieceType.Pawn:
                return BitboardHelper.GetPawnAttacks(square, board.IsWhiteToMove);
            case PieceType.Knight:
                return BitboardHelper.GetKnightAttacks(square);
            case PieceType.King:
                return BitboardHelper.GetKingAttacks(square);
            default:
                return BitboardHelper.GetSliderAttacks(piece, square, board);
        }
    }

    private Piece IsPieceTypeInDanger(Board board, PieceList pieceList)
    {
        Piece mostValuablePiece = new Piece(PieceType.None, true, new Square());
        for (int i = 0; i < pieceList.Count; i++)
        {
            if (board.SquareIsAttackedByOpponent(pieceList.GetPiece(i).Square))
            {
                if (PieceScore(pieceList.GetPiece(i).PieceType) > PieceScore(mostValuablePiece.PieceType))
                {
                    mostValuablePiece =  pieceList.GetPiece(i);
                }
            }
        }

        return mostValuablePiece;
    }

    private Piece IsPieceInDanger(Board board)
    {
        Piece mostValuablePiece = new Piece(PieceType.None, true, new Square());

        for (int i = 1; i < 7; i++)
        {
            PieceList pieceList = board.GetPieceList((PieceType)i, board.IsWhiteToMove);
            Piece bestPiece = IsPieceTypeInDanger(board, pieceList);
            if (PieceScore(bestPiece.PieceType) > PieceScore(mostValuablePiece.PieceType))
            {
                mostValuablePiece = bestPiece;
            }
        }

        return mostValuablePiece;
    }

    private bool DoesMoveCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isInCheckmate = board.IsInCheckmate();
        board.UndoMove(move);
        return isInCheckmate;
    }

    private int ReturnSeenSquare(Board board, Move move)
    {
        int currentSquaresSeen = BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(move.MovePieceType, move.StartSquare, board));
        int targetSquaresSeen = BitboardHelper.GetNumberOfSetBits(BitboardHelper.GetSliderAttacks(move.MovePieceType, move.TargetSquare, board));
        
        return (targetSquaresSeen - currentSquaresSeen);
    }
}