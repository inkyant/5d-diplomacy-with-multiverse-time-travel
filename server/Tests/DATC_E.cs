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
public class DATC_E : AdjudicationTestBase
{
    [Fact(DisplayName = "DATC E.01. Dislodged unit has no effect on attacker's area")]
    public void DATC_E_1()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation3, UnitType.Army, "Ber"),
                (nation3, UnitType.Fleet, "Kie"),
                (nation3, UnitType.Army, "Sil"),
                (nation4, UnitType.Army, "Pru"),
            ]);

        var germanMove1 = units.Get("Ber").Move("Pru");
        var germanMove2 = units.Get("Kie").Move("Ber");
        var germanSupport = units.Get("Sil").Support(units.Get("Ber"), "Pru");
        var russianMove = units.Get("Pru").Move("Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove1.Status.Should().Be(OrderStatus.Success);
        germanMove2.Status.Should().Be(OrderStatus.Success);
        germanSupport.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Ber", false),
                (nation3, UnitType.Fleet, "Kie", false),
                (nation3, UnitType.Army, "Sil", false),
                (nation4, UnitType.Army, "Pru", true),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.02. No self dislodgement in head-to-head battle")]
    public void DATC_E_2()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation3, UnitType.Army, "Ber"),
                (nation3, UnitType.Fleet, "Kie"),
                (nation3, UnitType.Army, "Mun"),
            ]);

        var germanMove1 = units.Get("Ber").Move("Kie");
        var germanMove2 = units.Get("Kie").Move("Ber");
        var germanSupport = units.Get("Mun").Support(units.Get("Ber"), "Kie");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove1.Status.Should().Be(OrderStatus.Failure);
        germanMove2.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Ber", false),
                (nation3, UnitType.Fleet, "Kie", false),
                (nation3, UnitType.Army, "Mun", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.03. No help in dislodging own unit")]
    public void DATC_E_3()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation3, UnitType.Army, "Ber"),
                (nation3, UnitType.Army, "Mun"),
                (nation1, UnitType.Fleet, "Kie"),
            ]);

        var germanMove = units.Get("Ber").Move("Kie");
        var germanSupport = units.Get("Mun").Support(units.Get("Kie"), "Ber");
        var englishMove = units.Get("Kie").Move("Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);
        englishMove.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Ber", false),
                (nation3, UnitType.Army, "Mun", false),
                (nation1, UnitType.Fleet, "Kie", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.04. Non-dislodged loser still has effect")]
    public void DATC_E_4()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation3, UnitType.Fleet, "Hol"),
                (nation3, UnitType.Fleet, "HEL"),
                (nation3, UnitType.Fleet, "SKA"),
                (nation2, UnitType.Fleet, "NTH"),
                (nation2, UnitType.Fleet, "Bel"),
                (nation1, UnitType.Fleet, "Edi"),
                (nation1, UnitType.Fleet, "Yor"),
                (nation1, UnitType.Fleet, "NWG"),
                (nation5, UnitType.Army, "Kie"),
                (nation5, UnitType.Army, "Ruh"),
            ]);

        var germanMove = units.Get("Hol").Move("NTH");
        var germanSupport1 = units.Get("HEL").Support(units.Get("Hol"), "NTH");
        var germanSupport2 = units.Get("SKA").Support(units.Get("Hol"), "NTH");
        var frenchMove = units.Get("NTH").Move("Hol");
        var frenchSupport = units.Get("Bel").Support(units.Get("NTH"), "Hol");
        var englishMove = units.Get("NWG").Move("NTH");
        var englishSupport1 = units.Get("Edi").Support(units.Get("NWG"), "NTH");
        var englishSupport2 = units.Get("Yor").Support(units.Get("NWG"), "NTH");
        var austrianMove = units.Get("Ruh").Move("Hol");
        var austrianSupport = units.Get("Kie").Support(units.Get("Ruh"), "Hol");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport1.Status.Should().Be(OrderStatus.Failure);
        germanSupport2.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchSupport.Status.Should().Be(OrderStatus.Failure);
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport1.Status.Should().Be(OrderStatus.Failure);
        englishSupport2.Status.Should().Be(OrderStatus.Failure);
        austrianMove.Status.Should().Be(OrderStatus.Failure);
        austrianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation3, UnitType.Fleet, "Hol", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation3, UnitType.Fleet, "SKA", false),
                (nation2, UnitType.Fleet, "NTH", false),
                (nation2, UnitType.Fleet, "Bel", false),
                (nation1, UnitType.Fleet, "Edi", false),
                (nation1, UnitType.Fleet, "Yor", false),
                (nation1, UnitType.Fleet, "NWG", false),
                (nation5, UnitType.Army, "Kie", false),
                (nation5, UnitType.Army, "Ruh", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.05. Loser dislodged by another army still has effect")]
    public void DATC_E_5()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation3, UnitType.Fleet, "Hol"),
                (nation3, UnitType.Fleet, "HEL"),
                (nation3, UnitType.Fleet, "SKA"),
                (nation2, UnitType.Fleet, "NTH"),
                (nation2, UnitType.Fleet, "Bel"),
                (nation1, UnitType.Fleet, "Edi"),
                (nation1, UnitType.Fleet, "Yor"),
                (nation1, UnitType.Fleet, "NWG"),
                (nation1, UnitType.Fleet, "Lon"),
                (nation5, UnitType.Army, "Kie"),
                (nation5, UnitType.Army, "Ruh"),
            ]);

        var germanMove = units.Get("Hol").Move("NTH");
        var germanSupport1 = units.Get("HEL").Support(units.Get("Hol"), "NTH");
        var germanSupport2 = units.Get("SKA").Support(units.Get("Hol"), "NTH");
        var frenchMove = units.Get("NTH").Move("Hol");
        var frenchSupport = units.Get("Bel").Support(units.Get("NTH"), "Hol");
        var englishMove = units.Get("NWG").Move("NTH");
        var englishSupport1 = units.Get("Edi").Support(units.Get("NWG"), "NTH");
        var englishSupport2 = units.Get("Yor").Support(units.Get("NWG"), "NTH");
        var englishSupport3 = units.Get("Lon").Support(units.Get("NWG"), "NTH");
        var austrianMove = units.Get("Ruh").Move("Hol");
        var austrianSupport = units.Get("Kie").Support(units.Get("Ruh"), "Hol");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport1.Status.Should().Be(OrderStatus.Failure);
        germanSupport2.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchSupport.Status.Should().Be(OrderStatus.Failure);
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishSupport1.Status.Should().Be(OrderStatus.Success);
        englishSupport2.Status.Should().Be(OrderStatus.Success);
        englishSupport3.Status.Should().Be(OrderStatus.Success);
        austrianMove.Status.Should().Be(OrderStatus.Failure);
        austrianSupport.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation3, UnitType.Fleet, "Hol", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation3, UnitType.Fleet, "SKA", false),
                (nation2, UnitType.Fleet, "NTH", true),
                (nation2, UnitType.Fleet, "Bel", false),
                (nation1, UnitType.Fleet, "Edi", false),
                (nation1, UnitType.Fleet, "Yor", false),
                (nation1, UnitType.Fleet, "NWG", false),
                (nation1, UnitType.Fleet, "Lon", false),
                (nation5, UnitType.Army, "Kie", false),
                (nation5, UnitType.Army, "Ruh", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.06. Not dislodge because of own support still has effect")]
    public void DATC_E_6()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation3, UnitType.Fleet, "Hol"),
                (nation3, UnitType.Fleet, "HEL"),
                (nation2, UnitType.Fleet, "NTH"),
                (nation2, UnitType.Fleet, "Bel"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation5, UnitType.Army, "Kie"),
                (nation5, UnitType.Army, "Ruh"),
            ]);

        var germanMove = units.Get("Hol").Move("NTH");
        var germanSupport = units.Get("HEL").Support(units.Get("Hol"), "NTH");
        var frenchMove = units.Get("NTH").Move("Hol");
        var frenchSupport1 = units.Get("Bel").Support(units.Get("NTH"), "Hol");
        var frenchSupport2 = units.Get("ENG").Support(units.Get("Hol"), "NTH");
        var austrianMove = units.Get("Ruh").Move("Hol");
        var austrianSupport = units.Get("Kie").Support(units.Get("Ruh"), "Hol");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchSupport1.Status.Should().Be(OrderStatus.Failure);
        frenchSupport2.Status.Should().Be(OrderStatus.Failure);
        austrianMove.Status.Should().Be(OrderStatus.Failure);
        austrianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation3, UnitType.Fleet, "Hol", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation2, UnitType.Fleet, "NTH", false),
                (nation2, UnitType.Fleet, "Bel", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation5, UnitType.Army, "Kie", false),
                (nation5, UnitType.Army, "Ruh", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.07. No self dislodgement with beleaguered garrison")]
    public void DATC_E_7()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Fleet, "Yor"),
                (nation3, UnitType.Fleet, "Hol"),
                (nation3, UnitType.Fleet, "HEL"),
                (nation4, UnitType.Fleet, "SKA"),
                (nation4, UnitType.Fleet, "Nwy"),
            ]);

        var englishHold = units.Get("NTH").Hold();
        var englishSupport = units.Get("Yor").Support(units.Get("Nwy"), "NTH");
        var germanMove = units.Get("HEL").Move("NTH");
        var germanSupport = units.Get("Hol").Support(units.Get("HEL"), "NTH");
        var russianMove = units.Get("Nwy").Move("NTH");
        var russianSupport = units.Get("SKA").Support(units.Get("Nwy"), "NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishHold.Status.Should().Be(OrderStatus.Success);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Fleet, "Yor", false),
                (nation3, UnitType.Fleet, "Hol", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation4, UnitType.Fleet, "SKA", false),
                (nation4, UnitType.Fleet, "Nwy", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.08. No self dislodgement with beleaguered garrison and head-to-head battle")]
    public void DATC_E_8()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Fleet, "Yor"),
                (nation3, UnitType.Fleet, "Hol"),
                (nation3, UnitType.Fleet, "HEL"),
                (nation4, UnitType.Fleet, "SKA"),
                (nation4, UnitType.Fleet, "Nwy"),
            ]);

        var englishMove = units.Get("NTH").Move("Nwy");
        var englishSupport = units.Get("Yor").Support(units.Get("Nwy"), "NTH");
        var germanMove = units.Get("HEL").Move("NTH");
        var germanSupport = units.Get("Hol").Support(units.Get("HEL"), "NTH");
        var russianMove = units.Get("Nwy").Move("NTH");
        var russianSupport = units.Get("SKA").Support(units.Get("Nwy"), "NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Fleet, "Yor", false),
                (nation3, UnitType.Fleet, "Hol", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation4, UnitType.Fleet, "SKA", false),
                (nation4, UnitType.Fleet, "Nwy", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.09. Almost self dislodgement with beleaguered garrison")]
    public void DATC_E_9()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Fleet, "Yor"),
                (nation3, UnitType.Fleet, "Hol"),
                (nation3, UnitType.Fleet, "HEL"),
                (nation4, UnitType.Fleet, "SKA"),
                (nation4, UnitType.Fleet, "Nwy"),
            ]);

        var englishMove = units.Get("NTH").Move("NWG");
        var englishSupport = units.Get("Yor").Support(units.Get("Nwy"), "NTH");
        var germanMove = units.Get("HEL").Move("NTH");
        var germanSupport = units.Get("Hol").Support(units.Get("HEL"), "NTH");
        var russianMove = units.Get("Nwy").Move("NTH");
        var russianSupport = units.Get("SKA").Support(units.Get("Nwy"), "NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishSupport.Status.Should().Be(OrderStatus.Success);
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Success);
        russianSupport.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NWG", false),
                (nation1, UnitType.Fleet, "Yor", false),
                (nation3, UnitType.Fleet, "Hol", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation4, UnitType.Fleet, "SKA", false),
                (nation4, UnitType.Fleet, "NTH", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.10. Almost circular movement with no self dislodgement with beleaguered garrison")]
    public void DATC_E_10()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Fleet, "Yor"),
                (nation3, UnitType.Fleet, "Hol"),
                (nation3, UnitType.Fleet, "HEL"),
                (nation3, UnitType.Fleet, "Den"),
                (nation4, UnitType.Fleet, "SKA"),
                (nation4, UnitType.Fleet, "Nwy"),
            ]);

        var englishMove = units.Get("NTH").Move("Den");
        var englishSupport = units.Get("Yor").Support(units.Get("Nwy"), "NTH");
        var germanMove1 = units.Get("HEL").Move("NTH");
        var germanMove2 = units.Get("Den").Move("HEL");
        var germanSupport = units.Get("Hol").Support(units.Get("HEL"), "NTH");
        var russianMove = units.Get("Nwy").Move("NTH");
        var russianSupport = units.Get("SKA").Support(units.Get("Nwy"), "NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        germanMove1.Status.Should().Be(OrderStatus.Failure);
        germanMove2.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Fleet, "Yor", false),
                (nation3, UnitType.Fleet, "Hol", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation3, UnitType.Fleet, "Den", false),
                (nation4, UnitType.Fleet, "SKA", false),
                (nation4, UnitType.Fleet, "Nwy", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.11. No self dislodgement with beleaguered garrison, unit swap with adjacent convoying and two coasts")]
    public void DATC_E_11()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation2, UnitType.Army, "Spa"),
                (nation2, UnitType.Fleet, "MAO"),
                (nation2, UnitType.Fleet, "LYO"),
                (nation3, UnitType.Army, "Mar"),
                (nation3, UnitType.Army, "Gas"),
                (nation1, UnitType.Fleet, "Por"),
                (nation1, UnitType.Fleet, "WES"),
            ]);

        var frenchMove = units.Get("Spa").Move("Por");
        var frenchSupport = units.Get("LYO").Support(units.Get("Por"), "Spa_N");
        var frenchConvoy = units.Get("MAO").Convoy(units.Get("Spa"), "Por");
        var germanMove = units.Get("Gas").Move("Spa");
        var germanSupport = units.Get("Mar").Support(units.Get("Gas"), "Spa");
        var italianMove = units.Get("Por").Move("Spa_N");
        var italianSupport = units.Get("WES").Support(units.Get("Por"), "Spa_N");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        frenchMove.Status.Should().Be(OrderStatus.Success);
        frenchSupport.Status.Should().Be(OrderStatus.Success);
        frenchConvoy.Status.Should().Be(OrderStatus.Success);
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);
        italianMove.Status.Should().Be(OrderStatus.Success);
        italianSupport.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation2, UnitType.Army, "Por", false),
                (nation2, UnitType.Fleet, "MAO", false),
                (nation2, UnitType.Fleet, "LYO", false),
                (nation3, UnitType.Army, "Mar", false),
                (nation3, UnitType.Army, "Gas", false),
                (nation1, UnitType.Fleet, "Spa_N", false),
                (nation1, UnitType.Fleet, "WES", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.12. Support on attack on own unit can be used for other means")]
    public void DATC_E_12()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation5, UnitType.Army, "Bud"),
                (nation5, UnitType.Army, "Ser"),
                (nation1, UnitType.Army, "Vie"),
                (nation4, UnitType.Army, "Gal"),
                (nation4, UnitType.Army, "Rum"),
            ]);

        var austrianMove = units.Get("Bud").Move("Rum");
        var austrianSupport = units.Get("Ser").Support(units.Get("Vie"), "Bud");
        var italianMove = units.Get("Vie").Move("Bud");
        var russianMove = units.Get("Gal").Move("Bud");
        var russianSupport = units.Get("Rum").Support(units.Get("Gal"), "Bud");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianMove.Status.Should().Be(OrderStatus.Failure);
        austrianSupport.Status.Should().Be(OrderStatus.Failure);
        italianMove.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation5, UnitType.Army, "Bud", false),
                (nation5, UnitType.Army, "Ser", false),
                (nation1, UnitType.Army, "Vie", false),
                (nation4, UnitType.Army, "Gal", false),
                (nation4, UnitType.Army, "Rum", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.13. Three way beleaguered garrison")]
    public void DATC_E_13()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Edi"),
                (nation1, UnitType.Fleet, "Yor"),
                (nation2, UnitType.Fleet, "Bel"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation3, UnitType.Fleet, "NTH"),
                (nation4, UnitType.Fleet, "NWG"),
                (nation4, UnitType.Fleet, "Nwy"),
            ]);

        var englishMove = units.Get("Yor").Move("NTH");
        var englishSupport = units.Get("Edi").Support(units.Get("Yor"), "NTH");
        var frenchMove = units.Get("Bel").Move("NTH");
        var frenchSupport = units.Get("ENG").Support(units.Get("Bel"), "NTH");
        var germanHold = units.Get("NTH").Hold();
        var russianMove = units.Get("NWG").Move("NTH");
        var russianSupport = units.Get("Nwy").Support(units.Get("NWG"), "NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchSupport.Status.Should().Be(OrderStatus.Failure);
        germanHold.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Edi", false),
                (nation1, UnitType.Fleet, "Yor", false),
                (nation2, UnitType.Fleet, "Bel", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation3, UnitType.Fleet, "NTH", false),
                (nation4, UnitType.Fleet, "NWG", false),
                (nation4, UnitType.Fleet, "Nwy", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.14. Illegal head-to-head battle can still defend")]
    public void DATC_E_14()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Lvp"),
                (nation4, UnitType.Fleet, "Edi"),
            ]);

        var englishMove = units.Get("Lvp").Move("Edi");
        var russianMove = units.Get("Edi").Move("Lvp");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Lvp", false),
                (nation4, UnitType.Fleet, "Edi", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC E.15. The friendly head-to-head battle")]
    public void DATC_E_15()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Hol"),
                (nation1, UnitType.Army, "Ruh"),
                (nation2, UnitType.Army, "Kie"),
                (nation2, UnitType.Army, "Mun"),
                (nation2, UnitType.Army, "Sil"),
                (nation3, UnitType.Army, "Ber"),
                (nation3, UnitType.Fleet, "Den"),
                (nation3, UnitType.Fleet, "HEL"),
                (nation4, UnitType.Fleet, "BAL"),
                (nation4, UnitType.Army, "Pru"),
            ]);

        var englishMove = units.Get("Ruh").Move("Kie");
        var englishSupport = units.Get("Hol").Support(units.Get("Ruh"), "Kie");
        var frenchMove = units.Get("Kie").Move("Ber");
        var frenchSupport1 = units.Get("Mun").Support(units.Get("Kie"), "Ber");
        var frenchSupport2 = units.Get("Sil").Support(units.Get("Kie"), "Ber");
        var germanMove = units.Get("Ber").Move("Kie");
        var germanSupport1 = units.Get("Den").Support(units.Get("Ber"), "Kie");
        var germanSupport2 = units.Get("HEL").Support(units.Get("Ber"), "Kie");
        var russianMove = units.Get("Pru").Move("Ber");
        var russianSupport = units.Get("BAL").Support(units.Get("Pru"), "Ber");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchSupport1.Status.Should().Be(OrderStatus.Failure);
        frenchSupport2.Status.Should().Be(OrderStatus.Failure);
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport1.Status.Should().Be(OrderStatus.Failure);
        germanSupport2.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Hol", false),
                (nation1, UnitType.Army, "Ruh", false),
                (nation2, UnitType.Army, "Kie", false),
                (nation2, UnitType.Army, "Mun", false),
                (nation2, UnitType.Army, "Sil", false),
                (nation3, UnitType.Army, "Ber", false),
                (nation3, UnitType.Fleet, "Den", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation4, UnitType.Fleet, "BAL", false),
                (nation4, UnitType.Army, "Pru", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }
}
