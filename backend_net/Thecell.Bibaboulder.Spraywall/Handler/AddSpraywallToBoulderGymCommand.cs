using System;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class AddSpraywallToBoulderGymCommand
{
    public Guid BoulderGymId { get; set; }
    public Guid SpraywallId { get; set; }
}
