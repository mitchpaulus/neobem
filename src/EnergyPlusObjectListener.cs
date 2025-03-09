using System;
using System.Collections.Generic;
using System.Linq;

namespace src;

public class EnergyPlusObjectListener : NeobemParserBaseListener
{
    public override void EnterObject(NeobemParser.ObjectContext context)
    {
        Console.Write(context.OBJECT_TYPE().GetText().Replace("\t", "\\t"));
        foreach (var f in context.FIELD())
        {
            Console.Write('\t');
            Console.Write(f.GetText().Replace("\t", "\\t"));
        }
        Console.Write('\n');
    }
}