﻿using System.Collections.Generic;
using UnityEngine;

public class King : Piece {

    public King()
    {

    }

    public King(int all, Point p, Board b, PieceTypeE t) : base(all, p, b, t)
    {

    }

    public override List<Vector3> canMoveEval()
    {
        List<Vector3> scores = new List<Vector3>();
        List<Point> pts = canMoveList();
        
        foreach (Point point in pts)
        {
            int basenum = pts.Count * (int)ScoreWeightsE.MOBILITY + (int)PieceWeightsE.KINGWEIGHT;
            basenum = basenum + ((getAllegiance() == 0) ? (7 - point.getY()) : (point.getY())) * (int)ScoreWeightsE.KINGX + System.Math.Abs(point.getY() - 4)*(int)ScoreWeightsE.KINGY;
            if (gameBoard.pieceAt(point) != null)
            {
                switch ((((Piece)gameBoard.pieceAt(point).GetComponent("Piece")).getType()))
                {
                    case (PieceTypeE.PAWN):
                        basenum = basenum + (int)PieceWeightsE.PAWNCAPUTURE;
                        break;
                    case (PieceTypeE.BISHOP):
                        basenum = basenum + (int)PieceWeightsE.BISHOPCAPUTURE;
                        break;
                    case (PieceTypeE.KNIGHT):
                        basenum = basenum + (int)PieceWeightsE.KNIGHTCAPUTURE;
                        break;
                    case (PieceTypeE.ROOK):
                        basenum = basenum + (int)PieceWeightsE.ROOKCAPUTURE;
                        break;
                    case (PieceTypeE.QUEEN):
                        basenum = basenum + (int)PieceWeightsE.QUEENCAPUTURE;
                        break;
                    case (PieceTypeE.KING):
                        basenum = basenum + (int)PieceWeightsE.KINGCAPUTURE;
                        break;
                    default:
                        break;
                }
            }
            if(System.Math.Abs(loc.getY() - point.getY()) == 2)
            {
                basenum = basenum + (int)ScoreWeightsE.CASTLE;
            }
            Debug.Log("King at (" + loc.getX() + ", " + loc.getY() + ") can move to (" + point.getX() + ", " + point.getY() + ")  with weight " + basenum);
            scores.Add(new Vector3(point.getX(), point.getY(), basenum));
        }
        return scores;
    }

    //Calculates all spaces in box around piece to see if legal
    public override List<Point> canMoveList()
    {
        List<Point> retMoveList = new List<Point>();
        for (int i = -1; i <= 1; i++)
            for (int j = -1; j <= 1; j++)
            {
                Point pt = new Point(loc.getX() + i, loc.getY() + j);
                if (canMove(pt) != MoveTypesE.ILLEGAL)
                    retMoveList.Add(pt);
            }
        if (!hasMoved)
        {
            Point pt = new Point(loc.getX(), loc.getY() + 2);
            if (canMove(pt) != MoveTypesE.ILLEGAL)
                retMoveList.Add(pt);
            Point pt1 = new Point(loc.getX(), loc.getY() - 2);
            if (canMove(pt1) != MoveTypesE.ILLEGAL)
                retMoveList.Add(pt1);
        }

        /*Debug.Log("King at (" + loc.getX() + ", " + loc.getY() + ") can move to: ");
        foreach (Point p in retMoveList)
        {
            Debug.Log("(" + p.getX() + ", " + p.getY() + ")");
        }
        if (retMoveList.Count == 0)
            Debug.Log("No Possible Moves");*/

        return retMoveList;
    }

    /// <summary>
    /// (a) Except when castling, the king moves to any adjoining square that is not attacked by an opponent's piece.
    /// (b) Castling is a move of the king and either rook, counting as a single move of the king and executed as follows: the king is transferred from 
    /// its original square two squares toward either rook on the same rank; then that rook is transferred over the king to the square the king has just crossed.
    ///  (e) Castling is [permanently] illegal:
    ///    (i) if the king has already been moved; or
    ///    (ii) with a rook that has already been moved.
    ///  (f) Castling is prevented for the time being:
    ///    (i) if the king's original square, or the square which the king must pass over, or that which it is to occupy, is attacked by an opponent's piece; or
    ///    (ii) if there is any piece between the king and the rook with which castling is to be effected[i.e.castling may still be legal even if the rook is attacked or, 
    ///          when castling queenside, passes over an attacked square] .
    /// </summary>
    /// <param name="p">The point the pawn is trying to move to</param>
    /// <returns>The move type for the piece trying to make that move</returns>
    override public MoveTypesE canMove(Point p)
    {
        MoveTypesE mt = base.canMove(p);
        if (mt == MoveTypesE.ILLEGAL)
            return MoveTypesE.ILLEGAL;
        int dy = p.getY() - loc.getY();
        int dx = p.getX() - loc.getX();

        if ((System.Math.Abs(dx) <= 1 && System.Math.Abs(dy) <= 1))
            return mt;

        if(!hasMoved && System.Math.Abs(dy) == 2 && dx == 0)
        {
            //Debug.Log("Seeing if can castle");
            if (gameBoard.inCheck(loc))
                return MoveTypesE.ILLEGAL;
            if(dy > 0)
            {
                for(int i = loc.getY()+1; i < 7; i++)
                {
                    if (gameBoard.pieceAt(loc.getX(), i) != null)
                    {
                        //Debug.Log("Piece in the way");
                        return MoveTypesE.ILLEGAL;
                    }
                }
                GameObject rookMaybe = gameBoard.pieceAt(loc.getX(), 7);
                if (rookMaybe == null || ((Piece)rookMaybe.GetComponent("Piece")).getHasMoved())
                {
                    //Debug.Log("Rook Has Moved");
                    return MoveTypesE.ILLEGAL;
                }
                if (gameBoard.inCheck(gameObject, rookMaybe, loc.getX(), loc.getY() + 1) || gameBoard.inCheck(gameObject, rookMaybe, loc.getX(), loc.getY() + 2))
                {
                    //Debug.Log("In Check On Way");
                    return MoveTypesE.ILLEGAL;
                }
            }
            else
            {
                for (int i = loc.getY()-1; i > 0; i--)
                {
                    if (gameBoard.pieceAt(loc.getX(), i) != null)
                    {
                        return MoveTypesE.ILLEGAL;
                    }
                }
                GameObject rookMaybe = gameBoard.pieceAt(loc.getX(), 0);
                if (rookMaybe == null || ((Piece)rookMaybe.GetComponent("Piece")).getHasMoved())
                {
                   // Debug.Log("Rook Has Moved");
                    return MoveTypesE.ILLEGAL;
                }
                if (gameBoard.inCheck(gameObject, rookMaybe, loc.getX(), loc.getY() - 1) || gameBoard.inCheck(gameObject, rookMaybe, loc.getX(), loc.getY() - 2))
                {
                   // Debug.Log("In Check On Way");
                    return MoveTypesE.ILLEGAL;
                }
            }
            return MoveTypesE.CASTLE;
        }

        return MoveTypesE.ILLEGAL;
    }

    /// <summary>
    /// Similar tryToMove as the default, but if castling, moves the correct king to the correct position as well.
    /// </summary>
    /// <param name="p"></param>
    public override void tryToMove(Point p)
    {
        MoveTypesE mt = canMove(p);
        if (mt != MoveTypesE.ILLEGAL)
        {
            if (mt == MoveTypesE.CASTLE)
            {
                Point tmploc = loc;
                if (loc.getY() > p.getY())
                {
                    gameBoard.Move(new Point(p.getX(), 0), new Point(p.getX(), p.getY()+1));
                }
                else
                {
                    gameBoard.Move(new Point(p.getX(), 7), new Point(p.getX(), p.getY()-1));
                }
                gameBoard.Move(loc, p);
                gameBoard.switchTurn();
            }
            else
            {
                gameBoard.Move(loc, p);
            }
            hasMoved = true;
        }
    }

	public override void moveObjectLoc(Point pt)
	{
		loc = pt;
		gameObject.transform.position = new Vector3(pt.turnToWorld()[0], .45f, pt.turnToWorld()[1] + .35f);
	}

}
