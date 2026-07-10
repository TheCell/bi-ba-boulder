using System;
using Thecell.Bibaboulder.Common.Commands;
using Thecell.Bibaboulder.Model.Enums;
using Thecell.Bibaboulder.Model.Model;

namespace Thecell.Bibaboulder.Outdoor.Handler;

public class CreateLineCommand : CreateCommand
{
    public required Guid BlocId { get; set; }

    public required string Identifier { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public FontGrade? FontGrade { get; set; }

    public required LineData Data { get; set; }
}