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
public class DATC_G : AdjudicationTestBase
{
    [Fact(DisplayName = "DATC G.01. Two units can swap provinces by convoy")]
    public void DATC_G_1()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Nwy"),
                (nation1, UnitType.Fleet, "SKA"),
                (nation4, UnitType.Army, "Swe"),
            ]);

        var englishMove = units.Get("Nwy").Move("Swe");
        var englishConvoy = units.Get("SKA").Convoy(units.Get("Nwy"), "Swe");
        var russianMove = units.Get("Swe").Move("Nwy");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Swe", false),
                (nation1, UnitType.Fleet, "SKA", false),
                (nation4, UnitType.Army, "Nwy", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.02. Kidnapping an army")]
    public void DATC_G_2()
    {
        // Decision is to allow non-dislodged convoy kidnapping because it's fun, maybe even more so in 5D.

        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Nwy"),
                (nation4, UnitType.Fleet, "Swe"),
                (nation3, UnitType.Fleet, "SKA"),
            ]);

        var englishMove = units.Get("Nwy").Move("Swe");
        var russianMove = units.Get("Swe").Move("Nwy");
        var germanConvoy = units.Get("SKA").Convoy(units.Get("Nwy"), "Swe");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Success);
        germanConvoy.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Swe", false),
                (nation4, UnitType.Fleet, "Nwy", false),
                (nation3, UnitType.Fleet, "SKA", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.03. An unwanted convoy to adjacent province")]
    public void DATC_G_3()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation2, UnitType.Fleet, "Bre"),
                (nation2, UnitType.Army, "Pic"),
                (nation2, UnitType.Army, "Bur"),
                (nation2, UnitType.Fleet, "MAO"),
                (nation1, UnitType.Fleet, "ENG"),
            ]);

        var frenchMove1 = units.Get("Bre").Move("ENG");
        var frenchMove2 = units.Get("Pic").Move("Bel");
        var frenchSupport1 = units.Get("MAO").Support(units.Get("Bre"), "ENG");
        var frenchSupport2 = units.Get("Bur").Support(units.Get("Pic"), "Bel");
        var englishConvoy = units.Get("ENG").Convoy(units.Get("Pic"), "Bel");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        frenchMove1.Status.Should().Be(OrderStatus.Success);
        frenchMove2.Status.Should().Be(OrderStatus.Success);
        frenchSupport1.Status.Should().Be(OrderStatus.Success);
        frenchSupport2.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation2, UnitType.Fleet, "Bre", false),
                (nation2, UnitType.Army, "Pic", false),
                (nation2, UnitType.Army, "Bur", false),
                (nation2, UnitType.Fleet, "MAO", false),
                (nation1, UnitType.Fleet, "ENG", true),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.04. An unwanted disrupted convoy to adjacent province and opposite move")]
    public void DATC_G_4()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation2, UnitType.Fleet, "Bre"),
                (nation2, UnitType.Army, "Pic"),
                (nation2, UnitType.Army, "Bur"),
                (nation2, UnitType.Fleet, "MAO"),
                (nation1, UnitType.Fleet, "ENG"),
                (nation1, UnitType.Army, "Bel"),
            ]);

        var frenchMove1 = units.Get("Bre").Move("ENG");
        var frenchMove2 = units.Get("Pic").Move("Bel");
        var frenchSupport1 = units.Get("MAO").Support(units.Get("Bre"), "ENG");
        var frenchSupport2 = units.Get("Bur").Support(units.Get("Pic"), "Bel");
        var englishMove = units.Get("Bel").Move("Pic");
        var englishConvoy = units.Get("ENG").Convoy(units.Get("Pic"), "Bel");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        frenchMove1.Status.Should().Be(OrderStatus.Success);
        frenchMove2.Status.Should().Be(OrderStatus.Success);
        frenchSupport1.Status.Should().Be(OrderStatus.Success);
        frenchSupport2.Status.Should().Be(OrderStatus.Success);
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishConvoy.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation2, UnitType.Fleet, "Bre", false),
                (nation2, UnitType.Army, "Pic", false),
                (nation2, UnitType.Army, "Bur", false),
                (nation2, UnitType.Fleet, "MAO", false),
                (nation1, UnitType.Fleet, "ENG", true),
                (nation1, UnitType.Army, "Bel", true),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.05. Swapping with multiple fleets with one own fleet")]
    public void DATC_G_5()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Rom"),
                (nation1, UnitType.Fleet, "TYS"),
                (nation5, UnitType.Army, "Apu"),
                (nation5, UnitType.Fleet, "ION"),
            ]);

        var italianMove = units.Get("Rom").Move("Apu");
        var italianConvoy = units.Get("TYS").Convoy(units.Get("Rom"), "Apu");
        var turkishMove = units.Get("Apu").Move("Rom");
        var turkishConvoy = units.Get("ION").Convoy(units.Get("Rom"), "Apu");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        italianMove.Status.Should().Be(OrderStatus.Success);
        italianConvoy.Status.Should().Be(OrderStatus.Success);
        turkishMove.Status.Should().Be(OrderStatus.Success);
        turkishConvoy.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Apu", false),
                (nation1, UnitType.Fleet, "TYS", false),
                (nation5, UnitType.Army, "Rom", false),
                (nation5, UnitType.Fleet, "ION", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.06. Swapping with unintended intent")]
    public void DATC_G_6()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Lvp"),
                (nation1, UnitType.Fleet, "ENG"),
                (nation3, UnitType.Army, "Edi"),
                (nation2, UnitType.Fleet, "IRI"),
                (nation2, UnitType.Fleet, "NTH"),
                (nation4, UnitType.Fleet, "NWG"),
                (nation4, UnitType.Fleet, "NAO"),
            ]);

        var englishMove = units.Get("Lvp").Move("Edi");
        var englishConvoy = units.Get("ENG").Convoy(units.Get("Lvp"), "Edi");
        var germanMove = units.Get("Edi").Move("Lvp");
        var frenchHold1 = units.Get("IRI").Hold();
        var frenchHold2 = units.Get("NTH").Hold();
        var russianConvoy1 = units.Get("NWG").Convoy(units.Get("Lvp"), "Edi");
        var russianConvoy2 = units.Get("NAO").Convoy(units.Get("Lvp"), "Edi");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Invalid);
        germanMove.Status.Should().Be(OrderStatus.Success);
        frenchHold1.Status.Should().Be(OrderStatus.Success);
        frenchHold2.Status.Should().Be(OrderStatus.Success);
        russianConvoy1.Status.Should().Be(OrderStatus.Success);
        russianConvoy2.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Edi", false),
                (nation1, UnitType.Fleet, "ENG", false),
                (nation3, UnitType.Army, "Lvp", false),
                (nation2, UnitType.Fleet, "IRI", false),
                (nation2, UnitType.Fleet, "NTH", false),
                (nation4, UnitType.Fleet, "NWG", false),
                (nation4, UnitType.Fleet, "NAO", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.07. Swapping with illegal intent")]
    public void DATC_G_7()
    {
        // Another overrule of the DATC to allow non-dislodged convoy kidnapping.

        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "SKA"),
                (nation1, UnitType.Fleet, "Nwy"),
                (nation4, UnitType.Army, "Swe"),
                (nation4, UnitType.Fleet, "BOT"),
            ]);

        var englishMove = units.Get("Nwy").Move("Swe");
        var englishConvoy = units.Get("SKA").Convoy(units.Get("Swe"), "Nwy");
        var russianMove = units.Get("Swe").Move("Nwy");
        var russianConvoy = units.Get("BOT").Convoy(units.Get("Swe"), "Nwy");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Success);
        russianConvoy.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "SKA", false),
                (nation1, UnitType.Fleet, "Swe", false),
                (nation4, UnitType.Army, "Nwy", false),
                (nation4, UnitType.Fleet, "BOT", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.08. Explicit convoy that isn't there", Skip = "Not applicable")]
    public void DATC_G_8()
    {
        // 5D Diplomacy does not have a concept of an explicit "via convoy" move, so this case does not apply.
    }

    [Fact(DisplayName = "DATC G.09. Swapped or dislodged?")]
    public void DATC_G_9()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Nwy"),
                (nation1, UnitType.Fleet, "SKA"),
                (nation1, UnitType.Fleet, "Fin"),
                (nation4, UnitType.Army, "Swe"),
            ]);

        var englishMove = units.Get("Nwy").Move("Swe");
        var englishSupport = units.Get("Fin").Support(units.Get("Nwy"), "Swe");
        var englishConvoy = units.Get("SKA").Convoy(units.Get("Nwy"), "Swe");
        var russianMove = units.Get("Swe").Move("Nwy");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishSupport.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Swe", false),
                (nation1, UnitType.Fleet, "SKA", false),
                (nation1, UnitType.Fleet, "Fin", false),
                (nation4, UnitType.Army, "Nwy", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.10. Swapped or a head-to-head battle?")]
    public void DATC_G_10()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Nwy"),
                (nation1, UnitType.Fleet, "Den"),
                (nation1, UnitType.Fleet, "Fin"),
                (nation3, UnitType.Fleet, "SKA"),
                (nation4, UnitType.Army, "Swe"),
                (nation4, UnitType.Fleet, "BAR"),
                (nation2, UnitType.Fleet, "NWG"),
                (nation2, UnitType.Fleet, "NTH"),
            ]);

        var englishMove = units.Get("Nwy").Move("Swe");
        var englishSupport1 = units.Get("Fin").Support(units.Get("Nwy"), "Swe");
        var englishSupport2 = units.Get("Den").Support(units.Get("Nwy"), "Swe");
        var germanConvoy = units.Get("SKA").Convoy(units.Get("Nwy"), "Swe");
        var russianMove = units.Get("Swe").Move("Nwy");
        var russianSupport = units.Get("BAR").Support(units.Get("Swe"), "Nwy");
        var frenchMove = units.Get("NWG").Move("Nwy");
        var frenchSupport = units.Get("NTH").Support(units.Get("NWG"), "Nwy");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishSupport1.Status.Should().Be(OrderStatus.Success);
        englishSupport2.Status.Should().Be(OrderStatus.Success);
        germanConvoy.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Swe", false),
                (nation1, UnitType.Fleet, "Den", false),
                (nation1, UnitType.Fleet, "Fin", false),
                (nation3, UnitType.Fleet, "SKA", false),
                (nation4, UnitType.Fleet, "BAR", false),
                (nation2, UnitType.Fleet, "NWG", false),
                (nation2, UnitType.Fleet, "NTH", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.11. A convoy to an adjacent province with a paradox")]
    public void DATC_G_11()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Nwy"),
                (nation1, UnitType.Fleet, "NTH"),
                (nation4, UnitType.Army, "Swe"),
                (nation4, UnitType.Fleet, "SKA"),
                (nation4, UnitType.Fleet, "BAR"),
            ]);

        var englishMove = units.Get("NTH").Move("SKA");
        var englishSupport = units.Get("Nwy").Support(units.Get("NTH"), "SKA");
        var russianMove = units.Get("Swe").Move("Nwy");
        var russianSupport = units.Get("BAR").Support(units.Get("Swe"), "Nwy");
        var russianConvoy = units.Get("SKA").Convoy(units.Get("Swe"), "Nwy");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Success);
        russianSupport.Status.Should().Be(OrderStatus.Success);
        russianConvoy.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Nwy", true),
                (nation1, UnitType.Fleet, "NTH", false),
                (nation4, UnitType.Army, "Swe", false),
                (nation4, UnitType.Fleet, "SKA", false),
                (nation4, UnitType.Fleet, "BAR", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.12. Swapping two units with two convoys")]
    public void DATC_G_12()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Lvp"),
                (nation1, UnitType.Fleet, "NAO"),
                (nation1, UnitType.Fleet, "NWG"),
                (nation3, UnitType.Army, "Edi"),
                (nation3, UnitType.Fleet, "NTH"),
                (nation3, UnitType.Fleet, "ENG"),
                (nation3, UnitType.Fleet, "IRI"),
            ]);

        var englishMove = units.Get("Lvp").Move("Edi");
        var englishConvoy1 = units.Get("NAO").Convoy(units.Get("Lvp"), "Edi");
        var englishConvoy2 = units.Get("NWG").Convoy(units.Get("Lvp"), "Edi");
        var germanMove = units.Get("Edi").Move("Lvp");
        var germanConvoy1 = units.Get("NTH").Convoy(units.Get("Edi"), "Lvp");
        var germanConvoy2 = units.Get("ENG").Convoy(units.Get("Edi"), "Lvp");
        var germanConvoy3 = units.Get("IRI").Convoy(units.Get("Edi"), "Lvp");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishConvoy1.Status.Should().Be(OrderStatus.Success);
        englishConvoy2.Status.Should().Be(OrderStatus.Success);
        germanMove.Status.Should().Be(OrderStatus.Success);
        germanConvoy1.Status.Should().Be(OrderStatus.Success);
        germanConvoy2.Status.Should().Be(OrderStatus.Success);
        germanConvoy3.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Edi", false),
                (nation1, UnitType.Fleet, "NAO", false),
                (nation1, UnitType.Fleet, "NWG", false),
                (nation3, UnitType.Army, "Lvp", false),
                (nation3, UnitType.Fleet, "NTH", false),
                (nation3, UnitType.Fleet, "ENG", false),
                (nation3, UnitType.Fleet, "IRI", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.13. Support cut on attack on itself via convoy")]
    public void DATC_G_13()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation5, UnitType.Fleet, "ADR"),
                (nation5, UnitType.Army, "Tri"),
                (nation1, UnitType.Army, "Ven"),
                (nation1, UnitType.Fleet, "Alb"),
            ]);

        var austrianMove = units.Get("Tri").Move("Ven");
        var austrianConvoy = units.Get("ADR").Convoy(units.Get("Tri"), "Ven");
        var italianMove = units.Get("Alb").Move("Tri");
        var italianSupport = units.Get("Ven").Support(units.Get("Alb"), "Tri");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianMove.Status.Should().Be(OrderStatus.Failure);
        austrianConvoy.Status.Should().Be(OrderStatus.Failure);
        italianMove.Status.Should().Be(OrderStatus.Success);
        italianSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation5, UnitType.Fleet, "ADR", false),
                (nation5, UnitType.Army, "Tri", true),
                (nation1, UnitType.Army, "Ven", false),
                (nation1, UnitType.Fleet, "Alb", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.14. Bounce by convoy to adjacent province")]
    public void DATC_G_14()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Nwy"),
                (nation1, UnitType.Fleet, "Den"),
                (nation1, UnitType.Fleet, "Fin"),
                (nation2, UnitType.Fleet, "NWG"),
                (nation2, UnitType.Fleet, "NTH"),
                (nation3, UnitType.Fleet, "SKA"),
                (nation4, UnitType.Army, "Swe"),
                (nation4, UnitType.Fleet, "BAR"),
            ]);

        var englishMove = units.Get("Nwy").Move("Swe");
        var englishSupport1 = units.Get("Den").Support(units.Get("Nwy"), "Swe");
        var englishSupport2 = units.Get("Fin").Support(units.Get("Nwy"), "Swe");
        var frenchMove = units.Get("NWG").Move("Nwy");
        var frenchSupport = units.Get("NTH").Support(units.Get("NWG"), "Nwy");
        var germanConvoy = units.Get("SKA").Convoy(units.Get("Swe"), "Nwy");
        var russianMove = units.Get("Swe").Move("Nwy");
        var russianSupport = units.Get("BAR").Support(units.Get("Swe"), "Nwy");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishSupport1.Status.Should().Be(OrderStatus.Success);
        englishSupport2.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchSupport.Status.Should().Be(OrderStatus.Failure);
        germanConvoy.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Swe", false),
                (nation1, UnitType.Fleet, "Den", false),
                (nation1, UnitType.Fleet, "Fin", false),
                (nation2, UnitType.Fleet, "NWG", false),
                (nation2, UnitType.Fleet, "NTH", false),
                (nation3, UnitType.Fleet, "SKA", false),
                (nation4, UnitType.Fleet, "BAR", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.15. Bounce and dislodge with double convoy")]
    public void DATC_G_15()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Hol"),
                (nation1, UnitType.Army, "Yor"),
                (nation1, UnitType.Army, "Lon"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation2, UnitType.Army, "Bel"),
            ]);

        var englishMove1 = units.Get("Yor").Move("Lon");
        var englishMove2 = units.Get("Lon").Move("Bel");
        var englishSupport = units.Get("Hol").Support(units.Get("Lon"), "Bel");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Bel");
        var frenchMove = units.Get("Bel").Move("Lon");
        var frenchSupport = units.Get("ENG").Convoy(units.Get("Bel"), "Lon");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove1.Status.Should().Be(OrderStatus.Failure);
        englishMove2.Status.Should().Be(OrderStatus.Success);
        englishSupport.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchSupport.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Army, "Hol", false),
                (nation1, UnitType.Army, "Yor", false),
                (nation1, UnitType.Army, "Lon", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation2, UnitType.Army, "Bel", true),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.16. The two unit in one area bug, moving by convoy")]
    public void DATC_G_16()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Nwy"),
                (nation1, UnitType.Army, "Den"),
                (nation1, UnitType.Fleet, "BAL"),
                (nation1, UnitType.Fleet, "NTH"),
                (nation4, UnitType.Army, "Swe"),
                (nation4, UnitType.Fleet, "SKA"),
                (nation4, UnitType.Fleet, "NWG"),
            ]);

        var englishMove1 = units.Get("Nwy").Move("Swe");
        var englishMove2 = units.Get("NTH").Move("Nwy");
        var englishSupport1 = units.Get("Den").Support(units.Get("Nwy"), "Swe");
        var englishSupport2 = units.Get("BAL").Support(units.Get("Nwy"), "Swe");
        var russianMove = units.Get("Swe").Move("Nwy");
        var russianSupport = units.Get("NWG").Support(units.Get("Swe"), "Nwy");
        var russianConvoy = units.Get("SKA").Convoy(units.Get("Swe"), "Nwy");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove1.Status.Should().Be(OrderStatus.Success);
        englishMove2.Status.Should().Be(OrderStatus.Failure);
        englishSupport1.Status.Should().Be(OrderStatus.Success);
        englishSupport2.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Success);
        russianSupport.Status.Should().Be(OrderStatus.Success);
        russianConvoy.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Swe", false),
                (nation1, UnitType.Army, "Den", false),
                (nation1, UnitType.Fleet, "BAL", false),
                (nation1, UnitType.Fleet, "NTH", false),
                (nation4, UnitType.Army, "Nwy", false),
                (nation4, UnitType.Fleet, "SKA", false),
                (nation4, UnitType.Fleet, "NWG", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.17. The two unit in one area bug, moving over land")]
    public void DATC_G_17()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Nwy"),
                (nation1, UnitType.Army, "Den"),
                (nation1, UnitType.Fleet, "BAL"),
                (nation1, UnitType.Fleet, "SKA"),
                (nation1, UnitType.Fleet, "NTH"),
                (nation4, UnitType.Army, "Swe"),
                (nation4, UnitType.Fleet, "NWG"),
            ]);

        var englishMove1 = units.Get("Nwy").Move("Swe");
        var englishMove2 = units.Get("NTH").Move("Nwy");
        var englishSupport1 = units.Get("Den").Support(units.Get("Nwy"), "Swe");
        var englishSupport2 = units.Get("BAL").Support(units.Get("Nwy"), "Swe");
        var englishConvoy = units.Get("SKA").Convoy(units.Get("Nwy"), "Swe");
        var russianMove = units.Get("Swe").Move("Nwy");
        var russianSupport = units.Get("NWG").Support(units.Get("Swe"), "Nwy");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove1.Status.Should().Be(OrderStatus.Success);
        englishMove2.Status.Should().Be(OrderStatus.Failure);
        englishSupport1.Status.Should().Be(OrderStatus.Success);
        englishSupport2.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Success);
        russianSupport.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Swe", false),
                (nation1, UnitType.Army, "Den", false),
                (nation1, UnitType.Fleet, "BAL", false),
                (nation1, UnitType.Fleet, "SKA", false),
                (nation1, UnitType.Fleet, "NTH", false),
                (nation4, UnitType.Army, "Nwy", false),
                (nation4, UnitType.Fleet, "NWG", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.18. The two unit in one area bug, with double convoy")]
    public void DATC_G_18()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Hol"),
                (nation1, UnitType.Army, "Yor"),
                (nation1, UnitType.Army, "Lon"),
                (nation1, UnitType.Army, "Ruh"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation2, UnitType.Army, "Bel"),
                (nation2, UnitType.Army, "Wal"),
            ]);

        var englishMove1 = units.Get("Yor").Move("Lon");
        var englishMove2 = units.Get("Lon").Move("Bel");
        var englishSupport1 = units.Get("Hol").Support(units.Get("Lon"), "Bel");
        var englishSupport2 = units.Get("Ruh").Support(units.Get("Lon"), "Bel");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Bel");
        var frenchMove = units.Get("Bel").Move("Lon");
        var frenchSupport = units.Get("Wal").Support(units.Get("Bel"), "Lon");
        var frenchConvoy = units.Get("ENG").Convoy(units.Get("Bel"), "Lon");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove1.Status.Should().Be(OrderStatus.Failure);
        englishMove2.Status.Should().Be(OrderStatus.Success);
        englishSupport1.Status.Should().Be(OrderStatus.Success);
        englishSupport2.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Success);
        frenchSupport.Status.Should().Be(OrderStatus.Success);
        frenchConvoy.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Army, "Hol", false),
                (nation1, UnitType.Army, "Yor", false),
                (nation1, UnitType.Army, "Bel", false),
                (nation1, UnitType.Army, "Ruh", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation2, UnitType.Army, "Lon", false),
                (nation2, UnitType.Army, "Wal", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.19. Swapping with intent of unnecesary convoy")]
    public void DATC_G_19()
    {
        // Another case of overruling the DATC on convoy kidnapping.

        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation2, UnitType.Army, "Mar"),
                (nation2, UnitType.Fleet, "WES"),
                (nation1, UnitType.Fleet, "LYO"),
                (nation1, UnitType.Army, "Spa"),
            ]);

        var frenchMove = units.Get("Mar").Move("Spa");
        var frenchConvoy = units.Get("WES").Convoy(units.Get("Mar"), "Spa");
        var italianMove = units.Get("Spa").Move("Mar");
        var italianConvoy = units.Get("LYO").Convoy(units.Get("Mar"), "Spa");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        frenchMove.Status.Should().Be(OrderStatus.Success);
        frenchConvoy.Status.Should().Be(OrderStatus.Invalid);
        italianMove.Status.Should().Be(OrderStatus.Success);
        italianConvoy.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation2, UnitType.Army, "Spa", false),
                (nation2, UnitType.Fleet, "WES", false),
                (nation1, UnitType.Fleet, "LYO", false),
                (nation1, UnitType.Army, "Mar", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC G.20. Explicit convoy to adjacent province disrupted", Skip = "Not applicable")]
    public void DATC_G_20()
    {
        // 5D Diplomacy does not have a concept of specifying a move is "via convoy", so this test case is irrelevant.
    }
}
