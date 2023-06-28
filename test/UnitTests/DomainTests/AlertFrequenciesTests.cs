using SiteWatcher.Domain.Alerts.Enums;
using SiteWatcher.Domain.DomainServices;
using SiteWatcher.IntegrationTests.Utils;

namespace UnitTests.DomainTests;

public sealed class AlertFrequenciesTests
{
    private static readonly List<Frequencies> _emptyFrequencyList = new();

    public static TheoryData<DateTime, List<Frequencies>> CorrectFrequenciesData = new()
    {
        {DateTimeUtils.GetTimeWithHour(0), new List<Frequencies> {Frequencies.TwoHours, Frequencies.FourHours, Frequencies.EightHours, Frequencies.TwelveHours, Frequencies.TwentyFourHours}},
        {DateTimeUtils.GetTimeWithHour(1), _emptyFrequencyList},
        {DateTimeUtils.GetTimeWithHour(2), new List<Frequencies> {Frequencies.TwoHours}},
        {DateTimeUtils.GetTimeWithHour(3), _emptyFrequencyList},
        {DateTimeUtils.GetTimeWithHour(4), new List<Frequencies> {Frequencies.TwoHours, Frequencies.FourHours}},
        {DateTimeUtils.GetTimeWithHour(5), _emptyFrequencyList},
        {DateTimeUtils.GetTimeWithHour(6), new List<Frequencies> {Frequencies.TwoHours}},
        {DateTimeUtils.GetTimeWithHour(7), _emptyFrequencyList},
        {DateTimeUtils.GetTimeWithHour(8), new List<Frequencies> {Frequencies.TwoHours, Frequencies.FourHours, Frequencies.EightHours}},
        {DateTimeUtils.GetTimeWithHour(9), _emptyFrequencyList},
        {DateTimeUtils.GetTimeWithHour(10), new List<Frequencies> {Frequencies.TwoHours}},
        {DateTimeUtils.GetTimeWithHour(11), _emptyFrequencyList},
        {DateTimeUtils.GetTimeWithHour(12), new List<Frequencies> {Frequencies.TwoHours, Frequencies.FourHours, Frequencies.TwelveHours}},
        {DateTimeUtils.GetTimeWithHour(13), _emptyFrequencyList},
        {DateTimeUtils.GetTimeWithHour(14), new List<Frequencies> {Frequencies.TwoHours}},
        {DateTimeUtils.GetTimeWithHour(15), _emptyFrequencyList},
        {DateTimeUtils.GetTimeWithHour(16), new List<Frequencies> {Frequencies.TwoHours,Frequencies.FourHours, Frequencies.EightHours}},
        {DateTimeUtils.GetTimeWithHour(17), _emptyFrequencyList},
        {DateTimeUtils.GetTimeWithHour(18), new List<Frequencies> {Frequencies.TwoHours}},
        {DateTimeUtils.GetTimeWithHour(19), _emptyFrequencyList},
        {DateTimeUtils.GetTimeWithHour(20), new List<Frequencies> {Frequencies.TwoHours, Frequencies.FourHours}},
        {DateTimeUtils.GetTimeWithHour(21), _emptyFrequencyList},
        {DateTimeUtils.GetTimeWithHour(22), new List<Frequencies> {Frequencies.TwoHours}},
        {DateTimeUtils.GetTimeWithHour(23), _emptyFrequencyList}
    };

    [Theory]
    [MemberData(nameof(CorrectFrequenciesData))]
    public void CorrectFrequenciesAreReturned(DateTime dateTime, List<Frequencies> expectedFrequencies)
    {
        // Act
        var actualFrequencies = AlertFrequencies.GetCurrentFrequencies(dateTime);

        // Assert
        Assert.Equal(expectedFrequencies, actualFrequencies);
    }
}