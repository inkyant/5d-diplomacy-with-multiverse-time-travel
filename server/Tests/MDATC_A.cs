using Adjudication;
using Entities;
using Enums;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Tests;

// Adapted and extended from Multiversal Diplomacy Adjudicator Test Cases, Tim Van Baak
// https://github.com/Jaculabilis/5dplomacy/blob/master/MDATC.html

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class MDATC_A : AdjudicationTestBase
{
    [Fact(DisplayName = "MDATC A.01. Move to same region with loose adjacencies")]
    public void MDATC_A_1()
    {
        // Arrange
        var world = new World();

        var topBoard = world.AddBoard();
        var bottomBoard = world.AddBoard(timeline: 2);

        var units = topBoard.AddUnits([(nation1, UnitType.Army, "Lon")]);

        var order = units.Get("Lon").Move("Lon", timeline: 2);

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        order.Status.Should().Be(OrderStatus.Success);

        topBoard.Next().ShouldHaveUnits([]);
        bottomBoard.Next().ShouldHaveUnits([(nation1, UnitType.Army, "Lon", false)]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC A.02. Move to same region with strict adjacencies")]
    public void MDATC_A_2()
    {
        // Arrange
        var world = new World();

        var topBoard = world.AddBoard();
        var bottomBoard = world.AddBoard(timeline: 2);

        var units = topBoard.AddUnits([(nation5, UnitType.Fleet, "Smy")]);

        var order = units.Get("Smy").Move("Smy", timeline: 2);

        // Act
        new Adjudicator(world, true, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        order.Status.Should().Be(OrderStatus.Success);

        topBoard.Next().ShouldHaveUnits([]);
        bottomBoard.Next().ShouldHaveUnits([(nation5, UnitType.Fleet, "Smy", false)]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC A.03. Move to neighbouring region with loose adjacencies")]
    public void MDATC_A_3()
    {
        // Arrange
        var world = new World();

        var topBoard = world.AddBoard();
        var bottomBoard = world.AddBoard(timeline: 2);

        var units = bottomBoard.AddUnits([(nation5, UnitType.Army, "Vie")]);

        var order = units.Get("Vie", timeline: 2).Move("Bud");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        order.Status.Should().Be(OrderStatus.Success);

        topBoard.Next().ShouldHaveUnits([(nation5, UnitType.Army, "Bud", false)]);
        bottomBoard.Next().ShouldHaveUnits([]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC A.04. Move to neighbouring region with strict adjacencies")]
    public void MDATC_A_4()
    {
        // Arrange
        var world = new World();

        var topBoard = world.AddBoard();
        var bottomBoard = world.AddBoard(timeline: 2);

        var units = bottomBoard.AddUnits([(nation1, UnitType.Fleet, "Tun")]);

        var order = units.Get("Tun", timeline: 2).Move("WES");

        // Act
        new Adjudicator(world, true, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        order.Status.Should().Be(OrderStatus.Invalid);

        topBoard.Next().ShouldHaveUnits([]);
        bottomBoard.Next().ShouldHaveUnits([(nation1, UnitType.Fleet, "Tun", false)]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC A.05. Move to non-neighbouring region")]
    public void MDATC_A_5()
    {
        // Arrange
        var world = new World();

        var topBoard = world.AddBoard();
        var bottomBoard = world.AddBoard(timeline: 2);

        var units = topBoard.AddUnits([(nation1, UnitType.Fleet, "Lvp")]);

        var order = units.Get("Lvp").Move("Edi", timeline: 2);

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        order.Status.Should().Be(OrderStatus.Invalid);

        topBoard.Next().ShouldHaveUnits([(nation1, UnitType.Fleet, "Lvp", false)]);
        bottomBoard.Next().ShouldHaveUnits([]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC A.06. Can't cross coasts across time")]
    public void MDATC_A_6()
    {
        // Arrange
        var world = new World();

        var topBoard = world.AddBoard();
        var bottomBoard = world.AddBoard(timeline: 2);

        var units = bottomBoard.AddUnits([(nation2, UnitType.Fleet, "Spa_S")]);

        var order = units.Get("Spa_S", timeline: 2).Move("Spa_N");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        order.Status.Should().Be(OrderStatus.Invalid);

        topBoard.Next().ShouldHaveUnits([]);
        bottomBoard.Next().ShouldHaveUnits([(nation2, UnitType.Fleet, "Spa_S", false)]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC A.07. No diagonal board movement")]
    public void MDATC_A_7()
    {
        // Arrange
        var world = new World();

        var topBoard = world.AddBoard(phase: Phase.Fall);
        var bottomBoard = world.AddBoard(timeline: 2);

        var units = topBoard.AddUnits([(nation3, UnitType.Army, "Mun")]);

        var order = units.Get("Mun", phase: Phase.Fall).Move("Mun", timeline: 2);

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        order.Status.Should().Be(OrderStatus.Invalid);

        topBoard.Next().ShouldHaveUnits([(nation3, UnitType.Army, "Mun", false)]);
        bottomBoard.Next().ShouldHaveUnits([]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC A.08. Must be adjacent timeline without convoy")]
    public void MDATC_A_8()
    {
        // Arrange
        var world = new World();

        var topBoard = world.AddBoard();
        var middleBoard = world.AddBoard(timeline: 2);
        var bottomBoard = world.AddBoard(timeline: 3);

        var units = topBoard.AddUnits([(nation1, UnitType.Army, "Lon")]);

        var order = units.Get("Lon").Move("Lon", timeline: 3);

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        order.Status.Should().Be(OrderStatus.Invalid);

        topBoard.Next().ShouldHaveUnits([(nation1, UnitType.Army, "Lon", false)]);
        middleBoard.Next().ShouldHaveUnits([]);
        bottomBoard.Next().ShouldHaveUnits([]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC A.09. Must be immediate past major board without convoy")]
    public void MDATC_A_9()
    {
        // Arrange
        var world = new World();

        var presentBoard = world.AddBoard(year: 1902);
        var pastBoard1 = world.AddBoard(phase: Phase.Winter);
        var pastBoard2 = world.AddBoard(phase: Phase.Fall);
        var pastBoard3 = world.AddBoard();

        var units = presentBoard.AddUnits(
            [
                (nation1, UnitType.Army, "Lon"),
                (nation2, UnitType.Army, "Par"),
            ]);

        var englishMove = units.Get("Lon", year: 1902).Move("Lon");
        var frenchMove = units.Get("Par", year: 1902).Move("Gas", phase: Phase.Fall);

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Invalid);
        frenchMove.Status.Should().Be(OrderStatus.Success);

        presentBoard.Next().ShouldHaveUnits([(nation1, UnitType.Army, "Lon", false)]);
        pastBoard1.ShouldNotHaveNextBoard(timeline: 2);
        pastBoard2.Next(timeline: 2).ShouldHaveUnits([(nation2, UnitType.Army, "Gas", false)]);
        pastBoard3.ShouldNotHaveNextBoard(timeline: 2);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC A.10. No move to winter board")]
    public void MDATC_A_10()
    {
        // Arrange
        var world = new World();

        var presentBoard = world.AddBoard(year: 1902);
        var pastBoard = world.AddBoard(phase: Phase.Winter);

        presentBoard.AddCentres([(nation4, "Sev")]);
        pastBoard.AddCentres([(nation4, "Sev")]);

        List<Unit> units =
            [
                .. pastBoard.AddUnits([(nation4, UnitType.Fleet, "Sev")]),
                .. presentBoard.AddUnits([(nation4, UnitType.Fleet, "Sev")]
            )];

        var order = units.Get("Sev", year: 1902).Move("Sev", phase: Phase.Winter);

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        order.Status.Should().Be(OrderStatus.Invalid);

        presentBoard.Next().ShouldHaveUnits([(nation4, UnitType.Fleet, "Sev", false)]);
        pastBoard.ShouldNotHaveNextBoard(timeline: 2);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC A.11. Simultaneous adjustment and movement phases advance together")]
    public void MDATC_A_11()
    {
        // Arrange
        var world = new World();

        var topBoard = world.AddBoard(year: 1902);
        var bottomBoard = world.AddBoard(timeline: 2, phase: Phase.Winter);

        bottomBoard.AddCentres([(nation3, "Ber")]);

        var units = topBoard.AddUnits([(nation1, UnitType.Fleet, "Edi")]);

        var englishMove = units.Get("Edi", year: 1902).Move("Cly", year: 1902);
        var germanBuild = bottomBoard.Build(nation3, UnitType.Army, "Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        germanBuild.Status.Should().Be(OrderStatus.Success);

        topBoard.Next().ShouldHaveUnits([(nation1, UnitType.Fleet, "Cly", false)]);
        bottomBoard.Next().ShouldHaveUnits([(nation3, UnitType.Army, "Ber", false)]);

        world.ShouldHaveAllOrdersResolved();
    }
}
