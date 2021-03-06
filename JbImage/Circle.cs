﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JbImage
{
    public class Line
    {
        public int Start;
        public int End;
        public int Y;
        public int Length
        {
            get
            {
                return (End - Start + 1);
            }
        }
        public Line(int start)
        {
            Start = start;
            End = start;
        }
        public Line(int start, int end)
        {
            Start = start;
            End = end;
        }
        public bool Equals(Line l)
        {
            return (Start == l.Start && End == l.End);
        }
        public bool LongerThan(Line l)
        {
            return Length > l.Length;
        }
        public bool AdjacentTo(Line line)
        {
            return !((End < line.Start) || (Start > line.End));
        }
        public void Merge(Line line)
        {
            End = line.End;
        }
        public bool AddTo(Round round)
        {
            if (round.Bottom == null || (!round.IsLineAdded && AdjacentTo(round.Bottom))
                || (round.IsLineAdded && (round.Lines.Count - 2) >= 0 && AdjacentTo(round.Lines[round.Lines.Count - 2])))
            {
                round.Add(this);
                return true;
            }

            return false;
        }

        public static List<Line> FindLines(byte[] binArray, int rowNo = 0)
        {
            List<Line> lines = new List<Line>();

            Line newLine = null;
            for (int i = 0; i < binArray.Length; i++)
            {
                if (binArray[i] > 0)/* white */
                {
                    if (newLine == null)
                    {
                        newLine = new Line(i);
                        newLine.Y = rowNo;
                    }
                    else
                    {
                        newLine.End = i;
                    }
                }
                else
                {
                    if (newLine != null)
                    {
                        newLine.End = i - 1;
                        lines.Add(newLine);
                        newLine = null;
                    }
                }

            }

            if (newLine != null)
            {
                newLine.End = binArray.Length - 1;
                lines.Add(newLine);
            }

            return lines;
        }
    }
    public class Round
    {
        public int Id;
        public List<Line> Lines = new List<Line>();
        #region operation
        public void Add(Line l)
        {
            EndY = _rowNo;

            if (!IsLineAdded)
            {
                Lines.Add(l);
                IsLineAdded = true;
            }
            else
            {
                Bottom.Merge(l);
            }

            if (MaxLenLine == null || l.LongerThan(MaxLenLine))
            {
                MaxLenLine = l;
                MaxLenY = _rowNo;
            }

            IsEnd = false;
        }
        public void StartScan(int rowNo)
        {
            _rowNo = rowNo;
            IsLineAdded = false;
        }
        public void FinishScan()
        {
            IsEnd = !IsLineAdded;
            if (IsEnd)
            {
            }
        }
        #endregion
        #region image information
        public int CenterX
        {
            get
            {
                return ImgLeftTopX + ImgX / 2;
            }
        }
        public int CenterY
        {
            get
            {
                return ImgLeftTopY + ImgY / 2;
            }
        }
        public int ImgLeftTopX
        {
            get
            {
                return MaxLenLine.Start;
            }
        }
        public int ImgLeftTopY
        {
            get
            {
                return StartY;
            }
        }
        public int ImgX
        {
            get
            {
                return MaxLenLine.Length;
            }
        }
        public int ImgY
        {
            get
            {
                return (EndY - StartY);
            }
        }
        #endregion
        #region statistic information
        public int StartY;
        public int EndY;
        public int MaxLenY;
        public Line MaxLenLine;
        public double LenXDiff;
        public double LenYDiff;
        public int Weight;
        public double WeightDiff;
        #endregion
        #region running information
        private int _rowNo;
        public Line Bottom
        {
            get
            {
                if (Lines.Count > 0)
                {
                    return Lines[Lines.Count - 1];
                }

                return null;
            }
        }
        public bool IsLineAdded;
        public bool IsEnd;
        #endregion
        public override string ToString()
        {
            string output = "";

            output += string.Format("ImgLeftTop: ({0},{1}), XLen: {2}, YLen: {3}",
                ImgLeftTopX, ImgLeftTopY, ImgX, ImgY) + Environment.NewLine;
            foreach (var line in Lines)
            {
                output += string.Format("({0},{1}) - ({2},{3})", line.Start, line.Y, line.End, line.Y) + Environment.NewLine;
            }

            return output;
        }
    }
}
