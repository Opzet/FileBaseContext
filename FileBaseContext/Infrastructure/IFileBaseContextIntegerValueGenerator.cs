﻿namespace FileBaseContext.Infrastructure;

public interface IFileBaseContextIntegerValueGenerator
{
    void Bump(object[] row);
}