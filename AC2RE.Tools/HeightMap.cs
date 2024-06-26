﻿using AC2RE.Definitions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace AC2RE.Tools
{
    public class HeightMap
    {
        private const int LANDBLOCK_SIZE = 255;
        private const int BLOCK_SIZE = 17;
        private const int CELL_SIZE = 10;
        private const int GLOBAL_SIZE = LANDBLOCK_SIZE * BLOCK_SIZE * CELL_SIZE;
        
        private DataId LANDDESCFILE_ID = new(0x36000000);

        public DatReader cellReader;
        public DatReader portalReader;
        public LandScapeDefs landScapeDefs;

        public HeightMap()
        {
            cellReader = new(@"/Users/troispoils/Documents/DatFiles/cell_1.dat");
            portalReader = new(@"/Users/troispoils/Documents/DatFiles/portal.dat");

            using (var data = portalReader.getFileReader(LANDDESCFILE_ID))
            {
                landScapeDefs = new(data);
            }
        }

        public Point[,] GeneratePositions()
        {
            var points = new Point[LANDBLOCK_SIZE * (BLOCK_SIZE - 1), LANDBLOCK_SIZE * (BLOCK_SIZE - 1)];

            for (byte landy = 0; landy < LANDBLOCK_SIZE; landy++)
            {
                for (byte landx = 0; landx < LANDBLOCK_SIZE; landx++)
                {
                    ProcessLandBlock(points, landx, landy);
                }
            }

            CalculateSlopeEachPoint(points);

            return points;
        }

        private void CalculateSlopeEachPoint(Point[,] points)
        {
            Console.WriteLine("Calcule Slope.");
            for (int y = 0; y < points.GetLength(0) - 1; y++)
            {
                for (int x = 0; x < points.GetLength(1) - 1; x++)
                {
                    if (points[x, y] == null) Console.WriteLine($"{x}-{y}");
                    if (points[x + 1, y] == null) Console.WriteLine($"{x + 1}-{y}");
                    if (points[x, y + 1] == null) Console.WriteLine($"{x}-{y + 1}");
                    var slope = MathsTools.CalculateSlope(points[x, y].point, points[x + 1, y].point, points[x, y + 1].point);
                    points[x, y].passable = slope < 40 ? true : false;
                    points[x, y].slope = slope;
                }
            }
            Console.WriteLine("Done calcule Slope.");
        }

        private void ProcessLandBlock(Point[,] points, byte landx, byte landy)
        {
            var cellId = new CellId(landx, landy, 0xFF, 0xFF);
            var landBlockId = new DataId(cellId.id);

            if (!cellReader.contains(landBlockId))
                return;

            using var data = cellReader.getFileReader(landBlockId);
            var landBlockData = new CLandBlockData(data);

            if (landBlockData == null)
                return;

            PopulatePoints(points, landBlockData, landx, landy);
        }

        private void PopulatePoints(Point[,] points, CLandBlockData landBlockData, byte landx, byte landy)
        {
            for (int y = 0; y < BLOCK_SIZE - 1; y++)
            {
                for (int x = 0; x < BLOCK_SIZE - 1; x++)
                {
                    var vectorPoint = CreateVectorPoint(landBlockData, landx, landy, x, y);
                    points[landx * (BLOCK_SIZE - 1) + x, landy * (BLOCK_SIZE - 1) + y] = vectorPoint;
                }
            }
        }

        private Point CreateVectorPoint(CLandBlockData landBlockData, byte landx, byte landy, int x, int y)
        {
            //TODO: add CELL_SIZE for calculate real pitch or another position
            var pos = x * BLOCK_SIZE + y;
            int posX = (landx * BLOCK_SIZE + x) * CELL_SIZE - (landx * CELL_SIZE);
            int posY = GLOBAL_SIZE - ((landy * BLOCK_SIZE + y) * CELL_SIZE) - 1 - ((LANDBLOCK_SIZE * CELL_SIZE) - (landy * CELL_SIZE) - 1);
            //int rposX = landx * BLOCK_SIZE + x - landx;
            //int rposY = (LANDBLOCK_SIZE * BLOCK_SIZE) - (landy * BLOCK_SIZE + y) - 1 - (LANDBLOCK_SIZE - landy - 1);
            var posZ = landScapeDefs.landHeightTable[landBlockData.heights[pos]];

            var cellInfo = landBlockData.cellInfos[pos];


            return new Point()
            {
                point = new(posX, posY, posZ),
                passable = false,
                terrainType = MathsTools.GetTerrainInCellInfo(cellInfo),
                sceneIndex = MathsTools.GetSceneInCellInfo(cellInfo),
                roadIndex = MathsTools.GetRoadInCellInfo(cellInfo)
            };
        }
    }
}
