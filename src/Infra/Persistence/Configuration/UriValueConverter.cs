using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SiteWatcher.Infra.Persistence.Configuration;

public class UriValueConverter : ValueConverter<Uri,string>
{
    public UriValueConverter() : base(v => v.ToString(), v => new Uri(v))
    { }
}