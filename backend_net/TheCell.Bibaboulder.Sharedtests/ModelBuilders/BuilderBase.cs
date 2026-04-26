using System;
using Thecell.Bibaboulder.Model.Basics;

namespace TheCell.Bibaboulder.Sharedtests.ModelBuilders;

public abstract class BuilderBase<T> : IBuilder<T> where T : class
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "This field must be accessed by the implementation classes")]
    protected T _instance;

    protected BuilderBase()
    {
        _instance = Activator.CreateInstance<T>();

        if (_instance is EntityAuditFields auditable)
        {
            auditable.CreatedDate = DateTime.UtcNow;
            auditable.CreatedUserId = Guid.Empty;
        }

        if (_instance is VersionedEntity versioned)
        {
            versioned.Version = 1;
        }
    }

    public virtual T Build()
    {
        return _instance;
    }

    public virtual T Instance => _instance;
}
