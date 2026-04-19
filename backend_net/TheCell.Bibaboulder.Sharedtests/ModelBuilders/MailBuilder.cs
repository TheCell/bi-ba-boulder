using System;
using Bogus;
using Thecell.Bibaboulder.Model.Model;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public class MailBuilder : BuilderBase<Mail>
{
    public MailBuilder() : base()
    {
        var bogus = new Faker("de_CH");
        _instance.Id = Guid.CreateVersion7();
        _instance.To = bogus.Internet.Email();
        _instance.Subject = bogus.Lorem.Sentence();
        _instance.Body = bogus.Lorem.Paragraph();
        _instance.SentAt = DateTime.UtcNow;
    }

    public MailBuilder SetTo(string value)
    {
        _instance.To = value;
        return this;
    }

    public MailBuilder SetCc(string? value)
    {
        _instance.Cc = value;
        return this;
    }

    public MailBuilder SetBcc(string? value)
    {
        _instance.Bcc = value;
        return this;
    }

    public MailBuilder SetSubject(string value)
    {
        _instance.Subject = value;
        return this;
    }

    public MailBuilder SetBody(string value)
    {
        _instance.Body = value;
        return this;
    }

    public MailBuilder SetSentAt(DateTime value)
    {
        _instance.SentAt = value;
        return this;
    }
}
