using AltecSystem.Application.Commands.ElectronicInvoices;
using AltecSystem.Application.DTOs.ElectronicInvoices;
using AltecSystem.Application.Queries.ElectronicInvoices;
using AltecSystem.Application.Services.Sri;
using MediatR;

namespace AltecSystem.Application.Handlers.ElectronicInvoices;

public class CreateElectronicInvoiceHandler : IRequestHandler<CreateElectronicInvoiceCommand, ElectronicInvoiceResultDto>
{
    private readonly ElectronicInvoiceProcessor _processor;

    public CreateElectronicInvoiceHandler(ElectronicInvoiceProcessor processor)
    {
        _processor = processor;
    }

    public Task<ElectronicInvoiceResultDto> Handle(CreateElectronicInvoiceCommand request, CancellationToken cancellationToken)
        => _processor.CreateAsync(request.Request, cancellationToken);
}

public class RetryElectronicInvoiceHandler : IRequestHandler<RetryElectronicInvoiceCommand, ElectronicInvoiceResultDto>
{
    private readonly ElectronicInvoiceProcessor _processor;

    public RetryElectronicInvoiceHandler(ElectronicInvoiceProcessor processor)
    {
        _processor = processor;
    }

    public Task<ElectronicInvoiceResultDto> Handle(RetryElectronicInvoiceCommand request, CancellationToken cancellationToken)
        => _processor.RetryAsync(request.Id, cancellationToken);
}

public class GetElectronicInvoiceHandler : IRequestHandler<GetElectronicInvoiceQuery, ElectronicInvoiceResultDto?>
{
    private readonly ElectronicInvoiceProcessor _processor;

    public GetElectronicInvoiceHandler(ElectronicInvoiceProcessor processor)
    {
        _processor = processor;
    }

    public Task<ElectronicInvoiceResultDto?> Handle(GetElectronicInvoiceQuery request, CancellationToken cancellationToken)
        => _processor.GetAsync(request.Id, cancellationToken);
}

public class SendElectronicInvoiceEmailHandler : IRequestHandler<SendElectronicInvoiceEmailCommand, ElectronicInvoiceResultDto>
{
    private readonly ElectronicInvoiceProcessor _processor;

    public SendElectronicInvoiceEmailHandler(ElectronicInvoiceProcessor processor)
    {
        _processor = processor;
    }

    public Task<ElectronicInvoiceResultDto> Handle(SendElectronicInvoiceEmailCommand request, CancellationToken cancellationToken)
        => _processor.SendEmailAsync(request.Id, request.Email, cancellationToken);
}

public class GetElectronicInvoicePdfHandler : IRequestHandler<GetElectronicInvoicePdfQuery, (byte[] Pdf, string NumeroFactura)?>
{
    private readonly ElectronicInvoiceProcessor _processor;

    public GetElectronicInvoicePdfHandler(ElectronicInvoiceProcessor processor)
    {
        _processor = processor;
    }

    public Task<(byte[] Pdf, string NumeroFactura)?> Handle(GetElectronicInvoicePdfQuery request, CancellationToken cancellationToken)
        => _processor.GetPdfAsync(request.Id, cancellationToken);
}
