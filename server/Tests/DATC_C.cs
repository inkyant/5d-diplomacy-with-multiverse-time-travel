﻿using Adjudication;
using Entities;
using Enums;
using FluentAssertions;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Tests;

// Diplomacy Adjudicator Test Cases, Lucas B. Kruijswijk
// https://boardgamegeek.com/filepage/274846/datc-diplomacy-adjudicator-test-cases

[SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores")]
public class DATC_C : AdjudicationTestBase
{
    [Fact(DisplayName = "DATC C.01. Three army circular movement")]
    public void DATC_C_1()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation5, UnitType.Fleet, "Ank"),
                (nation5, UnitType.Army, "Con"),
                (nation5, UnitType.Army, "Smy"),
            ]);

        var move1 = units.Get("Ank").Move("Con");
        var move2 = units.Get("Con").Move("Smy");
        var move3 = units.Get("Smy").Move("Ank");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move1.Status.Should().Be(OrderStatus.Success);
        move2.Status.Should().Be(OrderStatus.Success);
        move3.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation5, UnitType.Fleet, "Con", false),
                (nation5, UnitType.Army, "Smy", false),
                (nation5, UnitType.Army, "Ank", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC C.02. Three army circular movement with support")]
    public void DATC_C_2()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation5, UnitType.Fleet, "Ank"),
                (nation5, UnitType.Army, "Con"),
                (nation5, UnitType.Army, "Smy"),
                (nation5, UnitType.Army, "Bul"),
            ]);

        var move1 = units.Get("Ank").Move("Con");
        var move2 = units.Get("Con").Move("Smy");
        var move3 = units.Get("Smy").Move("Ank");
        var support = units.Get("Bul").Support(units.Get("Ank"), "Con");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move1.Status.Should().Be(OrderStatus.Success);
        move2.Status.Should().Be(OrderStatus.Success);
        move3.Status.Should().Be(OrderStatus.Success);
        support.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation5, UnitType.Fleet, "Con", false),
                (nation5, UnitType.Army, "Smy", false),
                (nation5, UnitType.Army, "Ank", false),
                (nation5, UnitType.Army, "Bul", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC C.03. A disrupted three army circular movement")]
    public void DATC_C_3()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation5, UnitType.Fleet, "Ank"),
                (nation5, UnitType.Army, "Con"),
                (nation5, UnitType.Army, "Smy"),
                (nation5, UnitType.Army, "Bul"),
            ]);

        var move1 = units.Get("Ank").Move("Con");
        var move2 = units.Get("Con").Move("Smy");
        var move3 = units.Get("Smy").Move("Ank");
        var move4 = units.Get("Bul").Move("Con");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        move1.Status.Should().Be(OrderStatus.Failure);
        move2.Status.Should().Be(OrderStatus.Failure);
        move3.Status.Should().Be(OrderStatus.Failure);
        move4.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation5, UnitType.Fleet, "Ank", false),
                (nation5, UnitType.Army, "Con", false),
                (nation5, UnitType.Army, "Smy", false),
                (nation5, UnitType.Army, "Bul", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC C.04. A circular movement with attacked convoy")]
    public void DATC_C_4()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation5, UnitType.Army, "Tri"),
                (nation5, UnitType.Army, "Ser"),
                (nation5, UnitType.Army, "Bul"),
                (nation5, UnitType.Fleet, "AEG"),
                (nation5, UnitType.Fleet, "ION"),
                (nation5, UnitType.Fleet, "ADR"),
                (nation3, UnitType.Fleet, "Nap"),
            ]);

        var austrianMove1 = units.Get("Tri").Move("Ser");
        var austrianMove2 = units.Get("Ser").Move("Bul");
        var turkishMove = units.Get("Bul").Move("Tri");
        var turkishConvoy1 = units.Get("AEG").Convoy(units.Get("Bul"), "Tri");
        var turkishConvoy2 = units.Get("ION").Convoy(units.Get("Bul"), "Tri");
        var turkishConvoy3 = units.Get("ADR").Convoy(units.Get("Bul"), "Tri");
        var italianMove = units.Get("Nap").Move("ION");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianMove1.Status.Should().Be(OrderStatus.Success);
        austrianMove2.Status.Should().Be(OrderStatus.Success);
        turkishMove.Status.Should().Be(OrderStatus.Success);
        turkishConvoy1.Status.Should().Be(OrderStatus.Success);
        turkishConvoy2.Status.Should().Be(OrderStatus.Success);
        turkishConvoy3.Status.Should().Be(OrderStatus.Success);
        italianMove.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation5, UnitType.Army, "Ser", false),
                (nation5, UnitType.Army, "Bul", false),
                (nation5, UnitType.Army, "Tri", false),
                (nation5, UnitType.Fleet, "AEG", false),
                (nation5, UnitType.Fleet, "ION", false),
                (nation5, UnitType.Fleet, "ADR", false),
                (nation3, UnitType.Fleet, "Nap", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC C.05. A disrupted circular movement due to dislodged convoy")]
    public void DATC_C_5()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation5, UnitType.Army, "Tri"),
                (nation5, UnitType.Army, "Ser"),
                (nation5, UnitType.Army, "Bul"),
                (nation5, UnitType.Fleet, "AEG"),
                (nation5, UnitType.Fleet, "ION"),
                (nation5, UnitType.Fleet, "ADR"),
                (nation3, UnitType.Fleet, "Nap"),
                (nation3, UnitType.Fleet, "Tun"),
            ]);

        var austrianMove1 = units.Get("Tri").Move("Ser");
        var austrianMove2 = units.Get("Ser").Move("Bul");
        var turkishMove = units.Get("Bul").Move("Tri");
        var turkishConvoy1 = units.Get("AEG").Convoy(units.Get("Bul"), "Tri");
        var turkishConvoy2 = units.Get("ION").Convoy(units.Get("Bul"), "Tri");
        var turkishConvoy3 = units.Get("ADR").Convoy(units.Get("Bul"), "Tri");
        var italianMove = units.Get("Nap").Move("ION");
        var italianSupport = units.Get("Tun").Support(units.Get("Nap"), "ION");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        austrianMove1.Status.Should().Be(OrderStatus.Failure);
        austrianMove2.Status.Should().Be(OrderStatus.Failure);
        turkishMove.Status.Should().Be(OrderStatus.Failure);
        turkishConvoy1.Status.Should().Be(OrderStatus.Failure);
        turkishConvoy2.Status.Should().Be(OrderStatus.Failure);
        turkishConvoy3.Status.Should().Be(OrderStatus.Failure);
        italianMove.Status.Should().Be(OrderStatus.Success);
        italianSupport.Status.Should().Be(OrderStatus.Success);

        board.ShouldHaveUnits(
            [
                (nation5, UnitType.Army, "Tri", false),
                (nation5, UnitType.Army, "Ser", false),
                (nation5, UnitType.Army, "Bul", false),
                (nation5, UnitType.Fleet, "AEG", false),
                (nation5, UnitType.Fleet, "ION", true),
                (nation5, UnitType.Fleet, "ADR", false),
                (nation3, UnitType.Fleet, "Nap", false),
                (nation3, UnitType.Fleet, "Tun", false),
            ]);
        board.ShouldNotHaveNextBoard();

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC C.06. Two armies with two convoys")]
    public void DATC_C_6()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Lon"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation2, UnitType.Army, "Bel"),
            ]);

        var englishMove = units.Get("Lon").Move("Bel");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Bel");
        var frenchMove = units.Get("Bel").Move("Lon");
        var frenchConvoy = units.Get("ENG").Convoy(units.Get("Bel"), "Lon");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Success);
        englishConvoy.Status.Should().Be(OrderStatus.Success);
        frenchMove.Status.Should().Be(OrderStatus.Success);
        frenchConvoy.Status.Should().Be(OrderStatus.Success);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Army, "Bel", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation2, UnitType.Army, "Lon", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC C.07. Disrupted unit swap")]
    public void DATC_C_7()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation1, UnitType.Fleet, "NTH"),
                (nation1, UnitType.Army, "Lon"),
                (nation2, UnitType.Fleet, "ENG"),
                (nation2, UnitType.Army, "Bel"),
                (nation2, UnitType.Army, "Bur"),
            ]);

        var englishMove = units.Get("Lon").Move("Bel");
        var englishConvoy = units.Get("NTH").Convoy(units.Get("Lon"), "Bel");
        var frenchMove1 = units.Get("Bel").Move("Lon");
        var frenchMove2 = units.Get("Bur").Move("Bel");
        var frenchConvoy = units.Get("ENG").Convoy(units.Get("Bel"), "Lon");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        englishMove.Status.Should().Be(OrderStatus.Failure);
        englishConvoy.Status.Should().Be(OrderStatus.Failure);
        frenchMove1.Status.Should().Be(OrderStatus.Failure);
        frenchMove2.Status.Should().Be(OrderStatus.Failure);
        frenchConvoy.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation1, UnitType.Fleet, "NTH", false),
                (nation1, UnitType.Army, "Lon", false),
                (nation2, UnitType.Fleet, "ENG", false),
                (nation2, UnitType.Army, "Bel", false),
                (nation2, UnitType.Army, "Bur", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC C.08. No self dislodgement in disrupted circular movement")]
    public void DATC_C_8()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation5, UnitType.Fleet, "Con"),
                (nation5, UnitType.Army, "Bul"),
                (nation5, UnitType.Army, "Smy"),
                (nation4, UnitType.Fleet, "BLA"),
                (nation5, UnitType.Army, "Ser"),
            ]);

        var turkishMove1 = units.Get("Con").Move("BLA");
        var turkishMove2 = units.Get("Bul").Move("Con");
        var turkishSupport = units.Get("Smy").Support(units.Get("Bul"), "Con");
        var russianMove = units.Get("BLA").Move("Bul_E");
        var austrianMove = units.Get("Ser").Move("Bul");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        turkishMove1.Status.Should().Be(OrderStatus.Failure);
        turkishMove2.Status.Should().Be(OrderStatus.Failure);
        turkishSupport.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        austrianMove.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation5, UnitType.Fleet, "Con", false),
                (nation5, UnitType.Army, "Bul", false),
                (nation5, UnitType.Army, "Smy", false),
                (nation4, UnitType.Fleet, "BLA", false),
                (nation5, UnitType.Army, "Ser", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }

    [Fact(DisplayName = "DATC C.09. No help in dislodgement of own unit in disrupted circular movement")]
    public void DATC_C_9()
    {
        // Arrange
        var world = new World();
        var board = world.AddBoard();

        var units = board.AddUnits(
            [
                (nation5, UnitType.Fleet, "Con"),
                (nation5, UnitType.Army, "Smy"),
                (nation4, UnitType.Fleet, "BLA"),
                (nation5, UnitType.Army, "Ser"),
                (nation5, UnitType.Army, "Bul"),
            ]);

        var turkishMove = units.Get("Con").Move("BLA");
        var turkishSupport = units.Get("Smy").Support(units.Get("Bul"), "Con");
        var russianMove = units.Get("BLA").Move("Bul_E");
        var austrianMove1 = units.Get("Ser").Move("Bul");
        var austrianMove2 = units.Get("Bul").Move("Con");

        // Act
        new Adjudicator(world, false, MapFactory, DefaultWorldFactory).Adjudicate();

        // Assert
        turkishMove.Status.Should().Be(OrderStatus.Failure);
        turkishSupport.Status.Should().Be(OrderStatus.Failure);
        russianMove.Status.Should().Be(OrderStatus.Failure);
        austrianMove1.Status.Should().Be(OrderStatus.Failure);
        austrianMove2.Status.Should().Be(OrderStatus.Failure);

        board.Next().ShouldHaveUnits(
            [
                (nation5, UnitType.Fleet, "Con", false),
                (nation5, UnitType.Army, "Smy", false),
                (nation4, UnitType.Fleet, "BLA", false),
                (nation5, UnitType.Army, "Ser", false),
                (nation5, UnitType.Army, "Bul", false),
            ]);

        world.ShouldHaveAllOrdersResolved();
    }
}
