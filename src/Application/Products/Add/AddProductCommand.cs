using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Application.Abstractions.Messaging;
using SharedKernel.ValueObjects;

namespace Application.Products.Add;
public sealed record AddProductCommand(
    string Name,
    string Description,
    int Quantity,
    Money Price,
    ICollection<Guid> CategoryIds,
    string MainPhotoPath) : ICommand<Guid>;
