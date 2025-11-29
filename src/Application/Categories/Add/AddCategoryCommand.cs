using Application.Abstractions.Messaging;

namespace Application.Categories.Add;

public sealed record AddCategoryCommand(string Name) : ICommand<Guid>;
