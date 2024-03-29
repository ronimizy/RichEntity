﻿namespace RichEntity.Annotations;

public interface IEntity<out TIdentifier>
{
    TIdentifier Id { get; }
}

public interface IEntity { }