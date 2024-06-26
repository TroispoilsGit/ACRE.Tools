﻿using System.Numerics;
using AC2RE.Tools;

namespace AC2RE.ToolsTest;

public class QuadTreeTests
{
    [Fact]
    public void Insert_ShouldAddPoint_WhenWithinBounds()
    {
        // Arrange
        Bound bound = new Bound(0, 0, 10, 10);
        QuadTree quadTree = new QuadTree(bound);
        Point point = new Point() { point = new(5, 5, 0) };

        // Act
        bool result = quadTree.Insert(point);

        // Assert
        Assert.True(result);
        Assert.Contains(point, quadTree.points);
    }

    [Fact]
    public void Insert_ShouldNotAddPoint_WhenOutsideBounds()
    {
        // Arrange
        Bound bound = new Bound(0, 0, 10, 10);
        QuadTree quadTree = new QuadTree(bound);
        Point point = new Point() { point = new(15, 15, 0) };

        // Act
        bool result = quadTree.Insert(point);

        // Assert
        Assert.False(result);
        Assert.DoesNotContain(point, quadTree.points);
    }

    [Fact]
    public void SubDivide_ShouldDivideIntoFourQuadrants()
    {
        // Arrange
        Bound bound = new Bound(0, 0, 10, 10);
        QuadTree quadTree = new QuadTree(bound);

        // Act
        quadTree.SubDivide();

        // Assert
        Assert.True(quadTree.divided);
        Assert.NotNull(quadTree.nw);
        Assert.NotNull(quadTree.ne);
        Assert.NotNull(quadTree.sw);
        Assert.NotNull(quadTree.se);
    }

    [Fact]
    public void Insert_ShouldSubDivideAndAddPointToCorrectQuadrant()
    {
        // Arrange
        Bound bound = new Bound(0, 0, 10, 10);
        QuadTree quadTree = new QuadTree(bound, 1); // Set capacity to 1 for easy subdivision
        Point point1 = new Point() { point = new(5, 5, 0) };
        Point point2 = new Point() { point = new(7, 7, 0) };

        // Act
        quadTree.Insert(point1);
        bool result = quadTree.Insert(point2);

        // Assert
        Assert.True(quadTree.divided);
        Assert.Contains(point1, quadTree.points);
        Assert.True(result);
        Assert.Contains(point2, quadTree.ne.points); // Point2 should be in the NE quadrant
    }

    [Fact]
    public void Query_ShouldReturnPointsWithinRange()
    {
        // Arrange
        Bound bound = new Bound(0, 0, 10, 10);
        QuadTree quadTree = new QuadTree(bound);
        Point point1 = new Point() { point = new(2, 2, 0) };
        Point point2 = new Point() { point = new(8, 8, 0) };
        Point point3 = new Point() { point = new(12, 12, 0) }; // Outside the initial bounds
        quadTree.Insert(point1);
        quadTree.Insert(point2);
        quadTree.Insert(point3);

        // Act
        Bound queryBound = new Bound(0, 0, 10, 10);
        List<Point> foundPoints = quadTree.Query(queryBound, new List<Point>());

        // Assert
        Assert.Contains(point1, foundPoints);
        Assert.Contains(point2, foundPoints);
        Assert.DoesNotContain(point3, foundPoints);
    }
}
