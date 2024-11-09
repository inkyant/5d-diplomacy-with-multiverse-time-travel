﻿using Adjudication;
using Entities;
using Enums;
using Factories;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Tests;

// Adapted and extended from Multiversal Diplomacy Adjudicator Test Cases, Tim Van Baak
// https://github.com/Jaculabilis/5dplomacy/blob/master/MDATC.html

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class MDATC_E : AdjudicationTestBase
{
    private readonly List<string> allRegionIds;

    public MDATC_E() => allRegionIds = MapFactory.CreateRegions().Select(r => r.Id).ToList();

    [Fact(DisplayName = "MDATC E.01. No winner when all centre counts below 18")]
    public void MDATC_E_1()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var englishCentres = allRegionIds[..4].Select(r => (nation1, r));
        var germanCentres = allRegionIds.Slice(4, 4).Select(r => (nation3, r));
        var russianCentres = allRegionIds.Slice(8, 4).Select(r => (nation4, r));
        var turkishCentres = allRegionIds.Slice(12, 4).Select(r => (nation5, r));
        var austrianCentres = allRegionIds.Slice(16, 4).Select(r => (nation5, r));
        var italianCentres = allRegionIds.Slice(20, 4).Select(r => (nation6, r));
        var frenchCentres = allRegionIds.Slice(24, 4).Select(r => (nation2, r));

        board.AddCentres(
            [
                .. englishCentres,
                .. germanCentres,
                .. russianCentres,
                .. turkishCentres,
                .. austrianCentres,
                .. italianCentres,
                .. frenchCentres,
            ]);

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        world.Winner.Should().BeNull();
    }

    [Fact(DisplayName = "MDATC E.02. Winner if single nation has at least 18 centres")]
    public void MDATC_E_2()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var englishCentres = allRegionIds[..18].Select(r => (nation1, r));
        var germanCentres = allRegionIds.Slice(18, 2).Select(r => (nation3, r));
        var russianCentres = allRegionIds.Slice(20, 2).Select(r => (nation4, r));
        var turkishCentres = allRegionIds.Slice(22, 2).Select(r => (nation5, r));
        var austrianCentres = allRegionIds.Slice(24, 2).Select(r => (nation5, r));
        var italianCentres = allRegionIds.Slice(26, 2).Select(r => (nation6, r));
        var frenchCentres = allRegionIds.Slice(28, 2).Select(r => (nation2, r));

        board.AddCentres(
            [
                .. englishCentres,
                .. germanCentres,
                .. russianCentres,
                .. turkishCentres,
                .. austrianCentres,
                .. italianCentres,
                .. frenchCentres,
            ]);

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        world.Winner.Should().Be(nation1);
    }

    [Fact(DisplayName = "MDATC E.03. No winner if two nations on same centre count above 18 centres")]
    public void MDATC_E_3()
    {
        // Arrange
        var world = new World();
        var topBoard = world.AddBoard();
        var bottomBoard = world.AddBoard(timeline: 2);

        var englishCentres = allRegionIds[..18].Select(r => (nation1, r));
        var germanCentres = allRegionIds.Slice(16, 18).Select(r => (nation3, r));

        topBoard.AddCentres(englishCentres.ToList());
        bottomBoard.AddCentres(germanCentres.ToList());

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        world.Winner.Should().BeNull();
    }

    [Fact(DisplayName = "MDATC E.04. Winner if one nation has clear majority with more than one above 18 centres")]
    public void MDATC_E_4()
    {
        // Arrange
        var world = new World();
        var topBoard = world.AddBoard();
        var bottomBoard = world.AddBoard(timeline: 2);

        var englishCentres = allRegionIds[..19].Select(r => (nation1, r));
        var germanCentres = allRegionIds.Slice(16, 18).Select(r => (nation3, r));

        topBoard.AddCentres(englishCentres.ToList());
        bottomBoard.AddCentres(germanCentres.ToList());

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        world.Winner.Should().Be(nation1);
    }

    [Fact(DisplayName = "MDATC E.05. Past boards don't count towards centre total")]
    public void MDATC_E_5()
    {
        // Arrange
        var world = new World();
        var pastBoard = world.AddBoard();
        var presentBoard = world.AddBoard(phase: Phase.Fall);

        var pastCentres = allRegionIds[..9].Select(r => (nation1, r));
        var presentCentres = allRegionIds.Slice(9, 9).Select(r => (nation1, r));

        pastBoard.AddCentres(pastCentres.ToList());
        presentBoard.AddCentres(presentCentres.ToList());

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        world.Winner.Should().BeNull();
    }

    [Fact(DisplayName = "MDATC E.06. Victory total must count only unique centres")]
    public void MDATC_E_6()
    {
        // Arrange
        var world = new World();
        var topBoard = world.AddBoard();
        var bottomBoard = world.AddBoard(timeline: 2);

        var topCentres = allRegionIds[..9].Select(r => (nation1, r));
        var bottomCentres = allRegionIds.Slice(6, 9).Select(r => (nation1, r));

        topBoard.AddCentres(topCentres.ToList());
        bottomBoard.AddCentres(bottomCentres.ToList());

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        world.Winner.Should().BeNull();
    }

    [Fact(DisplayName = "MDATC E.07. Victory can come from distributed centres")]
    public void MDATC_E_7()
    {
        // Arrange
        var world = new World();
        var topBoard = world.AddBoard(phase: Phase.Winter);
        var middleBoard = world.AddBoard(timeline: 2, phase: Phase.Fall);
        var bottomBoard = world.AddBoard(timeline: 3);

        var topCentres = allRegionIds[..6].Select(r => (nation1, r));
        var middleCentres = allRegionIds.Slice(6, 6).Select(r => (nation1, r));
        var bottomCentres = allRegionIds.Slice(12, 6).Select(r => (nation1, r));

        topBoard.AddCentres(topCentres.ToList());
        middleBoard.AddCentres(middleCentres.ToList());
        bottomBoard.AddCentres(bottomCentres.ToList());

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        world.Winner.Should().Be(nation1);
    }

    [Fact(DisplayName = "MDATC E.08. No further adjudication after victory")]
    public void MDATC_E_8()
    {
        // Arrange
        var world = new World
        {
            Winner = nation2
        };
        var board = world.AddBoard();

        var units = board.AddUnits([(nation2, UnitType.Army, "Par")]);

        var order = units.Get("Par").Move("Bur");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        order.Status.Should().Be(OrderStatus.New);

        // Assert
        world.Orders.Should().BeEmpty();
        world.Winner.Should().Be(nation2);
    }
}
