﻿namespace System.IoFx
{
    interface IVisitor<in T>
    {
        void Visit(T visitor);
    }
}