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
public class DATC_D : AdjudicationTestBase
{
    [Fact(DisplayName = "DATC D.01. Supported hold can prevent dislodgement")]
    public void DATC_D_1()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "ADR"),
                (nation1, UnitType.Army, "Tri"),
                (nation2, UnitType.Army, "Ven"),
                (nation2, UnitType.Army, "Tyr"),
            ]);

        var austrianMove = units.Get("Tri").Move("Ven");
        var austrianSupport = units.Get("ADR").Support(units.Get("Tri"), "Ven");
        var italianHold = units.Get("Ven").Hold();
        var italianSupport = units.Get("Tyr").Support(units.Get("Ven"));

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianMove.Status.Should().Be(OrderStatus.Failure);
        austrianSupport.Status.Should().Be(OrderStatus.Failure);
        italianHold.Status.Should().Be(OrderStatus.Success);
        italianSupport.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "ADR", false),
                (nation1, UnitType.Army, "Tri", false),
                (nation2, UnitType.Army, "Ven", false),
                (nation2, UnitType.Army, "Tyr", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.02. A move cuts support on hold")]
    public void DATC_D_2()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "ADR"),
                (nation1, UnitType.Army, "Tri"),
                (nation1, UnitType.Army, "Vie"),
                (nation2, UnitType.Army, "Ven"),
                (nation2, UnitType.Army, "Tyr"),
            ]);

        var austrianMove1 = units.Get("Tri").Move("Ven");
        var austrianMove2 = units.Get("Vie").Move("Tyr");
        var austrianSupport = units.Get("ADR").Support(units.Get("Tri"), "Ven");
        var italianHold = units.Get("Ven").Hold();
        var italianSupport = units.Get("Tyr").Support(units.Get("Ven"));

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianMove1.Status.Should().Be(OrderStatus.Success);
        austrianMove2.Status.Should().Be(OrderStatus.Failure);
        austrianSupport.Status.Should().Be(OrderStatus.Success);
        italianHold.Status.Should().Be(OrderStatus.Failure);
        italianSupport.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "ADR", false),
                (nation1, UnitType.Army, "Tri", false),
                (nation1, UnitType.Army, "Vie", false),
                (nation2, UnitType.Army, "Ven", true),
                (nation2, UnitType.Army, "Tyr", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.03. A move cuts support on move")]
    public void DATC_D_3()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "ADR"),
                (nation1, UnitType.Army, "Tri"),
                (nation2, UnitType.Army, "Ven"),
                (nation2, UnitType.Fleet, "ION"),
            ]);

        var austrianMove = units.Get("Tri").Move("Ven");
        var austrianSupport = units.Get("ADR").Support(units.Get("Tri"), "Ven");
        var italianHold = units.Get("Ven").Hold();
        var italianMove = units.Get("ION").Move("ADR");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianMove.Status.Should().Be(OrderStatus.Failure);
        austrianSupport.Status.Should().Be(OrderStatus.Failure);
        italianHold.Status.Should().Be(OrderStatus.Success);
        italianMove.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "ADR", false),
                (nation1, UnitType.Army, "Tri", false),
                (nation2, UnitType.Army, "Ven", false),
                (nation2, UnitType.Fleet, "ION", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.04. Support to hold on unit supporting a hold allowed")]
    public void DATC_D_4()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Ber"),
                (nation1, UnitType.Fleet, "Kie"),
                (nation2, UnitType.Fleet, "BAL"),
                (nation2, UnitType.Army, "Pru"),
            ]);

        var germanSupport1 = units.Get("Ber").Support(units.Get("Kie"));
        var germanSupport2 = units.Get("Kie").Support(units.Get("Ber"));
        var russianMove = units.Get("Pru").Move("Ber");
        var russianSupport = units.Get("BAL").Support(units.Get("Pru"), "Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanSupport1.Status.Should().Be(OrderStatus.Failure);
        germanSupport2.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Ber", false),
                (nation1, UnitType.Fleet, "Kie", false),
                (nation2, UnitType.Fleet, "BAL", false),
                (nation2, UnitType.Army, "Pru", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.05. Support to hold on unit supporting a move allowed")]
    public void DATC_D_5()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Ber"),
                (nation1, UnitType.Fleet, "Kie"),
                (nation1, UnitType.Army, "Mun"),
                (nation4, UnitType.Fleet, "BAL"),
                (nation4, UnitType.Army, "Pru"),
            ]);

        var germanMove = units.Get("Mun").Move("Sil");
        var germanSupport1 = units.Get("Ber").Support(units.Get("Mun"), "Sil");
        var germanSupport2 = units.Get("Kie").Support(units.Get("Ber"));
        var russianMove = units.Get("Pru").Move("Ber");
        var russianSupport = units.Get("BAL").Support(units.Get("Pru"), "Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove.Status.Should().Be(OrderStatus.Success);
        germanSupport1.Status.Should().Be(OrderStatus.Failure);
        germanSupport2.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Ber", false),
                (nation1, UnitType.Fleet, "Kie", false),
                (nation1, UnitType.Army, "Sil", false),
                (nation4, UnitType.Fleet, "BAL", false),
                (nation4, UnitType.Army, "Pru", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.06. Support to hold on convoying unit allowed")]
    public void DATC_D_6()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Ber"),
                (nation1, UnitType.Fleet, "BAL"),
                (nation1, UnitType.Fleet, "Pru"),
                (nation4, UnitType.Fleet, "Lvn"),
                (nation4, UnitType.Fleet, "BOT"),
            ]);

        var germanMove = units.Get("Ber").Move("Swe");
        var germanConvoy = units.Get("BAL").Convoy(units.Get("Ber"), "Swe");
        var germanSupport = units.Get("Pru").Support(units.Get("BAL"));
        var russianMove = units.Get("Lvn").Move("BAL");
        var russianSupport = units.Get("BOT").Support(units.Get("Lvn"), "BAL");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove.Status.Should().Be(OrderStatus.Success);
        germanConvoy.Status.Should().Be(OrderStatus.Success);
        germanSupport.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Swe", false),
                (nation1, UnitType.Fleet, "BAL", false),
                (nation1, UnitType.Fleet, "Pru", false),
                (nation4, UnitType.Fleet, "Lvn", false),
                (nation4, UnitType.Fleet, "BOT", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.07. Support to hold on moving unit not allowed")]
    public void DATC_D_7()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "BAL"),
                (nation1, UnitType.Fleet, "Pru"),
                (nation4, UnitType.Fleet, "Lvn"),
                (nation4, UnitType.Fleet, "BOT"),
                (nation4, UnitType.Army, "Fin"),
            ]);

        var germanMove = units.Get("BAL").Move("Swe");
        var germanSupport = units.Get("Pru").Support(units.Get("BAL"));
        var russianMove1 = units.Get("Lvn").Move("BAL");
        var russianMove2 = units.Get("Fin").Move("Swe");
        var russianSupport = units.Get("BOT").Support(units.Get("Lvn"), "BAL");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Invalid);
        russianMove1.Status.Should().Be(OrderStatus.Success);
        russianMove2.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "BAL", true),
                (nation1, UnitType.Fleet, "Pru", false),
                (nation4, UnitType.Fleet, "Lvn", false),
                (nation4, UnitType.Fleet, "BOT", false),
                (nation4, UnitType.Army, "Fin", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.08. Failed convoy cannot receive hold support")]
    public void DATC_D_8()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "ION"),
                (nation1, UnitType.Army, "Ser"),
                (nation1, UnitType.Army, "Alb"),
                (nation2, UnitType.Army, "Gre"),
                (nation2, UnitType.Army, "Bul"),
            ]);

        var austrianHold = units.Get("ION").Hold();
        var austrianMove = units.Get("Alb").Move("Gre");
        var austrianSupport = units.Get("Ser").Support(units.Get("Alb"), "Gre");
        var turkishMove = units.Get("Gre").Move("Nap");
        var turkishSupport = units.Get("Bul").Support(units.Get("Gre"));

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianHold.Status.Should().Be(OrderStatus.Success);
        austrianMove.Status.Should().Be(OrderStatus.Success);
        austrianSupport.Status.Should().Be(OrderStatus.Success);
        turkishMove.Status.Should().Be(OrderStatus.Invalid);
        turkishSupport.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "ION", false),
                (nation1, UnitType.Army, "Ser", false),
                (nation1, UnitType.Army, "Gre", false),
                (nation2, UnitType.Army, "Bul", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.09. Support to move on holding unit not allowed")]
    public void DATC_D_9()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation2, UnitType.Army, "Ven"),
                (nation2, UnitType.Army, "Tyr"),
                (nation1, UnitType.Army, "Alb"),
                (nation1, UnitType.Army, "Tri"),
            ]);

        var italianMove = units.Get("Ven").Move("Tri");
        var italianSupport = units.Get("Tyr").Support(units.Get("Ven"), "Tri");
        var austrianHold = units.Get("Tri").Hold();
        var austrianSupport = units.Get("Alb").Support(units.Get("Tri"), "Ser");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        italianMove.Status.Should().Be(OrderStatus.Success);
        italianSupport.Status.Should().Be(OrderStatus.Success);
        austrianHold.Status.Should().Be(OrderStatus.Failure);
        austrianSupport.Status.Should().Be(OrderStatus.Invalid);

        board.ShouldHaveUnits(
            [
                (nation2, UnitType.Army, "Ven", false),
                (nation2, UnitType.Army, "Tyr", false),
                (nation1, UnitType.Army, "Alb", false),
                (nation1, UnitType.Army, "Tri", true),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.10. Self dislodgement prohibited")]
    public void DATC_D_10()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Ber"),
                (nation1, UnitType.Fleet, "Kie"),
                (nation1, UnitType.Army, "Mun"),
            ]);

        var hold = units.Get("Ber").Hold();
        var move = units.Get("Kie").Move("Ber");
        var support = units.Get("Mun").Support(units.Get("Kie"), "Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        hold.Status.Should().Be(OrderStatus.Success);
        move.Status.Should().Be(OrderStatus.Failure);
        support.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Ber", false),
                (nation1, UnitType.Fleet, "Kie", false),
                (nation1, UnitType.Army, "Mun", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.11. No self dislodgement of returning unit")]
    public void DATC_D_11()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Ber"),
                (nation1, UnitType.Fleet, "Kie"),
                (nation1, UnitType.Army, "Mun"),
                (nation4, UnitType.Army, "War"),
            ]);

        var germanMove1 = units.Get("Ber").Move("Pru");
        var germanMove2 = units.Get("Kie").Move("Ber");
        var germanSupport = units.Get("Mun").Support(units.Get("Kie"), "Ber");
        var russianMove = units.Get("War").Move("Pru");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove1.Status.Should().Be(OrderStatus.Failure);
        germanMove2.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Ber", false),
                (nation1, UnitType.Fleet, "Kie", false),
                (nation1, UnitType.Army, "Mun", false),
                (nation4, UnitType.Army, "War", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.12. Supporting a foreign unit to dislodge own unit prohibited")]
    public void DATC_D_12()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Tri"),
                (nation1, UnitType.Army, "Vie"),
                (nation2, UnitType.Army, "Ven"),
            ]);

        var austrianHold = units.Get("Tri").Hold();
        var austrianSupport = units.Get("Vie").Support(units.Get("Ven"), "Tri");
        var italianMove = units.Get("Ven").Move("Tri");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianHold.Status.Should().Be(OrderStatus.Success);
        austrianSupport.Status.Should().Be(OrderStatus.Failure);
        italianMove.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Tri", false),
                (nation1, UnitType.Army, "Vie", false),
                (nation2, UnitType.Army, "Ven", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.13. Supporting a foreign unit to dislodge a returning own unit prohibited")]
    public void DATC_D_13()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Tri"),
                (nation1, UnitType.Army, "Vie"),
                (nation2, UnitType.Army, "Ven"),
                (nation2, UnitType.Fleet, "Apu"),
            ]);

        var austrianMove = units.Get("Tri").Move("ADR");
        var austrianSupport = units.Get("Vie").Support(units.Get("Ven"), "Tri");
        var italianMove1 = units.Get("Ven").Move("Tri");
        var italianMove2 = units.Get("Apu").Move("ADR");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianMove.Status.Should().Be(OrderStatus.Failure);
        austrianSupport.Status.Should().Be(OrderStatus.Failure);
        italianMove1.Status.Should().Be(OrderStatus.Failure);
        italianMove2.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Tri", false),
                (nation1, UnitType.Army, "Vie", false),
                (nation2, UnitType.Army, "Ven", false),
                (nation2, UnitType.Fleet, "Apu", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.14. Supporting a foreign unit is not enough to prevent dislodgement")]
    public void DATC_D_14()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Tri"),
                (nation1, UnitType.Army, "Vie"),
                (nation2, UnitType.Army, "Ven"),
                (nation2, UnitType.Army, "Tyr"),
                (nation2, UnitType.Fleet, "ADR"),
            ]);

        var austrianHold = units.Get("Tri").Hold();
        var austrianSupport = units.Get("Vie").Support(units.Get("Ven"), "Tri");
        var italianMove = units.Get("Ven").Move("Tri");
        var italianSupport1 = units.Get("Tyr").Support(units.Get("Ven"), "Tri");
        var italianSupport2 = units.Get("ADR").Support(units.Get("Ven"), "Tri");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianHold.Status.Should().Be(OrderStatus.Failure);
        austrianSupport.Status.Should().Be(OrderStatus.Failure);
        italianMove.Status.Should().Be(OrderStatus.Success);
        italianSupport1.Status.Should().Be(OrderStatus.Success);
        italianSupport2.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Tri", true),
                (nation1, UnitType.Army, "Vie", false),
                (nation2, UnitType.Army, "Ven", false),
                (nation2, UnitType.Army, "Tyr", false),
                (nation2, UnitType.Fleet, "ADR", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.15. Defender cannot cut support for attack on itself")]
    public void DATC_D_15()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation4, UnitType.Fleet, "Con"),
                (nation4, UnitType.Fleet, "BLA"),
                (nation1, UnitType.Fleet, "Ank"),
            ]);

        var russianMove = units.Get("BLA").Move("Ank");
        var russianSupport = units.Get("Con").Support(units.Get("BLA"), "Ank");
        var turkishMove = units.Get("Ank").Move("Con");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        russianMove.Status.Should().Be(OrderStatus.Success);
        russianSupport.Status.Should().Be(OrderStatus.Success);
        turkishMove.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation4, UnitType.Fleet, "Con", false),
                (nation4, UnitType.Fleet, "BLA", false),
                (nation1, UnitType.Fleet, "Ank", true),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.16. Convoying a unit dislodging a unit of same power is allowed")]
    public void DATC_D_16()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Lon"),
                (nation1, UnitType.Fleet, "NTH"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation2, UnitType.Army, "Bel"),
            ]);

        var englishHold = units.Get("Lon").Hold();
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Bel"), "Lon");
        var frenchMove = units.Get("Bel").Move("Lon");
        var frenchSupport = units.Get("ENG").Support(units.Get("Bel"), "Lon");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishHold.Status.Should().Be(OrderStatus.Failure);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Success);
        frenchSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Lon", true),
                (nation1, UnitType.Fleet, "NTH", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation2, UnitType.Army, "Bel", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.17. Dislodgement cuts support")]
    public void DATC_D_17()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation4, UnitType.Fleet, "Con"),
                (nation4, UnitType.Fleet, "BLA"),
                (nation1, UnitType.Fleet, "Ank"),
                (nation1, UnitType.Army, "Smy"),
                (nation1, UnitType.Army, "Arm"),
            ]);

        var russianMove = units.Get("BLA").Move("Ank");
        var russianSupport = units.Get("Con").Support(units.Get("BLA"), "Ank");
        var turkishMove1 = units.Get("Ank").Move("Con");
        var turkishMove2 = units.Get("Arm").Move("Ank");
        var turkishSupport = units.Get("Smy").Support(units.Get("Ank"), "Con");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);
        turkishMove1.Status.Should().Be(OrderStatus.Success);
        turkishMove2.Status.Should().Be(OrderStatus.Failure);
        turkishSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation4, UnitType.Fleet, "Con", true),
                (nation4, UnitType.Fleet, "BLA", false),
                (nation1, UnitType.Fleet, "Ank", false),
                (nation1, UnitType.Army, "Smy", false),
                (nation1, UnitType.Army, "Arm", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.18. A surviving unit will sustain support")]
    public void DATC_D_18()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation4, UnitType.Fleet, "Con"),
                (nation4, UnitType.Fleet, "BLA"),
                (nation4, UnitType.Army, "Bul"),
                (nation3, UnitType.Fleet, "Ank"),
                (nation3, UnitType.Army, "Smy"),
                (nation3, UnitType.Army, "Arm"),
            ]);

        var russianMove = units.Get("BLA").Move("Ank");
        var russianSupport1 = units.Get("Con").Support(units.Get("BLA"), "Ank");
        var russianSupport2 = units.Get("Bul").Support(units.Get("Con"));
        var turkishMove1 = units.Get("Ank").Move("Con");
        var turkishMove2 = units.Get("Arm").Move("Ank");
        var turkishSupport = units.Get("Smy").Support(units.Get("Ank"), "Con");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        russianMove.Status.Should().Be(OrderStatus.Success);
        russianSupport1.Status.Should().Be(OrderStatus.Success);
        russianSupport2.Status.Should().Be(OrderStatus.Success);
        turkishMove1.Status.Should().Be(OrderStatus.Failure);
        turkishMove2.Status.Should().Be(OrderStatus.Failure);
        turkishSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation4, UnitType.Fleet, "Con", false),
                (nation4, UnitType.Fleet, "Ank", false),
                (nation4, UnitType.Army, "Bul", false),
                (nation3, UnitType.Army, "Smy", false),
                (nation3, UnitType.Army, "Arm", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.19. Even when surviving is in alternative way")]
    public void DATC_D_19()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Con"),
                (nation1, UnitType.Fleet, "BLA"),
                (nation1, UnitType.Army, "Smy"),
                (nation2, UnitType.Fleet, "Ank"),
            ]);

        var russianMove = units.Get("BLA").Move("Ank");
        var russianSupport1 = units.Get("Con").Support(units.Get("BLA"), "Ank");
        var russianSupport2 = units.Get("Smy").Support(units.Get("Ank"), "Con");
        var turkishMove = units.Get("Ank").Move("Con");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        russianMove.Status.Should().Be(OrderStatus.Success);
        russianSupport1.Status.Should().Be(OrderStatus.Success);
        russianSupport2.Status.Should().Be(OrderStatus.Failure);
        turkishMove.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Con", false),
                (nation1, UnitType.Fleet, "BLA", false),
                (nation1, UnitType.Army, "Smy", false),
                (nation2, UnitType.Fleet, "Ank", true),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.20. Unit cannot cut support of its own country")]
    public void DATC_D_20()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Lon"),
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Yor"),
                (nation2, UnitType.Fleet, "ENG"),
            ]);

        var englishMove1 = units.Get("NTH").Move("ENG");
        var englishMove2 = units.Get("Yor").Move("Lon");
        var englishSupport = units.Get("Lon").Support(units.Get("NTH"), "ENG");
        var frenchHold = units.Get("ENG").Hold();

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove1.Status.Should().Be(OrderStatus.Success);
        englishMove2.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Success);
        frenchHold.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Lon", false),
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Army, "Yor", false),
                (nation2, UnitType.Fleet, "ENG", true),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.21. Dislodging does not cancel a support cut")]
    public void DATC_D_21()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Tri"),
                (nation2, UnitType.Army, "Ven"),
                (nation2, UnitType.Army, "Tyr"),
                (nation3, UnitType.Army, "Mun"),
                (nation4, UnitType.Army, "Sil"),
                (nation4, UnitType.Army, "Ber"),
            ]);

        var austrianHold = units.Get("Tri").Hold();
        var italianMove = units.Get("Ven").Move("Tri");
        var italianSupport = units.Get("Tyr").Support(units.Get("Ven"), "Tri");
        var germanMove = units.Get("Mun").Move("Tyr");
        var russianMove = units.Get("Sil").Move("Mun");
        var russianSupport = units.Get("Ber").Support(units.Get("Sil"), "Mun");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianHold.Status.Should().Be(OrderStatus.Success);
        italianMove.Status.Should().Be(OrderStatus.Failure);
        italianSupport.Status.Should().Be(OrderStatus.Failure);
        germanMove.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Success);
        russianSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Tri", false),
                (nation2, UnitType.Army, "Ven", false),
                (nation2, UnitType.Army, "Tyr", false),
                (nation3, UnitType.Army, "Mun", true),
                (nation4, UnitType.Army, "Sil", false),
                (nation4, UnitType.Army, "Ber", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.22. Impossible fleet move cannot be supported")]
    public void DATC_D_22()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Kie"),
                (nation1, UnitType.Army, "Bur"),
                (nation4, UnitType.Army, "Mun"),
                (nation4, UnitType.Army, "Ber"),
            ]);

        var germanMove = units.Get("Kie").Move("Mun");
        var germanSupport = units.Get("Bur").Support(units.Get("Kie"), "Mun");
        var russianMove = units.Get("Mun").Move("Kie");
        var russianSupport = units.Get("Ber").Support(units.Get("Mun"), "Kie");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove.Status.Should().Be(OrderStatus.Invalid);
        germanSupport.Status.Should().Be(OrderStatus.Invalid);
        russianMove.Status.Should().Be(OrderStatus.Success);
        russianSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Kie", true),
                (nation1, UnitType.Army, "Bur", false),
                (nation4, UnitType.Army, "Mun", false),
                (nation4, UnitType.Army, "Ber", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.23. Impossible coast move cannot be supported")]
    public void DATC_D_23()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation2, UnitType.Fleet, "LYO"),
                (nation2, UnitType.Fleet, "WES"),
                (nation1, UnitType.Fleet, "Spa_N"),
                (nation1, UnitType.Fleet, "Mar"),
            ]);

        var italianMove = units.Get("LYO").Move("Spa_S");
        var italianSupport = units.Get("WES").Support(units.Get("LYO"), "Spa_S");
        var frenchMove = units.Get("Spa_N").Move("LYO");
        var frenchSupport = units.Get("Mar").Support(units.Get("Spa_N"), "LYO");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        italianMove.Status.Should().Be(OrderStatus.Success);
        italianSupport.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Invalid);
        frenchSupport.Status.Should().Be(OrderStatus.Invalid);

        board.ShouldHaveUnits(
            [
                (nation2, UnitType.Fleet, "LYO", false),
                (nation2, UnitType.Fleet, "WES", false),
                (nation1, UnitType.Fleet, "Spa_N", true),
                (nation1, UnitType.Fleet, "Mar", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.24. Impossible army move cannot be supported")]
    public void DATC_D_24()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Mar"),
                (nation1, UnitType.Fleet, "Spa_S"),
                (nation2, UnitType.Fleet, "LYO"),
                (nation3, UnitType.Fleet, "TYS"),
                (nation3, UnitType.Fleet, "WES"),
            ]);

        var frenchMove = units.Get("Mar").Move("LYO");
        var frenchSupport = units.Get("Spa_S").Support(units.Get("Mar"), "LYO");
        var italianHold = units.Get("LYO").Hold();
        var turkishMove = units.Get("WES").Move("LYO");
        var turkishSupport = units.Get("TYS").Support(units.Get("WES"), "LYO");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        frenchMove.Status.Should().Be(OrderStatus.Invalid);
        frenchSupport.Status.Should().Be(OrderStatus.Invalid);
        italianHold.Status.Should().Be(OrderStatus.Failure);
        turkishMove.Status.Should().Be(OrderStatus.Success);
        turkishSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Mar", false),
                (nation1, UnitType.Fleet, "Spa_S", false),
                (nation2, UnitType.Fleet, "LYO", true),
                (nation3, UnitType.Fleet, "TYS", false),
                (nation3, UnitType.Fleet, "WES", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.25. Failing hold support can be supported")]
    public void DATC_D_25()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Ber"),
                (nation1, UnitType.Fleet, "Kie"),
                (nation4, UnitType.Fleet, "BAL"),
                (nation4, UnitType.Army, "Pru"),
            ]);

        var germanSupport1 = units.Get("Ber").Support(units.Get("Pru"));
        var germanSupport2 = units.Get("Kie").Support(units.Get("Ber"));
        var russianMove = units.Get("Pru").Move("Ber");
        var russianSupport = units.Get("BAL").Support(units.Get("Pru"), "Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanSupport1.Status.Should().Be(OrderStatus.Invalid);
        germanSupport2.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Ber", false),
                (nation1, UnitType.Fleet, "Kie", false),
                (nation4, UnitType.Fleet, "BAL", false),
                (nation4, UnitType.Army, "Pru", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.26. Failing move support can be supported")]
    public void DATC_D_26()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Ber"),
                (nation1, UnitType.Fleet, "Kie"),
                (nation2, UnitType.Fleet, "BAL"),
                (nation2, UnitType.Army, "Pru"),
            ]);

        var germanSupport1 = units.Get("Ber").Support(units.Get("Pru"), "Sil");
        var germanSupport2 = units.Get("Kie").Support(units.Get("Ber"));
        var russianMove = units.Get("Pru").Move("Ber");
        var russianSupport = units.Get("BAL").Support(units.Get("Pru"), "Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanSupport1.Status.Should().Be(OrderStatus.Invalid);
        germanSupport2.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Ber", false),
                (nation1, UnitType.Fleet, "Kie", false),
                (nation2, UnitType.Fleet, "BAL", false),
                (nation2, UnitType.Army, "Pru", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.27. Failing convoy can be supported")]
    public void DATC_D_27()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Swe"),
                (nation1, UnitType.Fleet, "Den"),
                (nation2, UnitType.Army, "Ber"),
                (nation3, UnitType.Fleet, "BAL"),
                (nation3, UnitType.Fleet, "Pru"),
            ]);

        var englishMove = units.Get("Swe").Move("BAL");
        var englishSupport = units.Get("Den").Support(units.Get("Swe"), "BAL");
        var germanHold = units.Get("Ber").Hold();
        var russianConvoy = units.Get("BAL").Convoy(units.Get("Ber"), "Lvn");
        var russianSupport = units.Get("Pru").Support(units.Get("BAL"));

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        germanHold.Status.Should().Be(OrderStatus.Success);
        russianConvoy.Status.Should().Be(OrderStatus.Invalid);
        russianSupport.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Swe", false),
                (nation1, UnitType.Fleet, "Den", false),
                (nation2, UnitType.Army, "Ber", false),
                (nation3, UnitType.Fleet, "BAL", false),
                (nation3, UnitType.Fleet, "Pru", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.28. Impossible move and support")]
    public void DATC_D_28()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Bud"),
                (nation2, UnitType.Fleet, "Rum"),
                (nation3, UnitType.Fleet, "BLA"),
                (nation3, UnitType.Army, "Bul"),
            ]);

        var austrianSupport = units.Get("Bud").Support(units.Get("Rum"));
        var russianMove = units.Get("Rum").Move("Hol");
        var turkishMove = units.Get("BLA").Move("Rum");
        var turkishSupport = units.Get("Bul").Support(units.Get("BLA"), "Rum");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianSupport.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Invalid);
        turkishMove.Status.Should().Be(OrderStatus.Failure);
        turkishSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Bud", false),
                (nation2, UnitType.Fleet, "Rum", false),
                (nation3, UnitType.Fleet, "BLA", false),
                (nation3, UnitType.Army, "Bul", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.29. Move to impossible coast and support")]
    public void DATC_D_29()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Bud"),
                (nation2, UnitType.Fleet, "Rum"),
                (nation3, UnitType.Fleet, "BLA"),
                (nation3, UnitType.Army, "Bul"),
            ]);

        var austrianSupport = units.Get("Bud").Support(units.Get("Rum"));
        var russianMove = units.Get("Rum").Move("Bul_S");
        var turkishMove = units.Get("BLA").Move("Rum");
        var turkishSupport = units.Get("Bul").Support(units.Get("BLA"), "Rum");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianSupport.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Invalid);
        turkishMove.Status.Should().Be(OrderStatus.Failure);
        turkishSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Bud", false),
                (nation2, UnitType.Fleet, "Rum", false),
                (nation3, UnitType.Fleet, "BLA", false),
                (nation3, UnitType.Army, "Bul", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.30. Move without coast and support")]
    public void DATC_D_30()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation2, UnitType.Fleet, "AEG"),
                (nation1, UnitType.Fleet, "Con"),
                (nation3, UnitType.Fleet, "BLA"),
                (nation3, UnitType.Army, "Bul"),
            ]);

        var italianSupport = units.Get("AEG").Support(units.Get("Con"));
        var russianMove = units.Get("Con").Move("Bul");
        var turkishMove = units.Get("BLA").Move("Con");
        var turkishSupport = units.Get("Bul").Support(units.Get("BLA"), "Con");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        italianSupport.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Invalid);
        turkishMove.Status.Should().Be(OrderStatus.Failure);
        turkishSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation2, UnitType.Fleet, "AEG", false),
                (nation1, UnitType.Fleet, "Con", false),
                (nation3, UnitType.Fleet, "BLA", false),
                (nation3, UnitType.Army, "Bul", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.31. A tricky impossible scenario")]
    public void DATC_D_31()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Rum"),
                (nation2, UnitType.Fleet, "BLA"),
            ]);

        var austrianMove = units.Get("Rum").Move("Arm");
        var turkishSupport = units.Get("BLA").Support(units.Get("Rum"), "Arm");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianMove.Status.Should().Be(OrderStatus.Invalid);
        turkishSupport.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Rum", false),
                (nation2, UnitType.Fleet, "BLA", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.32. A missing fleet")]
    public void DATC_D_32()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Edi"),
                (nation1, UnitType.Army, "Lvp"),
                (nation2, UnitType.Fleet, "Lon"),
                (nation3, UnitType.Army, "Yor"),
            ]);

        var englishMove = units.Get("Lvp").Move("Yor");
        var englishSupport = units.Get("Edi").Support(units.Get("Lvp"), "Yor");
        var frenchSupport = units.Get("Lon").Support(units.Get("Yor"));
        var germanMove = units.Get("Yor").Move("Hol");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        frenchSupport.Status.Should().Be(OrderStatus.Success);
        germanMove.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Edi", false),
                (nation1, UnitType.Army, "Lvp", false),
                (nation2, UnitType.Fleet, "Lon", false),
                (nation3, UnitType.Army, "Yor", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.33. Unwanted support allowed")]
    public void DATC_D_33()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Ser"),
                (nation1, UnitType.Army, "Vie"),
                (nation2, UnitType.Army, "Gal"),
                (nation3, UnitType.Army, "Bul"),
            ]);

        var austrianMove1 = units.Get("Ser").Move("Bud");
        var austrianMove2 = units.Get("Vie").Move("Bud");
        var russianSupport = units.Get("Gal").Support(units.Get("Ser"), "Bud");
        var turkishMove = units.Get("Bul").Move("Ser");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianMove1.Status.Should().Be(OrderStatus.Success);
        austrianMove2.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Success);
        turkishMove.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Bud", false),
                (nation1, UnitType.Army, "Vie", false),
                (nation2, UnitType.Army, "Gal", false),
                (nation3, UnitType.Army, "Ser", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC D.34. Support targeting own area not allowed")]
    public void DATC_D_34()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Ber"),
                (nation1, UnitType.Army, "Sil"),
                (nation1, UnitType.Fleet, "BAL"),
                (nation2, UnitType.Army, "Pru"),
                (nation3, UnitType.Army, "War"),
                (nation3, UnitType.Army, "Lvn"),
            ]);

        var germanMove = units.Get("Ber").Move("Pru");
        var germanSupport1 = units.Get("Sil").Support(units.Get("Ber"), "Pru");
        var germanSupport2 = units.Get("BAL").Support(units.Get("Ber"), "Pru");
        var italianSupport = units.Get("Pru").Support(units.Get("Lvn"), "Pru");
        var russianMove = units.Get("Lvn").Move("Pru");
        var russianSupport = units.Get("War").Support(units.Get("Lvn"), "Pru");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove.Status.Should().Be(OrderStatus.Success);
        germanSupport1.Status.Should().Be(OrderStatus.Success);
        germanSupport2.Status.Should().Be(OrderStatus.Success);
        italianSupport.Status.Should().Be(OrderStatus.Invalid);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Pru", false),
                (nation1, UnitType.Army, "Sil", false),
                (nation1, UnitType.Fleet, "BAL", false),
                (nation3, UnitType.Army, "War", false),
                (nation3, UnitType.Army, "Lvn", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }
}
