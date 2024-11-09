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
public class DATC_F : AdjudicationTestBase
{
    [Fact(DisplayName = "DATC F.01. No convoys in coastal areas")]
    public void DATC_F_1()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation5, UnitType.Army, "Gre"),
                (nation5, UnitType.Fleet, "AEG"),
                (nation5, UnitType.Fleet, "Con"),
                (nation5, UnitType.Fleet, "BLA"),
            ]);

        var move = units.Get("Gre").Move("Sev");
        var convoy1 = units.Get("AEG").Convoy(units.Get("Gre"), "Sev");
        var convoy2 = units.Get("Con").Convoy(units.Get("Gre"), "Sev");
        var convoy3 = units.Get("BLA").Convoy(units.Get("Gre"), "Sev");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move.Status.Should().Be(OrderStatus.Invalid);
        convoy1.Status.Should().Be(OrderStatus.Invalid);
        convoy2.Status.Should().Be(OrderStatus.Invalid);
        convoy3.Status.Should().Be(OrderStatus.Invalid);

        board.Next().ShouldHaveUnits(
            [
                (nation5, UnitType.Army, "Gre", false),
                (nation5, UnitType.Fleet, "AEG", false),
                (nation5, UnitType.Fleet, "Con", false),
                (nation5, UnitType.Fleet, "BLA", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.02. An army being convoyed can bounce as normal")]
    public void DATC_F_2()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "ENG"),
                (nation1, UnitType.Army, "Lon"),
                (nation2, UnitType.Army, "Par"),
            ]);

        var englishMove = units.Get("Lon").Move("Bre");
        var englishConvoy = units.Get("ENG").Convoy(units.Get("Lon"), "Bre");
        var frenchMove = units.Get("Par").Move("Bre");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishConvoy.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "ENG", false),
                (nation1, UnitType.Army, "Lon", false),
                (nation2, UnitType.Army, "Par", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.03. An army being convoyed can receive support")]
    public void DATC_F_3()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "ENG"),
                (nation1, UnitType.Army, "Lon"),
                (nation1, UnitType.Fleet, "MAO"),
                (nation2, UnitType.Army, "Par"),
            ]);

        var englishMove = units.Get("Lon").Move("Bre");
        var englishSupport = units.Get("MAO").Support(units.Get("Lon"), "Bre");
        var englishConvoy = units.Get("ENG").Convoy(units.Get("Lon"), "Bre");
        var frenchMove = units.Get("Par").Move("Bre");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishSupport.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "ENG", false),
                (nation1, UnitType.Army, "Bre", false),
                (nation1, UnitType.Fleet, "MAO", false),
                (nation2, UnitType.Army, "Par", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.04. An attacked convoy is not disrupted")]
    public void DATC_F_4()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Lon"),
                (nation3, UnitType.Fleet, "SKA"),
            ]);

        var englishMove = units.Get("Lon").Move("Hol");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Hol");
        var germanMove = units.Get("SKA").Move("NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        germanMove.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Army, "Hol", false),
                (nation3, UnitType.Fleet, "SKA", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.05. A beleaguered convoy is not disrupted")]
    public void DATC_F_5()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Lon"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation2, UnitType.Fleet, "Bel"),
                (nation3, UnitType.Fleet, "SKA"),
                (nation3, UnitType.Fleet, "Den"),
            ]);

        var englishMove = units.Get("Lon").Move("Hol");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Hol");
        var frenchMove = units.Get("ENG").Move("NTH");
        var frenchSupport = units.Get("Bel").Support(units.Get("ENG"), "NTH");
        var germanMove = units.Get("SKA").Move("NTH");
        var germanSupport = units.Get("Den").Support(units.Get("SKA"), "NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchSupport.Status.Should().Be(OrderStatus.Failure);
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Army, "Hol", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation2, UnitType.Fleet, "Bel", false),
                (nation3, UnitType.Fleet, "SKA", false),
                (nation3, UnitType.Fleet, "Den", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.06. Dislodged convoy does not cut support")]
    public void DATC_F_6()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Lon"),
                (nation3, UnitType.Army, "Hol"),
                (nation3, UnitType.Army, "Bel"),
                (nation3, UnitType.Fleet, "HEL"),
                (nation3, UnitType.Fleet, "SKA"),
                (nation2, UnitType.Army, "Pic"),
                (nation2, UnitType.Army, "Bur"),
            ]);

        var englishMove = units.Get("Lon").Move("Hol");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Hol");
        var germanMove = units.Get("SKA").Move("NTH");
        var germanSupport1 = units.Get("Hol").Support(units.Get("Bel"));
        var germanSupport2 = units.Get("Bel").Support(units.Get("Hol"));
        var germanSupport3 = units.Get("HEL").Support(units.Get("SKA"), "NTH");
        var frenchMove = units.Get("Pic").Move("Bel");
        var frenchSupport = units.Get("Bur").Support(units.Get("Pic"), "Bel");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishConvoy.Status.Should().Be(OrderStatus.Failure);
        germanMove.Status.Should().Be(OrderStatus.Success);
        germanSupport1.Status.Should().Be(OrderStatus.Success);
        germanSupport2.Status.Should().Be(OrderStatus.Failure);
        germanSupport3.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchSupport.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", true),
                (nation1, UnitType.Army, "Lon", false),
                (nation3, UnitType.Army, "Hol", false),
                (nation3, UnitType.Army, "Bel", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation3, UnitType.Fleet, "SKA", false),
                (nation2, UnitType.Army, "Pic", false),
                (nation2, UnitType.Army, "Bur", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.07. Dislodged convoy does not cause contested area")]
    public void DATC_F_7()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Lon"),
                (nation3, UnitType.Fleet, "HEL"),
                (nation3, UnitType.Fleet, "SKA"),
            ]);

        var englishMove = units.Get("Lon").Move("Hol");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Hol");
        var germanMove = units.Get("SKA").Move("NTH");
        var germanSupport = units.Get("HEL").Support(units.Get("SKA"), "NTH");

        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishConvoy.Status.Should().Be(OrderStatus.Failure);
        germanMove.Status.Should().Be(OrderStatus.Success);
        germanSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", true),
                (nation1, UnitType.Army, "Lon", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation3, UnitType.Fleet, "SKA", false),
            ]);
        board.ShouldNotHaveNextBoard();

        var englishRetreat = units.Get("NTH").Move("Hol");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishRetreat.Status.Should().Be(OrderStatus.RetreatSuccess);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Hol", false),
                (nation1, UnitType.Army, "Lon", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation3, UnitType.Fleet, "NTH", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.08. Dislodged convoy does not cause a bounce")]
    public void DATC_F_8()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Lon"),
                (nation3, UnitType.Fleet, "HEL"),
                (nation3, UnitType.Fleet, "SKA"),
                (nation3, UnitType.Army, "Bel"),
            ]);

        var englishMove = units.Get("Lon").Move("Hol");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Hol");
        var germanMove1 = units.Get("SKA").Move("NTH");
        var germanMove2 = units.Get("Bel").Move("Hol");
        var germanSupport = units.Get("HEL").Support(units.Get("SKA"), "NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishConvoy.Status.Should().Be(OrderStatus.Failure);
        germanMove1.Status.Should().Be(OrderStatus.Success);
        germanMove2.Status.Should().Be(OrderStatus.Success);
        germanSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", true),
                (nation1, UnitType.Army, "Lon", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation3, UnitType.Fleet, "SKA", false),
                (nation3, UnitType.Army, "Bel", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.09. Dislodge of multi-route convoy")]
    public void DATC_F_9()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Lon"),
                (nation1, UnitType.Fleet, "ENG"),
                (nation2, UnitType.Fleet, "Bre"),
                (nation2, UnitType.Fleet, "MAO"),
            ]);

        var englishMove = units.Get("Lon").Move("Bel");
        var englishConvoy1 = units.Get("NTH").Convoy(units.Get("Lon"), "Bel");
        var englishConvoy2 = units.Get("ENG").Convoy(units.Get("Lon"), "Bel");
        var frenchMove = units.Get("MAO").Move("ENG");
        var frenchSupport = units.Get("Bre").Support(units.Get("MAO"), "ENG");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishConvoy1.Status.Should().Be(OrderStatus.Success);
        englishConvoy2.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Success);
        frenchSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Army, "Lon", false),
                (nation1, UnitType.Fleet, "ENG", true),
                (nation2, UnitType.Fleet, "Bre", false),
                (nation2, UnitType.Fleet, "MAO", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.10. Dislodge of multi-route convoy with foreign fleet")]
    public void DATC_F_10()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Lon"),
                (nation3, UnitType.Fleet, "ENG"),
                (nation2, UnitType.Fleet, "Bre"),
                (nation2, UnitType.Fleet, "MAO"),
            ]);

        var englishMove = units.Get("Lon").Move("Bel");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Bel");
        var germanConvoy = units.Get("ENG").Convoy(units.Get("Lon"), "Bel");
        var frenchMove = units.Get("MAO").Move("ENG");
        var frenchSupport = units.Get("Bre").Support(units.Get("MAO"), "ENG");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        germanConvoy.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Success);
        frenchSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Army, "Lon", false),
                (nation3, UnitType.Fleet, "ENG", true),
                (nation2, UnitType.Fleet, "Bre", false),
                (nation2, UnitType.Fleet, "MAO", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.11. Dislodge of multi-route convoy with only foreign fleets")]
    public void DATC_F_11()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Lon"),
                (nation3, UnitType.Fleet, "ENG"),
                (nation4, UnitType.Fleet, "NTH"),
                (nation2, UnitType.Fleet, "Bre"),
                (nation2, UnitType.Fleet, "MAO"),
            ]);

        var englishMove = units.Get("Lon").Move("Bel");
        var germanConvoy = units.Get("ENG").Convoy(units.Get("Lon"), "Bel");
        var russianConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Bel");
        var frenchMove = units.Get("MAO").Move("ENG");
        var frenchSupport = units.Get("Bre").Support(units.Get("MAO"), "ENG");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        germanConvoy.Status.Should().Be(OrderStatus.Failure);
        russianConvoy.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Success);
        frenchSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Lon", false),
                (nation3, UnitType.Fleet, "ENG", true),
                (nation4, UnitType.Fleet, "NTH", false),
                (nation2, UnitType.Fleet, "Bre", false),
                (nation2, UnitType.Fleet, "MAO", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.12. Dislodged convoying fleet not on route")]
    public void DATC_F_12()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "ENG"),
                (nation1, UnitType.Army, "Lon"),
                (nation1, UnitType.Fleet, "IRI"),
                (nation2, UnitType.Fleet, "NAO"),
                (nation2, UnitType.Fleet, "MAO"),
            ]);

        var englishMove = units.Get("Lon").Move("Bel");
        var englishConvoy1 = units.Get("ENG").Convoy(units.Get("Lon"), "Bel");
        var englishConvoy2 = units.Get("IRI").Convoy(units.Get("Lon"), "Bel");
        var frenchMove = units.Get("MAO").Move("IRI");
        var frenchSupport = units.Get("NAO").Support(units.Get("MAO"), "IRI");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishConvoy1.Status.Should().Be(OrderStatus.Success);
        englishConvoy2.Status.Should().Be(OrderStatus.Invalid);
        frenchMove.Status.Should().Be(OrderStatus.Success);
        frenchSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "ENG", false),
                (nation1, UnitType.Army, "Lon", false),
                (nation1, UnitType.Fleet, "IRI", true),
                (nation2, UnitType.Fleet, "NAO", false),
                (nation2, UnitType.Fleet, "MAO", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.13. The unwanted alternative")]
    public void DATC_F_13()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Army, "Lon"),
                (nation1, UnitType.Fleet, "NTH"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation3, UnitType.Fleet, "Hol"),
                (nation3, UnitType.Fleet, "Den"),
            ]);

        var englishMove = units.Get("Lon").Move("Bel");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Bel");
        var frenchConvoy = units.Get("ENG").Convoy(units.Get("Lon"), "Bel");
        var germanMove = units.Get("Den").Move("NTH");
        var germanSupport = units.Get("Hol").Support(units.Get("Den"), "NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Failure);
        frenchConvoy.Status.Should().Be(OrderStatus.Success);
        germanMove.Status.Should().Be(OrderStatus.Success);
        germanSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Army, "Lon", false),
                (nation1, UnitType.Fleet, "NTH", true),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation3, UnitType.Fleet, "Hol", false),
                (nation3, UnitType.Fleet, "Den", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.14. Simple convoy paradox")]
    public void DATC_F_14()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Lon"),
                (nation1, UnitType.Fleet, "Wal"),
                (nation2, UnitType.Army, "Bre"),
                (nation2, UnitType.Fleet, "ENG"),
            ]);

        var englishMove = units.Get("Wal").Move("ENG");
        var englishSupport = units.Get("Lon").Support(units.Get("Wal"), "ENG");
        var frenchMove = units.Get("Bre").Move("Lon");
        var frenchConvoy = units.Get("ENG").Convoy(units.Get("Bre"), "Lon");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishSupport.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchConvoy.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Lon", false),
                (nation1, UnitType.Fleet, "Wal", false),
                (nation2, UnitType.Army, "Bre", false),
                (nation2, UnitType.Fleet, "ENG", true),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.15. Simple convoy paradox with additional convoy")]
    public void DATC_F_15()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Lon"),
                (nation1, UnitType.Fleet, "Wal"),
                (nation2, UnitType.Army, "Bre"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation3, UnitType.Fleet, "IRI"),
                (nation3, UnitType.Fleet, "MAO"),
                (nation3, UnitType.Army, "Naf"),
            ]);

        var englishMove = units.Get("Wal").Move("ENG");
        var englishSupport = units.Get("Lon").Support(units.Get("Wal"), "ENG");
        var frenchMove = units.Get("Bre").Move("Lon");
        var frenchConvoy = units.Get("ENG").Convoy(units.Get("Bre"), "Lon");
        var italianMove = units.Get("Naf").Move("Wal");
        var italianConvoy1 = units.Get("MAO").Convoy(units.Get("Naf"), "Wal");
        var italianConvoy2 = units.Get("IRI").Convoy(units.Get("Naf"), "Wal");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishSupport.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchConvoy.Status.Should().Be(OrderStatus.Failure);
        italianMove.Status.Should().Be(OrderStatus.Success);
        italianConvoy1.Status.Should().Be(OrderStatus.Success);
        italianConvoy2.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Lon", false),
                (nation1, UnitType.Fleet, "Wal", false),
                (nation2, UnitType.Army, "Bre", false),
                (nation2, UnitType.Fleet, "ENG", true),
                (nation3, UnitType.Fleet, "IRI", false),
                (nation3, UnitType.Fleet, "MAO", false),
                (nation3, UnitType.Army, "Naf", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.16. Pandin's paradox")]
    public void DATC_F_16()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Lon"),
                (nation1, UnitType.Fleet, "Wal"),
                (nation2, UnitType.Army, "Bre"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation3, UnitType.Fleet, "NTH"),
                (nation3, UnitType.Fleet, "Bel"),
            ]);

        var englishMove = units.Get("Wal").Move("ENG");
        var englishSupport = units.Get("Lon").Support(units.Get("Wal"), "ENG");
        var frenchMove = units.Get("Bre").Move("Lon");
        var frenchConvoy = units.Get("ENG").Convoy(units.Get("Bre"), "Lon");
        var germanMove = units.Get("Bel").Move("ENG");
        var germanSupport = units.Get("NTH").Support(units.Get("Bel"), "ENG");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchConvoy.Status.Should().Be(OrderStatus.Failure);
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Lon", false),
                (nation1, UnitType.Fleet, "Wal", false),
                (nation2, UnitType.Army, "Bre", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation3, UnitType.Fleet, "NTH", false),
                (nation3, UnitType.Fleet, "Bel", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.17. Pandin's extended paradox")]
    public void DATC_F_17()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Lon"),
                (nation1, UnitType.Fleet, "Wal"),
                (nation2, UnitType.Army, "Bre"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation2, UnitType.Fleet, "Yor"),
                (nation3, UnitType.Fleet, "NTH"),
                (nation3, UnitType.Fleet, "Bel"),
            ]);

        var englishMove = units.Get("Wal").Move("ENG");
        var englishSupport = units.Get("Lon").Support(units.Get("Wal"), "ENG");
        var frenchMove = units.Get("Bre").Move("Lon");
        var frenchSupport = units.Get("Yor").Support(units.Get("Bre"), "Lon");
        var frenchConvoy = units.Get("ENG").Convoy(units.Get("Bre"), "Lon");
        var germanMove = units.Get("Bel").Move("ENG");
        var germanSupport = units.Get("NTH").Support(units.Get("Bel"), "ENG");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchSupport.Status.Should().Be(OrderStatus.Failure);
        frenchConvoy.Status.Should().Be(OrderStatus.Failure);
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Lon", false),
                (nation1, UnitType.Fleet, "Wal", false),
                (nation2, UnitType.Army, "Bre", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation2, UnitType.Fleet, "Yor", false),
                (nation3, UnitType.Fleet, "NTH", false),
                (nation3, UnitType.Fleet, "Bel", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.18. Betrayal paradox")]
    public void DATC_F_18()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Lon"),
                (nation1, UnitType.Fleet, "ENG"),
                (nation2, UnitType.Fleet, "Bel"),
                (nation3, UnitType.Fleet, "HEL"),
                (nation3, UnitType.Fleet, "SKA"),
            ]);

        var englishMove = units.Get("Lon").Move("Bel");
        var englishSupport = units.Get("ENG").Support(units.Get("Lon"), "Bel");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Bel");
        var frenchSupport = units.Get("Bel").Support(units.Get("NTH"));
        var germanMove = units.Get("SKA").Move("NTH");
        var germanSupport = units.Get("HEL").Support(units.Get("SKA"), "NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        englishConvoy.Status.Should().Be(OrderStatus.Failure);
        frenchSupport.Status.Should().Be(OrderStatus.Success);
        germanMove.Status.Should().Be(OrderStatus.Failure);
        germanSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Army, "Lon", false),
                (nation1, UnitType.Fleet, "ENG", false),
                (nation2, UnitType.Fleet, "Bel", false),
                (nation3, UnitType.Fleet, "HEL", false),
                (nation3, UnitType.Fleet, "SKA", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.19. Multi-route convoy disruption paradox")]
    public void DATC_F_19()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation2, UnitType.Army, "Tun"),
                (nation2, UnitType.Fleet, "TYS"),
                (nation2, UnitType.Fleet, "ION"),
                (nation3, UnitType.Fleet, "Nap"),
                (nation3, UnitType.Fleet, "Rom"),
            ]);

        var frenchMove = units.Get("Tun").Move("Nap");
        var frenchConvoy1 = units.Get("TYS").Convoy(units.Get("Tun"), "Nap");
        var frenchConvoy2 = units.Get("ION").Convoy(units.Get("Tun"), "Nap");
        var italianMove = units.Get("Rom").Move("TYS");
        var italianSupport = units.Get("Nap").Support(units.Get("Rom"), "TYS");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchConvoy1.Status.Should().Be(OrderStatus.Failure);
        frenchConvoy2.Status.Should().Be(OrderStatus.Failure);
        italianMove.Status.Should().Be(OrderStatus.Failure);
        italianSupport.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation2, UnitType.Army, "Tun", false),
                (nation2, UnitType.Fleet, "TYS", false),
                (nation2, UnitType.Fleet, "ION", false),
                (nation3, UnitType.Fleet, "Nap", false),
                (nation3, UnitType.Fleet, "Rom", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.20. Unwanted multi-route convoy paradox")]
    public void DATC_F_20()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation2, UnitType.Army, "Tun"),
                (nation2, UnitType.Fleet, "TYS"),
                (nation3, UnitType.Fleet, "Nap"),
                (nation3, UnitType.Fleet, "ION"),
                (nation5, UnitType.Fleet, "AEG"),
                (nation5, UnitType.Fleet, "EAS"),
            ]);

        var frenchMove = units.Get("Tun").Move("Nap");
        var frenchConvoy = units.Get("TYS").Convoy(units.Get("Tun"), "Nap");
        var italianSupport = units.Get("Nap").Support(units.Get("ION"));
        var italianConvoy = units.Get("ION").Convoy(units.Get("Tun"), "Nap");
        var turkishMove = units.Get("EAS").Move("ION");
        var turkishSupport = units.Get("AEG").Support(units.Get("EAS"), "ION");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchConvoy.Status.Should().Be(OrderStatus.Failure);
        italianSupport.Status.Should().Be(OrderStatus.Failure);
        italianConvoy.Status.Should().Be(OrderStatus.Failure);
        turkishMove.Status.Should().Be(OrderStatus.Success);
        turkishSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation2, UnitType.Army, "Tun", false),
                (nation2, UnitType.Fleet, "TYS", false),
                (nation3, UnitType.Fleet, "Nap", false),
                (nation3, UnitType.Fleet, "ION", true),
                (nation5, UnitType.Fleet, "AEG", false),
                (nation5, UnitType.Fleet, "EAS", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.21. Dad's army convoy")]
    public void DATC_F_21()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation4, UnitType.Army, "Edi"),
                (nation4, UnitType.Fleet, "NWG"),
                (nation4, UnitType.Army, "Nwy"),
                (nation2, UnitType.Fleet, "IRI"),
                (nation2, UnitType.Fleet, "MAO"),
                (nation1, UnitType.Army, "Lvp"),
                (nation1, UnitType.Fleet, "NAO"),
                (nation1, UnitType.Fleet, "Cly"),
            ]);

        var russianMove = units.Get("Nwy").Move("Cly");
        var russianSupport = units.Get("Edi").Support(units.Get("Nwy"), "Cly");
        var russianConvoy = units.Get("NWG").Convoy(units.Get("Nwy"), "Cly");
        var frenchMove = units.Get("MAO").Move("NAO");
        var frenchSupport = units.Get("IRI").Support(units.Get("MAO"), "NAO");
        var englishMove = units.Get("Lvp").Move("Cly");
        var englishSupport = units.Get("Cly").Support(units.Get("NAO"));
        var englishConvoy = units.Get("NAO").Convoy(units.Get("Lvp"), "Cly");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        russianMove.Status.Should().Be(OrderStatus.Success);
        russianSupport.Status.Should().Be(OrderStatus.Success);
        russianConvoy.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Success);
        frenchSupport.Status.Should().Be(OrderStatus.Success);
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        englishConvoy.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation4, UnitType.Army, "Edi", false),
                (nation4, UnitType.Fleet, "NWG", false),
                (nation4, UnitType.Army, "Cly", false),
                (nation2, UnitType.Fleet, "IRI", false),
                (nation2, UnitType.Fleet, "NAO", false),
                (nation1, UnitType.Army, "Lvp", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.22. Second order paradox with two resolutions")]
    public void DATC_F_22()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Edi"),
                (nation1, UnitType.Fleet, "Lon"),
                (nation2, UnitType.Army, "Bre"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation3, UnitType.Fleet, "Bel"),
                (nation3, UnitType.Fleet, "Pic"),
                (nation4, UnitType.Army, "Nwy"),
                (nation4, UnitType.Fleet, "NTH"),
            ]);

        var englishMove = units.Get("Edi").Move("NTH");
        var englishSupport = units.Get("Lon").Support(units.Get("Edi"), "NTH");
        var frenchMove = units.Get("Bre").Move("Lon");
        var frenchConvoy = units.Get("ENG").Convoy(units.Get("Bre"), "Lon");
        var germanMove = units.Get("Pic").Move("ENG");
        var germanSupport = units.Get("Bel").Support(units.Get("Pic"), "ENG");
        var russianMove = units.Get("Nwy").Move("Bel");
        var russianConvoy = units.Get("NTH").Convoy(units.Get("Nwy"), "Bel");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishSupport.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchConvoy.Status.Should().Be(OrderStatus.Failure);
        germanMove.Status.Should().Be(OrderStatus.Success);
        germanSupport.Status.Should().Be(OrderStatus.Success);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianConvoy.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Edi", false),
                (nation1, UnitType.Fleet, "Lon", false),
                (nation2, UnitType.Army, "Bre", false),
                (nation2, UnitType.Fleet, "ENG", true),
                (nation3, UnitType.Fleet, "Bel", false),
                (nation3, UnitType.Fleet, "Pic", false),
                (nation4, UnitType.Army, "Nwy", false),
                (nation4, UnitType.Fleet, "NTH", true),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.23. Second order paradox with two exclusive convoys")]
    public void DATC_F_23()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Edi"),
                (nation1, UnitType.Fleet, "Yor"),
                (nation2, UnitType.Army, "Bre"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation3, UnitType.Fleet, "Bel"),
                (nation3, UnitType.Fleet, "Lon"),
                (nation5, UnitType.Fleet, "MAO"),
                (nation5, UnitType.Fleet, "IRI"),
                (nation4, UnitType.Army, "Nwy"),
                (nation4, UnitType.Fleet, "NTH"),
            ]);

        var englishMove = units.Get("Edi").Move("NTH");
        var englishSupport = units.Get("Yor").Support(units.Get("Edi"), "NTH");
        var frenchMove = units.Get("Bre").Move("Lon");
        var frenchConvoy = units.Get("ENG").Convoy(units.Get("Bre"), "Lon");
        var germanSupport1 = units.Get("Bel").Support(units.Get("ENG"));
        var germanSupport2 = units.Get("Lon").Support(units.Get("NTH"));
        var italianMove = units.Get("MAO").Move("ENG");
        var italianSupport = units.Get("IRI").Support(units.Get("MAO"), "ENG");
        var russianMove = units.Get("Nwy").Move("Bel");
        var russianConvoy = units.Get("NTH").Convoy(units.Get("Nwy"), "Bel");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishSupport.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchConvoy.Status.Should().Be(OrderStatus.Failure);
        germanSupport1.Status.Should().Be(OrderStatus.Success);
        germanSupport2.Status.Should().Be(OrderStatus.Success);
        italianMove.Status.Should().Be(OrderStatus.Failure);
        italianSupport.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianConvoy.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Edi", false),
                (nation1, UnitType.Fleet, "Yor", false),
                (nation2, UnitType.Army, "Bre", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation3, UnitType.Fleet, "Bel", false),
                (nation3, UnitType.Fleet, "Lon", false),
                (nation5, UnitType.Fleet, "MAO", false),
                (nation5, UnitType.Fleet, "IRI", false),
                (nation4, UnitType.Army, "Nwy", false),
                (nation4, UnitType.Fleet, "NTH", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.24. Second order paradox with no resolution")]
    public void DATC_F_24()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "Edi"),
                (nation1, UnitType.Fleet, "Lon"),
                (nation1, UnitType.Fleet, "IRI"),
                (nation1, UnitType.Fleet, "MAO"),
                (nation2, UnitType.Army, "Bre"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation2, UnitType.Fleet, "Bel"),
                (nation4, UnitType.Army, "Nwy"),
                (nation4, UnitType.Fleet, "NTH"),
            ]);

        var englishMove1 = units.Get("Edi").Move("NTH");
        var englishMove2 = units.Get("IRI").Move("ENG");
        var englishSupport1 = units.Get("Lon").Support(units.Get("Edi"), "NTH");
        var englishSupport2 = units.Get("MAO").Support(units.Get("IRI"), "ENG");
        var frenchMove = units.Get("Bre").Move("Lon");
        var frenchSupport = units.Get("Bel").Support(units.Get("ENG"));
        var frenchConvoy = units.Get("ENG").Convoy(units.Get("Bre"), "Lon");
        var russianMove = units.Get("Nwy").Move("Bel");
        var russianConvoy = units.Get("NTH").Convoy(units.Get("Nwy"), "Bel");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove1.Status.Should().Be(OrderStatus.Success);
        englishMove2.Status.Should().Be(OrderStatus.Failure);
        englishSupport1.Status.Should().Be(OrderStatus.Success);
        englishSupport2.Status.Should().Be(OrderStatus.Failure);
        frenchMove.Status.Should().Be(OrderStatus.Failure);
        frenchSupport.Status.Should().Be(OrderStatus.Success);
        frenchConvoy.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        russianConvoy.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "Edi", false),
                (nation1, UnitType.Fleet, "Lon", false),
                (nation1, UnitType.Fleet, "IRI", false),
                (nation1, UnitType.Fleet, "MAO", false),
                (nation2, UnitType.Army, "Bre", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation2, UnitType.Fleet, "Bel", false),
                (nation4, UnitType.Army, "Nwy", false),
                (nation4, UnitType.Fleet, "NTH", true),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC F.25. Cut support last")]
    public void DATC_F_25()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation3, UnitType.Army, "Ruh"),
                (nation3, UnitType.Army, "Hol"),
                (nation3, UnitType.Army, "Den"),
                (nation3, UnitType.Fleet, "SKA"),
                (nation3, UnitType.Army, "Fin"),
                (nation1, UnitType.Army, "Yor"),
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Fleet, "HEL"),
                (nation1, UnitType.Army, "Bel"),
                (nation4, UnitType.Fleet, "NWG"),
                (nation4, UnitType.Fleet, "Nwy"),
                (nation4, UnitType.Fleet, "Swe"),
            ]);

        var germanMove1 = units.Get("Ruh").Move("Bel");
        var germanMove2 = units.Get("Den").Move("Nwy");
        var germanSupport1 = units.Get("Hol").Support(units.Get("Ruh"), "Bel");
        var germanSupport2 = units.Get("Fin").Support(units.Get("Den"), "Nwy");
        var germanConvoy = units.Get("SKA").Convoy(units.Get("Den"), "Nwy");
        var englishHold = units.Get("Bel").Hold();
        var englishMove = units.Get("Yor").Move("Hol");
        var englishSupport = units.Get("HEL").Support(units.Get("Yor"), "Hol");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Yor"), "Hol");
        var russianMove1 = units.Get("NWG").Move("NTH");
        var russianMove2 = units.Get("Swe").Move("SKA");
        var russianSupport = units.Get("Nwy").Support(units.Get("NWG"), "NTH");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        germanMove1.Status.Should().Be(OrderStatus.Failure);
        germanMove2.Status.Should().Be(OrderStatus.Success);
        germanSupport1.Status.Should().Be(OrderStatus.Failure);
        germanSupport2.Status.Should().Be(OrderStatus.Success);
        germanConvoy.Status.Should().Be(OrderStatus.Success);
        englishHold.Status.Should().Be(OrderStatus.Success);
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishSupport.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        russianMove1.Status.Should().Be(OrderStatus.Failure);
        russianMove2.Status.Should().Be(OrderStatus.Failure);
        russianSupport.Status.Should().Be(OrderStatus.Failure);

        board.ShouldHaveUnits(
            [
                (nation3, UnitType.Army, "Ruh", false),
                (nation3, UnitType.Army, "Hol", true),
                (nation3, UnitType.Army, "Den", false),
                (nation3, UnitType.Fleet, "SKA", false),
                (nation3, UnitType.Army, "Fin", false),
                (nation1, UnitType.Army, "Yor", false),
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Fleet, "HEL", false),
                (nation1, UnitType.Army, "Bel", false),
                (nation4, UnitType.Fleet, "NWG", false),
                (nation4, UnitType.Fleet, "Nwy", true),
                (nation4, UnitType.Fleet, "Swe", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }
}
