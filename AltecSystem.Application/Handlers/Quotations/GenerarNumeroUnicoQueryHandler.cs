using AltecSystem.Application.Interfaces;
using AltecSystem.Application.Queries.Quotations;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AltecSystem.Application.Handlers.Quotations
{
    public class GenerarNumeroUnicoQueryHandler : IRequestHandler<GenerarNumeroUnicoQuery, string>
    {
        private readonly IQuotationRepository _repository;
        private static readonly Random _random = new Random();

        public GenerarNumeroUnicoQueryHandler(IQuotationRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> Handle(GenerarNumeroUnicoQuery request, CancellationToken cancellationToken)
        {
            string numero;
            int intentos = 0;
            const int maxIntentos = 10;

            do
            {
                numero = _random.Next(10000, 99999).ToString();
                intentos++;

                if (intentos > maxIntentos)
                    throw new InvalidOperationException("No se pudo generar un número de cotización único. Intente nuevamente.");
            }
            while (await _repository.ExisteAsync(numero));

            return numero;
        }
    }
}
