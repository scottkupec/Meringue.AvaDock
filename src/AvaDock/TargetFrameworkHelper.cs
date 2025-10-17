// Copyright (C) Scott Kupec. All rights reserved.

using System;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1403:File may only contain a single namespace", Justification = "Providing compatibility layer.")]

#if !NET6_0_OR_GREATER
#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Runtime.CompilerServices
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    /// <summary>
    /// Implements IsExternalInit for .NET versions earlier than .NET 6.0.
    /// </summary>
    /// <remarks>
    /// Reserved for use by the compiler to support <c>init</c> accessors.
    /// This type enables the use of <c>init</c>-only setters in C# 9.0 and later.
    /// </remarks>
    [Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Providing compatibility layer.")]
    [Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Providing compatibility layer.")]
    internal static class IsExternalInit
    {
    }

    /// <summary>
    /// Implements CallerArgumentExpression for .NET versions earlier than .NET 6.0.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    [Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Providing compatibility layer.")]
    [Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Providing compatibility layer.")]
    internal sealed class CallerArgumentExpressionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CallerArgumentExpressionAttribute"/> class.
        /// </summary>
        /// <param name="parameterName">The caller's name for the parameter.</param>
        public CallerArgumentExpressionAttribute(String parameterName)
        {
            this.ParameterName = parameterName;
        }

        /// <summary>
        /// Gets the caller's name for the parameter.
        /// </summary>
        public String ParameterName { get; }
    }
}
#endif

namespace Meringue.AvaDock
{
    /// <summary>
    /// Provides extension methods.
    /// </summary>
    internal static class TargetFrameworkHelper
    {
        /// <summary>
        /// Throws an Argument null exception.
        /// </summary>
        /// <param name="argument">The value of the paramenter.</param>
        /// <param name="paramName">The name of the parameter.</param>
        public static void ThrowIfArgumentNull([System.Diagnostics.CodeAnalysis.NotNull] Object? argument, [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(argument))] String? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
