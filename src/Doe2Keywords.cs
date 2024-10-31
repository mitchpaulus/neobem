using System;
using System.Collections.Generic;
using System.Linq;

namespace src;

public static class Doe2Keywords
{

    public static HashSet<string> Keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "INPUT", "TITLE", "DIAGNOSTIC", "ABORT", "RUN-PERIOD", "SCHEDULE", "WEEK-SCHEDULE", "DAY-SCHEDULE",
        "SITE-PARAMETERS", "BUILD-PARAMETERS", "LAYERS", "CONSTRUCTION", "GLASS-TYPE", "SPACE", "EXTERIOR-WALL", "ROOF", "WINDOW", "DOOR", "INTERIOR-WALL", "UNDERGROUND-WALL", "UNDERGROUND-FLOOR", "LOADS-REPORT",
        "SYSTEM", "ZONE", "DAY-RESET-SCH", "RESET-SCHEDULE", "SYSTEMS-REPORT",
        "ELEC-METER", "FUEL-METER", "MASTER-METERS", "PUMP", "CIRCULATION-LOOP", "BOILER", "CHILLER", "HEAT-REJECTION", "DW-HEATER",
        "UTILITY-RATE", "BLOCK-CHARGE", "ECONOMICS-REPORT",
    };
}

public class Doe2Printer
{
    private readonly ObjectVariableReplacer _replacer;
    private readonly List<Dictionary<string, Expression>> _environments;
    private readonly FileType _fileType;

    public Doe2Printer(ObjectVariableReplacer replacer, List<Dictionary<string, Expression>> environments, FileType fileType)
    {
        _replacer = replacer;
        _environments = environments;
        _fileType = fileType;
    }

    public string PrettyPrint(NeobemParser.Doe2objectContext context)
    {
        var (replaced, errors) = _replacer.Replace(context.GetText(), _environments, _fileType);

        if (errors.Count > 0)
        {
            throw new Exception(errors.Select(error => error.WriteError()).LineListConcat());
        }

        return replaced;
    }
}
