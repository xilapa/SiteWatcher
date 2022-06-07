﻿namespace SiteWatcher.Domain.Models.Email;

public class MailMessage
{
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool HtmlBody { get; set; }
    public MailRecipient[] Recipients { get; set; }
}