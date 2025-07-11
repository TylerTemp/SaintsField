using System;

// ReSharper disable once CheckNamespace
namespace SaintsField
{
    [Flags]
    public enum EType
    {
        /// <summary>
        /// Current assembly
        /// </summary>
        CurrentOnly = 1,
        /// <summary>
        ///  Current referenced assemblies.
        /// </summary>
        CurrentReferenced = 1 << 1,

        /// <summary>
        /// Current & referenced assemblies.
        /// </summary>
        Current = CurrentOnly | CurrentReferenced,

        /// <summary>
        /// Include "mscorlib" assembly.
        /// </summary>
        MsCorLib = 1 << 2,
        /// <summary>
        /// Include "System" assembly.
        /// </summary>
        System = 1 << 3,
        /// <summary>
        /// Include "System.Core" assembly.
        /// </summary>
        SystemCore = 1 << 4,
        /// <summary>
        /// Anything except "mscorlib", "System", "System.Core" assemblies.
        /// </summary>
        NonEssential = 1 << 5,
        /// <summary>
        /// All assemblies.
        /// </summary>
        AllAssembly = MsCorLib | System | SystemCore | NonEssential,

        /// <summary>
        /// Allow non-public types.
        /// </summary>
        AllowInternal = 1 << 6,

        /// <summary>
        /// Group the list by the assmbly short name.
        /// </summary>
        GroupAssmbly = 1 << 7,
        /// <summary>
        /// Group the list by the type namespace.
        /// </summary>
        GroupNameSpace = 1 << 8,
    }

    public static class ETypeExtensions
    {
        public static bool HasFlagFast(this EType lType, EType rType) => (lType & rType) != 0;
    }
}
