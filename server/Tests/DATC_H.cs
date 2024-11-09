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
public class DATC_H : AdjudicationTestBase
{
    [Fact(DisplayName = "DATC H.01. No supports during retreat")]
    public void DATC_H_1()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation5, UnitType.Fleet, "Tri"),
                (nation5, UnitType.Army, "Ser"),
                (nation5, UnitType.Fleet, "Gre"),
            ]);

        units.Get("Tri").MustRetreat = true;
        units.Get("Gre").MustRetreat = true;

        var austrianMove = units.Get("Tri").Move("Alb");
        var austrianSupport = units.Get("Ser").Support(units.Get("Tri"), "Alb");
        var turkishMove = units.Get("Gre").Move("Alb");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianMove.Status.Should().Be(OrderStatus.RetreatFailure);
        austrianSupport.Status.Should().Be(OrderStatus.Invalid);
        turkishMove.Status.Should().Be(OrderStatus.RetreatFailure);

        board.Next().ShouldHaveUnits(
            [
                (nation5, UnitType.Army, "Ser", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.02. No supports from retreating unit")]
    public void DATC_H_2()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Nwy"),
                (nation4, UnitType.Fleet, "Edi"),
                (nation4, UnitType.Fleet, "Hol"),
            ]);

        units.Get("Nwy").MustRetreat = true;
        units.Get("Edi").MustRetreat = true;
        units.Get("Hol").MustRetreat = true;

        var englishMove = units.Get("Nwy").Move("NTH");
        var russianMove = units.Get("Edi").Move("NTH");
        var russianSupport = units.Get("Hol").Support(units.Get("Edi"), "NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.RetreatFailure);
        russianMove.Status.Should().Be(OrderStatus.RetreatFailure);
        russianSupport.Status.Should().Be(OrderStatus.RetreatInvalid);

        board.Next().ShouldHaveUnits([]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.03. No convoy during retreat")]
    public void DATC_H_3()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Hol"),
                (nation1, UnitType.Fleet, "NTH"),
            ]);

        units.Get("Hol").MustRetreat = true;

        var englishMove = units.Get("Hol").Move("Yor");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Hol"), "Yor");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.RetreatInvalid);
        englishConvoy.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.04. No other moves during retreat")]
    public void DATC_H_4()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Hol"),
                (nation1, UnitType.Fleet, "NTH"),
            ]);

        units.Get("Hol").MustRetreat = true;

        var englishMove1 = units.Get("Hol").Move("Bel");
        var englishMove2 = units.Get("NTH").Move("NWG");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove1.Status.Should().Be(OrderStatus.RetreatSuccess);
        englishMove2.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Bel", false),
                (nation1, UnitType.Fleet, "NTH", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.05. A unit may not retreat to the area from which it is attacked")]
    public void DATC_H_5()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation4, UnitType.Fleet, "Con"),
                (nation4, UnitType.Fleet, "BLA"),
                (nation5, UnitType.Fleet, "Ank"),
            ]);

        units.Get("BLA").Move("Ank", status: OrderStatus.Success);
        units.Get("Con").Support(units.Get("BLA"), "Ank", status: OrderStatus.Success);
        units.Get("Ank").Hold(status: OrderStatus.Failure);
        units.Get("Ank").MustRetreat = true;

        var move = units.Get("Ank").Move("BLA");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move.Status.Should().Be(OrderStatus.RetreatFailure);

        board.Next().ShouldHaveUnits(
            [
                (nation4, UnitType.Fleet, "Con", false),
                (nation4, UnitType.Fleet, "Ank", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.06. Unit may not retreat to a contested area")]
    public void DATC_H_6()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation5, UnitType.Army, "Bud"),
                (nation5, UnitType.Army, "Tri"),
                (nation3, UnitType.Army, "Mun"),
                (nation3, UnitType.Army, "Sil"),
                (nation1, UnitType.Army, "Vie"),
            ]);

        units.Get("Tri").Move("Vie", status: OrderStatus.Success);
        units.Get("Bud").Support(units.Get("Tri"), "Vie", status: OrderStatus.Success);
        units.Get("Mun").Move("Boh", status: OrderStatus.Failure);
        units.Get("Sil").Move("Boh", status: OrderStatus.Failure);
        units.Get("Vie").Hold(status: OrderStatus.Failure);
        units.Get("Vie").MustRetreat = true;

        var move = units.Get("Vie").Move("Boh");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move.Status.Should().Be(OrderStatus.RetreatFailure);

        board.Next().ShouldHaveUnits(
            [
                (nation5, UnitType.Army, "Bud", false),
                (nation5, UnitType.Army, "Vie", false),
                (nation3, UnitType.Army, "Mun", false),
                (nation3, UnitType.Army, "Sil", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.07. Multiple reteat to same area will disband units")]
    public void DATC_H_7()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Boh"),
                (nation1, UnitType.Army, "Vie"),
            ]);

        units.Get("Boh").MustRetreat = true;
        units.Get("Vie").MustRetreat = true;

        var move1 = units.Get("Boh").Move("Tyr");
        var move2 = units.Get("Vie").Move("Tyr");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move1.Status.Should().Be(OrderStatus.RetreatFailure);
        move2.Status.Should().Be(OrderStatus.RetreatFailure);

        board.Next().ShouldHaveUnits([]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.08. Triple retreat to same area will disband units")]
    public void DATC_H_8()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Nwy"),
                (nation4, UnitType.Fleet, "Edi"),
                (nation4, UnitType.Fleet, "Hol"),
            ]);

        units.Get("Nwy").MustRetreat = true;
        units.Get("Edi").MustRetreat = true;
        units.Get("Hol").MustRetreat = true;

        var englishMove = units.Get("Nwy").Move("NTH");
        var russianMove1 = units.Get("Edi").Move("NTH");
        var russianMove2 = units.Get("Hol").Move("NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.RetreatFailure);
        russianMove1.Status.Should().Be(OrderStatus.RetreatFailure);
        russianMove2.Status.Should().Be(OrderStatus.RetreatFailure);

        board.Next().ShouldHaveUnits([]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.09. Dislodged unit will not make attacker's area contested")]
    public void DATC_H_9()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "HEL"),
                (nation1, UnitType.Fleet, "Den"),
                (nation3, UnitType.Army, "Ber"),
                (nation3, UnitType.Fleet, "Kie"),
                (nation3, UnitType.Army, "Sil"),
                (nation4, UnitType.Army, "Pru"),
            ]);

        units.Get("HEL").Move("Kie", status: OrderStatus.Success);
        units.Get("Den").Support(units.Get("HEL"), "Kie", status: OrderStatus.Success);
        units.Get("Kie").Hold(status: OrderStatus.Failure);
        units.Get("Ber").Move("Pru", status: OrderStatus.Success);
        units.Get("Sil").Support(units.Get("Ber"), "Pru", status: OrderStatus.Success);
        units.Get("Pru").Move("Ber", status: OrderStatus.Failure);
        units.Get("Kie").MustRetreat = true;
        units.Get("Pru").MustRetreat = true;

        var move = units.Get("Kie").Move("Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move.Status.Should().Be(OrderStatus.RetreatSuccess);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Kie", false),
                (nation1, UnitType.Fleet, "Den", false),
                (nation3, UnitType.Army, "Pru", false),
                (nation3, UnitType.Fleet, "Ber", false),
                (nation3, UnitType.Army, "Sil", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.10. Not retreating to attacker does not mean contested")]
    public void DATC_H_10()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Kie"),
                (nation3, UnitType.Army, "Ber"),
                (nation3, UnitType.Army, "Mun"),
                (nation3, UnitType.Army, "Pru"),
                (nation4, UnitType.Army, "War"),
                (nation4, UnitType.Army, "Sil"),
            ]);

        units.Get("Kie").Hold(status: OrderStatus.Failure);
        units.Get("Pru").Hold(status: OrderStatus.Failure);
        units.Get("Ber").Move("Kie", status: OrderStatus.Success);
        units.Get("Mun").Support(units.Get("Ber"), "Kie", status: OrderStatus.Success);
        units.Get("War").Move("Pru", status: OrderStatus.Success);
        units.Get("Sil").Support(units.Get("War"), "Pru", status: OrderStatus.Success);
        units.Get("Kie").MustRetreat = true;
        units.Get("Pru").MustRetreat = true;

        var englishMove = units.Get("Kie").Move("Ber");
        var germanMove = units.Get("Pru").Move("Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.RetreatFailure);
        germanMove.Status.Should().Be(OrderStatus.RetreatSuccess);

        board.Next().ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Kie", false),
                (nation3, UnitType.Army, "Mun", false),
                (nation3, UnitType.Army, "Ber", false),
                (nation4, UnitType.Army, "Pru", false),
                (nation4, UnitType.Army, "Sil", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.11. Retreat when dislodged by adjacent convoy")]
    public void DATC_H_11()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation2, UnitType.Army, "Gas"),
                (nation2, UnitType.Army, "Bur"),
                (nation2, UnitType.Fleet, "MAO"),
                (nation2, UnitType.Fleet, "WES"),
                (nation2, UnitType.Fleet, "LYO"),
                (nation1, UnitType.Army, "Mar"),
            ]);

        var frenchMove = units.Get("Gas").Move("Mar", status: OrderStatus.Success);
        units.Get("Bur").Support(units.Get("Gas"), "Mar", status: OrderStatus.Success);
        var frenchConvoy1 = units.Get("MAO").Convoy(units.Get("Gas"), "Mar", status: OrderStatus.Success);
        var frenchConvoy2 = units.Get("WES").Convoy(units.Get("Gas"), "Mar", status: OrderStatus.Success);
        var frenchConvoy3 = units.Get("LYO").Convoy(units.Get("Gas"), "Mar", status: OrderStatus.Success);
        units.Get("Mar").Hold(status: OrderStatus.Failure);
        frenchMove.ConvoyPath = [frenchConvoy1, frenchConvoy2, frenchConvoy3];
        units.Get("Mar").MustRetreat = true;

        var italianMove = units.Get("Mar").Move("Gas");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        italianMove.Status.Should().Be(OrderStatus.RetreatSuccess);

        board.Next().ShouldHaveUnits(
            [
                (nation2, UnitType.Army, "Mar", false),
                (nation2, UnitType.Army, "Bur", false),
                (nation2, UnitType.Fleet, "MAO", false),
                (nation2, UnitType.Fleet, "WES", false),
                (nation2, UnitType.Fleet, "LYO", false),
                (nation1, UnitType.Army, "Gas", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.12. Retreat when dislodged by adjacent convoy while trying to do the same")]
    public void DATC_H_12()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Lvp"),
                (nation1, UnitType.Fleet, "IRI"),
                (nation1, UnitType.Fleet, "ENG"),
                (nation1, UnitType.Fleet, "NTH"),
                (nation2, UnitType.Fleet, "Bre"),
                (nation2, UnitType.Fleet, "MAO"),
                (nation4, UnitType.Army, "Edi"),
                (nation4, UnitType.Fleet, "NWG"),
                (nation4, UnitType.Fleet, "NAO"),
                (nation4, UnitType.Army, "Cly"),
            ]);

        units.Get("Lvp").Move("Edi", status: OrderStatus.Failure);
        units.Get("IRI").Convoy(units.Get("Lvp"), "Edi", status: OrderStatus.Failure);
        units.Get("ENG").Convoy(units.Get("Lvp"), "Edi", status: OrderStatus.Failure);
        units.Get("NTH").Convoy(units.Get("Lvp"), "Edi", status: OrderStatus.Failure);
        units.Get("Bre").Move("ENG", status: OrderStatus.Success);
        units.Get("MAO").Support(units.Get("Bre"), "ENG", status: OrderStatus.Success);
        var russianMove = units.Get("Edi").Move("Lvp", status: OrderStatus.Success);
        var russianConvoy1 = units.Get("NWG").Convoy(units.Get("Edi"), "Lvp", status: OrderStatus.Success);
        var russianConvoy2 = units.Get("NAO").Convoy(units.Get("Edi"), "Lvp", status: OrderStatus.Success);
        units.Get("Cly").Support(units.Get("Edi"), "Lvp", status: OrderStatus.Success);
        russianMove.ConvoyPath = [russianConvoy1, russianConvoy2];
        units.Get("Lvp").MustRetreat = true;
        units.Get("ENG").MustRetreat = true;

        var englishMove = units.Get("Lvp").Move("Edi");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.RetreatSuccess);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Edi", false),
                (nation1, UnitType.Fleet, "IRI", false),
                (nation1, UnitType.Fleet, "NTH", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation2, UnitType.Fleet, "MAO", false),
                (nation4, UnitType.Army, "Lvp", false),
                (nation4, UnitType.Fleet, "NWG", false),
                (nation4, UnitType.Fleet, "NAO", false),
                (nation4, UnitType.Army, "Cly", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.13. No retreat with convoy in movement phase")]
    public void DATC_H_13()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Pic"),
                (nation1, UnitType.Fleet, "ENG"),
            ]);

        units.Get("Pic").Hold(status: OrderStatus.Failure);
        units.Get("ENG").Convoy(units.Get("Pic"), "Lon", status: OrderStatus.Invalid);
        units.Get("Pic").MustRetreat = true;

        var move = units.Get("Pic").Move("Lon");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move.Status.Should().Be(OrderStatus.RetreatInvalid);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "ENG", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.14. No retreat with support in movement phase")]
    public void DATC_H_14()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Pic"),
                (nation1, UnitType.Fleet, "ENG"),
                (nation2, UnitType.Army, "Bur"),
            ]);

        units.Get("Pic").Hold(status: OrderStatus.RetreatFailure);
        units.Get("ENG").Support(units.Get("Pic"), "Bel", status: OrderStatus.Invalid);
        units.Get("Bur").Hold(status: OrderStatus.Failure);
        units.Get("Pic").MustRetreat = true;
        units.Get("Bur").MustRetreat = true;

        var englishMove = units.Get("Pic").Move("Bel");
        var frenchMove = units.Get("Bur").Move("Bel");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.RetreatFailure);
        frenchMove.Status.Should().Be(OrderStatus.RetreatFailure);

        board.Next().ShouldHaveUnits(
            [
                 (nation1, UnitType.Fleet, "ENG", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.15. No coastal crawl in retreat")]
    public void DATC_H_15()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Por"),
                (nation2, UnitType.Fleet, "Spa_S"),
                (nation2, UnitType.Fleet, "MAO"),
            ]);

        var englishHold = units.Get("Por").Hold();
        var frenchMove = units.Get("Spa_S").Move("Por");
        var frenchSupport = units.Get("MAO").Support(units.Get("Spa_S"), "Por");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishHold.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Success);
        frenchSupport.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation2, UnitType.Fleet, "Por", false),
                (nation2, UnitType.Fleet, "MAO", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC H.16. Contested for both coasts")]
    public void DATC_H_16()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation2, UnitType.Fleet, "MAO"),
                (nation2, UnitType.Fleet, "Gas"),
                (nation2, UnitType.Fleet, "WES"),
                (nation1, UnitType.Fleet, "Tun"),
                (nation1, UnitType.Fleet, "TYS"),
            ]);

        units.Get("MAO").Move("Spa_N", status: OrderStatus.Failure);
        units.Get("Gas").Move("Spa_N", status: OrderStatus.Failure);
        units.Get("WES").Hold(status: OrderStatus.Failure);
        units.Get("TYS").Move("WES", status: OrderStatus.Success);
        units.Get("Tun").Support(units.Get("TYS"), "WES", status: OrderStatus.Success);
        units.Get("WES").MustRetreat = true;

        var move = units.Get("WES").Move("Spa_S");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move.Status.Should().Be(OrderStatus.RetreatFailure);

        board.Next().ShouldHaveUnits(
            [
                (nation2, UnitType.Fleet, "MAO", false),
                (nation2, UnitType.Fleet, "Gas", false),
                (nation1, UnitType.Fleet, "Tun", false),
                (nation1, UnitType.Fleet, "WES", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }
}
