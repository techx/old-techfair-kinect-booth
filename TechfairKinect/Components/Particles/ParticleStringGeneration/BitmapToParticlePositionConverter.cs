using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace TechfairKinect.Components.Particles.ParticleStringGeneration
{
    //given a text bitmap, perform a greedy algorithm: any (i,j)..(i+2R,j+2R) region with a percentage of nonzero pixels
    //greater than a treshold percentage turns into a particle with center (i+R,j+R) and radius R
    //after a particle has been created for (i,j)..(i+2R,j+2R), no particle can be in the region (i-0.3R,j-0.3R)..(i+2.3R,j+2.3R)
    //i.e. give a padding of 0.3R on all sides

    //we can find the percentage of nonzero particles of some region (r1,c1)..(r2,c2) in constant time by preprocessing an array
    //counts[c][r] which represents the number of nonzero particles in 0..r,0..c; 
    //the number of nonzero particles in (r1,c1)..(r2,c2) then becomes 
    //counts[c2][r2] - counts[c1][r2] - counts[c2][r1] + counts[c1][r1]

    //to process the bitmap and create counts in one pass, do the following:
    //process each column iteratively
    //  process each element in the column iteratively
    //  keep a column total while processing the column (number of nonzero particles encountered so far in the row)
    //  for the first col:
    //      counts[col][row] = colTotal
    //  for all other rows:
    //      counts[col][row] = colTotal + counts[col - 1][row]

    //we make a further memory optimization by using a sliding window technique
    //we only need to keep a 2d array of dimensions 2RxH, and processing first by column then by row; 
    //columns more than 2R ago will not be used (we optimize out the width component rather than the height
    //because the text runs left to right and will thus be much wider than it is tall)

    //in order to check for intersecting particles, we keep a sorted set (impl as a red-black tree) of Tuple<int, int> 
    //this contains the non-intersecting right-most particles in (r, c) form.

    //to check if the particle ending at (r, c) intersects some other particle we call GetViewBetween(<r-5.2R, -1>, <r + 1, c + 1>).
    //each particle is 2.6R on both sides (the set contains the top left corners of particles including the padding)

    //if nothing is in that range, we can put a particle ending at (r, c)
    //if there is at least one particle in that range, we can put a particle at (r, c) if and only if the columns
    //of all particles in that range are at at least 5.2R less than c (and in that case, we need to remove those)

    //the performance of GetViewBetween() will be O(lgN) with respect to the size of the tree (height of our grid) 
    //and O(N) with respect to the size of the subtree, but the subtree will be at most size 2 since only 2 particles 
    //will ever intersect us. for details on the performance of GetViewBetween(), see: 
    //http://stackoverflow.com/questions/9850975/why-sortedsett-getviewbetween-isnt-olog-n

    //Memory: sliding window O(RH) + sorted set O(H)
    //Runtime: O(WH lg H)
    //note: this is a greedy algorithm, but should be fine for large enough font sizes
    internal class BitmapToParticlePositionConverter
    {
        private const double SidePadding = 0.3;
        private const double ThresholdPercentage = 0.1; //percentage of particles in a block to constitute a particle

        private int _bytesPerPixel;

        private SortedSet<Tuple<int, int>> _rightMostNonintersectingParticles;
        private int[][] _nonzeroPixelCounts;
        private int[] _nextAcceptableParticleRowByRow;

        public IEnumerable<Point> GenerateParticlePositions(Bitmap bitmap, int bytesPerPixel, Rectangle stringRectangle, int particleRadius)
        {
            if (stringRectangle.Width < 2 * particleRadius || stringRectangle.Height < 2 * particleRadius)
                throw new Exception(string.Format("Rectangle dimensions (width: {0}, height: {1}) too small for particle radius ({2})", 
                    stringRectangle.Width, stringRectangle.Height, particleRadius));

            var data = bitmap.LockBits(stringRectangle, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            _bytesPerPixel = bytesPerPixel;

            var particlePositions = GenerateParticlePositionsFromBitmapData(data, particleRadius);

            bitmap.UnlockBits(data);
            return particlePositions;
        }

        private IEnumerable<Point> GenerateParticlePositionsFromBitmapData(BitmapData data, int particleRadius)
        {
            var windowSize = 2 * particleRadius;
            _nonzeroPixelCounts = new int[windowSize][];
            _nextAcceptableParticleRowByRow = new int[data.Height];
            _rightMostNonintersectingParticles = new SortedSet<Tuple<int, int>>();
            
            var points = new List<Point>();
            var colPtr = data.Scan0;

            for (var col = 0; col < data.Width; col++, colPtr += _bytesPerPixel)
                points.AddRange(CalculateCumulativeColumnCount(data, col, colPtr, particleRadius));

            return points;
        }

        private IEnumerable<Point> CalculateCumulativeColumnCount(BitmapData data, int col, IntPtr colPtr, int particleRadius)
        {
            var windowIndex = col % _nonzeroPixelCounts.Length;
            _nonzeroPixelCounts[windowIndex] = new int[data.Height];
            var colTotal = 0;

            for (var row = 0; row < data.Height; row++, colPtr += data.Stride)
            {
                if (PixelHasColor(colPtr))
                    colTotal++;

                _nonzeroPixelCounts[windowIndex][row] = colTotal;

                if (col != 0)
                    _nonzeroPixelCounts[windowIndex][row] += _nonzeroPixelCounts[(col - 1) % _nonzeroPixelCounts.Length][row];

                if (ParticleCanEndAtRowCol(row, col, particleRadius))
                {
                    //this is the bottom right corner of a particle not including its padding on this side; we want the top left including the padding
                    _rightMostNonintersectingParticles.Add(new Tuple<int, int>((int)(row - (2 + SidePadding) * particleRadius), (int)(col - (2 + SidePadding) * particleRadius)));
                    yield return new Point(col - particleRadius, row - particleRadius);
                }
            }
        }
        private unsafe bool PixelHasColor(IntPtr pointer)
        {
            return *(short*)pointer == 0;
        }

        private bool ParticleCanEndAtRowCol(int row, int col, int particleRadius)
        {
            var blockSize = 2 * particleRadius;
            if (row < blockSize || col < blockSize)
                return false;

            if (!RegionHasEnoughNonzeroPixels(row, col, blockSize))
                return false;

            return !ParticleIntersectsOtherParticles(row, col, particleRadius);
        }

        private bool RegionHasEnoughNonzeroPixels(int row, int col, int blockSize)
        {
            var windowSize = _nonzeroPixelCounts.Length;

            var usedPixels = _nonzeroPixelCounts[col % windowSize][row]
                            - _nonzeroPixelCounts[(col - blockSize + 1) % windowSize][row]
                            - _nonzeroPixelCounts[col % windowSize][row - blockSize + 1]
                            + _nonzeroPixelCounts[(col - blockSize + 1) % windowSize][row - blockSize + 1];
            
            return usedPixels >= ThresholdPercentage * 4 * blockSize * blockSize;
        }

        private bool ParticleIntersectsOtherParticles(int row, int col, int particleRadius)
        {
            var intersection = _rightMostNonintersectingParticles.GetViewBetween(
                new Tuple<int, int>((int)(row - 4 * (1 + SidePadding) * particleRadius), -1), //two particles' worth of space
                new Tuple<int, int>(row + 1, col + 1));

            if (intersection.Any(particle => particle.Item2 + 5.2 * particleRadius >= col))
                return true;

            //if (r, c) doesn't intersect these particles, then nothing will since all future positions (r2, c2) 
            //will have r2 >= r and c2 >= c; remove them to keep performance high
            intersection.ToList().ForEach(particle => _rightMostNonintersectingParticles.Remove(particle)); 

            return false;
        }
    }
}
