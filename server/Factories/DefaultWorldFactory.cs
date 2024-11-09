﻿using Entities;
using Enums;
using System.Text.Json;
using Utilities;

namespace Factories;

public class DefaultWorldFactory
{
    private const string CentresFilePath = "Data/centres.json";
    private const string UnitsFilePath = "Data/units.json";

    public World CreateWorld()
    {
        var world = new World
        {
            Boards = [CreateBoard()],
            Orders = [],
        };

        Check(world);
        return world;
    }

    public Board CreateBoard() => new()
    {
        Timeline = 1,
        Year = 1900,
        Phase = Phase.Winter,
        ChildTimelines = [],
        Centres = CreateCentres(),
        Units = CreateUnits(),
    };

    public List<Centre> CreateCentres()
    {
        var centresFile = File.ReadAllText(CentresFilePath);
        var centres = JsonSerializer.Deserialize<List<Centre>>(centresFile, Constants.JsonOptions)
            ?? throw new JsonException("Deserialised centres file was null");

        foreach (var centre in centres)
        {
            centre.Location.Timeline = 1;
            centre.Location.Year = 1900;
            centre.Location.Phase = Phase.Winter;
        }

        return centres;
    }

    public List<Unit> CreateUnits()
    {
        var unitsFile = File.ReadAllText(UnitsFilePath);
        var units = JsonSerializer.Deserialize<List<Unit>>(unitsFile, Constants.JsonOptions)
            ?? throw new JsonException("Deserialised units file was null");

        foreach (var unit in units)
        {
            unit.Location.Timeline = 1;
            unit.Location.Year = 1900;
            unit.Location.Phase = Phase.Winter;
        }

        return units;
    }

    private void Check(World world)
    {
        var centres = world.Boards[0].Centres;
        var units = world.Boards[0].Units;

        foreach (var centre in centres)
        {
            if (centres.Where(c => c.Location.RegionId == centre.Location.RegionId).Count() > 1)
            {
                throw new JsonException($"Non-unique ID {centre.Location.RegionId} found");
            }
        }

        foreach (var unit in units)
        {
            if (units.Where(u => u.Location.RegionId == unit.Location.RegionId).Count() > 1)
            {
                throw new JsonException($"Non-unique ID {unit.Location.RegionId} found");
            }

            if (centres.Where(c => c.Location.RegionId == unit.Location.RegionId || unit.Location.RegionId.Split("_")[0] == c.Location.RegionId).Count() != 1)
            {
                throw new JsonException($"Unit {unit.Location.RegionId} with invalid centre count found");
            }
        }
    }
}

