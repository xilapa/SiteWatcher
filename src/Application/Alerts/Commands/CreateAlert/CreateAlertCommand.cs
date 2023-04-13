using MediatR;
using SiteWatcher.Application.Common.Commands;
using SiteWatcher.Application.Common.Constants;
using SiteWatcher.Common.Repositories;
using SiteWatcher.Common.Services;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Alerts.Repositories;
using SiteWatcher.Domain.Authentication;
using SiteWatcher.Domain.Common;

namespace SiteWatcher.Application.Alerts.Commands.CreateAlert;

public class CreateAlertCommand : IRequest<CommandResult>
{
    public string? Name { get; set; }
    public Frequencies? Frequency { get; set; }
    public string? SiteName { get; set; }
    public string? SiteUri { get; set; }
    public Rules? Rule { get; set; }

    // Term watch
    public string? Term { get; set; }

    // Regex watch
    public bool? NotifyOnDisappearance { get; set; }
    public string? RegexPattern { get; set; }

    public static implicit operator CreateAlertInput(CreateAlertCommand command) =>
        new(command.Name!,
            command.Frequency!.Value,
            command.SiteName!,
            command.SiteUri!,
            command.Rule!.Value,
            command.Term,
            command.NotifyOnDisappearance,
            command.RegexPattern);

    public List<string> IsValid()
    {
        var errs = new List<string>();

        // Name validation
        if (string.IsNullOrEmpty(Name))
            errs.Add(ApplicationErrors.ValueIsNullOrEmpty(nameof(Name)));
        if (Name?.Length < 3)
            errs.Add(ApplicationErrors.ValueBellowMinimumLength(nameof(Name)));
        if (Name?.Length > 64)
            errs.Add(ApplicationErrors.ValueAboveMaximumLength(nameof(Name)));

        // Frequency validation
        if (Frequency == null || !Enum.IsDefined(typeof(Frequencies), (int) Frequency))
            errs.Add(ApplicationErrors.ValueIsInvalid(nameof(Frequency)));

        // Site name validation
        if (string.IsNullOrEmpty(SiteName))
            errs.Add(ApplicationErrors.ValueIsNullOrEmpty(nameof(SiteName)));
        if (SiteName?.Length < 3)
            errs.Add(ApplicationErrors.ValueBellowMinimumLength(nameof(SiteName)));
        if (SiteName?.Length > 64)
            errs.Add(ApplicationErrors.ValueAboveMaximumLength(nameof(SiteName)));

        // Site URI validation
        if (string.IsNullOrEmpty(SiteUri) || !Uri.IsWellFormedUriString(SiteUri, UriKind.Absolute))
            errs.Add(ApplicationErrors.ValueIsInvalid(nameof(SiteUri)));

        // Rule validation
        if (Rule == null || !Enum.IsDefined(typeof(Rules), (int) Rule))
            errs.Add(ApplicationErrors.ValueIsInvalid(nameof(Rule)));

        // Term rule validation
        if (Rule == Rules.Term && string.IsNullOrEmpty(Term))
            errs.Add(ApplicationErrors.ValueIsNullOrEmpty(nameof(Term)));
        if (Rule == Rules.Term && Term?.Length < 3)
            errs.Add(ApplicationErrors.ValueBellowMinimumLength(nameof(Term)));
        if (Rule == Rules.Term && Term?.Length > 64)
            errs.Add(ApplicationErrors.ValueAboveMaximumLength(nameof(Term)));

        // Regex rule validation
        if (Rule == Rules.Regex && !Utils.IsRegexValid(RegexPattern))
            errs.Add(ApplicationErrors.ValueIsInvalid(nameof(RegexPattern)));
        if (Rule == Rules.Regex && RegexPattern?.Length > 512)
            errs.Add(ApplicationErrors.ValueAboveMaximumLength(nameof(RegexPattern)));
        if (Rule == Rules.Regex && NotifyOnDisappearance == null)
            errs.Add(ApplicationErrors.ValueIsInvalid(nameof(NotifyOnDisappearance)));

        return errs;
    }
}

public class CreateAlertCommandHandler : IRequestHandler<CreateAlertCommand, CommandResult>
{
    private readonly ISession _session;
    private readonly IAlertRepository _alertRepository;
    private readonly IUnitOfWork _uow;
    private readonly IIdHasher _idHasher;

    public CreateAlertCommandHandler(ISession session, IAlertRepository alertRepository, IUnitOfWork uow,
        IIdHasher idHasher)
    {
        _session = session;
        _alertRepository = alertRepository;
        _uow = uow;
        _idHasher = idHasher;
    }

    public async Task<CommandResult> Handle(CreateAlertCommand request, CancellationToken cancellationToken)
    {
        var errs = request.IsValid();
        if (errs.Count != 0)
            return CommandResult.FromErrors(errs);

        var alert = AlertFactory.Create(request, _session.UserId!.Value, _session.Now);
        _alertRepository.Add(alert);
        await _uow.SaveChangesAsync(cancellationToken);
        return CommandResult.FromValue(DetailedAlertView.FromAlert(alert, _idHasher));
    }
}