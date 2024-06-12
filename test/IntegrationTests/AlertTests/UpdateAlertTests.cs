using System.Net;
using FluentAssertions;
using IntegrationTests.Setup;
using Microsoft.EntityFrameworkCore;
using SiteWatcher.Application.Alerts.Commands.UpdateAlert;
using SiteWatcher.Domain.Alerts;
using SiteWatcher.Domain.Alerts.DTOs;
using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.Common.DTOs;
using SiteWatcher.Domain.Common.ValueObjects;
using SiteWatcher.Infra.IdHasher;
using SiteWatcher.IntegrationTests.Setup.TestServices;
using SiteWatcher.IntegrationTests.Setup.WebApplicationFactory;
using SiteWatcher.IntegrationTests.Utils;

namespace IntegrationTests.AlertTests;

public sealed class UpdateAlertTestsBase : BaseTestFixture
{
    public DetailedAlertView XilapaAlert;
    public Alert XulipaAlert;

    protected override void OnConfiguringTestServer(BaseTestFixtureOptionsBuilder optionsBuilder)
    {
        optionsBuilder.SetInitialDate(new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        XilapaAlert = await AppFactory.CreateAlert<DetailedAlertView>("Xilapa alert", RuleType.AnyChanges,
            Users.Xilapa.Id); // Id = 1

        XulipaAlert = await AppFactory.CreateAlert("Xulipa alert", RuleType.AnyChanges,
            Users.Xulipa.Id, AppFactory.CurrentTime.AddDays(-1)); // Id = 2
    }
}

public sealed class UpdateAlertTests : BaseTest, IClassFixture<UpdateAlertTestsBase>
{
    private readonly UpdateAlertTestsBase _fixture;

    public UpdateAlertTests(UpdateAlertTestsBase fixture) : base(fixture)
    {
        _fixture = fixture;
    }

    public static IEnumerable<object[]> UpdateData()
    {
        yield return new object[]
        {
            new UpdateAlertCommmand
            {
                AlertId = new IdHasher(new TestAppSettings()).HashId(1),
                Name = new UpdateInfo<string> {NewValue = "XilapaUpdatedAlert"},
                SiteName = new UpdateInfo<string> {NewValue = "Updated site name"},
                SiteUri = new UpdateInfo<string> {NewValue = "https://new-site-updated.com"},
                Frequency = new UpdateInfo<Frequencies> {NewValue = Frequencies.TwentyFourHours},
                Rule = new UpdateInfo<RuleType> {NewValue = RuleType.Term},
                Term = new UpdateInfo<string> {NewValue = "new term"}
            }
        };

        yield return new object[]
        {
            new UpdateAlertCommmand
            {
                AlertId = new IdHasher(new TestAppSettings()).HashId(1),
                Name = new UpdateInfo<string> {NewValue = "XilapaUpdatedAlert2"},
                SiteName = new UpdateInfo<string> {NewValue = "Updated site name2"},
                SiteUri = new UpdateInfo<string> {NewValue = "https://new-site-updated2.com"},
                Frequency = new UpdateInfo<Frequencies> {NewValue = Frequencies.TwentyFourHours},
                Rule = new UpdateInfo<RuleType> {NewValue = RuleType.Term},
                Term = new UpdateInfo<string> {NewValue = "new term2"}
            }
        };

        yield return new object[]
        {
            new UpdateAlertCommmand
            {
                AlertId = new IdHasher(new TestAppSettings()).HashId(1),
                Name = new UpdateInfo<string> {NewValue = "XilapaUpdatedAlert3"},
                SiteName = new UpdateInfo<string> {NewValue = "Updated site name3"},
                SiteUri = new UpdateInfo<string> {NewValue = "https://new-site-updated3.com"},
                Frequency = new UpdateInfo<Frequencies> {NewValue = Frequencies.FourHours},
                Rule = new UpdateInfo<RuleType> {NewValue = RuleType.Regex},
                RegexPattern = new UpdateInfo<string> {NewValue = "[0-9]"},
               NotifyOnDisappearance = new UpdateInfo<bool>(true)
            }
        };

        yield return new object[]
        {
            new UpdateAlertCommmand
            {
                AlertId = new IdHasher(new TestAppSettings()).HashId(1),
                Name = new UpdateInfo<string> {NewValue = "XilapaUpdatedAlert4"},
                SiteName = new UpdateInfo<string> {NewValue = "Updated site name4"},
                SiteUri = new UpdateInfo<string> {NewValue = "https://new-site-updated4.com"},
                Frequency = new UpdateInfo<Frequencies> {NewValue = Frequencies.FourHours},
                Rule = new UpdateInfo<RuleType> {NewValue = RuleType.Regex},
                RegexPattern = new UpdateInfo<string> {NewValue = "hello"},
                NotifyOnDisappearance = new UpdateInfo<bool>(false)
            }
        };

        yield return new object[]
        {
            new UpdateAlertCommmand
            {
                AlertId = new IdHasher(new TestAppSettings()).HashId(1),
                Name = new UpdateInfo<string> {NewValue = "XilapaUpdatedAlert5"},
                SiteName = new UpdateInfo<string> {NewValue = "Updated site name5"},
                SiteUri = new UpdateInfo<string> {NewValue = "https://new-site-updated5.com"},
                Frequency = new UpdateInfo<Frequencies> {NewValue = Frequencies.TwoHours},
                Rule = new UpdateInfo<RuleType> {NewValue = RuleType.AnyChanges}
            }
        };

        yield return new object[]
        {
            new UpdateAlertCommmand
            {
                AlertId = new IdHasher(new TestAppSettings()).HashId(1),
                Name = new UpdateInfo<string> {NewValue = "XilapaUpdatedAlert6"},
                SiteName = new UpdateInfo<string> {NewValue = "Updated site name6"},
                SiteUri = new UpdateInfo<string> {NewValue = "https://new-site-updated6.com"},
                Frequency = new UpdateInfo<Frequencies> {NewValue = Frequencies.FourHours},
                Rule = new UpdateInfo<RuleType> {NewValue = RuleType.AnyChanges},
                Term = new UpdateInfo<string> {NewValue = "new term6"}
            }
        };
    }

    [Theory]
    [MemberData(nameof(UpdateData))]
    public async Task AlertsAreUpdated(UpdateAlertCommmand updateCommand)
    {
        // Arrange
        LoginAs(Users.Xilapa);

        // Act
        var result = await PutAsync("alert", updateCommand);

        // Assert
        result.HttpResponse!.StatusCode.Should().Be(HttpStatusCode.OK);

        var detailedAlert = result.GetTyped<DetailedAlertView>();

        detailedAlert!.Id.Should().Be(_fixture.XilapaAlert.Id);
        detailedAlert.Name.Should().Be(updateCommand.Name!.NewValue);
        detailedAlert.Site!.Name.Should().Be(updateCommand.SiteName!.NewValue);
        detailedAlert.Site.Uri.Should().StartWith(updateCommand.SiteUri!.NewValue);
        detailedAlert.CreatedAt.Should().Be(_fixture.XilapaAlert.CreatedAt);
        detailedAlert.Frequency.Should().Be(updateCommand.Frequency!.NewValue);
        detailedAlert.Rule!.Rule.Should().Be(updateCommand.Rule!.NewValue);

        // Term rule
        detailedAlert.Rule.Term
            .Should()
            .Be(RuleType.Term.Equals(updateCommand.Rule!.NewValue) ? updateCommand.Term!.NewValue : null);

        // Regex rule
        detailedAlert.Rule.RegexPattern
            .Should()
            .Be(RuleType.Regex.Equals(updateCommand.Rule!.NewValue) ? updateCommand.RegexPattern!.NewValue : null);

        detailedAlert.Rule.NotifyOnDisappearance
            .Should()
            .Be(RuleType.Regex.Equals(updateCommand.Rule!.NewValue) ? updateCommand.NotifyOnDisappearance!.NewValue : null);

        // Checking alert update date on db
        (await AppFactory.WithDbContext(async ctx =>
                await ctx.Alerts
                    .SingleAsync(a => a.Id == new AlertId(1))))
            .LastUpdatedAt.Should().Be(CurrentTime);

        // Checking that Xulipa alert has not changed
        var xulipaAlert = await AppFactory.WithDbContext(async ctx =>
            await ctx.Alerts
                .Include(a => a.Rule)
                .AsSplitQuery()
                .SingleAsync(a => a.Id == new AlertId(2)));

        xulipaAlert.Should().BeEquivalentTo(_fixture.XulipaAlert);
    }
}