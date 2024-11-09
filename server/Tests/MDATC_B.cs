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
public class MDATC_B : AdjudicationTestBase
{
    [Fact(DisplayName = "MDATC B.01. Move into own past forks timeline")]
    public void MDATC_B_1()
    {
        // Arrange
        var world = new World();

        var presentBoard = world.AddBoard(phase: Phase.Fall);
        var pastBoard = world.AddBoard();

        List<Unit> units =
            [
                .. pastBoard.AddUnits([(nation3, UnitType.Army, "Mun")]),
                .. presentBoard.AddUnits([(nation3, UnitType.Army, "Mun")]),
            ];

        units.Get("Mun").Hold(status: OrderStatus.Success);

        var order = units.Get("Mun", phase: Phase.Fall).Move("Tyr");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        order.Status.Should().Be(OrderStatus.Success);

        presentBoard.Next().ShouldHaveUnits([]);
        pastBoard.Next(timeline: 2).ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Mun", false),
                (nation3, UnitType.Army, "Tyr", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC B.02. Support to past move repelled by move forks timeline")]
    public void MDATC_B_2()
    {
        // Arrange
        var world = new World();

        var presentBoard = world.AddBoard(phase: Phase.Fall);
        var pastBoard = world.AddBoard();

        List<Unit> units =
            [
                .. pastBoard.AddUnits(
                    [
                        (nation3, UnitType.Army, "Mun"),
                        (nation5, UnitType.Army, "Boh"),
                    ]),
                .. presentBoard.AddUnits(
                    [
                        (nation3, UnitType.Army, "Mun"),
                        (nation5, UnitType.Army, "Boh"),
                    ]),
            ];

        units.Get("Mun").Move("Tyr", status: OrderStatus.Failure);
        units.Get("Boh").Move("Tyr", status: OrderStatus.Failure);

        var austrianHold = units.Get("Boh", phase: Phase.Fall).Hold();
        var germanSupport = units.Get("Mun", phase: Phase.Fall).Support(units.Get("Mun"), "Tyr");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianHold.Status.Should().Be(OrderStatus.Success);
        germanSupport.Status.Should().Be(OrderStatus.Success);

        presentBoard.Next().ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Mun", false),
                (nation5, UnitType.Army, "Boh", false),
            ]);
        pastBoard.Next(timeline: 2).ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Tyr", false),
                (nation5, UnitType.Army, "Boh", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC B.03. Support to past move repelled by hold dislodges")]
    public void MDATC_B_3()
    {
        // Arrange
        var world = new World();

        var presentBoard = world.AddBoard(phase: Phase.Fall);
        var pastBoard = world.AddBoard();

        List<Unit> units =
            [
                .. pastBoard.AddUnits(
                    [
                        (nation3, UnitType.Army, "Mun"),
                        (nation5, UnitType.Army, "Tyr"),
                    ]),
                .. presentBoard.AddUnits(
                    [
                        (nation3, UnitType.Army, "Mun"),
                        (nation5, UnitType.Army, "Tyr"),
                    ]),
            ];

        units.Get("Mun").Move("Tyr", status: OrderStatus.Failure);
        units.Get("Tyr").Hold(status: OrderStatus.Success);

        var austrianHold = units.Get("Tyr", phase: Phase.Fall).Hold();
        var germanSupport = units.Get("Mun", phase: Phase.Fall).Support(units.Get("Mun"), "Tyr");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianHold.Status.Should().Be(OrderStatus.Success);
        germanSupport.Status.Should().Be(OrderStatus.Success);

        presentBoard.ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Mun", false),
                (nation5, UnitType.Army, "Tyr", false),
            ]);
        pastBoard.ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Mun", false),
                (nation5, UnitType.Army, "Tyr", true),
            ]);
        presentBoard.ShouldNotHaveNextBoard();
        pastBoard.ShouldNotHaveNextBoard(timeline: 2);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC B.04. Failed move does not fork timeline")]
    public void MDATC_B_4()
    {
        // Arrange
        var world = new World();

        var presentBoard = world.AddBoard(phase: Phase.Fall);
        var pastBoard = world.AddBoard();

        List<Unit> units =
            [
                .. pastBoard.AddUnits(
                    [
                        (nation3, UnitType.Army, "Mun"),
                        (nation5, UnitType.Army, "Tyr"),
                    ]),
                .. presentBoard.AddUnits(
                    [
                        (nation3, UnitType.Army, "Mun"),
                        (nation5, UnitType.Army, "Tyr"),
                    ]),
            ];

        units.Get("Mun").Hold(status: OrderStatus.Success);
        units.Get("Tyr").Hold(status: OrderStatus.Success);

        var austrianHold = units.Get("Tyr", phase: Phase.Fall).Hold();
        var germanMove = units.Get("Mun", phase: Phase.Fall).Move("Tyr");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianHold.Status.Should().Be(OrderStatus.Success);
        germanMove.Status.Should().Be(OrderStatus.Failure);

        presentBoard.Next().ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Mun", false),
                (nation5, UnitType.Army, "Tyr", false),
            ]);
        pastBoard.ShouldNotHaveNextBoard(timeline: 2);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC B.05. Superfluous support does not fork timeline")]
    public void MDATC_B_5()
    {
        // Arrange
        var world = new World();

        var presentBoard = world.AddBoard(phase: Phase.Fall);
        var pastBoard = world.AddBoard();

        List<Unit> units =
            [
                .. pastBoard.AddUnits(
                    [
                        (nation3, UnitType.Army, "Mun"),
                        (nation3, UnitType.Army, "Boh"),
                    ]),
                .. presentBoard.AddUnits(
                    [
                        (nation3, UnitType.Army, "Tyr"),
                        (nation3, UnitType.Army, "Boh"),
                    ]),
            ];

        units.Get("Mun").Move("Tyr", status: OrderStatus.Success);
        units.Get("Boh").Hold(status: OrderStatus.Success);

        var hold = units.Get("Tyr", phase: Phase.Fall).Hold();
        var support = units.Get("Boh", phase: Phase.Fall).Support(units.Get("Mun"), "Tyr");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        hold.Status.Should().Be(OrderStatus.Success);
        support.Status.Should().Be(OrderStatus.Success);

        presentBoard.Next().ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Tyr", false),
                (nation3, UnitType.Army, "Boh", false),
            ]);
        pastBoard.ShouldNotHaveNextBoard(timeline: 2);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC B.06. Cross-timeline support does not fork head")]
    public void MDATC_B_6()
    {
        // Arrange
        var world = new World();

        var topBoard = world.AddBoard();
        var bottomBoard = world.AddBoard(timeline: 2);

        List<Unit> units =
            [
                .. topBoard.AddUnits([(nation3, UnitType.Army, "Mun")]),
                .. bottomBoard.AddUnits([(nation3, UnitType.Army, "Mun")]),
            ];

        var move = units.Get("Mun", timeline: 2).Move("Tyr", timeline: 2);
        var support = units.Get("Mun").Support(units.Get("Mun", timeline: 2), "Tyr", timeline: 2);

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move.Status.Should().Be(OrderStatus.Success);
        support.Status.Should().Be(OrderStatus.Success);

        topBoard.Next().ShouldHaveUnits([(nation3, UnitType.Army, "Mun", false)]);
        bottomBoard.Next().ShouldHaveUnits([(nation3, UnitType.Army, "Tyr", false)]);
        topBoard.ShouldNotHaveNextBoard(timeline: 3);
        bottomBoard.ShouldNotHaveNextBoard(timeline: 3);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC B.07. Cutting cross-timeline support forks timeline")]
    public void MDATC_B_7()
    {
        // Arrange
        var world = new World();

        var presentTopBoard = world.AddBoard(phase: Phase.Fall);
        var presentBottomBoard = world.AddBoard(timeline: 2, phase: Phase.Fall);
        var pastTopBoard = world.AddBoard();
        var pastBottomBoard = world.AddBoard(timeline: 2);

        List<Unit> units =
            [
                .. presentTopBoard.AddUnits(
                    [
                        (nation3, UnitType.Army, "Mun"),
                        (nation5, UnitType.Army, "Boh"),
                    ]),
                .. presentBottomBoard.AddUnits(
                    [
                        (nation3, UnitType.Army, "Tyr"),
                        (nation5, UnitType.Army, "Boh"),
                    ]),
                .. pastTopBoard.AddUnits(
                    [
                        (nation3, UnitType.Army, "Mun"),
                        (nation5, UnitType.Army, "Boh"),
                    ]),
                .. pastBottomBoard.AddUnits(
                    [
                        (nation3, UnitType.Army, "Mun"),
                        (nation5, UnitType.Army, "Boh"),
                    ]),
            ];

        units.Get("Mun", timeline: 2).Move("Tyr", timeline: 2, status: OrderStatus.Success);
        units.Get("Mun").Support(units.Get("Mun", timeline: 2), "Tyr", timeline: 2, status: OrderStatus.Success);
        units.Get("Boh").Hold(OrderStatus.Success);
        units.Get("Boh", timeline: 2).Move("Tyr", timeline: 2, status: OrderStatus.Failure);

        var austrianMove = units.Get("Boh", phase: Phase.Fall).Move("Mun");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianMove.Status.Should().Be(OrderStatus.Failure);

        presentTopBoard.Next().ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Mun", false),
                (nation5, UnitType.Army, "Boh", false),
            ]);
        presentBottomBoard.Next().ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Tyr", false),
                (nation5, UnitType.Army, "Boh", false),
            ]);
        pastBottomBoard.Next(timeline: 3).ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Mun", false),
                (nation5, UnitType.Army, "Boh", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC B.08. Multiple powers move to same board")]
    public void MDATC_B_8()
    {
        // Arrange
        var world = new World();

        var presentBoard = world.AddBoard(phase: Phase.Fall);
        var pastBoard = world.AddBoard();

        List<Unit> units =
            [
                .. pastBoard.AddUnits(
                    [
                        (nation1, UnitType.Fleet, "Lon"),
                        (nation2, UnitType.Army, "Ber"),
                        (nation3, UnitType.Army, "Mos"),
                        (nation4, UnitType.Army, "Con"),
                        (nation5, UnitType.Army, "Vie"),
                        (nation6, UnitType.Fleet, "Rom"),
                        (nation7, UnitType.Army, "Par"),
                    ]),
                .. presentBoard.AddUnits(
                    [
                        (nation1, UnitType.Fleet, "Lon"),
                        (nation2, UnitType.Army, "Ber"),
                        (nation3, UnitType.Army, "Mos"),
                        (nation4, UnitType.Army, "Con"),
                        (nation5, UnitType.Army, "Vie"),
                        (nation6, UnitType.Fleet, "Rom"),
                        (nation7, UnitType.Army, "Par"),
                    ]),
            ];

        units.Get("Lon").Hold(status: OrderStatus.Success);
        units.Get("Ber").Hold(status: OrderStatus.Success);
        units.Get("Mos").Hold(status: OrderStatus.Success);
        units.Get("Con").Hold(status: OrderStatus.Success);
        units.Get("Vie").Hold(status: OrderStatus.Success);
        units.Get("Rom").Hold(status: OrderStatus.Success);
        units.Get("Par").Hold(status: OrderStatus.Success);

        var englishMove = units.Get("Lon", phase: Phase.Fall).Move("Wal");
        var germanMove = units.Get("Ber", phase: Phase.Fall).Move("Kie");
        var russianMove = units.Get("Mos", phase: Phase.Fall).Move("Stp");
        var turkishMove = units.Get("Con", phase: Phase.Fall).Move("Bul");
        var austrianMove = units.Get("Vie", phase: Phase.Fall).Move("Bud");
        var italianMove = units.Get("Rom", phase: Phase.Fall).Move("Tus");
        var frenchMove = units.Get("Par", phase: Phase.Fall).Move("Pic");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        germanMove.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Success);
        turkishMove.Status.Should().Be(OrderStatus.Success);
        austrianMove.Status.Should().Be(OrderStatus.Success);
        italianMove.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Success);

        presentBoard.Next().ShouldHaveUnits([]);
        pastBoard.Next(timeline: 2).ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Lon", false),
                (nation1, UnitType.Fleet, "Wal", false),
                (nation2, UnitType.Army, "Ber", false),
                (nation2, UnitType.Army, "Kie", false),
                (nation3, UnitType.Army, "Mos", false),
                (nation3, UnitType.Army, "Stp", false),
                (nation4, UnitType.Army, "Con", false),
                (nation4, UnitType.Army, "Bul", false),
                (nation5, UnitType.Army, "Vie", false),
                (nation5, UnitType.Army, "Bud", false),
                (nation6, UnitType.Fleet, "Rom", false),
                (nation6, UnitType.Fleet, "Tus", false),
                (nation7, UnitType.Army, "Par", false),
                (nation7, UnitType.Army, "Pic", false),
            ]);
        pastBoard.ShouldNotHaveNextBoard(timeline: 3);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC B.09. Grandfather paradox resolved")]
    public void MDATC_B_9()
    {
        // Arrange
        var world = new World();

        var presentBoard = world.AddBoard(phase: Phase.Fall);
        var pastBoard = world.AddBoard();

        List<Unit> units =
            [
                .. pastBoard.AddUnits(
                    [
                        (nation4, UnitType.Army, "Mos"),
                        (nation4, UnitType.Army, "War"),
                    ]),
                .. presentBoard.AddUnits(
                    [
                        (nation4, UnitType.Army, "Mos"),
                        (nation4, UnitType.Army, "War"),
                    ]),
            ];

        units.Get("Mos").Hold(status: OrderStatus.Success);
        units.Get("War").Hold(status: OrderStatus.Success);

        var move = units.Get("Mos", phase: Phase.Fall).Move("Mos");
        var support = units.Get("War", phase: Phase.Fall).Support(units.Get("Mos", phase: Phase.Fall), "Mos");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move.Status.Should().Be(OrderStatus.Failure);
        support.Status.Should().Be(OrderStatus.Failure);

        presentBoard.Next().ShouldHaveUnits(
            [
                (nation4, UnitType.Army, "Mos", false),
                (nation4, UnitType.Army, "War", false),
            ]);
        pastBoard.ShouldNotHaveNextBoard(timeline: 2);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "MDATC B.10. Vanishing in a puff of logic")]
    public void MDATC_B_10()
    {
        // It should never be possible for this situation to arise in an actual game. If a unit exists on a board and
        // has no past self, then it must have been built in the intervening winter board, and thus moving back in time
        // would create a different base for the resolution of that winter board and fork the timeline. However, if
        // there is indeed an edge case we've missed where a unit can travel to its own past without bouncing or
        // forking a timeline, it seems thematically appropriate for it to ouroboros itself out of existence.

        // Arrange
        var world = new World();

        var presentBoard = world.AddBoard(phase: Phase.Fall);
        var pastBoard = world.AddBoard();

        var units = presentBoard.AddUnits([(nation2, UnitType.Army, "Bel")]);

        var move = units.Get("Bel", phase: Phase.Fall).Move("Bel");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move.Status.Should().Be(OrderStatus.Success);

        presentBoard.Next().ShouldHaveUnits([]);
        pastBoard.ShouldNotHaveNextBoard(timeline: 2);

        world.ShouldHaveAllOrdersResolved();
    }
}
