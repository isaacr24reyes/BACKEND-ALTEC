using AltecSystem.Application.DTOs.User;
using MediatR;

namespace AltecSystem.Application.Commands.User;

public class ResetAndAddBalanceCommand(string username, decimal newBalance) : IRequest<UserDetailsDto>
{
    public string Username { get; } = username;
    public decimal NewBalance { get; } = newBalance;
}