using Adjudication;
using Entities;
using Enums;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Tests;

// Diplomacy Adjudicator Test Cases, Lucas B. Kruijswijk
// https://boardgamegeek.com/filepage/274846/datc-diplomacy-adjudicator-test-cases

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class DATC_I : AdjudicationTestBase
{
    [Fact(DisplayName = "DATC I.01. Too many build orders")]
    public void DATC_I_1()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard(phase: Phase.Winter);

        board.AddUnits([(nation3, UnitType.Army, "Sil")]);
        board.AddCentres(
            [
                (nation4, "War"),
                (nation3, "Kie"),
                (nation3, "Mun"),
            ]);

        var build1 = board.Build(nation3, UnitType.Army, "War");
        var build2 = board.Build(nation3, UnitType.Army, "Kie");
        var build3 = board.Build(nation3, UnitType.Army, "Mun");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        build1.Status.Should().Be(OrderStatus.Invalid);
        var build2Status = build2.Status;
        build2Status.Should().BeOneOf(OrderStatus.Success, OrderStatus.Failure);

        if (build2Status == OrderStatus.Success)
        {
            build3.Status.Should().Be(OrderStatus.Failure);

            board.Next().ShouldHaveUnits(
                [
                    (nation3, UnitType.Army, "Sil", false),
                    (nation3, UnitType.Army, "Kie", false),
                ]);
        }
        else
        {
            build3.Status.Should().Be(OrderStatus.Success);

            board.Next().ShouldHaveUnits(
                [
                    (nation3, UnitType.Army, "Sil", false),
                    (nation3, UnitType.Army, "Mun", false),
                ]);
        }

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC I.02. Fleets cannot be built in land areas")]
    public void DATC_I_2()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard(phase: Phase.Winter);

        board.AddCentres([(nation4, "Mos")]);

        var build = board.Build(nation4, UnitType.Fleet, "Mos");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        build.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits([]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC I.03. Supply centre must be empty for building")]
    public void DATC_I_3()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard(phase: Phase.Winter);

        board.AddUnits([(nation3, UnitType.Army, "Ber")]);
        board.AddCentres(
            [
                (nation3, "Ber"),
                (nation3, "Mun"),
            ]);

        var build = board.Build(nation3, UnitType.Army, "Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        build.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits([(nation3, UnitType.Army, "Ber", false)]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC I.04. Both coasts must be empty for building")]
    public void DATC_I_4()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard(phase: Phase.Winter);

        board.AddUnits([(nation4, UnitType.Fleet, "Stp_S")]);
        board.AddCentres(
            [
                (nation4, "Stp"),
                (nation4, "Mos"),
            ]);

        var build = board.Build(nation4, UnitType.Fleet, "Stp_N");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        build.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits([(nation4, UnitType.Fleet, "Stp_S", false)]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC I.05. Building in home supply centre that is not owned")]
    public void DATC_I_5()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard(phase: Phase.Winter);

        board.AddCentres(
            [
                (nation4, "Ber"),
                (nation3, "Mun"),
            ]);

        var build = board.Build(nation3, UnitType.Army, "Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        build.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits([]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC I.06. Building in owned supply centre that is not a home supply centre")]
    public void DATC_I_6()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard(phase: Phase.Winter);

        board.AddCentres([(nation3, "War")]);

        var build = board.Build(nation3, UnitType.Army, "War");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        build.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits([]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC I.07. Only one build in a home supply centre")]
    public void DATC_I_7()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard(phase: Phase.Winter);

        board.AddCentres(
            [
                (nation4, "Mos"),
                (nation4, "Sev"),
            ]);

        var build1 = board.Build(nation4, UnitType.Army, "Mos");
        var build2 = board.Build(nation4, UnitType.Army, "Mos");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        build1.Status.Should().Be(OrderStatus.Success);
        build2.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits([(nation4, UnitType.Army, "Mos", false)]);

        world.ShouldHaveAllOrdersResolved();
    }
}
