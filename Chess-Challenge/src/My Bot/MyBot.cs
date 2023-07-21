using ChessChallenge.API;

public class MyBot : IChessBot
{
    Piece strongPieceInDanger;

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        int currentScore = 0;
        Move currentMove = moves[new System.Random().Next(0, moves.Length)];
        
        strongPieceInDanger = IsPieceInDanger(board);

        for (int i = 0; i < moves.Length; i++)
        {
            int moveScore = ReturnScoreFromMove(board, moves[i]);
            if(moveScore > currentScore)
            {
                currentMove = moves[i];
                currentScore = moveScore;
            }
        }
        return currentMove;
    }
    
    private int ReturnScoreFromMove(Board board, Move move)
    {
        int score = 0;

        if(move.StartSquare == strongPieceInDanger.Square)
        {
            score += 100 * PieceScore(move.MovePieceType);
        }

        if(move.IsCapture)
        {
            score += PieceScore(move.CapturePieceType);
        }

        if(move.IsPromotion)
        {
            score += PieceScore(move.PromotionPieceType);
        }
        
        if(board.SquareIsAttackedByOpponent(move.TargetSquare))
        {
            score -= PieceScore(move.MovePieceType);    
        }

        if(DoesMoveCheck(board, move))
        {
            score += 20;
        }

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
}