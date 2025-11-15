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
    Money Price,
    ICollection<Guid> CategoryIds) : ICommand<Guid>;
