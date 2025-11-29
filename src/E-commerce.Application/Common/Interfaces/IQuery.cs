using MediatR;

namespace E_commerce.Application.Common.Interfaces;

/// <summary>
/// Marker interface for queries
/// </summary>
/// <typeparam name="TResponse">The response type</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
